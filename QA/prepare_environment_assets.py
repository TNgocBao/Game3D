"""Derive Unity OBJ assets from the user's original models without installing an importer."""
from pathlib import Path
import shutil,json
import numpy as np
from PIL import Image
import environment_model_io as model_io
root=Path(__file__).resolve().parents[1]/'Assets/Art/Environment'
downloads=Path(r'C:\Users\NGOC BAO\Downloads')
atlas=np.array(Image.open(downloads/'free-medieval-houses-3d-low-poly-pack/Texture/House_texture_atlas.png').convert('RGB'))
records=[]
def export(pack,name,source,meshes):
 if pack=='Medieval':
  vertices=[];faces=[];uvs=[];offset=0
  for p,f,uv,tex,color in meshes:
   vertices.append(p);faces.append(f+offset);uvs.append(uv);offset+=len(p)
  meshes=[(np.concatenate(vertices),np.concatenate(faces),np.concatenate(uvs),atlas,np.ones(3))]
 folder=root/pack;derived=folder/'Models';original=folder/'Source';derived.mkdir(parents=True,exist_ok=True);original.mkdir(exist_ok=True);shutil.copy2(source,original/source.name)
 allp=np.concatenate([m[0] for m in meshes]);low=allp.min(0);high=allp.max(0);height=high[1]-low[1];origin=np.array([(low[0]+high[0])/2,low[1],(low[2]+high[2])/2]);lines=[f'mtllib {name}.mtl'];mtl=[];offset=1
 for i,(p,f,uv,tex,color) in enumerate(meshes):
  p=(p-origin)/height;lines.extend([f'o mesh_{i}',f'usemtl {name}_{i}']);lines.extend('v '+' '.join(f'{v:.7f}' for v in pt) for pt in p)
  if uv is None:uv=np.zeros((len(p),2))
  lines.extend(f'vt {pt[0]:.7f} {1-pt[1]:.7f}' for pt in uv)
  for face in f:lines.append('f '+' '.join(f'{int(v)+offset}/{int(v)+offset}' for v in face))
  offset+=len(p);mtl.extend([f'newmtl {name}_{i}','Kd '+' '.join(str(float(v)) for v in color),'Ks 0 0 0','d 1'])
  if tex is not None:
   fn=f'{name}_{i}.png';Image.fromarray(tex).save(derived/fn);mtl.append('map_Kd '+fn)
 (derived/(name+'.obj')).write_text('\n'.join(lines),encoding='utf-8');(derived/(name+'.mtl')).write_text('\n'.join(mtl),encoding='utf-8')
 records.append({'pack':pack,'name':name,'vertices':sum(len(m[0]) for m in meshes),'triangles':sum(len(m[1]) for m in meshes),'normalized_size':((high-low)/height).tolist(),'materials':len(meshes)})
for name in ['tree_001','house2','house3','house4','house6']:
 source=downloads/'StylooVillageFREEPack/StylooVillageFREEPack/gltf'/(name+'.glb');export('Styloo',name,source,model_io.glb(source))
for name in ['House_01_full','House_04_full']:
 source=downloads/'free-medieval-houses-3d-low-poly-pack/fbx/House_Full_ordinar'/(name+'.fbx');export('Medieval',name,source,model_io.fbx(source,atlas))
shutil.copy2(downloads/'StylooVillageFREEPack/StylooVillageFREEPack/read me .txt',root/'Styloo/Source/Readme.txt')
for fn in ['License.txt','readme.txt']:shutil.copy2(downloads/'free-medieval-houses-3d-low-poly-pack'/fn,root/'Medieval/Source'/fn)
shutil.copy2(downloads/'free-medieval-houses-3d-low-poly-pack/Texture/House_texture_atlas.png',root/'Medieval/Source/House_texture_atlas.png')
(root/'ImportManifest.json').write_text(json.dumps(records,indent=2),encoding='utf-8');print(json.dumps(records,indent=2))
