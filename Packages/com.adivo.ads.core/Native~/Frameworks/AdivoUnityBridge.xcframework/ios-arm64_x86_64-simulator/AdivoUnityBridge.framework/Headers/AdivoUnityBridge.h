#import <Foundation/Foundation.h>

typedef void (*AdivoUnityCallback)(const char *json);
FOUNDATION_EXPORT void adivo_unity_set_callback(AdivoUnityCallback callback);
FOUNDATION_EXPORT void adivo_unity_set_can_request_ads(bool allowed);
FOUNDATION_EXPORT void adivo_unity_initialize(const char *json, long long requestID);
FOUNDATION_EXPORT void adivo_unity_load(const char *placement, long long requestID);
FOUNDATION_EXPORT bool adivo_unity_is_ready(const char *placement);
FOUNDATION_EXPORT void adivo_unity_show(const char *placement, long long requestID);
FOUNDATION_EXPORT void adivo_unity_privacy_did_change(void);
FOUNDATION_EXPORT void adivo_unity_show_mediation_debugger(void);
