using System;
using System.Linq;
using KSP.UI;
using KSP.UI.Screens;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NASA_CountDown.StateMachine.States
{
    public class SequenceStateFM : MonoBehaviour
    {
        private Rect _windowRect = GUIUtil.ScreenCenteredRect(270, 400);
        private bool _isEditorState;
        private int _stageIndex;
        
        private void Start()
        {
            if (!ConfigInfo.Instance.Sequences.ContainsKey(FlightGlobals.ActiveVessel.id))
            {
                ConfigInfo.Instance.Sequences.Add(FlightGlobals.ActiveVessel.id, Enumerable.Repeat(-1, 10).ToArray());
            }

            StageManager.Instance.Stages.ForEach(@group => group.Icons.ForEach(icon => icon.radioButton.onClick.AddListener(OnClickButton)));
        }

        private void OnGUI()
        {
            _windowRect = KSPUtil.ClampRectToScreen(GUI.Window(99, _windowRect, DrawSequenceWindow, "", BundleLoader.Instance.LaunchSequenceStyle));
            GUI.BringWindowToFront(99);
        }
        
        private void OnDestroy()
        {
            StageManager.Instance.Stages.ForEach(@group => group.Icons.ForEach(icon => icon.radioButton.onClick.RemoveListener(OnClickButton)));
        }
        
        private void OnClickButton(PointerEventData arg0, UIRadioButton.State arg1, UIRadioButton.CallType arg2)
        {
            if (!_isEditorState) return;
        
            var stage = arg0.pointerPress.GetComponentInParent<StageGroup>();

            if (stage == null) return;
            
            ConfigInfo.Instance.Sequences[FlightGlobals.fetch.activeVessel.id][_stageIndex] = stage.inverseStageIndex;
            _isEditorState = false;
        }
        
        private void DrawSequenceWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            ConfigInfo.Instance.EngineControl = GUILayout.Toggle(ConfigInfo.Instance.EngineControl, "Engine control", BundleLoader.Instance.ToggleStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Elapsed time", BundleLoader.Instance.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Stage number", BundleLoader.Instance.LabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < 10; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label($"{i + 1} second", BundleLoader.Instance.LabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(ConfigInfo.Instance.Sequences[FlightGlobals.fetch.activeVessel.id][i] < 0 ? "none" : ConfigInfo.Instance.Sequences[FlightGlobals.fetch.activeVessel.id][i].ToString(), BundleLoader.Instance.LabelStyle, GUILayout.MinWidth(40));

                bool flag = _isEditorState && i != _stageIndex;
                string label = _isEditorState && i == _stageIndex ? "Done" : "Set";

                GUI.enabled = !flag;

                if (GUILayout.Button(label))
                {
                    _isEditorState = !_isEditorState;
                    _stageIndex = i;
                }

                GUI.enabled = true;

                if (GUILayout.Button("Clear"))
                {
                    _isEditorState = false;
                    _stageIndex = -1;
                    ConfigInfo.Instance.Sequences[FlightGlobals.fetch.activeVessel.id][i] = -1;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(), BundleLoader.Instance.ButtonBackStyle))
            {
                this.SendMessage("ChangeState", PluginEvents.OnInit, SendMessageOptions.RequireReceiver);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}