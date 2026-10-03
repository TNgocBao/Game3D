import bpy, math, json
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
parts=[]
def linear(c):return tuple(((x+.055)/1.055)**2.4 if x>.04045 else x/12.92 for x in c)
def material(name,color,rough=.55,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*linear(color),1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=m.diffuse_color;p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 if name=='Skin':p.inputs['Subsurface Weight'].default_value=.045;p.inputs['Subsurface Radius'].default_value=(.02,.012,.008)
 return m
skin=material('Skin',(.9,.73,.62));hair=material('Soft blond hair',(.88,.76,.49),.48);orange=material('Orange woven jacket',(.88,.48,.24),.7);navy=material('Navy cloth',(.14,.20,.29),.7);steel=material('Brushed forehead steel',(.58,.61,.64),.36,.65);dark=material('Soft dark ink',(.15,.13,.12));white=material('Eye sclera',(.97,.97,.93),.28);blue=material('Blue iris',(.16,.46,.61),.26);pupil=material('Pupil',(.045,.085,.12),.25);beige=material('Canvas pouches',(.59,.51,.4),.8);red=material('Back emblem',(.64,.22,.20),.6)
def finish(obj,mat,bone,sub=0):
 obj.data.materials.clear();obj.data.materials.append(mat);obj['rig_bone']=bone
 for p in obj.data.polygons:p.use_smooth=True
 if sub:
  mod=obj.modifiers.new('Sculpt smoothing','SUBSURF');mod.levels=sub;bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=mod.name)
 parts.append(obj);return obj
def oval(name,pos,size,mat,bone,segments=40,rings=24):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,location=pos);o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,mat,bone)
def rounded_box(name,pos,size,mat,bone,bevel=.05):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 mod=o.modifiers.new('Rounded sewn corners','BEVEL');mod.width=bevel;mod.segments=5;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 for poly in o.data.polygons:poly.use_smooth=True
 mod=o.modifiers.new('Weighted smooth normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=mod.name)
 return finish(o,mat,bone)
def tube(name,points,radii,mat,bone,sides=16,sub=1):
 points=[Vector(p) for p in points];v=[];faces=[];old=Vector((1,0,0))
 for j,p in enumerate(points):
  tangent=(points[min(j+1,len(points)-1)]-points[max(j-1,0)]).normalized();side=tangent.cross(Vector((0,1,0)))
  if side.length<.01:side=tangent.cross(Vector((1,0,0)))
  side.normalize()
  if side.dot(old)<0:side=-side
  old=side;up=tangent.cross(side).normalized();r=radii[j] if isinstance(radii,list) else radii
  for i in range(sides):
   a=i*math.tau/sides;v.append(tuple(p+side*math.cos(a)*r+up*math.sin(a)*r))
 for j in range(len(points)-1):
  for i in range(sides):a=j*sides+i;b=j*sides+(i+1)%sides;faces.append((a,b,b+sides,a+sides))
 faces.append(tuple(range(sides-1,-1,-1)));faces.append(tuple((len(points)-1)*sides+i for i in range(sides)))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return finish(o,mat,bone,sub)
def curve(name,points,radius,mat,bone):
 data=bpy.data.curves.new(name,'CURVE');data.dimensions='3D';data.bevel_depth=radius;data.bevel_resolution=3;data.use_fill_caps=True
 spline=data.splines.new('POLY');spline.points.add(len(points)-1)
 for v,p in zip(spline.points,points):v.co=(*p,1)
 obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj;bpy.ops.object.convert(target='MESH');return finish(obj,mat,bone)

def strand(name,a,b,c,r):
 points=[];width=[]
 for i in range(20):
  t=i/19;points.append(tuple((1-t)**2*a[j]+2*(1-t)*t*b[j]+t*t*c[j] for j in range(3)));width.append(r*max(.12,math.sin((.20+t*.76)*math.pi)))
 return tube(name,points,width,hair,'Head pivot',16,1)
# Calibrated front silhouette: the face has a broad forehead and a short soft jaw, not a ball.
head_z=1.34
profiles=[(-.33,.04,.05,.05),(-.30,.17,.19,.20),(-.24,.29,.27,.30),(-.12,.395,.315,.34),(0,.455,.33,.35),(.12,.47,.325,.35),(.25,.465,.31,.33),(.33,.40,.27,.28),(.365,.10,.06,.06)]
def face_y(x,z):
 h=z-head_z
 for j in range(len(profiles)-1):
  a,b=profiles[j:j+2]
  if a[0]<=h<=b[0]:t=(h-a[0])/(b[0]-a[0]);w=a[1]*(1-t)+b[1]*t;d=a[2]*(1-t)+b[2]*t;return -d*max(.001,1-(x/w)**2)**.175
 return -.2
v=[];f=[];sides=96
for h,w,front,back in profiles:
 for i in range(sides):
  a=i*math.tau/sides;sy=math.sin(a);v.append((w*math.cos(a),-front*abs(sy)**.35 if sy<0 else back*sy,head_z+h))
for j in range(len(profiles)-1):
 for i in range(sides):a=j*sides+i;b=j*sides+(i+1)%sides;f.append((a,b,b+sides,a+sides))
f.extend([tuple(range(sides-1,-1,-1)),tuple((len(profiles)-1)*sides+i for i in range(sides))])
mesh=bpy.data.meshes.new('Reference facial topology');mesh.from_pydata(v,[],f);mesh.update();head=bpy.data.objects.new('Sculpted short face',mesh);bpy.context.collection.objects.link(head);finish(head,skin,'Head pivot',2)
# Fuse the nose and ears into a single continuous skin surface, then soften the joins.
nose=oval('Nose sculpt',(0,face_y(0,1.29)-.010,1.29),(.044,.035,.030),skin,'Head pivot')
ears=[]
for side in [-1,1]:ears.append(oval('Ear sculpt',(side*.463,0,1.35),(.070,.065,.106),skin,'Head pivot'))
bpy.ops.object.select_all(action='DESELECT')
for o in [head,nose]+ears:o.select_set(True)
bpy.context.view_layer.objects.active=head;bpy.ops.object.join();head.data.remesh_voxel_size=.005;bpy.ops.object.voxel_remesh()
mod=head.modifiers.new('Relax sculpted joins','SMOOTH');mod.factor=.55;mod.iterations=25;bpy.ops.object.modifier_apply(modifier=mod.name)
parts=[o for o in bpy.context.scene.objects if o.type=='MESH' and 'rig_bone' in o]
# Small recessed eyes: sculpt sockets before inserting the lenses.
for side in [-1,1]:
 x=side*.19;z=1.405;y=face_y(x,z)
 cutter=oval('Eye socket cutter',(x,y-.006,z),(.102,.047,.11),skin,'Head pivot');parts.remove(cutter)
 bpy.context.view_layer.objects.active=head;mod=head.modifiers.new('Inset eye socket','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter;bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True)
 oval('Inset eye white',(x,y+.007,z),(.087,.034,.095),white,'Head pivot')
 oval('Blue iris',(x-side*.003,y-.026,z),(.057,.007,.068),blue,'Head pivot')
 oval('Dark pupil',(x-side*.003,y-.032,z+.003),(.031,.005,.046),pupil,'Head pivot')
 oval('Eye highlight',(x-.023,y-.038,z+.031),(.014,.003,.017),white,'Head pivot')
 # A thin upper lid, with flesh at its ends; no thick black circular glasses.
 lid=[]
 for j in range(15):a=math.pi*j/14;px=x+math.cos(a)*.091;pz=z+math.sin(a)*.099;lid.append((px,face_y(px,pz)-.003,pz))
 curve('Soft upper eyelid',lid,.0035,dark,'Head pivot')
 brow=[]
 for j in range(9):t=j/8;px=x-.075+t*.15;pz=1.54+.016*math.sin(t*math.pi);brow.append((px,face_y(px,pz)-.006,pz))
 curve('Blond brow',brow,.010,hair,'Head pivot')
 for line in range(3):
  pts=[]
  for j in range(8):t=j/7;px=side*(.27+t*(.11-.02*line));pz=1.27-line*.05+(line-1)*.014*t;pts.append((px,face_y(px,pz)-.003,pz))
  curve('Fine cheek whisker',pts,.0023,dark,'Head pivot')
smile=[]
for j in range(20):t=j/19;x=(t-.5)*.18;z=1.155+.017*(abs(t-.5)*2)**2;smile.append((x,face_y(x,z)-.003,z))
curve('Gentle recessed smile',smile,.0028,dark,'Head pivot')
# Cloth band, bevelled steel plate, rivets and correct Leaf mark.
bandverts=[];bandfaces=[]
for z in [1.565,1.70]:
 for i in range(96):a=i*math.tau/96;bandverts.append((.475*math.sin(a),(-.35*abs(math.cos(a))**.35 if math.cos(a)<0 else .346*math.cos(a)),z))
for i in range(96):bandfaces.append((i,(i+1)%96,(i+1)%96+96,i+96))
data=bpy.data.meshes.new('Cloth band surface');data.from_pydata(bandverts,[],bandfaces);data.update();obj=bpy.data.objects.new('Fitted cloth forehead band',data);bpy.context.collection.objects.link(obj);finish(obj,navy,'Head pivot')

plate=rounded_box('Forehead protector',(0,-.358,1.63),(.47,.025,.138),steel,'Head pivot',.027)
for side in [-1,1]:
 for j in range(3):oval('Steel rivet',(side*.202,-.376,1.59+j*.04),(.007,.004,.007),steel,'Head pivot',16,12)
pts=[]
for i in range(40):t=i/39;a=t*math.pi*3;r=.004+t*.038;pts.append((math.cos(a)*r,-.378,1.633+math.sin(a)*r))
curve('Leaf spiral',pts,.0033,dark,'Head pivot');curve('Leaf tip',[(-.030,-.381,1.625),(-.050,-.381,1.603),(-.028,-.381,1.603),(-.005,-.381,1.603)],.0033,dark,'Head pivot')
# Rounded hair clumps: vary their direction and volume; no uniform helmet or conical crown.
oval('Hair underlayer',(0,.045,1.72),(.47,.34,.24),hair,'Head pivot')
for i in range(7):
 x=(i-3)*.125;strand('Front soft hair lock',(x*.48,-.04,1.85),(x*.94+.025*math.sin(i*2),-.30,1.97+.02*math.cos(i)),(x+.015*math.sin(i), -.37,1.72-abs(x)*.15+.025*math.sin(i*3)),.088+.012*math.sin(i))
for i in range(11):
 a=i*math.tau/11;dx,dy=math.cos(a),math.sin(a);strand('Crown sculpted lock',(dx*.17,dy*.14,1.82),(dx*.39,dy*.30,2.02),(dx*.44,dy*.34,1.97+.08*math.cos(a*2)),.105)
for row in range(2):
 for i in range(9):
  a=(i-4)*.31;dx,dy=math.sin(a),math.cos(a);z=1.77-row*.20;strand('Layered back hair',(dx*.26,dy*.22,z),(dx*.45,dy*.35,z+.035),(dx*.43,dy*.31,z-.18),.085)
for side in [-1,1]:strand('Sideburn',(side*.4,-.05,1.58),(side*.465,-.08,1.45),(side*.43,-.08,1.35),.046)
oval('Band knot',(0,.356,1.59),(.065,.035,.047),navy,'Head pivot')
for side in [-1,1]:curve('Cloth headband tie',[(side*.025,.364,1.57),(side*.055,.382,1.45),(side*.067,.37,1.34)],.023,navy,'Head pivot')
# Jacket cut from a softened box and fitted sleeves; fabric is not inflated into balls.
rounded_box('Tailored jacket',(0,0,.79),(.58,.36,.43),orange,'Body pivot',.07)
rounded_box('Navy shoulder yoke',(0,.006,.965),(.57,.34,.11),navy,'Body pivot',.045)
rounded_box('Waistband',(0,0,.568),(.565,.355,.06),navy,'Body pivot',.025)
curve('High collar',[(.173*math.sin(i*math.tau/64),.144*math.cos(i*math.tau/64),1.045) for i in range(65)],.043,navy,'Body pivot')
for i in range(25):
 a=i*math.tau/25;curve('Collar rib',[(.176*math.sin(a),.148*math.cos(a),1.013),(.176*math.sin(a),.148*math.cos(a),1.075)],.003,navy,'Body pivot')
curve('Zipper',[(0,-.195,.595),(0,-.195,.75),(0,-.188,.94),(0,-.15,1.062)],.006,steel,'Body pivot')
for i in range(28):rounded_box('Zipper teeth',((-.005 if i%2 else .005),-.202,.601+i*.015),(.013,.009,.005),steel,'Body pivot',.002)
rounded_box('Zipper pull',(0,-.201,.93),(.022,.013,.035),steel,'Body pivot',.006)
rounded_box('Back canvas pouch',(0,.214,.90),(.23,.115,.23),beige,'Body pivot',.08)
oval('Red spiral emblem',(0,.278,.985),(.08,.012,.08),red,'Body pivot')
pts=[]
for i in range(35):t=i/34;a=t*math.pi*3;pts.append((math.cos(a)*t*.047,.294,.985+math.sin(a)*t*.047))
curve('Back spiral line',pts,.0026,dark,'Body pivot');oval('Waist pouch',(0,.195,.565),(.065,.042,.065),beige,'Body pivot')
for side in [-1,1]:
 arm='Left arm' if side<0 else 'Right arm';elbow='Left elbow' if side<0 else 'Right elbow';leg='Left leg' if side<0 else 'Right leg'
 sleeve_points=[];sleeve_widths=[]
 for j in range(18):
  t=j/17;sleeve_points.append((side*(.26+.18*t),-.008*t,.965-.24*t));sleeve_widths.append(.105-.025*t)
 sleeve=tube('Continuous tailored sleeve',sleeve_points,sleeve_widths,orange,arm,24,1)
 curve('Sleeve hem',[(side*.438+.079*math.sin(i*math.tau/40),-.008+.078*math.cos(i*math.tau/40),.728) for i in range(41)],.004,orange,arm)
 rounded_box('Shoulder badge',(side*.30,-.10,.93),(.09,.018,.077),steel,arm,.014)
 # Joined hand sculpture with three rounded fingers and an anatomically placed thumb.
 palm=oval('Palm',(side*.435,-.008,.681),(.053,.039,.061),skin,elbow);handparts=[palm]
 for finger in range(3):handparts.append(oval('Finger',(side*.435+(finger-1)*.029,-.019,.632),(.016,.025,.032),skin,elbow,24,16))
 handparts.append(oval('Thumb',(side*.399,-.033,.676),(.025,.028,.040),skin,elbow,24,16))
 bpy.ops.object.select_all(action='DESELECT')
 for obj in handparts:obj.select_set(True)
 bpy.context.view_layer.objects.active=palm;bpy.ops.object.join();palm.data.remesh_voxel_size=.003;bpy.ops.object.voxel_remesh();mod=palm.modifiers.new('Soft finger joins','SMOOTH');mod.factor=.5;mod.iterations=3;bpy.ops.object.modifier_apply(modifier=mod.name)
 parts=[o for o in bpy.context.scene.objects if o.type=='MESH' and 'rig_bone' in o]
 rounded_box('Ninja trousers',(side*.145,0,.425),(.245,.285,.30),navy,leg,.055)
 rounded_box('Trouser hem',(side*.145,0,.288),(.244,.278,.040),navy,leg,.015)
 rounded_box('Sandal ankle',(side*.145,.009,.195),(.173,.205,.16),navy,leg,.035)
 rounded_box('Sandal sole',(side*.145,-.04,.045),(.224,.31,.035),navy,leg,.022)
 rounded_box('Sandal upper',(side*.145,.004,.099),(.21,.229,.094),navy,leg,.032)
 for toe in range(4):oval('Visible toes',(side*.145+(toe-1.5)*.037,-.163,.08),(.021,.027,.023),skin,leg,24,16)
 if side==1:
  for z in [.39,.445]:rounded_box('Cream leg wrap',(side*.145,-.008,z),(.251,.292,.027),beige,leg,.012)

rounded_box('Trouser seat',(0,0,.56),(.52,.29,.15),navy,'Body pivot',.045)
for part in parts:
 if part['rig_bone']=='Head pivot':part.location.z+=.08
# Rest-pose skeleton and rigid skin groups preserve every modeled detail during motion.
bones={'Rig root':((0,0,0),None),'Body pivot':((0,0,.82),'Rig root'),'Head pivot':((0,0,1.42),'Body pivot')}
for side,label in [(-1,'Left'),(1,'Right')]:
 bones[label+' arm']=((side*.265,0,.965),'Body pivot')
 bones[label+' elbow']=((side*.37,0,.82),label+' arm')
 bones[label+' hand']=((side*.435,-.008,.681),label+' elbow')
 bones[label+' leg']=((side*.145,0,.58),'Rig root')
bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='Naruto reference skeleton';bpy.ops.object.mode_set(mode='EDIT');rig.data.edit_bones.remove(rig.data.edit_bones[0])
for name,(pos,parent) in bones.items():
 b=rig.data.edit_bones.new(name);b.head=pos;b.tail=Vector(pos)+Vector((0,0,.1))
 if parent:b.parent=rig.data.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')
for obj in parts:
 group=obj.vertex_groups.new(name=obj['rig_bone']);group.add(list(range(len(obj.data.vertices))),1,'REPLACE')
bpy.ops.object.select_all(action='DESELECT')
for obj in parts:obj.select_set(True)
bpy.context.view_layer.objects.active=head;bpy.ops.object.join();model=head;model.name='Naruto reference sculpt'
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
mod=model.modifiers.new('Reference animation skin','ARMATURE');mod.object=rig;model.parent=rig
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);model.select_set(True)
try:
 bpy.ops.export_scene.fbx(filepath=str(OUT/'Naruto-Reference-Sculpt.fbx'),use_selection=True,add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
except Exception as e:print('FBX export issue',str(e),flush=True)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.resolution_x=768;scene.render.resolution_y=960;scene.render.resolution_percentage=100
scene.world.color=(.17,.17,.17);scene.view_settings.view_transform='AgX'
groundmat=material('Warm studio ground',(.42,.40,.37),.9)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,.023));ground=bpy.context.object;ground.name='Studio floor';ground.data.materials.append(groundmat)
def aim(obj,target):obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
for name,pos,power,size in [('Large soft key',(-3,-4,5),450,4),('Face fill',(3,-2,2.5),180,3),('Hair rim',(1,3,4),350,3)]:
 bpy.ops.object.light_add(type='AREA',location=pos);light=bpy.context.object;light.name=name;light.data.energy=power;light.data.shape='DISK';light.data.size=size;aim(light,(0,0,1))
bpy.ops.object.camera_add(location=(0,-4,1.1));camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=2.3;scene.camera=camera;aim(camera,(0,0,1.04))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Naruto-Reference-Sculpt.blend'))
print('MODEL READY vertices',len(model.data.vertices),'polygons',len(model.data.polygons),flush=True)
for name,pos in [('Front',(0,-4,1.1)),('Side',(4,0,1.1)),('Back',(0,4,1.1)),('ThreeQuarter',(2.7,-4,1.6))]:
 camera.location=pos;aim(camera,(0,0,1.04));scene.render.filepath=str(OUT/('Naruto-Blender-'+name+'.png'));bpy.ops.render.render(write_still=True);print('RENDER READY',name,flush=True)
