"""Transform tools: set position, rotation, scale on GameObjects."""

from mcp.server.fastmcp import FastMCP

from ..unity_client import send_command


def register(mcp: FastMCP):
    @mcp.tool()
    def set_transform(
        object_path: str,
        position: list[float] | None = None,
        rotation: list[float] | None = None,
        scale: list[float] | None = None,
        local_space: bool = True,
    ) -> str:
        """Set position, rotation, and/or scale on a GameObject in one call.
        Only provided values are changed.

        Args:
            object_path: Hierarchy path of the object.
            position: Position as [x, y, z]. Omit to leave unchanged.
            rotation: Euler rotation as [x, y, z]. Omit to leave unchanged.
            scale: Local scale as [x, y, z]. Omit to leave unchanged.
            local_space: Use local space (true) or world space (false). Default true.
        """
        params = {"object_path": object_path, "local_space": local_space}
        if position is not None:
            params["position"] = position
        if rotation is not None:
            params["rotation"] = rotation
        if scale is not None:
            params["scale"] = scale
        result = send_command("set_transform", params)
        return _format(result)

    @mcp.tool()
    def set_position(
        object_path: str,
        position: list[float],
        local_space: bool = True,
    ) -> str:
        """Set position on a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            position: Position as [x, y, z].
            local_space: Use local space (true) or world space (false). Default true.
        """
        result = send_command("set_position", {
            "object_path": object_path,
            "position": position,
            "local_space": local_space,
        })
        return _format(result)

    @mcp.tool()
    def set_rotation(
        object_path: str,
        rotation: list[float],
        local_space: bool = True,
    ) -> str:
        """Set euler rotation on a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            rotation: Euler angles as [x, y, z].
            local_space: Use local space (true) or world space (false). Default true.
        """
        result = send_command("set_rotation", {
            "object_path": object_path,
            "rotation": rotation,
            "local_space": local_space,
        })
        return _format(result)

    @mcp.tool()
    def set_scale(object_path: str, scale: list[float]) -> str:
        """Set local scale on a GameObject.

        Args:
            object_path: Hierarchy path of the object.
            scale: Local scale as [x, y, z].
        """
        result = send_command("set_scale", {
            "object_path": object_path,
            "scale": scale,
        })
        return _format(result)


def _format(result: dict) -> str:
    status = "OK" if result.get("success") else "ERROR"
    return f"[{status}] {result.get('message', '')}"
