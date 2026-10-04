from pathlib import Path
from PIL import Image
import imageio_ffmpeg
root=Path(__file__).resolve().parents[1]/'Logs/KuramaQA'
paths=sorted(p for p in (root/'Frames').glob('*.png') if p.stat().st_mtime >= (root/'Kurama-SizeComparison.png').stat().st_mtime)
assert paths,'No captured Unity frames'
writer=imageio_ffmpeg.write_frames(str(root/'Kurama-Run-and-Bomb.mp4'),(1280,900),fps=10,codec='libx264',pix_fmt_out='yuv420p',macro_block_size=2,quality=8)
writer.send(None)
gif=[]
for path in paths:
 with Image.open(path) as frame:
  frame=frame.convert('RGB');writer.send(frame.tobytes());gif.append(frame.resize((768,540)).quantize(colors=128))
writer.close()
gif[0].save(root/'Kurama-Run-and-Bomb.gif',save_all=True,append_images=gif[1:],duration=100,loop=0,optimize=False)
print('Encoded',len(paths),'Unity frames as MP4 and GIF')
