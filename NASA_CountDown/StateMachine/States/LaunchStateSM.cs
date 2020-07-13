using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;

namespace NASA_CountDown.StateMachine.States
{
    public class LaunchStateSM : InitialStateSM
    {
        private AudioSource _audioSource;
        private List<Action> _stages;

        protected override void Awake()
        {
            base.Awake();

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.volume = GameSettings.VOICE_VOLUME;
        }

        private void OnDisable()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightGlobals.ActiveVessel.OnFlyByWire =
                    (FlightInputCallback) Delegate.Remove(FlightGlobals.ActiveVessel.OnFlyByWire,
                        (FlightInputCallback) OnFlyByWire);  
            }
            GameEvents.onVesselSituationChange.Remove(SituationChanged);
            
            StopAllCoroutines();
        }

        private void OnEnable()
        {
            if (ConfigInfo.Instance.EngineControl && FlightGlobals.ActiveVessel != null)
            {
                FlightGlobals.ActiveVessel.OnFlyByWire =
                    (FlightInputCallback) Delegate.Combine(FlightGlobals.fetch.activeVessel.OnFlyByWire,
                        (FlightInputCallback) OnFlyByWire);
            }
            
            GameEvents.onVesselSituationChange.Add(SituationChanged);
            
            StartCoroutine(TickLaunch());
        }

        protected override void DrawButtons()
        {
            var buttonWidth = BundleLoader.Instance.ButtonLaunchStyle.fixedWidth;
            var buttonHeight = BundleLoader.Instance.ButtonLaunchStyle.fixedHeight;

            if (!GUI.Button(new Rect(_windowRect.xMin, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                string.Empty, BundleLoader.Instance.ButtonAbortStyle)) return;

            StopAllCoroutines();
            StartCoroutine(Abort());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            Destroy(_audioSource);
        }


        private IEnumerator Abort()
        {
            TimeWarp.SetRate(0, false);
            var clip = BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].Abort;

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

            SendMessage(nameof(IStateChange.ChangeState), PluginEvents.OnInit, SendMessageOptions.RequireReceiver);
        }

        private IEnumerator TickLaunch()
        {
            var count = ConfigInfo.Instance.IsSoundEnabled && BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].TimerSounds.Any()
                ? BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].TimerSounds.Count - 1
                : 15;
            _stages = ConfigInfo.Instance.Sequences[FlightGlobals.fetch.activeVessel.id].Select(x =>
                x < 0 ? () => { } : new Action(() => StageManager.ActivateStage(x))).ToList();

            for (var i = count; i >= 0; i--)
            {
                _tick = i;

                var sound = BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet]?.TimerSounds.SingleOrDefault(x =>
                    x.name.Equals(i.ToString(), StringComparison.OrdinalIgnoreCase));

                if (sound != null)
                {
                    _audioSource.PlayOneShot(sound);
                }

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
            if (data.host.id != FlightGlobals.fetch.activeVessel.id)
            {
                return;
            }

            switch (data.to)
            {
                case Vessel.Situations.FLYING:
                    SendMessage(nameof(IStateChange.ChangeState), PluginEvents.OnLaunched,
                        SendMessageOptions.RequireReceiver);
                    break;
                case Vessel.Situations.LANDED:
                    break;
                case Vessel.Situations.SPLASHED:
                case Vessel.Situations.SUB_ORBITAL:
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                case Vessel.Situations.DOCKED:
                    SendMessage(nameof(IStateChange.ChangeState), PluginEvents.OnFinish,
                        SendMessageOptions.RequireReceiver);
                    break;
                case Vessel.Situations.PRELAUNCH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}