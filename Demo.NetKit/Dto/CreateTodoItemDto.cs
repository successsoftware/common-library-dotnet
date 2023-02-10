using AutoMapper;
using Demo.NetKit.Data;
using Demo.NetKit.Mapping;
using FluentValidation;

namespace Demo.NetKit.Dto
{
    public class CreateTodoItemDto : IMapTo<ToDoItem>
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTodoItemDto, ToDoItem>();
        }
    }

    public class CreateTodoItemDtoValidator : AbstractValidator<CreateTodoItemDto>
    {
        public CreateTodoItemDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().NotNull();
            RuleFor(x => x.Description).NotEmpty().NotNull();
        }
    }
}
