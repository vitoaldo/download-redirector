using download_redirector.Models;
using download_redirector.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace download_redirector.UI;

public class SettingsForm : Form
{
    private readonly string _settingsPath = "appsettings.json";
    private RichTextBox _jsonEditor = null!;
    private TabControl _tabControl = null!;
    private FlowLayoutPanel _visualPanel = null!;
    private NumericUpDown _numInterval = null!;
    private CheckBox _chkAutoStart = null!;
    private string _activeAIProvider = "None";
    
    private const string RegistryAppName = "DownloadRedirector";

    private class RootSettings
    {
        public WatcherSettings WatcherSettings { get; set; } = new();
    }

    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
        ThemeManager.ApplyTheme(this);
        SyncJsonToVisual();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        SystemHelper.SetNormalPriority();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        SystemHelper.SetBackgroundPriority();
    }

    private void InitializeComponent()
    {
        this.Text = "Download Redirector - Configurações";
        this.Size = new Size(820, 800);
        this.MinimumSize = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 10);
        
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        this.Controls.Add(mainPanel);

        var lblTitle = new Label { Text = "Configurações", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(15, 15) };
        mainPanel.Controls.Add(lblTitle);

        var btnToggleTheme = new Button { Text = "Modo Claro/Escuro", Location = new Point(620, 15), Size = new Size(150, 35), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnToggleTheme.Click += (s, e) => { ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode; ThemeManager.ApplyTheme(this); _tabControl.Invalidate(true); };
        mainPanel.Controls.Add(btnToggleTheme);

        _tabControl = new TabControl { Location = new Point(15, 60), Size = new Size(770, 630), Alignment = TabAlignment.Bottom, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
        
        _tabControl.Deselecting += (s, e) => {
            if (e.TabPage?.Text == "Visual (Fácil)" || e.TabPage?.Text == "Inteligências Artificiais") SyncVisualToJson();
        };
        _tabControl.Selecting += (s, e) => {
            if (e.TabPage?.Text == "Inteligências Artificiais") {
                BuildAIPanels(_tabControl.TabPages[1].Controls.OfType<FlowLayoutPanel>().FirstOrDefault());
            } else if (e.TabPage?.Text == "Visual (Fácil)") {
                SyncJsonToVisual();
            }
            ThemeManager.ApplyTheme(this);
        };

        var tabVisual = new TabPage("Visual (Fácil)");
        var pnlTopVisual = new Panel { Dock = DockStyle.Top, Height = 40 };
        var lblInterval = new Label { Text = "Intervalo (minutos):", AutoSize = true, Location = new Point(10, 10) };
        _numInterval = new NumericUpDown { Location = new Point(140, 8), Width = 60, Minimum = 1, Maximum = 1440, Value = 20 };
        _chkAutoStart = new CheckBox { Text = "Iniciar com Windows", AutoSize = true, Location = new Point(220, 10), Checked = IsAutoStartEnabled() };
        var btnAddFolder = new Button { Text = "+ Adicionar Pasta", Location = new Point(560, 5), Size = new Size(180, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnAddFolder.Click += (s, e) => AddVisualFolder(new WatchedFolder());
        
        pnlTopVisual.Controls.AddRange(new Control[] { lblInterval, _numInterval, _chkAutoStart, btnAddFolder });
        _visualPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        _visualPanel.SizeChanged += (s, e) => {
            _visualPanel.SuspendLayout();
            int w = _visualPanel.ClientSize.Width - 25;
            if (w < 400) w = 400;
            foreach (Control c in _visualPanel.Controls) {
                if (c.MaximumSize.Width != w) c.MaximumSize = new Size(w, 0);
                if (c.MinimumSize.Width != w) c.MinimumSize = new Size(w, 0);
            }
            _visualPanel.ResumeLayout();
        };
        tabVisual.Controls.Add(_visualPanel);
        tabVisual.Controls.Add(pnlTopVisual);

        var tabAI = new TabPage("Inteligências Artificiais");
        var aiFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10) };
        aiFlow.SizeChanged += (s, e) => {
            aiFlow.SuspendLayout();
            int w = aiFlow.ClientSize.Width - 25;
            if (w < 400) w = 400;
            foreach (Control c in aiFlow.Controls) {
                if (c.MaximumSize.Width != w) c.MaximumSize = new Size(w, 0);
                if (c.MinimumSize.Width != w) c.MinimumSize = new Size(w, 0);
            }
            aiFlow.ResumeLayout();
        };
        BuildAIPanels(aiFlow);
        tabAI.Controls.Add(aiFlow);

        var tabJson = new TabPage("Código (Avançado)");
        _jsonEditor = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10), AcceptsTab = true, BorderStyle = BorderStyle.None, Margin = new Padding(10) };
        tabJson.Controls.Add(_jsonEditor);

        _tabControl.TabPages.AddRange(new[] { tabVisual, tabAI, tabJson });
        mainPanel.Controls.Add(_tabControl);

        var btnSave = new Button { Text = "Salvar Arquivo", Location = new Point(635, 700), Size = new Size(150, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
        btnSave.Click += BtnSave_Click;
        mainPanel.Controls.Add(btnSave);
    }

    private void BuildAIPanels(FlowLayoutPanel? parent)
    {
        if (parent == null) return;
        parent.Controls.Clear();
        parent.Controls.Add(CreateAICard("Gemini", "Google Gemini"));
        parent.Controls.Add(CreateAICard("ChatGPT", "OpenAI ChatGPT"));
        // Force layout update so SizeChanged logic applies to new cards
        parent.PerformLayout();
    }

    private Panel CreateAICard(string providerId, string title)
    {
        var pnl = new Panel { Height = 100, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 0, 10), AutoSize = true };
        var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(15, 15) };
        
        var lblKey = new Label { Text = "API Key:", AutoSize = true, Location = new Point(15, 55) };
        var txtKey = new TextBox { Text = CryptoHelper.GetDecryptedKey(providerId), PasswordChar = '*', Location = new Point(80, 52), Width = 400, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        
        var btnSaveKey = new Button { Text = "Salvar Chave", Location = new Point(490, 50), Size = new Size(100, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnSaveKey.Click += (s, e) => 
        {
            CryptoHelper.SaveEncryptedKey(providerId, txtKey.Text);
            MessageBox.Show("Chave encriptada salva no Registro do Windows com sucesso!", "Salvo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var chkActive = new CheckBox { Text = "Definir como Principal", AutoSize = true, Location = new Point(500, 15), Checked = _activeAIProvider == providerId, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        chkActive.Click += (s, e) => 
        {
            if (chkActive.Checked) { _activeAIProvider = providerId; }
            else if (_activeAIProvider == providerId) { _activeAIProvider = "None"; }
            
            if (pnl.Parent != null) {
                foreach (Control c in pnl.Parent.Controls) {
                    if (c is Panel card) {
                        var cb = card.Controls.OfType<CheckBox>().FirstOrDefault();
                        if (cb != null && cb != chkActive) cb.Checked = false;
                    }
                }
            }
        };

        pnl.Controls.AddRange(new Control[] { lblTitle, lblKey, txtKey, btnSaveKey, chkActive });
        return pnl;
    }

    private void SyncJsonToVisual()
    {
        _visualPanel.Controls.Clear();
        try
        {
            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<RootSettings>(_jsonEditor.Text, options);
            if (root?.WatcherSettings != null)
            {
                _numInterval.Value = root.WatcherSettings.ExecutionIntervalMinutes > 0 ? root.WatcherSettings.ExecutionIntervalMinutes : 20;
                _activeAIProvider = root.WatcherSettings.ActiveAIProvider;
                foreach (var folder in root.WatcherSettings.Folders)
                {
                    AddVisualFolder(folder);
                }
            }
        }
        catch { }
    }

    private void SyncVisualToJson()
    {
        var root = new RootSettings();
        root.WatcherSettings.ExecutionIntervalMinutes = (int)_numInterval.Value;
        root.WatcherSettings.ActiveAIProvider = _activeAIProvider;

        foreach (Control c in _visualPanel.Controls)
        {
            if (c is Panel folderPanel && folderPanel.Tag is string str && str == "FOLDER_PANEL")
            {
                var folder = new WatchedFolder();
                var targetsPanel = folderPanel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();

                foreach (Control fc in folderPanel.Controls)
                {
                    if (fc.Name == "txtSource") folder.SourcePath = fc.Text;
                    if (fc.Name == "txtDefaultTarget") folder.DefaultTargetPath = fc.Text;
                }

                if (targetsPanel != null)
                {
                    foreach (Control tc in targetsPanel.Controls)
                    {
                        if (tc is Panel targetPanel && targetPanel.Tag?.ToString() == "TARGET_PANEL")
                        {
                            var target = new TargetFolder();
                            foreach (Control inner in targetPanel.Controls)
                            {
                                if (inner.Name == "txtTarget") target.TargetPath = inner.Text;
                                if (inner is TagInputControl tagInput) target.Extensions = tagInput.GetTags();
                                if (inner.Name == "pnlRenameArea")
                                {
                                    var cmb = inner.Controls.OfType<ComboBox>().FirstOrDefault();
                                    var chk = inner.Controls.OfType<CheckBox>().FirstOrDefault();
                                    var pnlConfig = inner.Controls.Find("pnlConfig", false).FirstOrDefault() as FlowLayoutPanel;
                                    
                                    if (chk != null && chk.Checked && cmb != null)
                                    {
                                        target.RenameMode = cmb.SelectedIndex == 0 ? RenameMode.Manual : RenameMode.AI;
                                        
                                        if (target.RenameMode == RenameMode.Manual && pnlConfig != null)
                                        {
                                            foreach (Control ruleCtrl in pnlConfig.Controls)
                                            {
                                                if (ruleCtrl.Tag?.ToString() == "RULE_ROW")
                                                {
                                                    var txtFind = ruleCtrl.Controls.Find("txtFind", false).FirstOrDefault() as TextBox;
                                                    var txtRep = ruleCtrl.Controls.Find("txtRep", false).FirstOrDefault() as TextBox;
                                                    if (txtFind != null && !string.IsNullOrWhiteSpace(txtFind.Text))
                                                    {
                                                        target.ManualRules.Add(new ManualRenameRule { Find = txtFind.Text, Replace = txtRep?.Text ?? "" });
                                                    }
                                                }
                                            }
                                        }
                                        else if (target.RenameMode == RenameMode.AI && pnlConfig != null)
                                        {
                                            var txtPrompt = pnlConfig.Controls.OfType<TextBox>().FirstOrDefault();
                                            if (txtPrompt != null) target.AIPrompt = txtPrompt.Text;
                                        }
                                    }
                                    else
                                    {
                                        target.RenameMode = RenameMode.None;
                                    }
                                }
                            }
                            folder.Targets.Add(target);
                        }
                    }
                }
                root.WatcherSettings.Folders.Add(folder);
            }
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        _jsonEditor.Text = JsonSerializer.Serialize(root, options);
    }

    private void AddVisualFolder(WatchedFolder folder)
    {
        var panel = new Panel { AutoSize = true, Margin = new Padding(5, 10, 5, 10), Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, Tag = "FOLDER_PANEL" };
        var lblTitle = new Label { Text = "📦 Pasta Monitorada", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
        var btnRemoveFolder = new Button { Text = "X Remover", Location = new Point(620, 10), Size = new Size(90, 25), BackColor = Color.IndianRed, ForeColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnRemoveFolder.Click += (s, e) => { _visualPanel.Controls.Remove(panel); panel.Dispose(); };

        var lblSource = new Label { Text = "Origem:", AutoSize = true, Location = new Point(10, 45) };
        var txtSource = new TextBox { Name = "txtSource", Text = folder.SourcePath, Location = new Point(80, 42), Width = 530, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var btnSource = new Button { Text = "Procurar", Location = new Point(630, 40), Size = new Size(80, 28), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnSource.Click += (s, e) => PickFolder(txtSource);

        var lblDefault = new Label { Text = "Padrão:", AutoSize = true, Location = new Point(10, 80) };
        var txtDefault = new TextBox { Name = "txtDefaultTarget", Text = folder.DefaultTargetPath, Location = new Point(80, 77), Width = 530, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var btnDefault = new Button { Text = "Procurar", Location = new Point(630, 75), Size = new Size(80, 28), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnDefault.Click += (s, e) => PickFolder(txtDefault);
        
        panel.Controls.AddRange(new Control[] { lblTitle, btnRemoveFolder, lblSource, txtSource, btnSource, lblDefault, txtDefault, btnDefault });

        var lblTargetsTitle = new Label { Text = "Regras de Destino (Targets):", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(10, 120) };
        var btnAddTarget = new Button { Text = "+ Adicionar Regra", Location = new Point(220, 115), Size = new Size(130, 28) };
        panel.Controls.AddRange(new Control[] { lblTargetsTitle, btnAddTarget });

        var targetsFlow = new FlowLayoutPanel { Location = new Point(10, 150), Width = 700, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        targetsFlow.SizeChanged += (s, e) => {
            targetsFlow.SuspendLayout();
            int w = targetsFlow.ClientSize.Width - 10;
            if (w < 300) w = 300;
            foreach (Control c in targetsFlow.Controls) {
                if (c.MaximumSize.Width != w) c.MaximumSize = new Size(w, 0);
                if (c.MinimumSize.Width != w) c.MinimumSize = new Size(w, 0);
            }
            targetsFlow.ResumeLayout();
        };
        
        foreach (var target in folder.Targets) AddVisualTarget(targetsFlow, target);
        btnAddTarget.Click += (s, e) => { AddVisualTarget(targetsFlow, new TargetFolder()); ThemeManager.ApplyTheme(this); };

        panel.Controls.Add(targetsFlow);
        
        if (_visualPanel.ClientSize.Width > 0)
        {
            int w = _visualPanel.ClientSize.Width - 25;
            panel.MinimumSize = new Size(w, 0);
            panel.MaximumSize = new Size(w, 0);
        }

        _visualPanel.Controls.Add(panel);
    }

    private void AddVisualTarget(FlowLayoutPanel parentFlow, TargetFolder target)
    {
        var pnl = new Panel { AutoSize = true, Padding = new Padding(0,0,0,10), Margin = new Padding(0, 5, 0, 5), BorderStyle = BorderStyle.FixedSingle, Tag = "TARGET_PANEL" };
        var lblDest = new Label { Text = "Destino:", AutoSize = true, Location = new Point(5, 12) };
        var txtDest = new TextBox { Name = "txtTarget", Text = target.TargetPath, Location = new Point(70, 10), Width = 520, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var btnDest = new Button { Text = "Procurar", Location = new Point(600, 8), Size = new Size(80, 28), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnDest.Click += (s, e) => PickFolder(txtDest);
        var btnRemove = new Button { Text = "X", Location = new Point(690, 8), Size = new Size(30, 28), BackColor = Color.IndianRed, ForeColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnRemove.Click += (s, e) => { parentFlow.Controls.Remove(pnl); pnl.Dispose(); };

        var lblExts = new Label { Text = "Extensões:", AutoSize = true, Location = new Point(5, 45) };
        var tagInput = new TagInputControl { Location = new Point(80, 40), Width = 640, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        tagInput.SetTags(target.Extensions);
        
        var pnlRenameArea = new Panel { Name = "pnlRenameArea", Location = new Point(5, 85), Width = 710, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var chkRename = new CheckBox { Text = "Renomear arquivos", AutoSize = true, Location = new Point(5, 5), Checked = target.RenameMode != RenameMode.None };
        var cmbRenameMode = new ComboBox { Location = new Point(160, 3), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbRenameMode.Items.AddRange(new[] { "Manual", "IA" });
        if (target.RenameMode != RenameMode.None) cmbRenameMode.SelectedIndex = target.RenameMode == RenameMode.Manual ? 0 : 1;
        cmbRenameMode.Visible = chkRename.Checked;

        var pnlConfig = new FlowLayoutPanel { Name = "pnlConfig", Location = new Point(5, 35), Width = 700, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        pnlConfig.SizeChanged += (s, e) => {
            pnlConfig.SuspendLayout();
            int w = pnlConfig.ClientSize.Width - 10;
            if (w < 200) w = 200;
            foreach (Control c in pnlConfig.Controls) {
                if (c.MaximumSize.Width != w) c.MaximumSize = new Size(w, 0);
                if (c.MinimumSize.Width != w) c.MinimumSize = new Size(w, 0);
            }
            pnlConfig.ResumeLayout();
        };
        
        Action updateConfigUI = () =>
        {
            pnlConfig.Controls.Clear();
            if (!chkRename.Checked) { cmbRenameMode.Visible = false; return; }
            cmbRenameMode.Visible = true;
            if (cmbRenameMode.SelectedIndex == 0) // Manual
            {
                var btnAddRule = new Button { Text = "+ Adicionar Substituição", Size = new Size(160, 25) };
                pnlConfig.Controls.Add(btnAddRule);
                foreach (var rule in target.ManualRules) AddManualRuleUI(pnlConfig, rule);
                btnAddRule.Click += (s,e) => { AddManualRuleUI(pnlConfig, new ManualRenameRule()); ThemeManager.ApplyTheme(this); };
            }
            else if (cmbRenameMode.SelectedIndex == 1) // IA
            {
                if (_activeAIProvider == "None")
                {
                    var lblNoAI = new Label { Text = "Nenhuma IA conectada.", AutoSize = true, ForeColor = Color.IndianRed };
                    var btnGoAI = new Button { Text = "Conectar IA", Size = new Size(120, 25) };
                    btnGoAI.Click += (s, ev) => _tabControl.SelectedIndex = 1;
                    pnlConfig.Controls.AddRange(new Control[] { lblNoAI, btnGoAI });
                }
                else
                {
                    var lblPrompt = new Label { Text = "Prompt/Regras para a IA:", AutoSize = true };
                    var txtPrompt = new TextBox { Text = target.AIPrompt, Multiline = true, Width = 680, Height = 60, ScrollBars = ScrollBars.Vertical, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    pnlConfig.Controls.AddRange(new Control[] { lblPrompt, txtPrompt });
                }
            }
        };

        chkRename.CheckedChanged += (s, e) => { if (chkRename.Checked && cmbRenameMode.SelectedIndex == -1) cmbRenameMode.SelectedIndex = 0; updateConfigUI(); };
        cmbRenameMode.SelectedIndexChanged += (s, e) => updateConfigUI();
        updateConfigUI();

        pnlRenameArea.Controls.AddRange(new Control[] { chkRename, cmbRenameMode, pnlConfig });
        pnl.Controls.AddRange(new Control[] { lblDest, txtDest, btnDest, btnRemove, lblExts, tagInput, pnlRenameArea });
        
        if (parentFlow.ClientSize.Width > 0)
        {
            int w = parentFlow.ClientSize.Width - 10;
            pnl.MinimumSize = new Size(w, 0);
            pnl.MaximumSize = new Size(w, 0);
        }

        parentFlow.Controls.Add(pnl);
    }

    private void AddManualRuleUI(FlowLayoutPanel parent, ManualRenameRule rule)
    {
        var tlp = new TableLayoutPanel { 
            Tag = "RULE_ROW", AutoSize = true, Height = 35,
            ColumnCount = 5, RowCount = 1,
            Margin = new Padding(0)
        };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));

        var lbl1 = new Label { Text = "Trocar", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Padding = new Padding(0, 5, 0, 0) };
        var txtFind = new TextBox { Name = "txtFind", Text = rule.Find, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        var lbl2 = new Label { Text = "por", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Padding = new Padding(0, 5, 0, 0) };
        var txtRep = new TextBox { Name = "txtRep", Text = rule.Replace, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        var btnDel = new Button { Text = "X", Size = new Size(30, 25), BackColor = Color.IndianRed, ForeColor = Color.White, Anchor = AnchorStyles.Right | AnchorStyles.Top };
        btnDel.Click += (s, e) => parent.Controls.Remove(tlp);

        tlp.Controls.Add(lbl1, 0, 0);
        tlp.Controls.Add(txtFind, 1, 0);
        tlp.Controls.Add(lbl2, 2, 0);
        tlp.Controls.Add(txtRep, 3, 0);
        tlp.Controls.Add(btnDel, 4, 0);

        if (parent.ClientSize.Width > 0)
        {
            int w = parent.ClientSize.Width - 10;
            tlp.MinimumSize = new Size(w, 0);
            tlp.MaximumSize = new Size(w, 0);
        }

        parent.Controls.Add(tlp);
        parent.Controls.SetChildIndex(tlp, parent.Controls.Count - 2);
    }

    private void PickFolder(TextBox targetTextBox)
    {
        using var dialog = new FolderBrowserDialog { Description = "Selecione a pasta", UseDescriptionForTitle = true, ShowNewFolderButton = true };
        if (dialog.ShowDialog() == DialogResult.OK) targetTextBox.Text = dialog.SelectedPath;
    }

    private void LoadSettings()
    {
        if (File.Exists(_settingsPath)) _jsonEditor.Text = File.ReadAllText(_settingsPath);
        else _jsonEditor.Text = "{\n  \"WatcherSettings\": {\n    \"ExecutionIntervalMinutes\": 20,\n    \"Folders\": []\n  }\n}";
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_tabControl.SelectedTab?.Text == "Visual (Fácil)" || _tabControl.SelectedTab?.Text == "Inteligências Artificiais") SyncVisualToJson();
            File.WriteAllText(_settingsPath, _jsonEditor.Text);
            SetAutoStart(_chkAutoStart.Checked);
            MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private bool IsAutoStartEnabled()
    {
        try { using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false); return key?.GetValue(RegistryAppName) != null; }
        catch { return false; }
    }

    private void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null)
            {
                if (enable) key.SetValue(RegistryAppName, $"\"{Application.ExecutablePath}\"");
                else key.DeleteValue(RegistryAppName, false);
            }
        }
        catch { }
    }
}
