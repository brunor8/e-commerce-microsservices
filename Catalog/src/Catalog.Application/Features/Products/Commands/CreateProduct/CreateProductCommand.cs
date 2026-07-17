using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
       string Name,
       string Sku,
       decimal Price,
       string Currency,
       int StockQuantity,
       Guid CategoryId,
       string? Description
    ) : IRequest<ProductDto>;
