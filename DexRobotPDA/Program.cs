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

    
// 初始化构建器
var builder = WebApplication.CreateBuilder(args);

// 尽早配置Serilog以捕获更多启动日志
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// 添加Razor组件支持
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 配置MudBlazor服务及全局设置
builder.Services.AddMudServices(config =>
{
    // 全局Snackbar配置
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
});

// 添加控制器和API文档支持
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "DexRobotPDA API",
        Version = "v1",
        Description = "DexRobotPDA项目的API接口文档"
    });
});

// 注入数据库上下文
builder.Services.AddDbContext<DailyDbContext>(m =>
    m.UseSqlServer(builder.Configuration.GetConnectionString("ConnStr")));

// 注入AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperSetting));

// 创建全局JSON序列化选项
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
};

// 配置全局JSON序列化选项
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
    options.SerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
    options.SerializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
        options.JsonSerializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
    });

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
    });

// 配置RestClient并使用全局JSON设置
builder.Services.AddSingleton(serviceProvider =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    var options = new RestClientOptions(baseUrl);
    
    return new RestClient(options, configureSerialization: s => 
        s.UseSystemTextJson(jsonOptions));
});

// 注册服务 - 后续添加新服务只需在这里注册
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProcessOneService>();
builder.Services.AddScoped<ProcessTwoService>();
builder.Services.AddScoped<ProcessThreeService>();
builder.Services.AddScoped<BarcodeScannerService>();
builder.Services.AddScoped<DetectService>();

// 添加session
// builder.Services.AddBlazoredSessionStorage();

builder.Services.AddBlazoredLocalStorage(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    { 
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RobotMaker API V1"); 
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
    