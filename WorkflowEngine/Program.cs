using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WorkflowService>();

var app = builder.Build();

app.MapGet("/", () => "Workflow Engine API is running!");

// Create definition
app.MapPost("/workflows", (WorkflowDefinition def, WorkflowService svc) =>
{
    try
    {
        var created = svc.CreateDefinition(def);
        return Results.Ok(created);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Get definition
app.MapGet("/workflows/{id}", (string id, WorkflowService svc) =>
{
    var def = svc.GetDefinition(id);
    return def is not null ? Results.Ok(def) : Results.NotFound();
});

// Start instance
app.MapPost("/instances/{workflowId}", (string workflowId, WorkflowService svc) =>
{
    try
    {
        var instance = svc.StartInstance(workflowId);
        return Results.Ok(instance);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Get instance state and history
app.MapGet("/instances/{id}", (string id, WorkflowService svc) =>
{
    var inst = svc.GetInstance(id);
    return inst is not null ? Results.Ok(inst) : Results.NotFound();
});

// Execute action on instance
app.MapPost("/instances/{id}/actions/{actionId}", (string id, string actionId, WorkflowService svc) =>
{
    try
    {
        var updated = svc.ExecuteAction(id, actionId);
        return Results.Ok(updated);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
