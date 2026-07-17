using Catalog.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Products.Commands.AddStock;

public record AddStockCommand(
        Guid ProductId,
        int Quantity
    ) : IRequest;