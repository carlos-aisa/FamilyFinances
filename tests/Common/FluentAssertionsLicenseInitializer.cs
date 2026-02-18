using System.Runtime.CompilerServices;
using FluentAssertions;

namespace FamilyFinances.Tests.Common;

internal static class FluentAssertionsLicenseInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        License.Accepted = true;
    }
}
