using AutoMapper;
using Demo.NetKit.Dto;
using Demo.NetKit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSS.CommonLib.Models;

namespace Demo.NetKit.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ToDoController : ControllerBase
    {
        private readonly IToDoService _service;
        private readonly IMapper _mapper;

        public ToDoController(IToDoService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] BaseQuery @query)
            => Ok(await _service.SearchAsync(@query));

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(string id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoItemDto dto)
            => Ok(await _service.CreateAsync(dto));
    }
}
