using System;
using System.Collections.Generic;
using System.Linq;
using NASA_CountDown.Config;
using UnityEngine;

namespace NASA_CountDown.Helpers
{
    public class AudioSet
    {
        public AudioSet(string name)
        {
            Name = name;

            // var sounds = GameDatabase.Instance.databaseAudio.Where(x => x.name.StartsWith(ConfigInfo.Instance.FolderName, StringComparison.OrdinalIgnoreCase) && x.name.Contains(name)).ToList();
            //
            // TimerSounds = sounds.Where(x => x.name.Contains("/Timer")).ToList();
            // Abort = sounds.FirstOrDefault(x => x.name.EndsWith("Aborted", StringComparison.OrdinalIgnoreCase));
            // LiftOff = sounds.FirstOrDefault(x => x.name.EndsWith("LiftOff", StringComparison.OrdinalIgnoreCase));
            // AllEngineRunning = sounds.FirstOrDefault(x => x.name.EndsWith("AllEngineRuning", StringComparison.OrdinalIgnoreCase));
            // TowerCleared = sounds.FirstOrDefault(x => x.name.EndsWith("TowerCleared", StringComparison.OrdinalIgnoreCase));
        }

        public List<AudioClip> TimerSounds { get; set; }

        public string Name { get; private set; }

        public AudioClip Abort { get; set; }

        public AudioClip LiftOff { get; set; }

        public AudioClip TowerCleared { get; set; }

        public AudioClip AllEngineRunning { get; set; }
    }
}
