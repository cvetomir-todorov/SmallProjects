using Microsoft.EntityFrameworkCore;
using World.Service.Database.Continents;
using World.Service.Database.Countries;

namespace World.Service.Database;

public class WorldDbContext : DbContext
{
    public WorldDbContext(DbContextOptions<WorldDbContext> dbContextOptions) : base(dbContextOptions)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContinentEntity>().HasKey(c => c.Id);
        modelBuilder.Entity<ContinentEntity>().HasIndex(c => c.Name).IsUnique();

        modelBuilder.Entity<CountryEntity>().HasKey(c => c.Id);
        modelBuilder.Entity<CountryEntity>().HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<CountryEntity>().HasOne(c => c.Continent);
    }

    public DbSet<ContinentEntity> Continents { get; set; } = null!;

    public DbSet<CountryEntity> Countries { get; set; } = null!;
}
