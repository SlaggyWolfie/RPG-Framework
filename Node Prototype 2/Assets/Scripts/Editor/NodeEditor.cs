﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    [Serializable, CustomNodeEditor(typeof(Node))]
    public class NodeEditor : BaseEditor<NodeEditor, Node, CustomNodeEditorAttribute>
    {
        public static Action<Node> onUpdateNode;

        private enum RenamingState { Idle, StandBy, Renaming }
        private RenamingState _renaming = RenamingState.Idle;
        
        protected Dictionary<Port, Rect> _portRects = new Dictionary<Port, Rect>();
        
        protected virtual void OnValidate() { }

        public virtual void OnHeaderGUI()
        {
            DefaultNodeEditorHeader();
        }
        public virtual void OnBodyGUI()
        {
            DefaultNodeEditorBody();
        }

        protected void RenamingGUI()
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1;
            if (_renaming == RenamingState.StandBy)
            {
                EditorGUIUtility.keyboardControl = controlID;
                EditorGUIUtility.editingTextField = true;
                _renaming = RenamingState.Renaming;
            }

            Target.name = EditorGUILayout.TextField(Target.name, NodeResources.Styles.nodeHeader, GUILayout.Height(NodePreferences.PROPERTY_HEIGHT));

            if (EditorGUIUtility.editingTextField) return;
            Rename(Target.name);
            _renaming = RenamingState.Idle;
        }
        protected void DefaultNodeEditorHeader()
        {
            string title = Target.name;
            if (_renaming != RenamingState.Idle) RenamingGUI();
            else
            {
                EditorGUILayout.LabelField(title, NodeResources.Styles.nodeHeader, GUILayout.Height(NodePreferences.PROPERTY_HEIGHT));
            }

            bool headerInput = Target.InputPortIsInHeader();
            bool headerOutput = Target.OutputPortIsInHeader();
            if (!headerInput && !headerOutput) return;

            Rect headerRectProbably = GUILayoutUtility.GetLastRect();
            Rect portRect = headerRectProbably;
            portRect.size = NodePreferences.STANDARD_PORT_SIZE;
            const float xOffset = 2;

            if (headerInput && Target.PortHandler.inputNode != null)
            {
                Rect rect = portRect;
                rect.position += new Vector2(-xOffset, 8);
                NodeRendering.DrawPort(rect, NodeResources.DotOuter, NodeResources.Dot, Color.white, Color.gray, Target.PortHandler.inputNode.InputPort.IsConnected);
                //rect.position += Target.Position;
                _portRects[Target.PortHandler.inputNode.InputPort] = rect;
            }

            if (headerOutput && Target.PortHandler.singleOutputNode != null)
            {
                Rect rect = portRect;
                rect.position += new Vector2(headerRectProbably.width - (16 - xOffset), 8);
                NodeRendering.DrawPort(rect, NodeResources.DotOuter, NodeResources.Dot, Color.white, Color.gray, Target.PortHandler.singleOutputNode.OutputPort.IsConnected);
                //rect.position += Target.Position;
                _portRects[Target.PortHandler.singleOutputNode.OutputPort] = rect;
            }
        }
        protected void DefaultNodeEditorBody()
        {
            SerializedObject.Update();

            bool enterChildren = true;
            EditorGUIUtility.labelWidth = 84;
            for (SerializedProperty i = SerializedObject.GetIterator();
                i.NextVisible(enterChildren);
                enterChildren = false)
            {
                if (i.name == "m_Script" || i.type.ToLower().Contains("port")) continue;
                EditorGUILayout.PropertyField(i, true, GUILayout.MinWidth(NodePreferences.PROPERTY_MIN_WIDTH));
            }

            SerializedObject.ApplyModifiedProperties();
        }

        public void Rename(string newName)
        {
            Target.name = newName;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Target));
        }
        public void InitiateRename()
        {
            _renaming = RenamingState.StandBy;
        }

        public virtual float GetWidth()
        {
            return NodePreferences.STANDARD_NODE_SIZE.x;
        }
        public virtual Color GetTint()
        {
            return Color.white;
        }
        public virtual GUIStyle GetBodyStyle()
        {
            return NodeResources.Styles.nodeBody;
        }

        public virtual Rect GetPortRect(Port port)
        {
            Rect rect;
            //if (_portRects.ContainsKey(port)) Debug.Log("Port Rect: "/* + _portRects[port]*/);
            return _portRects.TryGetValue(port, out rect) ? rect : Rect.zero;
        }
        public static Rect FindPortRect(Port port)
        {
            return GetEditor(port.Node).GetPortRect(port);
        }

        public static void UpdateCallback(Node node)
        {
            if (node == null) return;
            GetEditor(node).OnValidate();
            if (onUpdateNode != null) onUpdateNode(node);
            if (node.onUpdate != null) node.onUpdate();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute, IEditorAttribute
    {
        private readonly Type _inspectedType;
        public CustomNodeEditorAttribute(Type inspectedType) { _inspectedType = inspectedType; }
        public Type GetInspectedType() { return _inspectedType; }
    }
}
