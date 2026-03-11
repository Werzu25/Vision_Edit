using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ORM.Services;

namespace Vision_Edit_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Document : ControllerBase
{
    private readonly DbManager _dbManager;
    private readonly UserService _userService;

    public Document(DbManager dbManager)
    {
        _dbManager = dbManager;
        _userService = new UserService(dbManager);
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] SaveDocumentModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
            return BadRequest("Username is required.");

        var user = await _userService.GetUserByUsername(model.Username);
        if (user == null)
            return Unauthorized();

        string name = string.IsNullOrWhiteSpace(model.Name) ? "Untitled" : model.Name.Trim();

        var existing = await _dbManager.Documents
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Name == name && d.User.Id == user.Id);

        if (existing == null)
        {
            var created = (await _dbManager.Documents.AddAsync(new DocumentModel
            {
                Name = name,
                Content = model.Content ?? string.Empty,
                CreationDate = DateTime.UtcNow,
                User = user
            })).Entity;

            if (await _dbManager.SaveChangesAsync() <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(new { created.Id, created.Name, created.CreationDate });
        }

        existing.Content = model.Content ?? string.Empty;

        if (await _dbManager.SaveChangesAsync() <= 0)
            return StatusCode(StatusCodes.Status500InternalServerError);

        return Ok(new { existing.Id, existing.Name, existing.CreationDate });
    }
}
