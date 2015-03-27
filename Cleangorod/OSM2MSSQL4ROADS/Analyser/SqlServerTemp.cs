using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using OsmMsSqlUpload.Target;
using OsmMsSqlUpload.Utils;

namespace OsmMsSqlUpload.Analyser
{
    class SqlServerTemp : ITempDatabase
    {
        private readonly string _connectionString;
        SqlConnection _cnx;
        private SqlCommand _cmdNewNode, _cmdNewWay, _cmdAddNd;
        SqlTransaction _trans;
        int _nb;

        public SqlServerTemp(string connectionString)
        {
            _connectionString = connectionString;

            _cnx = new SqlConnection(connectionString);
            _cnx.Open();

            using (var cmd = _cnx.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE #Node(Id BIGINT not null primary key,coord BIGINT not null,intersection BIGINT null);
CREATE TABLE #Way (Id BIGINT not null primary key,type INT, oneway BIT);
CREATE TABLE #WayPoint ( WayID BIGINT not null, NodeID BIGINT not null, position integer not null );
CREATE TABLE #Intersection(Id BIGINT PRIMARY KEY,NodeID BIGINT not null);
CREATE INDEX Intersection_NodeID on #Intersection(NodeID);
";
                cmd.ExecuteNonQuery();
            }

            _cmdNewNode = _cnx.CreateCommand();
            _cmdNewNode.CommandText = "INSERT INTO #Node (Id,coord,intersection) values (@id,@coord,null)";
            _cmdNewNode.Parameters.Add("@id", SqlDbType.BigInt);
            _cmdNewNode.Parameters.Add("@coord", SqlDbType.BigInt);
            _cmdNewNode.Prepare();

            _cmdNewWay = _cnx.CreateCommand();
            _cmdNewWay.CommandText = "INSERT INTO #Way (Id,type,oneway) values (@id,@type,@oneway)";
            _cmdNewWay.Parameters.Add("@id", SqlDbType.BigInt);
            _cmdNewWay.Parameters.Add("@type", SqlDbType.Int);
            _cmdNewWay.Parameters.Add("@oneway", SqlDbType.Bit);
            _cmdNewWay.Prepare();

            _cmdAddNd = _cnx.CreateCommand();
            _cmdAddNd.CommandText = "INSERT INTO #WayPoint (WayID,NodeID,position) values (@wid,@nid,@idx)";
            _cmdAddNd.Parameters.Add("@wid", SqlDbType.BigInt);
            _cmdAddNd.Parameters.Add("@nid", SqlDbType.BigInt);
            _cmdAddNd.Parameters.Add("@idx", SqlDbType.Int);
            _cmdAddNd.Prepare();

            _trans = _cnx.BeginTransaction();
            _cmdNewNode.Transaction = _trans;
            _cmdNewWay.Transaction = _trans;
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
            if(_cnx!=null)
            {
                _cnx.Close();
                _cnx = null;
            }
        }

        private void TransRoll(bool force = false)
        {
            _nb++;
            if (_nb > 5000 || force)
            {
                _trans.Commit();
                _trans = _cnx.BeginTransaction();
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
            using (var cmd = _cnx.CreateCommand())
            {
                cmd.CommandText = @"
CREATE INDEX WayPoint_WayNodes ON #WayPoint( WayID, position );
";
                cmd.ExecuteNonQuery();
            }

            progress.OnAnalyserStep("Intersection detection ...");
            var nbIntersections = 0;
            using (var cmdI = _cnx.CreateCommand())
            {
                cmdI.CommandText = "INSERT INTO #Intersection(Id,NodeID) VALUES(@iid,@nid);" +
                                   "UPDATE NODE SET #intersection=@iid WHERE Id=@nid";
                var pIid = cmdI.Parameters.Add("@iid", DbType.Int64);
                var pNid = cmdI.Parameters.Add("@nid", DbType.Int64);
                cmdI.Prepare();
                cmdI.Transaction = _cnx.BeginTransaction();

                var l = 0;
                var n = -1L;
                using (var cmd = _cnx.CreateCommand())
                {
                    cmd.CommandText =
                        @"
select w1.NodeID
from #WayPoint w1 inner join #WayPoint w2 on 
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
                                cmdI.Transaction = _cnx.BeginTransaction();
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
            using (var cmd = _cnx.CreateCommand())
            {
                var l = 0;
                cmd.CommandText = @"SELECT I.Id, n.coord FROM #Intersection i INNER JOIN #Node n on i.NodeID=n.ID";
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        progress.OnAnalyserProgress((++l * 100) / nbIntersections);
                        var coord = new CoordinateStore(rdr.GetInt64(1));
                        upload.AppendIntersection(rdr.GetInt64(0), coord.Lat, coord.Lon);
                    }
                }
            }
        }

        public IDataReader GetWays()
        {
            TransRoll(true);

            var cmd = _cnx.CreateCommand();
            cmd.CommandText =
                @"
select wp.WayID,wp.position,n.coord,n.intersection,w.oneway
from #WayPoint wp
inner join 			#Node n 				on wp.NodeID=n.Id
inner join 			#Way  w 				on wp.WayID =w.Id
order by wp.WayID,wp.position
";
            return cmd.ExecuteReader();
        }
    }
}
