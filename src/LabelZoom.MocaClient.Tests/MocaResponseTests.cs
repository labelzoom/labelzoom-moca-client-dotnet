using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelZoom.MocaClient.Tests;

public class MocaResponseTests
{
    private readonly string url;
    private readonly string user;
    private readonly string password;

    public MocaResponseTests()
    {
        url = Environment.GetEnvironmentVariable("MOCA_URL") ?? string.Empty;
        Assert.NotEmpty(url);
        user = Environment.GetEnvironmentVariable("MOCA_USER") ?? string.Empty;
        Assert.NotEmpty(user);
        password = Environment.GetEnvironmentVariable("MOCA_PASS") ?? string.Empty;
        Assert.NotEmpty(password);
    }

    [Fact]
    public async Task TestDataTypes()
    {
        using (MocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaResponse res = await conn.Execute("publish data" +
                " where intcol = 82332" +
                "   and strcol = 'IOJWExriojomwe'" +
                "   and boolcol = true" +
                "   and floatcol = 819233.389901782");
            Assert.Equal(0, res.StatusCode);
            Assert.NotNull(res.ResponseData);
            using (DataTable dt = res.ResponseData)
            {
                Assert.Equal(1, dt.Rows.Count);
                Assert.Equal(82332, dt.Rows[0].Field<int>("intcol"));
                Assert.Equal("IOJWExriojomwe", dt.Rows[0].Field<string>("strcol"));
                Assert.True(dt.Rows[0].Field<bool>("boolcol"));
                Assert.Equal(819233.389901782, dt.Rows[0].Field<double>("floatcol"));
            }
        }
    }
}
