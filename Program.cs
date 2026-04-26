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
    // Constante nativa do Kernel32 para forçar o processo a rodar com Prioridade de I/O de Background (Zero impacto no SSD/HD)
    private const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetPriorityClass(IntPtr handle, uint priorityClass);

    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            using var process = Process.GetCurrentProcess();
            // Primeiro seta a prioridade nativa da CPU para Idle
            process.PriorityClass = ProcessPriorityClass.Idle;
            
            // Depois aciona o modo Background I/O do Kernel para garantir que a cópia de arquivos ceda prioridade ao usuário
            SetPriorityClass(process.Handle, PROCESS_MODE_BACKGROUND_BEGIN);
        }
        catch 
        {
            // Failsafe silencioso: se faltarem privilégios, o aplicativo continua operando normalmente
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
