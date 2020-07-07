using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UnityEngine;

namespace NASA_CountDown.States
{
    public class LaunchState : InitialState
    {
        private AudioSource _audioSource;
        private List<Action> _stages;

        public LaunchState(string name, KerbalFsmEx machine) : base(name, machine)
        {
        }

        protected override void OnEnterToState(KFSMState kfsmState)
        {
            base.OnEnterToState(kfsmState);

            _audioSource = _obj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.volume = GameSettings.VOICE_VOLUME;

            if (ConfigInfo.Instance.EngineControl)
            {
                FlightGlobals.ActiveVessel.OnFlyByWire =
                    (FlightInputCallback) Delegate.Combine(FlightGlobals.ActiveVessel.OnFlyByWire,
                        (FlightInputCallback) OnFlyByWire);
            }

            GameEvents.onVesselSituationChange.Add(SituationChanged);

            _stages = ConfigInfo.Instance.Sequences[FlightGlobals.ActiveVessel.id].Select(x =>
                x < 0 ? new Action(() => { }) : new Action(() => StageManager.ActivateStage(x))).ToList();

            _dummy.StartCoroutine(this.TickLaunch());
        }

        protected override void OnLeaveFromState(KFSMState kfsmState)
        {
            base.OnLeaveFromState(kfsmState);
            FlightGlobals.ActiveVessel.OnFlyByWire =
                (FlightInputCallback) Delegate.Remove(FlightGlobals.ActiveVessel.OnFlyByWire,
                    (FlightInputCallback) OnFlyByWire);
            GameEvents.onVesselSituationChange.Remove(SituationChanged);
        }

        protected override void DrawButtons()
        {
            var buttonWidth = StyleFactory.ButtonLaunchStyle.fixedWidth;
            var buttonHeight = StyleFactory.ButtonLaunchStyle.fixedHeight;

            if (GUI.Button(new Rect(_windowRect.xMin, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                string.Empty, StyleFactory.ButtonAbortStyle))
            {
                _dummy.StopAllCoroutines();
                _dummy.StartCoroutine(Abort());
            }
        }

        private IEnumerator Abort()
        {
            TimeWarp.SetRate(0, false);
            var clip = ConfigInfo.Instance.CurrentAudio.Abort;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            if (ConfigInfo.Instance.AbortExecuted)
            {
                BaseAction.FireAction(FlightGlobals.ActiveVessel.Parts, KSPActionGroup.Abort, 0,
                    KSPActionType.Activate);
            }

            Machine.RunEvent("Init");
        }

        private IEnumerator TickLaunch()
        {
            var count = ConfigInfo.Instance.IsSoundEnabled && ConfigInfo.Instance.CurrentAudio.TimerSounds.Any()
                ? ConfigInfo.Instance.CurrentAudio.TimerSounds.Count - 1
                : 15;

            for (var i = count; i >= 0; i--)
            {
                _tick = i;

                _audioSource.PlayOneShot(
                    ConfigInfo.Instance.CurrentAudio.TimerSounds.FirstOrDefault(x => x.name.EndsWith($"/{i}")));

                if (_stages.Count > i)
                {
                    _stages[i]();
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        private void OnFlyByWire(FlightCtrlState st)
        {
            switch (_tick)
            {
                case 7:
                case 6:
                case 5:
                case 4:
                case 3:
                case 2:
                case 1:
                    st.mainThrottle = 0.01f;
                    break;
                case 0:
                    st.mainThrottle = 1f;
                    break;
                default:
                    st.mainThrottle = 0f;
                    break;
            }
        }

        private void SituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            switch (data.to)
            {
                case Vessel.Situations.FLYING:
                    Machine.RunEvent("Launched");
                    break;
                case Vessel.Situations.LANDED:
                    break;
                case Vessel.Situations.SPLASHED:
                case Vessel.Situations.SUB_ORBITAL:
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                case Vessel.Situations.DOCKED:
                    Machine.RunEvent("Finish");
                    break;
                case Vessel.Situations.PRELAUNCH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}