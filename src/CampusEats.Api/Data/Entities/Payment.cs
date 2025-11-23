using System;

namespace CampusEats.Api.Data.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
