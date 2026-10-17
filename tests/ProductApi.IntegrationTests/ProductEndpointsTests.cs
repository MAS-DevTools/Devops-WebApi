using System.Net;
using System.Net.Http.Json;
using ProductApi.Products;

namespace ProductApi.IntegrationTests;

public class ProductEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_DatabaseReachable_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Root_ReturnsVersionInfo()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("version", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Create_ValidProduct_CanBeRetrieved()
    {
        var createResponse = await _client.PostAsJsonAsync("/products", new ProductRequest("Coffee", 2.50m));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);

        var fetched = await _client.GetFromJsonAsync<ProductResponse>($"/products/{created.Id}");

        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Create_InvalidProduct_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/products", new ProductRequest("", 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingProduct_SavesChanges()
    {
        var created = await CreateProduct("Tea", 1.80m);

        var updateResponse = await _client.PutAsJsonAsync($"/products/{created.Id}", new ProductRequest("Green tea", 2.10m));
        var fetched = await _client.GetFromJsonAsync<ProductResponse>($"/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(new ProductResponse(created.Id, "Green tea", 2.10m), fetched);
    }

    [Fact]
    public async Task Delete_ExistingProduct_RemovesIt()
    {
        var created = await CreateProduct("Cake", 3.25m);

        var deleteResponse = await _client.DeleteAsync($"/products/{created.Id}");
        var getResponse = await _client.GetAsync($"/products/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ProductResponse> CreateProduct(string name, decimal price)
    {
        var response = await _client.PostAsJsonAsync("/products", new ProductRequest(name, price));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }
}
