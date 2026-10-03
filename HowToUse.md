# How To Use - Unity Scene MCP With Codex

This MCP has two pieces:

1. A Python MCP server that Codex starts.
2. A Unity Editor bridge script that listens on `http://localhost:9877/`.

```text
Codex -> Python MCP Server -> Unity Editor Bridge -> Your Scene
```

## Fresh Computer Install

Install the MCP from GitHub:

```powershell
py -m pip install "git+https://github.com/JNPCreates/UnitySceneMCP.git"
```

If you prefer an isolated install:

```powershell
py -m pip install pipx
py -m pipx install "git+https://github.com/JNPCreates/UnitySceneMCP.git"
```

## Install The Unity Bridge

Run this once per Unity project:

```powershell
py -m mcp_server.install_bridge "C:\Path\To\YourUnityProject"
```

This copies the bridge to:

```text
Assets/Editor/MCPBridge.cs
```

Open the Unity project and let Unity compile. Then open:

```text
Tools > MCP Bridge > Open
```

Click `Start`. The bridge should show that it is running on `http://localhost:9877/`.

## Configure Codex

Open:

```text
C:\Users\YOURNAME\.codex\config.toml
```

Add:

```toml
[mcp_servers.unity-scene]
command = "py"
args = ["-m", "mcp_server"]
enabled = true
```

Restart Codex after editing the config.

## Test It

With Unity open and the bridge started, ask Codex:

```text
Use the create_primitive MCP tool to create a cube named "TestCube" at position [0, 1, 0].
```

Then ask:

```text
Use get_hierarchy to show me what's in the scene.
```

If everything is working:

- Codex calls the `create_primitive` MCP tool.
- The Python server sends an HTTP request to Unity.
- A cube named `TestCube` appears in your Unity scene.
- The MCP Bridge window logs the command.

## Local Development

Clone the repo:

```powershell
git clone https://github.com/JNPCreates/UnitySceneMCP.git
cd UnitySceneMCP
py -m venv .venv
.\.venv\Scripts\Activate.ps1
py -m pip install -e .
```

Run the server:

```powershell
py -m mcp_server
```

## Example Commands

```text
Create an empty GameObject called "GameArea" at the origin.
Create a Sphere called "Ball" at [0, 3, 0] with scale [0.3, 0.3, 0.3].
Add a Rigidbody to "Ball".
Create a physics material called "Bouncy" with bounciness 0.9 and assign it to "Ball".
Use get_hierarchy to show the current scene tree.
```

## Troubleshooting

If Codex cannot connect to Unity:

- Unity must be open.
- The MCP Bridge window must be open.
- The bridge server must be started.
- Localhost port `9877` must not be blocked.

If Codex cannot find the MCP server, verify that `py -m mcp_server` works in PowerShell after installing the package.
