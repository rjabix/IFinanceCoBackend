using IFinanceCoBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<IFinanceDbContext, FinanceDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<IFinanceDbContext>();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGroup("identity").MapIdentityApi<AppUser>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("checkauth", async (HttpContext httpContext, UserManager<AppUser> userManager) =>
{
    var userName = httpContext.User.Identity?.Name;
    if (userName == null)
    {
        return Results.NotFound();
    }

    var userExists = await userManager.Users.AsNoTracking()
        .AnyAsync(u => u.UserName == userName);

    return userExists ? Results.Ok() : Results.NotFound();
}).RequireAuthorization();

app.Run();