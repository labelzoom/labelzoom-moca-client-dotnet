![LabelZoom Logo](docs/LabelZoom_Logo_f_400px.png)

# labelzoom-moca-client-dotnet

[![Build Status](https://github.com/labelzoom/labelzoom-moca-client-dotnet/actions/workflows/dotnet-build.yml/badge.svg?branch=main)](https://github.com/labelzoom/labelzoom-moca-client-dotnet/actions?query=branch%3Amain)
[![Release](https://img.shields.io/github/release/labelzoom/labelzoom-moca-client-dotnet.svg?style=flat-square)](https://github.com/labelzoom/labelzoom-moca-client-dotnet/releases)
[![codecov](https://codecov.io/gh/labelzoom/labelzoom-moca-client-dotnet/graph/badge.svg?token=XW4CQZTGBV)](https://codecov.io/gh/labelzoom/labelzoom-moca-client-dotnet)

MOCA client for .NET, sponsored by [LabelZoom](https://www.labelzoom.net).

## How To Use
See [tests](src/LabelZoom.MocaClient.Tests) for more examples.

```csharp
using (MocaConnection conn = new HttpMocaConnection(url))
{
    await conn.Login(user, password);
    MocaResponse res = await conn.Execute("publish data where message = 'Hello World!'");
    using (DataTable dt = res.ResponseData)
    {
        Console.WriteLine($"Message: {dt.Rows[0].Field<string>("message")}")
    }
}
```
