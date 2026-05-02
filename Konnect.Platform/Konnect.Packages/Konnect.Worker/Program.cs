var hostApplicationBuilder = Host.CreateApplicationBuilder(args);

var host = hostApplicationBuilder.Build();
host.Run();
