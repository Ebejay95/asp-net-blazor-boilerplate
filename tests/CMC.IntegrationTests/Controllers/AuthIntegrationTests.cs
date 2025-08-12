using FluentAssertions;
using CMC.IntegrationTests.WebApplicationFactory;
using System.Net;
using Xunit;

namespace CMC.IntegrationTests.Controllers;

public class AuthIntegrationTests: IClassFixture<CustomWebApplicationFactory<Program>> {
  private readonly HttpClient _client;
  private readonly CustomWebApplicationFactory<Program> _factory;

  public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory) {
    _factory = factory;
    _client = _factory.CreateClient();
  }[Fact]
  public async Task Get_Login_ReturnsSuccessAndCorrectContentType() {
    // Act
    var response = await _client.GetAsync("/login");

    // Assert
    response.EnsureSuccessStatusCode();
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("Login");
  }[Fact]
  public async Task Get_Register_ReturnsSuccessAndCorrectContentType() {
    // Act
    var response = await _client.GetAsync("/register");

    // Assert
    response.EnsureSuccessStatusCode();
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("Register");
  }[Fact]
  public async Task Get_ProtectedPage_WithoutAuth_RedirectsToLogin() {
    // Act
    var response = await _client.GetAsync("/");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
  }
}
