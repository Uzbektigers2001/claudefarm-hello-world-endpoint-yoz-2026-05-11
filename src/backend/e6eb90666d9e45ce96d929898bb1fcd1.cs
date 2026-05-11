## Kod

```csharp
// IHelloService.cs
public interface IHelloService
{
    Task<string> GetGreetingAsync();
}
```

```csharp
// HelloService.cs
public class HelloService : IHelloService
{
    public async Task<string> GetGreetingAsync()
    {
        return await Task.FromResult("Hello World");
    }
}
```

```csharp
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
    public async Task<IActionResult> Get()
    {
        try
        {
            var result = await _helloService.GetGreetingAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
```

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IHelloService, HelloService>();

var app = builder.Build();

app.MapControllers();
app.Run();
```

---

**Nima qildim:**
- `IHelloService` → SOLID (D — Dependency Inversion, I — Interface Segregation)
- `AddScoped` → DI container orqali inject qilindi
- `async/await` → kelajakda real async operatsiyalarga tayyor
- `try/catch` → exception handling, 500 qaytaradi

**Natija:** `GET /api/hello` → `200 OK` → `"Hello World"`