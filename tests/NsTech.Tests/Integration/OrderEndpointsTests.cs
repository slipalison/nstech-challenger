using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NsTech.Application.Features.Orders.CreateOrder;
using NsTech.Application.Features.Orders.GetOrder;
using NsTech.Application.Features.Products.CreateProduct;

namespace NsTech.Tests.Integration;

public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var response = await _client.PostAsync("/auth/token?username=admin&password=admin", null);
        var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return content!.Token;
    }

    private record TokenResponse(string Token);

    [Fact]
    public async Task CreateOrder_And_Workflow_Should_Succeed()
    {
        // 1. Get Token
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Product
        var productId = Guid.NewGuid();
        var createProduct = new CreateProductCommand(productId, "Test Integration Product", 150.50m, 50);
        await _client.PostAsJsonAsync("/products", createProduct);

        // 3. Create Order
        var createOrder = new CreateOrderCommand("cust-integration", "BRL", [new CreateOrderItemRequest(productId, 2)]);
        var response = await _client.PostAsJsonAsync("/orders", createOrder);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var orderIdObj = await response.Content.ReadFromJsonAsync<IdResponse>();
        var orderId = orderIdObj!.Id;

        // 4. Get Order
        var getOrderResponse = await _client.GetAsync($"/orders/{orderId}");
        getOrderResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var order = await getOrderResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        order!.Status.ToString().Should().Be("Placed");

        // 5. Confirm Order
        var confirmResponse = await _client.PostAsync($"/orders/{orderId}/confirm", null);
        confirmResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 6. Cancel Order
        var cancelResponse = await _client.PostAsync($"/orders/{orderId}/cancel", null);
        cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // 7. List Orders
        var listResponse = await _client.GetAsync("/orders");
        listResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    private record IdResponse(Guid Id);
}
