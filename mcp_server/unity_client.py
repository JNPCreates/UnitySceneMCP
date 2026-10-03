"""Thin HTTP client that sends commands to the Unity Editor bridge."""

import requests

UNITY_BRIDGE_URL = "http://localhost:9877"
TIMEOUT = 10  # seconds


def send_command(tool_name: str, params: dict) -> dict:
    """POST a command to the Unity bridge and return the parsed response."""
    url = f"{UNITY_BRIDGE_URL}/{tool_name}"
    try:
        resp = requests.post(url, json=params, timeout=TIMEOUT)
        resp.raise_for_status()
        return resp.json()
    except requests.ConnectionError:
        return {
            "success": False,
            "message": (
                "Cannot connect to Unity bridge at localhost:9877. "
                "Make sure Unity is open and the MCP Bridge window is running "
                "(Tools > MCP Bridge > Open, then click Start)."
            ),
        }
    except requests.Timeout:
        return {
            "success": False,
            "message": f"Command '{tool_name}' timed out after {TIMEOUT}s.",
        }
    except Exception as e:
        return {"success": False, "message": f"Error: {e}"}
