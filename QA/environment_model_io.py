import json,struct,zlib,io
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont
BASE=Path(r'C:\Users\NGOC BAO\Downloads')
def glb(path):
 b=path.read_bytes(); n=struct.unpack_from('<I',b,12)[0]; j=json.loads(b[20:20+n]); raw=b[28+n:]
 def acc(i):
  a=j['accessors'][i]; v=j['bufferViews'][a['bufferView']]; dt={5126:'f4',5125:'u4',5123:'u2',5121:'u1'}[a['componentType']]; k={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4}[a['type']]; off=v.get('byteOffset',0)+a.get('byteOffset',0)
  return np.ndarray((a['count'],k),dtype='<'+dt,buffer=raw,offset=off,strides=(v.get('byteStride',np.dtype(dt).itemsize*k),np.dtype(dt).itemsize)).copy()
 result=[]
 def walk(i,parent):
  node=j['nodes'][i]; m=np.eye(4)
  if 'matrix' in node:m=np.array(node['matrix']).reshape(4,4).T
  else:
   x,y,z,w=node.get('rotation',[0,0,0,1]); m[:3,:3]=np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],[2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],[2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]])@np.diag(node.get('scale',[1,1,1]));m[:3,3]=node.get('translation',[0,0,0])
  m=parent@m
  if 'mesh' in node:
   for p in j['meshes'][node['mesh']]['primitives']:
    a=p['attributes']; pos=acc(a['POSITION']);pos=(np.c_[pos,np.ones(len(pos))]@m.T)[:,:3]; faces=acc(p['indices']).reshape(-1,3) if 'indices' in p else np.arange(len(pos)).reshape(-1,3); uv=acc(a['TEXCOORD_0']) if 'TEXCOORD_0' in a else None
    mat=j.get('materials',[{}])[p.get('material',0)].get('pbrMetallicRoughness',{});color=np.array(mat.get('baseColorFactor',[.7,.7,.7,1]))[:3];tex=None
    if 'baseColorTexture' in mat:
     im=j['images'][j['textures'][mat['baseColorTexture']['index']]['source']];bv=j['bufferViews'][im['bufferView']];tex=np.array(Image.open(io.BytesIO(raw[bv.get('byteOffset',0):bv.get('byteOffset',0)+bv['byteLength']])).convert('RGB'))
    result.append((pos,faces,uv,tex,color))
  for c in node.get('children',[]):walk(c,m)
 for i in j['scenes'][j.get('scene',0)]['nodes']:walk(i,np.eye(4))
 return result
def fbx(path,texture):
 b=path.read_bytes();version=struct.unpack_from('<I',b,23)[0];wide=version>=7500;fmt='<QQQB' if wide else '<IIIB';size=25 if wide else 13
 def node(o):
  end,count,plen,nlen=struct.unpack_from(fmt,b,o);o+=size
  if end==0:return None,o
  name=b[o:o+nlen].decode();o+=nlen;props=[]
  for _ in range(count):
   t=chr(b[o]);o+=1
   if t in 'fdilbc':
    num,enc,l=struct.unpack_from('<III',b,o);o+=12;data=b[o:o+l];o+=l;data=zlib.decompress(data) if enc else data;props.append(np.frombuffer(data,dtype={'f':'<f4','d':'<f8','i':'<i4','l':'<i8','b':'u1','c':'u1'}[t]))
   elif t in 'SR':
    l=struct.unpack_from('<I',b,o)[0];o+=4;props.append(b[o:o+l]);o+=l
   else:
    f={'Y':'h','C':'?','I':'i','F':'f','D':'d','L':'q'}[t];props.append(struct.unpack_from('<'+f,b,o)[0]);o+=struct.calcsize(f)
  children=[]
  while o<end-size:
   c,o=node(o)
   if c:children.append(c)
   else:break
  return (name,props,children),end
 roots=[];o=27
 while o<len(b)-size:
  n,o=node(o)
  if not n:break
  roots.append(n)
 def find(ns,name):
  for n in ns:
   if n[0]==name:yield n
   yield from find(n[2],name)
 models={n[1][0]:n for n in find(roots,'Model')};links={n[1][1]:n[1][2] for n in find(roots,'C') if len(n[1])>=3 and n[1][0]==b'OO'}
 def transform(mid):
  if mid not in models:return np.eye(4)
  props={p[1][0]:p[1][4:] for p in find(models[mid][2],'P')}
  def mat(prefix):
   t=np.array(props.get(prefix+b'Translation',[0,0,0]),float);s=np.array(props.get(prefix+b'Scaling',[1,1,1]),float);r=np.radians(props.get(prefix+b'Rotation',[0,0,0]));x,y,z=r
   rx=np.array([[1,0,0],[0,np.cos(x),-np.sin(x)],[0,np.sin(x),np.cos(x)]]);ry=np.array([[np.cos(y),0,np.sin(y)],[0,1,0],[-np.sin(y),0,np.cos(y)]]);rz=np.array([[np.cos(z),-np.sin(z),0],[np.sin(z),np.cos(z),0],[0,0,1]])
   m=np.eye(4);m[:3,:3]=rz@ry@rx@np.diag(s);m[:3,3]=t;return m
  return transform(links.get(mid,0))@mat(b'Lcl ')
 result=[]
 for g in find(roots,'Geometry'):
  vs=list(find(g[2],'Vertices'));ids=list(find(g[2],'PolygonVertexIndex'))
  if not vs or not ids:continue
  pos=vs[0][1][0].reshape(-1,3);polys=ids[0][1][0];faces=[];uvfaces=[];poly=[];corn=[];uvnodes=list(find(g[2],'UV'));uin=list(find(g[2],'UVIndex'));uv=uvnodes[0][1][0].reshape(-1,2) if uvnodes else None
  for k,v in enumerate(polys):
   poly.append(int(v if v>=0 else -v-1));corn.append(k)
   if v<0:
    for t in range(1,len(poly)-1):faces.append([poly[0],poly[t],poly[t+1]]);uvfaces.extend([corn[0],corn[t],corn[t+1]])
    poly=[];corn=[]
  m=transform(links.get(g[1][0],0));pos=(np.c_[pos,np.ones(len(pos))]@m.T)[:,:3]
  faces=np.array(faces);expanded=pos[faces.reshape(-1)];uv=uv[uin[0][1][0][uvfaces]] if uv is not None and uin else None
  if uv is not None:uv[:,1]=1-uv[:,1]
  result.append((expanded,np.arange(len(expanded)).reshape(-1,3),uv,texture,np.ones(3)))
 return result
