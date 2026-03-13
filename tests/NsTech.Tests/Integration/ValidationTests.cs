using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using FluentAssertions;
using NsTech.Application.Features.Products.CreateProduct;

namespace NsTech.Tests.Integration;

public class ValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ValidationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Login to get token
        var loginResponse = await client.PostAsync("/auth/token?username=admin&password=admin", null);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        string token = loginData!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var command = new CreateProductCommand(Guid.Empty, "", -10, -5);

        // Act
        var response = await client.PostAsJsonAsync("/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Validation Error");
        problemDetails.Status.Should().Be(400);
        
        // Check if errors are present in Extensions
        problemDetails.Extensions.Should().ContainKey("errors");
    }

    private record LoginResult(string Token);
}
