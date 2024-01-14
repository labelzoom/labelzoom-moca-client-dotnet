
using System.Data;

namespace LabelZoom.MocaClient.Tests;

public class HttpMocaConnectionTests
{
    [Fact]
    public async Task TestHttpMocaConnectionAsync()
    {
        var url = Environment.GetEnvironmentVariable("MOCA_URL") ?? string.Empty;
        Assert.NotEmpty(url);
        var user = Environment.GetEnvironmentVariable("MOCA_USER") ?? string.Empty;
        Assert.NotEmpty(user);
        var password = Environment.GetEnvironmentVariable("MOCA_PASS") ?? string.Empty;
        Assert.NotEmpty(password);
        using (HttpMocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaResponse res = await conn.Execute("publish data where message = 'Hello World!'");
            Assert.Equal(0, res.StatusCode);
            Assert.NotNull(res.ResponseData);
            using (DataTable  dt = res.ResponseData)
            {
                Assert.Equal(1, dt.Rows.Count);
                Assert.Equal("Hello World!", dt.Rows[0].Field<string>("message"));
            }
        }
    }
}