using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;

namespace Catalog.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)

    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Category), request.Id);

        var productsInCategory = await _productRepository.GetByCategoryAsync(request.Id, cancellationToken);
        if (productsInCategory.Any())
            throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it has {productsInCategory.Count()} product(s) associated with it.");

        await _categoryRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
