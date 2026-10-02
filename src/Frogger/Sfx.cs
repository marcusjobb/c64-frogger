using Raylib_cs;

namespace Frogger;

// Gammaldags blip och blop. Ljuden byggs som små WAV-filer i minnet,
// så vi behöver inga ljudfiler i repot.
public static class Sfx
{
    const int SampleRate = 22050;

    static bool _ready;
    static Sound _hop, _die, _record;

    public static void Init()
    {
        Raylib.InitAudioDevice();
        _ready = Raylib.IsAudioDeviceReady();
        if (!_ready) return; // ingen ljudkälla, spelet fungerar ändå utan ljud

        _hop = Load(Tone(Shape.Square, 520, 900, 70));
        _die = Load(Tone(Shape.Noise, 0, 0, 90).Concat(Tone(Shape.Square, 220, 55, 280)).ToArray());
        _record = Load(Tone(Shape.Square, 660, 660, 70)
            .Concat(Tone(Shape.Square, 880, 880, 70))
            .Concat(Tone(Shape.Square, 1320, 1320, 140)).ToArray());
    }

    public static void Hop() => Play(_hop);
    public static void Die() => Play(_die);
    public static void Record() => Play(_record);

    public static void Close()
    {
        if (!_ready) return;
        Raylib.UnloadSound(_hop);
        Raylib.UnloadSound(_die);
        Raylib.UnloadSound(_record);
        Raylib.CloseAudioDevice();
    }

    static void Play(Sound sound)
    {
        if (_ready) Raylib.PlaySound(sound);
    }

    enum Shape { Square, Noise }

    // Skapar rå 16-bitars ljuddata. Tonen glider från startHz till endHz.
    static byte[] Tone(Shape shape, float startHz, float endHz, int ms)
    {
        int count = SampleRate * ms / 1000;
        var data = new byte[count * 2];
        var rng = new Random(1);
        float phase = 0;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            float hz = startHz + (endHz - startHz) * t;
            phase += hz / SampleRate;
            if (phase >= 1) phase -= 1;

            float raw = shape == Shape.Square
                ? (phase < 0.5f ? 1f : -1f)
                : rng.NextSingle() * 2 - 1;

            float fade = 1 - t * 0.6f;               // tonen tynar lite mot slutet
            short sample = (short)(raw * fade * 0.25f * short.MaxValue);
            data[i * 2] = (byte)(sample & 0xFF);
            data[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }
        return data;
    }

    // Packar in ljuddatat i en WAV-fil i minnet och låter raylib läsa den
    static Sound Load(byte[] pcm)
    {
        using var stream = new MemoryStream();
        using (var w = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            w.Write("RIFF"u8); w.Write(36 + pcm.Length); w.Write("WAVE"u8);
            w.Write("fmt "u8); w.Write(16); w.Write((short)1); w.Write((short)1);
            w.Write(SampleRate); w.Write(SampleRate * 2); w.Write((short)2); w.Write((short)16);
            w.Write("data"u8); w.Write(pcm.Length); w.Write(pcm);
        }
        var bytes = stream.ToArray();
        var wave = Raylib.LoadWaveFromMemory(".wav", bytes);
        var sound = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
        return sound;
    }
}
