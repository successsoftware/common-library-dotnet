namespace Demo.NetKit.Data
{
    public class SeedData
    {
        private readonly TodoContext _context;

        public SeedData(TodoContext context)
        {
            _context = context;
        }

        public void PopulateData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            if (!_context.ToDoItems.Any())
            {
                var data = new List<ToDoItem>
                {
                    new ToDoItem{ Id = Guid.NewGuid().ToString(), Title = "What is Lorem Ipsum?", Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry." },
                    new ToDoItem{ Id = Guid.NewGuid().ToString(), Title = "Why do we use it?",Description ="It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout." },
                    new ToDoItem{ Id = Guid.NewGuid().ToString(), Title = "Where does it come from?", Description = "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old." },
                    new ToDoItem{ Id = Guid.NewGuid().ToString(), Title = "Where can I get some?", Description = "There are many variations of passages of Lorem Ipsum available, but the majority have suffered alteration in some form, by injected humour, or randomised words which don't look even slightly believable." }
                };

                _context.ToDoItems.AddRange(data);

                _context.SaveChanges();
            }

        }
    }
}