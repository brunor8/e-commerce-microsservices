using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category() { }

    public Category(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Category name is required.");

        if (name.Length > 100) throw new DomainException("Category name cannot exceed 100 characters.");

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        if (!string.IsNullOrWhiteSpace(name)) 
            throw new DomainException("Category name is required.");

        Name = name;
        Description = description;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
