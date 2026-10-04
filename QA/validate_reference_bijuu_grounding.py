import bpy,json
from pathlib import Path
result={}
for name in ['Matatabi','Gyuki']:
 bpy.ops.wm.open_mainfile(filepath=str(Path('ArtSource/BijuuReference',name+'.blend').resolve()))
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
 names={g.index:g.name for g in mesh.vertex_groups};body=[v.index for v in mesh.data.vertices if not names[max(v.groups,key=lambda g:g.weight).group].startswith('Tail_')]
 def minimum():
  ev=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();lowest=min(body,key=lambda i:(mesh.matrix_world@m.vertices[i].co).z);v=(mesh.matrix_world@m.vertices[lowest].co).z;
  if name=="Gyuki" and v<-.04:print("low",bpy.context.scene.frame_current, v,[(names[g.group],g.weight) for g in mesh.data.vertices[lowest].groups])
  ev.to_mesh_clear();return v
 rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(0);ground=minimum();rig.animation_data.action=bpy.data.actions['Run'];values=[]
 for f in range(31):bpy.context.scene.frame_set(f);values.append(minimum()-ground)
 result[name]={'minRunClearanceSource':min(values),'maxRunClearanceSource':max(values),'bones':len(rig.pose.bones),'vertices':len(mesh.data.vertices)}
print(json.dumps(result));Path('Logs/BijuuReferenceQA/BlenderGrounding.json').write_text(json.dumps(result,indent=2))
