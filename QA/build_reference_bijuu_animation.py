"""Rig and animate the user-provided colored GLB meshes without replacing their geometry."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
ROOT=Path(__file__).resolve().parents[1]
def P(x,y,z):return Vector((x,-z,y))
def solve(rig,end,joints,target):
 for _ in range(24):
  current=rig.matrix_world@end.head
  if (current-target).length<.0005:break
  for joint in reversed(joints):
   at=rig.matrix_world@joint.head;a=(rig.matrix_world@end.head)-at;b=target-at
   if a.length<.00001 or b.length<.00001:continue
   delta=a.rotation_difference(b)
   joint.matrix=rig.matrix_world.inverted()@Matrix.Translation(at)@delta.to_matrix().to_4x4()@Matrix.Translation(-at)@rig.matrix_world@joint.matrix
   bpy.context.view_layer.update()
def build(kind):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.context.scene.render.fps=30
 bpy.ops.import_scene.gltf(filepath=str(ROOT/'Assets/Art/BijuuReference/Source'/f'{kind}.glb'))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 bpy.ops.object.select_all(action='DESELECT');bpy.ops.object.armature_add();rig=bpy.context.object;rig.name=kind+'_Rig'
 bpy.ops.object.mode_set(mode='EDIT');rig.data.edit_bones.remove(rig.data.edit_bones[0])
 def add(name,head,tail,parent=None):
  b=rig.data.edit_bones.new(name);b.head=head;b.tail=tail
  if parent:b.parent=rig.data.edit_bones[parent]
  return name
 cat=kind=='Matatabi';spine=add('Body',P(0,1.85 if cat else 1.85,0),P(0,2.2 if cat else 3.2,.55))
 head=add('Head',P(0,2.45 if cat else 3.0,1.3 if cat else 1.1),P(0,2.6 if cat else 3.1,1.9 if cat else 1.5),spine)
 add('Bijuu Mouth',P(0,2.29 if cat else 2.36,2.37 if cat else 2.0),P(0,2.29 if cat else 2.36,2.49 if cat else 2.12),head)
 leg_data=[];arm_data=[];tail_data={}
 for side,label in [(-1,'L'),(1,'R')]:
  for front in ([True,False] if cat else []):
   prefix=('Front' if front else 'Hind')+'_'+label
   if cat:
    x=side*(.49 if front else .6);z=.77 if front else -.93;h=1.71 if front else 1.56
    a=P(x,h,z);b=P(x,h*.63,z-.14);c=P(x,.44 if front else .42,z+.29)
   else:a=P(side*.71,1.8,-.17);b=P(side*.75,1.0,-.3);c=P(side*.8,.27,.64)
   upper=add(prefix+'_Upper',a,b,spine);lower=add(prefix+'_Lower',b,c,upper);foot=add(prefix+'_Foot',c,c+P(0,0,.25),lower)
   leg_data.append((front,label,upper,lower,foot,c.copy()))
  if not cat:
   a=P(side*1.48,2.4,.34);b=P(side*2.0,1.35,.55);c=P(side*2.1,.3,1.66)
   up=add('Arm_'+label+'_Upper',a,b,spine);lo=add('Arm_'+label+'_Lower',b,c,up);hand=add('Arm_'+label+'_Hand',c,c+P(0,0,.2),lo);arm_data.append((label,up,lo,hand));leg_data.append((True,label,up,lo,hand,c.copy()))
 for i in range(2 if cat else 8):
  if cat:
   side=-1 if i==0 else 1;label='L' if i==0 else 'R';name='Tail_'+label
   points=[P(side*.34,2.1,-1.14),P(side*.90,2.52,-1.85),P(side*1.33,3.24,-2.5),P(side*1.28,3.92,-2.65),P(side*.77,4.25,-2.37),P(side*.38,3.98,-2.05)]
  else:
   angle=2*math.pi*i/8;x=math.cos(angle);v=math.sin(angle);origin=P(x*.6,1.15+v*.3,-.68);name='Tentacle_'+str(i+1)
   points=[origin,origin+P(x*.65,.05,-.55),origin+P(x*1.75,.45+v*.55,-1.30),origin+P(x*2.35,1.15+v*.4,-1.40),origin+P(x*2.55,1.85+v*.45,-.8),origin+P(x*2.2,2.25+v*.4,-.15),origin+P(x*1.65,1.94+v*.4,.03)]
  names=[];parent=spine
  for j in range(len(points)-1):
   parent=add(f'Tail_{i+1:02d}_{j:02d}',points[j],points[j+1],parent);names.append(parent)
  tail_data[name]=(points,names)
 bpy.ops.object.mode_set(mode='OBJECT')
 # Assign smooth deformation weights by actual mesh part and tail centerline.
 for obj in meshes:
  obj.parent=rig;mod=obj.modifiers.new('Bijuu skin','ARMATURE');mod.object=rig
  def weight(vertex,name,value):
   group=obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name);group.add([vertex],value,'REPLACE')
  for vertex in obj.data.vertices:
   position=obj.matrix_world@vertex.co;name=obj.name;assigned=False
   for tail,(points,names) in tail_data.items():
    matches=name.startswith(tail) if not cat else (name in [tail,'Tail_core_'+tail[-1]] or name.startswith('TailFire_'+tail[-1]+'_'))
    if matches:
     distances=sorted(((position-points[j]).length,j) for j in range(len(names)))[:2]
     if len(distances)==1:weight(vertex.index,names[0],1)
     else:
      a,j=distances[0];b,k=distances[1];w=b*b/max(.000001,a*a+b*b);weight(vertex.index,names[j],w);weight(vertex.index,names[k],1-w)
     assigned=True;break
   if assigned:continue
   for front,label,upper,lower,foot,rest in leg_data:
    prefix=('Front' if front else 'Hind')
    match=(cat and name.startswith(prefix+'_') and ('_'+label in name)) or (not cat and (name=='Thigh_'+label or name=='Shin_'+label or name.startswith('Hoof_'+label) or name.startswith('HoofSplit_'+label)))
    if match:
     if ('Paw' in name or 'Claw' in name or 'Hoof' in name):weight(vertex.index,foot,1)
     elif 'Shoulder' in name or 'Thigh' in name:weight(vertex.index,upper,1)
     else:
      knee=rig.data.bones[lower].head_local.z;w=max(0,min(1,(position.z-knee+.16)/.32));weight(vertex.index,upper,w);weight(vertex.index,lower,1-w)
     assigned=True;break
   if assigned:continue
   if not cat:
    for label,up,lo,hand in arm_data:
     if name=='Shoulder_'+label or name=='UpperArm_'+label:weight(vertex.index,up,1);assigned=True;break
     if name=='Forearm_'+label:weight(vertex.index,lo,1);assigned=True;break
     if name=='Fist_'+label or name.startswith('HandClaw_'+label+'_'):weight(vertex.index,hand,1);assigned=True;break
   if assigned:continue
   head_part=any(name.startswith(p) for p in ['Head','Muzzle','Chin','Nose','Ear','InnerEar','Eye','Pupil','Whisker','Forehead','Snout','Nostril','Brow','Iris','Horn'])
   weight(vertex.index,head if head_part else spine,1)
 # One skinned renderer keeps preview and collision inexpensive.
 bpy.ops.object.select_all(action='DESELECT')
 for obj in meshes:obj.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();mesh=bpy.context.object;mesh.name=kind+'_Skin'
 for poly in mesh.data.polygons:poly.use_smooth=True
 base={b.name:b.matrix_basis.copy() for b in rig.pose.bones};ground=min((mesh.matrix_world@v.co).z for v in mesh.data.vertices)
 soles={foot:rest.z-min((mesh.matrix_world@v.co).z for v in mesh.data.vertices if any(g.group==mesh.vertex_groups[foot].index and g.weight>.5 for g in v.groups)) for front,label,up,lo,foot,rest in leg_data}
 minimum=1000
 for clip,frames in [('Idle',60),('Run',30),('Attack',24 if cat else 60)]:
  action=bpy.data.actions.new(clip);action.use_fake_user=True;rig.animation_data_create();rig.animation_data.action=action
  for frame in range(frames+1):
   phase=math.tau*frame/frames;t=frame/frames
   for b in rig.pose.bones:b.matrix_basis=base[b.name].copy();b.rotation_mode='QUATERNION'
   body=rig.pose.bones[spine];body.location.z+=.025*max(0,math.sin(phase*2)) if clip=='Run' else .01*math.sin(phase)
   if clip=='Attack':body.rotation_quaternion=Quaternion((1,0,0),math.radians((10 if cat else -10)*math.sin(math.pi*t)))
   rig.pose.bones[head].rotation_quaternion=Quaternion((1,0,0),math.radians(2*math.sin(phase) if clip!='Attack' else -8*math.sin(math.pi*t)))
   for tail,(points,names) in tail_data.items():
    for j,n in enumerate(names):
     b=rig.pose.bones[n];b.rotation_quaternion=Quaternion((0,1,0),math.radians((5 if cat else 2)*math.sin(phase-j*.65+len(tail)*.7)))
   bpy.context.view_layer.update()
   if clip=='Run':
    for front,label,up,lo,foot,rest in leg_data:
     p=phase+(0 if (front and label=='L') or (not front and label=='R') else math.pi)
     target=rest.copy();target.y-=(.26 if cat else .15)*math.cos(p);target.z=ground+soles[foot]+.012+(.21 if cat else .065)*max(0,math.sin(p))
     solve(rig,rig.pose.bones[foot],[rig.pose.bones[up],rig.pose.bones[lo]],target)
     end=rig.pose.bones[foot];m=end.matrix.copy();m=Matrix.Translation(m.translation)@rig.data.bones[foot].matrix_local.to_quaternion().to_matrix().to_4x4();end.matrix=m;bpy.context.view_layer.update()
   if clip=='Attack' and cat:
    for front,label,up,lo,foot,rest in leg_data:
     if not front:continue
     target=rest+P(0,.3*math.sin(math.pi*t),.55*math.sin(math.pi*t));solve(rig,rig.pose.bones[foot],[rig.pose.bones[up],rig.pose.bones[lo]],target)
   if not cat and clip!='Run':
    for label,up,lo,hand in arm_data:
     rig.pose.bones[up].rotation_quaternion=Quaternion((1,0,0),math.radians((16*math.sin(phase+(0 if label=='L' else math.pi))) if clip=='Run' else -22*math.sin(math.pi*t) if clip=='Attack' else 1.5*math.sin(phase)))
     rig.pose.bones[lo].rotation_quaternion=Quaternion((1,0,0),math.radians(-12*math.sin(math.pi*t) if clip=='Attack' else 0))
   for b in rig.pose.bones:
    b.keyframe_insert('location',frame=frame);b.keyframe_insert('rotation_quaternion',frame=frame);b.keyframe_insert('scale',frame=frame)
  # Preserve smooth loop and contact curves.
  for fc in action.fcurves:
   for key in fc.keyframe_points:key.interpolation='LINEAR'
 rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(0)
 out=ROOT/'Assets/Art/BijuuReference/Models'/f'{kind}.fbx'
 bpy.ops.export_scene.fbx(filepath=str(out),object_types={'ARMATURE','MESH'},use_selection=False,add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='AUTO')
 bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/BijuuReference'/f'{kind}.blend'))
 print(kind,'vertices',len(mesh.data.vertices),'bones',len(rig.pose.bones),'clips Idle/Run/Attack',flush=True)
for kind in ['Matatabi','Gyuki']:build(kind)
