using BFF.IdentityServer.Application.Infrastructure;
using BFF.IdentityServer.Data.Models;
using BFF.IdentityServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BFF.IdentityServer.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Configuration

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var domainSettings = builder.Configuration.GetSection(DomainSettings.SectionName).Get<DomainSettings>();
builder.Services.AddSingleton<IDomainSettings>(domainSettings);

var environmentSettings = builder.Configuration.GetSection(EnvironmentSettings.SectionName).Get<EnvironmentSettings>();
builder.Services.AddSingleton<IEnvironmentSettings>(environmentSettings);

#endregion Configuration

#region Logging

builder.Logging.ClearProviders(); // FYI Built-in logging providers: Console, Debug, EventSource, EventLog (https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0#built-in-logging-providers)

if (environmentSettings.IsLocal)
{
    builder.Logging.AddConsole();
}

#endregion Logging


#region Configure Services

// gates
builder.Services.AddControllersWithViews();

// SQL server
var sqlMigrationsAssemblyName = typeof(Program).Assembly.FullName;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(sqlMigrationsAssemblyName)));

builder.Services.AddDbContextFactory<ApplicationDbContext>(lifetime: ServiceLifetime.Transient);

// asp identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.User.AllowedUserNameCharacters = null)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// identity server
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseSuccessEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseErrorEvents = true;
    options.Caching.ResourceStoreExpiration = TimeSpan.FromMinutes(15);
    options.Caching.ClientStoreExpiration = TimeSpan.FromMinutes(15);
    options.ServerSideSessions.RemoveExpiredSessionsFrequency = TimeSpan.FromMinutes(60);
})
    .AddServerSideSessions()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = builder =>
            builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(sqlMigrationsAssemblyName));
    })
    .AddConfigurationStoreCache()
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = builder =>
            builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(sqlMigrationsAssemblyName));
        options.EnableTokenCleanup = true; // by default 3600 seconds
    })
    .AddAspNetIdentity<ApplicationUser>();

builder.Services.AddAuthentication();
builder.Services.AddLocalApiAuthentication();

// other services
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(24));
builder.Services.AddHttpContextAccessor();

#endregion Configure Services

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseAuthorization();
app.UseIdentityServer();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "[BFF] Identity Server API V1");
});
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute().RequireAuthorization();
});

app.Logger.LogInformation("Migrate & Seed");

app.ApplyDefaultSeedConfiguration(app.Configuration);

app.Logger.LogInformation("Application is starting...");

app.Run();
