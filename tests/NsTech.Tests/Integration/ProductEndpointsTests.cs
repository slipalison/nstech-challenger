using System.Net.Http.Json;
using FluentAssertions;
using NsTech.Application.Features.Products.CreateProduct;
using NsTech.Application.Features.Products.GetProducts;

namespace NsTech.Tests.Integration;

public class ProductEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var response = await _client.PostAsync("/auth/token?username=admin&password=admin", null);
        var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return content!.Token;
    }

    private record TokenResponse(string Token);

    [Fact]
    public async Task Create_And_List_Products_Should_Succeed()
    {
        // 1. Get Token
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Product
        var productId = Guid.NewGuid();
        var createProduct = new CreateProductCommand(productId, "Integration Product 2", 200, 10);
        var response = await _client.PostAsJsonAsync("/products", createProduct);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 3. List Products
        var listResponse = await _client.GetAsync("/products");
        listResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var products = await listResponse.Content.ReadFromJsonAsync<IEnumerable<ProductResponse>>();
        products.Should().NotBeEmpty();
    }
}
