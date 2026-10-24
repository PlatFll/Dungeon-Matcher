$ErrorActionPreference='Stop'
& powershell -ExecutionPolicy Bypass -File 'Tools/Validate-Unity.ps1'
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
& powershell -ExecutionPolicy Bypass -File 'C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/RunFrameFit.ps1' -Test 'SharedZoneFrameTests.ConsistentZoneFrames;ForestFrameFitPreviewTests.FullAssemblyReview' -Label Verified
exit $LASTEXITCODE
