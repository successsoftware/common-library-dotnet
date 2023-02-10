using Demo.NetKit.Dto;
using Microsoft.AspNetCore.Mvc;
using SSS.AspNetCore.Extensions.Jwt.Proxies;

namespace Demo.NetKit.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly OAuthClient _proxy;

        public UserController(OAuthClient proxy)
        {
            _proxy = proxy;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            var response = await _proxy.EnsureApiTokenAsync(dto.Username, dto.Password);

            if (response.Success) return Ok(response.Result);

            return BadRequest(response.Result);
        }
    }
}
