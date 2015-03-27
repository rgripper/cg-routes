using OsmMsSqlUpload.Analyser;
using OsmMsSqlUpload.Source;
using OsmMsSqlUpload.Target;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace OsmMsSqlUpload
{
    public class Loader
    {
        dynamic _parameters = new ExpandoObject();

        public void Load(string filePath, string connectionName)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("Unable to access source file. Please check path.");
            }

            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            using (var cnx = new SqlConnection(connectionString))
            {
                cnx.Open();

                if (CheckIfDatabaseEmpty(cnx))
                {
                    ClearDatabase(cnx);
                }

                _parameters.Connection = cnx;
                _parameters.ConnectionString = connectionString;
                _parameters.SourceFile = filePath;
                _parameters.UseDbAsTemp = false;
                _parameters.BulkInsert = true;
                _parameters.TempFile = Path.GetTempFileName();

                BackgroundProcessing();
            }
        }

        bool CheckIfDatabaseEmpty(SqlConnection cnx)
        {
            using (var cmd = cnx.CreateCommand())
            {
                cmd.CommandText =
                    @"
SELECT ISNULL( (
	SELECT 1
	WHERE
		OBJECT_ID ( 'dbo.PlnEdge',        'U' ) IS NOT NULL OR
		OBJECT_ID ( 'dbo.OsmWayEdge',     'U' ) IS NOT NULL OR
		OBJECT_ID ( 'dbo.PlnIntersection','U' ) IS NOT NULL OR
		OBJECT_ID ( 'dbo.OsmWays',        'U' ) IS NOT NULL OR
		OBJECT_ID ( 'dbo.OsmWayTypes',    'U' ) IS NOT NULL OR
		OBJECT_ID ( 'dbo.OsmPolygon',     'U' ) IS NOT NULL 
	),0)";
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        void ClearDatabase(SqlConnection cnx)
        {
            string[] tables = { "dbo.PlnEdge", "dbo.OsmWayEdge", "dbo.PlnIntersection", "dbo.OsmWay", "dbo.OsmWayType", "dbo.OsmPolygon" };
            foreach (var tableName in tables)
            {
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = String.Format("DROP TABLE {0}", tableName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        class DummyAnalyserProgress : IAnalyserProgress
        {
            public void OnAnalyserProgress(int percentage) { }
            public void OnAnalyserStep(string text) { }

            public void OnIntersectionNb(long nbIntersection) { }
            public void OnTotalEdges(long nbEdges) { }
        }

        void BackgroundProcessing()
        {
            OnReaderProgress onReader = (x, y, z) => { };
            OnLitSqlStatus onStatus = (active) => { };

            ITempDatabase tempStore = _parameters.UseDbAsTemp
                            ? new SqlServerTemp(_parameters.ConnectionString)
                            : (ITempDatabase)new SqliteTemp(_parameters.TempFile);

            using (var upload = ServerUploadBase.CreateUploader(
                _parameters.BulkInsert,
                (System.Data.SqlClient.SqlConnection)_parameters.Connection,
                (OnLitSqlStatus)onStatus))
            using (var analyser = new OsmUploadAnalyser(tempStore, upload, new DummyAnalyserProgress()))
            {


                string sourceFile = _parameters.SourceFile;
                if (sourceFile.EndsWith(".pbf", StringComparison.InvariantCultureIgnoreCase))
                    PBFReader.RunUpload(sourceFile, onReader, analyser);
                else
                    OSMReader.RunUpload(sourceFile, onReader, analyser);


                analyser.Process();


                using (var cmd = ((System.Data.SqlClient.SqlConnection)_parameters.Connection).CreateCommand())
                {
                    cmd.CommandText = @"
CREATE SPATIAL INDEX GeoWayEdge ON OsmWayEdge(Way);
CREATE SPATIAL INDEX GeoIntesec ON PlnIntersection(Position);";
                    cmd.CommandTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                    cmd.ExecuteNonQuery();
                }
            }


        }
    }
}
