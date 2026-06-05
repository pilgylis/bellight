using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static EntityFrameworkBuilder<TDbContext> AddEntityFrameworkContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : DbContext
    {
        if (configure is not null)
            services.AddDbContext<TDbContext>(configure);

        return new EntityFrameworkBuilder<TDbContext>(services);
    }
}
