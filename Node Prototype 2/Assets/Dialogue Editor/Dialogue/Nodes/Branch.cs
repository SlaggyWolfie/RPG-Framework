﻿using System;
using System.Linq;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Dialogue;
using WolfEditor.Variables;

namespace WolfEditor.Nodes
{
    [Serializable]
    public sealed class Branch : BaseObject, IOutput
    {
        [SerializeField]
        private OutputPort _outputPort = null;
        public OutputPort OutputPort
        {
            //get { return this.DefaultGetOutputPort(ref _outputPort); }
            get { return _outputPort; }
            set { this.ReplaceOutputPort(ref _outputPort, value); }
        }

        private DialogueNode _dialogueNode = null;
        public DialogueNode DialogueNode
        {
            get
            {
                if (_dialogueNode != null) return _dialogueNode;
                if (_outputPort == null || _outputPort.Connection == null ||
                    _outputPort.Connection.End == null ||
                    _outputPort.Connection.End.Node == null) return null;

                Node node = _outputPort.Connection.End.Node;
                _dialogueNode = node as DialogueNode;
                return _dialogueNode;
            }
        }

        public bool IsAvailable
        {
            get
            {
                return _outputPort.IsConnected &&
                       _outputPort.Connection.GetInstructions().OfType<ConditionInstruction>()
                           .All(instruction => instruction.Evaluate());
            }
        }

        public Branch(Node node)
        {
            _outputPort = new OutputPort { Node = node };
            node.InitPort(_outputPort);
            ID = node.Graph.branchCounter.Get();
            //Debug.Log("You! " + _outputPort.ID);
        }
    }
}
