using System;
using System.Drawing;
using System.Windows.Forms;
using OsmMsSqlUpload.Analyser;
using OsmMsSqlUpload.Source;
using OsmMsSqlUpload.Target;

namespace OsmMsSqlUpload
{
    public partial class ProcessDlg : Form, IAnalyserProgress
    {
        public const int RefreshRate = 2;


        readonly dynamic _parameters;

        public ProcessDlg(dynamic parameters)
        {
            _parameters = parameters;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            System.Threading.ThreadPool.QueueUserWorkItem(BackgroundProcessing);
        }

        void PrcOnCompletion(object ex)
        {
            if (ex is Exception)
            {
                MessageBox.Show(this, "Upload process failed :\n" + ex,
                                "OSM2MSSQL4ROADS - Upload process failure",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Process Failure !";
            }
            else
            {
                MessageBox.Show(this, "File upload process completed",
                                "OSM2MSSQL4ROADS - Upload process failure",
                                MessageBoxButtons.OK);
                lblStatus.Text = "Upload success !";
            }
            btnProcessCompleted.Enabled = true;
        }

        private void btnProcessCompleted_Click(object sender, EventArgs e)
        {
            Close();
        }



        void BackgroundProcessing(object @unused)
        {
            ITempDatabase tempStore = null;
            try
            {
                tempStore = _parameters.UseDbAsTemp
                                ?                new SqlServerTemp(_parameters.ConnectionString)
                                : (ITempDatabase)new SqliteTemp   (_parameters.TempFile);

                using (var upload = ServerUploadBase.CreateUploader(
                    _parameters.BulkInsert,
                    (System.Data.SqlClient.SqlConnection)_parameters.Connection,
                    (OnLitSqlStatus)OnLitSqlStatus))
                using (var analyser = new OsmUploadAnalyser(tempStore, upload, this))
                {
                    Invoke((Action<string>)ActChangeStatus, "Processing souce file ...");

                    string sourceFile = _parameters.SourceFile;
                    if (sourceFile.EndsWith(".pbf", StringComparison.InvariantCultureIgnoreCase))
                        PBFReader.RunUpload(sourceFile, OnReaderProgress, analyser);
                    else
                        OSMReader.RunUpload(sourceFile, OnReaderProgress, analyser);

                    Invoke((Action<string>)ActChangeStatus, "Detecting intersections and edges ...");
                    analyser.Process();

                    Invoke((Action<string>)ActChangeStatus, "Adding Geospatial indexes on SQL Server ...");
                    using (var cmd = ((System.Data.SqlClient.SqlConnection)_parameters.Connection).CreateCommand())
                    {
                        cmd.CommandText = @"
CREATE SPATIAL INDEX GeoWayEdge ON OsmWayEdge(Way);
CREATE SPATIAL INDEX GeoIntesec ON PlnIntersection(Position);";

                        OnLitSqlStatus(true);
                        cmd.ExecuteNonQuery();
                        OnLitSqlStatus(false);
                    }
                }


                Invoke((Action<object>)PrcOnCompletion, new object());
            }
            catch (Exception ex)
            {
                Invoke((Action<object>)PrcOnCompletion, ex);
            }
            finally
            {
                if(tempStore!=null) tempStore.Dispose();
            }

        }

        void ActChangeStatus(string text)
        {
            lblAnaStep.ForeColor = SystemColors.ControlText;
            lblStatus.Text = text;
        }

        void ActOnReaderProgress(int percentage,long nbPoints,long nbWays)
        {
            pgSourceBar.Value = percentage;
            txtNbPoints.Text = nbPoints.ToString();
            txtNbWays.Text = nbWays.ToString();
        }

        void ActOnLitSqlStatus(bool active)
        {
            txtSqlStatus.BackColor = active ? Color.GreenYellow : Color.DarkGray;
            txtSqlStatus.ForeColor = active ? Color.Black       : Color.LightGray;
        }
        void OnLitSqlStatus(bool active)
        {
            Invoke((Action<bool>)ActOnLitSqlStatus, active);
        }
        
        void OnReaderProgress(int percentage,long nbPoints,long nbWays)
        {
            Invoke((Action<int, long, long>) ActOnReaderProgress, percentage, nbPoints, nbWays);
        }

        void ActOnAnalyserProgress(int percentage) { pgAnalyser.Value = percentage; }
        public void OnAnalyserProgress(int percentage)
        {
            Invoke((Action<int>)ActOnAnalyserProgress, percentage);
        }

        void ActOnAnalyserStep(string text) { lblAnaStep.Text = text; }
        public void OnAnalyserStep(string text)
        {
            Invoke((Action<string>)ActOnAnalyserStep, text);
        }

        void ActOnIntersectionNb(long nbIntersection) { txtNbIntersections.Text = nbIntersection.ToString(); }
        public void OnIntersectionNb(long nbIntersection)
        {
            Invoke((Action<long>)ActOnIntersectionNb, nbIntersection);
        }

        void ActOnTotalEdges(long nbEdges) { txtNbEdges.Text = nbEdges.ToString(); }
        public void OnTotalEdges(long nbEdges)
        {
            Invoke((Action<long>)ActOnTotalEdges, nbEdges);
        }
    }
}
