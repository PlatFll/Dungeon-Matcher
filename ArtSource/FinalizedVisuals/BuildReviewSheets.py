"""Read-only contact sheets. Never writes production textures or native sources."""
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parent
names=['ButtonLargeNormal','ButtonLargeHighlighted','ButtonLargePressed','ButtonLargeDisabled',
       'ButtonSmallNormal','SettingsNormal','PlayerBadge','NormalBadge','SpecialBadge','MinibossBadge','BossBadge',
       'HealthFrame','EnergyFrame','WavePlaque','DungeonMatcherLogo','SplitStoryGem']
sheet=Image.new('RGB',(960,980),'#171323'); draw=ImageDraw.Draw(sheet)
draw.text((16,10),'NATIVE REVIEW — 1x at left / up to 2x nearest-neighbor at right. Full 3x inspection in HTML.',fill='white')
for i,name in enumerate(names):
    im=Image.open(ROOT/f'UI/{name}.png').convert('RGBA'); x=(i%2)*480; y=40+(i//2)*117
    draw.rectangle((x+4,y+18,x+204,y+114),fill='#dedbdc')
    draw.text((x+10,y),name,fill='white')
    sheet.paste(im,(x+12,y+21),im)
    big=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST)
    # Tall logo receives its own native inspection in HTML; fit its proof at 1x here.
    if big.width>258 or big.height>82: big=im
    sheet.paste(big,(x+218,y+21),big)
sheet.save(ROOT/'Validation/NativeReview.png')
cast=Image.open(ROOT.parent/'CombatIdles/Rattlebones_Idle.png').convert('RGBA').crop((0,0,64,64))
master=Image.open(ROOT/'Environment/DungeonMaster.png').convert('RGBA')
board=Image.new('RGB',(640,430),'#dedbdc');board.paste(master,(0,30),master);board.paste(cast,(552,286),cast)
ImageDraw.Draw(board).text((12,8),'512x384 native dungeon / current 64x64 cast for scale',fill='#171323')
board.save(ROOT/'Validation/NativeEnvironmentReview.png')
print('Created read-only native inspection sheets.')
