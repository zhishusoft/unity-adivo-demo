# 验证记录

- Unity Editor：6000.3.24f1 LTS（Apple silicon）
- Xcode：27.0（27A266a）
- iOS SDK：27.0
- 最低 iOS：15.0
- CocoaPods：1.16.2
- AppLovinSDK：13.6.4

验证结果：Unity 项目导入和脚本编译通过；DeviceSDK/IL2CPP 导出通过；导出工程只包含 Adivo Runtime/Editor DLL 与四个 Adivo XCFramework，不包含 Adivo SDK 实现源码；Xcode 无签名 iphoneos Debug 完整编译通过。

验证时只使用虚构的本地构建占位值。该文件在验证后删除，且 `Assets/StreamingAssets/Adivo.local.json` 与 `Packages/packages-lock.json` 均被 Git 忽略。
