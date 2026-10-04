"""Keep convex-hull vertices per identical skin-weight group: affine skinning preserves extrema."""
import json
from pathlib import Path
import numpy as np
from scipy.spatial import ConvexHull, QhullError
root=Path(__file__).resolve().parents[1]
report=[]
for name in ['Matatabi','Gyuki','Kurama']:
 data=json.loads((root/f'Logs/BijuuLiveQA/SkinData/{name}.json').read_text(encoding='utf-8-sig'))
 vertices=data['vertices']; points=np.array([[p['x'],p['y'],p['z']] for p in vertices])
 groups={}
 for i,w in enumerate(data['weights']):
  key=tuple(w[k] for k in ['b0','b1','b2','b3','w0','w1','w2','w3'])
  groups.setdefault(key,[]).append(i)
 selected=[]
 for ids in groups.values():
  if len(ids)<8: selected.extend(ids);continue
  unique,at=np.unique(points[ids],axis=0,return_index=True)
  ids=np.array(ids)[at]
  if len(ids)<8:selected.extend(ids.tolist());continue
  try:
   hull=ConvexHull(unique)
   selected.extend(ids[hull.vertices].tolist())
  except QhullError:
   # Degenerate groups retain all samples; no random jitter or approximate support points.
   selected.extend(ids.tolist())
 selected=sorted(set(selected))
 out={'vertexCount':len(vertices),'indices':selected}
 (root/f'Assets/Resources/LumiEnemies/{name}-HitHull.json').write_text(json.dumps(out,separators=(',',':')),encoding='utf-8')
 report.append(f'{name}: {len(vertices)} vertices -> {len(selected)} hull samples; {len(groups)} identical-weight groups')
(root/'Logs/BijuuLiveQA/HitHullBuild.txt').write_text('\n'.join(report),encoding='utf-8')
print('\n'.join(report))
