"""Asset tools: create and assign materials and physics materials."""

from mcp.server.fastmcp import FastMCP

from ..unity_client import send_command


def register(mcp: FastMCP):
    @mcp.tool()
    def create_material(
        name: str,
        save_path: str = "Assets/Materials",
        shader: str = "Universal Render Pipeline/Lit",
        color: list[float] | None = None,
        properties: dict | None = None,
    ) -> str:
        """Create a new Material asset in the Unity project.

        Args:
            name: Material name (e.g. "TableSurface").
            save_path: Folder path to save in (e.g. "Assets/Materials"). Created if missing.
            shader: Shader name. Default "Universal Render Pipeline/Lit".
                Falls back to "Standard" if URP not available.
            color: Base color as [r, g, b, a] with values 0-1. Default white.
            properties: Optional dict of extra shader properties.
                Float properties: {"_Smoothness": 0.8}
                Color properties: {"_EmissionColor": [1, 0, 0, 1]}
        """
        params = {"name": name, "save_path": save_path, "shader": shader}
        if color is not None:
            params["color"] = color
        if properties is not None:
            params["properties"] = properties
        result = send_command("create_material", params)
        return _format(result)

    @mcp.tool()
    def create_physics_material(
        name: str,
        save_path: str = "Assets/PhysicsMaterials",
        dynamic_friction: float = 0.4,
        static_friction: float = 0.4,
        bounciness: float = 0.0,
        friction_combine: str = "Average",
        bounce_combine: str = "Average",
    ) -> str:
        """Create a new PhysicsMaterial asset in the Unity project.

        Args:
            name: Material name (e.g. "BouncyBall").
            save_path: Folder path to save in. Created if missing.
            dynamic_friction: Dynamic friction coefficient (0-1).
            static_friction: Static friction coefficient (0-1).
            bounciness: Bounciness (0 = no bounce, 1 = full bounce).
            friction_combine: How friction is combined: Average, Minimum, Maximum, Multiply.
            bounce_combine: How bounciness is combined: Average, Minimum, Maximum, Multiply.
        """
        result = send_command("create_physics_material", {
            "name": name,
            "save_path": save_path,
            "dynamic_friction": dynamic_friction,
            "static_friction": static_friction,
            "bounciness": bounciness,
            "friction_combine": friction_combine,
            "bounce_combine": bounce_combine,
        })
        return _format(result)

    @mcp.tool()
    def assign_material(object_path: str, material_path: str) -> str:
        """Assign a Material to a GameObject's Renderer.

        Args:
            object_path: Hierarchy path of the object with a Renderer.
            material_path: Asset path of the material (e.g. "Assets/Materials/TableSurface.mat").
        """
        result = send_command("assign_material", {
            "object_path": object_path,
            "material_path": material_path,
        })
        return _format(result)

    @mcp.tool()
    def assign_physics_material(
        object_path: str,
        physics_material_path: str,
        collider_type: str = "",
    ) -> str:
        """Assign a PhysicsMaterial to a GameObject's Collider.

        Args:
            object_path: Hierarchy path of the object with a Collider.
            physics_material_path: Asset path (e.g. "Assets/PhysicsMaterials/BouncyBall.physicMaterial").
            collider_type: Specific collider type if multiple exist (e.g. "SphereCollider").
                Empty = use first collider found.
        """
        result = send_command("assign_physics_material", {
            "object_path": object_path,
            "physics_material_path": physics_material_path,
            "collider_type": collider_type,
        })
        return _format(result)


def _format(result: dict) -> str:
    status = "OK" if result.get("success") else "ERROR"
    msg = result.get("message", "")
    data = result.get("data")
    parts = [f"[{status}] {msg}"]
    if data:
        parts.append(f"Data: {data}")
    return "\n".join(parts)
