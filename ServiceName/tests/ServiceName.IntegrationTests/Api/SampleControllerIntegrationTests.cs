using FluentAssertions;
using ServiceName.Application.DTO;
using ServiceName.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace ServiceName.IntegrationTests.Api;

public class SampleControllerIntegrationTests
    : IClassFixture<ContainersFixture>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public SampleControllerIntegrationTests(
        ContainersFixture containers)
    {
        _factory = new ApiFactory(containers);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateClientWithRole(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private HttpClient CreateAnonymousClient()
        => _factory.CreateClient();

    [Fact]
    public async Task GetAll_ShouldReturnOk_ForReadUser()
    {
        var client = CreateClientWithRole("ReadUser");

        var response = await client.GetAsync("/api/v1/samples");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenNoAuth()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/v1/samples");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Add_ShouldReturnForbidden_ForReadUser()
    {
        var client = CreateClientWithRole("ReadUser");

        var dto = new AddSampleRequestDto
        {
            Name = "Test Sample"
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/samples", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Add_ShouldReturnOk_ForWriteUser()
    {
        var client = CreateClientWithRole("WriteUser");

        var dto = new AddSampleRequestDto
        {
            Name = "Test Sample"
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/samples", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbidden_ForWriteUser()
    {
        var client = CreateClientWithRole("WriteUser");

        var id = Guid.NewGuid();

        var response = await client.DeleteAsync(
            $"/api/v1/samples/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_ForAdmin()
    {
        var client = CreateClientWithRole("Admin");

        var id = Guid.NewGuid();

        var response = await client.DeleteAsync(
            $"/api/v1/samples/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}