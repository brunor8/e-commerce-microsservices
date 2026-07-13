using Catalog.Application.DTOs;
using MediatR;

namespace Catalog.Application.Features.Products.Commands.CreateCategory;

public record CreateCategoryCommand(
        string Name,
        string? Description
    ) : IRequest<CategoryDto>;