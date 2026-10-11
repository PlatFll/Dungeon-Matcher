# Gallery verification — 2026-10-11

The generated review was opened in the Codex browser through a temporary,
loopback-only HTTP server. The following checks passed:

- All 67 roster entries appeared, with bound states and individual recommendations.
- Puffer's InflatedIdle displayed beside unchanged Rattlebones at native size
  and 4× integer zoom; the light stage and next-frame control worked.
- The P1 filter returned exactly Lantern Warden, Puffer Sentinel, Snapvine and
  Seismic Smith. Searching for Seismic reduced this to one entry.
- Snapvine Death advanced during playback and stopped on drawing 9/9 with
  the button restored to Play, rather than looping the one-shot state.
- No browser console errors were reported during these checks.

The image assets, source hashes and exposure sums are checked separately by
`package_review.py`; see `verification.json`. These checks do not certify Unity
runtime transitions or replace the source-frame visual assessment.

The gallery uses embedded data and relative local images, with no network fetch,
remote font or CDN. Direct `file:` launch could not be exercised through the
automation browser because its URL policy permits only HTTP/HTTPS. To review
offline, extract the portable ZIP and open `START_HERE.html` in a normal browser.

Rebuild from the repository with Python and Pillow: run `build_review.py`, then
`package_review.py`. The latter writes the portable ZIP to `.utmp/ArtAudit/`.
Judgments live in `assessments.py` and `OVERVIEW.md`; the HTML shell lives in
`gallery_template.html`. Generated media are inspection copies, never imports.
