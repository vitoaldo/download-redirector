using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace download_redirector.UI;

public class SettingsForm : Form
{
    private readonly string _settingsPath = "appsettings.json";
    private RichTextBox _jsonEditor = null!;

    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
        ThemeManager.ApplyTheme(this);
    }

    private void InitializeComponent()
    {
        this.Text = "Download Redirector - Configurações";
        this.Size = new Size(700, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 10);
        
        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        this.Controls.Add(mainPanel);

        var lblTitle = new Label 
        { 
            Text = "Configurações Avançadas", 
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };
        mainPanel.Controls.Add(lblTitle);

        var btnToggleTheme = new Button
        {
            Text = "Modo Claro/Escuro",
            Location = new Point(500, 20),
            Size = new Size(150, 35)
        };
        btnToggleTheme.Click += (s, e) => 
        {
            ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode;
            ThemeManager.ApplyTheme(this);
        };
        mainPanel.Controls.Add(btnToggleTheme);

        var lblInfo = new Label
        {
            Text = "Edite o arquivo JSON de regras abaixo. As mudanças entrarão em vigor automaticamente no próximo ciclo.",
            AutoSize = true,
            Location = new Point(20, 70)
        };
        mainPanel.Controls.Add(lblInfo);

        _jsonEditor = new RichTextBox
        {
            Location = new Point(20, 100),
            Size = new Size(630, 380),
            Font = new Font("Consolas", 10),
            AcceptsTab = true,
            BorderStyle = BorderStyle.None
        };
        mainPanel.Controls.Add(_jsonEditor);

        var btnSave = new Button
        {
            Text = "Salvar Arquivo",
            Location = new Point(500, 500),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnSave.Click += BtnSave_Click;
        mainPanel.Controls.Add(btnSave);
    }

    private void LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            _jsonEditor.Text = File.ReadAllText(_settingsPath);
        }
        else
        {
            _jsonEditor.Text = "{\n  // Arquivo não encontrado\n}";
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
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
