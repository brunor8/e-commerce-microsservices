using Catalog.Application.DTOs;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductCommandHandler(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw NotFoundException.For(nameof(Category), request.CategoryId);

        var existingProduct = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);

        if (existingProduct is not null) throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists");

        var price = new Money(request.Price, request.Currency);

        var product = new Product(
            request.Name,
            request.Sku,
            price,
            request.StockQuantity,
            request.CategoryId,
            request.Description
            );

        await _productRepository.AddAsync(product, cancellationToken);

        return MapToDto(product, category.Name);
    }

    private static ProductDto MapToDto(Product product, string categoryName) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Sku = product.Sku.Value,
        Price = product.Price.Amount,
        Currency = product.Price.Currency,
        StockQuantity = product.StockQuantity,
        Status = product.Status.ToString(),
        CategoryId = product.CategoryId,
        CategoryName = categoryName,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
