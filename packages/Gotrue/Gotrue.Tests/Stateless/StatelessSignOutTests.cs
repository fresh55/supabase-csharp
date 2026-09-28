#region

using System.Threading.Tasks;
using FluentAssertions;
using Gotrue.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.StatelessClient;

#endregion

namespace Gotrue.Tests.Stateless;

/// <summary>
///     Stateless sign-out sends the requested scope to <c>/logout</c>, global by default, and throws when the request fails.
/// </summary>
[TestClass]
[TestCategory("Contract")]
public class StatelessSignOutTests
{
    private MockGotrueServer server = null!;

    [TestInitialize]
    public void TestInitialize() => this.server = new MockGotrueServer();

    [TestCleanup]
    public void TestCleanup() => this.server.Dispose();

    [TestMethod]
    [DataRow(SignOutScope.Global, "global")]
    [DataRow(SignOutScope.Local, "local")]
    [DataRow(SignOutScope.Others, "others")]
    public async Task SignOut_ShouldSendTheRequestedScope(SignOutScope scope, string expected)
    {
        this.StubLogout(200);
        await new StatelessClient().SignOut("user-access-token", this.Options(), scope);
        this.server.VerifySingleReceivedRequest().WithQueryParam("scope", expected);
    }

    [TestMethod]
    public async Task SignOut_ShouldSendGlobalScope_GivenNoScope()
    {
        this.StubLogout(200);
        await new StatelessClient().SignOut("user-access-token", this.Options());
        this.server.VerifySingleReceivedRequest().WithQueryParam("scope", "global");
    }

    [TestMethod]
    public async Task SignOut_ShouldThrow_GivenErrorResponse()
    {
        this.StubLogout(401);
        var signOut = () => new StatelessClient().SignOut("user-access-token", this.Options(), SignOutScope.Local);
        await signOut.Should().ThrowAsync<GotrueException>();
    }

    private void StubLogout(int statusCode) =>
        this.server.Given(Request.Create().WithPath("/logout").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(statusCode));

    private StatelessClientOptions Options() => new() { Url = this.server.Url };
}
