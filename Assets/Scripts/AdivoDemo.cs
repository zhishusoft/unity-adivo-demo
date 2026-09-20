using System;
using System.Text;
using System.Threading.Tasks;
using Adivo.Ads;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Adivo.Demo
{
    public sealed class AdivoDemo : MonoBehaviour
    {
        private readonly StringBuilder log = new StringBuilder();
        private AdivoDemoConfiguration settings;
        private Text status;
        private Text rewards;
        private int rewardCount;

        private void Start()
        {
            settings = AdivoDemoConfiguration.Load();
            BuildInterface();
            AdivoAds.EventReceived += OnEvent;
            AdivoAds.RewardEarned += reward => Append($"奖励事件 session={ShortSession(reward.SessionId)} {reward.Amount} {reward.Label}");
            AdivoAds.RevenuePaid += revenue => Append($"收入 [已隐藏] {revenue.Currency} / {revenue.Network}");
            Append(Application.isEditor ? "Editor 模拟模式" : "iOS MAX 模式");
        }

        private void OnDestroy()
        {
            AdivoAds.EventReceived -= OnEvent;
        }

        private async void InitializeAds()
        {
            await Run("初始化", async () =>
            {
                var configuration = new AdivoConfiguration()
                    .Add("revive", settings.rewardedAdUnitId, AdivoAdFormat.Rewarded)
                    .Add("level_end", settings.interstitialAdUnitId, AdivoAdFormat.Interstitial);
                var options = new AdivoMaxOptions
                {
                    SdkKey = settings.sdkKey, TestMode = settings.testMode,
                    HasUserConsent = settings.hasUserConsent, DoNotSell = settings.doNotSell,
                    VerboseLogging = settings.verboseLogging,
                    TestDeviceAdvertisingIdentifiers = settings.testDeviceAdvertisingIdentifiers ?? Array.Empty<string>()
                };
                await AdivoAds.InitializeAsync(configuration, options, new AdivoPrivacyState(settings.canRequestAds));
            });
        }

        private void LoadRewarded() => RunVoid("加载激励", () => AdivoAds.LoadAsync("revive"));
        private void LoadInterstitial() => RunVoid("加载插屏", () => AdivoAds.LoadAsync("level_end"));

        private void ShowRewarded(bool editorShouldReward)
        {
            AdivoAds.SetEditorRewardOutcome(editorShouldReward);
            RunVoid("展示激励", () => AdivoAds.ShowAsync("revive", reward =>
            {
                rewardCount++;
                rewards.text = $"有效奖励：{rewardCount} 次";
                Append($"业务发奖成功 session={ShortSession(reward.SessionId)}");
            }));
        }

        private void ShowInterstitial() => RunVoid("展示插屏", () => AdivoAds.ShowAsync("level_end"));

        private async void RunVoid(string name, Func<Task> operation) => await Run(name, operation);

        private async Task Run(string name, Func<Task> operation)
        {
            try { Append(name + "开始"); await operation(); Append(name + "完成"); }
            catch (AdivoException error) { Append($"{name}失败 [{error.Code}] {error.Message}"); }
            catch (Exception error) { Append($"{name}失败 {error.Message}"); }
        }

        private void OnEvent(AdivoEvent evt) => Append($"事件 {evt.Kind} placement={evt.Placement} session={ShortSession(evt.SessionId)}");

        internal static string ShortSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return string.Empty;
            return sessionId.Length <= 8 ? sessionId : sessionId.Substring(0, 8);
        }

        private void Append(string message)
        {
            Debug.Log("[Adivo Demo] " + message);
            log.AppendLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
            if (log.Length > 5000) log.Remove(0, 2000);
            if (status != null) status.text = log.ToString();
        }

        private void BuildInterface()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1170, 2532);
            var safeArea = CreateObject("SafeArea", canvas.transform, typeof(RectTransform));
            Stretch(safeArea.GetComponent<RectTransform>(), 0, 0, 0, 0);
            safeArea.AddComponent<AdivoSafeArea>();
            var panel = CreateObject("Panel", safeArea.transform, typeof(Image));
            panel.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 1);
            Stretch(panel.GetComponent<RectTransform>(), 32, 32, 48, 48);
            var layout = panel.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(32, 32, 32, 32); layout.spacing = 18; layout.childForceExpandHeight = false;

            MakeText(panel.transform, "Adivo Ads · Unity iOS Demo", 44, FontStyle.Bold, 70);
            rewards = MakeText(panel.transform, "有效奖励：0 次", 32, FontStyle.Bold, 52);
            MakeButton(panel.transform, "初始化", InitializeAds);
            MakeButton(panel.transform, "加载激励", LoadRewarded);
            MakeButton(panel.transform, Application.isEditor ? "模拟：完成并奖励" : "展示激励", () => ShowRewarded(true));
            if (Application.isEditor) MakeButton(panel.transform, "模拟：直接关闭", () => ShowRewarded(false));
            MakeButton(panel.transform, "加载插屏", LoadInterstitial);
            MakeButton(panel.transform, "展示插屏", ShowInterstitial);
            MakeButton(panel.transform, "打开 Mediation Debugger", AdivoAds.ShowMediationDebugger);
            status = MakeText(panel.transform, string.Empty, 23, FontStyle.Normal, 760);
            status.alignment = TextAnchor.UpperLeft;
        }

        private static GameObject CreateObject(string name, Transform parent, params Type[] components)
        {
            var value = new GameObject(name, components); value.transform.SetParent(parent, false); return value;
        }

        private static Text MakeText(Transform parent, string value, int size, FontStyle style, float height)
        {
            var gameObject = CreateObject("Text", parent, typeof(Text), typeof(LayoutElement));
            var text = gameObject.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size; text.fontStyle = style; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 18; text.resizeTextMaxSize = size;
            gameObject.GetComponent<LayoutElement>().preferredHeight = height;
            return text;
        }

        internal static GameObject EnsureEventSystem()
        {
            if (EventSystem.current != null) return EventSystem.current.gameObject;
            return new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void MakeButton(Transform parent, string title, UnityEngine.Events.UnityAction action)
        {
            var gameObject = CreateObject(title, parent, typeof(Image), typeof(Button), typeof(LayoutElement));
            gameObject.GetComponent<Image>().color = new Color(0.12f, 0.42f, 0.92f, 1);
            gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                Append("点击 " + title);
                action();
            });
            gameObject.GetComponent<LayoutElement>().preferredHeight = 86;
            MakeText(gameObject.transform, title, 27, FontStyle.Bold, 86);
        }

        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top);
        }
    }

    internal sealed class AdivoSafeArea : MonoBehaviour
        {
            private RectTransform rectTransform;
            private Rect appliedSafeArea;
            private Vector2Int appliedScreen;

            private void Awake()
            {
                rectTransform = GetComponent<RectTransform>();
                Apply();
            }

            private void Update()
            {
                if (appliedSafeArea != Screen.safeArea || appliedScreen.x != Screen.width || appliedScreen.y != Screen.height)
                    Apply();
            }

            internal void Apply()
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                CalculateAnchors(Screen.safeArea, Screen.width, Screen.height, out var minimum, out var maximum);
                rectTransform.anchorMin = minimum;
                rectTransform.anchorMax = maximum;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                appliedSafeArea = Screen.safeArea;
                appliedScreen = new Vector2Int(Screen.width, Screen.height);
            }

            internal static void CalculateAnchors(Rect safeArea, float width, float height,
                out Vector2 minimum, out Vector2 maximum)
            {
                if (width <= 0 || height <= 0)
                {
                    minimum = Vector2.zero; maximum = Vector2.one; return;
                }
                minimum = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
                maximum = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
            }
    }
}
