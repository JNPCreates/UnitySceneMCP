# Setup Guide Spec — How to Write Machine-Readable Setup Guides

This document defines the standard format for writing Unity scene setup guides that are both **human-readable** (you can follow them manually) and **machine-executable** (the MCP server can execute them automatically via tool calls).

Feed this document to any AI when asking it to write a setup guide for a new game/scene.

---

## Rules for AI Writing Setup Guides

1. **Every setup guide has two sections per step**: a human-readable description, then a `commands:` block with the exact MCP tool calls.
2. **One command per line** in the commands block. Each command maps to exactly one MCP tool call.
3. **Use only the available tools** (listed below). Do not invent tools.
4. **Be explicit** — every position, rotation, and scale must have exact `[x, y, z]` values. Never say "adjust as needed."
5. **Use consistent object paths** — after creating `"Table"` as a child of `"GameArea"`, reference it as `"GameArea/Table"` in all later commands.
6. **Group commands by logical step** — one step = one logical unit of work (e.g., "Create the table," "Set up the ball").
7. **Create parent objects first** — always create a parent before creating children under it.
8. **Create assets before assigning them** — create a material before assigning it to an object.
9. **Do NOT include C# script creation** — setup guides only handle scene structure, not gameplay code. Scripts are written separately and attached with `add_component`.

---

## Available MCP Tools

### Scene Graph
- `create_gameobject(name, parent_path?, position?, rotation?, scale?)`
- `create_primitive(name, primitive_type, parent_path?, position?, rotation?, scale?)`
  - primitive_type: Cube, Sphere, Cylinder, Capsule, Plane, Quad
- `set_parent(object_path, new_parent_path, world_position_stays?)`
- `delete_gameobject(object_path)`
- `set_layer(object_path, layer_name, include_children?)`
- `set_tag(object_path, tag_name)`

### Transforms
- `set_transform(object_path, position?, rotation?, scale?, local_space?)`
- `set_position(object_path, position, local_space?)`
- `set_rotation(object_path, rotation, local_space?)`
- `set_scale(object_path, scale)`

### Components
- `add_component(object_path, component_type)`
- `set_component_field(object_path, component_type, field_name, value)`
- `remove_component(object_path, component_type)`

### Assets
- `create_material(name, save_path?, shader?, color?, properties?)`
- `create_physics_material(name, save_path?, dynamic_friction?, static_friction?, bounciness?, friction_combine?, bounce_combine?)`
- `assign_material(object_path, material_path)`
- `assign_physics_material(object_path, physics_material_path, collider_type?)`

### Query (read-only)
- `get_hierarchy(root_path?, depth?)`

---

## Setup Guide Template

```markdown
# [Game Name] — Phase [N] Setup Guide

## Overview
- **Game:** [name]
- **Scene:** [scene name]
- **Description:** [one sentence]

---

## Step [N]: [Short description of what this step creates]

[Human-readable description of what we're building and why.
Include any relevant measurements, positions, or design notes.]

### Objects Created
| Name | Type | Parent | Position | Rotation | Scale |
|------|------|--------|----------|----------|-------|
| ... | Cube/Empty/etc | ... | [x,y,z] | [x,y,z] | [x,y,z] |

### Commands
```yaml
commands:
  - tool: create_gameobject
    params:
      name: "GameArea"
      position: [0, 0, 0]

  - tool: create_primitive
    params:
      name: "Table"
      primitive_type: "Cube"
      parent_path: "GameArea"
      position: [0, 0.75, 3]
      scale: [0.6, 0.02, 2.4]

  - tool: set_tag
    params:
      object_path: "GameArea/Table"
      tag_name: "Table"

  - tool: create_physics_material
    params:
      name: "TableSurface"
      save_path: "Assets/PhysicsMaterials"
      bounciness: 0.3
      bounce_combine: "Maximum"

  - tool: assign_physics_material
    params:
      object_path: "GameArea/Table"
      physics_material_path: "Assets/PhysicsMaterials/TableSurface.physicMaterial"
```

---

## Step [N+1]: [Next step]
[Continue the same pattern...]
```

---

## Example: Minimal Scene Setup

Here's a complete minimal example showing the format:

```markdown
# Test Scene — Phase 1 Setup Guide

## Overview
- **Game:** Test Scene
- **Scene:** TestScene
- **Description:** A simple scene with a floor, a cube, and a bouncy ball.

---

## Step 1: Create the Floor

A large flat plane for objects to rest on.

### Objects Created
| Name | Type | Parent | Position | Rotation | Scale |
|------|------|--------|----------|----------|-------|
| Floor | Plane | (root) | [0, 0, 0] | [0, 0, 0] | [5, 1, 5] |

### Commands
```yaml
commands:
  - tool: create_primitive
    params:
      name: "Floor"
      primitive_type: "Plane"
      position: [0, 0, 0]
      scale: [5, 1, 5]

  - tool: set_tag
    params:
      object_path: "Floor"
      tag_name: "Ground"
```

---

## Step 2: Create a Cube on the Floor

A simple red cube sitting on the floor.

### Objects Created
| Name | Type | Parent | Position | Rotation | Scale |
|------|------|--------|----------|----------|-------|
| RedCube | Cube | (root) | [0, 0.5, 0] | [0, 0, 0] | [1, 1, 1] |

### Commands
```yaml
commands:
  - tool: create_primitive
    params:
      name: "RedCube"
      primitive_type: "Cube"
      position: [0, 0.5, 0]

  - tool: create_material
    params:
      name: "RedMat"
      save_path: "Assets/Materials"
      color: [1, 0, 0, 1]

  - tool: assign_material
    params:
      object_path: "RedCube"
      material_path: "Assets/Materials/RedMat.mat"
```

---

## Step 3: Create a Bouncy Ball

A sphere with physics that bounces when dropped.

### Objects Created
| Name | Type | Parent | Position | Rotation | Scale |
|------|------|--------|----------|----------|-------|
| Ball | Sphere | (root) | [0, 3, 0] | [0, 0, 0] | [0.3, 0.3, 0.3] |

### Commands
```yaml
commands:
  - tool: create_primitive
    params:
      name: "Ball"
      primitive_type: "Sphere"
      position: [0, 3, 0]
      scale: [0.3, 0.3, 0.3]

  - tool: add_component
    params:
      object_path: "Ball"
      component_type: "Rigidbody"

  - tool: create_physics_material
    params:
      name: "BouncyBall"
      save_path: "Assets/PhysicsMaterials"
      bounciness: 0.9
      bounce_combine: "Maximum"

  - tool: assign_physics_material
    params:
      object_path: "Ball"
      physics_material_path: "Assets/PhysicsMaterials/BouncyBall.physicMaterial"

  - tool: set_tag
    params:
      object_path: "Ball"
      tag_name: "Ball"
```
```

---

## Key Conventions

| Convention | Rule |
|---|---|
| Object paths | Use `/` separator: `"Parent/Child/Grandchild"` |
| Positions | Always `[x, y, z]` — Unity left-handed coords |
| Rotations | Euler angles `[x, y, z]` in degrees |
| Scales | Always `[x, y, z]` — `[1,1,1]` is default, can be omitted |
| Colors | `[r, g, b, a]` with values 0.0 to 1.0 |
| Asset paths | Always start with `"Assets/"` (e.g., `"Assets/Materials/Red.mat"`) |
| Physics material extension | `.physicMaterial` |
| Material extension | `.mat` |
| Tags | Auto-created if they don't exist |
| Component field names | Use Unity serialized names (e.g., `m_IsTrigger` for "Is Trigger") |
| Object references in fields | `"go:HierarchyPath"` for GameObjects, `"asset:Assets/path"` for assets |

---

## Prompt to Give AI When Requesting a New Setup Guide

Copy-paste this when asking an AI to write a setup guide for a new game:

> Write a Phase 1 setup guide for [GAME NAME]. Follow the Setup Guide Spec format exactly.
> Each step must have a human-readable description, an "Objects Created" table, and a
> `commands:` YAML block with exact MCP tool calls. Use precise positions, rotations,
> and scales — no "adjust as needed." Create parent objects before children. Create
> materials/physics materials before assigning them. Do not include C# script creation.
> Available tools: create_gameobject, create_primitive, set_parent, delete_gameobject,
> set_layer, set_tag, set_transform, set_position, set_rotation, set_scale, add_component,
> set_component_field, remove_component, create_material, create_physics_material,
> assign_material, assign_physics_material, get_hierarchy.
