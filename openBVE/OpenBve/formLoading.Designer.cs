namespace OpenBve {
    partial class formLoading {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
        	this.components = new System.ComponentModel.Container();
        	this.buttonIgnore = new System.Windows.Forms.Button();
        	this.buttonCancel = new System.Windows.Forms.Button();
        	this.labelPanel = new System.Windows.Forms.Label();
        	this.labelSeparator = new System.Windows.Forms.Label();
        	this.labelErrorsSeparator = new System.Windows.Forms.Label();
        	this.buttonSave = new System.Windows.Forms.Button();
        	this.panelProblems = new System.Windows.Forms.Panel();
        	this.labelProblemsTitle = new System.Windows.Forms.Label();
        	this.listviewProblems = new System.Windows.Forms.ListView();
        	this.columnheaderType = new System.Windows.Forms.ColumnHeader();
        	this.columnheaderDescription = new System.Windows.Forms.ColumnHeader();
        	this.labelProblemsBackground = new System.Windows.Forms.Label();
        	this.panelLoading = new System.Windows.Forms.Panel();
        	this.pictureboxBanner = new System.Windows.Forms.PictureBox();
        	this.labelTrainPercentage = new System.Windows.Forms.Label();
        	this.labelRoutePercentage = new System.Windows.Forms.Label();
        	this.labelTrain = new System.Windows.Forms.Label();
        	this.progressbarTrain = new System.Windows.Forms.ProgressBar();
        	this.labelRoute = new System.Windows.Forms.Label();
        	this.progressbarRoute = new System.Windows.Forms.ProgressBar();
        	this.labelLoadingSeparator = new System.Windows.Forms.Label();
        	this.timerUpdate = new System.Windows.Forms.Timer(this.components);
        	this.panelAlmost = new System.Windows.Forms.Panel();
        	this.buttonIssues = new System.Windows.Forms.Button();
        	this.labelHelp = new System.Windows.Forms.Label();
        	this.labelFilesNotFoundValue = new System.Windows.Forms.Label();
        	this.labelFilesNotFoundCaption = new System.Windows.Forms.Label();
        	this.labelAlmostTitle = new System.Windows.Forms.Label();
        	this.labelAlmostSeparator = new System.Windows.Forms.Label();
        	this.labelAlmostBackground = new System.Windows.Forms.Label();
        	this.panelProblems.SuspendLayout();
        	this.panelLoading.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pictureboxBanner)).BeginInit();
        	this.panelAlmost.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// buttonIgnore
        	// 
        	this.buttonIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonIgnore.BackColor = System.Drawing.SystemColors.ButtonFace;
        	this.buttonIgnore.Location = new System.Drawing.Point(332, 193);
        	this.buttonIgnore.Name = "buttonIgnore";
        	this.buttonIgnore.Size = new System.Drawing.Size(96, 24);
        	this.buttonIgnore.TabIndex = 7;
        	this.buttonIgnore.Text = "Ignore";
        	this.buttonIgnore.UseVisualStyleBackColor = true;
        	this.buttonIgnore.Visible = false;
        	this.buttonIgnore.Click += new System.EventHandler(this.buttonIgnore_Click);
        	// 
        	// buttonCancel
        	// 
        	this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
        	this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.buttonCancel.Location = new System.Drawing.Point(436, 193);
        	this.buttonCancel.Name = "buttonCancel";
        	this.buttonCancel.Size = new System.Drawing.Size(96, 24);
        	this.buttonCancel.TabIndex = 8;
        	this.buttonCancel.Text = "Cancel";
        	this.buttonCancel.UseVisualStyleBackColor = true;
        	this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
        	// 
        	// labelPanel
        	// 
        	this.labelPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelPanel.BackColor = System.Drawing.Color.Silver;
        	this.labelPanel.Location = new System.Drawing.Point(0, 184);
        	this.labelPanel.Name = "labelPanel";
        	this.labelPanel.Size = new System.Drawing.Size(540, 40);
        	this.labelPanel.TabIndex = 5;
        	// 
        	// labelSeparator
        	// 
        	this.labelSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelSeparator.BackColor = System.Drawing.Color.White;
        	this.labelSeparator.Location = new System.Drawing.Point(0, 184);
        	this.labelSeparator.Name = "labelSeparator";
        	this.labelSeparator.Size = new System.Drawing.Size(540, 2);
        	this.labelSeparator.TabIndex = 4;
        	// 
        	// labelErrorsSeparator
        	// 
        	this.labelErrorsSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelErrorsSeparator.BackColor = System.Drawing.Color.White;
        	this.labelErrorsSeparator.Location = new System.Drawing.Point(0, 32);
        	this.labelErrorsSeparator.Name = "labelErrorsSeparator";
        	this.labelErrorsSeparator.Size = new System.Drawing.Size(540, 2);
        	this.labelErrorsSeparator.TabIndex = 2;
        	// 
        	// buttonSave
        	// 
        	this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.buttonSave.BackColor = System.Drawing.SystemColors.ButtonFace;
        	this.buttonSave.Location = new System.Drawing.Point(8, 193);
        	this.buttonSave.Name = "buttonSave";
        	this.buttonSave.Size = new System.Drawing.Size(96, 24);
        	this.buttonSave.TabIndex = 6;
        	this.buttonSave.Text = "Save report...";
        	this.buttonSave.UseVisualStyleBackColor = true;
        	this.buttonSave.Visible = false;
        	this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
        	// 
        	// panelProblems
        	// 
        	this.panelProblems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.panelProblems.BackColor = System.Drawing.Color.Linen;
        	this.panelProblems.Controls.Add(this.labelProblemsTitle);
        	this.panelProblems.Controls.Add(this.listviewProblems);
        	this.panelProblems.Controls.Add(this.labelErrorsSeparator);
        	this.panelProblems.Controls.Add(this.labelProblemsBackground);
        	this.panelProblems.Location = new System.Drawing.Point(0, 0);
        	this.panelProblems.Name = "panelProblems";
        	this.panelProblems.Size = new System.Drawing.Size(540, 184);
        	this.panelProblems.TabIndex = 0;
        	this.panelProblems.Visible = false;
        	// 
        	// labelProblemsTitle
        	// 
        	this.labelProblemsTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelProblemsTitle.AutoSize = true;
        	this.labelProblemsTitle.BackColor = System.Drawing.Color.Firebrick;
        	this.labelProblemsTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelProblemsTitle.ForeColor = System.Drawing.Color.White;
        	this.labelProblemsTitle.Location = new System.Drawing.Point(8, 8);
        	this.labelProblemsTitle.Name = "labelProblemsTitle";
        	this.labelProblemsTitle.Size = new System.Drawing.Size(66, 16);
        	this.labelProblemsTitle.TabIndex = 5;
        	this.labelProblemsTitle.Text = "Problems";
        	// 
        	// listviewProblems
        	// 
        	this.listviewProblems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.listviewProblems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        	        	        	this.columnheaderType,
        	        	        	this.columnheaderDescription});
        	this.listviewProblems.FullRowSelect = true;
        	this.listviewProblems.GridLines = true;
        	this.listviewProblems.Location = new System.Drawing.Point(8, 40);
        	this.listviewProblems.MultiSelect = false;
        	this.listviewProblems.Name = "listviewProblems";
        	this.listviewProblems.Size = new System.Drawing.Size(524, 136);
        	this.listviewProblems.TabIndex = 6;
        	this.listviewProblems.UseCompatibleStateImageBehavior = false;
        	this.listviewProblems.View = System.Windows.Forms.View.Details;
        	// 
        	// columnheaderType
        	// 
        	this.columnheaderType.Text = "Type";
        	// 
        	// columnheaderDescription
        	// 
        	this.columnheaderDescription.Text = "Description";
        	// 
        	// labelProblemsBackground
        	// 
        	this.labelProblemsBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelProblemsBackground.BackColor = System.Drawing.Color.Firebrick;
        	this.labelProblemsBackground.Location = new System.Drawing.Point(0, 0);
        	this.labelProblemsBackground.Name = "labelProblemsBackground";
        	this.labelProblemsBackground.Size = new System.Drawing.Size(540, 32);
        	this.labelProblemsBackground.TabIndex = 4;
        	// 
        	// panelLoading
        	// 
        	this.panelLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.panelLoading.BackColor = System.Drawing.Color.Lavender;
        	this.panelLoading.Controls.Add(this.pictureboxBanner);
        	this.panelLoading.Controls.Add(this.labelTrainPercentage);
        	this.panelLoading.Controls.Add(this.labelRoutePercentage);
        	this.panelLoading.Controls.Add(this.labelTrain);
        	this.panelLoading.Controls.Add(this.progressbarTrain);
        	this.panelLoading.Controls.Add(this.labelRoute);
        	this.panelLoading.Controls.Add(this.progressbarRoute);
        	this.panelLoading.Controls.Add(this.labelLoadingSeparator);
        	this.panelLoading.Location = new System.Drawing.Point(0, 0);
        	this.panelLoading.Name = "panelLoading";
        	this.panelLoading.Size = new System.Drawing.Size(540, 184);
        	this.panelLoading.TabIndex = 0;
        	// 
        	// pictureboxBanner
        	// 
        	this.pictureboxBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.pictureboxBanner.Location = new System.Drawing.Point(0, 0);
        	this.pictureboxBanner.Name = "pictureboxBanner";
        	this.pictureboxBanner.Size = new System.Drawing.Size(540, 96);
        	this.pictureboxBanner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        	this.pictureboxBanner.TabIndex = 15;
        	this.pictureboxBanner.TabStop = false;
        	// 
        	// labelTrainPercentage
        	// 
        	this.labelTrainPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.labelTrainPercentage.Location = new System.Drawing.Point(484, 144);
        	this.labelTrainPercentage.Name = "labelTrainPercentage";
        	this.labelTrainPercentage.Size = new System.Drawing.Size(48, 16);
        	this.labelTrainPercentage.TabIndex = 14;
        	this.labelTrainPercentage.Text = "0%";
        	this.labelTrainPercentage.TextAlign = System.Drawing.ContentAlignment.TopRight;
        	// 
        	// labelRoutePercentage
        	// 
        	this.labelRoutePercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        	this.labelRoutePercentage.Location = new System.Drawing.Point(484, 104);
        	this.labelRoutePercentage.Name = "labelRoutePercentage";
        	this.labelRoutePercentage.Size = new System.Drawing.Size(48, 16);
        	this.labelRoutePercentage.TabIndex = 13;
        	this.labelRoutePercentage.Text = "0%";
        	this.labelRoutePercentage.TextAlign = System.Drawing.ContentAlignment.TopRight;
        	// 
        	// labelTrain
        	// 
        	this.labelTrain.AutoSize = true;
        	this.labelTrain.Location = new System.Drawing.Point(8, 144);
        	this.labelTrain.Name = "labelTrain";
        	this.labelTrain.Size = new System.Drawing.Size(77, 13);
        	this.labelTrain.TabIndex = 12;
        	this.labelTrain.Text = "Loading train...";
        	// 
        	// progressbarTrain
        	// 
        	this.progressbarTrain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.progressbarTrain.Location = new System.Drawing.Point(8, 160);
        	this.progressbarTrain.Name = "progressbarTrain";
        	this.progressbarTrain.Size = new System.Drawing.Size(524, 16);
        	this.progressbarTrain.TabIndex = 11;
        	// 
        	// labelRoute
        	// 
        	this.labelRoute.AutoSize = true;
        	this.labelRoute.Location = new System.Drawing.Point(8, 104);
        	this.labelRoute.Name = "labelRoute";
        	this.labelRoute.Size = new System.Drawing.Size(81, 13);
        	this.labelRoute.TabIndex = 10;
        	this.labelRoute.Text = "Loading route...";
        	// 
        	// progressbarRoute
        	// 
        	this.progressbarRoute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.progressbarRoute.Location = new System.Drawing.Point(8, 120);
        	this.progressbarRoute.Name = "progressbarRoute";
        	this.progressbarRoute.Size = new System.Drawing.Size(524, 16);
        	this.progressbarRoute.TabIndex = 9;
        	// 
        	// labelLoadingSeparator
        	// 
        	this.labelLoadingSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelLoadingSeparator.BackColor = System.Drawing.Color.White;
        	this.labelLoadingSeparator.Location = new System.Drawing.Point(0, 96);
        	this.labelLoadingSeparator.Name = "labelLoadingSeparator";
        	this.labelLoadingSeparator.Size = new System.Drawing.Size(540, 2);
        	this.labelLoadingSeparator.TabIndex = 6;
        	// 
        	// timerUpdate
        	// 
        	this.timerUpdate.Enabled = true;
        	this.timerUpdate.Interval = 25;
        	this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
        	// 
        	// panelAlmost
        	// 
        	this.panelAlmost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.panelAlmost.BackColor = System.Drawing.Color.Cornsilk;
        	this.panelAlmost.Controls.Add(this.buttonIssues);
        	this.panelAlmost.Controls.Add(this.labelHelp);
        	this.panelAlmost.Controls.Add(this.labelFilesNotFoundValue);
        	this.panelAlmost.Controls.Add(this.labelFilesNotFoundCaption);
        	this.panelAlmost.Controls.Add(this.labelAlmostTitle);
        	this.panelAlmost.Controls.Add(this.labelAlmostSeparator);
        	this.panelAlmost.Controls.Add(this.labelAlmostBackground);
        	this.panelAlmost.Location = new System.Drawing.Point(0, 0);
        	this.panelAlmost.Name = "panelAlmost";
        	this.panelAlmost.Size = new System.Drawing.Size(540, 184);
        	this.panelAlmost.TabIndex = 0;
        	this.panelAlmost.Visible = false;
        	// 
        	// buttonIssues
        	// 
        	this.buttonIssues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonIssues.BackColor = System.Drawing.SystemColors.ButtonFace;
        	this.buttonIssues.Location = new System.Drawing.Point(435, 152);
        	this.buttonIssues.Name = "buttonIssues";
        	this.buttonIssues.Size = new System.Drawing.Size(96, 24);
        	this.buttonIssues.TabIndex = 5;
        	this.buttonIssues.Text = "Show issues";
        	this.buttonIssues.UseVisualStyleBackColor = false;
        	this.buttonIssues.Click += new System.EventHandler(this.buttonIssues_Click);
        	// 
        	// labelHelp
        	// 
        	this.labelHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelHelp.AutoEllipsis = true;
        	this.labelHelp.Location = new System.Drawing.Point(8, 40);
        	this.labelHelp.Name = "labelHelp";
        	this.labelHelp.Size = new System.Drawing.Size(522, 104);
        	this.labelHelp.TabIndex = 2;
        	this.labelHelp.Text = "Some issues were encountered while loading the route and train.";
        	// 
        	// labelFilesNotFoundValue
        	// 
        	this.labelFilesNotFoundValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.labelFilesNotFoundValue.AutoEllipsis = true;
        	this.labelFilesNotFoundValue.Location = new System.Drawing.Point(168, 152);
        	this.labelFilesNotFoundValue.Name = "labelFilesNotFoundValue";
        	this.labelFilesNotFoundValue.Size = new System.Drawing.Size(96, 16);
        	this.labelFilesNotFoundValue.TabIndex = 4;
        	this.labelFilesNotFoundValue.Text = "0";
        	// 
        	// labelFilesNotFoundCaption
        	// 
        	this.labelFilesNotFoundCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.labelFilesNotFoundCaption.AutoEllipsis = true;
        	this.labelFilesNotFoundCaption.Location = new System.Drawing.Point(8, 152);
        	this.labelFilesNotFoundCaption.Name = "labelFilesNotFoundCaption";
        	this.labelFilesNotFoundCaption.Size = new System.Drawing.Size(160, 16);
        	this.labelFilesNotFoundCaption.TabIndex = 3;
        	this.labelFilesNotFoundCaption.Text = "Files not found:";
        	this.labelFilesNotFoundCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
        	// 
        	// labelAlmostTitle
        	// 
        	this.labelAlmostTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelAlmostTitle.AutoSize = true;
        	this.labelAlmostTitle.BackColor = System.Drawing.Color.DarkGoldenrod;
        	this.labelAlmostTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelAlmostTitle.ForeColor = System.Drawing.Color.White;
        	this.labelAlmostTitle.Location = new System.Drawing.Point(8, 8);
        	this.labelAlmostTitle.Name = "labelAlmostTitle";
        	this.labelAlmostTitle.Size = new System.Drawing.Size(129, 16);
        	this.labelAlmostTitle.TabIndex = 1;
        	this.labelAlmostTitle.Text = "Almost ready to start";
        	// 
        	// labelAlmostSeparator
        	// 
        	this.labelAlmostSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelAlmostSeparator.BackColor = System.Drawing.Color.White;
        	this.labelAlmostSeparator.Location = new System.Drawing.Point(0, 32);
        	this.labelAlmostSeparator.Name = "labelAlmostSeparator";
        	this.labelAlmostSeparator.Size = new System.Drawing.Size(540, 2);
        	this.labelAlmostSeparator.TabIndex = 6;
        	// 
        	// labelAlmostBackground
        	// 
        	this.labelAlmostBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.labelAlmostBackground.BackColor = System.Drawing.Color.DarkGoldenrod;
        	this.labelAlmostBackground.Location = new System.Drawing.Point(0, 0);
        	this.labelAlmostBackground.Name = "labelAlmostBackground";
        	this.labelAlmostBackground.Size = new System.Drawing.Size(540, 32);
        	this.labelAlmostBackground.TabIndex = 0;
        	// 
        	// formLoading
        	// 
        	this.AcceptButton = this.buttonIgnore;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.CancelButton = this.buttonCancel;
        	this.ClientSize = new System.Drawing.Size(540, 224);
        	this.Controls.Add(this.labelSeparator);
        	this.Controls.Add(this.buttonSave);
        	this.Controls.Add(this.buttonCancel);
        	this.Controls.Add(this.buttonIgnore);
        	this.Controls.Add(this.labelPanel);
        	this.Controls.Add(this.panelLoading);
        	this.Controls.Add(this.panelProblems);
        	this.Controls.Add(this.panelAlmost);
        	this.Name = "formLoading";
        	this.ShowIcon = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "openBVE";
        	this.Load += new System.EventHandler(this.formLoading_Load);
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formLoading_FormClosing);
        	this.panelProblems.ResumeLayout(false);
        	this.panelProblems.PerformLayout();
        	this.panelLoading.ResumeLayout(false);
        	this.panelLoading.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.pictureboxBanner)).EndInit();
        	this.panelAlmost.ResumeLayout(false);
        	this.panelAlmost.PerformLayout();
        	this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelPanel;
        private System.Windows.Forms.Label labelSeparator;
        private System.Windows.Forms.Label labelErrorsSeparator;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Panel panelProblems;
        private System.Windows.Forms.Label labelProblemsTitle;
        private System.Windows.Forms.ListView listviewProblems;
        private System.Windows.Forms.ColumnHeader columnheaderType;
        private System.Windows.Forms.ColumnHeader columnheaderDescription;
        private System.Windows.Forms.Label labelProblemsBackground;
        private System.Windows.Forms.Panel panelLoading;
        private System.Windows.Forms.Label labelLoadingSeparator;
        private System.Windows.Forms.Label labelTrain;
        private System.Windows.Forms.ProgressBar progressbarTrain;
        private System.Windows.Forms.Label labelRoute;
        private System.Windows.Forms.ProgressBar progressbarRoute;
        private System.Windows.Forms.Label labelTrainPercentage;
        private System.Windows.Forms.Label labelRoutePercentage;
        private System.Windows.Forms.PictureBox pictureboxBanner;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Panel panelAlmost;
        private System.Windows.Forms.Label labelAlmostTitle;
        private System.Windows.Forms.Label labelAlmostSeparator;
        private System.Windows.Forms.Label labelAlmostBackground;
        private System.Windows.Forms.Label labelFilesNotFoundCaption;
        private System.Windows.Forms.Label labelFilesNotFoundValue;
        private System.Windows.Forms.Button buttonIssues;
        private System.Windows.Forms.Label labelHelp;
    }
}