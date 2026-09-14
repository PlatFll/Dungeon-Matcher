# SmallHold Games startup ident

`SmallHold_Games_Ident.aseprite` is the approved, editable LibreSprite source:
480x270, 30 frames, approximately 12 fps / 2.5 seconds. Layer names and seven
timeline tags are preserved. The moon orbits counterclockwise behind the western
island edge; the entire island turns white before the wordmark pops in.

The held final frame now uses a white background and the same silhouette/wordmark
in dark ink. The preceding 29 animation frames and source layers/tags are preserved.
`Tools/Prepare-SmallHoldWhiteHold.ps1` applies that idempotent LibreSprite source
edit; `Tools/Export-SmallHoldIdent.ps1` exports the atlas. Unity plays the original
2.5-second sequence, then holds its final white pose for 1.5 additional seconds.

The Unity runtime uses `Assets/_Game/Art/Branding/SmallHold_Games_Atlas.png`.
It contains all 30 full-size frames in reading order, five columns by six rows.
No cropping, resampling, palette reduction, frame deduplication or video decoding
is involved. It is silent; no clink sound has been supplied.

To re-export after editing the source:

```powershell
rtk powershell -ExecutionPolicy Bypass -File Tools/Export-SmallHoldIdent.ps1 -LibreSpritePath 'path/to/libresprite.exe'
```

Keep the atlas Point-filtered, uncompressed, without mipmaps or NPOT resizing,
and with a maximum texture size of 4096 on all build targets. The runtime and
validation expect exactly 30 cells of 480x270; update them if the sequence changes.

Build flow: Startup ident -> MainMenu -> existing Game navigation. Unity's built-in
splash and Unity logo are disabled in Player Settings, so the SmallHold animation
starts as soon as the startup scene is ready. Open
`Assets/_Game/Scenes/Startup.unity` and enter Play Mode to preview the ident.
Rebuild the player to apply the splash settings to an installed game.
