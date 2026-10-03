# Unity Scene MCP — Project Plan

## What We're Building

A custom MCP server + Unity Editor bridge that lets an AI client (Claude Code, or eventually a local model) execute structured Unity scene setup commands — creating GameObjects, primitives, setting transforms, adding components, wiring inspector references, creating materials, etc.

This replaces the manual "follow a setup guide step by step in the Unity Editor" workflow. Instead, structured commands get sent to Unity and executed automatically.

The system has two halves:
1. **Setup Guide Spec** — a standard format for writing setup guides that are both human-readable AND machine-executable
2. **MCP Server + Unity Bridge** — the infrastructure that executes those guides

## Architecture

```
AI Client (Claude Code / local model)
    ↓ MCP protocol (stdio)
Python MCP Server (this project)
    ↓ HTTP requests to localhost:9877
Unity Editor Bridge (C# EditorWindow script)
    ↓ Unity API calls on main thread
Unity Scene
```

The MCP server and Unity run on the same machine. The MCP server is a Python process that exposes tools via MCP. The Unity bridge is a single C# Editor script that listens on `http://localhost:9877/` and executes commands using the Unity API.

## Project Structure

```
MyUnityMCP/
├── mcp_server/
│   ├── server.py                 # Main MCP server entry point
│   ├── unity_client.py           # HTTP client that talks to Unity bridge
│   └── tools/
│       ├── __init__.py
│       ├── scene_graph.py        # create_gameobject, create_primitive, set_parent, delete_gameobject, set_layer, set_tag
│       ├── transforms.py         # set_transform, set_position, set_rotation, set_scale
│       ├── components.py         # add_component, set_component_field, remove_component
│       ├── assets.py             # create_material, create_physics_material, assign_material, assign_physics_material
│       └── query.py              # get_hierarchy (read-only inspection)
│
├── unity_bridge/
│   └── MCPBridge.cs              # Single Unity Editor script — copy into any Unity project's Assets/Editor/ folder
│
├── Supportingdocs/
│   ├── SetupGuideSpec.md         # The template/rules for how to write machine-readable setup guides
│   └── PhaseSetups/              # Example setup guides (Beer Pong, Safe Cracking, Rock Climbing)
│
├── requirements.txt              # mcp, requests
├── .mcp.json                     # Claude Code MCP config
├── plan.md                       # This file
└── README.md
```

---

## Phase 1 — Core Infrastructure + Basic Tools (Current)

**Goal:** Get the round-trip working: Claude Code calls a tool → Python MCP server receives it → sends HTTP to Unity bridge → Unity creates/modifies objects in the scene → returns confirmation. Prove the concept with enough tools to set up a simple scene (GameObjects, primitives, transforms, tags).

### Phase 1 Tools (18 total)

#### Scene Graph Tools (scene_graph.py)
1. **create_gameobject** — Create an empty GameObject
   - `name: str`, `parent_path: str = ""`, `position: [x,y,z]`, `rotation: [x,y,z]`, `scale: [x,y,z]`
2. **create_primitive** — Create a Unity primitive (Cube, Sphere, Cylinder, Capsule, Plane, Quad)
   - `name: str`, `primitive_type: str`, `parent_path: str = ""`, `position: [x,y,z]`, `rotation: [x,y,z]`, `scale: [x,y,z]`
3. **set_parent** — Reparent a GameObject
   - `object_path: str`, `new_parent_path: str`, `world_position_stays: bool = true`
4. **delete_gameobject** — Delete a GameObject
   - `object_path: str`
5. **set_layer** — Set layer on a GameObject
   - `object_path: str`, `layer_name: str`, `include_children: bool = true`
6. **set_tag** — Set tag (auto-creates if it doesn't exist)
   - `object_path: str`, `tag_name: str`

#### Transform Tools (transforms.py)
7. **set_transform** — Set position, rotation, and/or scale in one call
   - `object_path: str`, `position: [x,y,z] = null`, `rotation: [x,y,z] = null`, `scale: [x,y,z] = null`, `local_space: bool = true`
8. **set_position** — Just position
   - `object_path: str`, `position: [x,y,z]`, `local_space: bool = true`
9. **set_rotation** — Just rotation
   - `object_path: str`, `rotation: [x,y,z]`, `local_space: bool = true`
10. **set_scale** — Just local scale
    - `object_path: str`, `scale: [x,y,z]`

#### Component Tools (components.py)
11. **add_component** — Add a component by type name
    - `object_path: str`, `component_type: str` (e.g. "Rigidbody", "BoxCollider", "AudioSource", or script name)
12. **set_component_field** — Set a field/property on a component via SerializedProperty
    - `object_path: str`, `component_type: str`, `field_name: str`, `value: any`
    - Value types: float, int, bool, string, Vector3 as `[x,y,z]`, Color as `[r,g,b,a]`, enum as string, object reference as `"go:path"` or `"asset:path"`
13. **remove_component** — Remove a component
    - `object_path: str`, `component_type: str`

#### Asset Tools (assets.py)
14. **create_material** — Create a material asset
    - `name: str`, `save_path: str`, `shader: str = "Universal Render Pipeline/Lit"`, `color: [r,g,b,a]`, `properties: dict = {}`
15. **create_physics_material** — Create a PhysicsMaterial asset
    - `name: str`, `save_path: str`, `dynamic_friction: float`, `static_friction: float`, `bounciness: float`, `friction_combine: str`, `bounce_combine: str`
16. **assign_material** — Assign a material to a renderer
    - `object_path: str`, `material_path: str`
17. **assign_physics_material** — Assign a physics material to a collider
    - `object_path: str`, `physics_material_path: str`, `collider_type: str = ""`

#### Query Tools (query.py)
18. **get_hierarchy** — Read-only: return the current scene hierarchy
    - `root_path: str = ""` (empty = entire scene), `depth: int = -1` (-1 = unlimited)
    - Returns: tree of GameObjects with names, paths, active components, transform values

### Phase 1 Deliverables
- [ ] MCPBridge.cs — complete with all 18 endpoints
- [ ] Python MCP server with all 18 tools
- [ ] requirements.txt + .mcp.json
- [ ] SetupGuideSpec.md — template for writing machine-readable setup guides
- [ ] Round-trip test: Claude Code creates a cube in Unity via MCP

---

## Phase 2 — UI Tools + Polish (Future)

**Goal:** Add tools for Unity UI creation (Canvas, Text, Buttons, etc.) since every setup guide has a UI section. UI objects need special handling (Canvas parent, RectTransform, EventSystem).

### Phase 2 Tools (planned)
- **create_canvas** — Create a World Space canvas with proper VR settings
- **create_ui_element** — Create Text (TMP), Button (TMP), Panel, Image under a canvas
- **set_rect_transform** — Set anchors, pivot, size, position on RectTransform
- **create_prefab** — Save a hierarchy as a prefab asset
- **instantiate_prefab** — Instantiate a prefab into the scene

### Phase 2 Also
- Error recovery / retry logic
- Batch execution (send multiple commands in one call for speed)
- Better status reporting in the Unity EditorWindow

---

## Phase 3 — Setup Guide Runner (Future)

**Goal:** Instead of the AI translating a setup guide into individual tool calls in conversation, create a "runner" that takes a structured setup guide file and executes all commands automatically.

### Phase 3 Tools (planned)
- **run_setup_guide** — Takes a path to a setup guide file, parses the `## MCP Commands` section, executes all commands in order
- **validate_setup_guide** — Dry-run a guide without executing, report any issues

---

## Unity Bridge (MCPBridge.cs) — Implementation Requirements

This is a single Unity Editor script. Key requirements:

1. **EditorWindow** that opens via menu: `Tools > MCP Bridge > Start`
2. Listens on `http://localhost:9877/` using `HttpListener`
3. All Unity API calls MUST execute on the main thread — use `EditorApplication.delayCall` or a `ConcurrentQueue<Action>` drained in `EditorApplication.update`
4. Each endpoint receives a JSON body with command params and returns JSON: `{ "success": bool, "message": str, "data": any }`
5. **Endpoint routing**: POST to `http://localhost:9877/{tool_name}`
6. **Object resolution**: `object_path` params resolved via `GameObject.Find()` for root, recursive child search for paths like `"Safe/DoorPivot/SafeDoor"`
7. **Component field setting**: Use `SerializedObject` + `SerializedProperty` for correctness (handles undo, prefab overrides). Reflection fallback if needed.
8. **Tag creation**: Auto-create tags via SerializedObject on TagManager asset
9. **Error handling**: Every command wrapped in try/catch. Never crash the bridge.
10. **Status display**: EditorWindow shows: running/stopped, last command, command count, recent log
11. **Thread safety**: HttpListener callback on threadpool thread → queue to main thread via ConcurrentQueue
12. **Undo support**: Use `Undo.RegisterCreatedObjectUndo()` and `Undo.RecordObject()` so everything is undoable

## Python MCP Server — Implementation Requirements

1. Uses the `mcp` Python SDK (`pip install mcp`)
2. Runs as stdio server (Claude Code connects via stdio)
3. Each tool registered with full parameter descriptions and type hints
4. `unity_client.py`: thin wrapper — POST to `http://localhost:9877/{tool_name}` with JSON body, return response
5. 5-second timeout per request, clear error if Unity bridge not running
6. Tool functions call `unity_client.send_command(tool_name, params)` and return the result

## .mcp.json

```json
{
  "mcpServers": {
    "unity-scene": {
      "command": "python",
      "args": ["mcp_server/server.py"],
      "cwd": "."
    }
  }
}
```

## Critical Implementation Notes

- **Keep it simple.** Each tool does one thing. No batching (yet), no transactions, no undo system beyond Unity's built-in Undo.
- **Object paths use `/` as separator**: `"BeerPongTable"` for root, `"Safe/DoorPivot/SafeDoor"` for nested.
- **set_component_field value types**: bridge must handle: float, int, bool, string, Vector3 `[x,y,z]`, Color `[r,g,b,a]`, object references (`"go:path"` or `"asset:path"`), enums (string name).
- **All transforms default to local space** since setup guides specify local transforms.
- **Bridge auto-starts** HTTP listener when EditorWindow opens, stops when closed. Toggle button in window.
- **Undo everything** — `Undo.RegisterCreatedObjectUndo()` and `Undo.RecordObject()` for all ops.

## Do NOT (Phase 1)

- Do not add tools beyond the 18 listed
- Do not add WebSocket, batching, or multi-command support
- Do not add authentication
- Do not create gameplay scripts — this is ONLY bridge + MCP infrastructure
- Do not modify any existing Unity project files
