using System;
using System.Collections;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;

namespace NASA_CountDown.StateMachine.States
{
    public class InitialStateSM : MonoBehaviour
    {
        protected Rect _windowRect;
        protected int _tick = 0;
        protected float _delta;
        private bool _buttonOpened;

        protected virtual void Awake()
        {
            _windowRect = ScaleRect(GUIUtil.ScreenCenteredRect(459, 120));
        }

        private void OnGUI()
        {
            _windowRect = KSPUtil.ClampRectToScreen(GUI.Window(99, _windowRect, DrawMainWindow, "",
                BundleLoader.Instance.MainWindowStyle));

            ConfigInfo.Instance.WindowPosition = _windowRect;

            if (Event.current.type == EventType.Repaint)
            {
                if (_buttonOpened)
                {
                    var openedRect = new Rect(_windowRect.xMin, _windowRect.yMin,
                        _windowRect.width, _windowRect.height + 29);

                    this.StartCoroutine(
                        openedRect.Contains(new Vector2(Input.mousePosition.x,
                            Screen.height - Input.mousePosition.y))
                            ? ShowBottomButton()
                            : HideBottomButton());
                }
                else
                {
                    this.StartCoroutine(
                        _windowRect.Contains(new Vector2(Input.mousePosition.x,
                            Screen.height - Input.mousePosition.y))
                            ? ShowBottomButton()
                            : HideBottomButton());
                }
            }

            DrawButtons();

            GUI.BringWindowToFront(99);
        }

        private void DrawMainWindow(int id)
        {
            GUILayout.BeginHorizontal();

            GUI.DrawTexture(ScaleRect(new Rect(13, 41, 25, 27)), BundleLoader.Instance.GetTexture("minus"));
            GUI.DrawTexture(ScaleRect(new Rect(45, 14, 54, 77)), BundleLoader.Instance.GetTexture("Digit0"));
            GUI.DrawTexture(ScaleRect(new Rect(98, 14, 54, 77)), BundleLoader.Instance.GetTexture("Digit0"));

            GUI.DrawTexture(ScaleRect(new Rect(166, 28, 16, 51)), BundleLoader.Instance.GetTexture("colon"));

            GUI.DrawTexture(ScaleRect(new Rect(190, 14, 54, 77)), BundleLoader.Instance.GetTexture("Digit0"));
            GUI.DrawTexture(ScaleRect(new Rect(247, 14, 54, 77)), BundleLoader.Instance.GetTexture("Digit0"));

            GUI.DrawTexture(ScaleRect(new Rect(316, 28, 16, 51)), BundleLoader.Instance.GetTexture("colon"));

            int firstDigit = _tick / 10;
            int secondDigit = _tick % 10;

            //seconds
            GUI.DrawTexture(ScaleRect(new Rect(342, 14, 54, 77)),
                BundleLoader.Instance.GetTexture($"Digit{firstDigit}"));
            GUI.DrawTexture(ScaleRect(new Rect(396, 14, 54, 77)),
                BundleLoader.Instance.GetTexture($"Digit{secondDigit}"));

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }


        protected virtual void DrawButtons()
        {
            var buttonWidth = BundleLoader.Instance.ButtonLaunchStyle.fixedWidth;
            var buttonHeight = BundleLoader.Instance.ButtonLaunchStyle.fixedHeight;

            //launch
            if (GUI.Button(new Rect(_windowRect.xMin, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                string.Empty,
                BundleLoader.Instance.ButtonLaunchStyle))
            {
                this.SendMessage("ChangeState", PluginEvents.OnLauch, SendMessageOptions.RequireReceiver);
            }

            //sequence
            if (
                GUI.Button(
                    new Rect(_windowRect.xMin + buttonWidth, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                    string.Empty,
                    BundleLoader.Instance.ButtonSequenceStyle))
            {
                // Machine.RunEvent("Sequence");
                this.SendMessage("ChangeState", PluginEvents.OnSequence, SendMessageOptions.RequireReceiver);
            }

            //settings
            if (
                GUI.Button(
                    new Rect(_windowRect.xMin + buttonWidth * 2, _windowRect.yMax - _delta, buttonWidth, buttonHeight),
                    string.Empty,
                    BundleLoader.Instance.ButtonSettingsStyle))
            {
                // Machine.RunEvent("Settings");
                this.SendMessage("ChangeState", PluginEvents.OnSettings, SendMessageOptions.RequireReceiver);
            }
        }

        private Rect ScaleRect(Rect r)
        {
            var factor = ConfigInfo.Instance.Scale;
            return new Rect(
                r.xMin * factor,
                r.yMin * factor,
                r.width * factor,
                r.height * factor
            );
        }

        private IEnumerator ShowBottomButton()
        {
            while (_delta > 0)
            {
                _delta--;
                yield return new WaitForEndOfFrame();
            }

            _buttonOpened = true;
        }

        private IEnumerator HideBottomButton()
        {
            while (_delta < 34)
            {
                _delta++;
                yield return new WaitForEndOfFrame();
            }

            _buttonOpened = false;
        }
    }
}