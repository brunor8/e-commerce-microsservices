using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Sku Sku { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public ProductStatus Status { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { }

    public Product(
        string name,
        string sku,
        Money price,
        int stockQuantity,
        Guid categoryId,
        string? description = null)
    {
        Validate(name, price, stockQuantity, categoryId);

        Id = Guid.NewGuid();
        Name = name;
        Sku = new Sku(sku);
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        Description = description;
        Status = stockQuantity > 0 ? ProductStatus.Active : ProductStatus.OutOfStock;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, Money price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId is required.");

        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to add must be greater than zero.");

        StockQuantity += quantity;
        UpdateStatus();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity to remove must be greater than zero.");

        if (quantity > StockQuantity)
            throw new DomainException($"Insufficient stock. Available: {StockQuantity}, requested: {quantity}.");

        StockQuantity -= quantity;
        UpdateStatus();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Inactive)
            throw new DomainException("Product is already inactive.");

        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateStatus()
    {
        if (Status == ProductStatus.Inactive) return;
        Status = StockQuantity > 0 ? ProductStatus.Active : ProductStatus.OutOfStock;
    }

    private static void Validate(string name, Money price, int stockQuantity, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        if (name.Length > 200)
            throw new DomainException("Product name cannot exceed 200 characters.");

        if (price is null)
            throw new DomainException("Price is required.");

        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");

        if (categoryId == Guid.Empty)
            throw new DomainException("CategoryId is required.");
    }
}