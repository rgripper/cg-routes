using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using OsmMsSqlUpload.Utils;

namespace OsmMsSqlUpload.Target
{
    class SqlServerUpload : ServerUploadBase
    {
        private const int MaxBuffer = 5 * 1024 * 1024;

        readonly StringBuilder _sbCommand;

        long _nextEdgeId = 0;
        SqlCommand _lastCommand = null;
        IAsyncResult _lastQuery = null;
        int _counter = 0;

        public SqlServerUpload(SqlConnection cnx, OnLitSqlStatus status) : base(cnx,status)
        {
            _sbCommand = new StringBuilder(MaxBuffer+1024);
            _sbCommand.AppendLine("BEGIN TRAN;");
        }

        public override void FlushBuffer()
        {
            if (_sbCommand.Length == 0) return;

            _status(true);

            if(_lastQuery!=null) 
            {
                while (!_lastQuery.IsCompleted)
                {
                    _lastQuery.AsyncWaitHandle.WaitOne(500);    
                }
                _lastCommand.EndExecuteNonQuery(_lastQuery);
                _lastCommand.Dispose();
                _lastQuery = null;
                _lastCommand = null;
            }

            _sbCommand.AppendLine("COMMIT;");
            _lastCommand = _cnx.CreateCommand();
            _lastCommand.CommandText = _sbCommand.ToString();
            _lastCommand.CommandTimeout = 0;
            _lastQuery = _lastCommand.BeginExecuteNonQuery();
            _sbCommand.Clear();
            _sbCommand.AppendLine("BEGIN TRAN;");

            _status(false);
        }

        void EndStatement()
        {
            if ((++_counter) % 10 == 0)
            {
                _sbCommand.AppendLine("COMMIT;");
                _sbCommand.AppendLine("BEGIN TRAN;");
            }
            if(_lastQuery!=null)
            {
                if (_lastQuery.IsCompleted) FlushBuffer();
            }
            if (_sbCommand.Length > MaxBuffer) FlushBuffer();

        }


        void AppendString(string text)
        {
            if (text == null) _sbCommand.Append("NULL");
            else
            {
                _sbCommand.Append("N'");
                for (var i = 0; i < text.Length; ++i)
                    if (text[i] == '\'') _sbCommand.Append('\'', 2);
                    else _sbCommand.Append(text[i]);
                _sbCommand.Append("'");
            }            
        }

        public override void AppendWay(long index, string id, int type, string name, string speed, bool oneway, string tags)
        {
            _sbCommand.Append("INSERT INTO dbo.OsmWays (WayId,WayCode,WayTypeId,Name,MaxSpeed,Oneway,Tags) VALUES (");
            _sbCommand.Append(index); _sbCommand.Append(',');
            _sbCommand.Append(id);   _sbCommand.Append(',');
            _sbCommand.Append(type); _sbCommand.Append(',');
            AppendString(name); _sbCommand.Append(',');
            _sbCommand.Append(GetWaySpeed(type, speed));     _sbCommand.Append(',');
            _sbCommand.Append(oneway ? '1' : '0');          _sbCommand.Append(',');
            AppendString(tags);
            _sbCommand.AppendLine(");");

            EndStatement();
        }

        public override void AppendIntersection(long index, double lat, double lon)
        {
            _sbCommand.Append("INSERT INTO dbo.PlnIntersection (IntId,Position) VALUES (");

            _sbCommand.Append(index); _sbCommand.Append(',');
            _sbCommand.AppendFormat("geography::Point({0},{1},4326)", lat, lon);
            _sbCommand.AppendLine(");");

            if (_sbCommand.Length > MaxBuffer) FlushBuffer();
        }

        public override void AppendEdge(long wayId, long? left, long? right, 
            bool oneWay,
            List<KeyValuePair<double, double>> lineStrip)
        {
            var edgeId = _nextEdgeId++;

            _sbCommand.Append("INSERT INTO OsmWayEdge (EdgeId,WayId,Distance,Way) VALUES (");
            _sbCommand.Append(edgeId); _sbCommand.Append(',');
            _sbCommand.Append(wayId); _sbCommand.Append(',');
            _sbCommand.Append(Distance.Calc(lineStrip)); _sbCommand.Append(',');

            _sbCommand.Append("geography::Parse('LINESTRING(");
            for(var i=0;i<lineStrip.Count;++i)
            {
                if (i != 0) _sbCommand.Append(',');
                _sbCommand.AppendFormat("{0} {1}", lineStrip[i].Value, lineStrip[i].Key);
            }
            _sbCommand.AppendLine(")') );");

            if(left.HasValue && right.HasValue)
            {
                _sbCommand.Append("INSERT INTO PlnEdge ([Left],[Right],EdgeId) VALUES (");
                _sbCommand.Append(left.Value); _sbCommand.Append(',');
                _sbCommand.Append(right.Value); _sbCommand.Append(',');
                _sbCommand.Append(edgeId);
                _sbCommand.Append(")");

                if(!oneWay)
                {
                    _sbCommand.Append(",(");
                    _sbCommand.Append(right.Value); _sbCommand.Append(',');
                    _sbCommand.Append(left.Value); _sbCommand.Append(',');
                    _sbCommand.Append(edgeId);
                    _sbCommand.Append(")");
                }
                _sbCommand.AppendLine(";");
            }

            EndStatement();
        }
    }
}