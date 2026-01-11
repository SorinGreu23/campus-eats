using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Inventory.Restock;

public record RestockInventoryRequest(
    Guid InventoryItemId,
    decimal Quantity,
    string? Reason
) : IRequest<IResult>;
