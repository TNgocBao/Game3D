from pathlib import Path
p=Path(r'C:\Users\NGOC BAO\Downloads\Naruto_Unity_Starter\Naruto_Unity_Starter\Assets\NarutoModular\Models\FullCharacter_Static.obj')
name='';v=[]
def out():
 if v and (name.startswith(('Jacket','Head_','Hands','FaceDetails_00','Sandals','Pants_000'))):print(name,tuple(round(sum(x[i] for x in v)/len(v),3) for i in range(3)),len(v))
for line in p.read_text().splitlines():
 if line.startswith('o '):out();name=line[2:];v=[]
 elif line.startswith('v '):v.append(list(map(float,line.split()[1:])))
out()
