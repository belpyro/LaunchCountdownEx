using System.Collections.Generic;
using System.Linq;
using NASA_CountDown.Config;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;

namespace NASA_CountDown.StateMachine.States
{
    public class SettingsStateSM : MonoBehaviour
    {
        private Rect _windowRect;
        private List<string> _soundsList;
        private int _audioSet;
        
        private void Start()
        {
            _windowRect = GUIUtil.ScreenCenteredRect(200, 290);
            _soundsList = BundleLoader.Instance.AudioSets.Keys.ToList();
        }

        private void OnGUI()
        {
            _windowRect = KSPUtil.ClampRectToScreen(GUILayout.Window(99, _windowRect, DrawSettingsWindow, "", BundleLoader.Instance.SettingsStyle));
            GUI.BringWindowToFront(99);
        }
        
        private void DrawSettingsWindow(int id)
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            ConfigInfo.Instance.AbortExecuted = GUILayout.Toggle(ConfigInfo.Instance.AbortExecuted, "Abort execute", BundleLoader.Instance.ToggleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Scale window {ConfigInfo.Instance.Scale.ToString("0.0")}", BundleLoader.Instance.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ConfigInfo.Instance.Scale = GUILayout.HorizontalSlider(ConfigInfo.Instance.Scale, .2f, 1f, GUILayout.MaxWidth(160f), GUILayout.MinWidth(140f));

            GUI.enabled = _soundsList.Any();

            ConfigInfo.Instance.IsSoundEnabled = _soundsList.Any() && GUILayout.Toggle(ConfigInfo.Instance.IsSoundEnabled, "Sound enabled", BundleLoader.Instance.ToggleStyle);

            if (_soundsList.Any())
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("Sound set", BundleLoader.Instance.LabelStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", BundleLoader.Instance.ButtonSoundBackStyle))
                {
                    _audioSet = _audioSet <= 0 ? _soundsList.Count - 1 : _audioSet - 1;
                    ConfigInfo.Instance.SoundSet = _soundsList[_audioSet];
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(ConfigInfo.Instance.SoundSet, BundleLoader.Instance.LabelStyle);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("", BundleLoader.Instance.ButtonSoundNextStyle))
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
                    new Rect((BundleLoader.Instance.SettingsStyle.fixedWidth - BundleLoader.Instance.ButtonBackStyle.fixedWidth) / 2,
                        BundleLoader.Instance.SettingsStyle.fixedHeight - BundleLoader.Instance.ButtonBackStyle.fixedHeight - 10,
                        BundleLoader.Instance.ButtonBackStyle.fixedWidth, BundleLoader.Instance.ButtonBackStyle.fixedHeight), string.Empty,
                    BundleLoader.Instance.ButtonBackStyle))
            {
                this.SendMessage("ChangeState", PluginEvents.OnInit, SendMessageOptions.RequireReceiver);
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}