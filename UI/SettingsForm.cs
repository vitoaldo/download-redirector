using download_redirector.Models;
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

    private class RootSettings
    {
        public WatcherSettings WatcherSettings { get; set; } = new();
    }

    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
        ThemeManager.ApplyTheme(this);
        SyncJsonToVisual(); // Populate visual on load
    }

    private void InitializeComponent()
    {
        this.Text = "Download Redirector - Configurações";
        this.Size = new Size(800, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 10);
        
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        this.Controls.Add(mainPanel);

        var lblTitle = new Label 
        { 
            Text = "Configurações", 
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(15, 15)
        };
        mainPanel.Controls.Add(lblTitle);

        var btnToggleTheme = new Button
        {
            Text = "Modo Claro/Escuro",
            Location = new Point(600, 15),
            Size = new Size(150, 35)
        };
        btnToggleTheme.Click += (s, e) => 
        {
            ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode;
            ThemeManager.ApplyTheme(this);
            _tabControl.Invalidate(true);
        };
        mainPanel.Controls.Add(btnToggleTheme);

        _tabControl = new TabControl
        {
            Location = new Point(15, 60),
            Size = new Size(750, 580),
            Alignment = TabAlignment.Bottom
        };
        _tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

        // Visual Tab
        var tabVisual = new TabPage("Visual (Fácil)");
        
        var pnlTopVisual = new Panel { Dock = DockStyle.Top, Height = 40 };
        var lblInterval = new Label { Text = "Intervalo de Execução (minutos):", AutoSize = true, Location = new Point(10, 10) };
        _numInterval = new NumericUpDown { Location = new Point(230, 8), Width = 80, Minimum = 1, Maximum = 1440, Value = 20 };
        
        var btnAddFolder = new Button { Text = "+ Adicionar Pasta Vigiada", Location = new Point(540, 5), Size = new Size(190, 30) };
        btnAddFolder.Click += (s, e) => AddVisualFolder(new WatchedFolder());

        pnlTopVisual.Controls.Add(lblInterval);
        pnlTopVisual.Controls.Add(_numInterval);
        pnlTopVisual.Controls.Add(btnAddFolder);

        _visualPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        tabVisual.Controls.Add(_visualPanel);
        tabVisual.Controls.Add(pnlTopVisual);

        // JSON Tab
        var tabJson = new TabPage("Código (Avançado)");
        _jsonEditor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10),
            AcceptsTab = true,
            BorderStyle = BorderStyle.None,
            Margin = new Padding(10)
        };
        tabJson.Controls.Add(_jsonEditor);

        _tabControl.TabPages.Add(tabVisual);
        _tabControl.TabPages.Add(tabJson);
        mainPanel.Controls.Add(_tabControl);

        var btnSave = new Button
        {
            Text = "Salvar Arquivo",
            Location = new Point(615, 650),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;
        mainPanel.Controls.Add(btnSave);
    }

    private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_tabControl.SelectedIndex == 0) // Switch to Visual
        {
            SyncJsonToVisual();
            ThemeManager.ApplyTheme(this);
        }
        else // Switch to JSON
        {
            SyncVisualToJson();
        }
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
                foreach (var folder in root.WatcherSettings.Folders)
                {
                    AddVisualFolder(folder);
                }
            }
        }
        catch
        {
            // Fallback ou limpa se o json for inválido
        }
    }

    private void SyncVisualToJson()
    {
        var root = new RootSettings();
        root.WatcherSettings.ExecutionIntervalMinutes = (int)_numInterval.Value;

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
                        if (tc is Panel targetPanel)
                        {
                            var target = new TargetFolder();
                            foreach (Control inner in targetPanel.Controls)
                            {
                                if (inner.Name == "txtTarget") target.TargetPath = inner.Text;
                                if (inner is TagInputControl tagInput) target.Extensions = tagInput.GetTags();
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
        var panel = new Panel
        {
            Width = 700,
            AutoSize = true,
            Margin = new Padding(5, 10, 5, 10),
            Padding = new Padding(10),
            BorderStyle = BorderStyle.FixedSingle,
            Tag = "FOLDER_PANEL"
        };

        var lblTitle = new Label { Text = "📦 Pasta Monitorada", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
        
        var btnRemoveFolder = new Button { Text = "X Remover", Location = new Point(600, 10), Size = new Size(90, 25), BackColor = Color.IndianRed, ForeColor = Color.White };
        btnRemoveFolder.Click += (s, e) => { _visualPanel.Controls.Remove(panel); panel.Dispose(); };

        var lblSource = new Label { Text = "Origem (SourcePath):", AutoSize = true, Location = new Point(10, 45) };
        var txtSource = new TextBox { Name = "txtSource", Text = folder.SourcePath, Location = new Point(170, 42), Width = 430 };
        var btnSource = new Button { Text = "Procurar", Location = new Point(610, 40), Size = new Size(80, 28) };
        btnSource.Click += (s, e) => PickFolder(txtSource);

        var lblDefault = new Label { Text = "Padrão (DefaultTarget):", AutoSize = true, Location = new Point(10, 80) };
        var txtDefault = new TextBox { Name = "txtDefaultTarget", Text = folder.DefaultTargetPath, Location = new Point(170, 77), Width = 430 };
        var btnDefault = new Button { Text = "Procurar", Location = new Point(610, 75), Size = new Size(80, 28) };
        btnDefault.Click += (s, e) => PickFolder(txtDefault);

        panel.Controls.Add(lblTitle);
        panel.Controls.Add(btnRemoveFolder);
        panel.Controls.Add(lblSource);
        panel.Controls.Add(txtSource);
        panel.Controls.Add(btnSource);
        panel.Controls.Add(lblDefault);
        panel.Controls.Add(txtDefault);
        panel.Controls.Add(btnDefault);

        var lblTargetsTitle = new Label { Text = "Regras de Destino (Targets):", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(10, 120) };
        var btnAddTarget = new Button { Text = "+ Adicionar Regra", Location = new Point(220, 115), Size = new Size(130, 28) };
        panel.Controls.Add(lblTargetsTitle);
        panel.Controls.Add(btnAddTarget);

        var targetsFlow = new FlowLayoutPanel
        {
            Location = new Point(10, 150),
            Width = 680,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        foreach (var target in folder.Targets)
        {
            AddVisualTarget(targetsFlow, target);
        }

        btnAddTarget.Click += (s, e) => { AddVisualTarget(targetsFlow, new TargetFolder()); ThemeManager.ApplyTheme(this); };

        panel.Controls.Add(targetsFlow);
        _visualPanel.Controls.Add(panel);
    }

    private void AddVisualTarget(FlowLayoutPanel parentFlow, TargetFolder target)
    {
        var pnl = new Panel
        {
            Width = 660,
            Height = 85,
            Margin = new Padding(0, 5, 0, 5),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblDest = new Label { Text = "Destino:", AutoSize = true, Location = new Point(5, 12) };
        var txtDest = new TextBox { Name = "txtTarget", Text = target.TargetPath, Location = new Point(70, 10), Width = 450 };
        var btnDest = new Button { Text = "Procurar", Location = new Point(530, 8), Size = new Size(80, 28) };
        btnDest.Click += (s, e) => PickFolder(txtDest);

        var btnRemove = new Button { Text = "X", Location = new Point(620, 8), Size = new Size(30, 28), BackColor = Color.IndianRed, ForeColor = Color.White };
        btnRemove.Click += (s, e) => { parentFlow.Controls.Remove(pnl); pnl.Dispose(); };

        var lblExts = new Label { Text = "Extensões:", AutoSize = true, Location = new Point(5, 45) };
        var tagInput = new TagInputControl { Location = new Point(80, 40), Width = 560 };
        tagInput.SetTags(target.Extensions);

        pnl.Controls.Add(lblDest);
        pnl.Controls.Add(txtDest);
        pnl.Controls.Add(btnDest);
        pnl.Controls.Add(btnRemove);
        pnl.Controls.Add(lblExts);
        pnl.Controls.Add(tagInput);

        parentFlow.Controls.Add(pnl);
    }

    private void PickFolder(TextBox targetTextBox)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Selecione a pasta",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            targetTextBox.Text = dialog.SelectedPath;
        }
    }

    private void LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            _jsonEditor.Text = File.ReadAllText(_settingsPath);
        }
        else
        {
            _jsonEditor.Text = "{\n  \"WatcherSettings\": {\n    \"ExecutionIntervalMinutes\": 20,\n    \"Folders\": []\n  }\n}";
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_tabControl.SelectedIndex == 0)
            {
                SyncVisualToJson();
            }

            File.WriteAllText(_settingsPath, _jsonEditor.Text);
            MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
