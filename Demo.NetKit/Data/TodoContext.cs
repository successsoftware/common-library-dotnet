using Microsoft.EntityFrameworkCore;
using SSS.CommonLib.Interfaces;
using SSS.EntityFrameworkCore.Extensions;
using System.Reflection;

namespace Demo.NetKit.Data
{
    public class TodoContext : AuditContext
    {
        public TodoContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor,
            IDateTimeService dateTimeService)
            : base(options, httpContextAccessor, dateTimeService)
        {
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }

        protected override void ConfigureEntities(ModelBuilder builder)
        {
            foreach (Assembly currentassembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builder.ApplyConfigurationsFromAssembly(currentassembly);
            }
        }
    }
}