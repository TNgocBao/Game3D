from pathlib import Path
import json,shutil
root=Path.cwd();p=root/'Assets/Editor/LumiBlenderNarutoBuild.cs';s=p.read_text(encoding='utf-8-sig').replace('LumiBlenderNarutoBuild','LumiStarterNarutoBuild').replace('Assets/Art/NarutoReference/Naruto-Reference-Sculpt.fbx','Assets/Art/NarutoStarter/NarutoStarter.fbx').replace('Naruto Chibi Reference','Naruto Starter').replace('Naruto Blender Reference Skin','Naruto Starter Rigged Skin').replace('Naruto-Blender-Reference.asset','Naruto-Starter.asset').replace('/Blender-','/Starter-').replace('head.position+Vector3.up*.065f','head.position+Vector3.up*.0423f').replace('Blender reference skin','Starter rigged skin').replace('BlenderImport.txt','StarterImport.txt').replace('Contains("steel")','Contains("Metal")')
pos=s.index(' private static Color ReferenceColor');s=s[:pos]+' private static Color ReferenceColor(string name)\n {\n'
palette=json.loads((root/'Assets/Art/NarutoStarter/Palette.json').read_text())
for name,rgb in palette.items():s+='  if(name=="'+name+'")return new Color('+','.join(str(v)+'f' for v in rgb)+');\n'
s+='  return Color.gray;\n }\n}\n';(root/'Assets/Editor/LumiStarterNarutoBuild.cs').write_text(s,encoding='utf-8-sig')
p=root/'Assets/Editor/LumiNarutoBatchReview.cs';s=p.read_text(encoding='utf-8-sig');a=s.index('            if(File.Exists(');b=s.index('\n',a);s=s[:a]+'            LumiStarterNarutoBuild.Build();'+s[b:];s=s.replace('vertexCount<50000','vertexCount<1000');p.write_text(s,encoding='utf-8-sig')
qa=root/'Logs/StarterReview';(qa/'Assets/Editor').mkdir(parents=True,exist_ok=True);(qa/'Packages').mkdir(exist_ok=True);(qa/'Logs').mkdir(exist_ok=True)
shutil.copytree(root/'Assets/Scripts',qa/'Assets/Scripts',dirs_exist_ok=True)
shutil.copytree(root/'Assets/Art/NarutoStarter',qa/'Assets/Art/NarutoStarter',dirs_exist_ok=True)
shutil.copytree(root/'ProjectSettings',qa/'ProjectSettings',dirs_exist_ok=True)
for name in ['LumiStarterNarutoBuild.cs','LumiNarutoBatchReview.cs']:shutil.copy2(root/'Assets/Editor'/name,qa/'Assets/Editor'/name)
packages={}
for name in ['com.unity.ugui','com.unity.textmeshpro']:
 path=next((root/'Library/PackageCache').glob(name+'@*'));packages[name]='file:'+path.as_posix()
for name in ['ai','animation','audio','imageconversion','imgui','jsonserialize','particlesystem','physics','ui','uielements'] :packages['com.unity.modules.'+name]='1.0.0'
(qa/'Packages/manifest.json').write_text(json.dumps({'dependencies':packages},indent=2))
