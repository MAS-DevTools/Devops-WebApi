using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Products;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products");

        products.MapGet("/", GetAll);
        products.MapGet("/{id:int}", GetById);
        products.MapPost("/", Create);
        products.MapPut("/{id:int}", Update);
        products.MapDelete("/{id:int}", Delete);
    }

    private static async Task<IResult> GetAll(ProductDbContext db, CancellationToken cancellationToken)
    {
        var items = await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> GetById(int id, ProductDbContext db, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product is null ? Results.NotFound() : Results.Ok(ProductResponse.From(product));
    }

    private static async Task<IResult> Create(ProductRequest request, ProductDbContext db, CancellationToken cancellationToken)
    {
        var errors = ProductValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var product = new Product { Name = request.Name!.Trim(), Price = request.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/products/{product.Id}", ProductResponse.From(product));
    }

    private static async Task<IResult> Update(int id, ProductRequest request, ProductDbContext db, CancellationToken cancellationToken)
    {
        var errors = ProductValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return Results.NotFound();
        }

        product.Name = request.Name!.Trim();
        product.Price = request.Price;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(ProductResponse.From(product));
    }

    private static async Task<IResult> Delete(int id, ProductDbContext db, CancellationToken cancellationToken)
    {
        var deleted = await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);

        return deleted == 0 ? Results.NotFound() : Results.NoContent();
    }
}
