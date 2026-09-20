#!/usr/bin/env python3
import json
import plistlib
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / "Packages/com.adivo.ads.core"
EXPECTED = {"AdivoCore", "AdivoMAX", "AdivoAds", "AdivoUnityBridge"}

for dll in [PACKAGE / "Runtime/Adivo.Ads.dll", PACKAGE / "Editor/Adivo.Ads.Editor.dll"]:
    if not dll.is_file() or dll.stat().st_size == 0:
        raise SystemExit(f"missing binary: {dll.relative_to(ROOT)}")

implementation_sources = [
    p for p in PACKAGE.rglob("*")
    if p.is_file() and p.suffix.lower() in {".cs", ".swift", ".m", ".mm"}
]
if implementation_sources:
    raise SystemExit("SDK implementation source found: " + ", ".join(str(p) for p in implementation_sources))

frameworks = PACKAGE / "Native~/Frameworks"
actual = {p.stem for p in frameworks.glob("*.xcframework")}
if actual != EXPECTED:
    raise SystemExit(f"unexpected XCFramework set: {sorted(actual)}")
for framework in frameworks.glob("*.xcframework"):
    info = plistlib.loads((framework / "Info.plist").read_bytes())
    slices = info["AvailableLibraries"]
    if not any(x["SupportedPlatform"] == "ios" and "arm64" in x["SupportedArchitectures"] and not x.get("SupportedPlatformVariant") for x in slices):
        raise SystemExit(f"device arm64 missing: {framework.name}")
    if not any(x["SupportedPlatform"] == "ios" and x.get("SupportedPlatformVariant") == "simulator" for x in slices):
        raise SystemExit(f"simulator slice missing: {framework.name}")

example = json.loads((ROOT / "Assets/StreamingAssets/Adivo.local.json.example").read_text())
for key in ["sdkKey", "rewardedAdUnitId", "interstitialAdUnitId", "developmentTeam"]:
    if not str(example.get(key, "")).startswith("YOUR_"):
        raise SystemExit(f"example must use placeholder: {key}")
if example.get("bundleIdentifier") != "com.example.yourgame":
    raise SystemExit("example bundle identifier must be generic")
if example.get("testMode") is not True:
    raise SystemExit("example must default to testMode=true")

local = ROOT / "Assets/StreamingAssets/Adivo.local.json"
if local.exists():
    ignored = subprocess.run(["git", "check-ignore", "-q", str(local.relative_to(ROOT))], cwd=ROOT).returncode == 0
    if not ignored:
        raise SystemExit("local configuration is not ignored")

video = ROOT / "docs/demo.mp4"
if not video.is_file() or video.stat().st_size >= 100_000_000:
    raise SystemExit("demo video is missing or too large for GitHub")

print("Binary Unity demo validation passed")
print("- precompiled Runtime and Editor DLLs")
print("- four XCFrameworks with device and simulator slices")
print("- no SDK implementation source files")
print("- generic placeholder configuration with testMode=true")
print("- edited demo video is GitHub-compatible")
