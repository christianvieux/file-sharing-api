using Microsoft.EntityFrameworkCore;

class FileDB : DbContext
{
    public FileDB(DbContextOptions<FileDB> options)
        : base(options) { }

    public DbSet<File> Files => Set<File>();
}