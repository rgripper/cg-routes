using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using OsmMsSqlUpload.Utils;

namespace OsmMsSqlUpload.Target
{
    class SqlServerBulkUpload : ServerUploadBase
    {
        long _nextEdgeId = 0;

        SqlBulkCopy _bcTarget;

        DataTable _dtOsmWay;
        DataTable _dtPlnIntersection;
        DataTable _dtOsmWayEdge;
        DataTable _dtPlnEdge;

        int _nbRows = 0;

        public SqlServerBulkUpload(SqlConnection cnx, OnLitSqlStatus status)
            : base(cnx, status)
        {
            _bcTarget = new SqlBulkCopy(cnx);

            _dtOsmWay           = new DataTable();
            _dtPlnIntersection  = new DataTable();
            _dtOsmWayEdge       = new DataTable();
            _dtPlnEdge          = new DataTable();

            _dtOsmWay.Columns.Add( "WayId",        typeof(long) );
            _dtOsmWay.Columns.Add( "WayCode",      typeof(string) );
            _dtOsmWay.Columns.Add( "WayTypeId",    typeof(int) );
            _dtOsmWay.Columns.Add( "Name",         typeof(string) );
            _dtOsmWay.Columns.Add( "MaxSpeed",     typeof(decimal) );
            _dtOsmWay.Columns.Add( "Oneway",       typeof(bool) );
            _dtOsmWay.Columns.Add( "Tags",         typeof(string) );

            _dtPlnIntersection.Columns.Add( "IntId",    typeof(long) );
            _dtPlnIntersection.Columns.Add( "Position", typeof(byte[]) );

            _dtOsmWayEdge.Columns.Add( "EdgeId",        typeof(long) );
            _dtOsmWayEdge.Columns.Add( "WayId",         typeof(long) );
            _dtOsmWayEdge.Columns.Add( "Distance",      typeof(decimal) );
            _dtOsmWayEdge.Columns.Add( "Way",           typeof(byte[]) );

            _dtPlnEdge.Columns.Add( "Left",     typeof(long) );
            _dtPlnEdge.Columns.Add( "Right",    typeof(long) );
            _dtPlnEdge.Columns.Add( "EdgeId",   typeof(long) );
        }

        void UploadTable(string name, DataTable source)
        {
            if (source.Rows.Count == 0) return;
            _bcTarget.DestinationTableName = name;
            _bcTarget.WriteToServer(source);
            source.Rows.Clear();
        }

        public override void FlushBuffer()
        {
            _status(true);

            UploadTable("dbo.OsmWay",           _dtOsmWay);
            UploadTable("dbo.PlnIntersection",  _dtPlnIntersection);
            UploadTable("dbo.OsmWayEdge",       _dtOsmWayEdge);
            UploadTable("dbo.PlnEdge",          _dtPlnEdge);

            _status(false);
        }

        void RowProcess()
        {
            if (_nbRows > 10000) { FlushBuffer(); _nbRows = 0; }
        }

        public override void AppendWay(long index, string id, int type, string name, string speed, bool oneway, string tags)
        {
            _dtOsmWay.Rows.Add(
                index,
                id,
                type,
                name,
                GetWaySpeed(type, speed),
                oneway,
                tags
             );

            ++_nbRows; RowProcess();
        }

        public override void AppendIntersection(long index, double lat, double lon)
        {
            _dtPlnIntersection.Rows.Add(
                index,
                SqlServerShape.ParsePoint(lat,lon)
            );

            ++_nbRows; RowProcess();
        }

        public override void AppendEdge(long wayId, long? left, long? right,
            bool oneWay,
            List<KeyValuePair<double, double>> lineStrip)
        {
            var edgeId = _nextEdgeId++;

            _dtOsmWayEdge.Rows.Add(
                edgeId,
                wayId,
                Distance.Calc(lineStrip),
                SqlServerShape.LineStringToByte(lineStrip)
            );
            ++_nbRows;

            if (left.HasValue && right.HasValue)
            {
                _dtPlnEdge.Rows.Add(
                    left.Value,
                    right.Value,
                    edgeId
                );
                ++_nbRows;
            }
            RowProcess();
        }

    }
}
