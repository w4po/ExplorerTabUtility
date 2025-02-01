namespace ExplorerTabUtility.Forms;

partial class HotKeyProfileControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlMain = new System.Windows.Forms.Panel();
        btnCollapse = new MaterialSkin.Controls.MaterialButton();
        splitter5 = new System.Windows.Forms.Splitter();
        cbAction = new MaterialSkin.Controls.MaterialComboBox();
        splitter4 = new System.Windows.Forms.Splitter();
        cbScope = new MaterialSkin.Controls.MaterialComboBox();
        splitter3 = new System.Windows.Forms.Splitter();
        txtHotKeys = new MaterialSkin.Controls.MaterialTextBox();
        splitter2 = new System.Windows.Forms.Splitter();
        txtName = new MaterialSkin.Controls.MaterialTextBox();
        splitter1 = new System.Windows.Forms.Splitter();
        cbEnabled = new MaterialSkin.Controls.MaterialSwitch();
        pnlMore = new System.Windows.Forms.Panel();
        btnDelete = new MaterialSkin.Controls.MaterialButton();
        splitter8 = new System.Windows.Forms.Splitter();
        cbHandled = new MaterialSkin.Controls.MaterialSwitch();
        splitter7 = new System.Windows.Forms.Splitter();
        sDelay = new MaterialSkin.Controls.MaterialSlider();
        splitter6 = new System.Windows.Forms.Splitter();
        txtPath = new MaterialSkin.Controls.MaterialTextBox();
        toolTip = new System.Windows.Forms.ToolTip(components);
        pnlMain.SuspendLayout();
        pnlMore.SuspendLayout();
        SuspendLayout();
        // 
        // pnlMain
        // 
        pnlMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        pnlMain.Controls.Add(btnCollapse);
        pnlMain.Controls.Add(splitter5);
        pnlMain.Controls.Add(cbAction);
        pnlMain.Controls.Add(splitter4);
        pnlMain.Controls.Add(cbScope);
        pnlMain.Controls.Add(splitter3);
        pnlMain.Controls.Add(txtHotKeys);
        pnlMain.Controls.Add(splitter2);
        pnlMain.Controls.Add(txtName);
        pnlMain.Controls.Add(splitter1);
        pnlMain.Controls.Add(cbEnabled);
        pnlMain.Location = new System.Drawing.Point(0, 0);
        pnlMain.Margin = new System.Windows.Forms.Padding(0);
        pnlMain.Name = "pnlMain";
        pnlMain.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
        pnlMain.Size = new System.Drawing.Size(725, 37);
        pnlMain.TabIndex = 0;
        // 
        // btnCollapse
        // 
        btnCollapse.AutoSize = false;
        btnCollapse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnCollapse.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
        btnCollapse.Depth = 0;
        btnCollapse.Dock = System.Windows.Forms.DockStyle.Left;
        btnCollapse.HighEmphasis = true;
        btnCollapse.Icon = null;
        btnCollapse.Location = new System.Drawing.Point(687, 0);
        btnCollapse.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
        btnCollapse.MouseState = MaterialSkin.MouseState.HOVER;
        btnCollapse.Name = "btnCollapse";
        btnCollapse.NoAccentTextColor = System.Drawing.Color.Empty;
        btnCollapse.Size = new System.Drawing.Size(36, 37);
        btnCollapse.TabIndex = 4;
        btnCollapse.Text = "ᐯ";
        toolTip.SetToolTip(btnCollapse, "Show more.");
        btnCollapse.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
        btnCollapse.UseAccentColor = true;
        btnCollapse.UseVisualStyleBackColor = true;
        btnCollapse.Click += BtnCollapse_Click;
        // 
        // splitter5
        // 
        splitter5.Location = new System.Drawing.Point(682, 0);
        splitter5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter5.MinSize = 120;
        splitter5.Name = "splitter5";
        splitter5.Size = new System.Drawing.Size(5, 37);
        splitter5.TabIndex = 12;
        splitter5.TabStop = false;
        // 
        // cbAction
        // 
        cbAction.AutoResize = false;
        cbAction.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
        cbAction.Depth = 0;
        cbAction.Dock = System.Windows.Forms.DockStyle.Left;
        cbAction.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
        cbAction.DropDownHeight = 118;
        cbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cbAction.DropDownWidth = 175;
        cbAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
        cbAction.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
        cbAction.FormattingEnabled = true;
        cbAction.Hint = "Action";
        cbAction.IntegralHeight = false;
        cbAction.ItemHeight = 29;
        cbAction.Items.AddRange(new object[] { "Open", "ReopenClosed", "Duplicate", "Write" });
        cbAction.Location = new System.Drawing.Point(529, 0);
        cbAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        cbAction.MaxDropDownItems = 4;
        cbAction.MouseState = MaterialSkin.MouseState.OUT;
        cbAction.Name = "cbAction";
        cbAction.Size = new System.Drawing.Size(153, 35);
        cbAction.StartIndex = 0;
        cbAction.TabIndex = 3;
        toolTip.SetToolTip(cbAction, "What to do if the HotKeys got pressed.");
        cbAction.UseTallSize = false;
        cbAction.SelectedIndexChanged += CbAction_SelectedIndexChanged;
        // 
        // splitter4
        // 
        splitter4.Location = new System.Drawing.Point(525, 0);
        splitter4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter4.MinSize = 120;
        splitter4.Name = "splitter4";
        splitter4.Size = new System.Drawing.Size(4, 37);
        splitter4.TabIndex = 13;
        splitter4.TabStop = false;
        // 
        // cbScope
        // 
        cbScope.AutoResize = false;
        cbScope.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
        cbScope.Depth = 0;
        cbScope.Dock = System.Windows.Forms.DockStyle.Left;
        cbScope.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
        cbScope.DropDownHeight = 118;
        cbScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cbScope.DropDownWidth = 121;
        cbScope.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
        cbScope.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
        cbScope.FormattingEnabled = true;
        cbScope.Hint = "Scope";
        cbScope.IntegralHeight = false;
        cbScope.ItemHeight = 29;
        cbScope.Items.AddRange(new object[] { "Global", "FileExplorer" });
        cbScope.Location = new System.Drawing.Point(390, 0);
        cbScope.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        cbScope.MaxDropDownItems = 4;
        cbScope.MouseState = MaterialSkin.MouseState.OUT;
        cbScope.Name = "cbScope";
        cbScope.Size = new System.Drawing.Size(135, 35);
        cbScope.StartIndex = 0;
        cbScope.TabIndex = 2;
        toolTip.SetToolTip(cbScope, "Scope of the hotkeys, whether it's Global or only if the FileExplorer is focused.");
        cbScope.UseTallSize = false;
        cbScope.SelectedIndexChanged += CbScope_SelectedIndexChanged;
        // 
        // splitter3
        // 
        splitter3.Location = new System.Drawing.Point(386, 0);
        splitter3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter3.MinSize = 120;
        splitter3.Name = "splitter3";
        splitter3.Size = new System.Drawing.Size(4, 37);
        splitter3.TabIndex = 14;
        splitter3.TabStop = false;
        // 
        // txtHotKeys
        // 
        txtHotKeys.AnimateReadOnly = true;
        txtHotKeys.BorderStyle = System.Windows.Forms.BorderStyle.None;
        txtHotKeys.Depth = 0;
        txtHotKeys.DetectUrls = false;
        txtHotKeys.Dock = System.Windows.Forms.DockStyle.Left;
        txtHotKeys.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        txtHotKeys.Hint = "HotKeys";
        txtHotKeys.LeadingIcon = null;
        txtHotKeys.LeaveOnEnterKey = true;
        txtHotKeys.Location = new System.Drawing.Point(231, 0);
        txtHotKeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        txtHotKeys.MaxLength = 50;
        txtHotKeys.MouseState = MaterialSkin.MouseState.OUT;
        txtHotKeys.Multiline = false;
        txtHotKeys.Name = "txtHotKeys";
        txtHotKeys.ReadOnly = true;
        txtHotKeys.ShortcutsEnabled = false;
        txtHotKeys.Size = new System.Drawing.Size(155, 36);
        txtHotKeys.TabIndex = 1;
        txtHotKeys.Text = "";
        toolTip.SetToolTip(txtHotKeys, "HotKeys to listen for.");
        txtHotKeys.TrailingIcon = null;
        txtHotKeys.UseTallSize = false;
        txtHotKeys.Enter += TxtHotKeys_Enter;
        txtHotKeys.KeyDown += TxtHotKeys_KeyDown;
        txtHotKeys.Leave += TxtHotKeys_Leave;
        // 
        // splitter2
        // 
        splitter2.Location = new System.Drawing.Point(227, 0);
        splitter2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter2.MinSize = 120;
        splitter2.Name = "splitter2";
        splitter2.Size = new System.Drawing.Size(4, 37);
        splitter2.TabIndex = 15;
        splitter2.TabStop = false;
        // 
        // txtName
        // 
        txtName.AnimateReadOnly = false;
        txtName.BorderStyle = System.Windows.Forms.BorderStyle.None;
        txtName.Depth = 0;
        txtName.Dock = System.Windows.Forms.DockStyle.Left;
        txtName.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        txtName.Hint = "Name";
        txtName.LeadingIcon = null;
        txtName.LeaveOnEnterKey = true;
        txtName.Location = new System.Drawing.Point(72, 0);
        txtName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        txtName.MaxLength = 50;
        txtName.MouseState = MaterialSkin.MouseState.OUT;
        txtName.Multiline = false;
        txtName.Name = "txtName";
        txtName.Size = new System.Drawing.Size(155, 36);
        txtName.TabIndex = 0;
        txtName.Text = "";
        toolTip.SetToolTip(txtName, "Name of the profile.");
        txtName.TrailingIcon = null;
        txtName.UseTallSize = false;
        txtName.TextChanged += TxtName_TextChanged;
        // 
        // splitter1
        // 
        splitter1.Location = new System.Drawing.Point(68, 0);
        splitter1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter1.MinSize = 50;
        splitter1.Name = "splitter1";
        splitter1.Size = new System.Drawing.Size(4, 37);
        splitter1.TabIndex = 16;
        splitter1.TabStop = false;
        // 
        // cbEnabled
        // 
        cbEnabled.AutoSize = true;
        cbEnabled.Checked = true;
        cbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
        cbEnabled.Depth = 0;
        cbEnabled.Dock = System.Windows.Forms.DockStyle.Left;
        cbEnabled.Location = new System.Drawing.Point(10, 0);
        cbEnabled.Margin = new System.Windows.Forms.Padding(0);
        cbEnabled.MouseLocation = new System.Drawing.Point(-1, -1);
        cbEnabled.MouseState = MaterialSkin.MouseState.HOVER;
        cbEnabled.Name = "cbEnabled";
        cbEnabled.Ripple = true;
        cbEnabled.Size = new System.Drawing.Size(58, 37);
        cbEnabled.TabIndex = 8;
        toolTip.SetToolTip(cbEnabled, "Enable the profile.");
        cbEnabled.UseVisualStyleBackColor = true;
        cbEnabled.CheckedChanged += CbEnabled_CheckedChanged;
        // 
        // pnlMore
        // 
        pnlMore.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        pnlMore.Controls.Add(btnDelete);
        pnlMore.Controls.Add(splitter8);
        pnlMore.Controls.Add(cbHandled);
        pnlMore.Controls.Add(splitter7);
        pnlMore.Controls.Add(sDelay);
        pnlMore.Controls.Add(splitter6);
        pnlMore.Controls.Add(txtPath);
        pnlMore.Location = new System.Drawing.Point(0, 45);
        pnlMore.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        pnlMore.Name = "pnlMore";
        pnlMore.Padding = new System.Windows.Forms.Padding(72, 0, 0, 0);
        pnlMore.Size = new System.Drawing.Size(725, 37);
        pnlMore.TabIndex = 8;
        // 
        // btnDelete
        // 
        btnDelete.AutoSize = false;
        btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnDelete.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
        btnDelete.Depth = 0;
        btnDelete.Dock = System.Windows.Forms.DockStyle.Left;
        btnDelete.HighEmphasis = true;
        btnDelete.Icon = null;
        btnDelete.Location = new System.Drawing.Point(650, 0);
        btnDelete.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
        btnDelete.MouseState = MaterialSkin.MouseState.HOVER;
        btnDelete.Name = "btnDelete";
        btnDelete.NoAccentTextColor = System.Drawing.Color.Empty;
        btnDelete.Size = new System.Drawing.Size(63, 37);
        btnDelete.TabIndex = 21;
        btnDelete.Text = "Delete";
        toolTip.SetToolTip(btnDelete, "Delete current profile.");
        btnDelete.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
        btnDelete.UseAccentColor = true;
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += BtnDelete_Click;
        // 
        // splitter8
        // 
        splitter8.Location = new System.Drawing.Point(646, 0);
        splitter8.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter8.MinSize = 80;
        splitter8.Name = "splitter8";
        splitter8.Size = new System.Drawing.Size(4, 37);
        splitter8.TabIndex = 22;
        splitter8.TabStop = false;
        // 
        // cbHandled
        // 
        cbHandled.AutoSize = true;
        cbHandled.Checked = true;
        cbHandled.CheckState = System.Windows.Forms.CheckState.Checked;
        cbHandled.Depth = 0;
        cbHandled.Dock = System.Windows.Forms.DockStyle.Left;
        cbHandled.Location = new System.Drawing.Point(529, 0);
        cbHandled.Margin = new System.Windows.Forms.Padding(0);
        cbHandled.MouseLocation = new System.Drawing.Point(-1, -1);
        cbHandled.MouseState = MaterialSkin.MouseState.HOVER;
        cbHandled.Name = "cbHandled";
        cbHandled.Ripple = true;
        cbHandled.Size = new System.Drawing.Size(117, 37);
        cbHandled.TabIndex = 7;
        cbHandled.Text = "Handled";
        toolTip.SetToolTip(cbHandled, "Prevent further processing of the hotkeys in other applications.");
        cbHandled.UseVisualStyleBackColor = true;
        cbHandled.CheckedChanged += CbHandled_CheckedChanged;
        // 
        // splitter7
        // 
        splitter7.Location = new System.Drawing.Point(525, 0);
        splitter7.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter7.MinSize = 100;
        splitter7.Name = "splitter7";
        splitter7.Size = new System.Drawing.Size(4, 37);
        splitter7.TabIndex = 19;
        splitter7.TabStop = false;
        // 
        // sDelay
        // 
        sDelay.Depth = 0;
        sDelay.Dock = System.Windows.Forms.DockStyle.Left;
        sDelay.Font = new System.Drawing.Font("Roboto", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        sDelay.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
        sDelay.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
        sDelay.Location = new System.Drawing.Point(390, 0);
        sDelay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        sDelay.MouseState = MaterialSkin.MouseState.HOVER;
        sDelay.Name = "sDelay";
        sDelay.RangeMax = 10000;
        sDelay.ShowText = false;
        sDelay.Size = new System.Drawing.Size(135, 40);
        sDelay.TabIndex = 6;
        sDelay.Text = "Delay";
        toolTip.SetToolTip(sDelay, "Delay before doing the action.");
        sDelay.Value = 0;
        sDelay.ValueSuffix = " MS";
        sDelay.onValueChanged += SDelay_ValueChanged;
        // 
        // splitter6
        // 
        splitter6.Location = new System.Drawing.Point(386, 0);
        splitter6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        splitter6.MinExtra = 450;
        splitter6.MinSize = 155;
        splitter6.Name = "splitter6";
        splitter6.Size = new System.Drawing.Size(4, 37);
        splitter6.TabIndex = 20;
        splitter6.TabStop = false;
        // 
        // txtPath
        // 
        txtPath.AnimateReadOnly = false;
        txtPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
        txtPath.Depth = 0;
        txtPath.Dock = System.Windows.Forms.DockStyle.Left;
        txtPath.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        txtPath.Hint = "Path";
        txtPath.LeadingIcon = null;
        txtPath.LeaveOnEnterKey = true;
        txtPath.Location = new System.Drawing.Point(72, 0);
        txtPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        txtPath.MaxLength = 1000;
        txtPath.MouseState = MaterialSkin.MouseState.OUT;
        txtPath.Multiline = false;
        txtPath.Name = "txtPath";
        txtPath.Size = new System.Drawing.Size(314, 36);
        txtPath.TabIndex = 5;
        txtPath.Text = "";
        toolTip.SetToolTip(txtPath, "Folder path to open.");
        txtPath.TrailingIcon = null;
        txtPath.UseTallSize = false;
        txtPath.TextChanged += TxtPath_TextChanged;
        // 
        // HotKeyProfileControl
        // 
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        Controls.Add(pnlMore);
        Controls.Add(pnlMain);
        Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
        Name = "HotKeyProfileControl";
        Size = new System.Drawing.Size(725, 37);
        pnlMain.ResumeLayout(false);
        pnlMain.PerformLayout();
        pnlMore.ResumeLayout(false);
        pnlMore.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel pnlMain;
    private MaterialSkin.Controls.MaterialSwitch cbEnabled;
    private System.Windows.Forms.Splitter splitter1;
    private MaterialSkin.Controls.MaterialTextBox txtName;
    private MaterialSkin.Controls.MaterialTextBox txtHotKeys;
    private System.Windows.Forms.Splitter splitter2;
    private System.Windows.Forms.Splitter splitter3;
    private MaterialSkin.Controls.MaterialComboBox cbScope;
    private MaterialSkin.Controls.MaterialComboBox cbAction;
    private System.Windows.Forms.Splitter splitter4;
    private System.Windows.Forms.Splitter splitter5;
    private MaterialSkin.Controls.MaterialButton btnCollapse;
    private System.Windows.Forms.Panel pnlMore;
    private System.Windows.Forms.Splitter splitter6;
    private MaterialSkin.Controls.MaterialTextBox txtPath;
    private MaterialSkin.Controls.MaterialSlider sDelay;
    private MaterialSkin.Controls.MaterialSwitch cbHandled;
    private System.Windows.Forms.Splitter splitter7;
    private System.Windows.Forms.ToolTip toolTip;
    private MaterialSkin.Controls.MaterialButton btnDelete;
    private System.Windows.Forms.Splitter splitter8;
}