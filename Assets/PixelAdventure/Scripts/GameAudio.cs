using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure
{
    /// <summary>All sound is generated at runtime: chiptune music and crunchy square-wave SFX.</summary>
    public static partial class GameAudio
    {
        const int SampleRate = 22050;
        const string MuteKey = "PixelQuest.Muted";
        static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
        static readonly Dictionary<int, AudioClip> Music = new Dictionary<int, AudioClip>();
        static AudioSource _sfx;
        static AudioSource _music;
        static int _musicIndex = -1;
        static bool _muted;

        public static bool Muted => _muted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntimeCache()
        {
            Clips.Clear();
            Music.Clear();
            _sfx = null;
            _music = null;
            _musicIndex = -1;
            _ambience = null;
            _ambienceKey = null;
            Ambience.Clear();
        }

        public static void Init(GameObject host)
        {
            _muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            AudioListener.volume = _muted ? 0f : 1f;

            if (_sfx != null) return;
            var go = new GameObject("Audio");
            go.transform.SetParent(host.transform, false);

            _sfx = go.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _sfx.volume = 0.42f;

            _music = go.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.volume = 0.20f;

            _ambience = go.AddComponent<AudioSource>();
            _ambience.playOnAwake = false;
            _ambience.loop = true;
            _ambience.spatialBlend = 0f;
            _ambience.volume = 0.14f;
        }

        public static void Play(string name, float volume = 1f, float pitch = 1f)
        {
            if (_sfx == null) return;
            if (!Clips.TryGetValue(name, out AudioClip clip) || clip == null)
            {
                clip = BuildSfx(name);
                if (clip == null) return;
                Clips[name] = clip;
            }
            _sfx.pitch = pitch;
            _sfx.PlayOneShot(clip, volume);
        }

        public static void PlayMusic(int index)
        {
            if (_music == null) return;
            if (_musicIndex == index && _music.isPlaying) return;
            _musicIndex = index;
            if (!Music.TryGetValue(index, out AudioClip clip) || clip == null)
            {
                clip = index >= 10 ? BuildMusicExtra(index) : BuildMusic(index);   // 10+ = lagu baru (GameMusic.cs)
                Music[index] = clip;
            }
            _music.clip = clip;
            _music.Play();
        }

        public static void ToggleMute()
        {
            _muted = !_muted;
            AudioListener.volume = _muted ? 0f : 1f;
            PlayerPrefs.SetInt(MuteKey, _muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        // ------------------------------------------------------------------ synthesis

        static float Frac(float v) => v - Mathf.Floor(v);

        static float Wave(float phase, int type)
        {
            float p = Frac(phase);
            switch (type)
            {
                case 0: return Mathf.Sin(phase * Mathf.PI * 2f) >= 0f ? 1f : -1f;
                case 1: return 4f * Mathf.Abs(p - 0.5f) - 1f;
                case 2: return Mathf.Sin(phase * Mathf.PI * 2f);
                default: return p < 0.25f ? 1f : -1f;
            }
        }

        static void AddNote(float[] buffer, float startSec, float freq, float duration, float volume,
            int waveType, float attack = 0.006f, float echo = 0f)
        {
            int start = Mathf.RoundToInt(startSec * SampleRate);
            int length = Mathf.RoundToInt(duration * SampleRate);
            if (start >= buffer.Length) return;
            length = Mathf.Min(length, buffer.Length - start);
            if (length <= 0) return;

            double phase = 0;
            double increment = freq / SampleRate;
            int attackSamples = Mathf.Max(1, Mathf.RoundToInt(attack * SampleRate));
            for (int i = 0; i < length; i++)
            {
                float env = i < attackSamples ? i / (float)attackSamples : 1f;
                env *= Mathf.Pow(1f - i / (float)length, 1.6f);
                float s = Wave((float)phase, waveType) * volume * env;
                buffer[start + i] += s;
                if (echo > 0f && start + i + 3200 < buffer.Length)
                    buffer[start + i + 3200] += s * echo;
                phase += increment;
            }
        }

        static void AddNoise(float[] buffer, float startSec, float duration, float volume, float decay = 3f)
        {
            int start = Mathf.RoundToInt(startSec * SampleRate);
            int length = Mathf.RoundToInt(duration * SampleRate);
            if (start >= buffer.Length) return;
            length = Mathf.Min(length, buffer.Length - start);
            var rng = new System.Random(start + 13);
            for (int i = 0; i < length; i++)
            {
                float env = Mathf.Pow(1f - i / (float)length, decay);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                buffer[start + i] += noise * volume * env;
            }
        }

        static AudioClip Finish(string name, float[] buffer, float peak = 0.85f)
        {
            float max = 0.001f;
            for (int i = 0; i < buffer.Length; i++) max = Mathf.Max(max, Mathf.Abs(buffer[i]));
            float gain = peak / max;
            if (gain > 1f) gain = 1f;
            for (int i = 0; i < buffer.Length; i++) buffer[i] = Mathf.Clamp(buffer[i] * gain, -1f, 1f);
            var clip = AudioClip.Create(name, buffer.Length, 1, SampleRate, false);
            clip.SetData(buffer, 0);
            return clip;
        }

        static AudioClip BuildSfx(string name)
        {
            switch (name)
            {
                case "jump":
                {
                    var b = new float[(int)(0.14f * SampleRate)];
                    AddNote(b, 0f, 340f, 0.07f, 0.4f, 0);
                    AddNote(b, 0.05f, 620f, 0.09f, 0.35f, 0);
                    return Finish(name, b, 0.55f);
                }
                case "gem":
                {
                    var b = new float[(int)(0.26f * SampleRate)];
                    AddNote(b, 0f, 1046f, 0.06f, 0.3f, 2);
                    AddNote(b, 0.05f, 1318f, 0.08f, 0.3f, 2);
                    AddNote(b, 0.11f, 1568f, 0.14f, 0.25f, 2);
                    return Finish(name, b, 0.5f);
                }
                case "hurt":
                {
                    var b = new float[(int)(0.3f * SampleRate)];
                    AddNote(b, 0f, 280f, 0.11f, 0.4f, 0);
                    AddNote(b, 0.07f, 150f, 0.16f, 0.38f, 0);
                    AddNoise(b, 0f, 0.1f, 0.15f);
                    return Finish(name, b, 0.55f);
                }
                case "stomp":
                {
                    var b = new float[(int)(0.18f * SampleRate)];
                    AddNoise(b, 0f, 0.12f, 0.5f, 4f);
                    AddNote(b, 0f, 160f, 0.12f, 0.35f, 0);
                    return Finish(name, b, 0.5f);
                }
                case "spring":
                {
                    var b = new float[(int)(0.24f * SampleRate)];
                    AddNote(b, 0f, 220f, 0.2f, 0.32f, 1);
                    AddNote(b, 0.02f, 440f, 0.18f, 0.25f, 0);
                    return Finish(name, b, 0.5f);
                }
                case "land":
                {
                    var b = new float[(int)(0.09f * SampleRate)];
                    AddNoise(b, 0f, 0.07f, 0.35f, 4f);
                    return Finish(name, b, 0.35f);
                }
                case "portal":
                {
                    var b = new float[(int)(0.7f * SampleRate)];
                    float[] notes = { 523f, 659f, 784f, 1046f, 1318f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.09f, notes[i], 0.22f, 0.22f, 2, 0.004f, 0.12f);
                    return Finish(name, b, 0.55f);
                }
                case "checkpoint":
                {
                    var b = new float[(int)(0.4f * SampleRate)];
                    AddNote(b, 0f, 784f, 0.16f, 0.25f, 2);
                    AddNote(b, 0.12f, 1046f, 0.2f, 0.25f, 2);
                    return Finish(name, b, 0.5f);
                }
                case "gameover":
                {
                    var b = new float[(int)(0.9f * SampleRate)];
                    float[] notes = { 392f, 329f, 261f, 196f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.18f, notes[i], 0.25f, 0.3f, 0);
                    return Finish(name, b, 0.5f);
                }
                case "click":
                {
                    var b = new float[(int)(0.05f * SampleRate)];
                    AddNote(b, 0f, 880f, 0.04f, 0.25f, 0);
                    return Finish(name, b, 0.35f);
                }
                case "win":
                {
                    var b = new float[(int)(1.2f * SampleRate)];
                    float[] notes = { 523f, 659f, 784f, 1046f, 784f, 1046f, 1318f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.12f, notes[i], 0.3f, 0.24f, 0, 0.005f, 0.1f);
                    return Finish(name, b, 0.6f);
                }
                case "talk":
                {
                    var b = new float[(int)(0.12f * SampleRate)];
                    AddNote(b, 0f, 660f, 0.035f, 0.22f, 1);
                    AddNote(b, 0.045f, 880f, 0.05f, 0.18f, 1);
                    return Finish(name, b, 0.4f);
                }
                case "coin":
                {
                    var b = new float[(int)(0.28f * SampleRate)];
                    AddNote(b, 0f, 1046f, 0.06f, 0.22f, 0);
                    AddNote(b, 0.06f, 1568f, 0.14f, 0.22f, 0);
                    return Finish(name, b, 0.45f);
                }
                case "plant":
                {
                    var b = new float[(int)(0.2f * SampleRate)];
                    AddNote(b, 0f, 196f, 0.12f, 0.3f, 1);
                    AddNoise(b, 0f, 0.08f, 0.14f, 5f);
                    return Finish(name, b, 0.4f);
                }
                case "water":
                {
                    var b = new float[(int)(0.32f * SampleRate)];
                    AddNoise(b, 0f, 0.26f, 0.24f, 2.2f);
                    AddNote(b, 0.02f, 740f, 0.12f, 0.16f, 2);
                    AddNote(b, 0.1f, 560f, 0.16f, 0.14f, 2);
                    return Finish(name, b, 0.42f);
                }
                case "harvest":
                {
                    var b = new float[(int)(0.5f * SampleRate)];
                    float[] notes = { 659f, 784f, 1046f, 1318f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.075f, notes[i], 0.18f, 0.2f, 2, 0.004f, 0.08f);
                    return Finish(name, b, 0.5f);
                }
                case "heart":
                {
                    var b = new float[(int)(0.42f * SampleRate)];
                    AddNote(b, 0f, 784f, 0.1f, 0.2f, 2);
                    AddNote(b, 0.08f, 1046f, 0.22f, 0.2f, 2, 0.004f, 0.16f);
                    return Finish(name, b, 0.45f);
                }
                case "sleep":
                {
                    var b = new float[(int)(1.0f * SampleRate)];
                    float[] notes = { 523f, 440f, 392f, 262f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.22f, notes[i], 0.4f, 0.22f, 2, 0.02f, 0.14f);
                    return Finish(name, b, 0.5f);
                }
                case "error":
                {
                    var b = new float[(int)(0.24f * SampleRate)];
                    AddNote(b, 0f, 180f, 0.09f, 0.3f, 0);
                    AddNote(b, 0.1f, 130f, 0.12f, 0.28f, 0);
                    return Finish(name, b, 0.42f);
                }
                case "serve":
                {
                    var b = new float[(int)(0.2f * SampleRate)];
                    AddNote(b, 0f, 988f, 0.05f, 0.22f, 1);
                    AddNote(b, 0.05f, 1318f, 0.1f, 0.2f, 2);
                    return Finish(name, b, 0.42f);
                }
                case "date":
                {
                    var b = new float[(int)(1.3f * SampleRate)];
                    float[] notes = { 523f, 659f, 784f, 880f, 784f, 1046f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.18f, notes[i], 0.45f, 0.2f, 2, 0.01f, 0.2f);
                    return Finish(name, b, 0.5f);
                }
                case "confess":
                {
                    var b = new float[(int)(2.0f * SampleRate)];
                    float[] notes = { 523f, 659f, 784f, 1046f, 988f, 1046f, 1318f, 1568f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.16f, notes[i], 0.55f, 0.19f, 2, 0.01f, 0.22f);
                    return Finish(name, b, 0.55f);
                }
                case "festival":
                {
                    var b = new float[(int)(1.9f * SampleRate)];
                    float[] notes = { 523f, 659f, 784f, 1046f, 1318f, 1568f, 1318f, 1046f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.13f, notes[i], 0.42f, 0.2f, i % 2 == 0 ? 1 : 2,
                            0.004f, 0.2f);
                    AddNoise(b, 0f, 0.12f, 0.12f, 4f);
                    AddNoise(b, 0.7f, 0.2f, 0.1f, 3f);
                    return Finish(name, b, 0.62f);
                }
                case "discover":
                {
                    var b = new float[(int)(1.1f * SampleRate)];
                    float[] notes = { 659f, 784f, 988f, 1318f, 1568f };
                    for (int i = 0; i < notes.Length; i++)
                        AddNote(b, i * 0.11f, notes[i], 0.36f, 0.2f, 2, 0.006f, 0.2f);
                    return Finish(name, b, 0.55f);
                }
                default: return BuildExtraSfx(name);
            }
        }

        static AudioClip BuildMusic(int index)
        {
            float bpm = index == 0 ? 138f : index == 1 ? 104f : index == 2 ? 152f
                : index == 3 ? 116f : index == 4 ? 84f : 128f;
            float step = 60f / bpm / 2f; // eighth notes
            const int steps = 32;
            float duration = step * steps;
            var buffer = new float[Mathf.RoundToInt(duration * SampleRate) + SampleRate / 6];

            int[] melody;
            int[] bass;
            if (index == 0)
            {
                melody = new[]
                {
                    72, 76, 79, 76, 72, 76, 79, 84,
                    74, 77, 81, 77, 74, 77, 81, 79,
                    76, 79, 83, 79, 76, 79, 83, 86,
                    77, 81, 84, 81, 79, 76, 74, 72
                };
                bass = new[] { 48, 48, 53, 55, 48, 48, 57, 55 };
            }
            else if (index == 1)
            {
                melody = new[]
                {
                    69, 0, 72, 0, 76, 0, 72, 0,
                    69, 0, 72, 0, 74, 0, 71, 0,
                    67, 0, 71, 0, 74, 0, 71, 0,
                    69, 0, 72, 0, 76, 0, 79, 0
                };
                bass = new[] { 45, 41, 48, 40, 45, 41, 43, 40 };
            }
            else if (index == 3)
            {
                // sunny town theme
                melody = new[]
                {
                    72, 76, 79, 84, 79, 76, 74, 72,
                    77, 81, 84, 81, 79, 77, 76, 72,
                    74, 77, 81, 84, 81, 79, 77, 74,
                    76, 79, 83, 86, 84, 79, 76, 72
                };
                bass = new[] { 48, 53, 55, 53, 48, 53, 57, 55 };
            }
            else if (index == 4)
            {
                // quiet night theme
                melody = new[]
                {
                    69, 0, 72, 0, 76, 0, 72, 0,
                    67, 0, 71, 0, 74, 0, 71, 0,
                    69, 0, 72, 0, 76, 0, 79, 0,
                    77, 0, 74, 0, 72, 0, 69, 0
                };
                bass = new[] { 45, 45, 41, 43, 45, 45, 40, 45 };
            }
            else if (index == 5)
            {
                // festival theme: bright, busy and celebratory
                melody = new[]
                {
                    72, 76, 79, 84, 79, 84, 88, 84,
                    74, 77, 81, 86, 81, 86, 89, 86,
                    76, 79, 83, 88, 83, 88, 91, 88,
                    79, 83, 86, 91, 88, 84, 79, 76
                };
                bass = new[] { 48, 55, 50, 57, 53, 60, 55, 59 };
            }
            else
            {
                melody = new[]
                {
                    74, 74, 77, 74, 81, 79, 77, 74,
                    72, 72, 76, 72, 79, 77, 76, 72,
                    74, 74, 77, 79, 81, 81, 84, 81,
                    79, 77, 76, 74, 72, 74, 76, 77
                };
                bass = new[] { 38, 34, 41, 33, 38, 34, 36, 33 };
            }

            int leadWave = index == 1 || index == 4 ? 2 : index == 3 || index == 5 ? 1 : 0;
            float leadEcho = index == 1 ? 0.18f : index == 4 ? 0.24f : index == 3 ? 0.06f
                : index == 5 ? 0.10f : 0.05f;
            float leadVol = index == 4 ? 0.14f : index == 5 ? 0.18f : 0.16f;

            for (int i = 0; i < steps; i++)
            {
                float t = i * step;
                int note = melody[i];
                if (note > 0)
                {
                    float freq = MidiToFreq(note);
                    AddNote(buffer, t, freq, step * 2.4f, leadVol, leadWave, 0.006f, leadEcho);
                    if (index != 1 && index != 4 && i % 4 == 0)
                        AddNote(buffer, t, freq * 2f, step * 1.2f, 0.05f, 1);
                }

                int bassNote = bass[(i / 4) % bass.Length];
                if (i % 2 == 0)
                    AddNote(buffer, t, MidiToFreq(bassNote), step * 2.2f, 0.20f, 1, 0.004f);

                if (i % 2 == 1)
                    AddNoise(buffer, t, 0.035f, index == 4 ? 0.018f : 0.035f, 5f);
                if (index == 2 && i % 4 == 2)
                    AddNoise(buffer, t, 0.09f, 0.06f, 4f);
            }

            return Finish("music" + index, buffer, 0.78f);
        }

        static float MidiToFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }
}
