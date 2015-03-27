using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Xml;
using OsmMsSqlUpload.Source;
using OsmMsSqlUpload.Source.PbfBuffers;
using OsmMsSqlUpload.Target;
using OsmMsSqlUpload.Utils;

namespace OsmMsSqlUpload.Analyser
{
    class OsmUploadAnalyser : OSMConsumer, IDisposable
    {
        static readonly Dictionary<string, bool> OneWayStr = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"yes", true},
            {"true", true},
            {"1", true}
        };

        private static readonly Dictionary<string, bool> PolygonTypes =
            new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"multipolygon", true},
                    {"boundary",true}
                };

        char _lastType = 'I';

        readonly ITempDatabase _tempStore;
        readonly ServerUploadBase _upload;

        string _name, _highway, _maxspeed, _oneway, _id;
        long _nextWayId = 0;
        readonly List<long> _nds;
        readonly StringBuilder _tags;
        XmlWriter _tagsWriter;

        IAnalyserProgress _progress;

        public OsmUploadAnalyser(ITempDatabase tempStore, ServerUploadBase upload,
            IAnalyserProgress progress)
        {
            _tempStore = tempStore;

            _progress = progress;

            _tags = new StringBuilder();
            _upload = upload;
            _nds = new List<long>(1024);
        }

        public void Dispose()
        {
        }

        public void Process()
        {
            FlushPrevious();
            _upload.FlushBuffer();


            _tempStore.DetectIntersections(_upload, _progress);

            _progress.OnAnalyserStep("Uploading Edges");
            _progress.OnAnalyserProgress(0);
            using (var rdr = _tempStore.GetWays())
            {
                if (!rdr.Read()) throw new Exception("Unable to find a way");

                var coord = new CoordinateStore(rdr.GetInt64(2));
                var currentWay = rdr.GetInt64(0);
                var oneWay = rdr.GetBoolean(4);
                var lineStrip = new List<KeyValuePair<double, double>>(64);
                lineStrip.Add(new KeyValuePair<double, double>(coord.Lat, coord.Lon));
                long? lastIntersection = rdr.IsDBNull(3) ? (long?) null : rdr.GetInt64(3);
                var lastRefresh = DateTime.Now;
                var totalEdges=0L;

                var lastPercent = 0;
                while (rdr.Read())
                {
                    var wayId = rdr.GetInt64(0);

                    coord = new CoordinateStore(rdr.GetInt64(2));
                    if (wayId != currentWay)
                    {
                        if (lineStrip.Count > 1)
                        {
                            _upload.AppendEdge(currentWay, lastIntersection, null, oneWay, lineStrip);
                        }

                        lineStrip.Clear();

                        var percent = (int) ((wayId*100)/_nextWayId);
                        if (percent != lastPercent)
                        {
                            _progress.OnAnalyserProgress(percent);
                            _progress.OnTotalEdges(totalEdges);
                            lastPercent = percent;
                            lastRefresh = DateTime.Now;
                        }
                        else
                        {
                            var now = DateTime.Now;
                            if (now.Subtract(lastRefresh).Seconds > ProcessDlg.RefreshRate)
                            {
                                _progress.OnTotalEdges(totalEdges);
                                lastRefresh = now;
                            }                            
                        }

                        currentWay = wayId;
                        oneWay = rdr.GetBoolean(4);

                        lineStrip.Add(new KeyValuePair<double, double>(coord.Lat, coord.Lon));
                        lastIntersection = rdr.IsDBNull(3) ? (long?) null : rdr.GetInt64(3);
                        continue;
                    }

                    var point = new KeyValuePair<double, double>(coord.Lat, coord.Lon);
                    lineStrip.Add(point);
                    if (rdr.IsDBNull(3)) continue;

                    var intersection = rdr.GetInt64(3);
                    ++totalEdges;
                    _upload.AppendEdge(currentWay, lastIntersection, intersection, oneWay, lineStrip);

                    lastIntersection = intersection;
                    lineStrip.Clear();
                    lineStrip.Add(point);
                }
            }
        }

        static bool GetOneWay(string oneway)
        {
            if (string.IsNullOrEmpty(oneway)) return false;
            bool oneWay;
            if (!OneWayStr.TryGetValue(oneway, out oneWay)) oneWay = false;
            return oneWay;            
        }

        void FlushPrevious()
        {
            if(_id==null) return;
            if (_lastType == 'W')
            {
                if (_highway != null)
                {
                    int type;
                    if (SqlServerUpload.WayTypeId.TryGetValue(_highway, out type))
                    {
                        var oneway = GetOneWay(_oneway);
                        var id = ++_nextWayId;
                        _tagsWriter.WriteEndDocument();
                        _tagsWriter.Flush();
                        _upload.AppendWay(id, _id, type, _name, _maxspeed, oneway, _tags.ToString());

                        _tempStore.InsertWay      (id,type,oneway);
                        _tempStore.InsertWayPoints(id,_nds);
                    }
                }
            }
            _name = _highway = _maxspeed = _oneway = _id = null;
        }

        public void OnNode(long id, double lat, double lon)
        {
            _lastType = 'N';

            _tempStore.InsertNode(id,lat,lon);
        }

        public void OnWay(string id)
        {
            FlushPrevious();
            _lastType = 'W';
            _id = id;
            _nds.Clear();
            _tags.Clear();
            _tagsWriter = XmlWriter.Create(_tags, new XmlWriterSettings { OmitXmlDeclaration = true });
            _tagsWriter.WriteStartElement("tags");
            _tagsWriter.WriteAttributeString("w", id);
        }

        public void OnRelation(string id)
        {
            FlushPrevious();
            _lastType = 'R';
            _id = id;
            _tags.Clear();
            _tagsWriter = null;
        }

        public void OnTag(string key, string value)
        {
            if (_id==null) return;

            if (_tagsWriter != null)
            {
                _tagsWriter.WriteStartElement("t");
                _tagsWriter.WriteAttributeString("k",key);
                _tagsWriter.WriteValue(value);
                _tagsWriter.WriteEndElement();
            }

            switch (key)
            {
                case "type":
                    break;
                case "name":
                    _name = value;
                    break;
                case "highway":
                    _highway = value;
                    break;
                case "junction":
                    if (_highway == null) _highway = "junction";
                    break;
                case "maxspeed":
                    _maxspeed = value;
                    break;
                case "oneway":
                    _oneway = value;
                    break;
            }
        }

        public void OnNd(long @ref)
        {
            _nds.Add(@ref);
        }

        public void OnMember(PbfRelationMemberType NorW, long @ref, string role)
        {
        }
    }
}