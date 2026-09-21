"""Original Dungeon Matcher synthesis; standard-library-only, deterministic PCM exports.

Run from any directory: python ArtSource/Audio/generate_combat_sfx.py
No samples, external service, or downloaded audio are used. The clips deliberately
leave headroom; runtime gains and the six-voice budget are in CombatSoundMix.
"""
from pathlib import Path
import array
import hashlib
import json
import math
import random
import wave

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/_Game/Resources/Audio/Combat"
RATE = 44100
PEAK = 0.68
TAU = 2 * math.pi


def tone(t, freq, decay, overtone=0.15):
    return (math.sin(TAU * freq * t) + overtone * math.sin(TAU * freq * 2.01 * t)) * math.exp(-t / decay)


def sweep(t, high, low, decay):
    # Integral of an exponential frequency glide, without discontinuous phase.
    phase = low * t + (high - low) * decay * (1 - math.exp(-t / decay))
    return math.sin(TAU * phase)


def pluck(t, freq, decay):
    if t < 0:
        return 0.0
    return sum(math.sin(TAU * freq * k * t) * math.exp(-t * k / decay) / (k * k)
               for k in range(1, 7))


def sequence(t, notes, spacing, decay=0.15):
    return sum(pluck(t - i * spacing, note, decay) for i, note in enumerate(notes) if t >= i * spacing)


DURATIONS = {
    "GemMatch": .19, "GemLand": .085, "Explosion": .235, "PoisonBurst": .34,
    "Healing": .43, "ShieldGain": .30, "ShieldHit": .155, "PlayerHit": .16,
    "EnemyHit": .12, "PoisonTick": .105, "BardleyAbility": .57,
    "RattlebonesAbility": .52, "EnemyAbility": .285,
}


def make(name, duration):
    rng = random.Random("Dungeon Matcher original SFX " + name)
    low = 0.0
    values = []
    for i in range(round(duration * RATE)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        low += .12 * (noise - low)
        high = noise - low
        if name == "GemMatch":
            value = tone(t, 1318.5, .028) + .38 * tone(t, 2093, .042) + .18 * tone(t, 3136, .019) + .06 * high * math.exp(-t / .008)
        elif name == "GemLand":
            value = tone(t, 330, .014) + .3 * tone(t, 660, .01) + .16 * low * math.exp(-t / .008)
        elif name == "Explosion":
            value = .65 * sweep(t, 180, 62, .035) * math.exp(-t / .045) + 1.8 * low * math.exp(-t / .055) + .14 * high * math.exp(-t / .012)
        elif name == "PoisonBurst":
            value = 1.1 * low * math.exp(-t / .10) + .08 * high * math.exp(-t / .07)
            for start, freq in [(0, 530), (.07, 720), (.13, 430)]:
                if t >= start:
                    value += .32 * sweep(t-start, freq, 180, .024) * math.exp(-(t-start) / .028)
        elif name == "Healing":
            value = sequence(t, [659.25, 783.99, 1046.50, 1318.5], .055, .10)
        elif name == "ShieldGain":
            value = tone(t, 523.25, .075) + .55 * tone(t, 1046.5, .082) + .27 * tone(t, 1568, .05)
        elif name == "ShieldHit":
            value = tone(t, 784, .034) + .35 * tone(t, 1681, .023) + .22 * high * math.exp(-t/.012)
        elif name == "PlayerHit":
            value = sweep(t, 260, 95, .017) * math.exp(-t/.032) + .8 * low * math.exp(-t/.024) + .1 * high * math.exp(-t/.006)
        elif name == "EnemyHit":
            value = sweep(t, 390, 155, .015) * math.exp(-t/.023) + .65 * low * math.exp(-t/.014)
        elif name == "PoisonTick":
            value = sweep(t, 650, 220, .013) * math.exp(-t/.017) + .22 * low * math.exp(-t/.02)
        elif name == "BardleyAbility":
            # A tiny plucked lute flourish in C minor, brightened by the final octave.
            value = sequence(t, [261.63, 392, 523.25, 622.25, 783.99, 1046.5], .046, .12)
        elif name == "RattlebonesAbility":
            # Crown chime over a compact bone rattle; regal, lightly spooky, never shrill.
            value = sequence(t, [392, 523.25, 783.99, 1046.5], .065, .12)
            value += .26 * low * math.exp(-t/.07) * (.65 + .35 * math.cos(TAU * 42 * t))
        else:
            value = tone(t, 196, .05) + .38 * tone(t, 293.66, .07) + .18 * sweep(t, 630, 330, .03) * math.exp(-t/.045)
        # Quiet 1.5ms attack and 15ms fade prevent clicks at file boundaries.
        fade = min(1, t / .0015, (duration - t) / .015)
        values.append(value * max(0, fade))
    scale = PEAK / max(abs(v) for v in values)
    pcm = array.array("h", (round(v * scale * 32767) for v in values))
    return pcm


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    report = {"sample_rate": RATE, "channels": 1, "sample_bits": 16,
              "authorship": "Original local deterministic synthesis; no external samples.", "clips": {}}
    for name, duration in DURATIONS.items():
        pcm = make(name, duration)
        path = OUT / (name + ".wav")
        with wave.open(str(path), "wb") as output:
            output.setparams((1, 2, RATE, len(pcm), "NONE", "not compressed"))
            output.writeframes(pcm.tobytes())
        peak = max(abs(v) for v in pcm) / 32768
        rms = math.sqrt(sum(v * v for v in pcm) / len(pcm)) / 32768
        report["clips"][name] = {"seconds": len(pcm) / RATE, "peak_dbfs": round(20*math.log10(peak), 3),
                                 "rms_dbfs": round(20*math.log10(rms), 3),
                                 "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
    (Path(__file__).parent / "sfx_manifest.json").write_text(json.dumps(report, indent=2) + "\n")
    print(f"Generated {len(DURATIONS)} original mono PCM cues; peak ceiling {20*math.log10(PEAK):.2f} dBFS.")


if __name__ == "__main__":
    main()
