using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Repositories;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class TokenStorageService : ITokenStorageService
{
    private readonly ILogger<TokenStorageService> _logger;
    private readonly string _tokenFilePath;

    public TokenStorageService(ILogger<TokenStorageService> logger)
    {
        _logger = logger;

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _tokenFilePath = Path.Combine(baseDirectory, "token.json");

        _logger.LogInformation($"Token file path: {_tokenFilePath}");

        var directory = Path.GetDirectoryName(_tokenFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation($"Created directory: {directory}");
        }
    }

    /// <inheritdoc />
    public async Task SaveTokenAsync(TokenResponse tokenResponse)
    {
        try
        {
            var json = JsonSerializer.Serialize(tokenResponse, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_tokenFilePath, json);
            _logger.LogInformation($"Token saved successfully to: {_tokenFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to save token to: {_tokenFilePath}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TokenResponse?> LoadTokenAsync()
    {
        try
        {
            if (!File.Exists(_tokenFilePath))
            {
                _logger.LogInformation($"No token file found at: {_tokenFilePath}");
                return null;
            }

            var json = await File.ReadAllTextAsync(_tokenFilePath);
            _logger.LogInformation($"Token file loaded from: {_tokenFilePath}");

            var token = JsonSerializer.Deserialize<TokenResponse>(json);
            if (token == null)
            {
                _logger.LogWarning("Token file was empty or invalid");
                return null;
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load token from: {_tokenFilePath}");
            return null;
        }
    }
}