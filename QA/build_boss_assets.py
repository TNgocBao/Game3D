"""Ten original Naruto-inspired skill effect meshes authored with Blender 4.5 bpy.
No downloaded/ripped meshes. Run: uv run --python 3.11 --with bpy==4.5.3 python QA/build_boss_assets.py
"""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'ArtSource/Bosses';EXPORT=ROOT/'Assets/Art/Bosses/Models';PREVIEW=ROOT/'Logs/BossAssetsQA'
for p in [SOURCE,EXPORT,PREVIEW]:p.mkdir(parents=True,exist_ok=True)
bpy.context.preferences.filepaths.save_version=0
bpy.context.scene.render.fps=30
NAMES=['Hashirama','Gaara','Onoki','Raikage','Mei']
PALETTES=[(.60,.055,.04),(.45,.065,.065),(.37,.43,.15),(.84,.86,.79),(.035,.11,.34)]
def material(name,color,emit=False):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=.8
 if emit:n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=2
 return m
parts=[]
def part(name,pos,size,mat,bone=None,shape='sphere'):
 if shape=='cube':bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
 elif shape=='cone':bpy.ops.mesh.primitive_cone_add(vertices=10,radius1=.5,radius2=0,depth=1,location=pos)
 elif shape=='cylinder':bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=.5,depth=1,location=pos)
 else:bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.5,location=pos)
 o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
 for poly in o.data.polygons:poly.use_smooth=shape=='sphere'
 if bone:g=o.vertex_groups.new(name=bone);g.add(list(range(len(o.data.vertices))),1,'REPLACE');parts.append(o)
 return o
def bar(name,a,b,width,mat,bone=None):
 a,b=Vector(a),Vector(b);o=part(name,(a+b)*.5,(width,width,(b-a).length),mat,bone,'cylinder');o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def export_selected(path,objects,animations=False):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=animations,bake_anim_use_all_actions=animations,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,mesh_smooth_type='FACE')
# Ten authored static FX mesh templates. Runtime animation controls their growth/travel.
for index in range(5):
 for second in [False,True]:
  bpy.ops.wm.read_factory_settings(use_empty=True);objects=[];wood=material('Wood',(.24,.12,.045));sand=material('Sand',(.69,.44,.19));energy=material('Energy',[(.2,.8,.3),(.75,.5,.22),(.64,.89,1),(.1,.55,1),(1,.16,.01)][index],True)
  if index==0:
   if second:
    objects.append(part('WoodGolemTorso',(0,0,1.5),(1.4,.8,2),wood));objects.append(part('WoodGolemHead',(0,-.05,2.8),(.65,.65,.8),wood))
    for sign in [-1,1]:objects.append(bar('GolemArm',(sign*.8,0,2.1),(sign*1.5,-.2,.9),.6,wood));objects.append(bar('GolemLeg',(sign*.4,0,.8),(sign*.45,0,.1),.5,wood))
   else:
    for j in range(12):a=j*.28;objects.append(part('WoodDragonSegment',(math.sin(a)*.8,math.cos(a)*.8,.2+j*.23),(.5,.5,.5),wood))
    objects.append(part('DragonHead',(-.04,-.8,3.05),(.65,.95,.5),wood));objects.append(part('Snout',(-.04,-1.25,3.02),(.4,.6,.3),wood))
    for sign in [-1,1]:objects.append(bar('DragonHorn',(sign*.22,-.8,3.2),(sign*.32,-.6,3.65),.11,wood))
  elif index==1:
   for j in range(16):a=j*math.tau/16;objects.append(part('SandCrest',(math.sin(a)*1.5,math.cos(a)*1.5,.3+(j%3)*.35),(1.0,.7,1.1 if not second else 2.0),sand))
  elif index==2:
   width=2.0 if not second else 3.4
   for axis in range(3):
    for a in [-1,1]:
     for b in [-1,1]:
      x=Vector((a,b,-1))*width*.5;y=Vector((a,b,1))*width*.5
      if axis==1:x=Vector((x.z,x.x,x.y));y=Vector((y.z,y.x,y.y))
      if axis==2:x=Vector((x.y,x.z,x.x));y=Vector((y.y,y.z,y.x))
      objects.append(bar('DustCubeEdge',x+Vector((0,0,1.5)),y+Vector((0,0,1.5)),.055,energy))
  elif index==3:
   for j in range(8):a=j*math.tau/8;last=Vector((math.cos(a)*.55,math.sin(a)*.55,.1))
   # segmented jagged lightning, no external textures
   for j in range(8):
    a=j*math.tau/8
    for k in range(5):
     a1=Vector((math.cos(a)*(.5+(.10 if k%2 else 0)),math.sin(a)*.5,.1+k*.4));b=a1+Vector(((-1)**k*.12,(-1)**(k+1)*.08,.4));objects.append(bar('LightningBolt',a1,b,.035 if not second else .055,energy))
  else:
   if not second:
    for j in range(12):a=j*math.tau/12;objects.append(part('MoltenRock',(math.sin(a)*1.1,math.cos(a)*1.1,.12),(.8,.8,.25),energy))
   else:
    for j in range(10):a=j*2.4;objects.append(part('MistMesh',(math.sin(a)*1.4,math.cos(a)*1.4,.5+j*.10),(1.8,1.8,1),energy))
  key=NAMES[index]+('_Skill2' if second else '_Skill1');export_selected(EXPORT/(key+'.fbx'),objects);bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(key+'.blend')))
manifest={'author':'Original project-authored stylized interpretations','tool':'Blender 4.5.3 bpy','models':[],'skillMeshes':10,'clips':[],'thirdPartyAssets':[]}
(EXPORT.parent/'Manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('ALL BOSS ASSETS BUILT',flush=True)
