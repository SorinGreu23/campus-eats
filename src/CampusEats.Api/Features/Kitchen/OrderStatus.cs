namespace CampusEats.Api.Features.Kitchen;

/// <summary>
/// Represents the possible states of an order in the kitchen workflow.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been placed but kitchen hasn't started working on it
    /// </summary>
    Pending,

    /// <summary>
    /// Kitchen is actively preparing the order
    /// </summary>
    Preparing,

    /// <summary>
    /// Order is prepared and ready for pickup
    /// </summary>
    Ready,

    /// <summary>
    /// Order has been completed and picked up
    /// </summary>
    Completed
}