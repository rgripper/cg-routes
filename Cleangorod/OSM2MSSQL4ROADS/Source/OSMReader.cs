using System;
using System.Xml;
using OsmMsSqlUpload.Source.PbfBuffers;

// ReSharper disable InconsistentNaming
namespace OsmMsSqlUpload.Source
{
    interface OSMConsumer : IDisposable
    {
        void OnNode(long id, double lat, double lon);
        void OnWay (string id);
        void OnRelation(string id);

        void OnTag(string key, string value);
        void OnNd (long @ref);
        void OnMember(PbfRelationMemberType NorW, long @ref, string role);
    }

    public delegate void OnReaderProgress(
            int percentage,
            long nbPoints,
            long nbWays
    );

    class OSMReader : IDisposable
    {
        readonly System.IO.Stream _file;
        readonly OSMConsumer _backend;
        readonly XmlReader _reader;
        readonly OnReaderProgress _progress;

        private OSMReader(System.IO.Stream file, XmlReader reader, OSMConsumer backend, OnReaderProgress progress)
        {
            _reader = reader;
            _backend = backend;
            _file = file;

            _progress = progress;
        }

        public void Dispose()
        {
        }


        void ProcessNodeTag()
        {
            if (!_reader.HasAttributes) return; //< Invalid

            long nodeId=-1;
            double lat = double.NaN, lng = double.NaN;

            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                switch (nme.Length)
                {
                    case 2:
                        if (nme[0] == 'i' && nme[1] == 'd') nodeId = _reader.ReadContentAsLong();
                        break;
                    case 3:
                        if (nme[0] != 'l') break;
                        if (nme[1] == 'a' && nme[2] == 't') lat = _reader.ReadContentAsDouble(); //Convert.ToDouble(_reader.Value);
                        else if (nme[1] == 'o' && nme[2] == 'n') lng = _reader.ReadContentAsDouble();
                        break;
                }
            }

            if (double.IsNaN(lat) || double.IsNaN(lng)) return;

            _backend.OnNode(nodeId, lat, lng);
        }

        void ProcessWayTag()
        {
            if (!_reader.HasAttributes) return; //< Invalid
            string wayId = null;
            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                if(nme.Length==2)
                    if (nme[0] == 'i' && nme[1] == 'd') wayId = _reader.ReadContentAsString();
            }

            _backend.OnWay(wayId);
        }

        void ProcessRelTag()
        {
            if (!_reader.HasAttributes) return; //< Invalid
            string relId = null;
            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                if (nme.Length == 2)
                    if (nme[0] == 'i' && nme[1] == 'd') relId = _reader.ReadContentAsString();
            }

            _backend.OnRelation(relId);
        }

        void ProcessNdTag()
        {
            if (!_reader.HasAttributes) return; //< Invalid
            long nodeId = -1;
            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                if (nme.Length == 3)
                    if (nme[0] == 'r' && nme[1] == 'e' && nme[2] == 'f')
                        nodeId = _reader.ReadContentAsLong(); // Convert.ToInt64(_reader.Value);
            }

            _backend.OnNd(nodeId);
        }

        void ProcessMemberTag()
        {
            if (!_reader.HasAttributes) return; //< Invalid
            long nodeId = -1;
            string role = null;
            PbfRelationMemberType? type = null;
            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                if (nme.Length == 3)
                {
                    if (nme[0] == 'r' && nme[1] == 'e' && nme[2] == 'f')
                        nodeId = _reader.ReadContentAsLong(); // Convert.ToInt64(_reader.Value);
                }
                else if (nme.Length == 4)
                {
                    if (nme[0] == 't' && nme[1] == 'y' && nme[2] == 'p' && nme[3] == 'e')
                    {
                        switch (_reader.Value[0])
                        {
                            case 'n':
                                type = PbfRelationMemberType.Node;
                                break;
                            case 'w':
                                type = PbfRelationMemberType.Way;
                                break;
                        }
                    }
                    else if (nme[0] == 'r' && nme[1] == 'o' && nme[2] == 'l' && nme[3] == 'e')
                    {
                        role = _reader.Value;
                    }
                }
            }

            if (type.HasValue) _backend.OnMember(type.Value, nodeId, role);
        }


        void ProcessTagOf()
        {
            string tagType = null, tagValue = null;

            while (_reader.MoveToNextAttribute())
            {
                var nme = _reader.Name;
                if (nme.Length!=1) continue;
                switch (nme[0])
                {
                    case 'k':
                        tagType = _reader.Value;
                        break;
                    case 'v':
                        tagValue = _reader.Value;
                        break;
                }
            }

            if (tagType == null || tagValue == null) return;

            _backend.OnTag(tagType, tagValue);
        }

        private enum LastProcessNode
        {
            Node,
            Way,
            Relation,
            Invalid
        }

        public long TotalNodes { get; private set; }
        public long TotalWays { get; private set; }
        public long TotalRelations { get; private set; }

        void Process()
        {
            var lastType = LastProcessNode.Invalid;
            var size = _file.Length;
            var increment = size / 100;
            var nextPosition = increment;
            var percent = 0;
            var lastRefresh = DateTime.Now;

            _progress(0, 0, 0);

            for (var total = 0; _reader.Read(); ++total)
            {
                if (!_reader.IsStartElement()) continue;

                if (_file.Position > nextPosition)
                {
                    _progress(++percent, TotalNodes, TotalWays);
                    nextPosition += increment;
                    lastRefresh = DateTime.Now;
                }
                else
                {
                    var now = DateTime.Now;
                    if (now.Subtract(lastRefresh).Seconds > ProcessDlg.RefreshRate)
                    {
                        _progress(percent, TotalNodes, TotalWays);
                        lastRefresh = now;
                    }
                }

                switch (_reader.Name[0])
                {
                    case 'n': //"node":
                        if (_reader.Name[1]=='d') // "nd":
                        {
                            if (lastType == LastProcessNode.Way)
                                ProcessNdTag();
                            break;
                        }
                        ++TotalNodes;
                        ProcessNodeTag();
                        lastType = LastProcessNode.Node;
                        break;

                    case 'w': //"way":
                        ++TotalWays;
                        ProcessWayTag();
                        lastType = LastProcessNode.Way;
                        break;

                    case 'r': //"relation":
                        ++TotalRelations;
                        ProcessRelTag();
                        lastType = LastProcessNode.Relation;
                        break;

                    case 'm': //"member":
                        if (lastType == LastProcessNode.Relation)
                            ProcessMemberTag();
                        break;

                    case 't': // "tag":
                        if (lastType == LastProcessNode.Invalid) break;
                        ProcessTagOf();
                        break;

                    default:
                        lastType = LastProcessNode.Invalid;
                        break;
                }
            }

            _progress(100, TotalNodes, TotalWays);
        }

        public static void RunUpload(string sourceFile, 
            OnReaderProgress progress,
            OSMConsumer backend)
        {
            try
            {
                using (var file = System.IO.File.OpenRead(sourceFile))
                using (var bzs = new Bzip2.BZip2InputStream(file))
                using (var reader = XmlReader.Create(bzs))
                using (var processor = new OSMReader(file, reader, backend, progress))
                {
                    processor.Process();
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                // Silently ignore this one.
            }
        }
    }
}
