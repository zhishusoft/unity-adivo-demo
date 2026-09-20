using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adivo.Demo
{
    public static class AdivoDemoBuild
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Adivo/Create Demo Scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("AdivoDemo");
            root.AddComponent<AdivoDemo>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Adivo/Build iOS Demo")]
        public static void BuildIOS() => BuildIOS(false);

        [MenuItem("Adivo/Build iOS Simulator Demo")]
        public static void BuildIOSSimulator() => BuildIOS(true);

        private static void BuildIOS(bool simulator)
        {
            if (!File.Exists(ScenePath)) CreateScene();
            var localPath = Path.Combine(Application.dataPath, "StreamingAssets/Adivo.local.json");
            if (!File.Exists(localPath)) throw new BuildFailedException("请先复制 Adivo.local.json.example 为 Adivo.local.json 并填写 MAX 配置。");
            var config = JsonUtility.FromJson<AdivoDemoConfiguration>(File.ReadAllText(localPath));
            if (config == null || IsPlaceholder(config.sdkKey) || IsPlaceholder(config.rewardedAdUnitId) ||
                IsPlaceholder(config.bundleIdentifier) || IsPlaceholder(config.developmentTeam))
                throw new BuildFailedException("Adivo.local.json 必须填写自己的 MAX SDK Key、激励广告单元、Bundle ID 和 Apple Team ID。");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, config.bundleIdentifier);
            PlayerSettings.iOS.appleDeveloperTeamID = config.developmentTeam;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.sdkVersion = simulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.productName = "Adivo Unity Demo";
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            var directory = simulator ? "unity-ios-simulator" : "unity-ios";
            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../build/" + directory));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output,
                target = BuildTarget.iOS, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Unity iOS 导出失败：" + report.summary.result);
            Debug.Log("Adivo Unity iOS export: " + output);
        }

        private static bool IsPlaceholder(string value) =>
            string.IsNullOrWhiteSpace(value) || value.StartsWith("YOUR_", StringComparison.Ordinal);

        public static void RunEditModeTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new TestCallbacks());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
        }

        private sealed class TestCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../build/unity-editmode-results.json"));
                File.WriteAllText(output, JsonUtility.ToJson(new TestSummary
                {
                    passed = result.PassCount, failed = result.FailCount,
                    skipped = result.SkipCount, inconclusive = result.InconclusiveCount
                }, true));
                EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
            }
        }

        [Serializable]
        private sealed class TestSummary
        {
            public int passed;
            public int failed;
            public int skipped;
            public int inconclusive;
        }
    }
}
