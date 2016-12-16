﻿using System;
using System.Linq;
using NASA_CountDown.States;

namespace NASA_CountDown.StateMachine
{
    public class KerbalFsmEx: KerbalFSM
    {
        public void RunEvent(string eName)
        {
            var foundEvent =
                this.CurrentState.StateEvents.FirstOrDefault(
                    x => x.name.Equals(eName, StringComparison.OrdinalIgnoreCase));

            if (foundEvent == null) return;

            this.RunEvent(foundEvent);
        }
    }
}