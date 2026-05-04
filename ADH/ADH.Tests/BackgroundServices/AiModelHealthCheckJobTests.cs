using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ADH.Application.Interfaces;
using ADH.Infrastructure.BackgroundServices;
using ADH.Infrastructure.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace ADH.Tests.BackgroundServices;

public class AiModelHealthCheckJobTests
{
    [Fact]
    public async Task CheckOllamaHealthAsync_WhenUnreachable_TriggersFailover()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var aiManagerMock = new Mock<IAiProviderManager>();
        var hubContextMock = new Mock<IHubContext<ChatHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        aiManagerMock.SetupProperty(m => m.IsOllamaHealthy, true);
        hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
        scopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IAiProviderManager))).Returns(aiManagerMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IHubContext<ChatHub>))).Returns(hubContextMock.Object);

        var loggerMock = new Mock<IAppLogger<AiModelHealthCheckJob>>();
        
        // Mock HttpClient
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient("OllamaClient")).Returns(new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") });

        var job = new AiModelHealthCheckJob(serviceProviderMock.Object, loggerMock.Object, httpClientFactoryMock.Object);

        // Act
        // Using reflection to call private method for unit testing
        var method = typeof(AiModelHealthCheckJob).GetMethod("CheckOllamaHealthAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(job, null)!;

        // Assert
        aiManagerMock.Object.IsOllamaHealthy.Should().BeFalse();
        aiManagerMock.Verify(m => m.SetProvider("OpenAI"), Times.Once);
    }
}
