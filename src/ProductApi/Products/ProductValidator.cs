namespace ProductApi.Products;

public static class ProductValidator
{
    public const int NameMaxLength = 100;
    public const decimal MaxPrice = 100_000m;

    /// <summary>Returns validation errors per field; an empty dictionary means the request is valid.</summary>
    public static Dictionary<string, string[]> Validate(ProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Name is required."];
        }
        else if (request.Name.Trim().Length > NameMaxLength)
        {
            errors[nameof(request.Name)] = [$"Name can be at most {NameMaxLength} characters."];
        }

        if (request.Price <= 0)
        {
            errors[nameof(request.Price)] = ["Price must be greater than 0."];
        }
        else if (request.Price > MaxPrice)
        {
            errors[nameof(request.Price)] = [$"Price can be at most {MaxPrice}."];
        }

        return errors;
    }
}
