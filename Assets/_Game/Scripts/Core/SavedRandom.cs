using System;

// A small explicit state avoids depending on a particular .NET Random private
// layout or replaying an ever-growing history merely to restore its position.
public sealed class SavedRandom : Random
{
    public uint State { get; private set; }
    public SavedRandom(int seed) : this(unchecked((uint)seed)) { }
    public SavedRandom(uint state) { State = state == 0 ? 0x9e3779b9u : state; }
    private uint Draw()
    {
        uint value = State;
        value ^= value << 13; value ^= value >> 17; value ^= value << 5;
        return State = value;
    }
    protected override double Sample() => Draw() / 4294967296.0;
    public override double NextDouble() => Sample();
    public override int Next() => (int)(Draw() % int.MaxValue);
    public override int Next(int maximum)
    {
        if (maximum < 0) throw new ArgumentOutOfRangeException(nameof(maximum));
        return (int)(Sample() * maximum);
    }
    public override int Next(int minimum, int maximum)
    {
        if (minimum > maximum) throw new ArgumentOutOfRangeException(nameof(maximum));
        return (int)(minimum + Sample() * ((long)maximum - minimum));
    }
    public override void NextBytes(byte[] buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        for (int i=0;i<buffer.Length;i++) buffer[i]=(byte)Draw();
    }
}
