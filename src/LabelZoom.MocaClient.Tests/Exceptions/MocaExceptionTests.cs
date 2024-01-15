﻿using LabelZoom.MocaClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelZoom.MocaClient.Tests.Exceptions;

public class MocaExceptionTests
{
    private readonly string url;
    private readonly string user;
    private readonly string password;

    public MocaExceptionTests()
    {
        url = Environment.GetEnvironmentVariable("MOCA_URL") ?? string.Empty;
        Assert.NotEmpty(url);
        user = Environment.GetEnvironmentVariable("MOCA_USER") ?? string.Empty;
        Assert.NotEmpty(user);
        password = Environment.GetEnvironmentVariable("MOCA_PASS") ?? string.Empty;
        Assert.NotEmpty(password);
    }

    [Fact]
    public async Task TestSyntaxError()
    {
        using (MocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaException ex = await Assert.ThrowsAsync<MocaException>(async () => await conn.Execute("publish data a = 1"));
            Assert.NotEmpty(ex.Message);
            Assert.Equal(505, ex.ErrorCode);
        }
    }

    [Fact]
    public async Task TestCommandNotFound()
    {
        using (MocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaException ex = await Assert.ThrowsAsync<CommandNotFoundException>(async () => await conn.Execute("this command doesnt exist"));
            Assert.NotEmpty(ex.Message);
            Assert.Equal(501, ex.ErrorCode);
        }
    }

    [Fact]
    public async Task TestNoRowsAffected()
    {
        using (MocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaException ex = await Assert.ThrowsAsync<NotFoundException>(async () => await conn.Execute("[select polcod, polvar, polval from poldat where 1 = 2]"));
            Assert.NotEmpty(ex.Message);
            Assert.Equal(510, ex.ErrorCode);
            // TODO: Attach result set to exception
        }
    }

    [Fact]
    public async Task TestInvalidColumn()
    {
        using (MocaConnection conn = new HttpMocaConnection(url))
        {
            await conn.Login(user, password);
            MocaException ex = await Assert.ThrowsAsync<MocaException>(async () => await conn.Execute("[select qw9io0ejoower from poldat where 1 = 2]"));
            Assert.NotEmpty(ex.Message);
            Assert.Equal(511, ex.ErrorCode);
        }
    }
}