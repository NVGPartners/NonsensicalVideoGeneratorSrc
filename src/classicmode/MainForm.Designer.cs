#if !MONOGAME
namespace NonsensicalVideoGenerator
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Video = new System.Windows.Forms.Panel();
            this.Render = new System.Windows.Forms.Button();
            this.PausePlay = new System.Windows.Forms.Button();
            this.SaveAs = new System.Windows.Forms.Button();
            this.End = new System.Windows.Forms.Button();
            this.Start = new System.Windows.Forms.Button();
            this.Settings = new System.Windows.Forms.Panel();
            this.Clips = new System.Windows.Forms.NumericUpDown();
            this.MaxStreamDur = new System.Windows.Forms.NumericUpDown();
            this.MinStreamDur = new System.Windows.Forms.NumericUpDown();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.ClipCountLabel = new System.Windows.Forms.Label();
            this.MaxStreamLabel = new System.Windows.Forms.Label();
            this.MinSteamLabel = new System.Windows.Forms.Label();
            this.TransitionDir = new System.Windows.Forms.TextBox();
            this.InsertTransitions = new System.Windows.Forms.CheckBox();
            this.RenderSettings = new System.Windows.Forms.Panel();
            this.HeightSet = new System.Windows.Forms.NumericUpDown();
            this.WidthSet = new System.Windows.Forms.NumericUpDown();
            this.Intro = new System.Windows.Forms.TextBox();
            this.InsertIntro = new System.Windows.Forms.CheckBox();
            this.HeightLabel = new System.Windows.Forms.Label();
            this.WidthLabel = new System.Windows.Forms.Label();
            this.Outro = new System.Windows.Forms.TextBox();
            this.InsertOutro = new System.Windows.Forms.CheckBox();
            this.RenderSettingsLabel = new System.Windows.Forms.Label();
            this.Materials = new System.Windows.Forms.Panel();
            this.Material = new System.Windows.Forms.RichTextBox();
            this.AddMaterial = new System.Windows.Forms.Button();
            this.ClearMaterial = new System.Windows.Forms.Button();
            this.MaterialLabel = new System.Windows.Forms.Label();
            this.folderBrowserTemp = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialogMagick = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserSounds = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserMusic = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserResources = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialogSource = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogFFmpeg = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogFFProbe = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.folderBrowserVLC = new System.Windows.Forms.FolderBrowserDialog();
            this.Video.SuspendLayout();
            this.Settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Clips)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStreamDur)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinStreamDur)).BeginInit();
            this.RenderSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HeightSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WidthSet)).BeginInit();
            this.Materials.SuspendLayout();
            this.SuspendLayout();
            // 
            // Video
            // 
            this.Video.AccessibleDescription = "Generated Video";
            this.Video.AccessibleName = "Video Container";
            this.Video.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Video.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Video.Controls.Add(this.Render);
            this.Video.Controls.Add(this.PausePlay);
            this.Video.Controls.Add(this.SaveAs);
            this.Video.Controls.Add(this.End);
            this.Video.Controls.Add(this.Start);
            this.Video.Location = new System.Drawing.Point(12, 12);
            this.Video.Name = "Video";
            this.Video.Size = new System.Drawing.Size(320, 288);
            this.Video.TabIndex = 0;
            // 
            // Render
            // 
            this.Render.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Render.BackColor = System.Drawing.SystemColors.Control;
            this.Render.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Render.Location = new System.Drawing.Point(3, 245);
            this.Render.Name = "Render";
            this.Render.Size = new System.Drawing.Size(88, 38);
            this.Render.TabIndex = 1;
            this.Render.Text = "Render";
            this.Render.UseVisualStyleBackColor = false;
            this.Render.Click += new System.EventHandler(this.Render_Click);
            // 
            // PausePlay
            // 
            this.PausePlay.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PausePlay.BackColor = System.Drawing.SystemColors.Control;
            this.PausePlay.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PausePlay.Location = new System.Drawing.Point(141, 245);
            this.PausePlay.Name = "PausePlay";
            this.PausePlay.Size = new System.Drawing.Size(38, 38);
            this.PausePlay.TabIndex = 3;
            this.PausePlay.Text = "| |";
            this.PausePlay.UseVisualStyleBackColor = false;
            this.PausePlay.Click += new System.EventHandler(this.PausePlay_Click);
            // 
            // SaveAs
            // 
            this.SaveAs.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SaveAs.BackColor = System.Drawing.SystemColors.Control;
            this.SaveAs.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveAs.Location = new System.Drawing.Point(227, 245);
            this.SaveAs.Name = "SaveAs";
            this.SaveAs.Size = new System.Drawing.Size(88, 38);
            this.SaveAs.TabIndex = 5;
            this.SaveAs.Text = "Save as...";
            this.SaveAs.UseVisualStyleBackColor = false;
            this.SaveAs.Click += new System.EventHandler(this.SaveAs_Click);
            // 
            // End
            // 
            this.End.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.End.BackColor = System.Drawing.SystemColors.Control;
            this.End.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.End.Location = new System.Drawing.Point(183, 245);
            this.End.Name = "End";
            this.End.Size = new System.Drawing.Size(38, 38);
            this.End.TabIndex = 4;
            this.End.Text = ">";
            this.End.UseVisualStyleBackColor = false;
            this.End.Click += new System.EventHandler(this.End_Click);
            // 
            // Start
            // 
            this.Start.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Start.BackColor = System.Drawing.SystemColors.Control;
            this.Start.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Start.Location = new System.Drawing.Point(97, 245);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(38, 38);
            this.Start.TabIndex = 2;
            this.Start.Text = "|<";
            this.Start.UseVisualStyleBackColor = false;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Settings
            // 
            this.Settings.AccessibleDescription = "Generated Video";
            this.Settings.AccessibleName = "Video Container";
            this.Settings.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Settings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Settings.Controls.Add(this.Clips);
            this.Settings.Controls.Add(this.MaxStreamDur);
            this.Settings.Controls.Add(this.MinStreamDur);
            this.Settings.Controls.Add(this.progressBar1);
            this.Settings.Controls.Add(this.ClipCountLabel);
            this.Settings.Controls.Add(this.MaxStreamLabel);
            this.Settings.Controls.Add(this.MinSteamLabel);
            this.Settings.Controls.Add(this.TransitionDir);
            this.Settings.Controls.Add(this.InsertTransitions);
            this.Settings.Location = new System.Drawing.Point(13, 306);
            this.Settings.Name = "Settings";
            this.Settings.Size = new System.Drawing.Size(320, 145);
            this.Settings.TabIndex = 7;
            // 
            // Clips
            // 
            this.Clips.BackColor = System.Drawing.SystemColors.Control;
            this.Clips.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Clips.Location = new System.Drawing.Point(226, 80);
            this.Clips.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.Clips.Name = "Clips";
            this.Clips.Size = new System.Drawing.Size(88, 20);
            this.Clips.TabIndex = 10;
            this.Clips.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // MaxStreamDur
            // 
            this.MaxStreamDur.BackColor = System.Drawing.SystemColors.Control;
            this.MaxStreamDur.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MaxStreamDur.DecimalPlaces = 1;
            this.MaxStreamDur.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.MaxStreamDur.Location = new System.Drawing.Point(226, 54);
            this.MaxStreamDur.Name = "MaxStreamDur";
            this.MaxStreamDur.Size = new System.Drawing.Size(88, 20);
            this.MaxStreamDur.TabIndex = 9;
            this.MaxStreamDur.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // MinStreamDur
            // 
            this.MinStreamDur.BackColor = System.Drawing.SystemColors.Control;
            this.MinStreamDur.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MinStreamDur.DecimalPlaces = 1;
            this.MinStreamDur.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.MinStreamDur.Location = new System.Drawing.Point(227, 28);
            this.MinStreamDur.Name = "MinStreamDur";
            this.MinStreamDur.Size = new System.Drawing.Size(88, 20);
            this.MinStreamDur.TabIndex = 8;
            this.MinStreamDur.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(6, 108);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(309, 32);
            this.progressBar1.TabIndex = 20;
            // 
            // ClipCountLabel
            // 
            this.ClipCountLabel.AutoSize = true;
            this.ClipCountLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClipCountLabel.Location = new System.Drawing.Point(3, 82);
            this.ClipCountLabel.Name = "ClipCountLabel";
            this.ClipCountLabel.Size = new System.Drawing.Size(62, 13);
            this.ClipCountLabel.TabIndex = 19;
            this.ClipCountLabel.Text = "Clip Count";
            // 
            // MaxStreamLabel
            // 
            this.MaxStreamLabel.AutoSize = true;
            this.MaxStreamLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxStreamLabel.Location = new System.Drawing.Point(3, 56);
            this.MaxStreamLabel.Name = "MaxStreamLabel";
            this.MaxStreamLabel.Size = new System.Drawing.Size(179, 13);
            this.MaxStreamLabel.TabIndex = 17;
            this.MaxStreamLabel.Text = "Max Stream Duration (in seconds)";
            // 
            // MinSteamLabel
            // 
            this.MinSteamLabel.AutoSize = true;
            this.MinSteamLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinSteamLabel.Location = new System.Drawing.Point(3, 30);
            this.MinSteamLabel.Name = "MinSteamLabel";
            this.MinSteamLabel.Size = new System.Drawing.Size(178, 13);
            this.MinSteamLabel.TabIndex = 15;
            this.MinSteamLabel.Text = "Min Stream Duration (in seconds)";
            // 
            // TransitionDir
            // 
            this.TransitionDir.BackColor = System.Drawing.SystemColors.Control;
            this.TransitionDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TransitionDir.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TransitionDir.Location = new System.Drawing.Point(227, 2);
            this.TransitionDir.Name = "TransitionDir";
            this.TransitionDir.Size = new System.Drawing.Size(88, 22);
            this.TransitionDir.TabIndex = 7;
            this.TransitionDir.Text = "sources/";
            // 
            // InsertTransitions
            // 
            this.InsertTransitions.AutoSize = true;
            this.InsertTransitions.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.InsertTransitions.Checked = true;
            this.InsertTransitions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InsertTransitions.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InsertTransitions.Location = new System.Drawing.Point(3, 3);
            this.InsertTransitions.Name = "InsertTransitions";
            this.InsertTransitions.Size = new System.Drawing.Size(186, 17);
            this.InsertTransitions.TabIndex = 6;
            this.InsertTransitions.Text = "Insert Transition Clips (Sources)";
            this.InsertTransitions.UseVisualStyleBackColor = true;
            // 
            // RenderSettings
            // 
            this.RenderSettings.BackColor = System.Drawing.SystemColors.ControlDark;
            this.RenderSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RenderSettings.Controls.Add(this.HeightSet);
            this.RenderSettings.Controls.Add(this.WidthSet);
            this.RenderSettings.Controls.Add(this.Intro);
            this.RenderSettings.Controls.Add(this.InsertIntro);
            this.RenderSettings.Controls.Add(this.HeightLabel);
            this.RenderSettings.Controls.Add(this.WidthLabel);
            this.RenderSettings.Controls.Add(this.Outro);
            this.RenderSettings.Controls.Add(this.InsertOutro);
            this.RenderSettings.Controls.Add(this.RenderSettingsLabel);
            this.RenderSettings.Location = new System.Drawing.Point(339, 305);
            this.RenderSettings.Name = "RenderSettings";
            this.RenderSettings.Size = new System.Drawing.Size(210, 145);
            this.RenderSettings.TabIndex = 8;
            // 
            // HeightSet
            // 
            this.HeightSet.BackColor = System.Drawing.SystemColors.Control;
            this.HeightSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HeightSet.Location = new System.Drawing.Point(117, 116);
            this.HeightSet.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.HeightSet.Name = "HeightSet";
            this.HeightSet.Size = new System.Drawing.Size(88, 20);
            this.HeightSet.TabIndex = 27;
            this.HeightSet.Value = new decimal(new int[] {
            480,
            0,
            0,
            0});
            // 
            // WidthSet
            // 
            this.WidthSet.BackColor = System.Drawing.SystemColors.Control;
            this.WidthSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WidthSet.Location = new System.Drawing.Point(117, 87);
            this.WidthSet.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.WidthSet.Name = "WidthSet";
            this.WidthSet.Size = new System.Drawing.Size(88, 20);
            this.WidthSet.TabIndex = 26;
            this.WidthSet.Value = new decimal(new int[] {
            640,
            0,
            0,
            0});
            // 
            // Intro
            // 
            this.Intro.BackColor = System.Drawing.SystemColors.Control;
            this.Intro.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Intro.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Intro.Location = new System.Drawing.Point(117, 31);
            this.Intro.Name = "Intro";
            this.Intro.Size = new System.Drawing.Size(88, 22);
            this.Intro.TabIndex = 23;
            this.Intro.Text = "intro.mp4";
            // 
            // InsertIntro
            // 
            this.InsertIntro.AutoSize = true;
            this.InsertIntro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.InsertIntro.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InsertIntro.Location = new System.Drawing.Point(3, 33);
            this.InsertIntro.Name = "InsertIntro";
            this.InsertIntro.Size = new System.Drawing.Size(83, 17);
            this.InsertIntro.TabIndex = 22;
            this.InsertIntro.Text = "Insert Intro";
            this.InsertIntro.UseVisualStyleBackColor = true;
            // 
            // HeightLabel
            // 
            this.HeightLabel.AutoSize = true;
            this.HeightLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HeightLabel.Location = new System.Drawing.Point(3, 118);
            this.HeightLabel.Name = "HeightLabel";
            this.HeightLabel.Size = new System.Drawing.Size(82, 13);
            this.HeightLabel.TabIndex = 23;
            this.HeightLabel.Text = "Render Height";
            // 
            // WidthLabel
            // 
            this.WidthLabel.AutoSize = true;
            this.WidthLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WidthLabel.Location = new System.Drawing.Point(3, 89);
            this.WidthLabel.Name = "WidthLabel";
            this.WidthLabel.Size = new System.Drawing.Size(79, 13);
            this.WidthLabel.TabIndex = 21;
            this.WidthLabel.Text = "Render Width";
            // 
            // Outro
            // 
            this.Outro.BackColor = System.Drawing.SystemColors.Control;
            this.Outro.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Outro.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Outro.Location = new System.Drawing.Point(117, 59);
            this.Outro.Name = "Outro";
            this.Outro.Size = new System.Drawing.Size(88, 22);
            this.Outro.TabIndex = 25;
            this.Outro.Text = "outro.mp4";
            // 
            // InsertOutro
            // 
            this.InsertOutro.AutoSize = true;
            this.InsertOutro.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.InsertOutro.Checked = true;
            this.InsertOutro.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InsertOutro.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InsertOutro.Location = new System.Drawing.Point(3, 60);
            this.InsertOutro.Name = "InsertOutro";
            this.InsertOutro.Size = new System.Drawing.Size(89, 17);
            this.InsertOutro.TabIndex = 24;
            this.InsertOutro.Text = "Insert Outro";
            this.InsertOutro.UseVisualStyleBackColor = true;
            // 
            // RenderSettingsLabel
            // 
            this.RenderSettingsLabel.AutoSize = true;
            this.RenderSettingsLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RenderSettingsLabel.Location = new System.Drawing.Point(31, 1);
            this.RenderSettingsLabel.Name = "RenderSettingsLabel";
            this.RenderSettingsLabel.Size = new System.Drawing.Size(143, 25);
            this.RenderSettingsLabel.TabIndex = 12;
            this.RenderSettingsLabel.Text = "Render Settings";
            this.RenderSettingsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Materials
            // 
            this.Materials.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Materials.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Materials.Controls.Add(this.Material);
            this.Materials.Controls.Add(this.AddMaterial);
            this.Materials.Controls.Add(this.ClearMaterial);
            this.Materials.Controls.Add(this.MaterialLabel);
            this.Materials.Location = new System.Drawing.Point(339, 12);
            this.Materials.Name = "Materials";
            this.Materials.Size = new System.Drawing.Size(210, 288);
            this.Materials.TabIndex = 15;
            // 
            // Material
            // 
            this.Material.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Material.Location = new System.Drawing.Point(4, 26);
            this.Material.Name = "Material";
            this.Material.ReadOnly = true;
            this.Material.Size = new System.Drawing.Size(200, 220);
            this.Material.TabIndex = 28;
            this.Material.Text = "";
            // 
            // AddMaterial
            // 
            this.AddMaterial.Location = new System.Drawing.Point(3, 252);
            this.AddMaterial.Name = "AddMaterial";
            this.AddMaterial.Size = new System.Drawing.Size(100, 32);
            this.AddMaterial.TabIndex = 29;
            this.AddMaterial.Text = "Add (*.mp4)";
            this.AddMaterial.UseVisualStyleBackColor = true;
            this.AddMaterial.Click += new System.EventHandler(this.AddMaterial_Click);
            // 
            // ClearMaterial
            // 
            this.ClearMaterial.Location = new System.Drawing.Point(109, 252);
            this.ClearMaterial.Name = "ClearMaterial";
            this.ClearMaterial.Size = new System.Drawing.Size(95, 32);
            this.ClearMaterial.TabIndex = 30;
            this.ClearMaterial.Text = "Clear";
            this.ClearMaterial.UseVisualStyleBackColor = true;
            this.ClearMaterial.Click += new System.EventHandler(this.ClearMaterial_Click);
            // 
            // MaterialLabel
            // 
            this.MaterialLabel.AutoSize = true;
            this.MaterialLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaterialLabel.Location = new System.Drawing.Point(61, 0);
            this.MaterialLabel.Name = "MaterialLabel";
            this.MaterialLabel.Size = new System.Drawing.Size(82, 25);
            this.MaterialLabel.TabIndex = 12;
            this.MaterialLabel.Text = "Material";
            this.MaterialLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // YTPPlusPlus
            // 
            this.AccessibleDescription = "Youtube Poop Generator";
            this.AccessibleName = "Y T P Plus Plus";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(562, 466);
            this.Controls.Add(this.Materials);
            this.Controls.Add(this.RenderSettings);
            this.Controls.Add(this.Settings);
            this.Controls.Add(this.Video);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(578, 526);
            this.MinimumSize = new System.Drawing.Size(578, 526);
            this.Name = "YTPPlusPlus";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YTP++ [v2.2]";
            this.Load += new System.EventHandler(this.YTPPlusPlus_Load);
            this.Video.ResumeLayout(false);
            this.Settings.ResumeLayout(false);
            this.Settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Clips)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStreamDur)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinStreamDur)).EndInit();
            this.RenderSettings.ResumeLayout(false);
            this.RenderSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HeightSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WidthSet)).EndInit();
            this.Materials.ResumeLayout(false);
            this.Materials.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel Video;
        private System.Windows.Forms.Button PausePlay;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button End;
        private System.Windows.Forms.Button Render;
        private System.Windows.Forms.Button SaveAs;
        private System.Windows.Forms.Panel Settings;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label ClipCountLabel;
        private System.Windows.Forms.Label MaxStreamLabel;
        private System.Windows.Forms.Label MinSteamLabel;
        private System.Windows.Forms.TextBox TransitionDir;
        private System.Windows.Forms.CheckBox InsertTransitions;
        private System.Windows.Forms.Panel RenderSettings;
        private System.Windows.Forms.Label RenderSettingsLabel;
        private System.Windows.Forms.Label HeightLabel;
        private System.Windows.Forms.Label WidthLabel;
        private System.Windows.Forms.TextBox Outro;
        private System.Windows.Forms.CheckBox InsertOutro;
        private System.Windows.Forms.Panel Materials;
        private System.Windows.Forms.RichTextBox Material;
        private System.Windows.Forms.Button AddMaterial;
        private System.Windows.Forms.Button ClearMaterial;
        private System.Windows.Forms.Label MaterialLabel;
        private System.Windows.Forms.TextBox Intro;
        private System.Windows.Forms.CheckBox InsertIntro;
        private System.Windows.Forms.NumericUpDown MinStreamDur;
        private System.Windows.Forms.NumericUpDown Clips;
        private System.Windows.Forms.NumericUpDown MaxStreamDur;
        private System.Windows.Forms.NumericUpDown HeightSet;
        private System.Windows.Forms.NumericUpDown WidthSet;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserTemp;
        private System.Windows.Forms.OpenFileDialog openFileDialogMagick;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserSounds;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserMusic;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserResources;
        private System.Windows.Forms.OpenFileDialog openFileDialogSource;
        private System.Windows.Forms.OpenFileDialog openFileDialogFFmpeg;
        private System.Windows.Forms.OpenFileDialog openFileDialogFFProbe;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserVLC;
    }
}
#endif
