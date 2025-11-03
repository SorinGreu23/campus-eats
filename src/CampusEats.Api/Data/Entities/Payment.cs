using System;

namespace CampusEats.Api.Data.Entities;

public class Payment : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
}
