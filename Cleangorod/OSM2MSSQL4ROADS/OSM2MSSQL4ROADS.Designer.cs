namespace OsmMsSqlUpload
{
    partial class OSM2MSSQL4ROADS
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBrowseSource = new System.Windows.Forms.Button();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbIntegratedSecurity = new System.Windows.Forms.CheckBox();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbCleanData = new System.Windows.Forms.CheckBox();
            this.btnDbPopulate = new System.Windows.Forms.Button();
            this.cbDatabases = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTempFile = new System.Windows.Forms.TextBox();
            this.cbUseTargetDb = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnProceed = new System.Windows.Forms.Button();
            this.linkDupuisMe = new System.Windows.Forms.LinkLabel();
            this.cbBulkInsert = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnBrowseSource);
            this.groupBox1.Controls.Add(this.txtSourceFile);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 55);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source File:";
            // 
            // btnBrowseSource
            // 
            this.btnBrowseSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseSource.Location = new System.Drawing.Point(396, 19);
            this.btnBrowseSource.Name = "btnBrowseSource";
            this.btnBrowseSource.Size = new System.Drawing.Size(38, 23);
            this.btnBrowseSource.TabIndex = 1;
            this.btnBrowseSource.Text = "...";
            this.btnBrowseSource.UseVisualStyleBackColor = true;
            this.btnBrowseSource.Click += new System.EventHandler(this.btnBrowseSource_Click);
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourceFile.Location = new System.Drawing.Point(6, 22);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.Size = new System.Drawing.Size(384, 20);
            this.txtSourceFile.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtUserName);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cbIntegratedSecurity);
            this.groupBox2.Controls.Add(this.txtServerName);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 73);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Database Server:";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(234, 72);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(172, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Password:";
            // 
            // txtUserName
            // 
            this.txtUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUserName.Location = new System.Drawing.Point(234, 49);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(200, 20);
            this.txtUserName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(165, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "User Name:";
            // 
            // cbIntegratedSecurity
            // 
            this.cbIntegratedSecurity.AutoSize = true;
            this.cbIntegratedSecurity.Location = new System.Drawing.Point(9, 48);
            this.cbIntegratedSecurity.Name = "cbIntegratedSecurity";
            this.cbIntegratedSecurity.Size = new System.Drawing.Size(141, 17);
            this.cbIntegratedSecurity.TabIndex = 3;
            this.cbIntegratedSecurity.Text = "Windows Authentication";
            this.cbIntegratedSecurity.UseVisualStyleBackColor = true;
            this.cbIntegratedSecurity.CheckedChanged += new System.EventHandler(this.cbIntegratedSecurity_CheckedChanged);
            // 
            // txtServerName
            // 
            this.txtServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerName.Location = new System.Drawing.Point(84, 22);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(350, 20);
            this.txtServerName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Name:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.cbBulkInsert);
            this.groupBox3.Controls.Add(this.cbCleanData);
            this.groupBox3.Controls.Add(this.btnDbPopulate);
            this.groupBox3.Controls.Add(this.cbDatabases);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 179);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(440, 73);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Target Database";
            // 
            // cbCleanData
            // 
            this.cbCleanData.AutoSize = true;
            this.cbCleanData.Location = new System.Drawing.Point(9, 50);
            this.cbCleanData.Name = "cbCleanData";
            this.cbCleanData.Size = new System.Drawing.Size(118, 17);
            this.cbCleanData.TabIndex = 8;
            this.cbCleanData.Text = "Clean Existing Data";
            this.cbCleanData.UseVisualStyleBackColor = true;
            // 
            // btnDbPopulate
            // 
            this.btnDbPopulate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDbPopulate.Location = new System.Drawing.Point(359, 21);
            this.btnDbPopulate.Name = "btnDbPopulate";
            this.btnDbPopulate.Size = new System.Drawing.Size(75, 23);
            this.btnDbPopulate.TabIndex = 7;
            this.btnDbPopulate.Text = "Populate";
            this.btnDbPopulate.UseVisualStyleBackColor = true;
            this.btnDbPopulate.Click += new System.EventHandler(this.btnDbPopulate_Click);
            // 
            // cbDatabases
            // 
            this.cbDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDatabases.FormattingEnabled = true;
            this.cbDatabases.Location = new System.Drawing.Point(68, 23);
            this.cbDatabases.Name = "cbDatabases";
            this.cbDatabases.Size = new System.Drawing.Size(285, 21);
            this.cbDatabases.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Database:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.txtTempFile);
            this.groupBox4.Controls.Add(this.cbUseTargetDb);
            this.groupBox4.Location = new System.Drawing.Point(12, 258);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(440, 90);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Temporary File:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(298, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "(The file can be *very* large. Please ensure plenty of space...)";
            // 
            // txtTempFile
            // 
            this.txtTempFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTempFile.Location = new System.Drawing.Point(6, 42);
            this.txtTempFile.Name = "txtTempFile";
            this.txtTempFile.Size = new System.Drawing.Size(428, 20);
            this.txtTempFile.TabIndex = 10;
            this.txtTempFile.Text = "%TEMP%\\Temp.db";
            // 
            // cbUseTargetDb
            // 
            this.cbUseTargetDb.AutoSize = true;
            this.cbUseTargetDb.Location = new System.Drawing.Point(9, 19);
            this.cbUseTargetDb.Name = "cbUseTargetDb";
            this.cbUseTargetDb.Size = new System.Drawing.Size(194, 17);
            this.cbUseTargetDb.TabIndex = 9;
            this.cbUseTargetDb.Text = "Use Target Database (! Very slow !)";
            this.cbUseTargetDb.UseVisualStyleBackColor = true;
            this.cbUseTargetDb.CheckedChanged += new System.EventHandler(this.cbUseTargetDb_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 351);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(159, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "(C)opyright 2013 Laurent Dupuis";
            // 
            // btnProceed
            // 
            this.btnProceed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProceed.Location = new System.Drawing.Point(327, 354);
            this.btnProceed.Name = "btnProceed";
            this.btnProceed.Size = new System.Drawing.Size(125, 37);
            this.btnProceed.TabIndex = 11;
            this.btnProceed.Text = "Proceed";
            this.btnProceed.UseVisualStyleBackColor = true;
            this.btnProceed.Click += new System.EventHandler(this.btnProceed_Click);
            // 
            // linkDupuisMe
            // 
            this.linkDupuisMe.AutoSize = true;
            this.linkDupuisMe.Location = new System.Drawing.Point(15, 372);
            this.linkDupuisMe.Name = "linkDupuisMe";
            this.linkDupuisMe.Size = new System.Drawing.Size(82, 13);
            this.linkDupuisMe.TabIndex = 6;
            this.linkDupuisMe.TabStop = true;
            this.linkDupuisMe.Text = "www.dupuis.me";
            this.linkDupuisMe.VisitedLinkColor = System.Drawing.Color.Blue;
            // 
            // cbBulkInsert
            // 
            this.cbBulkInsert.AutoSize = true;
            this.cbBulkInsert.Checked = true;
            this.cbBulkInsert.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBulkInsert.Location = new System.Drawing.Point(133, 50);
            this.cbBulkInsert.Name = "cbBulkInsert";
            this.cbBulkInsert.Size = new System.Drawing.Size(98, 17);
            this.cbBulkInsert.TabIndex = 9;
            this.cbBulkInsert.Text = "Use Bulk Insert";
            this.cbBulkInsert.UseVisualStyleBackColor = true;
            // 
            // OSM2MSSQL4ROADS
            // 
            this.AcceptButton = this.btnProceed;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 402);
            this.Controls.Add(this.linkDupuisMe);
            this.Controls.Add(this.btnProceed);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximumSize = new System.Drawing.Size(1024, 440);
            this.MinimumSize = new System.Drawing.Size(480, 440);
            this.Name = "OSM2MSSQL4ROADS";
            this.Text = "OSM to MS-SQL for Roads";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBrowseSource;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbIntegratedSecurity;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox cbCleanData;
        private System.Windows.Forms.Button btnDbPopulate;
        private System.Windows.Forms.ComboBox cbDatabases;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTempFile;
        private System.Windows.Forms.CheckBox cbUseTargetDb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnProceed;
        private System.Windows.Forms.LinkLabel linkDupuisMe;
        private System.Windows.Forms.CheckBox cbBulkInsert;
    }
}

