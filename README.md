# Unity Scene MCP

Unity Scene MCP is a two-part bridge for controlling a Unity Editor scene from an MCP client:

1. A Python MCP server that exposes scene tools over stdio.
2. A Unity Editor bridge script that listens on `http://localhost:9877/` and applies those commands inside Unity.

## Requirements

- Python 3.10 or newer
- Unity Editor
- An MCP client that can launch stdio MCP servers

## Install From GitHub On A Fresh Computer

Install it with:

```powershell
py -m pip install "git+https://github.com/JNPCreates/UnitySceneMCP.git"
```

If you prefer an isolated command-line install:

```powershell
py -m pip install pipx
py -m pipx install "git+https://github.com/JNPCreates/UnitySceneMCP.git"
```

## Install The Unity Bridge

Run the included helper and pass the root folder of your Unity project:

```powershell
unity-scene-mcp-install-bridge "C:\Path\To\YourUnityProject"
```

If Windows says the command is not found, use the module form instead:

```powershell
py -m mcp_server.install_bridge "C:\Path\To\YourUnityProject"
```

This copies `MCPBridge.cs` into:

```text
Assets/Editor/MCPBridge.cs
```

Open the Unity project and let Unity compile. Then open:

```text
Tools > MCP Bridge > Open
```

Click `Start`. The bridge should show that it is running on `http://localhost:9877/`.

## Add The MCP Server To Codex

The installed server command is:

```powershell
unity-scene-mcp
```

Add this to your Codex config file at `C:\Users\YOURNAME\.codex\config.toml`:

```toml
[mcp_servers.unity-scene]
command = "unity-scene-mcp"
enabled = true
```

If your Python Scripts folder is not on `PATH`, use Python's module launcher:

```toml
[mcp_servers.unity-scene]
command = "py"
args = ["-m", "mcp_server"]
enabled = true
```

You can also copy `.mcp.example.json` to `.mcp.json` in a local workspace if your MCP client reads project-local MCP config files:

```json
{
  "mcpServers": {
    "unity-scene": {
      "command": "py",
      "args": ["-m", "mcp_server"]
    }
  }
}
```

## Test It

With Unity open and the bridge started, ask your MCP client to run:

```text
Use the create_primitive tool to create a cube named "TestCube" at position [0, 1, 0].
```

Then inspect the scene:

```text
Use get_hierarchy to show me what's in the scene.
```

## Local Development

Clone the repo, create a virtual environment, and install in editable mode:

```powershell
git clone https://github.com/JNPCreates/UnitySceneMCP.git
cd UnitySceneMCP
py -m venv .venv
.\.venv\Scripts\Activate.ps1
py -m pip install -e .
```

Run the MCP server directly:

```powershell
unity-scene-mcp
```

Or:

```powershell
py -m mcp_server
```

## Upload This Folder To GitHub

From this repository folder:

```powershell
git init
git add .
git commit -m "Initial Unity Scene MCP package"
git branch -M main
git remote add origin https://github.com/JNPCreates/UnitySceneMCP.git
git push -u origin main
```

Do not commit `.mcp.json`; it can contain machine-specific paths. Use `.mcp.example.json` as the portable template.

## Available Tool Groups

- Scene graph: create/delete GameObjects and primitives, parent objects, set tags and layers.
- Transforms: set position, rotation, scale, or all transform values at once.
- Components: add/remove components and set serialized fields.
- Assets: create/assign materials and physics materials.
- Query: read the current Unity hierarchy.

## Troubleshooting

If commands cannot connect to Unity, verify that the Unity project is open, the bridge window is open, and the bridge has been started.

If the MCP command is not found after installing with `pip`, make sure your Python Scripts folder is on `PATH`, or use `pipx`.
