from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage02'
assets=[]
for p in sorted((R/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');rt=Image.open(R/'Roundtrip'/p.name).convert('RGBA')
 assert im.tobytes()==rt.tobytes(),p.name+' roundtrip'
 alpha=sorted(set(im.getchannel('A').getdata()));assert set(alpha)<= {0,255}
 assets.append({'name':p.stem,'size':im.size,'opaque_colors':len({v[:3] for v in im.getdata() if v[3]}),'alpha':alpha,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'libresprite_roundtrip_exact':True})
rep=Image.open(R/'Exports/BeamRepeat.png').convert('RGBA');assert list(rep.crop((0,0,1,10)).getdata())==list(rep.crop((23,0,24,10)).getdata())
sheet=Image.new('RGBA',(670,190),(30,32,44,255));d=ImageDraw.Draw(sheet);x=10
for a in assets:
 im=Image.open(R/'Exports'/(a['name']+'.png')).convert('RGBA');d.text((x,5),a['name'],fill='white');d.text((x,23),str(im.width)+'x'+str(im.height),fill='#aab8c6');sheet.alpha_composite(im,(x,47));sheet.alpha_composite(im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST),(x,86));x+=im.width*4+18
sheet.save(R/'Preview/NativeAssets.png')
# Native and enlarged raw candidates are review evidence only, never scene textures.
rawboard=Image.new('RGBA',(540,560),(30,32,44,255));d=ImageDraw.Draw(rawboard)
for i in range(4):
 im=Image.open(R/'Raw'/f'Separator-{i}.png').convert('RGBA');d.text((4,8+i*138),str(i),fill='white');rawboard.alpha_composite(im.resize((512,128),Image.Resampling.NEAREST),(20,8+i*138))
rawboard.save(R/'Preview/RawCandidates.png')
manifest={'status':'PASS','guide_version':'1.17.2','selected':{'job':'71e6af15-a481-4e76-b583-ac452ecc0dec','index':0},'rejected_short_set':{'job':'53590f75-0190-4188-bf01-f9f1456d22f6','reason':'Transparent removal damaged timber; no short-set pixels used.'},'pixel_lab_generations':20,'starting_balance':266.2,'expected_remaining':246.2,'actual_image_inputs':[],'reference_policy':'Text-only focused UI asset generation, as requested by the supplied pack; local golden images reviewed but not uploaded.','cleanup':'LibreSprite native cel edits: condense redundant wood rows into a 10-pixel beam; retain the generated grain vocabulary and amber center; merge shades into existing wood/iron/amber ramps, close the diamond contour, author clean mirrored end geometry with upper-left highlights. No resizing or interpolation.','repeat_edge_match':True,'assets':assets}
(R/'QA/Assets.json').write_text(json.dumps(manifest,indent=2))
if (R/'Preview/Unity-720x1280.png').exists():
 before=Image.open(R/'Preview/Before-720x1280.png').convert('RGBA');after=Image.open(R/'Preview/Unity-720x1280.png').convert('RGBA')
 review=Image.new('RGBA',(1464,1320),(19,21,29,255));d=ImageDraw.Draw(review);d.text((10,8),'Before - Stage 01',fill='white');d.text((742,8),'Stage 02 - modular separator only',fill='white');review.alpha_composite(before,(8,32));review.alpha_composite(after,(740,32));review.save(R/'Preview/Comparison.png')
 detail=Image.new('RGBA',(1080,364),(19,21,29,255));d=ImageDraw.Draw(detail)
 for i,im in enumerate([before,after]):
  crop=im.crop((50,470,670,548));d.text((8,8+i*180),'Before' if i==0 else 'Stage 02',fill='white');crop=crop.resize((1054,132),Image.Resampling.NEAREST);detail.alpha_composite(crop,(8,36+i*180))
 detail.save(R/'Preview/SeparatorDetail.png')
cards=''.join('<article><h3>'+a['name']+'</h3><p>'+str(a['size'][0])+'x'+str(a['size'][1])+' / '+str(a['opaque_colors'])+' colors</p><img class="native" src="Exports/'+a['name']+'.png"><br><img style="width:'+str(a['size'][0]*6)+'px" src="Exports/'+a['name']+'.png"></article>' for a in assets)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein Stage 02 - separator review</title><style>body{background:#13151d;color:#eceaf2;font:16px system-ui;margin:24px}h1{font-size:26px}p{color:#c3c6d1}a{color:#b9caff}img{image-rendering:pixelated;max-width:100%;object-fit:contain}.compare{display:flex;gap:24px;flex-wrap:wrap}.scene{width:360px}button{padding:10px;background:#35394e;color:white;border:1px solid #78829b;cursor:pointer}.assets{display:flex;gap:24px;flex-wrap:wrap}article{background:#212330;padding:16px;border-radius:6px;min-width:190px}.native{max-width:none;margin:12px 0}summary{cursor:pointer;margin:18px 0}</style><h1>Stage 02 - reinforced mine separator</h1><p>Actual Unity art-only captures. The upper scene, board opening/cells/gems, player frame and bottom controls are unchanged. Only the separator above the board is new. No production integration or merge.</p><button onclick="document.querySelectorAll('.scene').forEach(i=>i.style.width=i.style.width==='720px'?'360px':'720px')">Toggle native 1x / 2x</button><div class="compare"><div><h2>Before</h2><img class="scene" src="Preview/Before-720x1280.png"></div><div><h2>Stage 02</h2><img class="scene" src="Preview/Unity-720x1280.png"></div></div><h2>Native components and 6x nearest-neighbor views</h2><div class="assets">'''+cards+'''</div><p>Four component families, five PNG/ASE files including separately lit left/right ends. 20 PixelLab generations across two calls / 8 candidates. Selected first-call candidate 0; short variants rejected. No image uploads.</p><p>Validation: <a href="QA/Assets.json">native pixel / roundtrip checks</a> · <a href="QA/Unity.json">Unity protected-region checks</a> · <a href="QA/Validation.json">final validation</a></p><p><a href="Preview/Unity-1080x1920.png">1080x1920 Unity image</a> · <a href="Preview/Unity-360x640.png">360x640 native image</a> · <a href="../Review.html">Stage 01 baseline</a></p><details><summary>Raw PixelLab candidates and selection</summary><p>First-call candidate 0 supplies the selected wood/iron/amber vocabulary. The over-tall source was separated and edited at native pixel resolution in LibreSprite.</p><img src="Preview/RawCandidates.png"><p>Short-canvas attempt: rejected because the beam was removed with the background.</p><img src="Preview/ShortCandidates.png"></details><p>Stopped after Stage 02. Stage 03 has not started.</p>'''
(R/'Review.html').write_text(html,encoding='utf-8');print(json.dumps({'assets':assets,'roundtrip':'PASS','generations':20}))
