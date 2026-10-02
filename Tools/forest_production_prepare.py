"""Reproducible native production preparation (no resampling)."""
from pathlib import Path
import json
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]

if __name__ == '__main__':
    folder = ROOT/'ArtSource/Forest/Production/Inputs'
    folder.mkdir(parents=True,exist_ok=True)
    for p in (ROOT/'ArtSource/Forest/Approved').glob('*.png'):
        im=Image.open(p).convert('RGBA')
        width=max(96,im.width)
        padded=Image.new('RGBA',(width,im.height))
        padded.paste(im,((width-im.width)//2,0))
        padded.save(folder/(p.stem+'.png'))
    study=Image.open(ROOT/'ArtSource/Forest/UI/Woodland_Study_Accepted.png')
    study.crop((0,0,128,128)).save(folder/'Woodland_Style_128.png')
    study.crop((0,64,64,128)).save(folder/'Woodland_Style_64.png')
