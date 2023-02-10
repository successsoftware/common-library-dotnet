using AutoMapper;
using Demo.NetKit.Data;
using Demo.NetKit.Dto;
using Microsoft.EntityFrameworkCore;
using SSS.AspNetCore.Extensions.Exceptions;
using SSS.AspNetCore.Extensions.ServiceProfiling;
using SSS.CommonLib.Entensions;
using SSS.CommonLib.Models;
using SSS.EntityFrameworkCore.Extensions;
using SSS.EntityFrameworkCore.Extensions.Extensions;

namespace Demo.NetKit.Services
{
    public interface IToDoService
    {
        Task<Pageable<TodoItemDto>> SearchAsync(BaseQuery @param);

        Task<TodoItemDto> CreateAsync(CreateTodoItemDto dto);

        Task<TodoItemDto> GetByIdAsync(string id);
    }


    public class ToDoService : BaseService<ToDoService>, IToDoService
    {
        private readonly IDbContext _context;
        private readonly DbSet<ToDoItem> _dbSet;

        public ToDoService(IMapper mapper, ILoggerFactory loggerFactory, IDbContext context) : base(mapper, loggerFactory)
        {
            _context = context;
            _dbSet = context.Set<ToDoItem>();
        }

        public async Task<TodoItemDto> CreateAsync(CreateTodoItemDto dto)
        {
            var entity = MapToEntity<ToDoItem, CreateTodoItemDto>(dto);

            _dbSet.Add(entity);

            await _context.SaveChangesAsync();

            return MapToDto<ToDoItem, TodoItemDto>(entity);
        }

        public async Task<TodoItemDto> GetByIdAsync(string id)
        {
            var todoItem = await _dbSet.FindAsync(id);

            if (todoItem == null) throw new NotFoundException();

            return MapToDto<ToDoItem, TodoItemDto>(todoItem);
        }

        public async Task<Pageable<TodoItemDto>> SearchAsync(BaseQuery @param)
        {
            var query = _dbSet.ApplySort(@param.OrderBy, @param.Direction).AsQueryable();

            if (!string.IsNullOrEmpty(@param.SearchText))
            {
                if (!string.IsNullOrEmpty(@param.SearchFields))
                {
                    query = query.ApplyLikeSearch(@param.SearchText, @param.GetFilteredFields());
                }
                else
                {
                    query = query.Where(x => EF.Functions.Like(x.Title, $"%{@param.SearchText}%"));
                }
            }

            return await query.Select(x => _mapper.Map<TodoItemDto>(x)).PaginatedListAsync(@param.PageIndex, param.PageSize);
        }
    }
}
