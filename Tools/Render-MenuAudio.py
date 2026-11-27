"""Render the original menu Foley. Fixed-pitch resonances, soft attacks and no sampled audio."""
from pathlib import Path
import math
import random
import struct
import wave

RATE = 44100
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/_Game/Resources/Audio/Menu"

# Each strike is (start, frequency, amplitude, ring-down). Stone uses short,
# inharmonic modes. The crystal and coin use lighter, longer upper partials.
CUES = {
    "Press":    (.080, .014, [(0, 438, .68, .011), (0, 1056, .35, .009), (.006, 1820, .10, .006)]),
    "PressAlt": (.083, .013, [(0, 408, .65, .012), (0, 946, .36, .010), (.005, 1690, .13, .006)]),
    "Page":     (.106, .034, [(0, 360, .46, .013), (.016, 812, .21, .015)]),
    "Socket":   (.138, .012, [(0, 509, .57, .014), (.008, 1154, .39, .024), (.008, 1740, .12, .018)]),
    "Coin":     (.194, .008, [(0, 1245, .44, .032), (0, 2071, .26, .021), (.037, 1570, .48, .031), (.037, 2562, .18, .022)]),
    "Level":    (.270, .006, [(0, 554, .64, .040), (0, 1108, .15, .024), (.050, 831, .49, .048), (.090, 1108, .21, .038)]),
    "Release":  (.092, .020, [(0, 523, .50, .010), (.018, 349, .39, .015)]),
    "Denied":   (.081, .012, [(0, 292, .63, .013), (0, 584, .20, .009)]),
}

def render(name, spec, seed):
    duration, noise_decay, strikes = spec
    rng = random.Random(seed)
    data = []
    last = 0.0
    for i in range(round(RATE * duration)):
        t = i / RATE
        grain = rng.uniform(-1, 1)
        filtered = .83 * last + .17 * grain
        last = filtered
        sample = .32 * filtered * math.exp(-t / noise_decay)
        for start, frequency, amplitude, decay in strikes:
            age = t - start
            if age >= 0:
                attack = 1 - math.exp(-age / .0018)
                sample += amplitude * attack * math.exp(-age / decay) * math.sin(math.tau * frequency * age)
        edge = min(1, t / .0015, max(0, (duration - t - 1/RATE) / .009))
        data.append(sample * edge)
    gain = .55 / max(abs(value) for value in data)
    with wave.open(str(OUTPUT / (name + ".wav")), "wb") as clip:
        clip.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        clip.writeframes(b"".join(struct.pack("<h", round(value * gain * 32767)) for value in data))

if __name__ == "__main__":
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for index, (cue, spec) in enumerate(CUES.items()):
        render(cue, spec, 12479 + index)
