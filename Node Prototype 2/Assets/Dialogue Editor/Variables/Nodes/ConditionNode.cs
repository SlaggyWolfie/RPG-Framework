﻿using System;
using System.Collections.Generic;
using WolfEditor.Nodes.Base;
using UnityEngine;
using WolfEditor.Nodes;
using WolfEditor.Utility;

namespace WolfEditor.Variables
{
    [Serializable]
    public sealed class ConditionNode : Node, IInput, IMultipleOutput, ICondition
    {
        private OutputPort _evaluatedOutputPort = null;

        [SerializeField] private InputPort _inputPort = null;
        [SerializeField] private OutputPort _ifOutputPort = null;
        [SerializeField] private OutputPort _elseOutputPort = null;

        //[SerializeField] private List<Value> _values = new List<Value>();
        [SerializeField] private List<Condition> _conditions = new List<Condition>();
        [SerializeField] private bool _logicalAND = true;

        private OutputPort EvaluatedOutputPort
        {
            get { return _evaluatedOutputPort ?? (_evaluatedOutputPort = Evaluate() ? IfOutputPort : ElseOutputPort); }
        }
        public InputPort InputPort
        {
            get { return this.DefaultGetInputPort(ref _inputPort); }
            set { this.ReplaceInputPort(ref _inputPort, value); }
        }
        public OutputPort IfOutputPort
        {
            get { return this.DefaultGetOutputPort(ref _ifOutputPort); }
            set { this.ReplaceOutputPort(ref _ifOutputPort, value); }
        }
        public OutputPort ElseOutputPort
        {
            get { return this.DefaultGetOutputPort(ref _elseOutputPort); }
            set { this.ReplaceOutputPort(ref _elseOutputPort, value); }
        }
        public bool LogicalAND
        {
            get { return _logicalAND; }
            set { _logicalAND = value; }
        }

        public int ConditionCount { get { return _conditions.Count; } }
        public Condition GetCondition(int index) { return _conditions[index]; }
        public void RemoveCondition(int index) { _conditions.RemoveAt(index); }
        public bool RemoveCondition(Condition condition) { return _conditions.Remove(condition); }
        public void AddCondition(Condition condition) { if (condition != null) _conditions.Add(condition); }

        private bool Evaluate()
        {
            //I realize that both values would align were I to assign
            //result a value in the 'if-else' scopes, so I just assign
            //it a value here.
            bool result = _logicalAND;

            //Already have the value on the stack and not on the heap
            //so why not?
            //Also must scope if-else cause nested unscoped scopes.
            if (result)
            {
                foreach (Condition condition in _conditions)
                    if (!condition.Evaluate()) return false;
            }
            else
            {
                foreach (Condition condition in _conditions)
                    if (condition.Evaluate()) return true;
            }

            return result;
        }

        public IEnumerable<OutputPort> GetOutputs() { return new OutputPort[] { IfOutputPort, ElseOutputPort }; }
        //public OutputPort[] GetOutputs() { return new OutputPort[] { IfOutputPort, ElseOutputPort }; }

        public void ReplacePort(OutputPort oldPort, OutputPort newPort)
        {
            if (oldPort == _ifOutputPort) _ifOutputPort = newPort;
            else if (oldPort == _elseOutputPort) _elseOutputPort = newPort;
        }

        public override Node NextNode()
        {
            //TODO
            //Unsaves the evaluation, I guess
            //Don't remember why it's here.
            //Don't know, don't care, should probably fix, but it works.
            _evaluatedOutputPort = null;
            return EvaluatedOutputPort != null ? EvaluatedOutputPort.Connection.End.Node : null;
        }

        public OutputPort GetExitPort() { return EvaluatedOutputPort; }

        public void ResetOutputPorts()
        {
            _evaluatedOutputPort = null;
            _ifOutputPort = null;
            _elseOutputPort = null;
        }
    }
}
