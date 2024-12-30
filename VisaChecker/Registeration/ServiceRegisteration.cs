using RabbitMQ.Client;
using VisaChecker.Services;

namespace VisaChecker.Registeration
{
    public static class ServiceRegisteration
    {
        public static IServiceCollection AddServiceRegisteration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory() { Uri = new Uri(configuration.GetConnectionString("RabbitMq")) });

            services.AddSingleton<RabbitMqService>();

            return services;
        }
    }
}
