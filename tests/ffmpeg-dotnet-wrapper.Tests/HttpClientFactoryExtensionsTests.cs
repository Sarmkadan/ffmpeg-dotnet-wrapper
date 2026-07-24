// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================
// Unit tests for HttpClientFactoryExtensions methods and HttpClientConfig properties.
// Tests extension methods for IServiceCollection that configure HTTP clients.
// ===================================================================

using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Integration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for <see cref="HttpClientFactoryExtensions"/> extension methods.
/// Tests service collection extensions for configuring HTTP clients and utilities.
/// </summary>
public class HttpClientFactoryExtensionsTests
{
    [Fact]
    public void AddFFmpegHttpClients_WithDefaultConfig_RegistersAllClients()
    {
        var services = new ServiceCollection();
        var result = services.AddFFmpegHttpClients();

        result.Should().NotBeNull().And.BeSameAs(services);

        services.Should().Contain(sd => sd.ServiceType == typeof(IHttpClientFactory) && sd.ImplementationFactory != null);
    }

    [Fact]
    public void AddFFmpegHttpClients_WithCustomConfig_AppliesCustomTimeouts()
    {
        var services = new ServiceCollection();
        var config = new HttpClientConfig
        {
            WebhookTimeoutSeconds = 15,
            ProbeTimeoutSeconds = 45,
            MediaTransferTimeoutMinutes = 15
        };

        var result = services.AddFFmpegHttpClients(c => c = config);

        result.Should().NotBeNull().And.BeSameAs(services);
        services.Should().Contain(sd => sd.ServiceType == typeof(IHttpClientFactory) && sd.ImplementationFactory != null);
    }

    [Fact]
    public void AddFFmpegHttpClients_WithNullConfig_UsesDefaults()
    {
        var services = new ServiceCollection();
        var result = services.AddFFmpegHttpClients(config: null);

        result.Should().NotBeNull().And.BeSameAs(services);
        services.Should().Contain(sd => sd.ServiceType == typeof(IHttpClientFactory) && sd.ImplementationFactory != null);
    }

    [Fact]
    public void AddCustomHttpClient_WithName_RegistersClient()
    {
        var services = new ServiceCollection();
        var builder = services.AddCustomHttpClient("test-client");

        builder.Should().NotBeNull();
        var client = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("test-client");
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddCustomHttpClient_WithBaseAddress_SetsBaseAddress()
    {
        var services = new ServiceCollection();
        services.AddCustomHttpClient("api-client", baseAddress: "https://api.example.com");

        var client = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("api-client");
        client.BaseAddress.Should().Be(new Uri("https://api.example.com"));
    }

    [Fact]
    public void AddCustomHttpClient_WithCustomTimeout_SetsTimeout()
    {
        var services = new ServiceCollection();
        services.AddCustomHttpClient("long-client", timeout: TimeSpan.FromSeconds(120));

        var client = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("long-client");
        client.Timeout.Should().Be(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void AddCustomHttpClient_WithHeaders_AddsHeaders()
    {
        var services = new ServiceCollection();
        var headers = new Dictionary<string, string> { { "X-API-Key", "test" } };
        services.AddCustomHttpClient("header-client", defaultHeaders: headers);

        var client = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("header-client");
        client.DefaultRequestHeaders.GetValues("X-API-Key").Should().Contain("test");
    }

    [Fact]
    public void AddCustomHttpClient_WithNullClientName_Throws()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddCustomHttpClient(null));
    }

    [Fact]
    public void HttpClientConfig_DefaultValues_AreCorrect()
    {
        var config = new HttpClientConfig();
        config.WebhookTimeoutSeconds.Should().Be(30);
        config.ProbeTimeoutSeconds.Should().Be(60);
        config.MediaTransferTimeoutMinutes.Should().Be(30);
        config.EnableRetries.Should().BeTrue();
        config.MaxRetryAttempts.Should().Be(3);
        config.InitialBackoffMs.Should().Be(100);
    }

    [Fact]
    public void HttpClientConfig_Properties_CanBeSet()
    {
        var config = new HttpClientConfig();
        config.WebhookTimeoutSeconds = 10;
        config.ProbeTimeoutSeconds = 20;
        config.MediaTransferTimeoutMinutes = 5;
        config.EnableRetries = false;
        config.MaxRetryAttempts = 1;
        config.InitialBackoffMs = 50;

        config.WebhookTimeoutSeconds.Should().Be(10);
        config.ProbeTimeoutSeconds.Should().Be(20);
        config.MediaTransferTimeoutMinutes.Should().Be(5);
        config.EnableRetries.Should().BeFalse();
        config.MaxRetryAttempts.Should().Be(1);
        config.InitialBackoffMs.Should().Be(50);
    }

    [Theory]
    [InlineData(408, true)] [InlineData(429, true)] [InlineData(500, true)]
    [InlineData(503, true)] [InlineData(504, true)]
    [InlineData(400, false)] [InlineData(401, false)] [InlineData(404, false)]
    public void HttpClientUtilities_IsTransientError_ClassifiesCorrectly(int code, bool expected) {
        HttpClientUtilities.IsTransientError(code).Should().Be(expected);
    }

    [Theory]
    [InlineData(400, true)] [InlineData(401, true)] [InlineData(404, true)]
    [InlineData(405, true)] [InlineData(408, false)] [InlineData(500, false)]
    public void HttpClientUtilities_IsPermanentError_ClassifiesCorrectly(int code, bool expected) {
        HttpClientUtilities.IsPermanentError(code).Should().Be(expected);
    }
}
