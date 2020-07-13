using System;
using UnityEngine;

namespace NASA_CountDown.Loaders
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class BundleLoaderAddon : MonoBehaviour
    {
        private void Awake()
        {
            LoadingScreen.Instance.loaders.Add(gameObject.AddComponent<BundleLoader>());
        }
    }
}