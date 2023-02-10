using AutoMapper;
using Demo.NetKit.Data;
using Demo.NetKit.Mapping;

namespace Demo.NetKit.Dto
{
    public class TodoItemDto : IMapFrom<ToDoItem>
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoItem, TodoItemDto>();
        }
    }
}
