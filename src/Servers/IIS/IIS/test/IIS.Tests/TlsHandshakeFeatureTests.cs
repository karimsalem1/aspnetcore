// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Server.IIS.FunctionalTests;

[SkipIfHostableWebCoreNotAvailable]
[MinimumOSVersion(OperatingSystems.Windows, WindowsVersions.Win8, SkipReason = "https://github.com/aspnet/IISIntegration/issues/866")]
[SkipOnHelix("Unsupported queue", Queues = "Windows.Amd64.VS2022.Pre.Open;")]
public class TlsHandshakeFeatureTests : StrictTestServerTests
{
    [ConditionalFact]
    public async Task SetsTlsHandshakeFeature()
    {
        ITlsHandshakeFeature tlsHandshakeFeature = null;
        using (var testServer = await TestServer.CreateHttps(ctx =>
        {
            tlsHandshakeFeature = ctx.Features.Get<ITlsHandshakeFeature>();
            return Task.CompletedTask;
        }, LoggerFactory))
        {
            await testServer.HttpClient.GetStringAsync("/");
        }

        Assert.NotNull(tlsHandshakeFeature);

        var protocol = tlsHandshakeFeature.Protocol;
        Assert.True(protocol > SslProtocols.None, "Protocol: " + protocol);
        Assert.True(Enum.IsDefined(typeof(SslProtocols), protocol), "Defined: " + protocol); // Mapping is required, make sure it's current

        var cipherAlgorithm = tlsHandshakeFeature.CipherAlgorithm;
        Assert.True(cipherAlgorithm > CipherAlgorithmType.Null, "Cipher: " + cipherAlgorithm);

        var cipherStrength = tlsHandshakeFeature.CipherStrength;
        Assert.True(cipherStrength > 0, "CipherStrength: " + cipherStrength);

        var hashAlgorithm = tlsHandshakeFeature.HashAlgorithm;
        Assert.True(hashAlgorithm >= HashAlgorithmType.None, "HashAlgorithm: " + hashAlgorithm);

        var hashStrength = tlsHandshakeFeature.HashStrength;
        Assert.True(hashStrength >= 0, "HashStrength: " + hashStrength); // May be 0 for some algorithms

        var keyExchangeAlgorithm = tlsHandshakeFeature.KeyExchangeAlgorithm;
        Assert.True(keyExchangeAlgorithm >= ExchangeAlgorithmType.None, "KeyExchangeAlgorithm: " + keyExchangeAlgorithm);

        var keyExchangeStrength = tlsHandshakeFeature.KeyExchangeStrength;
        Assert.True(keyExchangeStrength >= 0, "KeyExchangeStrength: " + keyExchangeStrength);

        if (Environment.OSVersion.Version > new Version(10, 0, 19043, 0))
        {
            var hostName = tlsHandshakeFeature.HostName;
            Assert.Equal("localhost", hostName);
        }
    }
}
