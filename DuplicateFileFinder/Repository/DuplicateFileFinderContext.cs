using Microsoft.EntityFrameworkCore;

namespace DuplicateFileFinder.Repository;

internal class DuplicateFileFinderContext : DbContext
{
    private readonly string _connectionString;

    public DuplicateFileFinderContext()
    {
    }

    public DuplicateFileFinderContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual DbSet<FileInfoSlim> FileInfoSlims { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=" + _connectionString + ";");
    }
}
