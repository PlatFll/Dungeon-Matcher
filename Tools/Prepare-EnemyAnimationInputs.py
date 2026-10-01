"""Inventory existing art and preserve exact native reference bytes for PixelLab."""
import base64, json, re
from pathlib import Path
root = Path(__file__).resolve().parents[1]
aliases = {'Knight':'SwordKnight', 'CourtMage':'RoyalMage', 'RoyalArchbishop':'RoyalArcanist'}
items=[]
for p in sorted((root/'Assets/_Game/Data/Enemies').glob('Enemy_*.asset')):
    text=p.read_text(encoding='utf-8-sig')
    if 'timeAutoAttackFromAnimation: 1' in text: continue
    name=p.stem.removeprefix('Enemy_')
    source=root/'ArtSource/RemainingCast/Recolored'/f'{aliases.get(name,name)}.png'
    items.append({'name':name,'source':source.relative_to(root).as_posix(), 'image':'data:image/png;base64,'+base64.b64encode(source.read_bytes()).decode(), 'ability':bool(re.search(r'hasSpecialAbility: 1',text))})
print(json.dumps(items))
