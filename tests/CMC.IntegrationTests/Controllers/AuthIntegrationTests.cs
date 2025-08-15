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
    // Don't follow redirects automatically so we can test redirect behavior
    _client.Timeout = TimeSpan.FromSeconds(30);
  }[Fact]
  public async Task Get_Login_ReturnsSuccessAndCorrectContentType() {
    // Act
    var response = await _client.GetAsync("/login");

    // Assert
    response.EnsureSuccessStatusCode();
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Check Content-Type header
    response.Content.Headers.ContentType
      ?.ToString().Should().StartWith("text/html");

    var content = await response.Content.ReadAsStringAsync();
    // Check for the basic HTML structure
    content.Should().Contain("CMC");
    content.Should().Contain("<!DOCTYPE html>");
    content.Should().Contain("<title>CMC</title>");
  }[Fact]
  public async Task Get_Register_ReturnsSuccessAndCorrectContentType() {
    // Act
    var response = await _client.GetAsync("/register");

    // Assert
    response.EnsureSuccessStatusCode();
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Check Content-Type header
    response.Content.Headers.ContentType
      ?.ToString().Should().StartWith("text/html");

    var content = await response.Content.ReadAsStringAsync();
    // Check for the basic HTML structure
    content.Should().Contain("CMC");
    content.Should().Contain("<!DOCTYPE html>");
    content.Should().Contain("<title>CMC</title>");
  }[Fact]
  public async Task Get_Dashboard_WithoutAuth_ShouldReturnOKButWithoutUserData() {
    // Note: In a Blazor Server app, authorization is handled client-side
    // The initial response will be OK, but the Blazor component will handle auth

    // Act
    var response = await _client.GetAsync("/dashboard");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("CMC");
  }[Fact]
  public async Task Post_LoginAPI_WithValidCredentials_ShouldReturnSuccess() {
    // Arrange
    var loginRequest = new {
      Email = "test@example.com",
      Password = "password123"
    };

    var json = System.Text.Json.JsonSerializer.Serialize(loginRequest);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync("/api/auth/login", content);

    // Assert
    // The API should return OK for valid credentials
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var responseContent = await response.Content.ReadAsStringAsync();
    responseContent.Should().Contain("Login successful");
  }[Fact]
  public async Task Post_LoginAPI_WithInvalidCredentials_ShouldReturnUnauthorized() {
    // Arrange
    var loginRequest = new {
      Email = "invalid@example.com",
      Password = "wrongpassword"
    };

    var json = System.Text.Json.JsonSerializer.Serialize(loginRequest);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync("/api/auth/login", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }[Fact]
  public async Task Get_LogoutAPI_ShouldRedirectToLogin() {
    // Create a client that doesn't follow redirects
    var clientNoRedirect = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions {
      AllowAutoRedirect = false
    });

    // Act
    var response = await clientNoRedirect.GetAsync("/api/auth/logout");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    response.Headers.Location
      ?.ToString().Should().Be("/login");
  }
}
