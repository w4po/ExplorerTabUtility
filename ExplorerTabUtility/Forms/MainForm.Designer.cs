using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Forms
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label1 = new System.Windows.Forms.Label();
            flpProfiles = new System.Windows.Forms.FlowLayoutPanel();
            btnNewProfile = new MaterialSkin.Controls.MaterialButton();
            btnImport = new MaterialSkin.Controls.MaterialButton();
            btnExport = new MaterialSkin.Controls.MaterialButton();
            btnSave = new MaterialSkin.Controls.MaterialButton();
            cbSaveProfilesOnExit = new MaterialSkin.Controls.MaterialCheckbox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            cbHideTrayIcon = new MaterialSkin.Controls.MaterialCheckbox();
            materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            tpShortcuts = new System.Windows.Forms.TabPage();
            tpPreferences = new System.Windows.Forms.TabPage();
            materialTabControl1.SuspendLayout();
            tpShortcuts.SuspendLayout();
            tpPreferences.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Transparent;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            label1.Location = new System.Drawing.Point(4, 2);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(127, 17);
            label1.TabIndex = 0;
            label1.Text = "Explorer Tab Utility";
            // 
            // flpProfiles
            // 
            flpProfiles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            flpProfiles.AutoScroll = true;
            flpProfiles.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpProfiles.Location = new System.Drawing.Point(1, 46);
            flpProfiles.Name = "flpProfiles";
            flpProfiles.Size = new System.Drawing.Size(838, 280);
            flpProfiles.TabIndex = 2;
            flpProfiles.WrapContents = false;
            flpProfiles.Resize += FlpProfiles_Resize;
            // 
            // btnNewProfile
            // 
            btnNewProfile.AutoSize = false;
            btnNewProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnNewProfile.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNewProfile.Depth = 0;
            btnNewProfile.HighEmphasis = true;
            btnNewProfile.Icon = null;
            btnNewProfile.Location = new System.Drawing.Point(6, 3);
            btnNewProfile.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            btnNewProfile.MouseState = MaterialSkin.MouseState.HOVER;
            btnNewProfile.Name = "btnNewProfile";
            btnNewProfile.NoAccentTextColor = System.Drawing.Color.Empty;
            btnNewProfile.Size = new System.Drawing.Size(76, 36);
            btnNewProfile.TabIndex = 5;
            btnNewProfile.Text = "New";
            toolTip.SetToolTip(btnNewProfile, "New profile.");
            btnNewProfile.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            btnNewProfile.UseAccentColor = true;
            btnNewProfile.UseVisualStyleBackColor = true;
            btnNewProfile.Click += BtnNewProfile_Click;
            // 
            // btnImport
            // 
            btnImport.AutoSize = false;
            btnImport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnImport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnImport.Depth = 0;
            btnImport.HighEmphasis = true;
            btnImport.Icon = null;
            btnImport.Location = new System.Drawing.Point(88, 3);
            btnImport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            btnImport.MouseState = MaterialSkin.MouseState.HOVER;
            btnImport.Name = "btnImport";
            btnImport.NoAccentTextColor = System.Drawing.Color.Empty;
            btnImport.Size = new System.Drawing.Size(76, 36);
            btnImport.TabIndex = 6;
            btnImport.Text = "Import";
            toolTip.SetToolTip(btnImport, "Import profiles.");
            btnImport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            btnImport.UseAccentColor = true;
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += BtnImport_Click;
            // 
            // btnExport
            // 
            btnExport.AutoSize = false;
            btnExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnExport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnExport.Depth = 0;
            btnExport.HighEmphasis = true;
            btnExport.Icon = null;
            btnExport.Location = new System.Drawing.Point(170, 3);
            btnExport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            btnExport.MouseState = MaterialSkin.MouseState.HOVER;
            btnExport.Name = "btnExport";
            btnExport.NoAccentTextColor = System.Drawing.Color.Empty;
            btnExport.Size = new System.Drawing.Size(76, 36);
            btnExport.TabIndex = 7;
            btnExport.Text = "Export";
            toolTip.SetToolTip(btnExport, "Export profiles.");
            btnExport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            btnExport.UseAccentColor = true;
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += BtnExport_Click;
            // 
            // btnSave
            // 
            btnSave.AutoSize = false;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSave.Depth = 0;
            btnSave.HighEmphasis = true;
            btnSave.Icon = null;
            btnSave.Location = new System.Drawing.Point(252, 3);
            btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            btnSave.Name = "btnSave";
            btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            btnSave.Size = new System.Drawing.Size(76, 36);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            toolTip.SetToolTip(btnSave, "Persist profiles for next time you open the app.");
            btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            btnSave.UseAccentColor = true;
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            // 
            // cbSaveProfilesOnExit
            // 
            cbSaveProfilesOnExit.AutoSize = true;
            cbSaveProfilesOnExit.Checked = true;
            cbSaveProfilesOnExit.CheckState = System.Windows.Forms.CheckState.Checked;
            cbSaveProfilesOnExit.Depth = 0;
            cbSaveProfilesOnExit.Location = new System.Drawing.Point(683, 3);
            cbSaveProfilesOnExit.Margin = new System.Windows.Forms.Padding(0);
            cbSaveProfilesOnExit.MouseLocation = new System.Drawing.Point(-1, -1);
            cbSaveProfilesOnExit.MouseState = MaterialSkin.MouseState.HOVER;
            cbSaveProfilesOnExit.Name = "cbSaveProfilesOnExit";
            cbSaveProfilesOnExit.ReadOnly = false;
            cbSaveProfilesOnExit.Ripple = true;
            cbSaveProfilesOnExit.Size = new System.Drawing.Size(121, 37);
            cbSaveProfilesOnExit.TabIndex = 9;
            cbSaveProfilesOnExit.Text = "Save on exit";
            toolTip.SetToolTip(cbSaveProfilesOnExit, "Automatically saves your profiles on exit.\r\nTo persist profiles for next time you open the app.");
            cbSaveProfilesOnExit.UseVisualStyleBackColor = true;
            cbSaveProfilesOnExit.CheckedChanged += CbSaveProfilesOnExit_CheckedChanged;
            // 
            // cbHideTrayIcon
            // 
            cbHideTrayIcon.AutoSize = true;
            cbHideTrayIcon.Depth = 0;
            cbHideTrayIcon.Location = new System.Drawing.Point(3, 3);
            cbHideTrayIcon.Margin = new System.Windows.Forms.Padding(0);
            cbHideTrayIcon.MouseLocation = new System.Drawing.Point(-1, -1);
            cbHideTrayIcon.MouseState = MaterialSkin.MouseState.HOVER;
            cbHideTrayIcon.Name = "cbHideTrayIcon";
            cbHideTrayIcon.ReadOnly = false;
            cbHideTrayIcon.Ripple = true;
            cbHideTrayIcon.Size = new System.Drawing.Size(101, 37);
            cbHideTrayIcon.TabIndex = 10;
            cbHideTrayIcon.Text = "Hide icon";
            toolTip.SetToolTip(cbHideTrayIcon, "Hide tray icon.");
            cbHideTrayIcon.UseVisualStyleBackColor = true;
            cbHideTrayIcon.CheckedChanged += CbHideTrayIcon_CheckedChanged;
            // 
            // materialTabControl1
            // 
            materialTabControl1.Controls.Add(tpShortcuts);
            materialTabControl1.Controls.Add(tpPreferences);
            materialTabControl1.Depth = 0;
            materialTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            materialTabControl1.Location = new System.Drawing.Point(0, 64);
            materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new System.Drawing.Size(849, 335);
            materialTabControl1.TabIndex = 10;
            // 
            // tpShortcuts
            // 
            tpShortcuts.Controls.Add(cbSaveProfilesOnExit);
            tpShortcuts.Controls.Add(btnNewProfile);
            tpShortcuts.Controls.Add(btnSave);
            tpShortcuts.Controls.Add(flpProfiles);
            tpShortcuts.Controls.Add(btnExport);
            tpShortcuts.Controls.Add(btnImport);
            tpShortcuts.Location = new System.Drawing.Point(4, 24);
            tpShortcuts.Name = "tpShortcuts";
            tpShortcuts.Padding = new System.Windows.Forms.Padding(3);
            tpShortcuts.Size = new System.Drawing.Size(841, 307);
            tpShortcuts.TabIndex = 0;
            tpShortcuts.Text = "Shortcuts";
            tpShortcuts.UseVisualStyleBackColor = true;
            // 
            // tpPreferences
            // 
            tpPreferences.Controls.Add(cbHideTrayIcon);
            tpPreferences.Location = new System.Drawing.Point(4, 24);
            tpPreferences.Name = "tpPreferences";
            tpPreferences.Padding = new System.Windows.Forms.Padding(3);
            tpPreferences.Size = new System.Drawing.Size(841, 307);
            tpPreferences.TabIndex = 1;
            tpPreferences.Text = "Preferences";
            tpPreferences.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(852, 402);
            Controls.Add(materialTabControl1);
            Controls.Add(label1);
            DrawerTabControl = materialTabControl1;
            DrawerWidth = 160;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(852, 225);
            Name = "MainForm";
            Padding = new System.Windows.Forms.Padding(0, 64, 3, 3);
            Text = "Settings";
            Deactivate += MainForm_Deactivate;
            materialTabControl1.ResumeLayout(false);
            tpShortcuts.ResumeLayout(false);
            tpShortcuts.PerformLayout();
            tpPreferences.ResumeLayout(false);
            tpPreferences.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flpProfiles;
        private MaterialSkin.Controls.MaterialButton btnNewProfile;
        private MaterialSkin.Controls.MaterialButton btnImport;
        private MaterialSkin.Controls.MaterialButton btnExport;
        private MaterialSkin.Controls.MaterialButton btnSave;
        private MaterialSkin.Controls.MaterialCheckbox cbSaveProfilesOnExit;
        private System.Windows.Forms.ToolTip toolTip;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tpShortcuts;
        private System.Windows.Forms.TabPage tpPreferences;
        private MaterialSkin.Controls.MaterialCheckbox cbHideTrayIcon;
    }
}