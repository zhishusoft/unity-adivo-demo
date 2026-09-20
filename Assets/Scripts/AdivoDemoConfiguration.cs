using System;
using System.IO;
using UnityEngine;

namespace Adivo.Demo
{
    [Serializable]
    public sealed class AdivoDemoConfiguration
    {
        public string sdkKey;
        public string rewardedAdUnitId;
        public string interstitialAdUnitId;
        public bool testMode = true;
        public bool canRequestAds = true;
        public bool hasUserConsent;
        public bool doNotSell;
        public bool verboseLogging = true;
        public string[] testDeviceAdvertisingIdentifiers = Array.Empty<string>();
        public bool googleEnabled;
        public string googleAppId;
        public string bundleIdentifier = "com.example.yourgame";
        public string developmentTeam = string.Empty;

        internal static AdivoDemoConfiguration Load()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Adivo.local.json");
            if (File.Exists(path)) return JsonUtility.FromJson<AdivoDemoConfiguration>(File.ReadAllText(path));
            return new AdivoDemoConfiguration
            {
                sdkKey = Application.isEditor ? "EDITOR_SIMULATION" : string.Empty,
                rewardedAdUnitId = Application.isEditor ? "SIMULATED_REWARDED" : string.Empty,
                interstitialAdUnitId = Application.isEditor ? "SIMULATED_INTERSTITIAL" : string.Empty
            };
        }
    }
}
