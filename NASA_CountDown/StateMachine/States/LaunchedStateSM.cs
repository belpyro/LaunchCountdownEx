using System;
using System.Collections;
using NASA_CountDown.Config;
using NASA_CountDown.Helpers;
using NASA_CountDown.Loaders;
using NASA_CountDown.StateMachine.Common;
using UnityEngine;

namespace NASA_CountDown.StateMachine.States
{
    public class LaunchedStateSM : MonoBehaviour
    {
        private AudioSource _audioSource;
        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.volume = GameSettings.VOICE_VOLUME;
            
            StartCoroutine(this.LaunchedSuccess());
        }

        private IEnumerator LaunchedSuccess()
        {
            var clip = BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].LiftOff;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            clip = BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].AllEngineRunning;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            clip = BundleLoader.Instance.AudioSets[ConfigInfo.Instance.SoundSet].TowerCleared;

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }

            this.SendMessage("ChangeState", PluginEvents.OnFinish, SendMessageOptions.RequireReceiver);
        }
        
        private void OnDestroy()
        {
            DestroyImmediate(_audioSource);
        }
    }
}