using System.Drawing;
using System.Windows.Forms;

namespace download_redirector.UI;

public static class ThemeManager
{
    public static bool IsDarkMode { get; set; } = true;

    public static Color BackColor => IsDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromArgb(240, 240, 240);
    public static Color ForeColor => IsDarkMode ? Color.White : Color.Black;
    public static Color PanelColor => IsDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
    public static Color ButtonColor => IsDarkMode ? Color.FromArgb(0, 122, 204) : Color.DodgerBlue;
    public static Color ButtonForeColor => Color.White;

    public static void ApplyTheme(Form form)
    {
        form.BackColor = BackColor;
        form.ForeColor = ForeColor;

        foreach (Control control in form.Controls)
        {
            ApplyThemeToControl(control);
        }
    }

    private static void ApplyThemeToControl(Control control)
    {
        control.BackColor = BackColor;
        control.ForeColor = ForeColor;

        if (control is Panel panel)
        {
            panel.BackColor = PanelColor;
        }
        else if (control is Button button)
        {
            button.BackColor = ButtonColor;
            button.ForeColor = ButtonForeColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
        }
        else if (control is TextBox || control is NumericUpDown || control is RichTextBox)
        {
            control.BackColor = PanelColor;
            control.ForeColor = ForeColor;
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child);
        }
    }
}
