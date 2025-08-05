using habitaai.webapi.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<DynamicEndpoint> DynamicEndpoints => Set<DynamicEndpoint>();
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Altere a connection string conforme seu ambiente
        // optionsBuilder.UseSqlServer("Server=DESKTOP-O3LGOT5;Database=HabitaAiDb;Trusted_Connection=True;");
        //optionsBuilder.UseSqlServer("Server=DESKTOP-O3LGOT5;Database=HabitaAiDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");
        optionsBuilder.UseSqlServer("Server=DESKTOP-O3LGOT5;Database=HabitaAiDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");

        return new AppDbContext(optionsBuilder.Options);
    }
} 

// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//     // Construtor sem parâmetros para uso em design-time (apenas para migrations)
//     public AppDbContext() : base(new DbContextOptionsBuilder<AppDbContext>().Options) { }

//     public DbSet<Property> Properties => Set<Property>();
//     public DbSet<DynamicEndpoint> DynamicEndpoints => Set<DynamicEndpoint>();
// }

// using habitaai.webapi.domain;
// using Microsoft.EntityFrameworkCore;
// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
//     public DbSet<Property> Properties => Set<Property>();
//     public DbSet<DynamicEndpoint> DynamicEndpoints => Set<DynamicEndpoint>();
// }