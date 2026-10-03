"""Install the Unity editor bridge into a Unity project."""

from __future__ import annotations

import argparse
import shutil
from importlib import resources
from pathlib import Path


def install_bridge(unity_project: str | Path, overwrite: bool = False) -> Path:
    project_path = Path(unity_project).expanduser().resolve()
    assets_path = project_path / "Assets"
    if not assets_path.exists():
        raise FileNotFoundError(
            f"Could not find an Assets folder under Unity project: {project_path}"
        )

    editor_path = assets_path / "Editor"
    editor_path.mkdir(parents=True, exist_ok=True)
    target_path = editor_path / "MCPBridge.cs"
    if target_path.exists() and not overwrite:
        raise FileExistsError(
            f"{target_path} already exists. Re-run with --overwrite to replace it."
        )

    bridge_resource = resources.files("mcp_server").joinpath("MCPBridge.cs")
    with resources.as_file(bridge_resource) as source_path:
        shutil.copy2(source_path, target_path)

    return target_path


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Copy MCPBridge.cs into a Unity project's Assets/Editor folder."
    )
    parser.add_argument("unity_project", help="Path to the Unity project root.")
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Replace an existing Assets/Editor/MCPBridge.cs file.",
    )
    args = parser.parse_args()

    target_path = install_bridge(args.unity_project, overwrite=args.overwrite)
    print(f"Installed Unity MCP bridge to: {target_path}")


if __name__ == "__main__":
    main()
