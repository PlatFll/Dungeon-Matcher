"""Build a self-contained local review site from measured PNGs and Unity captures."""
from pathlib import Path
import json,shutil

ROOT=Path(__file__).resolve().parents[1]
work=ROOT/'.utmp/Ironvein'
out=work/'ReviewDelivery';out.mkdir(parents=True,exist_ok=True)
for name in ('ArtReview','ScreenReview'):
    shutil.copytree(work/name,out/name,dirs_exist_ok=True)
motion=out/'MotionReview';motion.mkdir(exist_ok=True)
shutil.copytree(ROOT/'ArtSource/Ironvein/Motion',motion/'sheets',dirs_exist_ok=True)
doc=(work/'MotionReview/Review.html').read_text(encoding='utf-8')
doc=doc.replace('../../../ArtSource/Ironvein/Motion/','sheets/')
(motion/'Review.html').write_text(doc,encoding='utf-8')
shutil.copy2(ROOT/'ArtSource/Ironvein/Audio/Ironvein Rail Lanterns - TEMP review.wav',out/'TemporaryMusic.wav')
budget=json.loads((ROOT/'ArtSource/Ironvein/Production/budget-summary.json').read_text())
html='''<!doctype html><meta charset="utf-8"><title>Ironvein review</title>
<style>body{background:#171c23;color:#e7e5dc;font:18px system-ui;max-width:1150px;margin:40px auto;padding:0 24px}h1,h2{color:#efb363}a{color:#a8dafa}.links{display:flex;flex-wrap:wrap;gap:16px}.links a{background:#303945;padding:22px;border-radius:8px;text-decoration:none}.screens{display:flex;flex-wrap:wrap;gap:20px;align-items:start}img{max-width:270px;image-rendering:pixelated}p{max-width:900px;line-height:1.5}small{color:#b7c2cf}</style>
<h1>Ironvein Excavation</h1><p>Playable review candidate: dwarven crews, ore-powered machinery and a deep cave. New art and temporary music await your review.</p>
<nav class="links"><a href="ArtReview/Review.html">14 stills · native comparisons</a><a href="MotionReview/Review.html">108 motion clips · actual timing</a><a href="ScreenReview/Review.html">Unity screens · cave and UI materials</a></nav>
<h2>Actual Unity screen and composition target</h2><div class="screens"><a href="ScreenReview/phase09-1080x1920.png"><img src="ScreenReview/phase09-1080x1920.png" alt="Actual 1080 by 1920 Unity screen"></a><a href="ScreenReview/CompositionTarget.png"><img src="ScreenReview/CompositionTarget.png" alt="Supplied composition concept"></a></div>
<p>Click to open full resolution. The screen gallery includes four portrait/safe-inset layouts. Source dimensions, alpha, palettes and hashes are measured separately; the composite is a visual comparison.</p>
<h2>Original temporary music</h2><p>Two-minute local composition for listening review. No final music approval is claimed.</p><audio controls preload="none" src="TemporaryMusic.wav"></audio>
<h2>Review status</h2><p>The small drill stops at the first stone after one durability hit. Fixed drills traverse their complete lane. Global endless scaling, random crystal travel and existing player controls remain active.</p>
<p>Visual polish, human pacing and physical-device testing remain review gates. The complete executed evidence and provisional encounter values are in Docs/IronveinExcavation.</p>
<small>PixelLab: COST quoted generations across COUNT requests; 300 initial / 400 total ceiling. Reserve untouched. No credit purchases.</small>'''
html=html.replace('COST',str(budget['committed_quoted'])).replace('COUNT',str(budget['requests']))
(out/'Review.html').write_text(html,encoding='utf-8')
for target in ('ArtReview/Review.html','MotionReview/Review.html','ScreenReview/phase09-1080x1920.png','TemporaryMusic.wav'):
    assert (out/target).is_file(),target
print(json.dumps({'review':str(out/'Review.html'),'motion_sheets':len(list((motion/'sheets').rglob('*.png')))}))
