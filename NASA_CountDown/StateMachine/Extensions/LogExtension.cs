using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Extensions;
using Appccelerate.StateMachine.Machine.States;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;

namespace NASA_CountDown.StateMachine.Extensions
{
    public class LogExtension : ExtensionBase<PluginStates,PluginEvents>
    {
        public override void SwitchedState(IStateMachineInformation<PluginStates, PluginEvents> stateMachine, IStateDefinition<PluginStates, PluginEvents> oldState, IStateDefinition<PluginStates, PluginEvents> newState)
        {
            if (oldState == null || newState == null) return;
            
            Debug.Log($"[StateMachine]: Switched from {oldState.Id} to {newState.Id}");
        }
    }
}