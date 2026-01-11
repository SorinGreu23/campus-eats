using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Inventory.Use;

public record UseInventoryRequest(
    Guid InventoryItemId,
    decimal Quantity,
    string? Reason
) : IRequest<IResult>;
