using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Modules.Users;

public interface IUsersService
{
    Task<User> GetUsersByIdAsync(UserId id, CancellationToken cancellationToken);
    Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<UserId> ids, CancellationToken cancellationToken);
}

internal sealed class UsersService(IConfiguration configuration, HybridCache cache, ILogger<UsersService> logger) : IUsersService
{
    private readonly string domain = configuration["Auth0:Domain"] ?? throw new InvalidOperationException("Auth0:Domain is missing in configuration");
    private readonly string audience = configuration["Auth0:ManagementApi:Audience"] ?? throw new InvalidOperationException("Auth0:ManagementApi:Audience is missing in configuration");
    private readonly string clientId = configuration["Auth0:ManagementApi:ClientId"] ?? throw new InvalidOperationException("Auth0:ManagementApi:ClientId is missing in configuration");
    private readonly string clientSecret = configuration["Auth0:ManagementApi:ClientSecret"] ?? throw new InvalidOperationException("Auth0:ManagementApi:ClientSecret is missing in configuration");
    
    public async Task<User> GetUsersByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        using var managementClient = await GetManagementApiClientAsync(cancellationToken);
        
        return await GetUsersByIdAsync(id, managementClient, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<UserId> ids, CancellationToken cancellationToken)
    {
        using var managementClient = await GetManagementApiClientAsync(cancellationToken);
        
        var users = ids.Select(id => GetUsersByIdAsync(id, managementClient, cancellationToken)).ToList();
        return await Task.WhenAll(users);
    }

    private async Task<User> GetUsersByIdAsync(UserId id, ManagementApiClient managementApiClient, CancellationToken cancellationToken)
    {
        try
        {
            var user = await cache.GetOrCreateAsync($"user-{id}", async ct =>
            {
                var result = await managementApiClient.Users.GetAsync(id.Value, "user_id,email,username,picture", true, ct);
                return new User(UserId.Parse(result.UserId), result.Email, result.UserName, result.Picture);
            }, cancellationToken: cancellationToken);

            return user;
        }
        catch (RateLimitApiException ex)
        {
            logger.LogCritical(ex, "Rate limit exceeded when trying to get Auth0 user details");
            throw;
        }
        catch (ErrorApiException ex)
        {
            logger.LogError(ex, "Error occurred when trying to get Auth0 user details");
            throw;
        }
    }
    
    private async Task<ManagementApiClient> GetManagementApiClientAsync(CancellationToken ct)
    {
        var client = new AuthenticationApiClient(domain);

        try
        {
            var accessTokenResponse = await client.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Audience = audience,
            }, ct);
            
            return new ManagementApiClient(accessTokenResponse.AccessToken, new Uri($"https://{domain}/api/v2/"));
        }
        catch (RateLimitApiException ex)
        {
            logger.LogCritical(ex, "Rate limit exceeded when trying to get Auth0 Management API token");
            throw;
        }
        catch (ErrorApiException ex)
        {
            logger.LogError(ex, "Error occurred when trying to get Auth0 Management API token");
            throw;
        }
    }

}