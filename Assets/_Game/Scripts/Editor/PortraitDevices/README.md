# Portrait layout fixtures

Run **Dungeon Matcher > Validation > Portrait Device Simulator** to exercise the
actual Simulator screen API without a safe-area override. Reports and native
screenshots go to `.utmp/PortraitLayout`. The Game View matrix is available under
**Pixel Layout Play Mode**; it covers 16 resolutions with seven inset variants.

Fixtures 0–3 retain portrait display, navigation and safe-area values from Unity's
official `com.unity.device-simulator.devices` 1.0.1 profiles (iPhone 12, Pixel 5,
Note20 Ultra, Moto G7 Power). Unused hardware capabilities, landscape orientations,
and decorative overlays are omitted.

Fixtures 4–6 are synthetic Dynamic Island and 2026 large/tall shapes. Their insets
are test inputs, not claimed specifications of a named device. Unity's available
profile catalog has no exact 2026 device. No fixture values enter runtime layout.

Sources: [Unity device profile package](https://packages.unity.com/com.unity.device-simulator.devices),
[Unity simulator-aware Screen API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Device.Screen.html).
