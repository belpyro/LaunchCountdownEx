using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Extensions;
using NASA_CountDown.Helpers;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using NASA_CountDown.StateMachine.Extensions;
using NASA_CountDown.StateMachine.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NASA_CountDown
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT)]
    public class CountDownMain : ScenarioModule, IStateChange
    {
        private ApplicationLauncherButton _button;
        private PassiveStateMachine<PluginStates, PluginEvents> _sm;
        private Dictionary<PluginStates, MonoBehaviour> _states = new Dictionary<PluginStates, MonoBehaviour>();
        private GameObject _dialog1;
        private GameObject _dialog2;

        public override void OnAwake()
        {
            InitStates();
            InitMachine();
            _button = ApplicationLauncher.Instance.AddModApplication(() => _sm.Fire(PluginEvents.OnDummy),
                () => _sm.Fire(PluginEvents.OnInit), () => { }, () => { }, () => { }, () => { },
                ApplicationLauncher.AppScenes.FLIGHT,
                BundleLoader.Instance.Icon);
            var prefab =
                BundleLoader.Instance.Bundle.LoadAsset<GameObject>("Assets/Prefabs/CanvasTest.prefab".ToLower());
            // _dialog1 = Instantiate(prefab, this.transform, false);
            _dialog2 = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigInfo.Instance.Load(node);
            _sm.Start();
        }

        public override void OnSave(ConfigNode node)
        {
            ConfigInfo.Instance.Save(node);
        }

        public void ChangeState(PluginEvents smEvent)
        {
            _sm.Fire(smEvent);
        }

        public void OnDestroy()
        {
            _sm.Stop();
            Clear();
            ApplicationLauncher.Instance.RemoveModApplication(_button);
        }

        private void InitStates()
        {
            _states.Clear();
            _states.Add(PluginStates.Init, gameObject.AddComponent<InitialStateSM>());
            _states.Add(PluginStates.Settings, gameObject.AddComponent<SettingsStateSM>());
            _states.Add(PluginStates.Sequence, gameObject.AddComponent<SequenceStateFM>());
            _states.Add(PluginStates.Launch, gameObject.AddComponent<LaunchStateSM>());
            _states.Add(PluginStates.Launched, gameObject.AddComponent<LaunchedStateSM>());

            foreach (var component in _states.Values)
            {
                component.enabled = false;
            }
        }

        private void InitMachine()
        {
            var builder = new StateMachineDefinitionBuilder<PluginStates, PluginEvents>();

            builder.In(PluginStates.Init)
                .ExecuteOnEntry(() => { _states[PluginStates.Init].enabled = true; })
                .ExecuteOnExit(() => { _states[PluginStates.Init].enabled = false; })
                .On(PluginEvents.OnSettings).Goto(PluginStates.Settings)
                .On(PluginEvents.OnSequence).Goto(PluginStates.Sequence)
                .On(PluginEvents.OnLauch).Goto(PluginStates.Launch)
                .On(PluginEvents.OnDummy).Goto(PluginStates.Dummy);

            builder.In(PluginStates.Dummy).ExecuteOnEntry(() =>
            {
                foreach (var component in _states.Values)
                {
                    component.enabled = false;
                }
            }).On(PluginEvents.OnInit).Goto(PluginStates.Init);

            builder.In(PluginStates.Settings)
                .ExecuteOnEntry(() => _states[PluginStates.Settings].enabled = true)
                .ExecuteOnExit(() => _states[PluginStates.Settings].enabled = false)
                .On(PluginEvents.OnInit).Goto(PluginStates.Init);

            builder.In(PluginStates.Sequence).ExecuteOnEntry(() => _states[PluginStates.Sequence].enabled = true)
                .ExecuteOnExit(() => _states[PluginStates.Sequence].enabled = false)
                .On(PluginEvents.OnInit).Goto(PluginStates.Init);

            builder.In(PluginStates.Launch).ExecuteOnEntry(() => _states[PluginStates.Launch].enabled = true)
                .ExecuteOnExit(() => _states[PluginStates.Launch].enabled = false)
                .On(PluginEvents.OnInit).Goto(PluginStates.Init)
                .On(PluginEvents.OnLaunched).Goto(PluginStates.Launched)
                .On(PluginEvents.OnFinish).Goto(PluginStates.Finish);

            builder.In(PluginStates.Launched).ExecuteOnEntry(() => _states[PluginStates.Launched].enabled = true)
                .ExecuteOnExit(() => _states[PluginStates.Launched].enabled = false)
                .On(PluginEvents.OnFinish).Goto(PluginStates.Finish);

            builder.In(PluginStates.Finish).ExecuteOnEntry(() =>
            {
                Clear();
                ApplicationLauncher.Instance.RemoveModApplication(_button);
                _sm.Stop();
            });

            _sm = builder.WithInitialState(PluginStates.Init).Build().CreatePassiveStateMachine();
            _sm.AddExtension(new LogExtension());
        }

        private void Clear()
        {
            foreach (var component in _states.Values)
            {
                component.enabled = false;
            }

            gameObject
                .TryToRemoveComponent<InitialStateSM>()
                .TryToRemoveComponent<SettingsStateSM>()
                .TryToRemoveComponent<SequenceStateFM>()
                .TryToRemoveComponent<LaunchStateSM>()
                .TryToRemoveComponent<LaunchedStateSM>();
        }
    }
}