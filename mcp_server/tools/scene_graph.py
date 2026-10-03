"""Scene graph tools: create/delete GameObjects, primitives, tags, layers, parenting."""

from mcp.server.fastmcp import FastMCP

from ..unity_client import send_command


def register(mcp: FastMCP):
    @mcp.tool()
    def create_gameobject(
        name: str,
        parent_path: str = "",
        position: list[float] | None = None,
        rotation: list[float] | None = None,
        scale: list[float] | None = None,
    ) -> str:
        """Create an empty GameObject in the Unity scene.

        Args:
            name: Name for the new GameObject.
            parent_path: Hierarchy path of the parent (e.g. "Safe/DoorPivot"). Empty = scene root.
            position: Local position as [x, y, z]. Default [0,0,0].
            rotation: Local euler rotation as [x, y, z]. Default [0,0,0].
            scale: Local scale as [x, y, z]. Default [1,1,1].
        """
        params = {"name": name, "parent_path": parent_path}
        if position is not None:
            params["position"] = position
        if rotation is not None:
            params["rotation"] = rotation
        if scale is not None:
            params["scale"] = scale
        result = send_command("create_gameobject", params)
        return _format(result)

    @mcp.tool()
    def create_primitive(
        name: str,
        primitive_type: str,
        parent_path: str = "",
        position: list[float] | None = None,
        rotation: list[float] | None = None,
        scale: list[float] | None = None,
    ) -> str:
        """Create a Unity primitive (Cube, Sphere, Cylinder, Capsule, Plane, Quad).

        Args:
            name: Name for the new object.
            primitive_type: One of: Cube, Sphere, Cylinder, Capsule, Plane, Quad.
            parent_path: Hierarchy path of the parent. Empty = scene root.
            position: Local position as [x, y, z].
            rotation: Local euler rotation as [x, y, z].
            scale: Local scale as [x, y, z].
        """
        params = {"name": name, "primitive_type": primitive_type, "parent_path": parent_path}
        if position is not None:
            params["position"] = position
        if rotation is not None:
            params["rotation"] = rotation
        if scale is not None:
            params["scale"] = scale
        result = send_command("create_primitive", params)
        return _format(result)

    @mcp.tool()
    def set_parent(
        object_path: str,
        new_parent_path: str = "",
        world_position_stays: bool = True,
    ) -> str:
        """Reparent a GameObject to a new parent.

        Args:
            object_path: Hierarchy path of the object to move.
            new_parent_path: Hierarchy path of new parent. Empty = move to scene root.
            world_position_stays: If true, keep world position. If false, keep local position.
        """
        result = send_command("set_parent", {
            "object_path": object_path,
            "new_parent_path": new_parent_path,
            "world_position_stays": world_position_stays,
        })
        return _format(result)

    @mcp.tool()
    def delete_gameobject(object_path: str) -> str:
        """Delete a GameObject from the scene.

        Args:
            object_path: Hierarchy path of the object to delete (e.g. "Safe/DoorPivot").
        """
        result = send_command("delete_gameobject", {"object_path": object_path})
        return _format(result)

    @mcp.tool()
    def set_layer(
        object_path: str,
        layer_name: str,
        include_children: bool = True,
    ) -> str:
        """Set the layer on a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            layer_name: Name of the layer (must already exist in Unity).
            include_children: Also set layer on all children. Default true.
        """
        result = send_command("set_layer", {
            "object_path": object_path,
            "layer_name": layer_name,
            "include_children": include_children,
        })
        return _format(result)

    @mcp.tool()
    def set_tag(object_path: str, tag_name: str) -> str:
        """Set the tag on a GameObject. Auto-creates the tag if it doesn't exist.

        Args:
            object_path: Hierarchy path of the object.
            tag_name: Tag name (e.g. "Ball", "Table"). Created automatically if missing.
        """
        result = send_command("set_tag", {
            "object_path": object_path,
            "tag_name": tag_name,
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
