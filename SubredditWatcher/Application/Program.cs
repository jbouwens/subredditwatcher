using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubredditWatcher.Application;
using SubredditWatcher.Application.Interfaces;
using SubredditWatcher.Application.Services;
using SubredditWatcher.Domain.Services;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Repositories;
using SubredditWatcher.Infrastructure.Repositories.Interfaces;
using SubredditWatcher.Infrastructure.Services.Implementations;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.UI;
using SubredditWatcher.UI.Interfaces;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) => { config.AddJsonFile("local.settings.json", false, true); })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<RedditSettings>(configuration.GetSection("RedditSettings"));

        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<RedditSettings>>().Value);

        services.AddSingleton<IRedditAuthService, RedditAuthService>();
        services.AddSingleton<IRedditRepository, RedditRepository>();
        services.AddSingleton<IUserInterface, ConsoleUserInterface>();
        services.AddSingleton<IUiRenderer, SpectreUiRenderer>();
        services.AddSingleton<IHttpListener, HttpListenerWrapper>();
        services.AddSingleton<Application>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<TokenManager>();
        services.AddSingleton<IRedditRepository, RedditRepository>();
        services.AddSingleton<Application>();
        services.AddSingleton<IBrowserLauncher, DefaultBrowserLauncher>();
        services.AddSingleton<IRedditService, RedditService>();

        services.AddSingleton<Application>();
    })
    .ConfigureLogging(logging => { logging.ClearProviders(); })
    .Build();

var app = host.Services.GetRequiredService<Application>();
await app.RunAsync();