"""Read-only audit and faithful studio renders of the supplied kit GLBs."""
import bpy, json, math, struct, hashlib
from pathlib import Path
from mathutils import Vector
KIT=Path(r'C:\Users\NGOC BAO\Downloads\Bijuu_Matatabi_Gyuki_Production_Kit\Bijuu_Production_Kit')
OUT=Path(__file__).resolve().parents[1]/'Logs/ProductionKitAudit'
OUT.mkdir(parents=True,exist_ok=True)
report={'files':[], 'models':{}}
for p in sorted(KIT.rglob('*')):
 if p.is_file():
  item={'path':str(p.relative_to(KIT)), 'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
  if p.suffix=='.png':
   from PIL import Image
   with Image.open(p) as im: item.update(size=im.size,mode=im.mode,alpha=im.getextrema()[-1] if im.mode=='RGBA' else None)
  report['files'].append(item)
for kind,file in [('Matatabi','Matatabi_Two_Tails.glb'),('Gyuki','Gyuki_Eight_Tails.glb')]:
 p=KIT/'Models/PROXY_ONLY'/file
 raw=p.read_bytes();length,typ=struct.unpack_from('<II',raw,12);gltf=json.loads(raw[20:20+length])
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=str(p))
 scene=bpy.context.scene; meshes=[o for o in scene.objects if o.type=='MESH']
 points=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box]
 lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
 size=hi-lo;center=(lo+hi)/2;radius=max(size.length/2,.01)
 report['models'][kind]={'mesh_objects':len(meshes),'vertices':sum(len(o.data.vertices) for o in meshes),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes),'bounds_min':list(lo),'bounds_max':list(hi),'dimensions':list(size),'armatures':len([o for o in scene.objects if o.type=='ARMATURE']),'gltf_skins':len(gltf.get('skins',[])),'gltf_animations':len(gltf.get('animations',[])),'gltf_materials':gltf.get('materials',[]),'images':gltf.get('images',[]),'meshes':[{'name':o.name,'vertices':len(o.data.vertices),'uv_layers':len(o.data.uv_layers),'color_attributes':[a.name for a in o.data.color_attributes]} for o in meshes]}
 scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=800;scene.render.resolution_y=700;scene.render.resolution_percentage=100
 scene.world=bpy.data.worlds.new('Neutral studio');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.14,.18,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.6
 scene.view_settings.view_transform='Standard'
 bpy.ops.mesh.primitive_plane_add(size=radius*12,location=(center.x,center.y,lo.z-.005))
 ground=bpy.context.object;mat=bpy.data.materials.new('Studio floor');mat.diffuse_color=(.17,.19,.23,1);ground.data.materials.append(mat)
 for pos,power,scale in [((3,-4,5),1700,4),((-4,-1,3),1000,3),((1,4,5),2000,3)]:
  bpy.ops.object.light_add(type='AREA',location=center+Vector(pos)*radius/2)
  lamp=bpy.context.object;lamp.data.energy=power*radius*radius/24;lamp.data.shape='DISK';lamp.data.size=scale*radius/2;lamp.rotation_euler=(center-lamp.location).to_track_quat('-Z','Y').to_euler()
 bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=radius*2.35;cam.data.clip_end=radius*100
 for label,vec in [('Front',(0,-1,.16)),('Side',(1,0,.16)),('Back',(0,1,.16)),('ThreeQuarter',(1,-1,.5))]:
  cam.location=center+Vector(vec).normalized()*radius*5;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(OUT/f'{kind}-{label}.png');bpy.ops.render.render(write_still=True)
 (OUT/'Audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
from PIL import Image,ImageDraw
for kind in report['models']:
 sheet=Image.new('RGB',(1600,1480),(26,29,35));draw=ImageDraw.Draw(sheet)
 draw.text((20,12),f'{kind} | ORIGINAL KIT GLB - PROXY ONLY',(240,240,240))
 for i,label in enumerate(['Front','Side','Back','ThreeQuarter']):
  with Image.open(OUT/f'{kind}-{label}.png') as im:sheet.paste(im.convert('RGB'),((i%2)*800,40+(i//2)*720))
  draw.text(((i%2)*800+20,45+(i//2)*720),label,(255,255,255))
 sheet.save(OUT/f'{kind}-ContactSheet.jpg',quality=94)
print('Completed two GLB audits and eight renders.')

