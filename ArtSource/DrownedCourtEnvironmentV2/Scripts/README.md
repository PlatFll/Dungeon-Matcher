# Stage 00 verification

Run the explicit editor test `DrownedCourtV2PreflightTests.CaptureBaselinesAndBuildScaffold` in this isolated worktree. It enters play mode, uses disposable account profiles, captures Court/Dungeon/Forest at two identical viewport sizes, and saves the required art-only scene. Run only in a clean disposable editor session; it opens/replaces test scenes.

The test is excluded from routine suites by NUnit Explicit. `Assets/_Game/Scenes/ArtPreview_DrownedCourtV2.unity` is deliberately retained, so Unity reports its generated-file warning. It must not enter build settings.

Run `rtk proxy powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1` from the project root for the required compiler check. `VerifyStage00.py` checks the preserved-file hashes and saved results without changing anything. It uses only Python's standard library. Run from any directory.

Unity may normalize existing importer/settings YAML on open. Review and discard only that run's incidental changes in the isolated worktree; never reset the user's main checkout. The baseline byte hashes are intentionally exact and specific to this checkout's line endings.

The local full Unity logs are retained under QA and excluded by the repository log ignore rule. The tracked test XML, six geometry reports and UnityValidationReceipt.json preserve the results and full-log checksum.
