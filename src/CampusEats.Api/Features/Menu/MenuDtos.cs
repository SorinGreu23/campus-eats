namespace CampusEats.Api.Features.Menu;

/// <summary>
/// Allergen information - substances that may cause allergic reactions
/// </summary>
public record AllergenDto(Guid Id, string Name, string? Description, string? Icon);

/// <summary>
/// Dietary restriction/preference information (e.g., Vegetarian, Vegan, Carnivore)
/// </summary>
public record DietaryRestrictionDto(Guid Id, string Name, string? Description, string? Icon);
