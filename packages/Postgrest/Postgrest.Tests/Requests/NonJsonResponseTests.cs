using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Exceptions;
using Supabase.Postgrest.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Postgrest.Tests.Requests;

/// <summary>
///     A 2xx body that isn't JSON throws a <see cref="PostgrestException" /> carrying the body and status.
/// </summary>
[TestClass]
[TestCategory("Contract")]
public class NonJsonResponseTests
{
    private const string ProxyErrorPage = "<html><body>502 Bad Gateway</body></html>";

    private WireMockServer server = null!;
    private Client client = null!;

    [Table("todos")]
    private class Todo : BaseModel
    {
        [PrimaryKey("id")] public int Id { get; set; }
        [Column("name")] public string? Name { get; set; }
    }

    [TestInitialize]
    public void SetUp()
    {
        this.server = WireMockServer.Start();
        this.client = new Client(this.server.Url!, new ClientOptions());
    }

    [TestCleanup]
    public void TearDown() => this.server.Stop();

    [TestMethod]
    public async Task Get_ShouldThrowPostgrestException_GivenNonJsonBody()
    {
        this.MockProxyErrorPage("/todos");
        var act = () => this.client.Table<Todo>().Get();
        await act.Should().ThrowAsync<PostgrestException>()
            .Where(exception => exception.Content == ProxyErrorPage && exception.StatusCode == 200,
                "postgrest-js returns a non-JSON 2xx body as the error (#260)");
    }

    [TestMethod]
    public async Task Rpc_ShouldThrowPostgrestException_GivenNonJsonBody()
    {
        this.MockProxyErrorPage("/rpc/echo");
        var act = () => this.client.Rpc<int>("echo");
        await act.Should().ThrowAsync<PostgrestException>()
            .Where(exception => exception.Content == ProxyErrorPage && exception.StatusCode == 200,
                "postgrest-js handles rpc responses the same way (#260)");
    }

    private void MockProxyErrorPage(string path) =>
        this.server.Given(Request.Create().WithPath(path))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ProxyErrorPage));
}
