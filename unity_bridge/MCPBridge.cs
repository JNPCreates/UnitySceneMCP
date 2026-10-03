// MCPBridge.cs — Unity Editor script for the Unity Scene MCP bridge.
// Drop this file into any Unity project's Assets/Editor/ folder.
// Open via menu: Tools > MCP Bridge > Open
//
// Listens on http://localhost:9877/ and executes scene manipulation commands
// sent by the Python MCP server.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MCPBridge
{
    public class MCPBridgeWindow : EditorWindow
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private bool _isRunning;
        private int _commandCount;
        private string _lastCommand = "";
        private string _lastResult = "";
        private readonly List<string> _log = new List<string>();
        private const int MaxLogEntries = 50;
        private Vector2 _scrollPos;

        private static readonly ConcurrentQueue<Action> MainThreadQueue = new ConcurrentQueue<Action>();

        [MenuItem("Tools/MCP Bridge/Open")]
        public static void ShowWindow()
        {
            var window = GetWindow<MCPBridgeWindow>("MCP Bridge");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            EditorApplication.update += ProcessMainThreadQueue;
            // Auto-restart after domain reload if it was running before
            if (SessionState.GetBool("MCPBridge_WasRunning", false))
                StartServer();
        }

        private void OnDisable()
        {
            EditorApplication.update -= ProcessMainThreadQueue;
            // Don't clear WasRunning flag during domain reload - preserve state for auto-restart
            StopServer(preserveWasRunningFlag: true);
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity Scene MCP Bridge", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (_isRunning)
            {
                EditorGUILayout.HelpBox("Server RUNNING on http://localhost:9877/", MessageType.Info);
                if (GUILayout.Button("Stop", GUILayout.Width(60)))
                    StopServer();
            }
            else
            {
                EditorGUILayout.HelpBox("Server STOPPED", MessageType.Warning);
                if (GUILayout.Button("Start", GUILayout.Width(60)))
                    StartServer();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Commands Executed", _commandCount.ToString());
            EditorGUILayout.LabelField("Last Command", _lastCommand);

            GUILayout.Space(5);
            GUILayout.Label("Log", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
            foreach (var entry in _log)
                EditorGUILayout.LabelField(entry, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Log"))
            {
                _log.Clear();
                Repaint();
            }
        }

        // ── Server lifecycle ──

        private void StartServer()
        {
            if (_isRunning) return;
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:9877/");
                _listener.Start();
                _isRunning = true;
                SessionState.SetBool("MCPBridge_WasRunning", true);
                _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                _listenerThread.Start();
                Log("Server started on http://localhost:9877/");
            }
            catch (Exception e)
            {
                Log($"Failed to start: {e.Message}");
            }
        }

        private void StopServer(bool preserveWasRunningFlag = false)
        {
            if (!_isRunning) return;
            _isRunning = false;
            // Only clear the flag if this is an explicit stop (user clicked Stop button)
            // Don't clear it during domain reloads so it auto-restarts
            if (!preserveWasRunningFlag)
                SessionState.SetBool("MCPBridge_WasRunning", false);
            try { _listener?.Stop(); } catch { }
            try { _listener?.Close(); } catch { }
            _listener = null;
            Log("Server stopped.");
        }

        private void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    if (_isRunning) Log($"Listener error: {e.Message}");
                }
            }
        }

        // ── Request handling ──

        private void HandleRequest(HttpListenerContext context)
        {
            string toolName = context.Request.Url.AbsolutePath.TrimStart('/').ToLowerInvariant();
            string body = "";
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                body = reader.ReadToEnd();

            // Use a ManualResetEvent to block the threadpool thread until main thread finishes
            string responseJson = null;
            var done = new ManualResetEventSlim(false);

            MainThreadQueue.Enqueue(() =>
            {
                try
                {
                    responseJson = ExecuteCommand(toolName, body);
                }
                catch (Exception e)
                {
                    responseJson = JsonResult(false, $"Unhandled error: {e.Message}");
                }
                finally
                {
                    done.Set();
                }
            });

            // Wait up to 30 seconds for main thread to process
            if (!done.Wait(TimeSpan.FromSeconds(30)))
                responseJson = JsonResult(false, "Timeout waiting for Unity main thread");

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private static void ProcessMainThreadQueue()
        {
            // Drain all queued actions on the main thread
            while (MainThreadQueue.TryDequeue(out var action))
            {
                try { action(); }
                catch (Exception e) { Debug.LogError($"[MCPBridge] Main thread error: {e}"); }
            }
        }

        // ── Command router ──

        private string ExecuteCommand(string toolName, string jsonBody)
        {
            _commandCount++;
            _lastCommand = toolName;
            Log($">> {toolName}");

            var param = string.IsNullOrEmpty(jsonBody) ? new Dictionary<string, object>() : MiniJson.Deserialize(jsonBody);

            try
            {
                switch (toolName)
                {
                    // Scene graph
                    case "create_gameobject":     return CmdCreateGameObject(param);
                    case "create_primitive":       return CmdCreatePrimitive(param);
                    case "set_parent":             return CmdSetParent(param);
                    case "delete_gameobject":      return CmdDeleteGameObject(param);
                    case "set_layer":              return CmdSetLayer(param);
                    case "set_tag":                return CmdSetTag(param);

                    // Transforms
                    case "set_transform":          return CmdSetTransform(param);
                    case "set_position":           return CmdSetPosition(param);
                    case "set_rotation":           return CmdSetRotation(param);
                    case "set_scale":              return CmdSetScale(param);

                    // Components
                    case "add_component":          return CmdAddComponent(param);
                    case "set_component_field":    return CmdSetComponentField(param);
                    case "remove_component":       return CmdRemoveComponent(param);

                    // Assets
                    case "create_material":        return CmdCreateMaterial(param);
                    case "create_physics_material": return CmdCreatePhysicsMaterial(param);
                    case "assign_material":        return CmdAssignMaterial(param);
                    case "assign_physics_material": return CmdAssignPhysicsMaterial(param);

                    // Query
                    case "get_hierarchy":          return CmdGetHierarchy(param);

                    // Health check
                    case "ping":                   return JsonResult(true, "pong");

                    default:
                        return JsonResult(false, $"Unknown command: {toolName}");
                }
            }
            catch (Exception e)
            {
                Log($"   ERROR: {e.Message}");
                return JsonResult(false, e.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  SCENE GRAPH COMMANDS
        // ══════════════════════════════════════════════════════════════

        private string CmdCreateGameObject(Dictionary<string, object> p)
        {
            string name = GetStr(p, "name", "GameObject");
            string parentPath = GetStr(p, "parent_path", "");

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");

            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = FindByPath(parentPath);
                if (parent == null) return JsonResult(false, $"Parent not found: {parentPath}");
                go.transform.SetParent(parent.transform, false);
            }

            ApplyTransformFromParams(go, p, true);
            Log($"   Created empty: {GetFullPath(go)}");
            return JsonResult(true, $"Created GameObject '{name}'", new Dictionary<string, object> { { "path", GetFullPath(go) } });
        }

        private string CmdCreatePrimitive(Dictionary<string, object> p)
        {
            string name = GetStr(p, "name", "Primitive");
            string typeStr = GetStr(p, "primitive_type", "Cube");
            string parentPath = GetStr(p, "parent_path", "");

            if (!Enum.TryParse<PrimitiveType>(typeStr, true, out var primType))
                return JsonResult(false, $"Invalid primitive_type: {typeStr}. Use: Cube, Sphere, Cylinder, Capsule, Plane, Quad");

            var go = GameObject.CreatePrimitive(primType);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");

            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = FindByPath(parentPath);
                if (parent == null) return JsonResult(false, $"Parent not found: {parentPath}");
                go.transform.SetParent(parent.transform, false);
            }

            ApplyTransformFromParams(go, p, true);
            Log($"   Created {typeStr}: {GetFullPath(go)}");
            return JsonResult(true, $"Created {typeStr} '{name}'", new Dictionary<string, object> { { "path", GetFullPath(go) } });
        }

        private string CmdSetParent(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string parentPath = GetStr(p, "new_parent_path", "");
            bool worldStays = GetBool(p, "world_position_stays", true);

            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.SetTransformParent(obj.transform,
                string.IsNullOrEmpty(parentPath) ? null : FindByPath(parentPath)?.transform,
                worldStays, $"Reparent {obj.name}");

            return JsonResult(true, $"Reparented '{obj.name}'", new Dictionary<string, object> { { "path", GetFullPath(obj) } });
        }

        private string CmdDeleteGameObject(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.DestroyObjectImmediate(obj);
            return JsonResult(true, $"Deleted '{objPath}'");
        }

        private string CmdSetLayer(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string layerName = GetStr(p, "layer_name");
            bool children = GetBool(p, "include_children", true);

            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0) return JsonResult(false, $"Layer not found: {layerName}");

            Undo.RecordObject(obj, $"Set layer {layerName}");
            obj.layer = layer;
            if (children)
                foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
                {
                    Undo.RecordObject(child.gameObject, $"Set layer {layerName}");
                    child.gameObject.layer = layer;
                }

            return JsonResult(true, $"Set layer '{layerName}' on '{objPath}'");
        }

        private string CmdSetTag(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string tagName = GetStr(p, "tag_name");

            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            // Auto-create tag if missing
            EnsureTagExists(tagName);

            Undo.RecordObject(obj, $"Set tag {tagName}");
            obj.tag = tagName;
            return JsonResult(true, $"Set tag '{tagName}' on '{objPath}'");
        }

        // ══════════════════════════════════════════════════════════════
        //  TRANSFORM COMMANDS
        // ══════════════════════════════════════════════════════════════

        private string CmdSetTransform(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            bool local = GetBool(p, "local_space", true);
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.RecordObject(obj.transform, $"Set transform {obj.name}");
            if (p.ContainsKey("position"))
            {
                var pos = GetVec3(p, "position");
                if (local) obj.transform.localPosition = pos;
                else obj.transform.position = pos;
            }
            if (p.ContainsKey("rotation"))
            {
                var rot = GetVec3(p, "rotation");
                if (local) obj.transform.localEulerAngles = rot;
                else obj.transform.eulerAngles = rot;
            }
            if (p.ContainsKey("scale"))
                obj.transform.localScale = GetVec3(p, "scale");

            return JsonResult(true, $"Set transform on '{objPath}'");
        }

        private string CmdSetPosition(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            bool local = GetBool(p, "local_space", true);
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.RecordObject(obj.transform, $"Set position {obj.name}");
            var pos = GetVec3(p, "position");
            if (local) obj.transform.localPosition = pos;
            else obj.transform.position = pos;
            return JsonResult(true, $"Set position on '{objPath}'");
        }

        private string CmdSetRotation(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            bool local = GetBool(p, "local_space", true);
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.RecordObject(obj.transform, $"Set rotation {obj.name}");
            var rot = GetVec3(p, "rotation");
            if (local) obj.transform.localEulerAngles = rot;
            else obj.transform.eulerAngles = rot;
            return JsonResult(true, $"Set rotation on '{objPath}'");
        }

        private string CmdSetScale(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Undo.RecordObject(obj.transform, $"Set scale {obj.name}");
            obj.transform.localScale = GetVec3(p, "scale");
            return JsonResult(true, $"Set scale on '{objPath}'");
        }

        // ══════════════════════════════════════════════════════════════
        //  COMPONENT COMMANDS
        // ══════════════════════════════════════════════════════════════

        private string CmdAddComponent(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string typeName = GetStr(p, "component_type");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            var type = ResolveComponentType(typeName);
            if (type == null) return JsonResult(false, $"Component type not found: {typeName}");

            Undo.AddComponent(obj, type);
            return JsonResult(true, $"Added {typeName} to '{objPath}'");
        }

        private string CmdSetComponentField(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string typeName = GetStr(p, "component_type");
            string fieldName = GetStr(p, "field_name");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            var comp = GetComponentByName(obj, typeName);
            if (comp == null) return JsonResult(false, $"Component '{typeName}' not found on '{objPath}'");

            object rawValue = p.ContainsKey("value") ? p["value"] : null;

            // Try SerializedProperty first
            var so = new SerializedObject(comp);
            var sp = so.FindProperty(fieldName);
            if (sp != null)
            {
                if (SetSerializedPropertyValue(sp, rawValue))
                {
                    so.ApplyModifiedProperties();
                    return JsonResult(true, $"Set {typeName}.{fieldName} on '{objPath}'");
                }
            }

            // Fallback: reflection
            Undo.RecordObject(comp, $"Set {fieldName}");
            if (SetFieldViaReflection(comp, fieldName, rawValue))
                return JsonResult(true, $"Set {typeName}.{fieldName} on '{objPath}' (via reflection)");

            return JsonResult(false, $"Could not set field '{fieldName}' on {typeName}");
        }

        private string CmdRemoveComponent(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string typeName = GetStr(p, "component_type");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            var comp = GetComponentByName(obj, typeName);
            if (comp == null) return JsonResult(false, $"Component '{typeName}' not found on '{objPath}'");

            Undo.DestroyObjectImmediate(comp);
            return JsonResult(true, $"Removed {typeName} from '{objPath}'");
        }

        // ══════════════════════════════════════════════════════════════
        //  ASSET COMMANDS
        // ══════════════════════════════════════════════════════════════

        private string CmdCreateMaterial(Dictionary<string, object> p)
        {
            string matName = GetStr(p, "name", "NewMaterial");
            string savePath = GetStr(p, "save_path", "Assets/");
            string shaderName = GetStr(p, "shader", "Universal Render Pipeline/Lit");
            var color = p.ContainsKey("color") ? GetColor(p, "color") : Color.white;

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                // Fallback to Standard if URP shader not found
                shader = Shader.Find("Standard");
                if (shader == null) return JsonResult(false, $"Shader not found: {shaderName}");
            }

            var mat = new Material(shader);
            mat.color = color;

            // Apply additional properties if provided
            if (p.ContainsKey("properties") && p["properties"] is Dictionary<string, object> props)
            {
                foreach (var kvp in props)
                {
                    if (kvp.Value is double d) mat.SetFloat(kvp.Key, (float)d);
                    else if (kvp.Value is long l) mat.SetFloat(kvp.Key, l);
                    else if (kvp.Value is List<object> arr && arr.Count == 4)
                        mat.SetColor(kvp.Key, new Color(ToFloat(arr[0]), ToFloat(arr[1]), ToFloat(arr[2]), ToFloat(arr[3])));
                }
            }

            EnsureDirectoryExists(savePath);
            string fullPath = Path.Combine(savePath, matName + ".mat").Replace("\\", "/");
            AssetDatabase.CreateAsset(mat, fullPath);
            AssetDatabase.SaveAssets();

            Log($"   Created material: {fullPath}");
            return JsonResult(true, $"Created material '{matName}'", new Dictionary<string, object> { { "asset_path", fullPath } });
        }

        private string CmdCreatePhysicsMaterial(Dictionary<string, object> p)
        {
            string matName = GetStr(p, "name", "NewPhysicsMaterial");
            string savePath = GetStr(p, "save_path", "Assets/");
            float dynFriction = GetFloat(p, "dynamic_friction", 0.4f);
            float statFriction = GetFloat(p, "static_friction", 0.4f);
            float bounciness = GetFloat(p, "bounciness", 0f);
            string frictionCombine = GetStr(p, "friction_combine", "Average");
            string bounceCombine = GetStr(p, "bounce_combine", "Average");

            var mat = new PhysicMaterial(matName)
            {
                dynamicFriction = dynFriction,
                staticFriction = statFriction,
                bounciness = bounciness,
                frictionCombine = ParseCombine(frictionCombine),
                bounceCombine = ParseCombine(bounceCombine)
            };

            EnsureDirectoryExists(savePath);
            string fullPath = Path.Combine(savePath, matName + ".physicMaterial").Replace("\\", "/");
            AssetDatabase.CreateAsset(mat, fullPath);
            AssetDatabase.SaveAssets();

            Log($"   Created physics material: {fullPath}");
            return JsonResult(true, $"Created physics material '{matName}'", new Dictionary<string, object> { { "asset_path", fullPath } });
        }

        private string CmdAssignMaterial(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string matPath = GetStr(p, "material_path");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return JsonResult(false, $"No Renderer on '{objPath}'");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) return JsonResult(false, $"Material not found: {matPath}");

            Undo.RecordObject(renderer, "Assign material");
            renderer.sharedMaterial = mat;
            return JsonResult(true, $"Assigned material '{matPath}' to '{objPath}'");
        }

        private string CmdAssignPhysicsMaterial(Dictionary<string, object> p)
        {
            string objPath = GetStr(p, "object_path");
            string matPath = GetStr(p, "physics_material_path");
            string colliderType = GetStr(p, "collider_type", "");
            var obj = FindByPath(objPath);
            if (obj == null) return JsonResult(false, $"Object not found: {objPath}");

            Collider collider;
            if (!string.IsNullOrEmpty(colliderType))
            {
                var type = ResolveComponentType(colliderType);
                collider = type != null ? obj.GetComponent(type) as Collider : null;
            }
            else
            {
                collider = obj.GetComponent<Collider>();
            }

            if (collider == null) return JsonResult(false, $"No Collider on '{objPath}'");

            var mat = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(matPath);
            if (mat == null) return JsonResult(false, $"Physics material not found: {matPath}");

            Undo.RecordObject(collider, "Assign physics material");
            collider.sharedMaterial = mat;
            return JsonResult(true, $"Assigned physics material '{matPath}' to '{objPath}'");
        }

        // ══════════════════════════════════════════════════════════════
        //  QUERY COMMANDS
        // ══════════════════════════════════════════════════════════════

        private string CmdGetHierarchy(Dictionary<string, object> p)
        {
            string rootPath = GetStr(p, "root_path", "");
            int depth = GetInt(p, "depth", -1);

            if (!string.IsNullOrEmpty(rootPath))
            {
                var root = FindByPath(rootPath);
                if (root == null) return JsonResult(false, $"Object not found: {rootPath}");
                var tree = BuildHierarchyNode(root, depth, 0);
                return JsonResult(true, "Hierarchy retrieved", tree);
            }

            // Entire scene
            var roots = new List<object>();
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var rootGo in scene.GetRootGameObjects())
                roots.Add(BuildHierarchyNode(rootGo, depth, 0));

            return JsonResult(true, "Hierarchy retrieved", new Dictionary<string, object> { { "root_objects", roots } });
        }

        private Dictionary<string, object> BuildHierarchyNode(GameObject go, int maxDepth, int currentDepth)
        {
            var node = new Dictionary<string, object>
            {
                { "name", go.name },
                { "path", GetFullPath(go) },
                { "active", go.activeSelf },
                { "tag", go.tag },
                { "layer", LayerMask.LayerToName(go.layer) },
                { "position", Vec3ToList(go.transform.localPosition) },
                { "rotation", Vec3ToList(go.transform.localEulerAngles) },
                { "scale", Vec3ToList(go.transform.localScale) },
                { "components", go.GetComponents<Component>()
                    .Where(c => c != null)
                    .Select(c => c.GetType().Name).ToList() }
            };

            if (maxDepth < 0 || currentDepth < maxDepth)
            {
                var children = new List<object>();
                for (int i = 0; i < go.transform.childCount; i++)
                    children.Add(BuildHierarchyNode(go.transform.GetChild(i).gameObject, maxDepth, currentDepth + 1));
                if (children.Count > 0)
                    node["children"] = children;
            }

            return node;
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════

        /// <summary>Find a GameObject by hierarchy path, e.g. "Safe/DoorPivot/SafeDoor"</summary>
        private static GameObject FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            // Try direct find first (works for root objects and full paths with /)
            var found = GameObject.Find(path);
            if (found != null) return found;

            // Try searching from root objects
            string[] parts = path.Split('/');
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == parts[0])
                {
                    if (parts.Length == 1) return root;
                    var current = root.transform;
                    for (int i = 1; i < parts.Length; i++)
                    {
                        current = current.Find(parts[i]);
                        if (current == null) break;
                    }
                    if (current != null) return current.gameObject;
                }
            }
            return null;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private void ApplyTransformFromParams(GameObject go, Dictionary<string, object> p, bool localSpace)
        {
            if (p.ContainsKey("position"))
            {
                var pos = GetVec3(p, "position");
                if (localSpace) go.transform.localPosition = pos;
                else go.transform.position = pos;
            }
            if (p.ContainsKey("rotation"))
            {
                var rot = GetVec3(p, "rotation");
                if (localSpace) go.transform.localEulerAngles = rot;
                else go.transform.eulerAngles = rot;
            }
            if (p.ContainsKey("scale"))
                go.transform.localScale = GetVec3(p, "scale");
        }

        private static Type ResolveComponentType(string typeName)
        {
            // Try UnityEngine first
            var type = typeof(GameObject).Assembly.GetType("UnityEngine." + typeName);
            if (type != null) return type;

            // Try UnityEngine.UI
            var uiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "UnityEngine.UI");
            if (uiAssembly != null)
            {
                type = uiAssembly.GetType("UnityEngine.UI." + typeName);
                if (type != null) return type;
            }

            // Try TMPro
            var tmpAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Unity.TextMeshPro");
            if (tmpAssembly != null)
            {
                type = tmpAssembly.GetType("TMPro." + typeName);
                if (type != null) return type;
            }

            // Search all assemblies for the exact name (handles user scripts like "BeerPongGameManager")
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null) return type;

                // Also try without namespace
                type = asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(Component).IsAssignableFrom(t));
                if (type != null) return type;
            }

            return null;
        }

        private static Component GetComponentByName(GameObject go, string typeName)
        {
            var type = ResolveComponentType(typeName);
            return type != null ? go.GetComponent(type) : null;
        }

        private bool SetSerializedPropertyValue(SerializedProperty sp, object value)
        {
            try
            {
                switch (sp.propertyType)
                {
                    case SerializedPropertyType.Float:
                        sp.floatValue = ToFloat(value);
                        return true;
                    case SerializedPropertyType.Integer:
                        sp.intValue = Convert.ToInt32(value);
                        return true;
                    case SerializedPropertyType.Boolean:
                        sp.boolValue = Convert.ToBoolean(value);
                        return true;
                    case SerializedPropertyType.String:
                        sp.stringValue = value?.ToString() ?? "";
                        return true;
                    case SerializedPropertyType.Vector3:
                        sp.vector3Value = ListToVec3(value);
                        return true;
                    case SerializedPropertyType.Vector2:
                        var v2list = value as List<object>;
                        if (v2list != null && v2list.Count >= 2)
                            sp.vector2Value = new Vector2(ToFloat(v2list[0]), ToFloat(v2list[1]));
                        return true;
                    case SerializedPropertyType.Color:
                        var clist = value as List<object>;
                        if (clist != null && clist.Count >= 3)
                            sp.colorValue = new Color(ToFloat(clist[0]), ToFloat(clist[1]), ToFloat(clist[2]),
                                clist.Count >= 4 ? ToFloat(clist[3]) : 1f);
                        return true;
                    case SerializedPropertyType.Enum:
                        if (value is string enumStr)
                        {
                            int idx = Array.IndexOf(sp.enumNames, enumStr);
                            if (idx >= 0) { sp.enumValueIndex = idx; return true; }
                            // Try case-insensitive
                            idx = Array.FindIndex(sp.enumNames, n => n.Equals(enumStr, StringComparison.OrdinalIgnoreCase));
                            if (idx >= 0) { sp.enumValueIndex = idx; return true; }
                        }
                        else if (value is long || value is int || value is double)
                        {
                            sp.enumValueIndex = Convert.ToInt32(value);
                            return true;
                        }
                        return false;
                    case SerializedPropertyType.ObjectReference:
                        sp.objectReferenceValue = ResolveObjectReference(value);
                        return true;
                    default:
                        return false;
                }
            }
            catch { return false; }
        }

        private static UnityEngine.Object ResolveObjectReference(object value)
        {
            if (value == null) return null;
            string str = value.ToString();

            if (str.StartsWith("go:"))
            {
                var go = FindByPath(str.Substring(3));
                return go;
            }
            if (str.StartsWith("asset:"))
            {
                return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str.Substring(6));
            }
            // Try as asset path directly
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str);
            if (asset != null) return asset;

            // Try as hierarchy path
            return FindByPath(str);
        }

        private bool SetFieldViaReflection(Component comp, string fieldName, object value)
        {
            var type = comp.GetType();
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var prop = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(comp, ConvertValue(value, field.FieldType));
                return true;
            }
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(comp, ConvertValue(value, prop.PropertyType));
                return true;
            }
            return false;
        }

        private object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType == typeof(Vector3)) return ListToVec3(value);
            if (targetType == typeof(Vector2))
            {
                var list = value as List<object>;
                if (list != null && list.Count >= 2) return new Vector2(ToFloat(list[0]), ToFloat(list[1]));
            }
            if (targetType == typeof(Color)) return ListToColor(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(float)) return ToFloat(value);
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(string)) return value.ToString();
            if (targetType.IsEnum)
            {
                if (value is string s) return Enum.Parse(targetType, s, true);
                return Enum.ToObject(targetType, Convert.ToInt32(value));
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
                return ResolveObjectReference(value);

            return Convert.ChangeType(value, targetType);
        }

        private static void EnsureTagExists(string tagName)
        {
            // Check if tag already exists
            try
            {
                if (Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tagName) >= 0)
                    return;
            }
            catch { }

            // Create tag via TagManager
            var tagManager = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var tagsProp = tagManager.FindProperty("tags");

            // Find first empty slot or add new
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (string.IsNullOrEmpty(tagsProp.GetArrayElementAtIndex(i).stringValue))
                {
                    tagsProp.GetArrayElementAtIndex(i).stringValue = tagName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        private static PhysicMaterialCombine ParseCombine(string value)
        {
            if (Enum.TryParse<PhysicMaterialCombine>(value, true, out var result))
                return result;
            return PhysicMaterialCombine.Average;
        }

        private static void EnsureDirectoryExists(string assetPath)
        {
            // Convert asset path to full system path and ensure dirs exist
            string fullPath = Path.GetFullPath(assetPath);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }

        // ── JSON parameter extraction ──

        private static string GetStr(Dictionary<string, object> p, string key, string def = "")
            => p.ContainsKey(key) && p[key] != null ? p[key].ToString() : def;

        private static float GetFloat(Dictionary<string, object> p, string key, float def = 0f)
            => p.ContainsKey(key) ? ToFloat(p[key]) : def;

        private static int GetInt(Dictionary<string, object> p, string key, int def = 0)
            => p.ContainsKey(key) ? Convert.ToInt32(p[key]) : def;

        private static bool GetBool(Dictionary<string, object> p, string key, bool def = false)
            => p.ContainsKey(key) ? Convert.ToBoolean(p[key]) : def;

        private static Vector3 GetVec3(Dictionary<string, object> p, string key)
            => ListToVec3(p[key]);

        private static Color GetColor(Dictionary<string, object> p, string key)
            => ListToColor(p[key]);

        private static Vector3 ListToVec3(object obj)
        {
            if (obj is List<object> list && list.Count >= 3)
                return new Vector3(ToFloat(list[0]), ToFloat(list[1]), ToFloat(list[2]));
            return Vector3.zero;
        }

        private static Color ListToColor(object obj)
        {
            if (obj is List<object> list && list.Count >= 3)
                return new Color(ToFloat(list[0]), ToFloat(list[1]), ToFloat(list[2]),
                    list.Count >= 4 ? ToFloat(list[3]) : 1f);
            return Color.white;
        }

        private static List<object> Vec3ToList(Vector3 v) => new List<object> { v.x, v.y, v.z };

        private static float ToFloat(object o)
        {
            if (o is double d) return (float)d;
            if (o is long l) return l;
            if (o is int i) return i;
            if (o is float f) return f;
            return Convert.ToSingle(o);
        }

        // ── JSON response builder ──

        private static string JsonResult(bool success, string message, object data = null)
        {
            var dict = new Dictionary<string, object>
            {
                { "success", success },
                { "message", message }
            };
            if (data != null) dict["data"] = data;
            return MiniJson.Serialize(dict);
        }

        private void Log(string msg)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            _log.Add(entry);
            if (_log.Count > MaxLogEntries) _log.RemoveAt(0);
            _lastResult = msg;
            Repaint();
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  MINIMAL JSON SERIALIZER/DESERIALIZER
    //  (avoids dependency on Newtonsoft or Unity's JsonUtility which
    //   doesn't handle Dictionary<string,object>)
    // ══════════════════════════════════════════════════════════════
    public static class MiniJson
    {
        public static Dictionary<string, object> Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return new Dictionary<string, object>();
            int index = 0;
            return ParseObject(json, ref index);
        }

        public static string Serialize(object obj)
        {
            var sb = new StringBuilder();
            SerializeValue(obj, sb);
            return sb.ToString();
        }

        private static void SerializeValue(object value, StringBuilder sb)
        {
            if (value == null) { sb.Append("null"); return; }
            if (value is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (value is string s) { sb.Append('"'); sb.Append(EscapeString(s)); sb.Append('"'); return; }
            if (value is int || value is long) { sb.Append(value); return; }
            if (value is float f) { sb.Append(f.ToString("R")); return; }
            if (value is double d) { sb.Append(d.ToString("R")); return; }
            if (value is Dictionary<string, object> dict)
            {
                sb.Append('{');
                bool first = true;
                foreach (var kvp in dict)
                {
                    if (!first) sb.Append(',');
                    sb.Append('"'); sb.Append(EscapeString(kvp.Key)); sb.Append("\":");
                    SerializeValue(kvp.Value, sb);
                    first = false;
                }
                sb.Append('}');
                return;
            }
            if (value is System.Collections.IList list)
            {
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    SerializeValue(list[i], sb);
                }
                sb.Append(']');
                return;
            }
            sb.Append('"'); sb.Append(EscapeString(value.ToString())); sb.Append('"');
        }

        private static string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        private static Dictionary<string, object> ParseObject(string json, ref int i)
        {
            var dict = new Dictionary<string, object>();
            SkipWhitespace(json, ref i);
            if (i >= json.Length || json[i] != '{') return dict;
            i++; // skip {

            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);
                if (i >= json.Length || json[i] == '}') { i++; return dict; }
                if (json[i] == ',') { i++; continue; }

                string key = ParseString(json, ref i);
                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == ':') i++;
                SkipWhitespace(json, ref i);
                object value = ParseValue(json, ref i);
                dict[key] = value;
            }
            return dict;
        }

        private static List<object> ParseArray(string json, ref int i)
        {
            var list = new List<object>();
            i++; // skip [
            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);
                if (i >= json.Length || json[i] == ']') { i++; return list; }
                if (json[i] == ',') { i++; continue; }
                list.Add(ParseValue(json, ref i));
            }
            return list;
        }

        private static object ParseValue(string json, ref int i)
        {
            SkipWhitespace(json, ref i);
            if (i >= json.Length) return null;
            char c = json[i];

            if (c == '"') return ParseString(json, ref i);
            if (c == '{') return ParseObject(json, ref i);
            if (c == '[') return ParseArray(json, ref i);
            if (c == 't' || c == 'f') return ParseBool(json, ref i);
            if (c == 'n') { i += 4; return null; }
            return ParseNumber(json, ref i);
        }

        private static string ParseString(string json, ref int i)
        {
            i++; // skip opening "
            var sb = new StringBuilder();
            while (i < json.Length)
            {
                char c = json[i];
                if (c == '\\')
                {
                    i++;
                    if (i < json.Length)
                    {
                        char esc = json[i];
                        if (esc == '"') sb.Append('"');
                        else if (esc == '\\') sb.Append('\\');
                        else if (esc == 'n') sb.Append('\n');
                        else if (esc == 'r') sb.Append('\r');
                        else if (esc == 't') sb.Append('\t');
                        else if (esc == 'u')
                        {
                            if (i + 4 < json.Length)
                            {
                                string hex = json.Substring(i + 1, 4);
                                sb.Append((char)Convert.ToInt32(hex, 16));
                                i += 4;
                            }
                        }
                        else sb.Append(esc);
                    }
                }
                else if (c == '"') { i++; return sb.ToString(); }
                else sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        private static object ParseNumber(string json, ref int i)
        {
            int start = i;
            bool isFloat = false;
            if (i < json.Length && json[i] == '-') i++;
            while (i < json.Length && char.IsDigit(json[i])) i++;
            if (i < json.Length && json[i] == '.') { isFloat = true; i++; while (i < json.Length && char.IsDigit(json[i])) i++; }
            if (i < json.Length && (json[i] == 'e' || json[i] == 'E')) { isFloat = true; i++; if (i < json.Length && (json[i] == '+' || json[i] == '-')) i++; while (i < json.Length && char.IsDigit(json[i])) i++; }
            string numStr = json.Substring(start, i - start);
            if (isFloat) return double.Parse(numStr, System.Globalization.CultureInfo.InvariantCulture);
            return long.Parse(numStr);
        }

        private static bool ParseBool(string json, ref int i)
        {
            if (json[i] == 't') { i += 4; return true; }
            i += 5; return false;
        }

        private static void SkipWhitespace(string json, ref int i)
        {
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
        }
    }
}
