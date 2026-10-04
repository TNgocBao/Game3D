import bpy
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.render.fps=30
bpy.ops.import_scene.gltf(filepath=str(root/'Assets/Art/Kurama/Source/scene.gltf'))
bpy.context.scene.render.fps=30
bpy.context.scene.frame_set(1)
for obj in list(bpy.context.scene.objects):
 if obj.type=='MESH' and not any(m.type=='ARMATURE' for m in obj.modifiers):bpy.data.objects.remove(obj,do_unlink=True)
print([(o.name,o.type) for o in bpy.context.scene.objects],flush=True)
print([(a.name,a.frame_range[:]) for a in bpy.data.actions],flush=True)
from mathutils import Quaternion,Vector
import math
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
attack=bpy.data.actions[0];attack.name='Attack';bpy.context.scene.frame_set(0)
base={b.name:b.matrix_basis.decompose() for b in rig.pose.bones}
inv=rig.matrix_world.to_quaternion().inverted()
for name,length in [('Idle',60),('Run',30)]:
 action=bpy.data.actions.new(name);action.use_fake_user=True;rig.animation_data_create();rig.animation_data.action=action
 for frame in range(length+1):
  phase=frame/length*math.tau
  for bone in rig.pose.bones:
   loc,rot,scale=base[bone.name];bone.rotation_mode='QUATERNION';bone.location=loc.copy();bone.rotation_quaternion=rot.copy();bone.scale=scale.copy()
   angle=0;n=bone.name
   if name=='Run':
    side=-1 if ' L ' in n else 1
    if 'Thigh' in n:angle=math.sin(phase)*side*32
    if 'Calf' in n:angle=max(0,-math.sin(phase)*side)*54
    if 'HorseLink' in n:angle=-max(0,-math.sin(phase)*side)*20
    if 'Foot' in n:angle=-math.sin(phase)*side*14
    if 'UpperArm' in n:angle=-math.sin(phase)*side*16
    if 'Forearm' in n:angle=-12-max(0,math.sin(phase)*side)*14
    if 'Spine1' in n:angle=2
   elif 'Spine1' in n:angle=math.sin(phase)*1.4
   axis=bone.bone.matrix_local.to_quaternion().inverted() @ (inv @ Vector((1,0,0)))
   bone.rotation_quaternion=rot @ Quaternion(axis,math.radians(angle))
   if n.startswith('Bone0') and n not in ['Bone001_014','Bone038_015','Bone039_016']:
    axis2=bone.bone.matrix_local.to_quaternion().inverted() @ (inv @ Vector((0,0,1)))
    bone.rotation_quaternion=bone.rotation_quaternion @ Quaternion(axis2,math.sin(phase+int(n.split('_')[0][4:])*.3)*math.radians(4 if name=='Idle' else 7))
   bone.keyframe_insert('location',frame=frame);bone.keyframe_insert('rotation_quaternion',frame=frame);bone.keyframe_insert('scale',frame=frame)
# Plant all four paws with bone rotations, preserving limb lengths.
from mathutils import Matrix
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
group_names={g.index:g.name for g in mesh.vertex_groups}
def bone(part,side):
 return next(b for b in rig.pose.bones if (' '+side+' '+part+'_') in b.name)
def world_head(b):return rig.matrix_world@b.head
def mesh_points():
 ev=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh()
 points=[mesh.matrix_world@v.co for v in m.vertices];ev.to_mesh_clear();return points
rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(0)
points=mesh_points();ground=min(v.z for v in points)
chains=[]
for front in [True,False]:
 for side in ['L','R']:
  end=bone('Hand' if front else 'Foot',side)
  joints=[bone(part,side) for part in (['UpperArm','Forearm'] if front else ['Thigh','Calf','HorseLink'])]
  ids=[v.index for v in mesh.data.vertices if any(g.weight>.4 and (' '+side+' '+('Hand' if front else 'Foot') in group_names[g.group] or (front and ' '+side+' Finger' in group_names[g.group])) for g in v.groups)]
  sole=min(points[i].z for i in ids)
  chains.append((front,side,end,joints,ids,world_head(end).copy(),(rig.matrix_world@end.matrix).to_quaternion(),world_head(end).z-sole))
head=next(b for b in rig.pose.bones if b.name.startswith('Bip01 Head'))
mouth=next(b for b in rig.pose.bones if b.name.startswith('Shootpoint_017'))
forward=world_head(mouth)-world_head(head);forward.z=0;forward.normalize()
def solve(end,joints,target):
 for iteration in range(24):
  if (world_head(end)-target).length<.001:break
  for joint in reversed(joints):
   pivot=world_head(joint);current=world_head(end)-pivot;desired=target-pivot
   if current.length<.00001 or desired.length<.00001:continue
   delta=current.rotation_difference(desired)
   joint.matrix=rig.matrix_world.inverted()@Matrix.Translation(pivot)@delta.to_matrix().to_4x4()@Matrix.Translation(-pivot)@rig.matrix_world@joint.matrix
   bpy.context.view_layer.update()
run=bpy.data.actions['Run'];rig.animation_data.action=run
for frame in range(31):
 bpy.context.scene.frame_set(frame)
 for front,side,end,joints,ids,rest,orientation,offset in chains:
  phase=frame/30*math.tau+(0 if (front and side=='L') or (not front and side=='R') else math.pi)
  target=rest+forward*(.45*math.cos(phase));target.z=ground+offset+.025+.4*max(0,math.sin(phase))
  for correction in range(3):
   solve(end,joints,target)
   posed=rig.matrix_world@end.matrix;loc,rot,scale=posed.decompose()
   end.matrix=rig.matrix_world.inverted()@Matrix.LocRotScale(loc,orientation,scale)
   bpy.context.view_layer.update()
   evaluated_points=mesh_points();minimum=min(evaluated_points[i].z for i in ids)
   if minimum>=ground+.01:break
   target.z+=ground+.015-minimum
  for b in joints+[end]:
   b.keyframe_insert('location',frame=frame);b.keyframe_insert('rotation_quaternion',frame=frame);b.keyframe_insert('scale',frame=frame)
# Scan each paw over the complete cycle before exporting.
minimum=10**9
for frame in range(31):
 bpy.context.scene.frame_set(frame);points=mesh_points()
 for front,side,end,joints,ids,rest,orientation,offset in chains:minimum=min(minimum,min(points[i].z-ground for i in ids))
print('Run paw clearance minimum:',minimum,flush=True)
assert minimum>-.015, 'Run paws penetrate the ground'

rig.animation_data.action=bpy.data.actions.get('Idle');bpy.context.scene.frame_set(0)
out=root/'Assets/Art/Kurama/Models';out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out/'Kurama.fbx'),use_selection=False,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='AUTO')
source=root/'ArtSource/Kurama';source.mkdir(parents=True,exist_ok=True)
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(source/'Kurama-Imported.blend'))
