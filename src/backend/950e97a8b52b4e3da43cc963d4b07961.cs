## Kod

```csharp
// IHelloService.cs
public interface IHelloService
{
    Task<string> GetHelloMessageAsync();
}

// HelloService.cs
public class HelloService : IHelloService
{
    public async Task<string> GetHelloMessageAsync()
    {
        return await Task.FromResult("Hello World");
    }
}

// HelloController.cs
[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly IHelloService _helloService;

    public HelloController(IHelloService helloService)
    {
        _helloService = helloService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHello()
    {
        try
        {
            var message = await _helloService.GetHelloMessageAsync();
            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IHelloService, HelloService>();

var app = builder.Build();

app.MapControllers();
app.Run();
```

## Tushuntirish

| Qaror | Sabab |
|-------|-------|
| `IHelloService` interface | SOLID → Dependency Inversion, test qilish oson |
| `AddScoped` | Request scope — standart API xizmatlar uchun |
| `async/await` | Kelajakda DB/HTTP call qo'shilsa tayyor |
| `try/catch` | 500 xatolikni client'ga to'g'ri qaytarish |

**Endpoint:** `GET /api/hello` → `200 OK` → `"Hello World"`