#region

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue.Claims;

#endregion

namespace Gotrue.Tests.Claims;

/// <summary>
///     Which signing algorithms are verified locally: RS256 always, ES256 only where the platform has ECDsa.
/// </summary>
[TestClass]
[TestCategory("Unit")]
public class JwtVerificationTests
{
    [TestMethod]
    [DataRow("RS256", false, true)]
    [DataRow("ES256", true, true)]
    [DataRow("ES256", false, false)]
    [DataRow("HS256", true, false)]
    [DataRow(null, true, false)]
    public void CanVerify_ShouldMatchPlatformSupport(string? alg, bool ecdsaAvailable, bool expected) =>
        JwtVerification.CanVerify(alg, ecdsaAvailable).Should().Be(expected, "platforms without ECDsa, such as Unity, verify ES256 on the server (issue #481)");
}
