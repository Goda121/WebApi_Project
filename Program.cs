// Program.cs
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// app.UseHttpsRedirection();

app.MapGet("/api/items", () =>
{
    return Results.Ok(InMemoryDb.Items);
});

app.MapGet("/api/items/{id}", (int id) =>
{
    var item = InMemoryDb.Items.FirstOrDefault(i => i.Id == id);
    if (item is null)
    {
        return Results.NotFound(new { message = "Item not found" });
    }
    return Results.Ok(item);
});

app.MapPost("/api/items", ([FromBody] Item newItem) =>
{
    if (string.IsNullOrWhiteSpace(newItem.Name) || string.IsNullOrWhiteSpace(newItem.Description))
    {
        return Results.BadRequest(new { message = "Name and description are required." });
    }

    newItem.Id = InMemoryDb.GetNextId();
    InMemoryDb.Items.Add(newItem);

    return Results.Created($"/api/items/{newItem.Id}", newItem);
});

app.MapPut("/api/items/{id}", (int id, [FromBody] Item updatedItem) =>
{
    var itemToUpdate = InMemoryDb.Items.FirstOrDefault(i => i.Id == id);
    if (itemToUpdate is null)
    {
        return Results.NotFound(new { message = "Item not found" });
    }

    itemToUpdate.Name = updatedItem.Name ?? itemToUpdate.Name;
    itemToUpdate.Description = updatedItem.Description ?? itemToUpdate.Description;

    return Results.Ok(itemToUpdate);
});

app.MapDelete("/api/items/{id}", (int id) =>
{
    var initialCount = InMemoryDb.Items.Count;
    InMemoryDb.Items.RemoveAll(i => i.Id == id);

    if (InMemoryDb.Items.Count < initialCount)
    {
        return Results.NoContent();
    }
    else
    {
        return Results.NotFound(new { message = "Item not found" });
    }
});

app.Run();

public class Item
{
   
    public required int Id { get; set; }
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string? Name { get; set; }
[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}

public static class InMemoryDb
{
    private static int _nextId = 4;
    public static List<Item> Items { get; } = new List<Item>
    {
        new Item { Id = 1, Name = "Laptop", Description = "A powerful computing device." },
        new Item { Id = 2, Name = "Keyboard", Description = "Mechanical keyboard with RGB lighting." },
        new Item { Id = 3, Name = "Mouse", Description = "Ergonomic wireless mouse." }
    };

    public static  int GetNextId() => _nextId++;
}
