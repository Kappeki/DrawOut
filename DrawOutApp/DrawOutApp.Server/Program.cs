using DrawOutApp.Server.Settings;
using DrawOutApp.Server.Services;
using Microsoft.Extensions.Options;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Services.Contracts;
using MongoDB.Driver;
using StackExchange.Redis;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region MongoDB

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoDBSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddTransient<IRoomRepo, RoomRepository>();

builder.Services.AddScoped<IRoomService, RoomService>();

#endregion

#region REDIS
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<IRedisSettings>(sp =>
    sp.GetRequiredService<IOptions<RedisSettings>>().Value);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect(settings.ConnectionString);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();
    options.Configuration = redisSettings.ConnectionString;
    options.InstanceName = redisSettings.InstanceName;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always; 
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.IdleTimeout = TimeSpan.FromDays(7);
});

//repositories
builder.Services.AddTransient<IChatMessageRepo, ChatMessageRepository>();
builder.Services.AddTransient<ITeamRepo, TeamRepository>();
builder.Services.AddTransient<IRoundRepo, RoundRepository>();
builder.Services.AddTransient<IGameRepo, GameRepository>();
builder.Services.AddTransient<IUserRepo, UserRepository>();

//services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeamService,TeamService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IGameService, GameService>();

#endregion

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
