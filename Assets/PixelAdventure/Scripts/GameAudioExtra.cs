using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>
    /// Efek suara tambahan (langkah, pintu, bus, klakson, halaman, tablet, makan, kasir,
    /// naik level, misi selesai, telepon, ambil barang, menu) dan suara latar berulang
    /// (kota, alam/burung, pantai/ombak, malam/jangkrik). Semua disintesis saat runtime.
    /// </summary>
    public static partial class GameAudio
    {
        static AudioSource _ambience;
        static string _ambienceKey;
        static readonly Dictionary<string, AudioClip> Ambience = new Dictionary<string, AudioClip>();

        /// <summary>Putar suara latar berulang. null / "" = hening. Tidak memulai ulang bila sama.</summary>
        public static void PlayAmbience(string key)
        {
            if (_ambience == null) return;
            if (key == _ambienceKey) return;
            _ambienceKey = key;
            if (string.IsNullOrEmpty(key)) { _ambience.Stop(); return; }
            if (!Ambience.TryGetValue(key, out AudioClip clip) || clip == null)
            {
                clip = BuildAmbience(key);
                Ambience[key] = clip;
            }
            if (clip == null) { _ambience.Stop(); return; }
            _ambience.clip = clip;
            _ambience.Play();
        }

        // ------------------------------------------------------------------ efek suara

        static AudioClip BuildExtraSfx(string name)
        {
            switch (name)
            {
                case "step":            // langkah kaki pelan
                {
                    var b = new float[(int)(0.07f * SampleRate)];
                    AddNoise(b, 0f, 0.05f, 0.30f, 6f);
                    AddNote(b, 0f, 95f, 0.05f, 0.25f, 2);
                    return Finish(name, b, 0.25f);
                }
                case "step_run":        // langkah berlari, lebih tegas
                {
                    var b = new float[(int)(0.06f * SampleRate)];
                    AddNoise(b, 0f, 0.045f, 0.4f, 5f);
                    AddNote(b, 0f, 120f, 0.04f, 0.3f, 2);
                    return Finish(name, b, 0.32f);
                }
                case "door_open":       // derit engsel + klik
                {
                    var b = new float[(int)(0.42f * SampleRate)];
                    AddNote(b, 0f, 520f, 0.05f, 0.2f, 0);
                    for (int i = 0; i < 6; i++) AddNote(b, 0.06f + i * 0.045f, 300f + i * 40f, 0.05f, 0.12f, 1);
                    AddNoise(b, 0.3f, 0.08f, 0.2f, 5f);
                    return Finish(name, b, 0.4f);
                }
                case "door_close":      // pintu menutup (debam pendek)
                {
                    var b = new float[(int)(0.25f * SampleRate)];
                    AddNoise(b, 0f, 0.14f, 0.5f, 5f);
                    AddNote(b, 0f, 110f, 0.16f, 0.4f, 2);
                    AddNote(b, 0.12f, 640f, 0.04f, 0.12f, 0);
                    return Finish(name, b, 0.45f);
                }
                case "bus":             // mesin bus + klakson dua nada
                {
                    var b = new float[(int)(1.4f * SampleRate)];
                    for (int i = 0; i < 14; i++) AddNote(b, i * 0.08f, 70f + (i % 3) * 6f, 0.12f, 0.28f, 0, 0.01f);
                    AddNoise(b, 0f, 1.2f, 0.1f, 1.2f);
                    AddNote(b, 0.95f, 392f, 0.18f, 0.22f, 0);
                    AddNote(b, 0.95f, 494f, 0.18f, 0.18f, 0);
                    return Finish(name, b, 0.55f);
                }
                case "horn":            // klakson mobil "tin-tin"
                {
                    var b = new float[(int)(0.42f * SampleRate)];
                    AddNote(b, 0f, 415f, 0.13f, 0.25f, 0, 0.004f);
                    AddNote(b, 0f, 523f, 0.13f, 0.18f, 0, 0.004f);
                    AddNote(b, 0.2f, 415f, 0.16f, 0.25f, 0, 0.004f);
                    AddNote(b, 0.2f, 523f, 0.16f, 0.18f, 0, 0.004f);
                    return Finish(name, b, 0.42f);
                }
                case "engine_start":    // starter motor/mobil: "brrm-brrrm" lalu langsam
                {
                    var b = new float[(int)(0.9f * SampleRate)];
                    for (int i = 0; i < 12; i++)
                    {
                        float f = i < 4 ? 55f + i * 14f : 95f - (i - 4) * 3f;
                        AddNote(b, i * 0.07f, f, 0.1f, 0.3f, 0, 0.008f);
                    }
                    AddNoise(b, 0f, 0.85f, 0.08f, 1.5f);
                    return Finish(name, b, 0.5f);
                }
                case "bike_bell":       // bel sepeda "kring-kring"
                {
                    var b = new float[(int)(0.5f * SampleRate)];
                    AddNote(b, 0f, 1760f, 0.16f, 0.2f, 2, 0.002f);
                    AddNote(b, 0f, 2350f, 0.12f, 0.1f, 2, 0.002f);
                    AddNote(b, 0.2f, 1760f, 0.22f, 0.2f, 2, 0.002f);
                    AddNote(b, 0.2f, 2350f, 0.16f, 0.1f, 2, 0.002f);
                    return Finish(name, b, 0.4f);
                }
                case "page":            // membalik halaman buku
                {
                    var b = new float[(int)(0.22f * SampleRate)];
                    AddNoise(b, 0f, 0.18f, 0.28f, 1.6f);
                    AddNoise(b, 0.05f, 0.1f, 0.18f, 3f);
                    return Finish(name, b, 0.35f);
                }
                case "tablet_on":       // tablet menyala: dua nada naik
                {
                    var b = new float[(int)(0.3f * SampleRate)];
                    AddNote(b, 0f, 880f, 0.08f, 0.2f, 2);
                    AddNote(b, 0.08f, 1320f, 0.16f, 0.2f, 2, 0.004f, 0.12f);
                    return Finish(name, b, 0.4f);
                }
                case "tablet_off":      // tablet mati: dua nada turun
                {
                    var b = new float[(int)(0.26f * SampleRate)];
                    AddNote(b, 0f, 1175f, 0.07f, 0.18f, 2);
                    AddNote(b, 0.07f, 740f, 0.14f, 0.18f, 2);
                    return Finish(name, b, 0.36f);
                }
                case "menu_open":       // munculnya menu
                {
                    var b = new float[(int)(0.12f * SampleRate)];
                    AddNote(b, 0f, 660f, 0.05f, 0.16f, 1);
                    AddNote(b, 0.04f, 990f, 0.07f, 0.14f, 1);
                    return Finish(name, b, 0.3f);
                }
                case "confirm":         // memilih / konfirmasi
                {
                    var b = new float[(int)(0.16f * SampleRate)];
                    AddNote(b, 0f, 784f, 0.05f, 0.22f, 0);
                    AddNote(b, 0.05f, 1175f, 0.09f, 0.2f, 0);
                    return Finish(name, b, 0.4f);
                }
                case "eat":             // mengunyah: tiga gigitan
                {
                    var b = new float[(int)(0.5f * SampleRate)];
                    for (int i = 0; i < 3; i++)
                    {
                        AddNoise(b, i * 0.14f, 0.07f, 0.35f, 4f);
                        AddNote(b, i * 0.14f, 180f - i * 15f, 0.06f, 0.2f, 1);
                    }
                    AddNote(b, 0.42f, 988f, 0.07f, 0.12f, 2);
                    return Finish(name, b, 0.42f);
                }
                case "buy":             // mesin kasir "ka-ching"
                {
                    var b = new float[(int)(0.55f * SampleRate)];
                    AddNoise(b, 0f, 0.06f, 0.35f, 4f);
                    AddNote(b, 0.07f, 1568f, 0.12f, 0.2f, 2);
                    AddNote(b, 0.14f, 2093f, 0.32f, 0.2f, 2, 0.003f, 0.15f);
                    return Finish(name, b, 0.45f);
                }
                case "levelup":         // naik level: arpeggio naik + akord
                {
                    var b = new float[(int)(1.4f * SampleRate)];
                    float[] n = { 523f, 659f, 784f, 1046f, 1318f };
                    for (int i = 0; i < n.Length; i++) AddNote(b, i * 0.08f, n[i], 0.16f, 0.2f, 0, 0.004f, 0.1f);
                    AddNote(b, 0.45f, 1046f, 0.8f, 0.16f, 2, 0.01f, 0.2f);
                    AddNote(b, 0.45f, 1318f, 0.8f, 0.14f, 2, 0.01f);
                    AddNote(b, 0.45f, 1568f, 0.8f, 0.12f, 2, 0.01f);
                    return Finish(name, b, 0.6f);
                }
                case "quest_done":      // tujuan misi selesai
                {
                    var b = new float[(int)(0.75f * SampleRate)];
                    AddNote(b, 0f, 659f, 0.12f, 0.22f, 2);
                    AddNote(b, 0.1f, 880f, 0.12f, 0.22f, 2);
                    AddNote(b, 0.2f, 1175f, 0.42f, 0.22f, 2, 0.004f, 0.18f);
                    return Finish(name, b, 0.52f);
                }
                case "phone":           // dering telepon dua kali
                {
                    var b = new float[(int)(1.2f * SampleRate)];
                    for (int r = 0; r < 2; r++)
                        for (int i = 0; i < 8; i++)
                            AddNote(b, r * 0.6f + i * 0.045f, i % 2 == 0 ? 1318f : 1046f, 0.04f, 0.16f, 0);
                    return Finish(name, b, 0.42f);
                }
                case "pickup":          // mengambil barang
                {
                    var b = new float[(int)(0.3f * SampleRate)];
                    AddNote(b, 0f, 784f, 0.06f, 0.2f, 1);
                    AddNote(b, 0.06f, 1046f, 0.06f, 0.2f, 1);
                    AddNote(b, 0.12f, 1568f, 0.14f, 0.18f, 2, 0.004f, 0.1f);
                    return Finish(name, b, 0.45f);
                }
                case "notify":          // pemberitahuan kecil
                {
                    var b = new float[(int)(0.22f * SampleRate)];
                    AddNote(b, 0f, 1318f, 0.06f, 0.16f, 2);
                    AddNote(b, 0.07f, 1760f, 0.12f, 0.14f, 2);
                    return Finish(name, b, 0.34f);
                }
                case "step_hard":       // langkah di aspal / paving: ketukan tegas
                {
                    var b = new float[(int)(0.06f * SampleRate)];
                    AddNoise(b, 0f, 0.03f, 0.35f, 8f);
                    AddNote(b, 0f, 240f, 0.03f, 0.22f, 0);
                    return Finish(name, b, 0.28f);
                }
                case "step_grass":      // langkah di rumput: desir lembut
                {
                    var b = new float[(int)(0.1f * SampleRate)];
                    AddNoise(b, 0f, 0.09f, 0.22f, 1.5f);
                    return Finish(name, b, 0.2f);
                }
                case "step_sand":       // langkah di pasir: berderak panjang
                {
                    var b = new float[(int)(0.14f * SampleRate)];
                    AddNoise(b, 0f, 0.13f, 0.3f, 1.2f);
                    AddNoise(b, 0.03f, 0.08f, 0.2f, 3f);
                    return Finish(name, b, 0.26f);
                }
                case "step_wood":       // langkah di lantai dalam ruangan: bunyi kosong
                {
                    var b = new float[(int)(0.08f * SampleRate)];
                    AddNote(b, 0f, 170f, 0.07f, 0.3f, 1);
                    AddNoise(b, 0f, 0.02f, 0.15f, 8f);
                    return Finish(name, b, 0.28f);
                }
                case "flap":            // kepakan sayap burung
                {
                    var b = new float[(int)(0.3f * SampleRate)];
                    for (int i = 0; i < 4; i++) AddNoise(b, i * 0.06f, 0.05f, 0.25f, 3f);
                    AddNote(b, 0f, 2600f, 0.05f, 0.08f, 2);
                    return Finish(name, b, 0.3f);
                }
                case "splash_title":    // pembuka halaman utama
                {
                    var b = new float[(int)(1.6f * SampleRate)];
                    float[] n = { 392f, 523f, 659f, 784f, 1046f };
                    for (int i = 0; i < n.Length; i++) AddNote(b, i * 0.16f, n[i], 0.6f, 0.15f, 2, 0.02f, 0.25f);
                    return Finish(name, b, 0.5f);
                }
                default: return null;
            }
        }

        // ------------------------------------------------------------------ suara latar

        static AudioClip BuildAmbience(string key)
        {
            const float seconds = 8f;    // panjang satu putaran (berulang mulus)
            var b = new float[(int)(seconds * SampleRate)];
            var rng = new System.Random(key.GetHashCode());
            switch (key)
            {
                case "city":
                    // dengung lalu lintas: noise rendah + sesekali mesin lewat dan klakson jauh
                    LowNoise(b, 0.22f, 0.02f);
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (float)rng.NextDouble() * (seconds - 1.5f);
                        for (int k = 0; k < 10; k++) AddNote(b, t + k * 0.1f, 80f + k * 2f, 0.14f, 0.05f, 0, 0.02f);
                    }
                    AddNote(b, 3.1f, 466f, 0.12f, 0.04f, 0); AddNote(b, 3.3f, 466f, 0.14f, 0.04f, 0);
                    break;
                case "nature":
                    // angin lembut + kicau burung acak
                    LowNoise(b, 0.08f, 0.01f);
                    for (int i = 0; i < 14; i++)
                    {
                        float t = (float)rng.NextDouble() * (seconds - 0.6f);
                        float f = 2200f + (float)rng.NextDouble() * 1400f;
                        int chirps = 2 + rng.Next(3);
                        for (int k = 0; k < chirps; k++)
                            AddNote(b, t + k * 0.07f, f + (k % 2 == 0 ? 0f : 300f), 0.05f, 0.06f, 2, 0.004f);
                    }
                    break;
                case "beach":
                    // ombak: noise yang naik-turun pelan
                    for (int i = 0; i < b.Length; i++)
                    {
                        float t = i / (float)SampleRate;
                        float swell = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f / 4f);   // 4 detik per ombak
                        b[i] += ((float)rng.NextDouble() * 2f - 1f) * 0.18f * swell * swell;
                    }
                    Smooth(b, 6);
                    for (int i = 0; i < 4; i++) AddNote(b, 1f + i * 2f, 1900f, 0.06f, 0.03f, 2);   // camar jauh
                    break;
                case "night":
                    // jangkrik: denyut nada tinggi berulang
                    LowNoise(b, 0.04f, 0.005f);
                    for (float t = 0f; t < seconds - 0.3f; t += 0.55f + (float)rng.NextDouble() * 0.3f)
                        for (int k = 0; k < 4; k++)
                            AddNote(b, t + k * 0.03f, 4200f + (float)rng.NextDouble() * 300f, 0.02f, 0.035f, 2, 0.002f);
                    break;
                default:
                    return null;
            }
            FadeLoop(b);
            var clip = AudioClip.Create("amb_" + key, b.Length, 1, SampleRate, false);
            clip.SetData(b, 0);
            return clip;
        }

        /// <summary>Noise berfrekuensi rendah (dihaluskan) dengan sedikit getaran.</summary>
        static void LowNoise(float[] b, float volume, float wobble)
        {
            var rng = new System.Random(7);
            float last = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float n = (float)rng.NextDouble() * 2f - 1f;
                last = last * 0.96f + n * 0.04f;                       // filter low-pass sederhana
                float t = i / (float)SampleRate;
                b[i] += last * volume * 6f * (1f + wobble * 20f * Mathf.Sin(t * 1.3f));
            }
        }

        static void Smooth(float[] b, int passes)
        {
            for (int p = 0; p < passes; p++)
                for (int i = 1; i < b.Length; i++) b[i] = b[i] * 0.5f + b[i - 1] * 0.5f;
        }

        /// <summary>Pertemuan ujung-awal dibuat lembut agar putaran ulang tidak berbunyi "klik".</summary>
        static void FadeLoop(float[] b)
        {
            int f = SampleRate / 4;
            for (int i = 0; i < f && i < b.Length; i++)
            {
                float k = i / (float)f;
                b[i] *= k;
                b[b.Length - 1 - i] *= k;
            }
            float max = 0.001f;
            foreach (float v in b) max = Mathf.Max(max, Mathf.Abs(v));
            float gain = Mathf.Min(1f, 0.7f / max);
            for (int i = 0; i < b.Length; i++) b[i] = Mathf.Clamp(b[i] * gain, -1f, 1f);
        }
    }
}
