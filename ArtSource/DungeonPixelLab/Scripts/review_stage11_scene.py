from pathlib import Path
import re,subprocess,json,collections,difflib
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab'
scene='Assets/_Game/Scenes/ArtPreview_DungeonPixelLab.unity'
before=subprocess.check_output(['rtk','proxy','git','show','aff7f3a35e429005fd81c2ba7f55042b2e60f4de:'+scene],cwd=P).decode('utf8');after=(P/scene).read_text(encoding='utf8')
def canon(text):
 blocks={m[2]:(m[1],m[3]) for m in re.finditer(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
 transforms={};names={};parents={};labels={}
 for ident,(typ,b) in blocks.items():
  if typ=='1':
   n=re.search(r'^  m_Name: (.*)$',b,re.M)[1]
   if n.startswith('Dungeon PixelLab '):n='Dungeon PixelLab preview root'
   comps=re.findall(r'component: \{fileID: (\d+)\}',b)
   tid=next((i for i in comps if i in blocks and blocks[i][0] in ['224','4']),None)
   t=blocks[tid][1] if tid else '';pos=re.search(r'm_AnchoredPosition: (.*)',t)
   transforms[tid]=ident;parents[ident]=re.search(r'm_Father: \{fileID: (\d+)\}',t)[1] if tid else '0'
   names[ident]=n+(' @'+pos[1] if pos else '')
 def path(i):
  parent=transforms.get(parents[i]);return (path(parent)+'/' if parent else '')+names[i]
 for ident,(typ,b) in blocks.items():
  go=re.search(r'm_GameObject: \{fileID: (\d+)\}',b)
  labels[ident]=typ+':'+(path(ident) if typ=='1' else path(go[1]) if go and go[1] in names else 'global')
 count=collections.Counter(labels.values());assert max(count.values())==1,[k for k,v in count.items() if v>1]
 out={}
 for ident,(typ,b) in blocks.items():
  b=re.sub(r'Dungeon PixelLab [^\n]*candidate','Dungeon PixelLab preview root',b)
  b=re.sub(r'fileID: (\d+)',lambda m:'fileID: '+labels.get(m[1],m[1]),b)
  out[labels[ident]]='\n'.join(x.rstrip() for x in b.splitlines())
 return out
a,b=canon(before),canon(after)
added=sorted(b.keys()-a.keys());removed=sorted(a.keys()-b.keys());changes=[]
for k in sorted(a.keys()&b.keys()):
 if a[k]!=b[k]:changes.append(dict(object=k,diff='\n'.join(difflib.unified_diff(a[k].splitlines(),b[k].splitlines(),n=2))))
assert all('Portrait_' in k or 'Recess_Inset' in k for k in removed),removed
assert all('Portrait panel' in k or 'Board panel' in k for k in added),added
assert len(changes)==2 and all('Top UI' in x['object'] or x['object'].endswith('Dungeon PixelLab preview root') for x in changes),changes
out=dict(scene=scene,added_objects=added,removed_objects=removed,changed_existing_objects=changes)
(R/'Diagnostics/Stage11/SceneDiffReview.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf8')
print(json.dumps(dict(added=len(added),removed=len(removed),changed=[x['object'] for x in changes])))
