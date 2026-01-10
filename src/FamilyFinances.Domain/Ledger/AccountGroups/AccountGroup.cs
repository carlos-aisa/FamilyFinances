namespace FamilyFinances.Domain.Ledger.AccountGroups
{
    public sealed class AccountGroup
    {
        public AccountGroupId Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Optional description
        public string? Description { get; private set; }

        // Convenience for indexing/search (case-insensitive uniqueness enforced in Infra)
        public string NormalizedName { get; private set; } = string.Empty;

        private AccountGroup() { } // EF

        private AccountGroup(AccountGroupId id, string name, string? description)
        {
            Id = id;
            SetName(name);
            SetDescription(description);
        }

        public static AccountGroup Create(string name, string? description)
            => new(AccountGroupId.New(), name, description);

        public void Rename(string name) => SetName(name);

        public void UpdateDescription(string? description) => SetDescription(description);

        private void SetName(string name)
        {
            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length == 0)
                throw new ArgumentException("Account group name cannot be empty.", nameof(name));

            Name = trimmed;
            NormalizedName = Normalize(trimmed);
        }

        private void SetDescription(string? description)
        {
            var trimmed = description?.Trim();
            Description = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        public static string Normalize(string value)
            => value.Trim().ToUpperInvariant();
    }
}
