using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TodoListDB>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure()));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/projects", async (TodoListDB db) => await db.Projects.ToListAsync());

app.Run();

