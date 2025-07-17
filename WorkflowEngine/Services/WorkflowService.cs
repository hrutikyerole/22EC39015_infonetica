using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class WorkflowService
{
    private Dictionary<string, WorkflowDefinition> _definitions = new();
    private Dictionary<string, WorkflowInstance> _instances = new();

    private const string DefFile = "workflow_definitions.json";
    private const string InstFile = "workflow_instances.json";

    public WorkflowService()
    {
        Load();
    }

    public WorkflowDefinition CreateDefinition(WorkflowDefinition definition)
    {
        if (definition.States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
            throw new Exception("Duplicate state IDs are not allowed.");
        if (definition.Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))
            throw new Exception("Duplicate action IDs are not allowed.");
        if (definition.States.Count(s => s.IsInitial) != 1)
            throw new Exception("Exactly one initial state is required.");

        _definitions[definition.Id] = definition;
        Save();
        return definition;
    }

    public WorkflowDefinition? GetDefinition(string id)
    {
        return _definitions.TryGetValue(id, out var def) ? def : null;
    }

    public WorkflowInstance StartInstance(string definitionId)
    {
        if (!_definitions.ContainsKey(definitionId))
            throw new Exception("Workflow definition not found.");

        var def = _definitions[definitionId];
        var initial = def.States.First(s => s.IsInitial && s.Enabled);

        var instance = new WorkflowInstance
        {
            DefinitionId = definitionId,
            CurrentStateId = initial.Id
        };

        _instances[instance.Id] = instance;
        Save();
        return instance;
    }

    public WorkflowInstance? GetInstance(string instanceId)
    {
        return _instances.TryGetValue(instanceId, out var instance) ? instance : null;
    }

    public WorkflowInstance ExecuteAction(string instanceId, string actionId)
    {
        if (!_instances.TryGetValue(instanceId, out var instance))
            throw new Exception("Instance not found.");

        var def = _definitions[instance.DefinitionId];
        var action = def.Actions.FirstOrDefault(a => a.Id == actionId);

        if (action == null) throw new Exception("Action not found.");
        if (!action.Enabled) throw new Exception("Action is disabled.");
        if (!action.FromStates.Contains(instance.CurrentStateId))
            throw new Exception("Invalid source state for this action.");

        var toState = def.States.FirstOrDefault(s => s.Id == action.ToState);
        if (toState == null || !toState.Enabled)
            throw new Exception("Target state is invalid or disabled.");

        var currentState = def.States.First(s => s.Id == instance.CurrentStateId);
        if (currentState.IsFinal)
            throw new Exception("Cannot perform action from a final state.");

        // Perform the transition
        instance.CurrentStateId = toState.Id;
        instance.History.Add(new ActionHistory
        {
            ActionId = actionId,
            Timestamp = DateTime.UtcNow
        });


        Save();
        return instance;
    }

    private void Save()
    {
        File.WriteAllText(DefFile, JsonSerializer.Serialize(_definitions, new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(InstFile, JsonSerializer.Serialize(_instances, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void Load()
    {
        if (File.Exists(DefFile))
        {
            var defJson = File.ReadAllText(DefFile);
            var defs = JsonSerializer.Deserialize<Dictionary<string, WorkflowDefinition>>(defJson);
            if (defs != null) _definitions = defs;
        }

        if (File.Exists(InstFile))
        {
            var instJson = File.ReadAllText(InstFile);
            var insts = JsonSerializer.Deserialize<Dictionary<string, WorkflowInstance>>(instJson);
            if (insts != null) _instances = insts;
        }
    }
}
