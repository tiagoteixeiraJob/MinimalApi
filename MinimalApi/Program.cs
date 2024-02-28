using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

async Task<List<Student>> GetStudents(DataContext context) => await context.Students.ToListAsync();

app.MapGet("/Student", async (DataContext context) => await context.Students.ToListAsync())
.WithName("GetStudent")
.WithOpenApi();

app.MapGet("/Student/{id}", async (DataContext context, int id) => await context.Students.FindAsync(id) is Student student ? Results.Ok(student) : Results.NotFound("Student Not Found!"))
.Produces<Student>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetStudentById")
.WithOpenApi();

app.MapPost("Add/Student", async (DataContext context, Student item) =>
{
    context.Students.Add(item);
    await context.SaveChangesAsync();
    return Results.Ok(await GetStudents(context));
})
.Produces<Student>(StatusCodes.Status201Created) 
.Produces(StatusCodes.Status400BadRequest)
.WithName("AddStudent")
.WithOpenApi();

app.MapPut("/Student/{id}", async (DataContext context, Student item, int id) =>
{
    var studentItem = await context.Students.FindAsync(id);

    if (studentItem == null)
    {
        return Results.NotFound("Student Not Found!");
    }

    studentItem.FirstName = item.FirstName;
    studentItem.LastName = item.LastName;

    context.Students.Update(studentItem);
    await context.SaveChangesAsync();

    return Results.Ok(await GetStudents(context));
})
.WithName("UpdateStudentById")
.WithOpenApi();

app.MapDelete("/Student/{id}", async (DataContext context, int id) =>
{
    var studentItem = await context.Students.FindAsync(id);

    if (studentItem == null)
    {
        return Results.NotFound("Student Not Found!");
    }

    context.Remove(studentItem);
    await context.SaveChangesAsync();

    return Results.Ok(await GetStudents(context));
})
.WithName("DeleteStudentById")
.WithOpenApi();

app.Run();