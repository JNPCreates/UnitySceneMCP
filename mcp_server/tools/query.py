"""Query tools: read-only inspection of the Unity scene."""

import json

from mcp.server.fastmcp import FastMCP

from ..unity_client import send_command


def register(mcp: FastMCP):
    @mcp.tool()
    def get_hierarchy(
        root_path: str = "",
        depth: int = -1,
    ) -> str:
        """Get the current scene hierarchy as a tree. Read-only — does not modify anything.

        Use this to verify objects were created correctly, find object paths for
        subsequent commands, or inspect what components are attached.

        Args:
            root_path: Hierarchy path of root object to start from.
                Empty = return entire scene hierarchy.
            depth: How deep to recurse. -1 = unlimited. 0 = just the root object.
                1 = root + immediate children, etc.

        Returns:
            Tree structure with each node containing: name, path, active, tag, layer,
            position, rotation, scale, components list, and children.
        """
        result = send_command("get_hierarchy", {
            "root_path": root_path,
            "depth": depth,
        })
        if result.get("success") and result.get("data"):
            return json.dumps(result["data"], indent=2)
        status = "OK" if result.get("success") else "ERROR"
        return f"[{status}] {result.get('message', '')}"
