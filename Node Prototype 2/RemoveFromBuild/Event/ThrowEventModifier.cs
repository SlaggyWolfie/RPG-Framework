﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG.Events;
using RPG.Nodes.Base;
using RPG.Other;
using UnityEngine;
using Event = RPG.Events.Event;

namespace RPG.Dialogue
{
    [Serializable]
    public sealed class ThrowEventModifier : ConnectionModifier
    {
        [SerializeField]
        private Event _event = null;
        public Event Event
        {
            get { return _event; }
            set { _event = value; }
        }

        public override void Execute() { if (Event != null) EventQueue.Instance.Send(Event); }

        //public override void ApplyDataFromCopy(ConnectionModifier original)
        //{
        //    ThrowEventModifier em = original as ThrowEventModifier;
        //    if (em == null) return;
        //    _event = em._event;
        //}

        //public override ConnectionModifier Copy() { return (ThrowEventModifier)MemberwiseClone(); }
    }
}
