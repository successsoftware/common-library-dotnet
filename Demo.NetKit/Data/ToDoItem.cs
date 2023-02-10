using SSS.EntityFrameworkCore.Extensions;
using SSS.EntityFrameworkCore.Extensions.Entities;

namespace Demo.NetKit.Data
{
    public class ToDoItem : AuditEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class ToDoItemConfiguration : BaseConfiguration<ToDoItem> { }
}