using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TodoListDB>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"],
      ValidAudience = builder.Configuration["Jwt:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
  });

builder.Services.AddSingleton<AuthService>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapPost("/login", async (TodoListDB db, AuthService auth, [FromForm] string username, [FromForm] string password) =>
{
  var user = await db.Users.FirstOrDefaultAsync(user => user.Username == username);
  
  if (user == null) return Results.NotFound();

  if (PassHasher.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
  {
    var token = auth.GenerateJwtToken(username);

    return Results.Ok(token);
  }
  else
  {
    return Results.Unauthorized();
  }
}).DisableAntiforgery();

app.MapPost("/signup", async (TodoListDB db, AuthService auth, [FromForm] string username, [FromForm] string password, [FromForm] string name) =>
{
  var user = await db.Users.FirstOrDefaultAsync(user => user.Username == username);

  if (user == null)
  {
    PassHasher.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

    user = new User
    {
      Name = name,
      Username = username,
      PasswordHash = hash,
      PasswordSalt = salt
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = auth.GenerateJwtToken(username);

    return Results.Created($"/user/{user.Id}", token);
  }
  else
  {
    return Results.Conflict();
  }
}).DisableAntiforgery();

app.MapGet("/authorize", async (TodoListDB db) =>
{
  try
  {
    var users = await db.Users.ToListAsync();

    return Results.Ok(users);
  }
  catch (Exception ex)
  {
    return Results.Problem(ex.Message);
  }
}).AddEndpointFilter<AuthFilter>();

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

app.MapPost("/project/todo", async (TodoListDB db, Todo todo) =>
{
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

