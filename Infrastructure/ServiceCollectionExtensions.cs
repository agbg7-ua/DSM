using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using ApplicationCore.Domain.Repositories;
using Infrastructure.Repositories;
using Infrastructure.NHibernate;

namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // NHibernate
            var sessionFactory = NHibernateHelper.BuildSessionFactory();
            services.AddSingleton(sessionFactory);
            services.AddScoped(sp => sp.GetService<ISessionFactory>().OpenSession());
            
            // Unit of Work
            services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();

            // Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            return services;
        }
    }
}