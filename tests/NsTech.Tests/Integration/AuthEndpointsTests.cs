using System.Net.Http.Json;
using FluentAssertions;

namespace NsTech.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_With_Valid_Credentials_Should_Return_Token()
    {
        var response = await _client.PostAsync("/auth/token?username=admin&password=admin", null);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        content!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_With_Invalid_Credentials_Should_Return_Unauthorized()
    {
        var response = await _client.PostAsync("/auth/token?username=wrong&password=wrong", null);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private record TokenResponse(string Token);
}
