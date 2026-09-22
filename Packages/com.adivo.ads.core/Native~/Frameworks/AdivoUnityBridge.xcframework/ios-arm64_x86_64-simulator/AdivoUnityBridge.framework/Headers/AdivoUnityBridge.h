#import <Foundation/Foundation.h>

/** Unity 主线程接收原生 JSON 消息的回调函数。 */
typedef void (*AdivoUnityCallback)(const char *json);

/** 注册 Unity 用于接收完成和事件消息的全局回调。 */
FOUNDATION_EXPORT void adivo_unity_set_callback(AdivoUnityCallback callback);

/** 设置宿主当前是否具备广告请求资格。 */
FOUNDATION_EXPORT void adivo_unity_set_can_request_ads(bool allowed);

/**
 使用 JSON 配置初始化 Adivo Ads。

 @param json UTF-8 JSON 配置字符串。
 @param requestID 用于关联异步完成消息的请求标识。
 */
FOUNDATION_EXPORT void adivo_unity_initialize(const char *json, long long requestID);

/** 为业务广告位加载广告，并通过全局回调返回异步结果。 */
FOUNDATION_EXPORT void adivo_unity_load(const char *placement, long long requestID);

/** 返回业务广告位当前是否有可展示的有效缓存广告。 */
FOUNDATION_EXPORT bool adivo_unity_is_ready(const char *placement);

/**
 展示业务广告位对应的缓存广告。

 完成消息仅表示广告关闭或失败；有效奖励通过独立的 `rewardEarned` 事件传递。
 */
FOUNDATION_EXPORT void adivo_unity_show(const char *placement, long long requestID);

/** 通知 Adivo 宿主隐私状态已变化，并阻挡旧隐私状态下的缓存广告。 */
FOUNDATION_EXPORT void adivo_unity_privacy_did_change(void);

/** 在初始化成功后打开 MAX Mediation Debugger。 */
FOUNDATION_EXPORT void adivo_unity_show_mediation_debugger(void);
