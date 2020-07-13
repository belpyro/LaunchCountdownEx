using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NASA_CountDown.Helpers;
using UnityEngine;

namespace NASA_CountDown.Loaders
{
    public class BundleLoader : LoadingSystem
    {
        private bool _isReady = false;
        private float _progressDelta = 0.0f;
        private float _progressFraction = 0.0f;
        private AssetBundle _bundle;
        private string _progressTitle;
        public static BundleLoader Instance { get; private set; }

        public AssetBundle Bundle => _bundle;
        
        public Texture2D Icon { get; private set; }
        
        public Dictionary<string, AudioSet> AudioSets { get; private set; }
        
        public GUIStyle ButtonLaunchStyle { get; private set; }

        public GUIStyle ButtonSettingsStyle { get; private set; }

        public GUIStyle ButtonSequenceStyle { get; private set; }

        public GUIStyle ButtonSoundBackStyle { get; private set; }

        public GUIStyle ButtonSoundNextStyle { get; private set; }

        public GUIStyle ButtonAbortStyle { get; private set; }

        public GUIStyle ButtonBackStyle { get; private set; }

        public GUIStyle MainWindowStyle { get; private set; }

        public GUIStyle LabelStyle { get; private set; }

        public GUIStyle ToggleStyle { get; private set; }

        public GUIStyle TextBoxStyle { get; private set; }

        public GUIStyle ErrorTextBoxStyle { get; private set; }

        public GUIStyle LaunchSequenceStyle { get; private set; }

        public GUIStyle SettingsStyle { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            AudioSets = new Dictionary<string, AudioSet>();
            _progressTitle = "NASA CountDown";
        }

        public override string ProgressTitle()
        {
            return _progressTitle;
        }

        public override bool IsReady()
        {
            return _isReady;
        }

        public override void StartLoad()
        {
            StartCoroutine(LoadResources());
        }

        public override float ProgressFraction()
        {
            return _progressFraction;
        }

        private IEnumerator LoadResources()
        {
            _bundle = AssetBundle.LoadFromFile(
                Path.Combine(Path.GetDirectoryName(typeof(CountDownMain).Assembly.Location), "launchcountdown"));
            
            Icon = _bundle.LoadAsset<Texture2D>("Assets/Icons/launch_icon_normal.png");
            var names = _bundle.GetAllAssetNames();
            _progressDelta = 1.0f / names.Length;
            yield return StartCoroutine(LoadSounds(names.Where(x => x.Contains("sounds")).ToArray()));
            yield return StartCoroutine(LoadStyles());

            _isReady = true;
        }

        private IEnumerator LoadSounds(string[] names)
        {
            var sounds = names
                .Select(x => new {Path = x, Segments = x.Split('/')})
                .GroupBy(x => x.Segments[2])
                .ToArray();

            foreach (var sound in sounds)
            {
                var timers = sound.Where(c => c.Path.Contains("timer"));
                var audioSet = new AudioSet(sound.Key)
                {
                    TimerSounds = timers.Select(t => _bundle.LoadAsset<AudioClip>(t.Path)).ToList(),
                    Abort = _bundle.LoadAsset<AudioClip>(sound.FirstOrDefault(u => u.Path.Contains("aborted"))?.Path ?? "none"),
                    LiftOff = _bundle.LoadAsset<AudioClip>(sound.FirstOrDefault(u => u.Path.Contains("liftoff"))?.Path ?? "none"),
                    TowerCleared = _bundle.LoadAsset<AudioClip>(sound.FirstOrDefault(u => u.Path.Contains("towercleared"))?.Path ?? "none"),
                    AllEngineRunning = _bundle.LoadAsset<AudioClip>(sound.FirstOrDefault(u => u.Path.Contains("allenginerunning"))?.Path ?? "none"),
                };
                AudioSets.Add(audioSet.Name, audioSet);
                _progressFraction += _progressDelta;
            }
            
            yield break;
        }

        private IEnumerator LoadStyles()
        {
            var skin = _bundle.LoadAsset<GUISkin>("assets/countdownskin.guiskin");
            MainWindowStyle = skin.window;
            LaunchSequenceStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "LaunchSequenceStyle", StringComparison.CurrentCultureIgnoreCase));
            SettingsStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "SettingsStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonAbortStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonAbortStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonBackStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonBackStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonLaunchStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonLaunchStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonSequenceStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonSequenceStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonSettingsStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonSettingsStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonSoundBackStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonSoundBackStyle", StringComparison.CurrentCultureIgnoreCase));
            ButtonSoundNextStyle = skin.customStyles.Single(s =>
                String.Equals(s.name, "ButtonSoundNextStyle", StringComparison.CurrentCultureIgnoreCase));

            LabelStyle = new GUIStyle(HighLogic.Skin.label) {alignment = TextAnchor.MiddleCenter};

            ToggleStyle = new GUIStyle(HighLogic.Skin.toggle) {margin = new RectOffset(12, 12, 4, 4)};

            TextBoxStyle = new GUIStyle(HighLogic.Skin.textField) {fixedWidth = 50};

            ErrorTextBoxStyle = new GUIStyle(HighLogic.Skin.textField)
            {
                normal = {textColor = Color.red},
                fixedWidth = 50
            };
            yield break;
        }
        
        public void Reload(float scale)
        {
            MainWindowStyle.fixedHeight = Mathf.RoundToInt(MainWindowStyle.fixedHeight * scale);
            MainWindowStyle.fixedWidth = Mathf.RoundToInt(MainWindowStyle.fixedWidth * scale);

            ButtonAbortStyle.fixedHeight = Mathf.RoundToInt(ButtonAbortStyle.fixedHeight * scale);
            ButtonAbortStyle.fixedWidth = Mathf.RoundToInt(ButtonAbortStyle.fixedWidth * scale);

            ButtonLaunchStyle.fixedHeight = Mathf.RoundToInt(ButtonLaunchStyle.fixedHeight * scale);
            ButtonLaunchStyle.fixedWidth = Mathf.RoundToInt(ButtonLaunchStyle.fixedWidth * scale);

            ButtonSequenceStyle.fixedHeight =
                Mathf.RoundToInt(ButtonSequenceStyle.fixedHeight * scale);
            ButtonSequenceStyle.fixedWidth =
                Mathf.RoundToInt(ButtonSequenceStyle.fixedWidth * scale);

            ButtonSettingsStyle.fixedHeight =
                Mathf.RoundToInt(ButtonSettingsStyle.fixedHeight * scale);
            ButtonSettingsStyle.fixedWidth =
                Mathf.RoundToInt(ButtonSettingsStyle.fixedWidth * scale);
        }
        
        public Texture2D GetTexture(string name)
        {
            return _bundle.LoadAsset<Texture2D>($"assets/images/{name.ToLower()}.png");
        }
    }
}