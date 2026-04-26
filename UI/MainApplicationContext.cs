using System;
using System.Drawing;
using System.Windows.Forms;
using download_redirector.Services;
using Microsoft.Extensions.Hosting;

namespace download_redirector.UI;

public class MainApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly OrganizerService _organizerService;
    private readonly IHostApplicationLifetime _lifetime;
    
    public MainApplicationContext(OrganizerService organizerService, IHostApplicationLifetime lifetime)
    {
        _organizerService = organizerService;
        _lifetime = lifetime;

        var contextMenu = new ContextMenuStrip();
        
        var settingsItem = new ToolStripMenuItem("Configurações...", null, OnSettingsClicked);
        var pauseItem = new ToolStripMenuItem("Pausar", null, OnPauseClicked);
        var resumeItem = new ToolStripMenuItem("Retomar", null, OnResumeClicked);
        var exitItem = new ToolStripMenuItem("Sair", null, OnExitClicked);

        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(pauseItem);
        contextMenu.Items.Add(resumeItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = GenerateTrayIcon(),
            ContextMenuStrip = contextMenu,
            Visible = true,
            Text = "Download Redirector - Ativo"
        };
    }

    private Form? _settingsForm;

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        if (_settingsForm == null || _settingsForm.IsDisposed)
        {
            _settingsForm = new SettingsForm();
            _settingsForm.Show();
        }
        else
        {
            if (_settingsForm.WindowState == FormWindowState.Minimized)
                _settingsForm.WindowState = FormWindowState.Normal;
            _settingsForm.BringToFront();
        }
    }

    private void OnPauseClicked(object? sender, EventArgs e)
    {
        _organizerService.Pause();
        _notifyIcon.Text = "Download Redirector - Pausado";
        _notifyIcon.BalloonTipTitle = "Download Redirector";
        _notifyIcon.BalloonTipText = "Organização automática pausada.";
        _notifyIcon.ShowBalloonTip(2000);
    }

    private void OnResumeClicked(object? sender, EventArgs e)
    {
        _organizerService.Resume();
        _notifyIcon.Text = "Download Redirector - Ativo";
        _notifyIcon.BalloonTipTitle = "Download Redirector";
        _notifyIcon.BalloonTipText = "Organização automática retomada.";
        _notifyIcon.ShowBalloonTip(2000);
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _lifetime.StopApplication();
        Application.Exit();
    }

    private Icon GenerateTrayIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);
        
        using var brush = new SolidBrush(Color.DodgerBlue);
        graphics.FillEllipse(brush, 1, 1, 14, 14);
        
        using var pen = new Pen(Color.White, 2f);
        graphics.DrawLine(pen, 8, 4, 8, 12);
        graphics.DrawLine(pen, 8, 12, 5, 9);
        graphics.DrawLine(pen, 8, 12, 11, 9);
        
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
