using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;