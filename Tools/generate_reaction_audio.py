"""Authored procedural wind and small spectator claps, no external samples."""
import math,random,wave,struct,pathlib
root=pathlib.Path('Assets/VRBat/Audio');rate=24000
rng=random.Random(415)
def save(name,samples):
    peak=max(abs(s) for s in samples);gain=.68/max(peak,.001)
    with wave.open(str(root/(name+'.wav')),'wb') as w:
        w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(b''.join(struct.pack('<h',int(max(-.99,min(.99,s*gain))*32767)) for s in samples))
n=int(rate*.72);smooth=0;out=[]
for i in range(n):
    t=i/rate;smooth=.93*smooth+.07*rng.uniform(-1,1);out.append(smooth*math.sin(math.pi*t/.72)**2)
save('flight_air',out)
out=[0.0]*int(rate*1.6)
for j in range(27):
    at=.03+j*.043+rng.uniform(-.018,.018);amp=rng.uniform(.13,.32);tail=.035+rng.random()*.03;filtered=0
    for k in range(int(rate*tail*4)):
        idx=int(at*rate)+k
        if idx>=len(out):break
        filtered=.3*filtered+.7*rng.uniform(-1,1)
        out[idx]+=amp*filtered*math.exp(-k/rate/tail)*min(1,k/8)*min(1,(1.6-idx/rate)/.25)
save('crowd_claps',out)
