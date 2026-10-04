"""New volume meshes painted from the user's reference-parts sheets; no old blockout geometry."""
import bpy,math,random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];random.seed(9)
def P(x,y,z):return Vector((x,-z,y))
def linear(c):return tuple((x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4) for x in c)
def paint(o,color,atlas=None,uvmap=None):
 a=o.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
 for v in o.data.vertices:
  c=color
  if atlas and uvmap:
   u,w=uvmap(o.matrix_world@v.co);x=max(0,min(atlas[0]-1,int(u*atlas[0])));y=max(0,min(atlas[1]-1,int(w*atlas[1])));j=4*(y*atlas[0]+x);rgba=atlas[2][j:j+4]
   if rgba[3]>.45:c=tuple(rgba[:3])
  a.data[v.index].color=(*linear(c),1)
 for poly in o.data.polygons:poly.use_smooth=True
 return o
BLUE=(.035,.24,.95);CYAN=(.05,.76,1);DARK=(.008,.012,.045);SKIN=(.47,.23,.26);IVORY=(.83,.75,.59)
def ell(name,center,scale,c,sub=3):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=P(*center));o=bpy.context.object;o.name=name;o.scale=scale[0],scale[2],scale[1];bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return paint(o,c)
def curve(points,steps=5):
 p=[P(*v) for v in points];out=[]
 for i in range(len(p)-1):
  p0=p[max(0,i-1)];p1=p[i];p2=p[i+1];p3=p[min(len(p)-1,i+2)]
  for j in range(steps):
   t=j/steps;out.append(.5*(2*p1+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t))
 out.append(p[-1]);return out
 def_unused=0
def tube(name,points,radii,c,sides=12):
 points=[v if isinstance(v,Vector) else P(*v) for v in points];vertices=[];faces=[]
 if not isinstance(radii,list):radii=[radii]*len(points)
 for i,p in enumerate(points):
  direction=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();axis=Vector((0,0,1)) if abs(direction.z)<.9 else Vector((1,0,0));u=direction.cross(axis).normalized();v=direction.cross(u).normalized()
  for j in range(sides):vertices.append(p+radii[i]*(math.cos(j*math.tau/sides)*u+math.sin(j*math.tau/sides)*v))
 for i in range(len(points)-1):
  for j in range(sides):faces.append((i*sides+j,i*sides+(j+1)%sides,(i+1)*sides+(j+1)%sides,(i+1)*sides+j))
 faces.append(tuple(reversed(range(sides))));faces.append(tuple(range((len(points)-1)*sides,len(points)*sides)))
 m=bpy.data.meshes.new(name);m.from_pydata(vertices,[],faces);m.update();o=bpy.data.objects.new(name,m);bpy.context.collection.objects.link(o);return paint(o,c)
def merged(name,objects,voxel=.065):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 mod=o.modifiers.new('Continuous sculpt surface','REMESH');mod.mode='VOXEL';mod.voxel_size=voxel;bpy.ops.object.modifier_apply(modifier=mod.name)
 smooth=o.modifiers.new('Sculpt smoothing','SMOOTH');smooth.factor=.6;smooth.iterations=3;bpy.ops.object.modifier_apply(modifier=smooth.name)
 # Keep a practical game mesh after sculpt union.
 dec=o.modifiers.new('Game mesh','DECIMATE');dec.ratio=.55;bpy.ops.object.modifier_apply(modifier=dec.name)
 for a in list(o.data.color_attributes):o.data.color_attributes.remove(a)
 return o

def sheet(name):
 image=bpy.data.images.load(str(ROOT/'Assets/Art/BijuuReference/References'/f'{name}-Parts.png'));return image.size[0],image.size[1],list(image.pixels)
def write(kind):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.gltf(filepath=str(ROOT/'Assets/Art/BijuuReference/Source'/f'{kind}.glb'),export_format='GLB',export_animations=False,export_all_vertex_colors=True)
 print(kind,'new mesh vertices',sum(len(o.data.vertices) for o in bpy.context.scene.objects if o.type=='MESH'),flush=True)
def matatabi():
 bpy.ops.wm.read_factory_settings(use_empty=True);atlas=sheet('Matatabi')
 torso=merged('Torso_primary',[ell('core',(0,1.88,-.12),(.55,.56,1.18),BLUE),ell('chest',(0,2,.65),(.56,.63,.62),BLUE),ell('hip',(0,1.69,-.9),(.61,.58,.48),BLUE)],.065)
 paint(torso,BLUE,atlas,lambda p:(.31+max(0,min(1,(-p.y+1.3)/2.65))*.37,.38+max(0,min(1,(p.z-1.12)/1.46))*.5))
 ell('Neck',(0,2.22,.98),(.44,.48,.52),BLUE)
 head=merged('Head',[ell('skull',(0,2.58,1.37),(.47,.47,.5),BLUE),ell('cheekL',(-.35,2.34,1.6),(.23,.25,.35),BLUE),ell('cheekR',(.35,2.34,1.6),(.23,.25,.35),BLUE)],.045);paint(head,BLUE)
 ell('Mouth_cavity',(0,2.12,1.91),(.38,.42,.20),(.045,.006,.025))
 ell('Muzzle_L',(-.20,2.43,1.91),(.23,.19,.25),CYAN);ell('Muzzle_R',(.20,2.43,1.91),(.23,.19,.25),CYAN)
 ell('Chin',(0,1.84,1.89),(.31,.095,.22),BLUE);ell('Nose',(0,2.48,2.13),(.14,.085,.10),(.04,.065,.105));ell('Tongue',(0,1.91,2.01),(.15,.035,.1),(.64,.24,.38))
 for side,label in [(-1,'L'),(1,'R')]:
  ear=tube('Ear_'+label,curve([(side*.34,2.82,1.16),(side*.49,3.2,1.03),(side*.57,3.5,.93)],3),[.20,.18,.14,.11,.08,.035,.001],BLUE)
  tube('InnerEar_'+label,[(side*.37,2.97,1.34),(side*.49,3.32,1.10)],[.09,.005],DARK)
  ell('EyeMask_'+label,(side*.29,2.68,1.81),(.22,.115,.11),DARK)
  eye=ell('Eye_'+label,(side*.295,2.69,1.888),(.15,.073,.047),(.86,.88,.18) if side<0 else (.13,.94,.43),3);eye.rotation_euler.y=side*.2
  ell('Pupil_'+label,(side*.295,2.69,1.931),(.019,.061,.012),DARK)
  ell('EyeGlint_'+label,(side*.26,2.715,1.941),(.021,.012,.008),(.7,1,1),2)
  for j in range(2):
   points=curve([(side*(.20+j*.14),2.34,2.01),(side*(.20+j*.13),2.17,2.06),(side*(.16+j*.13),1.99,2.05)],3)
   tube('Head_Fang_'+label+str(j),points,[.065,.060,.055,.043,.03,.015,.001],(.93,.95,.9))
  for j in range(5):
   x=side*(.05+j*.055);tube('Head_LowerTooth_'+label+str(j),[(x,1.88,2.04),(x,2.02,2.03)],[.025,.001],(.92,.94,.9),8)
  # Flame ribbons around cheeks and skull give the silhouette in the supplied art.
  for j in range(10):
   y=2.05+(j%5)*.22;z=1.18+(j//5)*.38;x=side*(.30+.05*(j%3));points=curve([(x,y,z),(x+side*.17,y+.13,z-.15),(x+side*.27,y+.40,z-.33)],3);tube('HeadFlame_'+label+str(j),points,[.08,.085,.078,.062,.045,.023,.001],CYAN if j%3 else BLUE,7)
  for front,z,h in [(True,.77,1.71),(False,-.93,1.56)]:
   prefix='Front' if front else 'Hind';x=side*(.49 if front else .6)
   ell(prefix+'_Shoulder_'+label,(x,h,z),(.27,.42,.33),BLUE)
   pts=curve([(x,h,z),(x,h*.63,z-.14),(x,.49,z+.21)],4)
   tube(prefix+'_LowerLeg_'+label,pts,[.22-(.065*i/(len(pts)-1)) for i in range(len(pts))],BLUE)
   paw=ell(prefix+'_Paw_'+label,(x,.44 if front else .42,z+.29),(.29,.14,.34),BLUE)
   # Spiral markings and separated broad toes, following the actual paws.
   for j in range(4):
    xx=x+(j-1.5)*.14;ell(prefix+'_Paw_'+label+'_Toe'+str(j),(xx,.41 if front else .39,z+.53),(.087,.09,.17),BLUE,2)
    pts=curve([(xx,.43,z+.62),(xx,.39,z+.75),(xx,.31,z+.88)],3);tube(prefix+'_Claw_'+label+'_'+str(j),pts,[.058,.054,.048,.035,.022,.012,.001],(.035,.04,.07),9)
   for j in range(5):
    yy=.64+j*.18;tube(prefix+'_LowerLeg_'+label+'_Pattern'+str(j),[(x-.15,yy,z+.22),(x+.1,yy+.12,z+.21)],[.035,.025],DARK,7)
  for j in range(8):
   z=-1.2+j*.30;pts=curve([(side*.3,2.3,z),(side*.41,2.55,z-.12),(side*.5,2.84,z-.30)],3);tube('SpinalFlame_'+label+str(j),pts,[.13,.13,.11,.08,.05,.025,.001],CYAN,7)
 # Two thick sweeping tails; black spiral texture comes from the separated tail pieces.
 for side,label in [(-1,'L'),(1,'R')]:
  control=[(side*.34,2.1,-1.14),(side*.90,2.52,-1.85),(side*1.33,3.24,-2.5),(side*1.28,3.92,-2.65),(side*.77,4.25,-2.37),(side*.38,3.98,-2.05)]
  pts=curve(control,6);radii=[.21+.16*math.sin(math.pi*i/(len(pts)-1)) for i in range(len(pts))];radii[-1]=.025
  tail=tube('Tail_'+label,pts,radii,BLUE,16)
  # Sample the corresponding detached flame tail into the mesh's vertex colors.
  for attr in list(tail.data.color_attributes):tail.data.color_attributes.remove(attr)
  paint(tail,BLUE,atlas,lambda p:(.69+max(0,min(1,abs(p.x)/1.65))*.28,.02+max(0,min(1,(p.z-2)/2.3))*.28 if label=='R' else .60+max(0,min(1,(p.z-2)/2.3))*.33))
  for k in range(3,len(pts)-2,3):
   p=pts[k];out=p+Vector((side*.26,.12,.3));tip=out+Vector((side*.15,.12,.24));tube('TailFire_'+label+'_'+str(k),[p,out,tip],[.16,.07,.001],CYAN,7)
 write('Matatabi')
def gyuki():
 bpy.ops.wm.read_factory_settings(use_empty=True);atlas=sheet('Gyuki')
 pieces=[ell('body',(0,1.8,-.05),(1.32,1.05,.89),SKIN)]
 for side in [-1,1]:
  pieces+=[ell('pec',(side*.75,2.45,.43),(.92,.50,.65),SKIN),ell('lat',(side*1.08,2.14,-.12),(.65,.73,.59),SKIN)]
  for j in range(3):pieces.append(ell('rib',(side*.4,1.93-j*.28,.63),(.51,.23,.27),SKIN))
 torso=merged('MainBody',pieces,.07);paint(torso,SKIN,atlas,lambda p:(.42+max(0,min(1,(p.x+1.5)/3))*.35,.46+max(0,min(1,(p.z-.72)/2.4))*.51))
 ell('Neck',(0,2.5,.65),(.68,.68,.61),SKIN)
 head=merged('Head',[ell('bullskull',(0,3.0,1.0),(.63,.70,.56),SKIN),ell('jaw',(0,2.41,1.39),(.43,.4,.43),SKIN)],.042);paint(head,SKIN)
 ell('Head_Mouth',(0,2.36,1.775),(.36,.17,.07),(.13,.04,.05))
 ell('Nose',(0,2.60,1.79),(.37,.145,.12),IVORY)
 for side,label in [(-1,'L'),(1,'R')]:
  ell('Nostril_'+label,(side*.19,2.56,1.89),(.08,.034,.022),(.11,.05,.05),2)
  ell('EyeSocket_'+label,(side*.30,2.99,1.44),(.16,.11,.10),(.16,.055,.06))
  ell('Iris_'+label,(side*.30,3.005,1.526),(.083,.065,.035),(.92,.85,.67))
  ell('Pupil_'+label,(side*.30,3.005,1.558),(.022,.026,.010),(.025,.012,.014),2)
  ell('Brow_'+label,(side*.30,3.14,1.46),(.23,.11,.09),SKIN)
  points=curve([(side*.54,3.42,.82),(side*.90,3.61,.72),(side*1.28,3.56,.66),(side*1.59,3.68,.52),(side*1.80,3.98,.37)],4);tube('Horn_'+label,points,[.23*(1-i/(len(points)-1))+.005 for i in range(len(points))],(.36,.15,.18),14)
  for j in range(7):
   x=side*(.025+j*.045);ell('Head_Teeth_'+label+str(j),(x,2.37,1.84),(.026,.06,.035),(.93,.88,.74),2);ell('Head_LowerTeeth_'+label+str(j),(x,2.29,1.83),(.027,.045,.034),(.89,.82,.69),2)
  # Long weight-bearing forearms and huge fingered hands, no hoof legs.
  arm=merged('UpperArm_'+label,[ell('deltoid',(side*1.48,2.40,.34),(.70,.65,.64),SKIN),ell('biceps',(side*1.81,1.91,.72),(.60,.67,.48),SKIN)],.055);paint(arm,SKIN,atlas,lambda p:(.055+max(0,min(1,(abs(p.x)-1.0)/1.65))*.24,.06+max(0,min(1,(p.z-.2)/2.5))*.47))
  fore=merged('Forearm_'+label,[ell('longfore',(side*2.02,1.10,1.13),(.50,.68,.50),SKIN),ell('wrist',(side*2.10,.54,1.48),(.45,.41,.43),SKIN)],.05);paint(fore,SKIN,atlas,lambda p:(.055+max(0,min(1,(abs(p.x)-1.5)/1.15))*.24,.03+max(0,min(1,p.z/1.9))*.48))
  ell('Fist_'+label,(side*2.1,.30,1.66),(.59,.22,.51),SKIN)
  for j in range(4):
   xx=side*2.1+(j-1.5)*.25;pts=curve([(xx,.33,1.78),(xx,.22,2.07),(xx,.17,2.27)],3);tube('HandClaw_'+label+'_'+str(j),pts,[.145,.14,.13,.115,.10,.085,.07],SKIN,12);ell('HandClaw_'+label+'_Nail'+str(j),(xx,.18,2.28),(.075,.035,.07),(.25,.10,.12),2)
  tube('HandClaw_'+label+'_Thumb',curve([(side*1.65,.4,1.54),(side*1.47,.25,1.76),(side*1.44,.20,1.99)],3),[.17,.16,.14,.12,.10,.08,.055],SKIN,12)
 for j in range(6):
  x=(j-2.5)*.095;tube('Head_ForeheadRib'+str(j),curve([(x,3.50,1.36),(x,3.3,1.53),(x*.9,3.13,1.55)],4),[.047]*9,(.56,.29,.31),10)
 for j in range(5):
  tube('BackSpine_'+str(j),[(0,2.69,-.55+j*.28),(0,3.36,-.64+j*.28)],[.20,.001],(.50,.24,.27),12)
 for i in range(8):
  angle=math.tau*i/8;side=math.cos(angle);v=math.sin(angle);origin=Vector((side*.6,1.15+v*.3,-.68))
  raw=[origin,origin+Vector((side*.65,.05,-.55)),origin+Vector((side*1.75,.45+v*.55,-1.30)),origin+Vector((side*2.35,1.15+v*.4,-1.40)),origin+Vector((side*2.55,1.85+v*.45,-.8)),origin+Vector((side*2.2,2.25+v*.4,-.15)),origin+Vector((side*1.65,1.94+v*.4,.03))]
  points=curve([tuple(p) for p in raw],5);radii=[.44-.24*(k/(len(points)-1)) for k in range(len(points))];radii[-1]=.10
  tail=tube('Tentacle_'+str(i+1),points,radii,SKIN,16)
  for a in list(tail.data.color_attributes):tail.data.color_attributes.remove(a)
  paint(tail,SKIN,atlas,lambda p:(.73+max(0,min(1,(p.z-1)/3))*.24,.12+max(0,min(1,abs(p.x)/3.4))*.52))
  for k in range(3,len(points)-2,3):
   p=points[k];rad=radii[k];at=p+Vector((0,-rad*.85,.03));# Blender -Y is forward, cups face the viewer.
   bpy.ops.mesh.primitive_torus_add(major_segments=12,minor_segments=6,location=at,major_radius=rad*.32,minor_radius=rad*.075,rotation=(math.pi/2,0,0));cup=bpy.context.object;cup.name=f'Tentacle_{i+1}_Sucker_{k}';paint(cup,IVORY)
   # Inset center gives suction cups depth rather than painted dots.
   bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,location=at+Vector((0,.012,0)));cup=bpy.context.object;cup.name=f'Tentacle_{i+1}_SuckerCenter_{k}';cup.scale=rad*.22,.04,rad*.22;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);paint(cup,(.52,.32,.31))
 write('Gyuki')
matatabi();gyuki()
