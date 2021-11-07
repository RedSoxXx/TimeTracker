using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Common;
using TimeTracker.DAL;
using TimeTracker.DAL.Contracts;
using TimeTracker.DAL.Repositories;
using TimeTracker.Data;
using TimeTracker.Data.Identity;

var builder = WebApplication.CreateBuilder(args);

const string devConnectionStringName = "RTSDev";
const string prodConnectionStringName = "RTSProd";

// Add services to the container.
builder.Services.AddDbContext<TimeTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(CommonAppConfig.IsDebugMode ? devConnectionStringName : prodConnectionStringName)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = !CommonAppConfig.IsDebugMode)
    .AddRoles<Role>()
    .AddEntityFrameworkStores<TimeTrackerDbContext>();
builder.Services.AddRazorPages();

RegisterDataAccessInfrastructure(builder.Services);
RegisterBusinessInfrastructure(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

void RegisterDataAccessInfrastructure(IServiceCollection services)
{
    services.AddScoped(typeof(IBaseRepository<>), typeof(EntityRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}

void RegisterBusinessInfrastructure(IServiceCollection services)
{

}