using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NASA_CountDown.Extensions;
using NASA_CountDown.Helpers;
using NASA_CountDown.Loaders;
using UnityEngine;

namespace NASA_CountDown.Config
{
    class ConfigInfo : IConfigNode
    {
        private static ConfigInfo _config;
        private float _scale = 1;

        internal bool IsSoundEnabled { get; set; }

        internal string SoundSet { get; set; }

        internal Dictionary<Guid, int[]> Sequences { get; set; } = new Dictionary<Guid, int[]>();

        internal Rect WindowPosition { get; set; } = GUIUtil.ScreenCenteredRect(459, 120);

        internal bool EngineControl { get; set; } = true;

        internal bool AbortExecuted { get; set; } = true;

        internal float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                BundleLoader.Instance.Reload(value);
            }
        }

        internal bool IsLoaded { get; private set; }

        internal static ConfigInfo Instance => _config ?? (_config = new ConfigInfo());

        public void Load(ConfigNode node)
        {
            try
            {
                if (node == null) return;

                if (node.HasValue("soundEnabled"))
                {
                    IsSoundEnabled = bool.Parse(node.GetValue("soundEnabled"));
                }

                if (node.HasValue("engineControl"))
                {
                    EngineControl = bool.Parse(node.GetValue("engineControl"));
                }

                if (node.HasValue("abort"))
                {
                    AbortExecuted = bool.Parse(node.GetValue("abort"));
                }

                if (node.HasValue("scale"))
                {
                    Scale = float.Parse(node.GetValue("scale"));
                }

                if (node.HasValue("soundSet"))
                {
                    SoundSet = node.GetValue("soundSet");
                }

                if (node.HasNode("window"))
                {
                    var current = node.GetNode("window") ?? new ConfigNode();
                    var position = current.GetValue("position") ?? "100";
                    var size = current.GetValue("size") ?? "100";
                    WindowPosition = new Rect(KSPUtil.ParseVector2(position), KSPUtil.ParseVector2(size));
                }

                if (node.HasNode("sequence"))
                {
                    var sequences = node.GetNodes("sequence");

                    Sequences.Clear();

                    foreach (var sequence in sequences)
                    {
                        Sequences.Add(new Guid(sequence.GetValue("id")),
                            sequence.GetValue("stages").Split(',').Select(int.Parse).ToArray());
                    }
                }

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Cannot load config");
                Debug.LogException(ex);
                IsLoaded = false;
            }
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("soundEnabled", IsSoundEnabled);
            node.TryAddValue("soundSet", SoundSet);

            node.AddValue("engineControl", EngineControl);
            node.AddValue("abort", AbortExecuted);
            node.AddValue("scale", Scale);

            var current = node.AddNode("window");
            current.AddValue("position", KSPUtil.WriteVector(WindowPosition.position));
            current.AddValue("size", KSPUtil.WriteVector(WindowPosition.size));

            foreach (var sequence in Sequences)
            {
                var seqNode = node.AddNode("sequence");

                seqNode.AddValue("id", sequence.Key);

                var value = sequence.Value.Select(x => x.ToString()).Aggregate((x, y) => $"{x},{y}");

                seqNode.TryAddValue("stages", value);
            }
        }
    }
}