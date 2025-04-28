using System.Data;
using System.Net.Http.Headers;

namespace LabelZoom.MocaClient.Tests;

public class HttpMocaConnectionTests
{
    private readonly string url;
    private readonly string user;
    private readonly string password;

    public HttpMocaConnectionTests()
    {
        url = Environment.GetEnvironmentVariable("MOCA_URL") ?? string.Empty;
        Assert.False(string.IsNullOrEmpty(url), "Missing MOCA_URL environment variable");
        user = Environment.GetEnvironmentVariable("MOCA_USER") ?? string.Empty;
        Assert.False(string.IsNullOrEmpty(user), "Missing MOCA_USER environment variable");
        password = Environment.GetEnvironmentVariable("MOCA_PASS") ?? string.Empty;
        Assert.False(string.IsNullOrEmpty(password), "Missing MOCA_PASS environment variable");
    }

    [Fact]
    public async Task WithValidLogin_TestPublishDataWithOneRow()
    {
        using (HttpMocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaResponse res = await conn.Execute("publish data where message = 'Hello World!'");
            Assert.Equal(0, res.StatusCode);
            Assert.NotNull(res.ResponseData);
            using (DataTable dt = res.ResponseData)
            {
                Assert.Equal(1, dt.Rows.Count);
                Assert.Equal("Hello World!", dt.Rows[0].Field<string>("message"));
            }
        }
    }

    [Fact]
    public async Task WithValidLogin_TestPublishDataWithMultipleRow()
    {
        using (HttpMocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaResponse res = await conn.Execute("publish data where a = 1 and b = 2 & publish data where a = 3 and b = 4");
            Assert.Equal(0, res.StatusCode);
            Assert.NotNull(res.ResponseData);
            using (DataTable dt = res.ResponseData)
            {
                Assert.Equal(2, dt.Rows.Count);
                Assert.Equal(1, dt.Rows[0].Field<int>("a"));
                Assert.Equal(2, dt.Rows[0].Field<int>("b"));
                Assert.Equal(3, dt.Rows[1].Field<int>("a"));
                Assert.Equal(4, dt.Rows[1].Field<int>("b"));
            }
        }
    }

    [Fact]
    public async Task TestRawHttp()
    {
        string command = "<moca-request><query>get encryption information</query></moca-request>";
        using (HttpClient client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url)))
            using (var content = new StringContent(command))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/moca-xml");
                request.Content = content;
                using (var response = await client.SendAsync(request))
                {
                    Assert.True(response.IsSuccessStatusCode);
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}