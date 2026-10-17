namespace ProductApi.Products;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

public sealed record ProductRequest(string? Name, decimal Price);

public sealed record ProductResponse(int Id, string Name, decimal Price)
{
    public static ProductResponse From(Product product) => new(product.Id, product.Name, product.Price);
}
