using System;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Windows.Forms;

namespace OsmMsSqlUpload
{
    public partial class OSM2MSSQL4ROADS : Form
    {
        public OSM2MSSQL4ROADS()
        {
            InitializeComponent();
        }

        string GetConnectionString(string catalog = null)
        {
            var cnxStringBuilder = new SqlConnectionStringBuilder();
            cnxStringBuilder.DataSource = txtServerName.Text;
            cnxStringBuilder.IntegratedSecurity = cbIntegratedSecurity.Checked;
            cnxStringBuilder.AsynchronousProcessing = true;

            if(!cnxStringBuilder.IntegratedSecurity)
            {
                cnxStringBuilder.UserID     = txtUserName.Text.Trim();
                cnxStringBuilder.Password   = txtPassword.Text.Trim();
            }
            if(!string.IsNullOrEmpty(catalog))
                cnxStringBuilder.InitialCatalog = catalog;
            return cnxStringBuilder.ConnectionString;
        }

        private void btnDbPopulate_Click(object sender, EventArgs e)
        {
            cbDatabases.Items.Clear();
            try
            {
                using (var cnx = new SqlConnection(GetConnectionString()))
                {
                    cnx.Open();
                    using(var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = @"select name from master.sys.databases where database_id>4";
                        using(var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read()) cbDatabases.Items.Add(rdr.GetString(0));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Unable to get the list of database :\n" + ex.Message,
                                "OSM2MSSQL4ROADS - Database Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
        {
            var txtEnabled = !cbIntegratedSecurity.Checked;
            txtUserName.Enabled =
            txtPassword.Enabled = txtEnabled;
        }

        static bool CheckIfDatabaseEmpty(SqlConnection cnx)
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


        private void btnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                var dbName = cbDatabases.Text.Trim();
                var cnxString = GetConnectionString(dbName);
                using (var cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    if(string.IsNullOrEmpty(dbName))
                    {
                        cbDatabases.Text = cnx.Database;
                    }

                    if(CheckIfDatabaseEmpty(cnx) && !cbCleanData.Checked)
                    {
                        MessageBox.Show(this, "The database is not empty. Please check 'Clean Data'.",
                                        "OSM2MSSQL4ROADS - Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;                        
                    }

                    dynamic parameters = new ExpandoObject();

                    parameters.Connection = cnx;
                    parameters.ConnectionString = cnxString; 
                    parameters.SourceFile = txtSourceFile.Text;
                    parameters.UseDbAsTemp = cbUseTargetDb.Checked;
                    parameters.BulkInsert = cbBulkInsert.Checked;
                    parameters.TempFile =
                        Environment.ExpandEnvironmentVariables(
                                txtTempFile.Text.Trim()
                            );

                    if(!File.Exists(parameters.SourceFile))
                    {
                        MessageBox.Show(this, "Unable to access source file. Please check path.",
                                        "OSM2MSSQL4ROADS - Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }




                    var dialog = new ProcessDlg(parameters);
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unable to initialize upload process :\n" + ex.Message,
                                "OSM2MSSQL4ROADS - Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbUseTargetDb_CheckedChanged(object sender, EventArgs e)
        {
            txtTempFile.Enabled = !cbUseTargetDb.Checked;
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();

            open.AddExtension = true;
            open.CheckFileExists = true;
            open.Filter = @"BZip2 OSM XML (*.osm.bz2)|*.osm.bz2|Binary PBF OSM (*.osm.pbf)|*.osm.pbf";
            if(open.ShowDialog(this)!=DialogResult.OK) return;

            txtSourceFile.Text = open.FileName;

        }
    }
}
