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
using Blazored.LocalStorage;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides; // 识别反代/IIS 的真实协议/主机/端口
using Microsoft.Extensions.Options;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog 提前接管日志 =====
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
);

// ===== Razor 组件 =====
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== MudBlazor =====
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
});

// ===== Controllers & Swagger =====
builder.Services.AddControllers(options =>
{
    // 禁用 API 的防伪验证（便于 Swagger “Try it out”）
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

// ===== DbContext =====
builder.Services.AddDbContext<DailyDbContext>(m =>
    m.UseSqlServer(builder.Configuration.GetConnectionString("ConnStr")));

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(AutoMapperSetting));

// ===== JSON 设置（给 Minimal API / Controllers 使用）=====
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

// ===== 本地存储 =====
builder.Services.AddBlazoredLocalStorage();

// ===== 业务服务 =====
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProcessOneService>();
builder.Services.AddScoped<ProcessTwoService>();
builder.Services.AddScoped<ProcessThreeService>();
builder.Services.AddScoped<BarcodeScannerService>();
builder.Services.AddScoped<DetectService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<IDX023Service, DX023Service>();

// ===== 获取当前请求信息（用于动态 BaseUrl）=====
builder.Services.AddHttpContextAccessor();

// ===== RestClient（.NET 9 + RestSharp 111）：不显式指定序列化器，使用默认 System.Text.Json =====
builder.Services.AddScoped(serviceProvider =>
{
    var http = serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

    // 配置兜底（若要调用外部后端，可在 appsettings 里配置完整地址；为空则跟随当前请求）
    var cfgBase = builder.Configuration["ApiSettings:BaseUrl"];

    Uri? baseUri = null;
    if (!string.IsNullOrWhiteSpace(cfgBase))
    {
        // 明确外部后端地址时优先使用配置
        baseUri = new Uri(cfgBase, UriKind.Absolute);
    }
    else if (http != null)
    {
        // 跟随 IIS 的协议/主机/端口/PathBase
        baseUri = new Uri($"{http.Request.Scheme}://{http.Request.Host}{http.Request.PathBase}/", UriKind.Absolute);
    }

    var clientOptions = new RestClientOptions
    {
        BaseUrl = baseUri,
        ThrowOnAnyError = false,
        MaxTimeout = (builder.Configuration.GetValue<int?>("ApiSettings:Timeout") ?? 30) * 1000
    };

    // ⚠️ 不再调用 UseSerializer/UseSystemTextJson，直接用默认的 System.Text.Json
    var restClient = new RestClient(clientOptions);
    return restClient;
});

var app = builder.Build();

// ===== 识别反向代理/IIS 的真实协议/主机/端口（在任何使用 Request 之前）=====
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                     | ForwardedHeaders.XForwardedProto
                     | ForwardedHeaders.XForwardedHost
};
// 若有固定网关/代理，建议在此配置 KnownProxies/KnownNetworks；此处清空表示信任所有
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

// ===== Swagger（所有环境可用）——根据转发头动态生成服务器地址（端口/虚拟目录友好）=====
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var scheme   = httpReq.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? httpReq.Scheme;
        var host     = httpReq.Headers["X-Forwarded-Host"].FirstOrDefault()  ?? httpReq.Host.Value;
        var basePath = httpReq.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? httpReq.PathBase.Value;

        swagger.Servers = new List<OpenApiServer>
        {
            new() { Url = $"{scheme}://{host}{basePath}" }
        };
    });
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DexRobotPDA API V1");
    c.RoutePrefix = "swagger";
});

// ===== 异常页 =====
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

// 修复 Missing Antiforgery Middleware 错误
app.UseAntiforgery();

// ===== Serilog 请求日志 =====
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

// ===== Razor 页面（会自动使用防伪中间件）=====
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ===== 静态资源与 API =====
app.MapStaticAssets();
app.MapControllers();

app.Run();
