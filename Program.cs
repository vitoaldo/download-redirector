using download_redirector.Services;
using download_redirector.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace download_redirector;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            download_redirector.Services.SystemHelper.SetBackgroundPriority();
        }
        catch 
        {
        }

        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddSingleton<OrganizerService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<OrganizerService>());
        builder.Services.AddSingleton<MainApplicationContext>();

        var host = builder.Build();

        _ = host.StartAsync();

        var appContext = host.Services.GetRequiredService<MainApplicationContext>();
        Application.Run(appContext);

        host.StopAsync().GetAwaiter().GetResult();
    }
}
