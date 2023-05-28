using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(options =>
{
    options.UseInMemoryDatabase("TodoList");
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "TodoItems Api",
        Version = "v1",
        Contact = new OpenApiContact { Email = "tinashec93@live.com", Name = "Tinashe Chitakunye" },
        Description = "Simple api to manage TodoItems"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName}-v1"));

app.MapGet("/", () => $"Hello {Environment.UserName}. Visit the /todoitems endpoint to play with the api for a bit!").ExcludeFromDescription();

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync()).WithName("GetTodoItems");

app.MapGet("/todoitems/done", async (TodoDb db) => await db.Todos.Where(t => t.IsDone == true).ToListAsync()).WithName("GetDoneTodos");

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is Todo todo ?
    Results.Ok(todo) : Results.NotFound())
    .WithName("GetTodoById");

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    await db.Todos.AddAsync(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
}).WithName("AddTodoItem");

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var checkedTodo = await db.Todos.FindAsync(id);
    if (checkedTodo is null) return Results.NotFound();

    // Needs extra check to validate inputTodo is same object returned by FindAsync 
    checkedTodo.Name = inputTodo.Name;
    checkedTodo.IsDone = inputTodo.IsDone;

    await db.SaveChangesAsync();

    return Results.NoContent();
}).WithName("UpdateTodoItem");

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();
}).WithName("RemoveTodoItem");


app.Run();

internal class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }
    public DbSet<Todo> Todos => Set<Todo>();
}

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsDone { get; set; }
}
