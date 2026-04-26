using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace download_redirector.UI;

public class TagInputControl : UserControl
{
    private FlowLayoutPanel _flowPanel;
    private TextBox _inputBox;

    public TagInputControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.BorderStyle = BorderStyle.FixedSingle;
        this.Size = new Size(400, 40);
        this.BackColor = ThemeManager.PanelColor;

        _flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(3),
            BackColor = Color.Transparent
        };
        _flowPanel.Click += (s, e) => _inputBox.Focus();

        _inputBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Width = 80,
            Margin = new Padding(3, 5, 0, 0),
            BackColor = ThemeManager.PanelColor,
            ForeColor = ThemeManager.ForeColor,
            Font = new Font("Segoe UI", 9)
        };
        _inputBox.KeyDown += InputBox_KeyDown;
        _inputBox.TextChanged += InputBox_TextChanged;

        _flowPanel.Controls.Add(_inputBox);
        this.Controls.Add(_flowPanel);
    }

    private void InputBox_TextChanged(object? sender, EventArgs e)
    {
        Size size = TextRenderer.MeasureText(_inputBox.Text + "WW", _inputBox.Font);
        _inputBox.Width = Math.Max(80, size.Width);
    }

    private void InputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Oemcomma)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            string tagText = _inputBox.Text.Trim(' ', ',');
            if (!string.IsNullOrWhiteSpace(tagText))
            {
                if (!tagText.StartsWith(".")) tagText = "." + tagText;
                AddTag(tagText);
                _inputBox.Clear();
            }
        }
        else if (e.KeyCode == Keys.Back && string.IsNullOrEmpty(_inputBox.Text))
        {
            // Remove last tag if backspace is pressed on empty input
            var tags = _flowPanel.Controls.OfType<Panel>().ToList();
            if (tags.Any())
            {
                var lastTag = tags.Last();
                _flowPanel.Controls.Remove(lastTag);
            }
        }
    }

    public void AddTag(string text)
    {
        if (GetTags().Contains(text, StringComparer.OrdinalIgnoreCase)) return;

        var tagPanel = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = ThemeManager.ButtonColor,
            ForeColor = ThemeManager.ButtonForeColor,
            Margin = new Padding(2),
            Padding = new Padding(3)
        };

        // Arredondamento (GDI+)
        tagPanel.Paint += (s, pe) =>
        {
            var p = s as Panel;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(p.BackColor);
            pe.Graphics.FillRoundedRectangle(brush, new Rectangle(0, 0, p.Width, p.Height), 4);
        };

        var lblText = new Label
        {
            Text = text,
            AutoSize = true,
            Location = new Point(3, 3),
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9)
        };

        var lblRemove = new Label
        {
            Text = "X",
            AutoSize = true,
            Location = new Point(lblText.Right + 2, 3),
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        lblRemove.Click += (s, e) => _flowPanel.Controls.Remove(tagPanel);

        lblText.SizeChanged += (s, e) => lblRemove.Location = new Point(lblText.Right + 2, 3);

        tagPanel.Controls.Add(lblText);
        tagPanel.Controls.Add(lblRemove);

        _flowPanel.Controls.Add(tagPanel);
        _flowPanel.Controls.SetChildIndex(tagPanel, _flowPanel.Controls.Count - 2);
    }

    public List<string> GetTags()
    {
        var tags = new List<string>();
        foreach (Control c in _flowPanel.Controls)
        {
            if (c is Panel p)
            {
                foreach (Control child in p.Controls)
                {
                    if (child is Label lbl && lbl.Text != "X")
                    {
                        tags.Add(lbl.Text);
                        break;
                    }
                }
            }
        }
        return tags;
    }

    public void SetTags(IEnumerable<string> tags)
    {
        var panelsToRemove = _flowPanel.Controls.OfType<Panel>().ToList();
        foreach (var p in panelsToRemove) _flowPanel.Controls.Remove(p);

        foreach (var t in tags)
        {
            if (!string.IsNullOrWhiteSpace(t)) AddTag(t);
        }
    }

    public void ApplyTheme()
    {
        this.BackColor = ThemeManager.PanelColor;
        _inputBox.BackColor = ThemeManager.PanelColor;
        _inputBox.ForeColor = ThemeManager.ForeColor;
        
        foreach (Control c in _flowPanel.Controls)
        {
            if (c is Panel p)
            {
                p.BackColor = ThemeManager.ButtonColor;
                p.ForeColor = ThemeManager.ButtonForeColor;
                p.Invalidate();
            }
        }
    }
}

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
    {
        using System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = cornerRadius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
