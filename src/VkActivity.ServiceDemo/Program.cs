//https://www.growin.com/blog/create-worker-service-api-door-net-core-3-1/

using VkActivity.Service;

var builder = Host.CreateDefaultBuilder(args);


builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.ConfigureServices(services =>
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    })
    .Configure(app =>
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    });

});

builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
});

builder.Build().Run();