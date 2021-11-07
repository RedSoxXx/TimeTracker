using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TimeTracker.Common;
using TimeTracker.Common.Json;
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

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
})
.AddControllersAsServices()
.AddViewComponentsAsServices()
.AddNewtonsoftJson(o =>
{
    o.SerializerSettings.Merge(JsonSerializerSettingsProvider.GetDefault());

    // By default "$type" property is expected to be the first in json of request body, but in reality it's not always the case.
    // This behaviour can be changed using MetadataPropertyHandling option (see https://github.com/JamesNK/Newtonsoft.Json/issues/1653).
    o.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
});

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();

void RegisterDataAccessInfrastructure(IServiceCollection services)
{
    services.AddScoped(typeof(IBaseRepository<>), typeof(EntityRepository<>));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}

void RegisterBusinessInfrastructure(IServiceCollection services)
{

}