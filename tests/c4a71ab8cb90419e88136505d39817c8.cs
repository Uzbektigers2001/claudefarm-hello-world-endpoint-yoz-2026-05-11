## Topilgan muammolar

| # | Fayl | Muammo | Tuzatish |
|---|------|--------|----------|
| 1 | `HelloController.cs`, konstruktor | `_helloService` null bo'lsa `NullReferenceException` request paytida chiqadi | Konstruktorda `ArgumentNullException` throw qilish kerak |
| 2 | `HelloController.cs` | Service `null` qaytarsa `200 OK` + null body ketadi | Null tekshiruvi qo'shish yoki service kontraktida kafolat berish |

## Testlar

```csharp
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class HelloServiceTests
{
    private readonly HelloService _sut = new();

    [Fact]
    public async Task GetHelloMessageAsync_ReturnsHelloWorld()
    {
        var result = await _sut.GetHelloMessageAsync();
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public async Task GetHelloMessageAsync_ReturnsNonNullNonEmpty()
    {
        var result = await _sut.GetHelloMessageAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetHelloMessageAsync_CompletesSuccessfully()
    {
        var ex = await Record.ExceptionAsync(() => _sut.GetHelloMessageAsync());
        Assert.Null(ex);
    }
}

public class HelloControllerTests
{
    private readonly Mock<IHelloService> _mockService = new();
    private readonly HelloController _sut;

    public HelloControllerTests()
    {
        _sut = new HelloController(_mockService.Object);
    }

    // --- Happy path ---

    [Fact]
    public async Task GetHello_Returns200_WithHelloWorldMessage()
    {
        _mockService.Setup(s => s.GetHelloMessageAsync()).ReturnsAsync("Hello World");

        var result = await _sut.GetHello();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal("Hello World", ok.Value);
    }

    // --- Exception handling ---

    [Fact]
    public async Task GetHello_Returns500_WhenServiceThrows()
    {
        _mockService.Setup(s => s.GetHelloMessageAsync())
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.GetHello();

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
        Assert.Equal("Internal server error: DB error", obj.Value?.ToString());
    }

    [Fact]
    public async Task GetHello_Returns500_WhenServiceThrowsInvalidOperation()
    {
        _mockService.Setup(s => s.GetHelloMessageAsync())
            .ThrowsAsync(new InvalidOperationException("invalid op"));

        var result = await _sut.GetHello();

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);
    }

    // --- Edge cases ---

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
    {
        // BUG: hozirgi kodda bu test FAILS - NullReferenceException emas, ArgumentNullException kerak
        Assert.Throws<ArgumentNullException>(() => new HelloController(null!));
    }

    [Fact]
    public async Task GetHello_WhenServiceReturnsNull_Returns200WithNullBody()
    {
        // BUG: service null qaytarsa 200 OK + null body - kutilmagan xatti-harakat
        _mockService.Setup(s => s.GetHelloMessageAsync()).ReturnsAsync((string)null!);

        var result = await _sut.GetHello();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Null(ok.Value); // null body qaytadi — intentional emasdir
    }

    [Fact]
    public async Task GetHello_WhenServiceReturnsEmpty_Returns200()
    {
        _mockService.Setup(s => s.GetHelloMessageAsync()).ReturnsAsync(string.Empty);

        var result = await _sut.GetHello();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal(string.Empty, ok.Value);
    }

    [Fact]
    public async Task GetHello_CallsServiceExactlyOnce()
    {
        _mockService.Setup(s => s.GetHelloMessageAsync()).ReturnsAsync("Hello World");

        await _sut.GetHello();

        _mockService.Verify(s => s.GetHelloMessageAsync(), Times.Once);
    }
}
```

## Tuzatish tavsiyasi (konstruktor null check)

```csharp
public HelloController(IHelloService helloService)
{
    _helloService = helloService ?? throw new ArgumentNullException(nameof(helloService));
}
```