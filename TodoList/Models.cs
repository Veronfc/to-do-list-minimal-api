using Microsoft.EntityFrameworkCore;

public partial class Project
{
  public int Id { get; set;}
  public string Title { get; set; }
  public string? Description { get; set; }
  public DateTime DateCreated { get; set; }
  public ICollection<Todo> Todos { get; } = new List<Todo>();
}

public partial class Todo
{
  public int Id { get; set;}
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
}