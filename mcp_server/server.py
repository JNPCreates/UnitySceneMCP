"""Unity Scene MCP Server exposes Unity scene manipulation tools via MCP."""

import os
import sys

from mcp.server.fastmcp import FastMCP

# Ensure the project root is on the path so relative imports work when launched
# directly via `python mcp_server/server.py`.
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from mcp_server.tools import assets, components, query, scene_graph, transforms


def create_server() -> FastMCP:
    mcp = FastMCP(
        "Unity Scene MCP",
        instructions=(
            "This MCP server controls a Unity Editor scene. "
            "Use these tools to create GameObjects, primitives, set transforms, "
            "add/configure components, create/assign materials, and inspect the hierarchy. "
            "The Unity Editor must be open with the MCP Bridge window running "
            "(Tools > MCP Bridge > Open, then click Start). "
            "All object_path parameters use '/' as separator, e.g. 'Parent/Child/Grandchild'."
        ),
    )

    scene_graph.register(mcp)
    transforms.register(mcp)
    components.register(mcp)
    assets.register(mcp)
    query.register(mcp)
    return mcp


mcp = create_server()


def main() -> None:
    mcp.run(transport="stdio")


if __name__ == "__main__":
    main()
