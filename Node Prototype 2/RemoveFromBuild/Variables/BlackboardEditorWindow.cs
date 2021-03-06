﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WolfEditor.Editor.Nodes;
using WolfEditor.Nodes;
using WolfEditor.Nodes.Base;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WolfEditor.Utility.Editor;

namespace WolfEditor.Editor.Variables
{
    public sealed class BlackboardEditorWindow : Window
    {
        public static BlackboardEditorWindow CurrentBlackboardEditorWindow { get; private set; }

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceID, int line)
        {
            VariableInventory variableInventory = EditorUtility.InstanceIDToObject(instanceID) as VariableInventory;
            if (variableInventory == null) return false;
            return OpenWindow() != null;
        }

        [MenuItem("Window/RPG Framework/Blackboard")]
        public static BlackboardEditorWindow OpenWindow()
        {
            BlackboardEditorWindow window = (BlackboardEditorWindow)GetWindow(typeof(BlackboardEditorWindow), false, "Blackboard", true);
            window.wantsMouseMove = true;
            return window;
        }

        private Blackboard _blackboard = null;
        private Blackboard Blackboard { get { return _blackboard ?? Blackboard.Instance; } }
        private VariableInventoryEditor _globalVariableInventoryEditor = null;
        private VariableInventoryEditor GVIEditor
        {
            get
            {
                return _globalVariableInventoryEditor ?? (_globalVariableInventoryEditor =
                           VariableInventoryEditor.GetEditor(Blackboard.GlobalVariableInventory));
            }
        }
        private VariableInventoryEditor _localVariableInventoryEditor = null;

        static BlackboardEditorWindow()
        {
            CurrentBlackboardEditorWindow = null;
        }

        //private VariableInventoryEditor LVIEditor
        //{
        //    get
        //    {
        //        if (_localVariableInventoryEditor != null) return _localVariableInventoryEditor;

        //        if (Blackboard.CurrentLocalVariableInventory == null) return null;
        //        _localVariableInventoryEditor = VariableInventoryEditor.GetEditor(Blackboard.CurrentLocalVariableInventory);
        //        return _localVariableInventoryEditor;
        //    }
        //}

        protected override void OnFocus()
        {
            base.OnFocus();
            CurrentBlackboardEditorWindow = this;
            if (Blackboard != null) NodeEditorUtilities.AutoSaveAssets();
        }

        public override void OnGUI()
        {
            //return;
            if (Blackboard == null) return;
            bool globalIsNull = GVIEditor == null;
            bool localIsNull = LVIEditor == null;
            if (globalIsNull && localIsNull) return;

            EditorGUILayout.BeginVertical();
            if (!globalIsNull) GVIEditor.OnGUI();
            if (!localIsNull) LVIEditor.OnGUI();
            EditorGUILayout.EndVertical();
        }


        //public void Save()
        //{
        //    if (AssetDatabase.Contains(_graph))
        //    {
        //        EditorUtility.SetDirty(_graph);
        //        NodeUtility.AutoSaveAssets();
        //    }
        //    else SaveAs();
        //}

        //public void SaveAs()
        //{
        //    string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewNodeGraph", "asset", "");
        //    if (string.IsNullOrEmpty(path)) return;

        //    NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
        //    if (existingGraph != null) AssetDatabase.DeleteAsset(path);
        //    AssetDatabase.CreateAsset(_graph, path);
        //    EditorUtility.SetDirty(_graph);
        //    NodeUtility.AutoSaveAssets();
        //}
    }
}
