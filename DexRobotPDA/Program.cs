using DexRobotPDA.Components;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.Services;
using DexRobotPDA.DataModel;
using Serilog;
using MudBlazor;
using DexRobotPDA.AutoMappers;
using RestSharp;
using System.Text.Json;
using RestSharp.Serializers.Json;
using Blazored.LocalStorage;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 提前接管日志
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
);

// Razor 组件
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
});

// Controllers & Swagger
builder.Services.AddControllers(options =>
{
    // ✅ 禁用 API 的防伪验证（Swagger “Try it out” 才能正常使用）
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DexRobotPDA API",
        Version = "v1",
        Description = "DexRobotPDA项目的API接口文档"
    });
});

// DbContext
builder.Services.AddDbContext<DailyDbContext>(m =>
    m.UseSqlServer(builder.Configuration.GetConnectionString("ConnStr")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperSetting));

// JSON 设置
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
};
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
    options.SerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
    options.SerializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
});

// RestClient
builder.Services.AddSingleton(serviceProvider =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    var options = new RestClientOptions(baseUrl);
    return new RestClient(options, configureSerialization: s => s.UseSystemTextJson(jsonOptions));
});

// 注册服务
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProcessOneService>();
builder.Services.AddScoped<ProcessTwoService>();
builder.Services.AddScoped<ProcessThreeService>();
builder.Services.AddScoped<BarcodeScannerService>();
builder.Services.AddScoped<DetectService>();
builder.Services.AddScoped<LogService>();

builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Swagger（所有环境可用）
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DexRobotPDA API V1");
    c.RoutePrefix = "swagger";
});

// 异常页
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ 必须加上这一句（修复 Missing Antiforgery Middleware 错误）
// 但放在 Razor 页面映射之前即可，不影响 Swagger
app.UseAntiforgery();

// Serilog 请求日志
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsedMs, ex) =>
    {
        var path = httpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/_blazor") || path.StartsWith("/_framework") ||
            path.EndsWith(".js") || path.EndsWith(".css") ||
            path.EndsWith(".png") || path.EndsWith(".jpg") ||
            path.EndsWith(".ico") || path.EndsWith(".map"))
        {
            return LogEventLevel.Verbose;
        }
        if (ex != null) return LogEventLevel.Error;
        if (elapsedMs > 500) return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("RequestHost", http.Request.Host.Value);
        diag.Set("RequestScheme", http.Request.Scheme);
        diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        diag.Set("RemoteIpAddress", http.Connection.RemoteIpAddress?.ToString() ?? "");
        diag.Set("RequestPath", http.Request.Path.Value ?? "");
        diag.Set("RequestMethod", http.Request.Method ?? "");
        var actionName = http.GetEndpoint()?.DisplayName ?? "";
        diag.Set("ActionName", actionName);

        var wsMb = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024d / 1024d;
        diag.Set("MemoryUsageMB", Math.Round(wsMb, 1));
    };
});

// Razor 页面（会自动使用防伪中间件）
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 静态资源与 API
app.MapStaticAssets();
app.MapControllers();

app.Run();
