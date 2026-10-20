#!/usr/bin/env python3
"""Generate the original, loopable stadium rhythm used in Bat Carnival."""
from pathlib import Path
import wave
import numpy as np

RATE = 44100
BPM = 128
BEAT = 60 / BPM
BARS = 16
DURATION = BARS * 4 * BEAT
COUNT = round(DURATION * RATE)
left = np.zeros(COUNT, dtype=np.float32)
right = np.zeros(COUNT, dtype=np.float32)
rng = np.random.default_rng(20260928)


def add(start, signal, gain=1.0, pan=0.0):
    index = round(start * RATE)
    if index >= COUNT:
        return
    end = min(COUNT, index + len(signal))
    signal = signal[:end-index] * gain
    left[index:end] += signal * np.sqrt((1-pan)/2)
    right[index:end] += signal * np.sqrt((1+pan)/2)


def tone(freq, length, attack=.004, decay=6.0, shape='sine'):
    t = np.arange(round(length*RATE), dtype=np.float32) / RATE
    phase = 2*np.pi*freq*t
    if shape == 'bass':
        x = np.sin(phase) + .25*np.sin(2*phase) + .08*np.sin(3*phase)
    elif shape == 'bell':
        x = np.sin(phase)+.35*np.sin(2.01*phase)+.18*np.sin(3.98*phase)
    else:
        x = np.sin(phase)
    env = np.minimum(1, t/max(attack, .0001))*np.exp(-t*decay)
    return (x*env).astype(np.float32)


# A restrained, minor-key pulse leaves the hit effects dominant.
roots = [82.4069, 65.4064, 98.0, 73.4162]  # E2, C2, G2, D2
chords = [[329.63,392,493.88,587.33], [261.63,329.63,392,493.88],
          [392,493.88,587.33,783.99], [293.66,369.99,440,587.33]]
for bar in range(BARS):
    root = roots[(bar//4)%4]
    notes = chords[(bar//4)%4]
    start = bar*4*BEAT
    for beat in range(4):
        at = start+beat*BEAT
        if beat % 2 == 0:
            t = np.arange(round(.36*RATE), dtype=np.float32)/RATE
            phase = 2*np.pi*(92*t+(-45/.36)*t*t/2)
            kick = np.sin(phase)*np.exp(-t*17) + rng.standard_normal(len(t))*.13*np.exp(-t*70)
            add(at, kick.astype(np.float32), .62)
        else:
            t = np.arange(round(.20*RATE), dtype=np.float32)/RATE
            noise = rng.standard_normal(len(t)).astype(np.float32)
            snare = noise*np.exp(-t*24)*.65 + np.sin(2*np.pi*185*t)*np.exp(-t*20)*.4
            add(at, snare.astype(np.float32), .22)
        add(at, tone(root,.34,decay=8,shape='bass'), .26)
        if beat in (0,2):
            add(at+BEAT*.5, tone(root*2,.21,decay=12,shape='bass'), .10, -.1 if beat==0 else .1)
    for eighth in range(8):
        at=start+eighth*BEAT/2
        t=np.arange(round(.07*RATE),dtype=np.float32)/RATE
        n=rng.standard_normal(len(t)).astype(np.float32)
        high=(n-np.roll(n,1))*np.exp(-t*49)
        add(at, high, .065 if eighth%2==0 else .042, -.32 if eighth%2==0 else .32)
    for sixteenth in range(16):
        at=start+sixteenth*BEAT/4
        note=notes[(sixteenth+(bar%4))%4]
        if sixteenth%4 in (0,2) or (bar%4==3 and sixteenth%4==3):
            add(at, tone(note,.28,decay=12,shape='bell'), .075, -.38 if sixteenth%2 else .38)
    if bar%4==3:
        # Short ascending cue at the phrase boundary.
        for j in range(4):
            add(start+(3+j/4)*BEAT,tone(notes[j]*2,.22,decay=15,shape='bell'),.085,(j-1.5)*.2)

# Gentle limiter; keep substantial headroom for effects.
peak=max(np.max(np.abs(left)), np.max(np.abs(right)))
scale=.60/max(peak,1e-9)
stereo=np.stack([left*scale,right*scale],axis=1)
stereo=np.clip(stereo,-.95,.95)
out=Path(__file__).resolve().parents[1]/'Assets/VRBat/Audio/stadium_pulse.wav'
out.parent.mkdir(parents=True,exist_ok=True)
with wave.open(str(out),'wb') as f:
    f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE)
    f.writeframes((stereo*32767).astype('<i2').tobytes())
print(f'{out}: {DURATION:.3f}s, peak={float(np.max(np.abs(stereo))):.3f}')
