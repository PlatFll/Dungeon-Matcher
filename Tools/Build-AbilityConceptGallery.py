"""Package original PixelLab frames into a portable local review page; no image editing."""
import base64,json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=root/'ArtSource/EnemyAttacks'
specs=[
 ('King_GroundThrust','King','Royal Bombardment','Two sword plants; the board refills only after the second strike.'),
 ('King_RoyalCommand','King','Royal Assault / Judgment','A pointed sword command for the charge; the same raised blade can cue Judgment with a separate gold target flare.'),
 ('CourtMage_FrostSeal','Court Mage','Frost Seal','Staff anticipation, cyan release and short recovery. Ice seals land on the targeted gems at release.'),
 ('CrossbowGuard_ChainShot','Crossbow Guard','Chain Shot','Aim, hooked bolt and chain whip. Links bind the selected cells at impact.'),
 ('Archbishop_Benediction','Royal Archbishop','Restoration / Benediction','A green healing pulse for Restoration; a gold ward ring for Benediction. The draft colors will be brought back to the game palette.'),
 ('StandardBearer_Plant','Royal Standard Bearer','Plant the Standard','A planted pole and one forceful cloth ripple; the board banner appears at ground contact.'),
 ('ShieldKnight_Bulwark','Shield Knight','Bulwark','Brace, cyan shield rim and short protective pulse toward allies.'),
 ('Marshal_Rally','Town Marshal','Summon / Rally','Two bell beats signal reinforcements; a single gold pulse can distinguish the rally buff.'),
 ('BarricadeGuard_RaiseWall','Barricade Guard','Raise Barricades','Kneel and hammer once; board timbers rise at the impact.'),
 ('SiegeSergeant_HammerTime','Siege Sergeant','Hammer Time / Defenses','A heavy ground slam with a sharp amber pulse; a shorter placement beat can cue the defensive walls.'),
 ('KnightCaptain_Command','Knight Captain','Chains / Command','The raised sword signals the formation; a sharp point releases the command or targeted chain effect.')]
def data(path):return 'data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
items=[]
for ident,enemy,title,note in specs:
    frames=sorted((source/'Candidates'/ident).glob('*.png'))
    if frames:items.append(dict(id=ident,enemy=enemy,title=title,note=note,frames=[data(p) for p in frames]))
gems=[data(root/f'Assets/_Game/Art/Gems/Small gems/Gesm16/{name}16.png') for name in ['Ruby','Sapphire','Emerald']]
html=(source/'concepts-template.html').read_text(encoding='utf-8').replace('__DATA__',json.dumps(items)).replace('__GEMS__',json.dumps(gems))
dest=source/'AbilityConcepts.html';dest.write_text(html,encoding='utf-8')
print(f'{len(items)} concepts packaged: {dest}')
