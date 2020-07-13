using NASA_CountDown.StateMachine.Common;

namespace NASA_CountDown
{
    public interface IStateChange
    {
        void ChangeState(PluginEvents smEvent);
    }
}