using Microsoft.EntityFrameworkCore;

public class User
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public string Username { get; set; }
  public byte[] PasswordHash { get; set; }
  public byte[] PasswordSalt { get; set; }
  public ICollection<Project> Projects { get; } = new List<Project>();
}

public class Project
{
  public int Id { get; set; }
  public string Title { get; set; }
  public string? Description { get; set; }
  public DateTime DateCreated { get; set; } = DateTime.Now.ToUniversalTime();
  public int UserId { get; set; }
  public User? User { get; set; }
  public ICollection<Todo> Todos { get; } = new List<Todo>();
}

public class Todo
{
  public int Id { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  public DateTime? DueDate { get; set; }
  public string? Priority { get; set; }
  public string? Notes { get; set; }
  public bool Done { get; set; } = false;
  public int ProjectId { get; set; }
  public Project? Project { get; set; }
}

class TodoListDB : DbContext
{
  public TodoListDB(DbContextOptions<TodoListDB> options) : base(options) { }
  public DbSet<Project> Projects { get; set; }
  public DbSet<Todo> Todos { get; set; }
  public DbSet<User> Users { get; set; }
}