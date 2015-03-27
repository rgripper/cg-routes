using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using OsmMsSqlUpload.Target;
using OsmMsSqlUpload.Utils;

namespace OsmMsSqlUpload.Analyser
{
    internal class SqliteTemp : ITempDatabase
    {
        private SQLiteConnection _tempDb;
        private SQLiteCommand _cmdNewNode, _cmdNewWay, _cmdAddNd;
        private SQLiteTransaction _trans;

        private readonly string _filename;
        private int _nb;

        public SqliteTemp(string fileName)
        {
            _filename = fileName;
            if (System.IO.File.Exists(fileName)) System.IO.File.Delete(fileName);

            _tempDb = new SQLiteConnection(
                (new SQLiteConnectionStringBuilder
                     {
                         DataSource = _filename,
                         SyncMode = SynchronizationModes.Off
                     }).ConnectionString
                );
            _tempDb.Open();

            using (var cmd = _tempDb.CreateCommand())
            {
                cmd.CommandText =
                    @"
CREATE TABLE Node(Id integer not null primary key,coord integer not null,intersection integer null);
CREATE TABLE Way (Id INT not null primary key,type INT, oneway INT);
CREATE TABLE WayPoint ( WayID INT not null, NodeID integer not null, position integer not null );
CREATE TABLE Intersection(Id INTEGER PRIMARY KEY,NodeID integer not null);
CREATE INDEX Intersection_NodeID on Intersection(NodeID);
";
                cmd.ExecuteNonQuery();
            }

            _trans = _tempDb.BeginTransaction();

            _cmdNewNode = _tempDb.CreateCommand();
            _cmdNewNode.CommandText = "INSERT INTO Node (Id,coord,intersection) values (@id,@coord,null)";
            _cmdNewNode.Parameters.Add("@id", DbType.Int64);
            _cmdNewNode.Parameters.Add("@coord", DbType.Int64);
            _cmdNewNode.Prepare();
            _cmdNewNode.Transaction = _trans;

            _cmdNewWay = _tempDb.CreateCommand();
            _cmdNewWay.CommandText = "INSERT INTO Way (Id,type,oneway) values (@id,@type,@oneway)";
            _cmdNewWay.Parameters.Add("@id", DbType.Int64);
            _cmdNewWay.Parameters.Add("@type", DbType.Int32);
            _cmdNewWay.Parameters.Add("@oneway", DbType.Boolean);
            _cmdNewWay.Prepare();
            _cmdNewWay.Transaction = _trans;

            _cmdAddNd = _tempDb.CreateCommand();
            _cmdAddNd.CommandText = "INSERT INTO WayPoint (WayID,NodeID,position) values (@wid,@nid,@idx)";
            _cmdAddNd.Parameters.Add("@wid", DbType.Int64);
            _cmdAddNd.Parameters.Add("@nid", DbType.Int64);
            _cmdAddNd.Parameters.Add("@idx", DbType.UInt32);
            _cmdAddNd.Prepare();
            _cmdAddNd.Transaction = _trans;

        }

        public void Dispose()
        {
            if (_trans != null)
            {
                _trans.Commit();
                _trans = null;
            }

            if (_cmdNewNode != null)
            {
                _cmdNewNode.Dispose();
                _cmdNewNode = null;
            }
            if (_cmdNewWay != null)
            {
                _cmdNewWay.Dispose();
                _cmdNewWay = null;
            }
            if (_cmdAddNd != null)
            {
                _cmdAddNd.Dispose();
                _cmdAddNd = null;
            }

            if (_tempDb != null)
            {
                _tempDb.Close();
                _tempDb.Dispose();
                _tempDb = null;
            }

            GC.Collect();

            // It takes time for SQLite to shutdown.
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    if (System.IO.File.Exists(_filename)) System.IO.File.Delete(_filename);
                    break;
                }
                catch (System.IO.IOException)
                {
                    GC.Collect();
                    System.Threading.Thread.Sleep(500);
                }
            }
            
        }

        private void TransRoll(bool force = false)
        {
            _nb++;
            if (_nb > 5000 || force)
            {
                _trans.Commit();
                _trans = _tempDb.BeginTransaction();
                _cmdNewNode.Transaction = _trans;
                _cmdNewWay.Transaction = _trans;
                _cmdAddNd.Transaction = _trans;
                _nb = 0;
            }
        }


        public void InsertNode(long nid, double lat, double lon)
        {
            var p = _cmdNewNode.Parameters;
            p[0].Value = nid;
            p[1].Value = CoordinateStore.PackCoord(lon, lat);
            _cmdNewNode.ExecuteNonQuery();
            TransRoll();
        }

        public void InsertWay(long wid, int type, bool oneWay)
        {
            var p = _cmdNewWay.Parameters;
            p[0].Value = wid;
            p[1].Value = type;
            p[2].Value = oneWay;
            _cmdNewWay.ExecuteNonQuery();
        }

        public void InsertWayPoints(long wid, IEnumerable<long> nids)
        {
            var pP = _cmdAddNd.Parameters;
            var idx = 0;
            foreach (var nid in nids)
            {
                pP[0].Value = wid;
                pP[1].Value = nid;
                pP[2].Value = ++idx;
                _cmdAddNd.ExecuteNonQuery();
            }
        }

        public void DetectIntersections(ServerUploadBase upload, IAnalyserProgress progress)
        {
            TransRoll(true);

            progress.OnAnalyserStep("Prepare indexes ...");
            using (var cmd = _tempDb.CreateCommand())
            {
                cmd.CommandText = @"
CREATE INDEX WayPoint_WayNodes ON WayPoint( WayID, position );
CREATE INDEX WayPoint_NodeIDs  ON WayPoint( NodeID );
";
                cmd.ExecuteNonQuery();
            }

            progress.OnAnalyserStep("Intersection detection ...");
            var nbIntersections = 0;
            using (var cmdI = _tempDb.CreateCommand())
            {
                cmdI.CommandText = "INSERT OR IGNORE INTO Intersection(Id,NodeID) VALUES(@iid,@nid);" +
                                   "UPDATE NODE SET intersection=@iid WHERE Id=@nid";
                var pIid = cmdI.Parameters.Add("@iid", DbType.Int64);
                var pNid = cmdI.Parameters.Add("@nid", DbType.Int64);
                cmdI.Prepare();
                cmdI.Transaction = _tempDb.BeginTransaction();

                var l = 0;
                var n = -1L;
                using (var cmd = _tempDb.CreateCommand())
                {
                    cmd.CommandText =
                        @"
select w1.NodeID
from WayPoint w1 inner join WayPoint w2 on 
w1.NodeID=w2.NodeID and w1.WayID<>w2.WayID
order by w1.NodeID";
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var nid = rdr.GetInt64(0);
                            if (nid == n) continue;

                            pIid.Value = ++nbIntersections;
                            pNid.Value = n;
                            cmdI.ExecuteNonQuery();

                            if (++l > 10000 && n != -1)
                            {
                                progress.OnIntersectionNb(nbIntersections);

                                cmdI.Transaction.Commit();
                                cmdI.Transaction = _tempDb.BeginTransaction();
                                l = 0;
                            }
                            n = nid;
                        }
                    }
                }

                cmdI.Transaction.Commit();
                progress.OnIntersectionNb(nbIntersections);
            }

            progress.OnAnalyserStep("Uploading Intersection ...");
            progress.OnAnalyserProgress(0);
            using (var cmd = _tempDb.CreateCommand())
            {
                var l = 0;
                cmd.CommandText = @"SELECT I.Id, n.coord FROM Intersection i INNER JOIN Node n on i.NodeID=n.ID";
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        progress.OnAnalyserProgress((++l*100)/nbIntersections);
                        var coord = new CoordinateStore(rdr.GetInt64(1));
                        upload.AppendIntersection(rdr.GetInt64(0), coord.Lat, coord.Lon);
                    }
                }
            }
        }

        public IDataReader GetWays()
        {
            TransRoll(true);

            var cmd = _tempDb.CreateCommand();
            cmd.CommandText =
                @"
select wp.WayID,wp.position,n.coord,n.intersection,w.oneway
from WayPoint wp
inner join 			Node n 				on wp.NodeID=n.Id
inner join 			Way  w 				on wp.WayID =w.Id
order by wp.WayID,wp.position
";
            return cmd.ExecuteReader();
        }
    }
}
