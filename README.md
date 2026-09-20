# Adivo Ads Unity iOS Binary Demo

这是一个可直接打开的 Unity iOS 示例工程，演示如何使用 **Adivo Ads 二进制 SDK** 加载和展示 AppLovin MAX 激励广告与插屏广告。

- Unity：`6000.3.24f1 LTS`
- iOS：`15.0+`
- AppLovin MAX：`13.6.4`
- EDM4U：`1.2.189`
- 原生 Adivo SDK：4 个静态 XCFramework，包含真机 arm64 与模拟器 arm64/x86_64
- Adivo Unity Runtime / Editor：预编译 DLL

仓库不包含 Adivo 原生实现源码、Adivo Unity SDK C# 实现源码、MAX SDK Key、广告单元 ID、测试设备 ID、Apple Team ID、签名文件或真实运行数据。`Assets/Scripts` 只包含 Demo 页面和调用示例，方便集成者查看和修改。

## 演示视频

https://github.com/user-attachments/assets/00cb9c4a-01ca-40c8-8d36-99ddb20d42ba

[下载 MP4 文件](docs/demo.mp4)

## 1. 准备环境

需要：

1. Unity `6000.3.24f1`，安装 **iOS Build Support**。
2. Xcode 27.0 或兼容版本。
3. CocoaPods `1.16+`。
4. 可用的 AppLovin MAX SDK Key、激励广告单元 ID 和插屏广告单元 ID。
5. 用于真机签名的 Bundle ID 与 Apple Development Team ID。

首次打开时 Unity Package Manager 会下载 EDM4U。首次导出 iOS 时 EDM4U 会通过 CocoaPods 下载 AppLovinSDK 13.6.4，因此需要网络。

## 2. 填写自己的配置

复制示例文件：

```bash
cp Assets/StreamingAssets/Adivo.local.json.example \
   Assets/StreamingAssets/Adivo.local.json
```

编辑 `Adivo.local.json`：

```json
{
  "sdkKey": "YOUR_MAX_SDK_KEY",
  "rewardedAdUnitId": "YOUR_MAX_REWARDED_AD_UNIT_ID",
  "interstitialAdUnitId": "YOUR_MAX_INTERSTITIAL_AD_UNIT_ID",
  "testMode": true,
  "canRequestAds": true,
  "hasUserConsent": false,
  "doNotSell": false,
  "verboseLogging": true,
  "testDeviceAdvertisingIdentifiers": [],
  "googleEnabled": false,
  "googleAppId": "",
  "bundleIdentifier": "com.example.yourgame",
  "developmentTeam": "YOUR_APPLE_DEVELOPMENT_TEAM_ID"
}
```

`Adivo.local.json` 已被 Git 忽略，不会提交。请不要把正式 Key、广告单元或设备标识写进源码、README 或公开 Issue。

配置说明：

| 字段 | 必填 | 说明 |
| --- | --- | --- |
| `sdkKey` | 是 | AppLovin MAX SDK Key |
| `rewardedAdUnitId` | 是 | MAX 激励广告单元 ID |
| `interstitialAdUnitId` | 否 | MAX 插屏广告单元 ID；不测试插屏时可留空 |
| `testMode` | 建议开发期为 `true` | 开发验证使用测试模式；上线前按 MAX 要求调整 |
| `canRequestAds` | 是 | 宿主完成 CMP/隐私流程后得出的真实广告请求资格 |
| `hasUserConsent` | 按地区 | 宿主已有的同意信号；SDK 不替宿主弹 CMP |
| `doNotSell` | 按地区 | 宿主已有的出售/共享选择 |
| `testDeviceAdvertisingIdentifiers` | 开发期建议 | MAX 后台添加的测试设备 advertising identifier |
| `bundleIdentifier` | 真机必填 | 你自己的 App Bundle ID |
| `developmentTeam` | 真机必填 | Apple Developer Team ID |
| `googleEnabled` / `googleAppId` | 仅 Google 渠道 | 本基础 Demo 默认不集成 Google 适配器 |

## 3. 在 Unity 中运行

1. 用 Unity Hub 打开仓库根目录。
2. 打开 `Assets/Scenes/Main.unity`。
3. 在 Editor 中直接 Play 会进入明确标记的模拟模式，可验证按钮、关闭与奖励 UI。
4. 真实广告必须导出到 iOS 真机，Editor 模拟结果不能代替真实广告验证。

Demo 按钮顺序：

1. **初始化**
2. **加载激励**
3. **展示激励**
4. 看完广告并触发厂商有效奖励
5. **加载插屏** / **展示插屏**
6. **打开 Mediation Debugger**

奖励只由有效的 `RewardEarned` 回调触发；关闭广告不会自动发奖。同一个展示 session 在 SDK 内去重，Demo 的“有效奖励”计数应恰好增加一次。

## 4. 导出并运行 iOS

在 Unity 菜单选择：

**Adivo → Build iOS Demo**

输出目录：

```text
build/unity-ios
```

然后：

1. 打开 `build/unity-ios/Unity-iPhone.xcworkspace`，不要打开 `.xcodeproj`。
2. 选择 `Unity-iPhone` scheme。
3. 确认 Signing Team 与 Bundle ID 是你自己的配置。
4. 选择真机，按 `Command + R`。
5. 在 MAX Mediation Debugger 中确认 AppLovinSDK、广告单元和测试模式状态。

命令行导出：

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath "$PWD" \
  -executeMethod Adivo.Demo.AdivoDemoBuild.BuildIOS \
  -logFile /tmp/unity-adivo-export.log
```

## 5. 二进制 SDK 结构

```text
Packages/com.adivo.ads.core/
├── Runtime/Adivo.Ads.dll
├── Editor/Adivo.Ads.Editor.dll
├── Editor/AdivoDependencies.xml
└── Native~/Frameworks/
    ├── AdivoCore.xcframework
    ├── AdivoMAX.xcframework
    ├── AdivoAds.xcframework
    └── AdivoUnityBridge.xcframework
```

四个 XCFramework 均为静态二进制。AppLovinSDK 由 `AdivoDependencies.xml` 通过 EDM4U/CocoaPods 精确解析为 13.6.4。Adivo Editor DLL 在导出时添加 `-ObjC`、链接四个 XCFramework、处理 iOS15 deployment target、Privacy Manifest 与 AppLovin 动态框架嵌入。

每个二进制的 SHA-256 见 [BINARY-MANIFEST.json](BINARY-MANIFEST.json)。

## 6. 隐私与上线责任

Adivo 二进制与 AppLovinSDK 都携带 Privacy Manifest，但集成者仍需自行完成：

- 适用地区的 CMP/同意流程；
- ATT 展示时机与用途文案；
- App Store Connect 隐私标签；
- SKAdNetwork、app-ads.txt、MAX 应用和广告单元审核；
- 正式投放前关闭或调整测试模式；
- 服务端资产发放与业务幂等。

初始化成功只表示 SDK 初始化完成，不代表账号已获批、当前有填充或收入已产生。

## 7. 常见问题

| 现象 | 检查项 |
| --- | --- |
| 构建提示缺少配置 | 是否已复制并填写 `Adivo.local.json`，是否仍含 `YOUR_` 占位符 |
| `pod` 或 workspace 不存在 | CocoaPods 是否安装；EDM4U 是否完成 iOS Resolver |
| 初始化失败 | SDK Key、Bundle ID、隐私请求资格和网络 |
| 加载失败或无填充 | 广告单元类型、MAX 后台状态、测试设备、网络和地区 |
| 展示后没有奖励 | 只认有效奖励事件；确认广告完整观看，并查看 MAX/设备日志 |
| App 启动找不到 AppLovinSDK | 必须使用导出的 `.xcworkspace`，不要手动删除 Embed Dynamic CocoaPods Frameworks 阶段 |

## 验证记录

当前仓库已完成：

- Unity 6000.3.24f1 无界面导入与脚本编译；
- 二进制 Runtime / Editor DLL 加载；
- 四个 XCFramework 真机/模拟器架构检查；
- Unity iOS DeviceSDK + IL2CPP 导出；
- Xcode 27 / iPhoneOS 27.0 无签名完整编译；
- 仓库敏感配置与 SDK 实现源码扫描。

真实填充、广告素材、奖励和收入取决于集成者自己的 MAX 配置、账号、网络与设备环境。
