```csharp
using Microsoft.AspNetCore.Mvc;

namespace HelloWorldApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloWorldController : ControllerBase
{
    private readonly ILogger<HelloWorldController> _logger;

    public HelloWorldController(ILogger<HelloWorldController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/api/hello")]
    public async Task<IActionResult> GetHello()
    {
        try
        {
            _logger.LogInformation("GET /api/hello called at {Time}", DateTime.UtcNow);
            
            var result = await Task.FromResult("Hello World");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetHello endpoint");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

**Program.cs** (minimal setup):
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

---

**Nima qildim:**
- `[ApiController]` + `[Route]` — REST convention
- `ILogger` — DI orqali logging (SOLID: D prinsipi)
- `async/await` + `Task.FromResult` — async pattern to'g'ri ishlatildi
- `try/catch` — exception handling, 500 qaytaradi
- `/api/hello` → `200 OK: "Hello World"` response