using download_redirector;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DownloadRedirectorService";
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
