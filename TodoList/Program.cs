using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContextPool<TodoListDB>(options =>
    options.UseNpgsql(DotNetEnv.Env.GetString("CONN_STRING")));

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
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DotNetEnv.Env.GetString("JWT_KEY")))
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

    return Results.Ok(new { Token = token, user.Id, user.Username, user.Name });
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

    return Results.Created($"/user/{user.Id}", new { Token = token, user.Id, user.Username, user.Name });
  }
  else
  {
    return Results.Conflict();
  }
}).DisableAntiforgery();

app.MapGet("/projects", async (TodoListDB db, [FromHeader] int userId) =>
{
  try
  {
    return Results.Ok(await db.Projects.Where(p => p.UserId == userId).ToListAsync());
  }
  catch
  {
    return Results.Unauthorized();
  }
}).AddEndpointFilter<AuthFilter>();

app.MapGet("/project/{id}", async (TodoListDB db, int id) =>
{
  try
  {
    var project = await db.Projects.FindAsync(id);
    if (project == null) return Results.NotFound();
    return Results.Ok(project);
  }
  catch (Exception e)
  {
    return Results.Unauthorized();
  }
}).AddEndpointFilter<AuthFilter>();

app.MapPost("/project", async (TodoListDB db, Project project) =>
{
  try
  {
    var user = await db.Users.FindAsync(project.UserId);
    user.Projects.Add(project);
    await db.SaveChangesAsync();

    return Results.Created($"/project/{project.Id}", project);
  }
  catch
  {
    return Results.Unauthorized();
  }

}).AddEndpointFilter<AuthFilter>();

app.MapPut("/project/{id}", async (TodoListDB db, Project update, int id) =>
{
  try
  {
    var project = await db.Projects.FindAsync(id);

    if (project == null) return Results.NotFound();

    db.Projects.Entry(project).CurrentValues.SetValues(update);

    await db.SaveChangesAsync();

    return Results.NoContent();
  }
  catch
  {
    return Results.Unauthorized();
  }
}).AddEndpointFilter<AuthFilter>();

app.MapPost("/project/todo", async (TodoListDB db, Todo todo, int projectId) =>
{
  try
  {
    var project = await db.Projects.FindAsync(projectId);
    project.Todos.Add(todo);

    await db.SaveChangesAsync();

    return Results.Created($"/project/todo/{todo.Id}", todo);
  }
  catch
  {
    return Results.Unauthorized();
  }
}).AddEndpointFilter<AuthFilter>();

app.MapPut("/project/todo/{id}", async (TodoListDB db, Todo update, int id) =>
{
  try
  {
    var todo = await db.Todos.FindAsync(id);

    if (todo == null) return Results.NotFound();

    db.Todos.Entry(todo).CurrentValues.SetValues(update);

    await db.SaveChangesAsync();

    return Results.NoContent();
  }
  catch
  {
    return Results.Unauthorized();
  }
}).AddEndpointFilter<AuthFilter>();

app.Run();

