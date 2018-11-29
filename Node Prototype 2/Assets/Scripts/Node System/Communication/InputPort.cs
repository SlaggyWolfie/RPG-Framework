﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Nodes
{
    [Serializable]
    public class InputPort : Port
    {
        [SerializeField]
        //[HideInInspector]
        private List<Connection> _connections = new List<Connection>();

        public int ConnectionsCount
        {
            get { return _connections.Count; }
        }
        public Connection GetConnection(int index)
        {
            //if (index < 0 || index >= ConnectionsCount) return null;
            return _connections[index];
        }
        public void RemoveConnection(int index)
        {
            if (index < 0 || index >= ConnectionsCount) return;
            RemoveConnection(_connections[index]);
        }
        public void RemoveConnection(Connection connection)
        {
            if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(connection);
            _connections.Remove(connection);
        }
        public void AddConnection(Connection connection)
        {
            connection.End = this;
            _connections.Add(connection);
        }

        public Connection CreateConnection()
        {
            Connection inputConnection = new Connection();
            AddConnection(inputConnection);
            return inputConnection;
        }

        public bool CanConnect(OutputPort output)
        {
            return output != null &&
                   output.Node != null &&
                   output.Node != Node;
        }

        public void Connect(OutputPort output)
        {
            if (!CanConnect(output)) return;
            Connection connection = output.Connection;
            Connection.Connect(connection, this, output);
            //AddConnection(output.Connection);
        }

        public override bool CanConnect(Port port)
        {
            return !(port is InputPort) && CanConnect(port as OutputPort);
        }

        public override void Connect(Port port)
        {
            if (CanConnect(port)) ((OutputPort)port).Connect(this);
        }

        //public override void Disconnect(Port port)
        //{
        //    if (IsConnected) Disconnect(port as OutputPort);
        //}

        //public void Disconnect(OutputPort output)
        //{
        //    if (!IsConnectedTo(output)) return;
        //    output.Connection.Start = null;
        //}

        public override bool IsConnected
        {
            get { return ConnectionsCount != 0; }
        }

        public override bool IsConnectedTo(Port port)
        {
            return IsConnectedTo(port as OutputPort);
        }

        public bool IsConnectedTo(OutputPort output)
        {
            if (output == null) return false;

            bool result = false;
            foreach (Connection connection in _connections)
            {
                if (connection.Start != output) continue;
                result = true;
                break;
            }

            return result;
        }

        public override void ClearConnections()
        {
            if (UnityEngine.Application.isPlaying)
                for (int i = 0; i >= 0; i--)
                    UnityEngine.Object.Destroy(_connections[i]);

            _connections.Clear();
        }

        //public override void Reconnect(List<Node> oldNodes, List<Node> newNodes)
        //{
        //    foreach (Connection connection in _connections)
        //    {
        //        int index = oldNodes.IndexOf(connection.Start.Node);
        //        if (index >= 0) Connect(newNodes[index]);
        //    }
        //}

        //public void Redirect(List<Node> newNodes)
        //{
        //    foreach (Node newNode in newNodes)
        //    {
        //        ISingleOutput sOutput = newNode as ISingleOutput;
        //        if (sOutput != null) 
        //    }
        //}
    }
}
