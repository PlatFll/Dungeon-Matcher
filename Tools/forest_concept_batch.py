"""Prepare review-only PixelLab requests; never imports candidates into Unity."""
import base64, json, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'ArtSource/Forest/RosterConcepts'
COMMON = ('Dungeon Matcher native pixel-art battle sprite, one full body character, facing left in a three-quarter battle-ready pose. '
          'Cute whimsical fantasy, strong readable silhouette, simple expressive face, one-pixel near-black #0A0D11 contour. '
          'Broad connected hard shadow clusters, top-left lighting, matte materials, sparse highlights, no gradients, blur, dithering, glow, or loose speckles. '
          'Same source pixel density as reference, feet near bottom of canvas, full equipment visible, transparent background, no ground shadow, no text. '
          'Aim for 14-18 useful opaque colors. Shared wood/leather #4A2C1C #7A4D2E #B07A43; dull iron #2B313A #55606E #93A1B0; '
          'linen/tusks #9F907A #E0D2B8. New design candidate, do not clone the reference costume or equipment. ')
SPECS = [
 ('Elven_Thornkeeper',64,'Elven_Mender',
  'Slim adult elf special enemy, warm skin #B9825D #E6B08A, tied-back dark auburn hair, long pointed ears. Moss-teal apron and simple brown garden leathers. '
  'Carries a compact living crescent of intertwined brambles in one arm, three thorny outer branches and one smooth handle/opening inward; other hand grips a short hooked pruning stick. '
  'Practical woodland keeper, readable face, bramble prop is the focal point; no metal shield, no military armor.'),
 ('Orc_Berserker',64,'Orc_Trailguard',
  'Broad powerful olive-skinned orc, #393728 #665F3D #908E58 skin, heavy brow, two small ivory tusks, close-cropped charcoal hair. '
  'Bare muscular forearms and chest with one diagonal leather harness, muted brick-red waist cloth, heavy boots. One large chipped dull-iron cleaver in a low ready grip, one clenched fist. '
  'Forward-leaning angry stance with compact broad torso; distinct from the ochre axe-bearing Trailguard, no shield, no vines, no magic, no blood.'),
 ('Orc_Bloomcaller',64,'Orc_Rootbinder',
  'Broad olive-skinned orc nature shaman, open friendly-but-menacing tusked face, cream cloth turban with one dusty coral flower. '
  'Cream and muted terracotta layered robes over squat feet; hanging seed gourds, cupped hand offering one seedling. '
  'Other hand holds a short curved flowering branch, no long staff, no indigo hood, no antlers, no skulls. Distinct pot-bellied rounded summoner silhouette, few large flowers rather than leaf noise.'),
 ('Snapvine',64,'Orc_Rootbinder',
  'Small hostile Venus flytrap plant minion; occupy about 36 pixels high within the 64px canvas at the same pixel density as the humanoid reference, intentionally much smaller. '
  'One large hinged green mouth with muted wine-red inner lobes and few thick ivory thorn teeth, curled sturdy stalk, two broad leaves and a small root-foot cluster. '
  'Clever aggressive plant expression implied by mouth, no humanoid head, eyes, clothes, arms, pot or weapon. Three-tone olive foliage and warm wood root colors, simple distinct shapes.'),
 ('Orc_Drummer',64,'Orc_Trailguard',
  'Stocky olive-skinned orc ritual drummer, bald broad head with tiny cream feather tied at back, visible small tusks. '
  'Dusty ochre tunic and muted blue sash, large round wooden hide drum strapped across waist, two separate short wooden drum beaters held clearly in both hands. '
  'Broad circular drum dominates lower silhouette, cream taut drumhead, restrained lacing, grounded boots. No axe, staff, shield, facepaint noise or floating props.'),
 ('Briar_Archer',64,'Elven_Scout',
  'Slim adult woodland elf ambusher, ash-silver short hair, long pointed ears, warm natural skin. Muted wine-red hood folded behind head and short asymmetrical leaf-edged cloak. '
  'Uses a dark recurved bramble bow with two clear thorn knots and a pale wooden nocked arrow; both hands visibly hold the bow/arrow correctly. '
  'Upright poised stalker rather than the crouching brown-haired blue Scout. Pale face, wine cloth and dark bow separate clearly; no green skin, no extra vines on body, no shield.'),
 ('Ancient_Treant',96,'Barkhide_Warden',
  'Future miniboss ancient walking tree creature, native 96px canvas, about 78 pixels tall at unchanged pixel density. '
  'Wide hollow weathered trunk torso, thoughtful face carved naturally into bark, heavy root feet, one long knotted branch arm and one broken limb stump, sparse moss shoulder clumps. '
  'Asymmetrical old woodland sentinel, warm broad wood planes with cool gray dead-bark accent, two tiny amber eyes. No humanoid costume, no shield, no staff, no dense leafy crown or root-spell effect.')]

def prepare():
    OUT.mkdir(parents=True, exist_ok=True)
    jobs=[]
    for index,(name,size,ref,description) in enumerate(SPECS):
        path=ROOT/f'ArtSource/Forest/Approved/{ref}.png'
        jobs.append(dict(name=name,reference=path.relative_to(ROOT).as_posix(),request=dict(
            description=COMMON+description,width=size,height=size,no_background=True,seed=260003+index,
            style_image=dict(base64=base64.b64encode(path.read_bytes()).decode(),
                usage_description='Match native pixel scale, hard cluster shading, one-pixel outline and material treatment only. Preserve the new candidate identity described in the prompt.'),
            style_options=dict(outline=True,detail=True,shading=True,color_palette=True))))
    public=[dict(name=j['name'],reference=j['reference'],request={k:v for k,v in j['request'].items() if k!='style_image'}) for j in jobs]
    (OUT/'requests.json').write_text(json.dumps(public,indent=2),encoding='utf-8')
    (OUT/'budget.json').write_text(json.dumps(dict(totalCeiling=60,initialHardLimit=40,nominalFirstPass=35,
        reservedMinimumAfterReview=20,openingBalance=dict(remaining=1776,used=224,total=2000),
        status='Concept candidates only; pending user approval; no Unity imports or animations.'),indent=2),encoding='utf-8')
    print(json.dumps(jobs))

if __name__=='__main__': prepare()
