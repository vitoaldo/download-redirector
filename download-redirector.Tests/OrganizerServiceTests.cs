using download_redirector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace download_redirector.Tests;

public class OrganizerServiceTests
{
    [Fact]
    public void Pause_ShouldSetIsPausedToTrue()
    {
        var config = new ConfigurationBuilder().Build();
        var service = new OrganizerService(NullLogger<OrganizerService>.Instance, config);

        service.Pause();

        Assert.True(service.IsPaused);
    }

    [Fact]
    public void Resume_ShouldSetIsPausedToFalse()
    {
        var config = new ConfigurationBuilder().Build();
        var service = new OrganizerService(NullLogger<OrganizerService>.Instance, config);
        service.Pause();

        service.Resume();

        Assert.False(service.IsPaused);
    }
}
