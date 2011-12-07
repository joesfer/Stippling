namespace BlueNoise
{
    partial class StipplingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StipplingForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBoxWangTilesPath = new System.Windows.Forms.TextBox();
            this.labelTilesFile = new System.Windows.Forms.Label();
            this.buttonBrowseTiles = new System.Windows.Forms.Button();
            this.labelToneScale = new System.Windows.Forms.Label();
            this.toneScale = new System.Windows.Forms.TrackBar();
            this.browseSourceButton = new System.Windows.Forms.Button();
            this.sourceImagePath = new System.Windows.Forms.TextBox();
            this.sourceImageLabel = new System.Windows.Forms.Label();
            this.buttonGO = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxSource = new System.Windows.Forms.PictureBox();
            this.pictureBoxResult = new System.Windows.Forms.PictureBox();
            this.saveImgButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonIncremental = new System.Windows.Forms.RadioButton();
            this.radioButtonWangTiles = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.wtDestFilePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.wtBrowseDestFile = new System.Windows.Forms.Button();
            this.wtSamplesPerTile = new System.Windows.Forms.DomainUpDown();
            this.wtEdgeColors = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.wtDebugStepOnce = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.wtDebugPlayStop = new System.Windows.Forms.Button();
            this.wtGenerate = new System.Windows.Forms.Button();
            this.wtDebugSteps = new System.Windows.Forms.CheckBox();
            this.wtDebugImg = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.wtConsole = new System.Windows.Forms.ListBox();
            this.wtProgress = new System.Windows.Forms.ProgressBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toneScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wtEdgeColors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wtDebugImg)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1077, 830);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.textBoxWangTilesPath);
            this.tabPage1.Controls.Add(this.labelTilesFile);
            this.tabPage1.Controls.Add(this.buttonBrowseTiles);
            this.tabPage1.Controls.Add(this.labelToneScale);
            this.tabPage1.Controls.Add(this.toneScale);
            this.tabPage1.Controls.Add(this.browseSourceButton);
            this.tabPage1.Controls.Add(this.sourceImagePath);
            this.tabPage1.Controls.Add(this.sourceImageLabel);
            this.tabPage1.Controls.Add(this.buttonGO);
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Controls.Add(this.saveImgButton);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1069, 801);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Stippling";
            // 
            // textBoxWangTilesPath
            // 
            this.textBoxWangTilesPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxWangTilesPath.Location = new System.Drawing.Point(307, 741);
            this.textBoxWangTilesPath.Name = "textBoxWangTilesPath";
            this.textBoxWangTilesPath.ReadOnly = true;
            this.textBoxWangTilesPath.Size = new System.Drawing.Size(312, 22);
            this.textBoxWangTilesPath.TabIndex = 27;
            // 
            // labelTilesFile
            // 
            this.labelTilesFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTilesFile.AutoSize = true;
            this.labelTilesFile.Location = new System.Drawing.Point(192, 744);
            this.labelTilesFile.Name = "labelTilesFile";
            this.labelTilesFile.Size = new System.Drawing.Size(109, 17);
            this.labelTilesFile.TabIndex = 29;
            this.labelTilesFile.Text = "Wang Tiles File:";
            // 
            // buttonBrowseTiles
            // 
            this.buttonBrowseTiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBrowseTiles.Location = new System.Drawing.Point(625, 739);
            this.buttonBrowseTiles.Name = "buttonBrowseTiles";
            this.buttonBrowseTiles.Size = new System.Drawing.Size(100, 26);
            this.buttonBrowseTiles.TabIndex = 28;
            this.buttonBrowseTiles.Text = "Browse";
            this.buttonBrowseTiles.UseVisualStyleBackColor = true;
            this.buttonBrowseTiles.Click += new System.EventHandler(this.buttonBrowseTiles_Click);
            // 
            // labelToneScale
            // 
            this.labelToneScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelToneScale.AutoSize = true;
            this.labelToneScale.Location = new System.Drawing.Point(217, 700);
            this.labelToneScale.Name = "labelToneScale";
            this.labelToneScale.Size = new System.Drawing.Size(84, 17);
            this.labelToneScale.TabIndex = 26;
            this.labelToneScale.Text = "Tone Scale:";
            // 
            // toneScale
            // 
            this.toneScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toneScale.Location = new System.Drawing.Point(300, 700);
            this.toneScale.Maximum = 100;
            this.toneScale.Minimum = 1;
            this.toneScale.Name = "toneScale";
            this.toneScale.Size = new System.Drawing.Size(328, 56);
            this.toneScale.TabIndex = 25;
            this.toolTip1.SetToolTip(this.toneScale, "Provides a meassure of density (darkness) for the stippling process. Adjust to va" +
                    "ry the resulting image.");
            this.toneScale.Value = 50;
            // 
            // browseSourceButton
            // 
            this.browseSourceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseSourceButton.Location = new System.Drawing.Point(889, 17);
            this.browseSourceButton.Name = "browseSourceButton";
            this.browseSourceButton.Size = new System.Drawing.Size(149, 31);
            this.browseSourceButton.TabIndex = 24;
            this.browseSourceButton.Text = "Browse";
            this.browseSourceButton.UseVisualStyleBackColor = true;
            this.browseSourceButton.Click += new System.EventHandler(this.browseSourceButton_Click);
            // 
            // sourceImagePath
            // 
            this.sourceImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceImagePath.Location = new System.Drawing.Point(135, 21);
            this.sourceImagePath.Name = "sourceImagePath";
            this.sourceImagePath.ReadOnly = true;
            this.sourceImagePath.Size = new System.Drawing.Size(748, 22);
            this.sourceImagePath.TabIndex = 23;
            // 
            // sourceImageLabel
            // 
            this.sourceImageLabel.AutoSize = true;
            this.sourceImageLabel.Location = new System.Drawing.Point(30, 17);
            this.sourceImageLabel.Name = "sourceImageLabel";
            this.sourceImageLabel.Size = new System.Drawing.Size(99, 17);
            this.sourceImageLabel.TabIndex = 22;
            this.sourceImageLabel.Text = "Source Image:";
            // 
            // buttonGO
            // 
            this.buttonGO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGO.Location = new System.Drawing.Point(924, 680);
            this.buttonGO.Name = "buttonGO";
            this.buttonGO.Size = new System.Drawing.Size(115, 56);
            this.buttonGO.TabIndex = 18;
            this.buttonGO.Text = "Go!";
            this.buttonGO.UseVisualStyleBackColor = true;
            this.buttonGO.Click += new System.EventHandler(this.buttonGO_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(33, 55);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBoxSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBoxResult);
            this.splitContainer1.Size = new System.Drawing.Size(1005, 614);
            this.splitContainer1.SplitterDistance = 490;
            this.splitContainer1.TabIndex = 21;
            // 
            // pictureBoxSource
            // 
            this.pictureBoxSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxSource.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxSource.Name = "pictureBoxSource";
            this.pictureBoxSource.Size = new System.Drawing.Size(490, 614);
            this.pictureBoxSource.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSource.TabIndex = 4;
            this.pictureBoxSource.TabStop = false;
            // 
            // pictureBoxResult
            // 
            this.pictureBoxResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxResult.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxResult.Name = "pictureBoxResult";
            this.pictureBoxResult.Size = new System.Drawing.Size(511, 614);
            this.pictureBoxResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxResult.TabIndex = 0;
            this.pictureBoxResult.TabStop = false;
            // 
            // saveImgButton
            // 
            this.saveImgButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveImgButton.Location = new System.Drawing.Point(924, 739);
            this.saveImgButton.Name = "saveImgButton";
            this.saveImgButton.Size = new System.Drawing.Size(114, 43);
            this.saveImgButton.TabIndex = 20;
            this.saveImgButton.Text = "Save Result";
            this.saveImgButton.UseVisualStyleBackColor = true;
            this.saveImgButton.Click += new System.EventHandler(this.saveImgButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.radioButtonIncremental);
            this.groupBox1.Controls.Add(this.radioButtonWangTiles);
            this.groupBox1.Location = new System.Drawing.Point(33, 680);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(140, 103);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Method";
            // 
            // radioButtonIncremental
            // 
            this.radioButtonIncremental.AutoSize = true;
            this.radioButtonIncremental.Location = new System.Drawing.Point(25, 35);
            this.radioButtonIncremental.Name = "radioButtonIncremental";
            this.radioButtonIncremental.Size = new System.Drawing.Size(102, 21);
            this.radioButtonIncremental.TabIndex = 4;
            this.radioButtonIncremental.Text = "Incremental";
            this.radioButtonIncremental.UseVisualStyleBackColor = true;
            // 
            // radioButtonWangTiles
            // 
            this.radioButtonWangTiles.AutoSize = true;
            this.radioButtonWangTiles.Checked = true;
            this.radioButtonWangTiles.Location = new System.Drawing.Point(25, 62);
            this.radioButtonWangTiles.Name = "radioButtonWangTiles";
            this.radioButtonWangTiles.Size = new System.Drawing.Size(100, 21);
            this.radioButtonWangTiles.TabIndex = 3;
            this.radioButtonWangTiles.TabStop = true;
            this.radioButtonWangTiles.Text = "Wang Tiles";
            this.toolTip1.SetToolTip(this.radioButtonWangTiles, "Requires a set of precomputed tiles to be provided");
            this.radioButtonWangTiles.UseVisualStyleBackColor = true;
            this.radioButtonWangTiles.CheckedChanged += new System.EventHandler(this.radioButtonWangTiles_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.wtDebugImg);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.wtConsole);
            this.tabPage2.Controls.Add(this.wtProgress);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1069, 801);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Wang Tiles Generation";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.splitContainer2);
            this.groupBox2.Location = new System.Drawing.Point(8, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1053, 122);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 34);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.wtDestFilePath);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            this.splitContainer2.Panel1.Controls.Add(this.wtBrowseDestFile);
            this.splitContainer2.Panel1.Controls.Add(this.wtSamplesPerTile);
            this.splitContainer2.Panel1.Controls.Add(this.wtEdgeColors);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.wtDebugStepOnce);
            this.splitContainer2.Panel2.Controls.Add(this.wtDebugPlayStop);
            this.splitContainer2.Panel2.Controls.Add(this.wtGenerate);
            this.splitContainer2.Panel2.Controls.Add(this.wtDebugSteps);
            this.splitContainer2.Size = new System.Drawing.Size(1053, 82);
            this.splitContainer2.SplitterDistance = 700;
            this.splitContainer2.TabIndex = 14;
            // 
            // wtDestFilePath
            // 
            this.wtDestFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wtDestFilePath.Location = new System.Drawing.Point(142, 3);
            this.wtDestFilePath.Name = "wtDestFilePath";
            this.wtDestFilePath.ReadOnly = true;
            this.wtDestFilePath.Size = new System.Drawing.Size(474, 22);
            this.wtDestFilePath.TabIndex = 20;
            this.wtDestFilePath.Text = "tiles.xml";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 17);
            this.label4.TabIndex = 19;
            this.label4.Text = "Destination File:";
            // 
            // wtBrowseDestFile
            // 
            this.wtBrowseDestFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.wtBrowseDestFile.Location = new System.Drawing.Point(622, 1);
            this.wtBrowseDestFile.Name = "wtBrowseDestFile";
            this.wtBrowseDestFile.Size = new System.Drawing.Size(75, 25);
            this.wtBrowseDestFile.TabIndex = 18;
            this.wtBrowseDestFile.Text = "Browse";
            this.wtBrowseDestFile.UseVisualStyleBackColor = true;
            this.wtBrowseDestFile.Click += new System.EventHandler(this.wtBrowseDestFile_Click);
            // 
            // wtSamplesPerTile
            // 
            this.wtSamplesPerTile.Items.Add("1024");
            this.wtSamplesPerTile.Items.Add("512");
            this.wtSamplesPerTile.Items.Add("256");
            this.wtSamplesPerTile.Items.Add("128");
            this.wtSamplesPerTile.Items.Add("64");
            this.wtSamplesPerTile.Items.Add("32");
            this.wtSamplesPerTile.Location = new System.Drawing.Point(450, 31);
            this.wtSamplesPerTile.Name = "wtSamplesPerTile";
            this.wtSamplesPerTile.Size = new System.Drawing.Size(120, 22);
            this.wtSamplesPerTile.TabIndex = 17;
            this.wtSamplesPerTile.Text = "64";
            this.toolTip1.SetToolTip(this.wtSamplesPerTile, "Number of samples on each source Poisson distribution to be merged into a Wang Ti" +
                    "le. The more samples, the better noise quality.");
            this.wtSamplesPerTile.Wrap = true;
            // 
            // wtEdgeColors
            // 
            this.wtEdgeColors.Location = new System.Drawing.Point(142, 31);
            this.wtEdgeColors.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.wtEdgeColors.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.wtEdgeColors.Name = "wtEdgeColors";
            this.wtEdgeColors.Size = new System.Drawing.Size(120, 22);
            this.wtEdgeColors.TabIndex = 16;
            this.toolTip1.SetToolTip(this.wtEdgeColors, "Number of Wang Tile colors to generate a tile set from.. The higher the number, t" +
                    "he more random the distribution, but the longer to compute.");
            this.wtEdgeColors.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(277, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Samples per Source Tile:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "Edge Colors:";
            // 
            // wtDebugStepOnce
            // 
            this.wtDebugStepOnce.Enabled = false;
            this.wtDebugStepOnce.ImageIndex = 1;
            this.wtDebugStepOnce.ImageList = this.imageList1;
            this.wtDebugStepOnce.Location = new System.Drawing.Point(290, 33);
            this.wtDebugStepOnce.Name = "wtDebugStepOnce";
            this.wtDebugStepOnce.Size = new System.Drawing.Size(49, 48);
            this.wtDebugStepOnce.TabIndex = 17;
            this.toolTip1.SetToolTip(this.wtDebugStepOnce, "Performs one single debugging step");
            this.wtDebugStepOnce.UseVisualStyleBackColor = true;
            this.wtDebugStepOnce.Click += new System.EventHandler(this.wtDebugStepOnce_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "debugPlayPause.png");
            this.imageList1.Images.SetKeyName(1, "debugStep.png");
            // 
            // wtDebugPlayStop
            // 
            this.wtDebugPlayStop.Enabled = false;
            this.wtDebugPlayStop.ImageIndex = 0;
            this.wtDebugPlayStop.ImageList = this.imageList1;
            this.wtDebugPlayStop.Location = new System.Drawing.Point(231, 33);
            this.wtDebugPlayStop.Name = "wtDebugPlayStop";
            this.wtDebugPlayStop.Size = new System.Drawing.Size(53, 48);
            this.wtDebugPlayStop.TabIndex = 16;
            this.toolTip1.SetToolTip(this.wtDebugPlayStop, "Pauses/Resumes process while debugging");
            this.wtDebugPlayStop.UseVisualStyleBackColor = true;
            this.wtDebugPlayStop.Click += new System.EventHandler(this.wtDebugPlayStop_Click);
            // 
            // wtGenerate
            // 
            this.wtGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wtGenerate.Location = new System.Drawing.Point(9, 0);
            this.wtGenerate.Name = "wtGenerate";
            this.wtGenerate.Size = new System.Drawing.Size(189, 79);
            this.wtGenerate.TabIndex = 10;
            this.wtGenerate.Text = "Generate Tiles";
            this.toolTip1.SetToolTip(this.wtGenerate, "Generate wang tiles with the specified parameters and save results to a file");
            this.wtGenerate.UseVisualStyleBackColor = true;
            this.wtGenerate.Click += new System.EventHandler(this.wtGenerate_Click);
            // 
            // wtDebugSteps
            // 
            this.wtDebugSteps.AutoSize = true;
            this.wtDebugSteps.Location = new System.Drawing.Point(231, 1);
            this.wtDebugSteps.Name = "wtDebugSteps";
            this.wtDebugSteps.Size = new System.Drawing.Size(112, 21);
            this.wtDebugSteps.TabIndex = 15;
            this.wtDebugSteps.Text = "Debug Steps";
            this.toolTip1.SetToolTip(this.wtDebugSteps, "Perform verbose computation, displaying all intermediate steps");
            this.wtDebugSteps.UseVisualStyleBackColor = true;
            // 
            // wtDebugImg
            // 
            this.wtDebugImg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wtDebugImg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wtDebugImg.Location = new System.Drawing.Point(8, 269);
            this.wtDebugImg.Name = "wtDebugImg";
            this.wtDebugImg.Size = new System.Drawing.Size(1053, 524);
            this.wtDebugImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.wtDebugImg.TabIndex = 15;
            this.wtDebugImg.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 249);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 17);
            this.label5.TabIndex = 14;
            this.label5.Text = "Debug";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Progress";
            // 
            // wtConsole
            // 
            this.wtConsole.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wtConsole.FormattingEnabled = true;
            this.wtConsole.ItemHeight = 16;
            this.wtConsole.Location = new System.Drawing.Point(8, 157);
            this.wtConsole.Name = "wtConsole";
            this.wtConsole.Size = new System.Drawing.Size(1053, 84);
            this.wtConsole.TabIndex = 4;
            // 
            // wtProgress
            // 
            this.wtProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wtProgress.Location = new System.Drawing.Point(79, 134);
            this.wtProgress.Name = "wtProgress";
            this.wtProgress.Size = new System.Drawing.Size(982, 18);
            this.wtProgress.TabIndex = 3;
            // 
            // StipplingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1077, 830);
            this.Controls.Add(this.tabControl1);
            this.Name = "StipplingForm";
            this.Text = "Stippling - http://joesfer.com";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toneScale)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.wtEdgeColors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wtDebugImg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label labelTilesFile;
        private System.Windows.Forms.Button buttonBrowseTiles;
        private System.Windows.Forms.Label labelToneScale;
        private System.Windows.Forms.TrackBar toneScale;
        private System.Windows.Forms.Button browseSourceButton;
        private System.Windows.Forms.TextBox sourceImagePath;
        private System.Windows.Forms.Label sourceImageLabel;
        private System.Windows.Forms.Button buttonGO;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBoxSource;
        private System.Windows.Forms.PictureBox pictureBoxResult;
        private System.Windows.Forms.Button saveImgButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonIncremental;
        private System.Windows.Forms.RadioButton radioButtonWangTiles;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBoxWangTilesPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox wtConsole;
        private System.Windows.Forms.ProgressBar wtProgress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox wtDebugImg;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox wtDestFilePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button wtBrowseDestFile;
        private System.Windows.Forms.DomainUpDown wtSamplesPerTile;
        private System.Windows.Forms.NumericUpDown wtEdgeColors;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button wtGenerate;
        private System.Windows.Forms.CheckBox wtDebugSteps;
        private System.Windows.Forms.Button wtDebugStepOnce;
        private System.Windows.Forms.Button wtDebugPlayStop;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ImageList imageList1;

    }
}

