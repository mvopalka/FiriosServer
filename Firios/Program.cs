using Firios.Data;
using Firios.Middleware;
using Firios.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<FiriosSuperLightContext>(options => options.UseSqlite("Data Source=Firios.db;"));
builder.Services.AddDbContext<FiriosSuperLightContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));
builder.Services.AddTransient<UserHelperService>();
builder.Services.AddSingleton<WebSocketFiriosManagerService>();
builder.Services.AddSingleton<IncidentIdService>();
builder.Services.AddSingleton<FiriosSourceAuthentificationService>();
builder.Services.AddTransient<FiriosUserAuthenticationService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWebSockets();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Items.Add("error", "404");
        context.Request.Path = "/Home";
        await next();
    }
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseAuthMiddleware();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();