using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

using OpenTelemetry.Resources;
using Serilog;
using System.Diagnostics;
 
namespace MyApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //// ✅ Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Host.UseSerilog();
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Add OpenTelemetry
            //builder.Services.AddOpenTelemetry()
            //    .WithTracing(tracer =>
            //    {
            //        tracer
            //            .AddAspNetCoreInstrumentation()   // API calls track karega
            //            .AddHttpClientInstrumentation()   // external API calls
            //            .AddSqlClientInstrumentation()   // DB calls

            //            // Jaeger export
            //            .AddJaegerExporter(options =>
            //            {
            //                options.AgentHost = "localhost";
            //                options.AgentPort = 6831;
            //            });
            //    })
            //    .WithMetrics(metrics =>
            //    {
            //        metrics
            //            .AddAspNetCoreInstrumentation();
            //       //    .AddPrometheusExporter(); // Prometheus ke liye
            //    });
            //// ✅ OpenTelemetry
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracer =>
                   {
                    tracer
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault().AddService("MyApp")
                        )
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddHttpClientInstrumentation()
                        
                        .AddSqlClientInstrumentation()
                        
                      
        .AddOtlpExporter(opt => opt.Endpoint = new Uri("http://your-jaeger-url:4317"))

                    // 👉 Comment if Jaeger not running
                    .AddJaegerExporter(o =>
                    {
                        o.AgentHost = "localhost";
                        o.AgentPort = 6831;
                    });
                }).WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddPrometheusExporter(); // ✅ now works
                });
            ;
            builder.Services.AddControllers();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseOpenTelemetryPrometheusScrapingEndpoint();
            



app.MapPrometheusScrapingEndpoint();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //// ✅ Custom Middleware
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var sw = Stopwatch.StartNew();

                logger.LogInformation("Request Started: {method} {url}",
                    context.Request.Method,
                    context.Request.Path);

                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled Exception");
                    throw;
                }

                sw.Stop();

                logger.LogInformation("Request Finished: {statusCode} in {time} ms",
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            });

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}



//using OpenTelemetry.Logs;
//using OpenTelemetry.Metrics;
//using OpenTelemetry.Resources;
//using OpenTelemetry.Trace;
//using Sejil;

//var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.AddSejil("/sejil.db", LogLevel.Information);

//builder.Services.AddControllersWithViews();

//// Define service identity
//var serviceName = "MyCoolApp";

//builder.Services.AddOpenTelemetry()
//    .ConfigureResource(resource => resource.AddService(serviceName))
//    .WithTracing(tracing =>
//    {
//        tracing
//            .AddAspNetCoreInstrumentation()
//            .AddHttpClientInstrumentation()
//            .AddSqlClientInstrumentation()
//            // Modern way: Jaeger, Honeycomb, and Grafana all use OTLP now
//            .AddOtlpExporter(options =>
//            {
//                options.Endpoint = new Uri("http://localhost:4317"); // Default OTLP gRPC port
//            });
//    })
//    .WithMetrics(metrics =>
//    {
//        metrics
//            .AddAspNetCoreInstrumentation()
//            .AddHttpClientInstrumentation()
//            .AddRuntimeInstrumentation() // Tracks CPU, Memory, GC
//            .AddPrometheusExporter();
//    });
//// 1. Add this to your Program.cs
//builder.Logging.AddOpenTelemetry(options =>
//{
//    options.IncludeFormattedMessage = true;
//    options.IncludeScopes = true;

//    // Choose where to see them:
//    options.AddConsoleExporter(); // Good for local debugging
//    options.AddOtlpExporter(otlpOptions => {
//        otlpOptions.Endpoint = new Uri("http://localhost:4317");
//    });
//});


//var app = builder.Build();


//// Order matters: Map the endpoint BEFORE routing if possible
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}

//app.UseSejil();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();




////using Sejil;
////using OpenTelemetry.Logs;
////using OpenTelemetry.Metrics;
////using OpenTelemetry.Resources;
////using OpenTelemetry.Trace;

////var builder = WebApplication.CreateBuilder(args);

////// 1. IMPORTANT: Sejil Registration must be at the very top of builder section
////builder.WebHost.AddSejil("/sejil.db", LogLevel.Information);

////// Add your OpenTelemetry configuration
////builder.Services.AddOpenTelemetry()
////    .ConfigureResource(resource => resource.AddService("MyCoolApp"))
////    .WithTracing(tracing => {
////        tracing.AddAspNetCoreInstrumentation().AddOtlpExporter();
////    })
////    .WithMetrics(metrics => {
////        metrics.AddAspNetCoreInstrumentation().AddPrometheusExporter();
////    });

////builder.Services.AddControllersWithViews();

////var app = builder.Build();

////// 2. IMPORTANT: UseSejil must be the VERY FIRST middleware
////app.UseSejil();

////if (!app.Environment.IsDevelopment())
////{
////    app.UseExceptionHandler("/Home/Error");
////}

////app.UseStaticFiles();
////app.UseRouting();
////app.UseAuthorization();

////app.MapControllerRoute(
////    name: "default",
////    pattern: "{controller=Home}/{action=Index}/{id?}");

////// 3. Optional: Map the Prometheus endpoint if you still need it
////app.UseOpenTelemetryPrometheusScrapingEndpoint();

////app.Run();

//using OpenTelemetry.Trace;
//using Serilog;
//using System.Diagnostics;

//namespace MyApp
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            // ✅ Serilog configuration (file logging)
//            Log.Logger = new LoggerConfiguration()
//                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
//                .CreateLogger();

//            var builder = WebApplication.CreateBuilder(args);

//            // Use Serilog
//            builder.Host.UseSerilog();

//            // Add services
//            builder.Services.AddControllers();

//            // ✅ OpenTelemetry setup
//            builder.Services.AddOpenTelemetry()
//                .WithTracing(tracer =>
//                {
//                    tracer
//                        .AddAspNetCoreInstrumentation(options =>
//                        {
//                            options.RecordException = true; // capture exceptions
//                        })
//                        .AddHttpClientInstrumentation()
//                        .AddSqlClientInstrumentation()

//                        // Jaeger exporter
//                        .AddJaegerExporter(o =>
//                        {
//                            o.AgentHost = "localhost";
//                            o.AgentPort = 6831;
//                        });
//                });

//            var app = builder.Build();

//            // ✅ Custom Middleware (like Application Insights)
//            app.Use(async (context, next) =>
//            {
//                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//                var sw = Stopwatch.StartNew();

//                logger.LogInformation("Request Started: {method} {url}",
//                    context.Request.Method,
//                    context.Request.Path);

//                try
//                {
//                    await next();
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Unhandled Exception Occurred");
//                    throw;
//                }

//                sw.Stop();

//                logger.LogInformation("Request Finished: {statusCode} in {time} ms",
//                    context.Response.StatusCode,
//                    sw.ElapsedMilliseconds);
//            });

//            app.UseHttpsRedirection();

//            app.UseAuthorization();

//            app.MapControllers();

//            app.Run();
//        }
//    }
//}
//using OpenTelemetry.Trace;
//using OpenTelemetry.Resources;
//using Serilog;
//using System.Diagnostics;

//var builder = WebApplication.CreateBuilder(args);

//// ✅ Serilog
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();

//// ✅ OpenTelemetry
//builder.Services.AddOpenTelemetry()
//    .WithTracing(tracer =>
//    {
//        tracer
//            .SetResourceBuilder(
//                ResourceBuilder.CreateDefault().AddService("MyApp")
//            )
//            .AddAspNetCoreInstrumentation(options =>
//            {
//                options.RecordException = true;
//            })
//            .AddHttpClientInstrumentation()
//            .AddSqlClientInstrumentation()

//        // 👉 Comment if Jaeger not running
//        .AddJaegerExporter(o =>
//        {
//            o.AgentHost = "localhost";
//            o.AgentPort = 6831;
//        });
//    });

//builder.Services.AddControllers();

//var app = builder.Build();

//// ✅ IMPORTANT: Routing
//app.UseRouting();

//// ✅ Custom Middleware
//app.Use(async (context, next) =>
//{
//    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//    var sw = Stopwatch.StartNew();

//    logger.LogInformation("Request Started: {method} {url}",
//        context.Request.Method,
//        context.Request.Path);

//    try
//    {
//        await next();
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Unhandled Exception");
//        throw;
//    }

//    sw.Stop();

//    logger.LogInformation("Request Finished: {statusCode} in {time} ms",
//        context.Response.StatusCode,
//        sw.ElapsedMilliseconds);
//});

//// 👉 Optional (comment if issue)
//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
