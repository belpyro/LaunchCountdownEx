using System;
using System.Collections.Generic;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.StateMachine;
using UniLinq;
using UnityEngine;

namespace NASA_CountDown.States
{
    public sealed class SettingState : BaseGuiState
    {
        private Rect _windowRect;
        private List<string> _soundsList;
        private int _audioSet;

        public SettingState(string name, KerbalFsmEx machine) : base(name, machine)
        {
            _windowRect = GUIUtil.ScreenCenteredRect(200, 290);
            OnEnter = OnEnterToState;
        }

        private void OnEnterToState(KFSMState kfsmState)
        {
            _soundsList = ConfigInfo.Instance.AudioSets.Keys.ToList();
        }

        protected override void OnGui()
        {
            _windowRect = KSPUtil.ClampRectToScreen(GUILayout.Window(99, _windowRect, DrawSettingsWindow, "", StyleFactory.SettingsStyle));
            GUI.BringWindowToFront(99);
        }

        private void DrawSettingsWindow(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            ConfigInfo.Instance.AbortExecuted = GUILayout.Toggle(ConfigInfo.Instance.AbortExecuted, "Abort execute", StyleFactory.ToggleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Scale window {ConfigInfo.Instance.Scale.ToString("0.0")}", StyleFactory.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ConfigInfo.Instance.Scale = GUILayout.HorizontalSlider(ConfigInfo.Instance.Scale, .2f, 1f, GUILayout.MaxWidth(160f), GUILayout.MinWidth(140f));

            GUI.enabled = _soundsList.Any();

            ConfigInfo.Instance.IsSoundEnabled = _soundsList.Any() && GUILayout.Toggle(ConfigInfo.Instance.IsSoundEnabled, "Sound enabled", StyleFactory.ToggleStyle);

            if (_soundsList.Any())
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("Sound set", StyleFactory.LabelStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", StyleFactory.ButtonSoundBackStyle))
                {
                    _audioSet = _audioSet <= 0 ? _soundsList.Count - 1 : _audioSet - 1;
                    ConfigInfo.Instance.SoundSet = _soundsList[_audioSet];
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(ConfigInfo.Instance.SoundSet, StyleFactory.LabelStyle);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", StyleFactory.ButtonSoundNextStyle))
                {
                    _audioSet = _audioSet >= _soundsList.Count - 1 ? 0 : _audioSet + 1;
                    ConfigInfo.Instance.SoundSet = _soundsList[_audioSet];
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUI.enabled = true;

            if (
                GUI.Button(
                    new Rect((StyleFactory.SettingsStyle.fixedWidth - StyleFactory.ButtonBackStyle.fixedWidth) / 2,
                        StyleFactory.SettingsStyle.fixedHeight - StyleFactory.ButtonBackStyle.fixedHeight - 10,
                        StyleFactory.ButtonBackStyle.fixedWidth, StyleFactory.ButtonBackStyle.fixedHeight), string.Empty,
                    StyleFactory.ButtonBackStyle))
            {
                Machine.RunEvent("Init");
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}