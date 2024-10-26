using Microsoft.Extensions.Logging;
using SubredditWatcher.Application.Interfaces;
using SubredditWatcher.Domain.Services;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Application;

public class Application(
    IRedditAuthService authService,
    ITokenStorageService tokenStorageService,
    IUserInterface ui,
    TokenManager tokenManager,
    IRedditService redditService,
    ILogger<Application> logger)
{
    public async Task RunAsync()
    {
        logger.LogInformation("Application started.");

        var tokenResponse = await tokenStorageService.LoadTokenAsync();
        if (tokenResponse != null)
        {
            ui.WriteLine("Token found. Using saved token.");
        }
        else
        {
            ui.WriteLine("No saved token found. Starting authentication process.");
            tokenResponse = await authService.AuthenticateAsync();
            await tokenStorageService.SaveTokenAsync(tokenResponse);
            ui.WriteLine("Token obtained and saved for future use.");
        }

        tokenManager.Initialize(tokenResponse);

        await redditService.StartMonitoringAsync();

        logger.LogInformation("Application finished.");
    }
}