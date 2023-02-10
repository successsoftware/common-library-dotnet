using AutoMapper;
using Demo.NetKit.Dto;
using Demo.NetKit.Mapping;

namespace Demo.NetKit.Models
{
    public class User : IMapFrom<CreateTodoItemDto>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTodoItemDto, User>();
        }
    }
}
