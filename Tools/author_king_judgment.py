"""Author Judgment from approved native King poses; no scaling or new generation."""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'ArtSource/EnemyAttacks'
sheet = Image.open(OUT / 'King_AutoAttack.png').convert('RGBA')
data = json.loads((OUT / 'King_AutoAttack.json').read_text())
poses = [sheet.crop((f['frame']['x'], f['frame']['y'], f['frame']['x'] + 96, f['frame']['y'] + 80)) for f in data['frames']]


def heavy(pose, lean=0, drop=0):
    """Rigid head/torso travel over the original boots, with added weapon room."""
    src = poses[pose].copy()
    if pose == 3:
        # Remove the basic attack's early white trail. Judgment holds a clean
        # high guard, with the off-hand braced around the same hilt.
        d = ImageDraw.Draw(src)
        d.polygon([(19,0),(45,0),(36,18),(27,25),(23,15)], fill=(0,0,0,0))
        d.polygon([(62,54),(67,56),(67,70),(61,72),(60,66)], fill='#4B1127')
        d.polygon([(63,50),(67,53),(57,61),(50,60),(30,50),(28,44),(33,42),(38,48),(53,54)], fill='#0A0D11')
        d.polygon([(63,51),(65,54),(55,58),(51,57),(34,49),(34,46),(38,50),(53,55)], fill='#55606E')
        d.line([(62,51),(54,55),(36,48)], fill='#93A1B0', width=2)
        d.polygon([(29,43),(33,43),(35,47),(32,50),(29,48)], fill='#0A0D11')
        d.polygon([(30,44),(32,44),(34,47),(32,48),(30,47)], fill='#93A1B0')
        d.point((31,44),fill='#FDF5E5')
    image = Image.new('RGBA', (112, 80))
    image.alpha_composite(src.crop((0, 69, 96, 80)), (8, 69))
    # Preserve hip/greave joins beneath the lifted torso, never a transparent
    # horizontal seam between the rigid upper body and anchored boots.
    image.alpha_composite(src.crop((0, 65, 96, 71)), (8, 65))
    image.alpha_composite(src.crop((0, 0, 96, 69)), (8 + lean, drop))
    return image


def export(name, frames, durations, impact, provenance):
    w, h = frames[0].size
    atlas = Image.new('RGBA', (w * len(frames), h))
    entries = []
    for i, (frame, ms) in enumerate(zip(frames, durations)):
        atlas.alpha_composite(frame, (i * w, 0))
        entries.append({'filename': f'{name}_{i:02}.png', 'frame': {'x': i*w, 'y': 0, 'w': w, 'h': h},
                        'sourceSize': {'w': w, 'h': h}, 'duration': ms})
    atlas.save(OUT / (name + '.png'))
    (OUT / (name + '.json')).write_text(json.dumps({'frames': entries, 'provenance': provenance}, indent=2) + '\n')
    previews = []
    for frame in frames:
        canvas = Image.new('RGBA', frame.size, '#252332')
        canvas.alpha_composite(frame)
        previews.append(canvas.convert('RGB').resize((w*3, h*3), Image.Resampling.NEAREST))
    previews[0].save(OUT / (name + '.gif'), save_all=True, append_images=previews[1:], duration=durations, loop=0)
    palette = {p[:3] for frame in frames for p in frame.getdata() if p[3]}
    assert {p[3] for frame in frames for p in frame.getdata()} <= {0, 255}
    return {'name': name, 'character': 'King', 'state': name.split('_', 1)[1], 'durations': durations,
            'impactFrames': [impact], 'width': w, 'height': h, 'opaqueColors': len(palette),
            'sha256': hashlib.sha256((OUT / (name + '.png')).read_bytes()).hexdigest(), 'provenance': provenance}


entries = []
entries.append(export('King_JudgmentStrike1', [poses[i] for i in [0,1,2,3,4,5,6,7,8]],
                      [50,60,110,40,100,70,60,60,70], 4, 'Approved AutoAttack poses: deliberate first cut.'))
entries.append(export('King_JudgmentStrike2', [poses[i] for i in [8,1,3,4,5,6,7,8]],
                      [40,100,60,100,60,70,70,80], 3, 'Approved AutoAttack poses: faster second cut, distinct gathering cadence.'))

# A held high guard drives a deep committed cut. The native face/equipment
# clusters move rigidly, the boots remain planted, and the royal arc opens wide.
def low_cut(lean, drop, trail):
    body = heavy(5, lean, drop)
    d = ImageDraw.Draw(body)
    ox, oy = 8 + lean, drop
    def polygon(points, color): d.polygon([(x+ox,y+oy) for x,y in points],fill=color)
    # Remove the old outward blade; retain crown, face, torso, cape and boots.
    polygon([(64,53),(96,53),(96,69),(64,69)],(0,0,0,0))
    polygon([(76,53),(96,53),(96,79),(76,79)],(0,0,0,0))
    # A connected, two-handed low diagonal finish towards the player.
    polygon([(63,53),(65,58),(52,63),(41,61),(37,57),(40,53),(49,57),(58,54)],'#0A0D11')
    polygon([(62,54),(63,57),(51,60),(43,59),(41,56),(44,55),(50,58),(58,55)],'#55606E')
    polygon([(61,54),(62,55),(51,59),(43,57),(43,55),(50,57)],'#93A1B0')
    polygon([(39,57),(49,60),(48,64),(38,61)],'#0A0D11')
    polygon([(40,58),(42,58),(45,60),(43,62),(40,60)],'#93A1B0')
    polygon([(45,60),(48,60),(47,63),(44,62)],'#93A1B0')
    polygon([(37,55),(40,56),(37,64),(35,66),(33,65)],'#0A0D11')
    polygon([(37,56),(38,57),(35,64),(34,64)],'#C58826')
    polygon([(34,62),(37,66),(9,78),(3,78),(7,73)],'#0A0D11')
    polygon([(34,63),(35,65),(8,76),(6,76)],'#93A1B0')
    polygon([(35,65),(35,66),(9,77),(6,77)],'#55606E')
    if trail:
        arc=Image.new('RGBA',body.size);a=ImageDraw.Draw(arc)
        a.polygon([(19,5),(13,24),(15,43),(25,59),(39,69),(51,73),(26,72),(12,61),(4,39),(7,18)],fill='#C58826')
        a.polygon([(17,10),(11,29),(14,45),(25,62),(41,71),(26,69),(14,58),(7,39),(10,20)],fill='#FBEA90')
        arc.alpha_composite(body);body=arc
    return body

impact = low_cut(-3,2,True)
follow = low_cut(-3,2,False)
frames = [heavy(0), heavy(1,1), heavy(2,2,-1), heavy(3,2,-2), heavy(3,2,-2),
          impact, follow, heavy(0,-2,1), heavy(0,-1,1), heavy(8)]
entries.append(export('King_JudgmentFinisher', frames, [70,90,110,130,60,140,80,80,80,90], 5,
                      'Direct native edit of approved King: higher held windup, leftward committed torso, planted boots, expanded royal-gold cut and stepped recovery.'))
(OUT / 'judgment.json').write_text(json.dumps(entries, indent=2) + '\n')
print(json.dumps([{k: e[k] for k in ['name','width','height','opaqueColors','sha256']} for e in entries], indent=2))
