using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace OsmMsSqlUpload.Target
{
    delegate void OnLitSqlStatus(bool active);

    abstract class ServerUploadBase : IDisposable
    {
        static readonly public Dictionary<string, int> WayTypeId = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
                                                                       {
                                                                           {"living_street", 1  }, 
                                                                           {"motorway",      2  },
                                                                           {"motorway_link", 3  },
                                                                           {"primary",       4  },
                                                                           {"primary_link",  5  },
                                                                           {"residential",   6  },  
                                                                           {"road",          7  },         
                                                                           {"secondary",     8  },
                                                                           {"secondary_link",9  },
                                                                           {"tertiary",      10 },
                                                                           {"tertiary_link", 11 },
                                                                           {"trunk",         12 },       
                                                                           {"trunk_link",    13 },
                                                                           {"junction",      14 },
                                                                       };
        static readonly public Dictionary<int, decimal> SpeedByWayType = new Dictionary<int, decimal>()
                                                                             {
                                                                                 {/*"living_street",*/   1,  30m },
                                                                                 {/*"motorway",     */   2,  120m },
                                                                                 {/*"motorway_link",*/   3,  60m }, 
                                                                                 {/*"primary",      */   4,  90m },
                                                                                 {/*"primary_link", */   5,  50m },
                                                                                 {/*"residential",  */   6,  40m },   
                                                                                 {/*"road",         */   7,  50m },          
                                                                                 {/*"secondary",    */   8,  80m },
                                                                                 {/*"secondary_link",*/  9,  50m },
                                                                                 {/*"tertiary",     */   10, 75m },
                                                                                 {/*"tertiary_link",*/   11, 50m },
                                                                                 {/*"trunk",        */   12, 100m },        
                                                                                 {/*"trunk_link",   */   13, 60m },
                                                                                 {/*"junction",     */   14, 15m },
                                                                             };
        static readonly string DbCreateScript = @"
IF OBJECT_ID ( 'dbo.PlnEdge',        'U' ) IS NOT NULL DROP TABLE dbo.PlnEdge;
IF OBJECT_ID ( 'dbo.OsmWayEdge',     'U' ) IS NOT NULL DROP TABLE dbo.OsmWayEdge;
IF OBJECT_ID ( 'dbo.PlnIntersection','U' ) IS NOT NULL DROP TABLE dbo.PlnIntersection;
IF OBJECT_ID ( 'dbo.OsmWay',         'U' ) IS NOT NULL DROP TABLE dbo.OsmWay;
IF OBJECT_ID ( 'dbo.OsmWayType',     'U' ) IS NOT NULL DROP TABLE dbo.OsmWayType;

CREATE TABLE dbo.OsmWayType (
        WayTypeId    INT           PRIMARY KEY,   
        WayTypeCode  VARCHAR(64)   NOT NULL UNIQUE,
        WayTypeDesc  VARCHAR(MAX),
        WayTypeSpeed DECIMAL(5,2)
);

INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (1, 'living_street', 10,    'A street where pedestrians have priority over cars, children can play on the street, maximum speed is low');
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (2, 'motorway',      120,   'A restricted access major divided highway, normally with 2 or more running lanes plus emergency hard shoulder. Equivalent to the Freeway, Autobahn, etc..');
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (3, 'motorway_link', 60,    'The link roads (sliproads/ramps) leading to/from a motorway from/to a motorway or lower class highway.');
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (4, 'primary',       90,    'Administrative classification in the UK, generally linking larger towns.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (5, 'primary_link',  50,    'The link roads (sliproads/ramps) leading to/from a primary road from/to a primary road or lower class highway.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (6, 'residential',   40,    'Roads accessing or around residential areas but which are not a classified or unclassified highway.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (7, 'road',          50,    'A road of unknown classification.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (8, 'secondary',     80,    'Administrative classification in the UK, generally linking smaller towns and villages' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (9, 'secondary_link',50,    'The link roads (sliproads/ramps) leading to/from a secondary road from/to a secondary road or lower class highway.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (10,'tertiary',      75,    'A ''C'' road in the UK. Generally for use on roads wider than 4 metres (13'') in width, and for faster/wider minor roads that aren''t A or B roads. In the UK, they tend to have dashed lines down the middle, whereas unclassified roads don''t.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (11,'tertiary_link', 50,    'The link roads (sliproads/ramps) leading to/from a tertiary road from/to a tertiary road or lower class highway.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (12,'trunk',         100,   'Important roads that aren''t motorways.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (13,'trunk_link',    60,    'The link roads (sliproads/ramps) leading to/from a trunk road from/to a trunk road or lower class highway.' );
INSERT INTO dbo.OsmWayType (WayTypeId,WayTypeCode,WayTypeSpeed,WayTypeDesc) VALUES (14,'junction',      15,    'Road junction');
CREATE TABLE dbo.OsmWay (
        WayId       BIGINT          PRIMARY KEY,
        WayCode     VARCHAR(64)     NOT NULL UNIQUE,
        WayTypeId   INT             NOT NULL REFERENCES dbo.OsmWayType(WayTypeId),
        Name        NVARCHAR(MAX),
        MaxSpeed    DECIMAL(5,2)    NOT NULL, 
        Oneway      BIT             NOT NULL,
        Tags        XML
);

CREATE TABLE dbo.PlnIntersection (
        IntId       BIGINT          PRIMARY KEY,
        Position    GEOGRAPHY       NOT NULL
);

CREATE TABLE dbo.OsmWayEdge (
        EdgeId      BIGINT          PRIMARY KEY,    
        WayId       BIGINT          REFERENCES dbo.OsmWay(WayId),
        Distance    DECIMAL(15,5)   NOT NULL,
        Way         GEOGRAPHY       NOT NULL
);

CREATE TABLE dbo.PlnEdge (
        [Left]      BIGINT          NOT NULL REFERENCES dbo.PlnIntersection(IntId),
        [Right]     BIGINT          NOT NULL REFERENCES dbo.PlnIntersection(IntId),
        EdgeId      BIGINT          NOT NULL REFERENCES dbo.OsmWayEdge     (EdgeId)
);
";

        static protected readonly Regex RxSpeed = new Regex(@"^\s*(?<speed>[0-9.]+)\s*(?<mph>mph)?\s*$",
                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static protected readonly Dictionary<string, bool> OneWayStr = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase)
                                                                  {
                                                                      {"yes", true},
                                                                      {"true", true},
                                                                      {"1", true}
                                                                  };

        static void CheckServerVersion(SqlConnection cnx)
        {
            string version;
            using (var cmd = cnx.CreateCommand())
            {
                cmd.CommandText = "SELECT SERVERPROPERTY('productversion')";
                version = Convert.ToString(cmd.ExecuteScalar());
            }

            var majorVersion = 0;
            if (!int.TryParse(version.Split('.')[0], out majorVersion))
                throw new Exception("Invalid productversion property. Your server is probably not compatible...");
            if (majorVersion < 11)
                throw new Exception("You need SQL Server 2012 or later.");
        }

        static protected decimal GetWaySpeed(int type, string speed)
        {
            var speedVal = 15m;
            if (speed != null)
            {
                var m = RxSpeed.Match(speed);
                if (m.Success)
                {
                    if (decimal.TryParse(m.Groups["speed"].Value, out speedVal))
                    {
                        speedVal = Math.Abs(speedVal);
                        if (m.Groups["mph"] != null) speedVal =Convert.ToDecimal(((double)speedVal) * 1.609344);
                        return Math.Min(150m, speedVal);
                    }
                }
            }
            return SpeedByWayType[type];
        }


        readonly protected SqlConnection _cnx;
        readonly protected OnLitSqlStatus _status;

        protected ServerUploadBase(SqlConnection cnx, OnLitSqlStatus status)
        {
            _status = status;
            _cnx = cnx;

            CheckServerVersion(_cnx);

            using (var cmd = _cnx.CreateCommand())
            {
                cmd.CommandText = DbCreateScript;

                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }


        public void Dispose()
        {
            FlushBuffer();
            _cnx.Close();
        }

        abstract public void FlushBuffer();
        abstract public void AppendWay(long index, string id, int type, string name, string speed, bool oneway, string tags);
        abstract public void AppendIntersection(long index, double lat, double lon);
        abstract public void AppendEdge(long wayId, long? left, long? right,
            bool oneWay,
            List<KeyValuePair<double, double>> lineStrip);


        static public ServerUploadBase CreateUploader(bool useBulkCopy, SqlConnection cnx, OnLitSqlStatus status)
        {
            return
                useBulkCopy ?
                new SqlServerBulkUpload(cnx, status) :
                (ServerUploadBase)(new SqlServerUpload(cnx, status));
        }
    }
}
