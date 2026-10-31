SNOWY MOUNTAIN — SIDE FLAG MOUNTS
Focused correction, 2026-10-31. Review only, no production registration or merge.

The flags retained older coordinates after the cliffs changed, leaving no visible connection to their supports. Two small LibreSprite-authored mounting arms now join their center poles to bolted plates on the existing timber uprights. Placement is derived from each viewport's actual shaft width. Flags move four native pixels upward to clear the brace joint; horizontal positions follow the uprights. Cloth, emblem and original pole pixels are exact copies at +4,+12 in the new 48x82 sources. The original flags and all prior assets remain unchanged.

The native palette adds the side structure's existing iron shades. No generation or PixelLab credits. Both ASEs reopen pixel-identically. The two mounted banners replace the two original banner renderers; no additional scene objects or backing layers are added.

Unity compilation and SnowyMountainMountedBannersReview passed at 720x1280, 1080x1920, 1080x2400, inset-safe 1080x2400 and 360x640. The initial run found the right anchor on the brace outline; moving the mount four pixels upward fixed the placement, with the strict timber/iron check retained. Final validation checks bolted-plate alignment over actual opaque timber/iron pixels, original flag pixel equality, native scale, no gem overlap, continuous cliff edge/frame coverage, and all existing shared layout/control assertions. Paired screenshots have zero changed pixels outside the old/new flag rectangles.

Existing compact HUD crowding and Unity editor pixel-camera warning remain. This is not an Android build or phone test. Open Review.html for actual Unity captures and mounting closeups.

Reproduce native art: Scripts/BuildNative.ps1 -OutputDirectory <fresh-directory>; Scripts/ValidateNative.py checks current source/reopen pixels. Unity: Tools/Validate-Unity.ps1 then Tools/Test-ForestFoundation.ps1 -Filter ForestFoundationPlayTests.SnowyMountainMountedBannersReview -Graphics.

Draft PR #218. Stop for visual review; do not merge.
