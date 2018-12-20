﻿using System;
using System.Collections.Generic;
using System.Reflection;
using RPG.Base;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Other;
using RPG.Utility;
using RPG.Utility.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
//using ScrObj = RPG.Nodes.Base.BaseScriptableObject;
using ScrObj = UnityEngine.ScriptableObject;

namespace RPG.Editor.Nodes
{
    public sealed partial class NodeEditorWindow
    {
        #region Drawing
        private void DrawHeldConnection()
        {
            if (!IsDraggingPort) return;

            Color color = NodePreferences.CONNECTION_PORT_COLOR;
            Vector2 start, end;
            Node node = _draggedPort.Node;
            //NodeEditor nodeEditor = NodeEditor.GetEditor(node);

            color.a *= 0.6f;

            Vector2 portPosition = NodeEditor.FindPortRect(DraggedPort).center + node.Position;
            //Vector2 mousePosition = WindowToGridPosition(_mousePosition) + node.Position;
            Vector2 mousePosition = _mousePosition;

            //mousePosition = WindowToGridPosition(mousePosition);
            portPosition = GridToWindowPosition(portPosition);

            //Vector2 mousePosition = _mousePosition;

            if (DraggedPort is InputPort)
            {
                start = mousePosition;
                end = portPosition;
            }
            else
            {
                start = portPosition;
                end = mousePosition;
            }

            NodeRendering.DrawConnection(start, end, color, NodePreferences.CONNECTION_WIDTH / Zoom);
            //NodeUtilities.DrawPort(new Rect());
            //DrawModifiers((start + end) / 2);
        }

        private void DrawConnections()
        {
            //return;
            for (int i = 0; i < _graph.NodeCount; i++)
            {
                Node node = _graph.GetNode(i);
                IInput inputNode = node as IInput;
                InputPort input = inputNode != null ? inputNode.InputPort : null;
                if (input == null) continue;

                Vector2 end = NodeEditor.GetEditor(node).GetPortRect(input).center + node.Position;
                end = GridToWindowPosition(end);
                for (int j = 0; j < input.ConnectionsCount; j++)
                {
                    Connection connection = input.GetConnection(j);
                    //if (connection.End == input) Debug.Log("NANI!?");
                    OutputPort output = connection.Start;
                    Vector2 start = NodeEditor.FindPortRect(output).center + output.Node.Position;

                    //if (connection.Start.Node == connection.End.Node) Debug.Log("Nani?");
                    //Debug.Log(string.Format("Testing: {0} vs {1}", start,
                    //    NodeEditor.FindPortRect(connection.End.Node.PortHandler.singleOutputNode.OutputPort).center +
                    //    connection.End.Node.Position));

                    start = GridToWindowPosition(start);

                    //Debug.Log(string.Format("Drawing Connection from: {0} to {1}", connection.Start.Node.name, connection.End.Node.name));

                    NodeRendering.DrawConnection(start, end, NodePreferences.CONNECTION_PORT_COLOR, NodePreferences.CONNECTION_WIDTH / Zoom);
                    DrawConnectionModifiers((start + end) / 2, connection);
                }
            }
        }

        private void DrawConnectionModifiers(Vector2 position, Connection connection)
        {
            if (_isLayoutEvent) _culledMods = new List<ConnectionModifier>();

            _modifierSizes = new Dictionary<ConnectionModifier, Vector2>();
            Color oldColor = GUI.color;
            Utilities.BeginZoom(this.position, Zoom, TopPadding);

            for (int i = 0; i < connection.ModifierCount; i++)
            {
                ConnectionModifier mod = connection.GetModifier(i);
                if (mod == null) continue;
                bool selected = Selection.Contains(mod);

                if (_isLayoutEvent)
                {
                    if (!selected && ShouldBeCulled(mod, position))
                    {
                        _culledMods.Add(mod);
                        continue;
                    }
                }
                else if (_culledMods.Contains(mod)) continue;

                ConnectionModifierEditor modEditor = ConnectionModifierEditor.GetEditor(mod);

                Vector2 modPosition = GridToWindowPositionNotClipped(position);
                GUILayout.BeginArea(new Rect(modPosition, new Vector2(modEditor.GetWidth(), 4000)));

                if (selected)
                {
                    GUIStyle style = new GUIStyle(modEditor.GetBodyStyle());
                    GUIStyle highlightStyle = new GUIStyle(NodeResources.Styles.nodeHighlight) { padding = style.padding };
                    style.padding = new RectOffset();
                    GUILayout.BeginVertical(style);
                    GUI.color = Color.white;
                    GUILayout.BeginVertical(new GUIStyle(highlightStyle));
                }
                else
                {
                    GUIStyle style = new GUIStyle(modEditor.GetBodyStyle());
                    GUILayout.BeginVertical(style);
                }

                GUI.color = oldColor;
                EditorGUI.BeginChangeCheck();

                modEditor.OnGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(mod);
                    modEditor.SerializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();
                if (!_isLayoutEvent)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    _modifierSizes[mod] = rect.size;
                }
                if (selected) GUILayout.EndVertical();

                GUILayout.EndArea();
            }

            Utilities.EndZoom(this.position, Zoom, TopPadding);
        }

        private void DrawNodes()
        {
            if (_isLayoutEvent) _culledNodes = new List<Node>();

            _nodeSizes = new Dictionary<Node, Vector2>();
            Color oldColor = GUI.color;
            Utilities.BeginZoom(position, Zoom, TopPadding);

            for (int i = 0; i < _graph.NodeCount; i++)
            {
                Node node = _graph.GetNode(i);
                if (node == null) continue;
                if (i >= _graph.NodeCount) return;
                bool selected = Selection.Contains(node);

                if (_isLayoutEvent)
                {
                    if (!selected && ShouldBeCulled(node))
                    {
                        _culledNodes.Add(node);
                        continue;
                    }
                }
                else if (_culledNodes.Contains(node)) continue;

                NodeEditor nodeEditor = NodeEditor.GetEditor(node);

                Vector2 nodePosition = GridToWindowPositionNotClipped(node.Position);
                GUILayout.BeginArea(new Rect(nodePosition, new Vector2(nodeEditor.GetWidth(), 4000)));

                if (selected)
                {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUIStyle highlightStyle = new GUIStyle(NodeResources.Styles.nodeHighlight) { padding = style.padding };
                    style.padding = new RectOffset();
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(style);
                    GUI.color = Color.white;
                    GUILayout.BeginVertical(new GUIStyle(highlightStyle));
                }
                else
                {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(style);
                }

                GUI.color = oldColor;
                EditorGUI.BeginChangeCheck();

                nodeEditor.OnHeaderGUI();
                nodeEditor.OnBodyGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    NodeEditor.UpdateCallback(node);
                    EditorUtility.SetDirty(node);
                    nodeEditor.SerializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();
                if (!_isLayoutEvent)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    //Debug.Log("Caching Rect: " + rect);
                    _nodeSizes[node] = rect.size;
                }
                if (selected) GUILayout.EndVertical();

                GUILayout.EndArea();
            }

            Utilities.EndZoom(position, Zoom, TopPadding);
        }

        private void DrawGrid()
        {
            NodeRendering.DrawGrid(position, Zoom, PanOffset);
        }

        public void DrawSelectionBox()
        {
            if (_currentActivity != Activity.DraggingGrid) return;

            //Vector2 mousePosition = _mousePosition;
            Vector2 mousePosition = WindowToGridPosition(_mousePosition);
            Vector2 size = mousePosition - _dragStart;
            Rect rect = new Rect(_dragStart, size);
            rect.position = GridToWindowPosition(rect.position);
            rect.size /= Zoom;
            Handles.DrawSolidRectangleWithOutline(rect, NodePreferences.SELECTION_FACE_COLOR, NodePreferences.SELECTION_BORDER_COLOR);
        }

        private bool ShouldBeCulled(Node node)
        {
            Vector2 nodePosition = GridToWindowPositionNotClipped(node.Position);
            if (nodePosition.x / Zoom > position.width) return true;
            if (nodePosition.y / Zoom > position.height) return true;

            if (!_nodeSizes.ContainsKey(node)) return false;

            Vector2 size = _nodeSizes[node];
            if (nodePosition.x + size.x < 0) return true;
            if (nodePosition.y + size.y < 0) return true;
            return false;
        }

        private bool ShouldBeCulled(ConnectionModifier mod, Vector2 position)
        {
            Vector2 modPosition = GridToWindowPositionNotClipped(position);
            if (modPosition.x / Zoom > this.position.width) return true;
            if (modPosition.y / Zoom > this.position.height) return true;

            if (!_modifierSizes.ContainsKey(mod)) return false;

            Vector2 size = _modifierSizes[mod];
            if (modPosition.x + size.x < 0) return true;
            if (modPosition.y + size.y < 0) return true;
            return false;
        }
        #endregion

        #region Object Manipulation 
        private static void Select(ScrObj obj, bool add)
        {
            if (add)
            {
                List<Object> selection = new List<Object>(Selection.objects) { obj };
                Selection.objects = selection.ToArray();
            }
            else Selection.objects = new Object[] { obj };
        }

        private static void Deselect(ScrObj obj)
        {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(obj);
            Selection.objects = selection.ToArray();
        }

        #region Nodes
        //private static void SelectNode(Node node, bool add)
        //{
        //    if (add)
        //    {
        //        List<Object> selection = new List<Object>(Selection.objects) { node };
        //        Selection.objects = selection.ToArray();
        //    }
        //    else Selection.objects = new Object[] { node };
        //}

        //private static void DeselectNode(Node node)
        //{
        //    List<Object> selection = new List<Object>(Selection.objects);
        //    selection.Remove(node);
        //    Selection.objects = selection.ToArray();
        //}

        private void CreateNode<T>(Vector2 position) where T : Node
        {
            CreateNode(typeof(T), position);
        }

        private void CreateNode(Type type, Vector2 position)
        {
            if (!ReflectionUtilities.IsOfType(type, typeof(Node))) return;

            Node node = Graph.CreateAndAddNode(type);
            node.Position = position;
            node.name = ObjectNames.NicifyVariableName(type.Name);
            node.Init();
            //node.PortSetup();

            AssetDatabase.AddObjectToAsset(node, Graph);
            EditorUtilities.AutoSaveAssets();
            Repaint();
        }

        private void RemoveSelectedNodes()
        {
            foreach (Node node in GetSelected<Node>())
                GraphEditor.RemoveNode(node);
        }

        private void SendNodeToFront(Node node)
        {
            Graph.SendNodeToFront(node);
        }

        private void SendNodeToBack(Node node)
        {
            Graph.SendNodeToBack(node);
        }

        private void SendNodeForward(Node node)
        {
            Graph.SendNodeForward(node);
        }

        private void SendNodeBackward(Node node)
        {
            Graph.SendNodeBackward(node);
        }

        public void DuplicateSelectedNodes()
        {
            //TODO: add the option to Duplicate Connections if they have also been selected
            Node[] oldSelectedNodes = GetSelected<Node>();
            Object[] newNodes = new Object[oldSelectedNodes.Length];

            Dictionary<Node, Node> substitutes = new Dictionary<Node, Node>();
            for (int i = 0; i < oldSelectedNodes.Length; i++)
            {
                Node oldNode = oldSelectedNodes[i];
                if (oldNode.Graph != Graph) continue;
                Node newNode = GraphEditor.CopyNode(oldNode);
                substitutes[oldNode] = newNode;
                newNode.Position = oldNode.Position + NodePreferences.DUPLICATION_OFFSET;
                newNode.ClearConnections();
                newNodes[i] = newNode;
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            //foreach (var oldNode in oldSelectedNodes)
            //{
            //    if (oldNode.Graph != Graph) continue;
            //}

            Selection.objects = newNodes;
        }

        private void RenameSelectedNode()
        {
            if (Selection.objects.Length != 1 || !(Selection.activeObject is Node)) return;
            Node node = (Node)Selection.activeObject;
            NodeEditor.GetEditor(node).InitiateRename();
        }
        #endregion
        #region Connections

        private void Connect(OutputPort output, InputPort input)
        {
            if (output.Connection != null) GraphEditor.RemoveConnection(output.Connection);
            output.Connection = CreateConnection();
            DraggedOutput.Connect(input);
        }

        private Connection CreateConnection()
        {
            Connection connection = Graph.CreateAndAddConnection();
            connection.name = "Connection";
            AssetDatabase.AddObjectToAsset(connection, Graph);
            EditorUtilities.AutoSaveAssets();
            Repaint();

            return connection;
        }
        private void RemoveSelectedConnections()
        {
            foreach (Connection connection in GetSelected<Connection>())
                GraphEditor.RemoveConnection(connection);
        }

        private void SendConnectionToFront(Connection connection)
        {
            Graph.SendConnectionToFront(connection);
        }
        private void SendConnectionToBack(Connection connection)
        {
            Graph.SendConnectionToBack(connection);
        }
        private void SendConnectionForward(Connection connection)
        {
            Graph.SendConnectionForward(connection);
        }
        private void SendConnectionBackward(Connection connection)
        {
            Graph.SendConnectionBackward(connection);
        }

        public void DuplicateSelectedConnections()
        {
            //TODO: add the option to Duplicate Connections if they have also been selected
            Node[] oldSelectedNodes = GetSelected<Node>();
            Object[] newNodes = new Object[oldSelectedNodes.Length];

            Dictionary<Node, Node> substitutes = new Dictionary<Node, Node>();
            for (int i = 0; i < oldSelectedNodes.Length; i++)
            {
                Node oldNode = oldSelectedNodes[i];
                if (oldNode.Graph != Graph) continue;
                Node newNode = GraphEditor.CopyNode(oldNode);
                substitutes[oldNode] = newNode;
                newNode.Position = oldNode.Position + NodePreferences.DUPLICATION_OFFSET;
                newNode.ClearConnections();
                newNodes[i] = newNode;
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            foreach (var oldNode in oldSelectedNodes)
            {
                if (oldNode.Graph != Graph) continue;
            }

            Selection.objects = newNodes;
        }
        #region Modifiers

        private void AddConnectionModifierToConnection<T>(Connection connection)
            where T : ConnectionModifier
        {
            AddConnectionModifierToConnection(connection, typeof(T));
        }
        private void AddConnectionModifierToConnection(Connection connection, Type type)
        {
            if (!ReflectionUtilities.IsOfType(type, typeof(ConnectionModifier))) return;

            ConnectionModifier mod = connection.CreateAndAddModifier(type);
            mod.name = ObjectNames.NicifyVariableName(type.Name);

            AssetDatabase.AddObjectToAsset(mod, connection);
            EditorUtilities.AutoSaveAssets();
            Repaint();
        }

        private void RemoveSelectedConnectionModifiers()
        {
            foreach (Node node in GetSelected<Node>())
                GraphEditor.RemoveNode(node);
        }
        #endregion
        #endregion

        #endregion

    }
}
