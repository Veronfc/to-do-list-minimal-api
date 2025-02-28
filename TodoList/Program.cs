using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TodoListDB>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure()));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/projects", async (TodoListDB db) => await db.Projects.ToListAsync());

app.MapGet("/project/{id}", async (TodoListDB db, int id) => await db.Projects.FindAsync(id));

app.MapPost("/project", async (TodoListDB db, Project project) =>
{
  await db.Projects.AddAsync(project);
  await db.SaveChangesAsync();
  return Results.Created($"/project/{project.Id}", project);
});

app.MapPut("/project/{id}", async (TodoListDB db, Project update, int id) =>
{
  var project = await db.Projects.FindAsync(id);
  if (project == null) return Results.NotFound();
  db.Projects.Entry(project).CurrentValues.SetValues(update);
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.MapGet("/project/todo", async (TodoListDB db, Todo todo) => {
  await db.Todos.AddAsync(todo);
  await db.SaveChangesAsync();
  return Results.Created($"/project/todo/{todo.Id}", todo);
});

app.MapPut("/project/todo/{id}", async (TodoListDB db, Todo update, int id) =>
{
  var todo = await db.Todos.FindAsync(id);
  if (todo == null) return Results.NotFound();
  db.Todos.Entry(todo).CurrentValues.SetValues(update);
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.Run();

