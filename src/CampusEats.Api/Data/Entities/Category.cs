using System;

namespace CampusEats.Api.Data.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
