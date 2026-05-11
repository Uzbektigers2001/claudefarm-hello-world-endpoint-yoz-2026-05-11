## Topilgan muammolar

| # | Qator | Muammo | Tuzatish |
|---|-------|--------|----------|
| 1 | `[Route("api/[controller]")]` | Controller-darajadagi route bekor qilingan — `[HttpGet("/api/hello")]` dagi leading `/` uni override qiladi. Route attribute ortiqcha va chalkash. | Controller route'ni olib tashlash yoki `[HttpGet("")]` ishlatish |
| 2 | Constructor | `logger` null bo'lsa — `LogInformation` chaqirilganda `NullReferenceException` | `ArgumentNullException.ThrowIfNull(logger)` qo'shish |
| 3 | `await Task.FromResult(...)` | Real async operatsiya yo'q — `Task.FromResult` sinxron bajariladi, `async/await` overhead faqat qo'shadi | `return Ok("Hello World")` + `Task` o'chirish |

---

## Testlar

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using HelloWorldApi.Controllers;

namespace HelloWorldApi.Tests;

public class HelloWorldControllerTests
{
    private readonly Mock<ILogger<HelloWorldController>> _loggerMock;
    private readonly HelloWorldController _controller;

    public HelloWorldControllerTests()
    {
        _loggerMock = new Mock<ILogger<HelloWorldController>>();
        _controller = new HelloWorldController(_loggerMock.Object);
    }

    // ── Happy path ──────────────────────────────────────────────

    [Fact]
    public async Task GetHello_Returns200Ok()
    {
        var result = await _controller.GetHello();

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ((OkObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task GetHello_ReturnsExactString_HelloWorld()
    {
        var result = await _controller.GetHello() as OkObjectResult;

        Assert.Equal("Hello World", result!.Value);
    }

    [Fact]
    public async Task GetHello_LogsInformation_Once()
    {
        await _controller.GetHello();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("GET /api/hello")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Edge cases ──────────────────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // BUG #2: hozirgi kodda bu test FAIL bo'ladi — null check yo'q
        Assert.Throws<ArgumentNullException>(() =>
            new HelloWorldController(null!));
    }

    [Fact]
    public async Task GetHello_ResponseValue_IsNotNullOrEmpty()
    {
        var result = await _controller.GetHello() as OkObjectResult;

        Assert.NotNull(result!.Value);
        Assert.NotEmpty(result.Value!.ToString()!);
    }

    [Fact]
    public async Task GetHello_ResponseValue_IsString()
    {
        var result = await _controller.GetHello() as OkObjectResult;

        Assert.IsType<string>(result!.Value);
    }

    [Fact]
    public async Task GetHello_CalledMultipleTimes_AlwaysReturnsHelloWorld()
    {
        for (int i = 0; i < 5; i++)
        {
            var result = await _controller.GetHello() as OkObjectResult;
            Assert.Equal("Hello World", result!.Value);
        }
    }

    // ── Exception handling ──────────────────────────────────────

    [Fact]
    public async Task GetHello_WhenLoggerThrows_Returns500()
    {
        _loggerMock
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Throws(new InvalidOperationException("Logger failed"));

        var result = await _controller.GetHello();

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Internal server error", statusResult.Value);
    }

    [Fact]
    public async Task GetHello_WhenException_LogsError()
    {
        _loggerMock
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Throws(new Exception("fail"));

        await _controller.GetHello();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

**Kerakli paketlar** (`*.csproj`):
```xml
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
```

> ⚠️ `Constructor_NullLogger_ThrowsArgumentNullException` testi hozirgi kodda **FAIL** bo'ladi — fix uchun constructorga `ArgumentNullException.ThrowIfNull(logger)` qo'shish kerak.