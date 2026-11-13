using UnityEngine;

namespace PixelAdventure
{
    /// <summary>
    /// Musik latar baru (disusun otomatis saat runtime, berulang mulus):
    /// 10 Kota siang (pop ceria), 11 Desa & Kampung (pentatonik ala gamelan + gong),
    /// 12 Pasar (irama kendang), 13 Pantai (santai), 14 Malam (tenang), 15 Halaman utama (haru).
    /// Tiap lagu: akor + bas + arpeggio + melodi 4 birama yang diulang dengan variasi + perkusi.
    /// </summary>
    public static partial class GameAudio
    {
        public const int MusicTown = 10, MusicVillage = 11, MusicMarket = 12, MusicBeach = 13, MusicNight = 14, MusicTitle = 15;

        enum Drums { None, Pop, Kendang, Shaker, Gong }

        struct Song
        {
            public float Bpm, RootHz;
            public int[] Scale;          // semitone dari nada dasar
            public int[] Progression;    // derajat skala akor per birama (diulang)
            public int LeadWave, ArpWave;
            public float LeadVol, ArpVol, BassVol;
            public float Echo;
            public Drums Drum;
            public int Seed, Bars;
            public float NoteChance;     // peluang ada nada melodi tiap seperdelapan
        }

        static Song SongFor(int index)
        {
            int[] major = { 0, 2, 4, 5, 7, 9, 11 };
            int[] pentaMaj = { 0, 2, 4, 7, 9 };
            int[] slendro = { 0, 2, 5, 7, 9 };          // pendekatan laras slendro
            int[] minorH = { 0, 2, 3, 5, 7, 8, 11 };    // minor harmonik (nuansa dangdut)
            int[] pentaMin = { 0, 3, 5, 7, 10 };
            switch (index)
            {
                case MusicTown:
                    return new Song { Bpm = 112, RootHz = 261.6f, Scale = major, Progression = new[] { 0, 4, 5, 3 },
                        LeadWave = 0, ArpWave = 1, LeadVol = 0.085f, ArpVol = 0.045f, BassVol = 0.13f, Echo = 0.12f,
                        Drum = Drums.Pop, Seed = 11, Bars = 16, NoteChance = 0.62f };
                case MusicVillage:
                    return new Song { Bpm = 84, RootHz = 293.7f, Scale = slendro, Progression = new[] { 0, 0, 3, 2 },
                        LeadWave = 2, ArpWave = 2, LeadVol = 0.11f, ArpVol = 0.05f, BassVol = 0.10f, Echo = 0.22f,
                        Drum = Drums.Gong, Seed = 23, Bars = 16, NoteChance = 0.55f };
                case MusicMarket:
                    return new Song { Bpm = 126, RootHz = 220f, Scale = minorH, Progression = new[] { 0, 3, 4, 0 },
                        LeadWave = 3, ArpWave = 1, LeadVol = 0.075f, ArpVol = 0.04f, BassVol = 0.14f, Echo = 0.1f,
                        Drum = Drums.Kendang, Seed = 37, Bars = 16, NoteChance = 0.68f };
                case MusicBeach:
                    return new Song { Bpm = 92, RootHz = 246.9f, Scale = pentaMaj, Progression = new[] { 0, 3, 1, 4 },
                        LeadWave = 1, ArpWave = 2, LeadVol = 0.10f, ArpVol = 0.05f, BassVol = 0.11f, Echo = 0.25f,
                        Drum = Drums.Shaker, Seed = 51, Bars = 16, NoteChance = 0.5f };
                case MusicNight:
                    return new Song { Bpm = 70, RootHz = 220f, Scale = pentaMin, Progression = new[] { 0, 3, 2, 4 },
                        LeadWave = 2, ArpWave = 2, LeadVol = 0.08f, ArpVol = 0.05f, BassVol = 0.08f, Echo = 0.3f,
                        Drum = Drums.None, Seed = 67, Bars = 16, NoteChance = 0.38f };
                default: // MusicTitle
                    return new Song { Bpm = 96, RootHz = 261.6f, Scale = major, Progression = new[] { 5, 3, 0, 4 },
                        LeadWave = 2, ArpWave = 1, LeadVol = 0.10f, ArpVol = 0.05f, BassVol = 0.10f, Echo = 0.28f,
                        Drum = Drums.Shaker, Seed = 79, Bars = 16, NoteChance = 0.5f };
            }
        }

        /// <summary>Frekuensi derajat skala ke-n (bisa negatif / melewati oktaf).</summary>
        static float Degree(Song s, int degree)
        {
            int n = s.Scale.Length;
            int oct = Mathf.FloorToInt(degree / (float)n);
            int idx = ((degree % n) + n) % n;
            return s.RootHz * Mathf.Pow(2f, (s.Scale[idx] + 12 * oct) / 12f);
        }

        static AudioClip BuildMusicExtra(int index)
        {
            Song s = SongFor(index);
            float beat = 60f / s.Bpm;
            float eighth = beat * 0.5f;
            int bars = s.Bars;
            float length = bars * 4 * beat;
            var b = new float[Mathf.CeilToInt(length * SampleRate) + 1];
            var rng = new System.Random(s.Seed);

            // ---- melodi 4 birama (frase), dipakai ulang dengan variasi agar mudah diingat
            int phraseSteps = 4 * 8;
            var phrase = new int[phraseSteps];          // derajat; int.MinValue = diam
            int deg = s.Scale.Length;                   // mulai di oktaf tengah
            for (int i = 0; i < phraseSteps; i++)
            {
                bool strong = i % 4 == 0;
                if (rng.NextDouble() < (strong ? s.NoteChance + 0.2f : s.NoteChance))
                {
                    deg += rng.Next(-2, 3);
                    deg = Mathf.Clamp(deg, s.Scale.Length - 2, s.Scale.Length * 2 + 2);
                    phrase[i] = deg;
                }
                else phrase[i] = int.MinValue;
            }

            for (int bar = 0; bar < bars; bar++)
            {
                float t0 = bar * 4 * beat;
                int chordDeg = s.Progression[bar % s.Progression.Length];

                // Bas: nada dasar akor (oktaf rendah) di ketukan 1 & 3, kadang nada kelima.
                float root = Degree(s, chordDeg - s.Scale.Length) * 0.5f;
                for (int k = 0; k < 4; k += 2)
                    AddNote(b, t0 + k * beat, k == 2 && s.Drum == Drums.Kendang ? Degree(s, chordDeg + 4 - s.Scale.Length) * 0.5f : root,
                        beat * 1.6f, s.BassVol, 1, 0.01f);

                // Arpeggio akor tiap seperdelapan (atau pad panjang untuk lagu tenang).
                int[] chord = { chordDeg, chordDeg + 2, chordDeg + 4 };
                if (s.Bpm < 80f)
                    foreach (int cd in chord) AddNote(b, t0, Degree(s, cd), beat * 4f, s.ArpVol * 0.8f, s.ArpWave, 0.15f);
                else
                    for (int e = 0; e < 8; e++)
                        AddNote(b, t0 + e * eighth, Degree(s, chord[e % 3] + (e >= 4 ? s.Scale.Length : 0)), eighth * 0.9f, s.ArpVol, s.ArpWave, 0.004f);

                // Melodi: frase diulang; birama 5–8 & 13–16 digeser naik/turun sebagai variasi.
                int shift = (bar / 4) % 4 == 1 ? 2 : (bar / 4) % 4 == 3 ? -1 : 0;
                for (int e = 0; e < 8; e++)
                {
                    int step = (bar % 4) * 8 + e;
                    int d = phrase[step];
                    if (d == int.MinValue) continue;
                    // panjang nada: sampai nada berikutnya (maks. 3 seperdelapan)
                    int len = 1;
                    while (len < 3 && step + len < phraseSteps && phrase[step + len] == int.MinValue) len++;
                    AddNote(b, t0 + e * eighth, Degree(s, d + shift), eighth * len * 0.95f, s.LeadVol, s.LeadWave, 0.006f, s.Echo);
                    if (s.Drum == Drums.Gong && e % 2 == 0)                         // bonang: gema satu oktaf di atas
                        AddNote(b, t0 + e * eighth + 0.02f, Degree(s, d + shift + s.Scale.Length), eighth * 0.6f, s.LeadVol * 0.35f, 2, 0.002f);
                }

                // Perkusi.
                switch (s.Drum)
                {
                    case Drums.Pop:
                        for (int k = 0; k < 4; k++)
                        {
                            if (k % 2 == 0) Kick(b, t0 + k * beat, 0.22f);
                            else AddNoise(b, t0 + k * beat, 0.12f, 0.10f, 3f);                  // snare
                        }
                        for (int e = 0; e < 8; e++) AddNoise(b, t0 + e * eighth, 0.03f, 0.035f, 6f);   // hi-hat
                        break;
                    case Drums.Kendang:
                        // pola kendang: dhung (rendah) & tak (tinggi) bersinkop
                        float[] pos = { 0f, 1.5f, 2f, 3f, 3.5f };
                        for (int k = 0; k < pos.Length; k++)
                        {
                            bool low = k % 2 == 0;
                            AddNote(b, t0 + pos[k] * beat, low ? 110f : 330f, low ? 0.16f : 0.07f, low ? 0.2f : 0.12f, 2, 0.002f);
                            if (!low) AddNoise(b, t0 + pos[k] * beat, 0.04f, 0.06f, 5f);
                        }
                        for (int e = 1; e < 8; e += 2) AddNoise(b, t0 + e * eighth, 0.025f, 0.03f, 6f);
                        break;
                    case Drums.Shaker:
                        for (int e = 0; e < 8; e++) AddNoise(b, t0 + e * eighth, 0.05f, e % 2 == 0 ? 0.03f : 0.05f, 4f);
                        Kick(b, t0, 0.12f);
                        break;
                    case Drums.Gong:
                        if (bar % 4 == 0) AddNote(b, t0, 98f, beat * 4f, 0.2f, 2, 0.02f, 0.2f);   // gong
                        if (bar % 2 == 1) AddNote(b, t0 + 2 * beat, 196f, beat * 1.5f, 0.1f, 2, 0.01f);   // kenong
                        break;
                }
            }

            // Akhir & awal disambung lembut agar berulang tanpa "klik".
            int fade = SampleRate / 20;
            for (int i = 0; i < fade; i++) { float k = i / (float)fade; b[i] *= k; b[b.Length - 1 - i] *= k; }
            return Finish("music_" + index, b, 0.7f);
        }

        /// <summary>Kick drum: nada rendah yang turun cepat + sedikit noise.</summary>
        static void Kick(float[] b, float t, float vol)
        {
            int start = Mathf.RoundToInt(t * SampleRate);
            int len = Mathf.RoundToInt(0.14f * SampleRate);
            double phase = 0;
            for (int i = 0; i < len && start + i < b.Length; i++)
            {
                float k = i / (float)len;
                float freq = Mathf.Lerp(120f, 45f, k);
                phase += freq / SampleRate;
                b[start + i] += Mathf.Sin((float)phase * Mathf.PI * 2f) * vol * Mathf.Pow(1f - k, 2f);
            }
        }
    }
}
