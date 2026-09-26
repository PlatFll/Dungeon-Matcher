"""Verify that the restored guard sources and Unity copies preserve the selected backups."""
from pathlib import Path
import sys,json,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parent
PROJECT=ROOT.parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase,color_counts
report={}
for name in ['CrossbowGuard','BarricadeGuard','SpearGuard','SiegeSergeant']:
    stem=name+'_Idle';source=ROOT/(stem+'.aseprite')
    backup=ROOT.parent/'RemainingCast/BeforeCorrection/Idles'/(stem+'.aseprite')
    repaired=name=='CrossbowGuard'
    preserved=ROOT/'BeforeCrossbowRepair'/source.name if repaired else source
    assert preserved.read_bytes()==backup.read_bytes(),(name,'native backup changed')
    frames,ms=read_ase(source)
    sheet=Image.open(ROOT/(stem+'.png')).convert('RGBA')
    prior=Image.open(backup.with_suffix('.png')).convert('RGBA')
    if not repaired:assert sheet.tobytes()==prior.tobytes(),(name,'backup pixels changed')
    else:
        sys.path.insert(0,str(ROOT.parent/'RemainingCast/Scripts'))
        from validate_geometry import check_parts
        check_parts(name,frames)
        assert sheet.crop((0,0,64,64)).tobytes()==prior.crop((0,0,64,64)).tobytes(),(name,'ready pose changed')
    allowed=set(json.loads((ROOT.parent/'RemainingCast/Palettes.json').read_text())[name])
    assert set(color_counts(frames))<=allowed,(name,'palette drift')
    assert len(frames)==9 and ms==[130]*9 and sheet.size==(576,64)
    meta=json.loads((ROOT/(stem+'.json')).read_text())
    assert [f['duration'] for f in meta['frames']]==ms
    assert meta['meta']['image']==stem+'.png'
    gif=Image.open(ROOT/(stem+'.gif'))
    assert gif.n_frames==9
    for f,im in enumerate(frames):
        assert im.size==(64,64) and set(im.getchannel('A').get_flattened_data())<={0,255}
        assert im.getbbox()[3]==64,(name,f,'ground contact')
        gif.seek(f);assert gif.info['duration']==130
        for target in [sheet.crop((f*64,0,(f+1)*64,64)),gif.convert('RGBA')]:
            assert all(a==b if a[3] else b[3]==0 for a,b in zip(im.get_flattened_data(),target.get_flattened_data())),(name,f,'export pixels')
    assert frames[0].tobytes()==frames[-1].tobytes(),(name,'loop closure')
    unity=PROJECT/'Assets/_Game/Art/CombatIdles'/(stem+'.png')
    if unity.exists():assert unity.read_bytes()==(ROOT/(stem+'.png')).read_bytes(),(name,'Unity sheet differs')
    report[name]={'selected_source':str(backup.relative_to(PROJECT)).replace('\\','/'),'native_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sheet_sha256':hashlib.sha256((ROOT/(stem+'.png')).read_bytes()).hexdigest(),'frames':9,'duration_ms':130,'loop_ms':1170,'canvas':[64,64],'original_backup_preserved':True,'production_matches_backup':not repaired,'crossbow_contour_repaired':repaired,'exact_unity_sheet':unity.exists()}
(ROOT/'Verification.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: guard backups preserved; repaired crossbow rigid contour, 36 exact native/export frames, approved palettes, clean alpha and 130ms grounded loops. Unity copies checked when present.')
