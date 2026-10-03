import bpy,math,json
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent;ROOT=OUT.parents[1];S=.90
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def lin(x):return ((x+.055)/1.055)**2.4 if x>.04045 else x/12.92
mats={};palette={};name=None
for line in (OUT/'NarutoPalette.mtl').read_text().splitlines():
 if line.startswith('newmtl '):name=line.split()[1]
 elif line.startswith('Kd '):
  rgb=list(map(float,line.split()[1:]));palette[name]=rgb;m=bpy.data.materials.new(name);m.diffuse_color=(*[lin(c) for c in rgb],1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=m.diffuse_color;p.inputs['Roughness'].default_value=.55;p.inputs['Metallic'].default_value=.6 if 'Metal' in name else 0;mats[name]=m
verts=[];objects=[];obj=None;material='Skin'
for line in (OUT/'FullCharacter_Static.obj').read_text().splitlines():
 if line.startswith('v '):x,y,z=map(float,line.split()[1:]);verts.append((x*S,z*S,y*S))
 elif line.startswith('o '):obj={'name':line[2:],'faces':[],'material':material};objects.append(obj)
 elif line.startswith('usemtl '):material=line.split()[1];obj['material']=material
 elif line.startswith('f '):obj['faces'].append([int(x.split('/')[0])-1 for x in line.split()[1:]])
parts=[]
for spec in objects:
 indices=sorted(set(i for f in spec['faces'] for i in f));lookup={v:i for i,v in enumerate(indices)};mesh=bpy.data.meshes.new(spec['name']);mesh.from_pydata([verts[i] for i in indices],[],[[lookup[i] for i in f] for f in spec['faces']]);mesh.update();o=bpy.data.objects.new(spec['name'],mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mats[spec['material']]);parts.append(o)
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.mesh.dissolve_limited(angle_limit=.001);bpy.ops.object.mode_set(mode='OBJECT')
 # Keep the supplied silhouette while smoothing round low-poly parts; bevel angular trim.
 before=[(min(v.co[a] for v in mesh.vertices),max(v.co[a] for v in mesh.vertices)) for a in range(3)]
 if len(mesh.vertices)>20:
  mod=o.modifiers.new('Smooth supplied mesh','SUBSURF');mod.levels=2;bpy.ops.object.modifier_apply(modifier=mod.name)
  for a in range(3):
   lo=min(v.co[a] for v in o.data.vertices);hi=max(v.co[a] for v in o.data.vertices);targetLo,targetHi=before[a]
   if hi-lo>1e-7:
    for v in o.data.vertices:v.co[a]=targetLo+(v.co[a]-lo)/(hi-lo)*(targetHi-targetLo)
 else:
  mod=o.modifiers.new('Soft trim edges','BEVEL');mod.width=.016 if spec['name'].startswith(('Jacket','Headband')) else .004;mod.segments=4;bpy.ops.object.modifier_apply(modifier=mod.name)
 for p in o.data.polygons:p.use_smooth=True
 center=sum((v.co for v in o.data.vertices),Vector())/len(o.data.vertices);side='Left' if center.x<0 else 'Right'
 if spec['name'].startswith(('Head','Face','Hair')):bone='Head pivot'
 elif spec['name'].startswith(('Pants','Sandals')):bone=side+' leg'
 elif spec['name'].startswith('Hands'):bone=side+' hand'
 elif spec['name'].startswith('Jacket') and int(spec['name'].split('_')[1]) in range(4,12):bone=side+' arm'
 else:bone='Body pivot'
 if bone.endswith(' arm'):
  upper=o.vertex_groups.new(name=bone);lower=o.vertex_groups.new(name=side+' elbow')
  for v in o.data.vertices:
   t=max(0,min(1,(1.39*S-v.co.z)/(.25*S)));t=t*t*(3-2*t);upper.add([v.index],1-t,'REPLACE');lower.add([v.index],t,'REPLACE')
 else:g=o.vertex_groups.new(name=bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
bones={'Rig root':((0,0,0),None),'Body pivot':((0,0,1.15),'Rig root'),'Head pivot':((0,-.018,1.875),'Body pivot')}
for side,label in [(-1,'Left'),(1,'Right')]:
 bones[label+' arm']=((side*.30,.005,1.51),'Body pivot');bones[label+' elbow']=((side*.425,.012,1.22),label+' arm');bones[label+' hand']=((side*.487,-.05,.87),label+' elbow');bones[label+' leg']=((side*.175,0,.91),'Rig root')
bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='Naruto Starter Rig';bpy.ops.object.mode_set(mode='EDIT');rig.data.edit_bones.remove(rig.data.edit_bones[0])
for name,(pos,parent) in bones.items():
 b=rig.data.edit_bones.new(name);b.head=Vector(pos)*S;b.tail=b.head+Vector((0,0,.1))
 if parent:b.parent=rig.data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT');bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();model=parts[0];model.name='Naruto Starter Skinned';mod=model.modifiers.new('Gameplay skeleton','ARMATURE');mod.object=rig;model.parent=rig
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);model.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Art/NarutoStarter/NarutoStarter.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
(ROOT/'Assets/Art/NarutoStarter/Palette.json').write_text(json.dumps(palette,indent=2))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True;scene.render.resolution_x=768;scene.render.resolution_y=960;scene.render.resolution_percentage=100;scene.world.color=(.18,.18,.18)
def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200);ground=bpy.context.object;ground.location.z=-.005
for pos,energy,size in [((-3,-4,5),450,4),((3,-2,3),180,3),((1,3,4),300,3)]:
 bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=energy;o.data.size=size;aim(o,(0,0,1))
bpy.ops.object.camera_add(location=(0,-4,1.1));cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=2.65;aim(cam,(0,0,1.08));scene.camera=cam
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'NarutoStarter-Rigged.blend'));print('READY',len(model.data.vertices),'vertices',flush=True)
scene.render.filepath=str(OUT/'NarutoStarter-Front.png');bpy.ops.render.render(write_still=True)
