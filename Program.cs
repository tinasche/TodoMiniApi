using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(options =>
{
    options.UseInMemoryDatabase("TodoList");
});

var app = builder.Build();

app.MapGet("/", () => $"Hello {Environment.UserName}");

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

app.MapGet("/todoitems/done", async (TodoDb db) => await db.Todos.Where(t => t.IsDone == true).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    await db.Todos.AddAsync(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var checkedTodo = await db.Todos.FindAsync(id);
    if (checkedTodo is null) return Results.NotFound();

    // Needs extra check to validate inputTodo is same object returned by FindAsync 

    checkedTodo.Name = inputTodo.Name;
    checkedTodo.IsDone = inputTodo.IsDone;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();
});


app.Run();

internal class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos { get; set; } = null!;
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}