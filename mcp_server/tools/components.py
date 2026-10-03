"""Component tools: add, remove, and set fields on components."""

from mcp.server.fastmcp import FastMCP

from ..unity_client import send_command


def register(mcp: FastMCP):
    @mcp.tool()
    def add_component(object_path: str, component_type: str) -> str:
        """Add a component to a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            component_type: Component type name. Built-in types: Rigidbody, BoxCollider,
                SphereCollider, CapsuleCollider, MeshCollider, AudioSource, Light, Camera,
                Canvas, etc. Also accepts custom script names (e.g. "BeerPongGameManager").
        """
        result = send_command("add_component", {
            "object_path": object_path,
            "component_type": component_type,
        })
        return _format(result)

    @mcp.tool()
    def set_component_field(
        object_path: str,
        component_type: str,
        field_name: str,
        value: object,
    ) -> str:
        """Set a field or property on a component.

        Args:
            object_path: Hierarchy path of the object.
            component_type: Component type name (e.g. "Rigidbody", "BoxCollider").
            field_name: Field name as it appears in the Unity Inspector / serialization.
                Common examples: "m_IsTrigger", "mass", "drag", "useGravity".
                For serialized fields use the Unity internal name (m_ prefix for built-in).
            value: The value to set. Type depends on the field:
                - float/int: number (e.g. 1.5, 10)
                - bool: true/false
                - string: "text"
                - Vector3: [x, y, z] (e.g. [0, 1, 0])
                - Vector2: [x, y]
                - Color: [r, g, b, a] with values 0-1 (e.g. [1, 0, 0, 1] for red)
                - Enum: string name (e.g. "Discrete" for collision detection)
                - Object reference: "go:HierarchyPath" for GameObjects,
                  "asset:Assets/path.ext" for assets
        """
        result = send_command("set_component_field", {
            "object_path": object_path,
            "component_type": component_type,
            "field_name": field_name,
            "value": value,
        })
        return _format(result)

    @mcp.tool()
    def remove_component(object_path: str, component_type: str) -> str:
        """Remove a component from a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            component_type: Component type name to remove (e.g. "BoxCollider").
        """
        result = send_command("remove_component", {
            "object_path": object_path,
            "component_type": component_type,
        })
        return _format(result)


def _format(result: dict) -> str:
    status = "OK" if result.get("success") else "ERROR"
    return f"[{status}] {result.get('message', '')}"
