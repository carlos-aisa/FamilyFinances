namespace FamilyFinances.Domain.Common
{
    public static class NameNormalizer
    {
        public static string Normalize(string value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
