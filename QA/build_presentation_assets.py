"""Build original looping game music and Vietnamese-capable rounded fonts."""
from pathlib import Path
import urllib.request, wave, numpy as np
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

root = Path(__file__).resolve().parents[1]
fonts = root / 'Assets/Resources/LumiFonts'
fonts.mkdir(parents=True, exist_ok=True)
source_dir = root / 'QA/FontSources'
source_dir.mkdir(parents=True, exist_ok=True)
source = source_dir / 'Inter-Variable.ttf'
urllib.request.urlretrieve('https://raw.githubusercontent.com/google/fonts/main/ofl/inter/Inter%5Bopsz,wght%5D.ttf', source)
urllib.request.urlretrieve('https://raw.githubusercontent.com/google/fonts/main/ofl/inter/OFL.txt', fonts / 'Inter-OFL.txt')
for weight, name in [(500, 'Inter-Medium'), (600, 'Inter-SemiBold'), (700, 'Inter-Bold')]:
    font = instantiateVariableFont(TTFont(source), {'opsz': 24, 'wght': weight}, inplace=False)
    font.save(fonts / (name + '.ttf'))
source.unlink()

music = root / 'Assets/Resources/LumiMusic'
music.mkdir(parents=True, exist_ok=True)
rate = 22050
for style, name in enumerate(['Menu', 'Forest', 'Desert', 'Ice', 'Dungeon', 'Facility']):
    bpm = [100, 108, 112, 92, 96, 120][style]
    beat = 60 / bpm
    count = int(round(32 * beat * rate))
    out = np.zeros((count, 2), dtype=np.float64)
    rng = np.random.default_rng(73 + style)
    root_note = [60, 62, 57, 65, 53, 60][style]
    chords = [0, 5, 9, 7] if style != 4 else [0, 5, 8, 7]
    motif = [0, 4, 7, 12, 11, 7, 4, 2]
    def add(note, start, length, amp, instrument, pan=0):
        n = int(length * rate)
        t = np.arange(n) / rate
        freq = 440 * 2 ** ((note - 69) / 12)
        attack = np.minimum(t / .018, 1)
        release = np.minimum((length - t) / .09, 1)
        if instrument == 'pluck':
            signal = (np.sin(2*np.pi*freq*t) + .25*np.sin(4*np.pi*freq*t)) * np.exp(-t*5/length)
        else:
            signal = np.sin(2*np.pi*freq*t) + .14*np.sin(4*np.pi*freq*t)
        signal *= attack * release * amp
        indices = (int(start * rate) + np.arange(n)) % count
        np.add.at(out[:, 0], indices, signal * (1-pan*.3))
        np.add.at(out[:, 1], indices, signal * (1+pan*.3))
    for bar in range(8):
        chord = root_note + chords[(bar//2)%4]
        for j, offset in enumerate([0, 3 if style==4 else 4, 7]):
            add(chord+offset, bar*4*beat, 4*beat, .065, 'pad', j-1)
        for step in range(8):
            add(chord+12+motif[(step+bar)%8], (bar*4+step*.5)*beat, .48*beat, .19, 'pluck', (-1)**step)
        for step in range(4):
            add(chord-24, (bar*4+step)*beat, .85*beat, .16, 'pluck')
            n = int(.06*rate)
            t = np.arange(n)/rate
            noise = rng.normal(0, .035, n)*np.exp(-t*65)
            indices = (int((bar*4+step+.5)*beat*rate)+np.arange(n))%count
            out[indices] += noise[:, None]
    out *= .78 / max(np.max(np.abs(out)), .78)
    with wave.open(str(music/(name+'.wav')), 'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(rate)
        f.writeframes((out*32767).astype('<i2').tobytes())
    print(name, 'seconds=', round(count/rate, 2), 'rms=', round(float(np.sqrt(np.mean(out*out))), 3))
