using System;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PixelAdventure.LifeSim.Pulang
{
    /// <summary>
    /// Nama tokoh utama. Semua teks (dialog, UI, quest, surat Ibu) menulis token {nama}
    /// dan memanggil <see cref="Format"/>, jadi nama tidak pernah ditulis mati di skrip.
    /// </summary>
    public static class PulangProfile
    {
        public const string DefaultName = "Zidan";
        public const string Token = "{nama}";
        public const int MaxLength = 12;

        static string _name = DefaultName;

        public static string Name => _name;
        public static event Action<string> NameChanged;

        /// <summary>Huruf dan spasi saja (font pixel 5x7), spasi ganda dirapikan, kosong jadi Zidan.</summary>
        public static string Sanitize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return DefaultName;

            var sb = new StringBuilder(MaxLength);
            bool lastSpace = true;
            foreach (char c in raw)
            {
                if (sb.Length >= MaxLength) break;
                if (char.IsLetter(c) && c < 128)
                {
                    // Huruf pertama tiap kata kapital: "budi santoso" -> "Budi Santoso"
                    sb.Append(lastSpace ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                    lastSpace = false;
                }
                else if (c == ' ' && !lastSpace)
                {
                    sb.Append(' ');
                    lastSpace = true;
                }
            }

            string result = sb.ToString().Trim();
            return result.Length == 0 ? DefaultName : result;
        }

        public static void SetName(string raw)
        {
            _name = Sanitize(raw);
            NameChanged?.Invoke(_name);
        }

        /// <summary>Ganti {nama} di teks apa pun. Dipakai HUD sebelum menampilkan dialog.</summary>
        public static string Format(string template)
        {
            return string.IsNullOrEmpty(template) ? template : template.Replace(Token, _name);
        }
    }

    /// <summary>
    /// Layar "Siapa namamu?" sebelum Prolog. Logika murni: HUD cukup menggambar
    /// <see cref="Display"/> lalu menunggu <see cref="Confirmed"/>.
    /// </summary>
    public class NameEntry
    {
        readonly StringBuilder _buffer = new StringBuilder(PulangProfile.MaxLength);

        public event Action<string> Confirmed;

        public string Buffer => _buffer.ToString();
        public bool IsEmpty => _buffer.Length == 0;

        /// <summary>Kosong menampilkan nama default sebagai placeholder.</summary>
        public string Display => IsEmpty ? PulangProfile.DefaultName : Buffer;

        public void Type(char c)
        {
            if (_buffer.Length >= PulangProfile.MaxLength) return;
            if ((char.IsLetter(c) && c < 128) || (c == ' ' && _buffer.Length > 0))
                _buffer.Append(c);
        }

        public void Backspace()
        {
            if (_buffer.Length > 0) _buffer.Length--;
        }

        public void UseDefault()
        {
            _buffer.Clear();
            Confirm();
        }

        public void Confirm()
        {
            PulangProfile.SetName(Buffer); // kosong -> "Zidan"
            Confirmed?.Invoke(PulangProfile.Name);
        }
    }

    /// <summary>Menyambungkan keyboard ke <see cref="NameEntry"/>. Aktifkan hanya selama layar nama tampil.</summary>
    public class NameEntryInput : MonoBehaviour
    {
        public NameEntry Entry { get; private set; }

        public void Begin(NameEntry entry)
        {
            Entry = entry;
            enabled = true;
        }

        void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) Keyboard.current.onTextInput += OnChar;
#endif
        }

        void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) Keyboard.current.onTextInput -= OnChar;
#endif
        }

        void Update()
        {
            if (Entry == null) return;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.backspaceKey.wasPressedThisFrame) Entry.Backspace();
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Finish(false);
            if (kb.escapeKey.wasPressedThisFrame) Finish(true);
#elif ENABLE_LEGACY_INPUT_MANAGER
            foreach (char c in Input.inputString)
            {
                if (c == '\b') Entry.Backspace();
                else if (c == '\n' || c == '\r') { Finish(false); return; }
                else OnChar(c);
            }
            if (Input.GetKeyDown(KeyCode.Escape)) Finish(true);
#endif
        }

        void OnChar(char c)
        {
            if (Entry != null && c != '\b' && c != '\n' && c != '\r') Entry.Type(c);
        }

        void Finish(bool useDefault)
        {
            var entry = Entry;
            Entry = null;
            enabled = false;
            if (useDefault) entry.UseDefault();
            else entry.Confirm();
        }
    }
}
