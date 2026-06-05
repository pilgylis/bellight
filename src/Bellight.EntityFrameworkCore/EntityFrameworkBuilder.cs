using Bellight.DataManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.EntityFrameworkCore;

public class EntityFrameworkBuilder<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceCollection _services;

    internal EntityFrameworkBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public EntityFrameworkBuilder<TDbContext> AddRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        _services.AddScoped<IRepository<TEntity, TKey>>(sp =>
            new EntityFrameworkRepository<TEntity, TKey>(sp.GetRequiredService<TDbContext>()));
        return this;
    }
}
