## Topilgan muammolar

| # | Fayl | Muammo | Tuzatish |
|---|------|--------|----------|
| 1 | `HelloController.cs` (ctor) | `helloService` null bo'lsa — `NullReferenceException` `Get()` da sodir bo'ladi | `ArgumentNullException.ThrowIfNull(helloService)` qo'sh |
| 2 | `HelloController.cs:Get()` | Service `null` qaytarsa — `Ok(null)` → 200 OK, bu noto'g'ri | Null tekshiruvi + `NotFound()` yoki `Problem()` |
| 3 | `HelloController.cs:catch` | `ex.Message` produksiyada ichki ma'lumot chiqaradi | Production-da xabarni yashirish kerak |

---

## Testlar

```csharp
// HelloServiceTests.cs
public class HelloServiceTests
{
    private readonly HelloService _sut = new();

    [Fact]
    public async Task GetGreetingAsync_ReturnsHelloWorld()
    {
        var result = await _sut.GetGreetingAsync();
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public async Task GetGreetingAsync_ReturnsNotNull()
    {
        var result = await _sut.GetGreetingAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetGreetingAsync_ReturnsNonEmptyString()
    {
        var result = await _sut.GetGreetingAsync();
        Assert.NotEmpty(result);
    }
}
```

```csharp
// HelloControllerTests.cs
public class HelloControllerTests
{
    private readonly Mock<IHelloService> _mockService = new();
    private readonly HelloController _sut;

    public HelloControllerTests()
    {
        _sut = new HelloController(_mockService.Object);
    }

    // ✅ Happy path
    [Fact]
    public async Task Get_ReturnsOk_WithHelloWorld()
    {
        _mockService.Setup(s => s.GetGreetingAsync()).ReturnsAsync("Hello World");

        var result = await _sut.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal("Hello World", ok.Value);
    }

    // ❌ Bug #1: constructor null guard yo'q — bu test FAIL bo'ladi
    [Fact]
    public void Constructor_NullService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HelloController(null!));
    }

    // ❌ Bug #2: service null qaytarsa — 200 OK bilan null keladi
    [Fact]
    public async Task Get_ServiceReturnsNull_ShouldNotReturn200()
    {
        _mockService.Setup(s => s.GetGreetingAsync()).ReturnsAsync((string?)null);

        var result = await _sut.Get();

        // Hozirgi kod OkObjectResult qaytaradi — bu noto'g'ri
        Assert.IsNotType<OkObjectResult>(result); // ← FAIL qiladi (bug bor)
    }

    // ✅ Exception → 500
    [Fact]
    public async Task Get_ServiceThrows_Returns500()
    {
        _mockService.Setup(s => s.GetGreetingAsync())
                    .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.Get();

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
        Assert.Contains("DB error", obj.Value?.ToString());
    }

    // Edge: bo'sh string
    [Fact]
    public async Task Get_ServiceReturnsEmpty_Returns200WithEmpty()
    {
        _mockService.Setup(s => s.GetGreetingAsync()).ReturnsAsync(string.Empty);

        var result = await _sut.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(string.Empty, ok.Value);
    }

    // Edge: service bir necha marta chaqirilmasin
    [Fact]
    public async Task Get_CallsServiceExactlyOnce()
    {
        _mockService.Setup(s => s.GetGreetingAsync()).ReturnsAsync("Hello World");

        await _sut.Get();

        _mockService.Verify(s => s.GetGreetingAsync(), Times.Once);
    }
}
```

---

**Tuzatish taklifi (Bug #1 va #2):**

```csharp
public HelloController(IHelloService helloService)
{
    ArgumentNullException.ThrowIfNull(helloService); // Bug #1
    _helloService = helloService;
}

public async Task<IActionResult> Get()
{
    try
    {
        var result = await _helloService.GetGreetingAsync();
        if (result is null) return Problem("Service returned null", statusCode: 500); // Bug #2
        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Internal server error"); // Bug #3: ex.Message yashirildi
    }
}
```