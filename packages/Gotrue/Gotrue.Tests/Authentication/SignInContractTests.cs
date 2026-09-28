#region

using System.Threading.Tasks;
using Gotrue.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using static Gotrue.Tests.TestUtils;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.StatelessClient;

#endregion

namespace Gotrue.Tests.Authentication;

/// <summary>
///     Password sign-in sends the captcha token in <c>gotrue_meta_security</c>.
/// </summary>
[TestClass]
[TestCategory("Contract")]
public class SignInContractTests
{
    private MockGotrueServer server = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        this.server = new MockGotrueServer();
        this.server.Given(Request.Create().WithPath("/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithHeader("Content-Type", "application/json").WithBody("{}"));
    }

    [TestCleanup]
    public void TestCleanup() => this.server.Dispose();

    [TestMethod]
    [DataRow(SignInType.Email, "captcha@example.com")]
    [DataRow(SignInType.Phone, "+15555550123")]
    public async Task SignInWithPassword_ShouldSendTheCaptchaToken(SignInType type, string identifier)
    {
        await TestClients.Against(this.server).SignInWithPassword(type, identifier, Password, new SignInWithPasswordOptions { CaptchaToken = "the-captcha" });
        this.server.VerifySingleReceivedRequest().WithNestedJsonBody("gotrue_meta_security", "captcha_token", "the-captcha");
    }

    [TestMethod]
    [DataRow(SignInType.Email, "captcha@example.com")]
    [DataRow(SignInType.Phone, "+15555550123")]
    public async Task StatelessSignIn_ShouldSendTheCaptchaToken(SignInType type, string identifier)
    {
        var options = new StatelessClientOptions { Url = this.server.Url };
        await new StatelessClient().SignIn(type, identifier, Password, options, new SignInWithPasswordOptions { CaptchaToken = "the-captcha" });
        this.server.VerifySingleReceivedRequest().WithNestedJsonBody("gotrue_meta_security", "captcha_token", "the-captcha");
    }
}
