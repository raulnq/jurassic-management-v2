using SqlKata.Execution;
using static Rebus.Routing.TypeBased.TypeBasedRouterConfigurationExtensions;
using Rebus.Config;
using Rebus.Serialization.Json;
using Rebus.Bus;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRebus(this IServiceCollection services, IConfiguration configuration, Action<IConfiguration, TypeBasedRouterConfigurationBuilder>? map = null, Func<IBus, Task>? onCreated = null)
    {
        var rebusConfig = configuration.GetSection("Rebus");

        if (!rebusConfig.Exists())
        {
            return services;
        }

        var rabbitmqConfig = configuration.GetSection("RabbitMQ");

        var serviceBusConfig = configuration.GetSection("AzureServiceBus");

        if (rabbitmqConfig.Exists() || serviceBusConfig.Exists())
        {
            var queue = rebusConfig.GetValue("Queue", "default");

            services.AutoRegisterHandlersFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            //var logger = Log.ForContext("queue", queue);

            services
                .AddRebus(configurer =>
                {
                    return configurer
                        //.Logging(l => l.Serilog(logger))
                        .Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                        .Transport(t =>
                        {
                            if (serviceBusConfig.Exists())
                            {
                                var serviceBusConnectionString = serviceBusConfig["ConnectionString"];

                                var automaticallyRenewPeekLock = serviceBusConfig.GetValue("AutomaticallyRenewPeekLock", false);

                                if (automaticallyRenewPeekLock)
                                {
                                    t.UseAzureServiceBus(serviceBusConnectionString, queue).AutomaticallyRenewPeekLock();
                                }
                                else
                                {
                                    t.UseAzureServiceBus(serviceBusConnectionString, queue);
                                }
                            }
                            else
                            {
                                if (rabbitmqConfig.Exists())
                                {
                                    var rabbitMqConnectionString = rabbitmqConfig["ConnectionString"];

                                    t.UseRabbitMq(rabbitMqConnectionString, queue);
                                }
                            }
                        })
                        .Routing(r =>
                        {
                            if (map != null)
                            {
                                map(configuration, r.TypeBased());
                            }
                            else
                            {
                                r.TypeBased();
                            }
                        })
                        .Options(o =>
                        {
                            //o.Decorate<IPipeline>(c =>
                            //{
                            //    var pipeline = c.Get<IPipeline>();
                            //    var tracingStep = new MessageTracingStep();
                            //    var loggingStep = new MessageLoggingStep();

                            //    return new PipelineStepInjector(pipeline)
                            //        .OnReceive(tracingStep, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep))
                            //        .OnReceive(loggingStep, PipelineRelativePosition.After, typeof(DispatchIncomingMessageStep));
                            //});
                        });
                }, onCreated: onCreated);
        }

        return services;
    }
}
