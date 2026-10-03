"""Prepare native PixelLab inputs and a bounded, reviewable production manifest."""
from pathlib import Path
from PIL import Image
import json, hashlib, base64

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'ArtSource/Forest/RosterProduction'
SOURCE=ROOT/'ArtSource/Forest/RosterConcepts'
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'Inputs').mkdir(exist_ok=True)
names=['Elven_Thornkeeper','Orc_Berserker','Orc_Bloomcaller','Snapvine','Orc_Drummer','Briar_Archer','Ancient_Treant']
attacks=[
 'Lean into a quick short sickle slash with the hand holding the sickle. Keep the thorn branch bundle carried in the other hand. Recover smoothly.',
 'Grip the heavy cleaver firmly, draw it back with the shoulder and deliver a powerful chopping slash toward the left. Knees absorb the follow-through. Recover smoothly.',
 'Swing the flower-tipped staff forward for a short grounded staff strike. Both body and arm move, the flowers stay attached. Recover smoothly.',
 'Pull the plant head back on its flexible stem then snap its toothed jaws forward to the left for one fast decisive bite. Root feet remain planted. Retract smoothly.',
 'Draw one drumstick back then strike outward toward the left, with a compact body lean, retaining the strapped drum and other stick. Recover smoothly.',
 'Draw the bowstring firmly with the rear hand while the bow arm stays extended to the left, release one arrow then lower back into the ready pose. No sword or shield.',
 'The bulky old stump creature draws one massive branch arm up then swings it down and forward in a heavy grounded slam, root feet firmly planted. Recover smoothly.'
]
jobs=[]
for name,attack in zip(names,attacks):
    path=SOURCE/('TreantVariants/Treant_C_Old_Stump.png' if name=='Ancient_Treant' else name+'.png')
    ref=Image.open(path).convert('RGBA')
    # Symmetric transparent clearance only. Never resample approved source pixels.
    canvas=Image.new('RGBA',(128,112) if name=='Ancient_Treant' else (96,80))
    canvas.paste(ref,((canvas.width-ref.width)//2,canvas.height-ref.height))
    dest=OUT/'Inputs'/f'{name}.png';canvas.save(dest)
    suffix=' Preserve the exact face, costume, equipment, material colors, source pixel size and fixed ground contact of this input. No new props, detached limbs, camera motion, zoom, glow cloud, background or motion blur. Return smoothly to the exact supplied ready pose.'
    for state,motion,count in [
      ('Idle','A very restrained planted battle idle: knees and torso settle slightly, shoulders follow, one tiny blink, recover. Both feet or root contacts stay anchored. Do not bounce the whole sprite.',8),
      ('AutoAttack',attack,8),
      ('Hit','A short recoil from one hit: shoulders and head lean back together, knees brace, held equipment stays attached, then return to ready. Feet remain planted.',4),
      ('Death','Defeat: body slumps downward, knees fold, held props settle with the hands, ending in a low collapsed pose. Plant creature withers or stump creature folds its branch arms and stoops. No disappearing pixels or new debris.',8)]:
        jobs.append(dict(name=name,state=state,input=str(dest.relative_to(ROOT)),frame_count=count,loop=state=='Idle',pinEnd=state!='Death',action=motion+suffix.replace(' Return smoothly to the exact supplied ready pose.','' if state=='Death' else ' Return smoothly to the exact supplied ready pose.')))

(OUT/'requests.json').write_text(json.dumps(jobs,indent=2),encoding='utf-8')
if not (OUT/'budget.json').exists():
    (OUT/'budget.json').write_text(json.dumps(dict(authorizedTotal=160,initialCeiling=120,reserve=40,baselineRemaining=1702,baselineUsed=298,spent=0,scope='Seven approved forest enemies: animations and kits; two concepts each for wood, stone, chain, thorn, root level 1, root level 2 and dense vines. New obstacle art requires user selection before replacement. No purchases.',status='Production authorized 2026-10-03'),indent=2),encoding='utf-8')
print(json.dumps(dict(requests=len(jobs),inputs=[str((OUT/'Inputs'/f'{n}.png').relative_to(ROOT)) for n in names])))
