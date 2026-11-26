"""Rebuild the shopkeeper's native cels, Unity strips and original wordless hum.

Run from any directory with Pillow and LibreSprite. The approved shopkeeper is
the drawing source. All positions below are expressed on his 63 x 62 canvas.
"""
import argparse
import math
import struct
import subprocess
import wave
import zlib
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
RES = ROOT / 'Assets/_Game/Resources/UI'
OUT = RES / 'MerchantMotion'
EDITABLE = ROOT / 'ArtSource/MouseMerchant/Native'
STILL = Image.open(RES / 'MerchantStall/MouseHappy.png').convert('RGBA')
INK, CREAM, SHADE, PINK = '#1b0f15', '#fae8c8', '#ceb297', '#f4a690'
W, H = STILL.size


def hum(frame):
    phase = 2 * math.pi * frame / 16
    angle = math.radians(3.4) * math.sin(phase)
    cel = Image.new('RGBA', STILL.size)
    # Rotate about the scarf. Taper the displacement into the shoulder seam.
    # The backpack, resting paws and tail keep their original registration.
    for y in range(H):
        for x in range(W):
            weight = min(1, max(0, (50 - y) / 8))
            if y >= 39 and x < 23:
                weight = 0
            a = angle * weight
            dx, dy = x - 36, y - 45
            sx = round(36 + dx * math.cos(a) + dy * math.sin(a))
            sy = round(45 - dx * math.sin(a) + dy * math.cos(a))
            if 0 <= sx < W and 0 <= sy < H:
                cel.putpixel((x, y), STILL.getpixel((sx, sy)))
    return cel


def speaking(frame):
    cel = STILL.copy()
    draw = ImageDraw.Draw(cel)
    opening = (0, 1, 2, 1, 0, 2)[frame]
    if opening:
        draw.polygon([(42,42),(45,43),(48,41),(48,42+opening),(46,42+opening),(44,42+opening)], fill=INK)
        if opening == 2:
            draw.line((45,44,46,44), fill='#cd726d')
    return cel


def raised_paw(lift, tilt):
    if lift == 0:
        return STILL.copy()
    cel = STILL.copy()
    draw = ImageDraw.Draw(cel)
    # Remove just the resting right paw and close the coat at its former edge.
    draw.rectangle((48,53,56,59), fill=(0,0,0,0))
    draw.polygon([(47,53),(51,52),(52,54),(50,56),(47,57)], fill=INK)
    draw.polygon([(48,53),(50,53),(50,54),(48,55)], fill=SHADE)
    cx, cy = 51 + round(lift * .43) + tilt, 55 - lift
    # Bent forearm joins the shoulder beneath a mitten-shaped open mouse paw.
    draw.line([(49,54),(53,53),(cx,cy+2)], fill=INK, width=5)
    draw.line([(49,53),(53,52),(cx,cy+2)], fill=SHADE, width=3)
    palm = [(-4,-1),(-3,-3),(-2,-3),(-2,-4),(0,-4),(1,-3),(2,-4),(3,-3),(4,-1),(4,1),(2,4),(-1,4),(-4,2)]
    draw.polygon([(cx+x,cy+y) for x,y in palm], fill=INK)
    draw.polygon([(cx+x,cy+y) for x,y in [(-3,-1),(-2,-2),(-1,-3),(1,-2),(2,-3),(3,-1),(3,1),(1,3),(-1,3),(-3,1)]], fill=CREAM)
    draw.line((cx-1,cy+1,cx+1,cy+1),fill=PINK)
    draw.point((cx,cy),fill=PINK)
    return cel


def goodbye(frame):
    gesture = [(1,0),(4,0),(8,-1),(11,-1),(12,0),(12,1),(12,0),(11,-1),(8,0),(3,0)]
    lift, tilt = gesture[frame] if frame < len(gesture) else (0,0)
    drop = (0,0,0,0,0,0,0,0,0,1,4,12,25,43,61,62,62,62,62,62)[frame]
    cel = Image.new('RGBA', STILL.size)
    if drop < 60:
        pose = raised_paw(lift,tilt)
        # The shoulders tuck before the feet leave the ledge, then the head follows them down.
        tuck = {9:1, 10:3, 11:4, 12:3, 13:2}.get(frame, 0)
        if tuck:
            pose = pose.resize((W, H-tuck), Image.Resampling.NEAREST)
        visible = min(pose.height, 60 - drop - tuck)
        if visible > 0:
            cel.paste(pose.crop((0,0,W,visible)), (0,drop+tuck))
    # Three overlapping wisps expand, drift apart, then dissolve into small chips.
    if 12 <= frame <= 18:
        age = frame-12
        puffs = [(31,-1,1), (36,0,0), (40,1,2)]
        draw = ImageDraw.Draw(cel)
        for x, direction, delay in puffs:
            t=age-delay
            if t < 0 or t > 5: continue
            r=(2,4,5,4,3,1)[t]
            cx,cy=x+direction*t,58-t*2
            def silhouette(rad):
                return [(cx-rad,cy),(cx-rad+1,cy-rad+1),(cx-1,cy-rad),(cx+rad-1,cy-rad+1),(cx+rad,cy),(cx+rad-1,cy+rad-1),(cx,cy+rad),(cx-rad+1,cy+rad-1)]
            draw.polygon(silhouette(r), fill='#685065' if t<4 else '#887080')
            if r>2:
                draw.polygon(silhouette(r-1),fill='#b098a5')
                draw.line((cx-r+2,cy-1,cx-1,cy-r+2),fill='#d1bfc2')
        draw.rectangle((0,60,W,H), fill=(0,0,0,0))
    return cel


def save_clip(name, frames, milliseconds, libre):
    # A native RGBA cel per exposure preserves identical holds and the transparent last cel.
    def chunk(kind, data):
        return struct.pack('<IH',6+len(data),kind)+data
    blocks = []
    for i,im in enumerate(frames):
        chunks = []
        if not i:
            label=name.encode()
            chunks.append(chunk(0x2004,struct.pack('<6HB3xH',3,0,0,0,0,0,255,len(label))+label))
        data = struct.pack('<HhhBH7xHH',0,0,0,255,2,W,H)+zlib.compress(im.tobytes(),9)
        chunks.append(chunk(0x2005,data)); payload=b''.join(chunks)
        blocks.append(struct.pack('<IHHH6x',16+len(payload),0xf1fa,len(chunks),milliseconds)+payload)
    body=b''.join(blocks);header=bytearray(128)
    struct.pack_into('<I5HIH',header,0,128+len(body),0xa5e0,len(frames),W,H,32,1,milliseconds)
    header[34:36]=bytes((1,1))
    source=EDITABLE/(name+'.aseprite');source.write_bytes(header+body)
    target=OUT/(name+'.png')
    subprocess.run([libre,'-b',str(source),'--sheet',str(target),'--sheet-type','horizontal'],check=True)
    sheet=Image.open(target).convert('RGBA')
    assert sheet.size==(W*len(frames),H), (name,sheet.size)
    for i,f in enumerate(frames):
        assert sheet.crop((i*W,0,(i+1)*W,H)).tobytes()==f.tobytes(), (name,i)


def write_hum():
    # A soft closed-mouth three-note phrase, with portamento and a short breath between notes.
    rate=22050
    notes=[(0,.42,293.66),(.46,.91,349.23),(.95,1.70,329.63)]
    phase=0; buffer=bytearray()
    for index in range(int(rate*1.82)):
        t=index/rate
        voice=0
        for start,end,freq in notes:
            if start<=t<end:
                age=t-start
                envelope=min(1,age/.065,(end-t)/.11)
                pitch=freq*(1-.025*math.exp(-age*28)+.003*math.sin(2*math.pi*4.8*t))
                phase+=2*math.pi*pitch/rate
                voice=envelope*(.68*math.sin(phase)+.23*math.sin(2*phase)+.065*math.sin(3*phase)+.025*math.sin(4*phase))
                break
        buffer.extend(struct.pack('<h',round(voice*12500)))
    with wave.open(str(OUT/'MerchantHum.wav'),'wb') as sound:
        sound.setparams((1,2,rate,0,'NONE','not compressed'));sound.writeframes(buffer)


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--libresprite',default='libresprite')
    editor=parser.parse_args().libresprite
    OUT.mkdir(exist_ok=True)
    for name,artist,n,ms in [('Humming',hum,16,160),('Talking',speaking,6,95),('Goodbye',goodbye,20,50)]:
        save_clip(name,[artist(i) for i in range(n)],ms,editor)
    write_hum()
