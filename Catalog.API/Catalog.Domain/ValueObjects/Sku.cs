using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

public sealed class Sku
{
    public string Value { get; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SKU is required.");

        if (value.Length > 50)
            throw new DomainException("SKU cannot exceed 50 characters.");

        // Formato: letras maiúsculas, números e hífen. Ex: PROD-001, CAT-ABC-123
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Z0-9\-]+$"))
            throw new DomainException("SKU must contain only uppercase letters, numbers and hyphens.");

        Value = value.Trim().ToUpper();
    }

    public override bool Equals(object? obj) =>
        obj is Sku other && Value == other.Value;

    public override int GetHashCode() => HashCode.Combine(Value);

    public override string ToString() => Value;

    // Permite comparar Sku com string diretamente
    public static implicit operator string(Sku sku) => sku.Value;
}