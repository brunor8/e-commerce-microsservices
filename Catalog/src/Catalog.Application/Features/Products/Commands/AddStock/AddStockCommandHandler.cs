using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;

namespace Catalog.Application.Features.Products.Commands.AddStock;

public class AddStockCommandHandler : IRequestHandler<AddStockCommand>
{
    private readonly IProductRepository _productRepository;

    public AddStockCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw NotFoundException.For(nameof(Product), request.ProductId);

        product.AddStock(request.Quantity);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }

}
