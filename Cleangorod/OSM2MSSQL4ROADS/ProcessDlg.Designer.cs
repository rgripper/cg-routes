namespace OsmMsSqlUpload
{
    partial class ProcessDlg
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
            this.txtNbWays = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNbPoints = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pgSourceBar = new System.Windows.Forms.ProgressBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSqlStatus = new System.Windows.Forms.TextBox();
            this.txtNbEdges = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtNbIntersections = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblAnaStep = new System.Windows.Forms.Label();
            this.pgAnalyser = new System.Windows.Forms.ProgressBar();
            this.btnProcessCompleted = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txtNbWays);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtNbPoints);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.pgSourceBar);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(528, 73);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source File Scan Progress:";
            // 
            // txtNbWays
            // 
            this.txtNbWays.Location = new System.Drawing.Point(200, 47);
            this.txtNbWays.Name = "txtNbWays";
            this.txtNbWays.ReadOnly = true;
            this.txtNbWays.Size = new System.Drawing.Size(100, 20);
            this.txtNbWays.TabIndex = 3;
            this.txtNbWays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(157, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Ways:";
            // 
            // txtNbPoints
            // 
            this.txtNbPoints.Location = new System.Drawing.Point(51, 48);
            this.txtNbPoints.Name = "txtNbPoints";
            this.txtNbPoints.ReadOnly = true;
            this.txtNbPoints.Size = new System.Drawing.Size(100, 20);
            this.txtNbPoints.TabIndex = 1;
            this.txtNbPoints.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Points:";
            // 
            // pgSourceBar
            // 
            this.pgSourceBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgSourceBar.Location = new System.Drawing.Point(6, 19);
            this.pgSourceBar.MarqueeAnimationSpeed = 10;
            this.pgSourceBar.Name = "pgSourceBar";
            this.pgSourceBar.Size = new System.Drawing.Size(516, 23);
            this.pgSourceBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgSourceBar.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.txtSqlStatus);
            this.groupBox2.Controls.Add(this.txtNbEdges);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtNbIntersections);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lblAnaStep);
            this.groupBox2.Controls.Add(this.pgAnalyser);
            this.groupBox2.Location = new System.Drawing.Point(12, 91);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(528, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MS SQL Upload Process";
            // 
            // txtSqlStatus
            // 
            this.txtSqlStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSqlStatus.BackColor = System.Drawing.Color.DarkGray;
            this.txtSqlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSqlStatus.ForeColor = System.Drawing.Color.LightGray;
            this.txtSqlStatus.Location = new System.Drawing.Point(453, 23);
            this.txtSqlStatus.Name = "txtSqlStatus";
            this.txtSqlStatus.ReadOnly = true;
            this.txtSqlStatus.Size = new System.Drawing.Size(63, 20);
            this.txtSqlStatus.TabIndex = 6;
            this.txtSqlStatus.Text = "<SQL>";
            this.txtSqlStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtNbEdges
            // 
            this.txtNbEdges.Location = new System.Drawing.Point(292, 22);
            this.txtNbEdges.Name = "txtNbEdges";
            this.txtNbEdges.ReadOnly = true;
            this.txtNbEdges.Size = new System.Drawing.Size(100, 20);
            this.txtNbEdges.TabIndex = 5;
            this.txtNbEdges.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(219, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Total Edges:";
            // 
            // txtNbIntersections
            // 
            this.txtNbIntersections.Location = new System.Drawing.Point(113, 22);
            this.txtNbIntersections.Name = "txtNbIntersections";
            this.txtNbIntersections.ReadOnly = true;
            this.txtNbIntersections.Size = new System.Drawing.Size(100, 20);
            this.txtNbIntersections.TabIndex = 3;
            this.txtNbIntersections.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Total Intersections:";
            // 
            // lblAnaStep
            // 
            this.lblAnaStep.AutoSize = true;
            this.lblAnaStep.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblAnaStep.Location = new System.Drawing.Point(10, 55);
            this.lblAnaStep.Name = "lblAnaStep";
            this.lblAnaStep.Size = new System.Drawing.Size(183, 13);
            this.lblAnaStep.TabIndex = 1;
            this.lblAnaStep.Text = "(Idle - Waiting for upload to complete)";
            // 
            // pgAnalyser
            // 
            this.pgAnalyser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgAnalyser.Location = new System.Drawing.Point(9, 71);
            this.pgAnalyser.Name = "pgAnalyser";
            this.pgAnalyser.Size = new System.Drawing.Size(513, 23);
            this.pgAnalyser.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgAnalyser.TabIndex = 0;
            // 
            // btnProcessCompleted
            // 
            this.btnProcessCompleted.Enabled = false;
            this.btnProcessCompleted.Location = new System.Drawing.Point(465, 197);
            this.btnProcessCompleted.Name = "btnProcessCompleted";
            this.btnProcessCompleted.Size = new System.Drawing.Size(75, 23);
            this.btnProcessCompleted.TabIndex = 2;
            this.btnProcessCompleted.Text = "Close";
            this.btnProcessCompleted.UseVisualStyleBackColor = true;
            this.btnProcessCompleted.Click += new System.EventHandler(this.btnProcessCompleted_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(15, 202);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(154, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Please wait while processing ...";
            // 
            // ProcessDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 233);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnProcessCompleted);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProcessDlg";
            this.Text = "Upload Progress";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNbPoints;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar pgSourceBar;
        private System.Windows.Forms.TextBox txtNbWays;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtSqlStatus;
        private System.Windows.Forms.TextBox txtNbEdges;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtNbIntersections;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblAnaStep;
        private System.Windows.Forms.ProgressBar pgAnalyser;
        private System.Windows.Forms.Button btnProcessCompleted;
        private System.Windows.Forms.Label lblStatus;
    }
}