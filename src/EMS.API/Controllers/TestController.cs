using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public TestController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var userCount = await _uow.Users.CountAsync();

        return Ok(new
        {
            Message = "Repository pattern working!",
            TotalUsers = userCount,
            DatabaseReady = true
        });
    }
}