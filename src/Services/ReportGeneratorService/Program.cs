using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (context, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserCreatedConsumer>();

                x.UsingRabbitMq(
                    (ctx, cfg) =>
                    {
                        cfg.Host(
                            "localhost",
                            "/",
                            h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            }
                        );

                        cfg.ConfigureEndpoints(ctx);
                    }
                );
            });
        }
    )
    .Build();

await host.RunAsync();
