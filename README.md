# 22EC39015_infonetica

# Configurable Workflow Engine (State-Machine API)

This is a minimal backend service built in **.NET 8 / C#** that allows you to:

- Define workflows as configurable state machines (states + actions)
- Start workflow instances based on definitions
- Execute actions to move instances between states (with validation)
- Inspect workflow definitions and instance history

---

## Quick Start

### 1. Clone the repo

 ⁠bash
git clone https://github.com/your-username/workflow-engine.git
cd workflow-engine
⁠ `

### 2. Run the project

 ⁠bash
dotnet run


The API will start on:


http://localhost:5000


⁠ ---

## Features

| Area            | Description                                                                                      |
| --------------- | ------------------------------------------------------------------------------------------------ |
| **Definition**  | Create workflows with states and actions                                                         |
| **Runtime**     | Start instances, execute actions                                                                 |
| **Validation**  | Ensures correct transitions, prevents invalid operations                                         |
| **Persistence** | Automatically saved to local JSON files (`workflow_definitions.json`, `workflow_instances.json`) |

---

## API Endpoints

### 1. Create a workflow

`POST /workflows`

**Body:**

```{
  "id": "leave-approval",
  "states": [
    { "id": "start", "name": "Start", "isInitial": true, "isFinal": false, "enabled": true },
    { "id": "manager_approval", "name": "Manager Approval", "isInitial": false, "isFinal": false, "enabled": true },
    { "id": "approved", "name": "Approved", "isInitial": false, "isFinal": true, "enabled": true },
    { "id": "rejected", "name": "Rejected", "isInitial": false, "isFinal": true, "enabled": true }
  ],
  "actions": [
    {
      "id": "submit",
      "name": "Submit Leave",
      "enabled": true,
      "fromStates": ["start"],
      "toState": "manager_approval"
    },
    {
      "id": "approve",
      "name": "Approve Leave",
      "enabled": true,
      "fromStates": ["manager_approval"],
      "toState": "approved"
    },
    {
      "id": "reject",
      "name": "Reject Leave",
      "enabled": true,
      "fromStates": ["manager_approval"],
      "toState": "rejected"
    }
  ]
}
```


---

### 2. Get a workflow definition

`GET /workflows/{id}`

---

### 3. Start a new instance

`POST /instances/{workflowId}`

---

### 4. Execute an action

`POST /instances/{instanceId}/actions/{actionId}`

---

### 5. Get instance state & history

`GET /instances/{instanceId}`

---

## Example Flow

1. Create the `leave-approval` workflow (with actions like `submit`, `approve`, `reject`)
2. Start an instance → initial state will be `start`
3. Call `/actions/submit` to move to `manager_approval`
4. Call `/actions/approve` or `/actions/reject` to reach a final state
5. Get instance history at any point

---

## File-based Persistence

* All definitions and instances are automatically saved to:

  * `workflow_definitions.json`
  * `workflow_instances.json`

No database required.

---

## Assumptions & Notes

* Only **one initial state** per workflow is allowed.
* Actions must originate from **enabled** states and go to **enabled** target states.
* Final states **cannot be transitioned from**.
* Workflow/action IDs must be unique.
* Data is **reloaded from JSON files** when the app restarts.

---

## Testing

Use **Postman** or `curl` to test the endpoints.
You can also import the [Postman Collection](./postman_collection.json) if provided.

---

## Project Structure


WorkflowEngine/
├── Models/               # State, Action, WorkflowDefinition, WorkflowInstance, ActionHistory
├── Services/             # WorkflowService (business logic)
├── Program.cs            # API routes (Minimal API)
└── README.md


---

## License

MIT (or as per your submission requirements)

---
