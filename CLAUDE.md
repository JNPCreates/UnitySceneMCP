# Unity Scene MCP Project

This project is a custom MCP server + Unity Editor bridge for AI-driven scene setup.

## When writing setup guides

When asked to create a setup guide for a game or scene, ALWAYS follow the format defined in `Supportingdocs/SetupGuideSpec.md`. Key rules:

- Every step needs a human-readable description AND a `commands:` YAML block with exact MCP tool calls
- Every position, rotation, and scale must have exact [x, y, z] values — never "adjust as needed"
- Create parent objects before children
- Create materials/physics materials before assigning them
- Use consistent object paths with `/` separator (e.g. "GameArea/Table")
- Do NOT include C# script creation in setup guides — only scene structure

Read the full spec at `Supportingdocs/SetupGuideSpec.md` before writing any setup guide.

## When executing setup guides

- Read the setup guide file first
- Execute the `commands:` YAML blocks step by step using the MCP tools
- After each step, use `get_hierarchy` to verify objects were created correctly
- If a command fails, stop and report the error — don't skip ahead

## Available MCP tools (unity-scene server)

Scene graph: create_gameobject, create_primitive, set_parent, delete_gameobject, set_layer, set_tag
Transforms: set_transform, set_position, set_rotation, set_scale
Components: add_component, set_component_field, remove_component
Assets: create_material, create_physics_material, assign_material, assign_physics_material
Query: get_hierarchy
