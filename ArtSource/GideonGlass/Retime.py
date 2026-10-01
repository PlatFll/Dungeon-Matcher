"""Set authored exposure metadata in native ASE files; cel bytes are untouched."""
from pathlib import Path
import struct
ROOT=Path(__file__).resolve().parent
TIMINGS={'Gideon_Idle':[130]*9, 'Gideon_Cast':[80,80,120,80,50,40,80,120,100,100],
         'Gideon_Hold':[130], 'Gideon_Recovery':[80,80,80,130]}
for name,durations in TIMINGS.items():
    path=ROOT/(name+'.aseprite'); data=bytearray(path.read_bytes())
    assert struct.unpack_from('<H',data,4)[0]==0xA5E0
    assert struct.unpack_from('<H',data,6)[0]==len(durations)
    offset=128
    for duration in durations:
        assert struct.unpack_from('<H',data,offset+4)[0]==0xF1FA
        struct.pack_into('<H',data,offset+8,duration)
        offset+=struct.unpack_from('<I',data,offset)[0]
    assert offset==len(data)
    path.write_bytes(data)
    print(name,durations,sum(durations))
