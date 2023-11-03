using Microsoft.AspNetCore.HttpOverrides;

namespace RestApiConfigurationProvider.Demo;

public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
    }

    public IConfiguration Configuration { get; }

    public IHostEnvironment HostEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddOptions();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
