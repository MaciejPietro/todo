namespace todo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<NoteModel> Notes { get; set; }

        public DbSet<UserModel> Users { get; set; }

    }
}

 
    
