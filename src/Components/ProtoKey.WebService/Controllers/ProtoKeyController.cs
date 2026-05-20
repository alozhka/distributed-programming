using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ProtoKey.Storage;

namespace ProtoKey.WebService.Controllers;

[ApiController]
[Route("api/keys")]
public partial class ProtoKeyController(StorageService storage) : ControllerBase
{
    private static readonly Regex KeyPattern = KeyRegex();

    [HttpPut("{key}")]
    public async Task<IActionResult> Set(string key, [FromBody] int value)
    {
        if (!KeyPattern.IsMatch(key))
        {
            return BadRequest();
        }

        await storage.Set(key, value);

        return NoContent();
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        if (!KeyPattern.IsMatch(key))
        {
            return BadRequest();
        }

        GetResponse response = await storage.Get(key);

        return Ok(new { response.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Keys([FromQuery] string prefix)
    {
        KeysResponse response = await storage.Keys(prefix);

        return Ok(response.Keys);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_\-.]{1,1000}$")]
    private static partial Regex KeyRegex();
}