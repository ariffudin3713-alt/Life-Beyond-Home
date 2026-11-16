using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Isi layar ketik jumlah uang (dibuat ulang oleh PulangGame setiap kali isinya berubah).</summary>
    public class AmountView
    {
        public string Title = "MENABUNG";
        public string Amount = "";                 // jumlah terketik, sudah berformat ("RP150.000"); kosong = belum ada
        public string LeftLabel = "UANGMU", LeftNow = "", LeftAfter = "";
        public string RightLabel = "TABUNGAN", RightNow = "", RightAfter = "";
        public string Message = "";
        public bool MessageError;
        public bool CanConfirm;
        public string ConfirmLabel = "SIMPAN";
        public readonly List<string> Chips = new List<string>();
        public readonly List<bool> ChipOn = new List<bool>();
    }

    /// <summary>
    /// Layar ketik jumlah uang (dipakai MENABUNG di kost), tersusun dari atas ke bawah:
    /// - judul;
    /// - dua kolom: UANGMU dan TABUNGAN (sekarang + "SETELAH" sebagai pratinjau);
    /// - kotak jumlah besar dengan kursor berkedip pelan;
    /// - baris pesan (target tabungan / peringatan uang tidak cukup);
    /// - tombol cepat (10RB, 50RB, ..., SEMUA);
    /// - papan angka 3 x 4 untuk mouse / layar sentuh (1-9, 000, 0, HAPUS);
    /// - tombol BATAL & SIMPAN, lalu petunjuk keyboard.
    /// Indeks papan angka: 0-8 = angka 1-9, 9 = "000", 10 = "0", 11 = HAPUS. Tombol: 0 = BATAL, 1 = SIMPAN.
    /// </summary>
    public partial class LifeHud
    {
        public static readonly string[] AmountKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "000", "0", "HAPUS" };

        static readonly Color AmCard = new Color(0.10f, 0.08f, 0.14f, 1f);
        static readonly Color AmBox = new Color(0.05f, 0.04f, 0.08f, 1f);
        static readonly Color AmKey = new Color(0.20f, 0.16f, 0.28f, 1f);
        static readonly Color AmKeyOff = new Color(0.13f, 0.11f, 0.17f, 1f);
        static readonly Color AmOk = new Color(0.36f, 0.62f, 0.34f, 1f);

        Transform _amGroup;
        UiPool _aui;
        AmountView _am = new AmountView();

        void BuildAmount()
        {
            _amGroup = Group("Amount");
            _aui = new UiPool(_amGroup, 4010);
            _amGroup.gameObject.SetActive(false);
        }

        public bool AmountVisible => _amGroup != null && _amGroup.gameObject.activeSelf;

        public void ShowAmount(AmountView view)
        {
            _am = view ?? new AmountView();
            SetDim(0.8f);
            _amGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideAmount()
        {
            if (_amGroup != null) _amGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        void LayoutAmount(float halfW, float halfH)
        {
            if (!AmountVisible) return;
            var ui = _aui;
            var v = _am;
            ui.Begin();

            float w = Mathf.Min(halfW * 2f - 1f, 17f);
            float keyH = 0.86f, keyGap = 0.16f;
            float want = 1.35f + 1.55f + 1.75f + 0.75f + 0.95f + 4 * keyH + 3 * keyGap + 0.35f + 1.15f + 0.75f;
            float h = Mathf.Min(halfH * 2f - 0.8f, want);
            float top = h * 0.5f, left = -w * 0.5f;
            float innerL = left + 0.6f, innerW = w - 1.2f, innerR = innerL + innerW;
            ui.Panel("lc_panel_gold", 0f, 0f, w, h, 0);
            CloseAt(w * 0.5f, top, 3);

            float y = top - 0.72f;
            ui.Text(Safe(v.Title), innerL, y, Gold, 2);
            y -= 0.75f;

            // ---------------- dua kolom: sekarang & sesudah
            float colW = (innerW - 0.3f) * 0.5f;
            AmountColumn(ui, innerL, colW, y, v.LeftLabel, v.LeftNow, v.LeftAfter);
            AmountColumn(ui, innerL + colW + 0.3f, colW, y, v.RightLabel, v.RightNow, v.RightAfter);
            y -= 1.55f;

            // ---------------- kotak jumlah
            float boxH = 1.35f, boxCy = y - boxH * 0.5f;
            bool empty = string.IsNullOrEmpty(v.Amount);
            ui.Solid(0f, boxCy, innerW, boxH, v.MessageError ? Pink : Gold, 1);
            ui.Solid(0f, boxCy, innerW - 0.12f, boxH - 0.12f, AmBox, 2);
            ui.Text("JUMLAH", innerL + 0.3f, boxCy + boxH * 0.5f - 0.32f, Soft, 1, 0f, 3);
            string shown = empty ? "RP0" : v.Amount;
            int scale = shown.Length * UiCharW * 2f <= innerW - 1.2f ? 2 : 1;
            var amt = ui.Text(shown, 0f, boxCy - 0.12f, empty ? Soft : Ink, scale, 0.5f, 3);
            // Kursor diam-berkedip pelan (tidak menyilaukan) di kanan angka.
            if (Mathf.Repeat(_time, 1.2f) < 0.75f && amt.sprite != null)
            {
                float cx = amt.sprite.bounds.size.x * 0.5f + 0.18f;
                ui.Solid(cx, boxCy - 0.12f, 0.1f, scale == 2 ? 0.8f : 0.45f, Gold, 3);
            }
            y -= boxH + 0.4f;

            // ---------------- pesan
            ui.Text(CompactHint(v.Message, CharsFor(innerW)), 0f, y, v.MessageError ? Pink : Soft, 1, 0.5f);
            y -= 0.75f;

            // ---------------- tombol cepat
            int n = v.Chips.Count;
            if (n > 0)
            {
                const float chipGap = 0.2f, chipH = 0.62f;
                float chipW = (innerW - chipGap * (n - 1)) / n;
                for (int i = 0; i < n; i++)
                {
                    float cx = innerL + chipW * 0.5f + i * (chipW + chipGap);
                    bool on = i < v.ChipOn.Count && v.ChipOn[i];
                    ui.Solid(cx, y, chipW, chipH, on ? AmKey : AmKeyOff, 1);
                    ui.Text(Fit(v.Chips[i], CharsFor(chipW - 0.2f)), cx, y, on ? Gold : WmLocked, 1, 0.5f);
                    if (on) Hit(UiHit.AmountChip, i, RectAt(cx, y, chipW, chipH), 3);
                }
            }
            y -= 0.95f;

            // ---------------- papan angka 3 x 4
            float keyW = Mathf.Min(2.6f, (innerW - 2 * keyGap) / 3f);
            float padW = keyW * 3f + keyGap * 2f;
            float kx0 = -padW * 0.5f + keyW * 0.5f;
            float ky0 = y - keyH * 0.5f;
            for (int k = 0; k < AmountKeys.Length; k++)
            {
                int col = k % 3, row = k / 3;
                float kx = kx0 + col * (keyW + keyGap), ky = ky0 - row * (keyH + keyGap);
                bool del = k == 11;
                ui.Solid(kx, ky, keyW, keyH, del ? new Color(0.42f, 0.18f, 0.22f, 1f) : AmKey, 1);
                ui.Text(AmountKeys[k], kx, ky, del ? Ink : Gold, 1, 0.5f);
                Hit(UiHit.AmountKey, k, RectAt(kx, ky, keyW, keyH), 3);
            }
            y -= 4 * keyH + 3 * keyGap + 0.35f;

            // ---------------- BATAL & SIMPAN
            float bw = (innerW - 0.3f) * 0.5f, bh = 0.9f, by = y - bh * 0.5f;
            float bl = innerL + bw * 0.5f, br = innerR - bw * 0.5f;
            ui.Solid(bl, by, bw, bh, AmKey, 1);
            ui.Text("BATAL", bl, by, Ink, 1, 0.5f);
            Hit(UiHit.AmountButton, 0, RectAt(bl, by, bw, bh), 3);
            ui.Solid(br, by, bw, bh, v.CanConfirm ? AmOk : AmKeyOff, 1);
            ui.Text(Safe(v.ConfirmLabel), br, by, v.CanConfirm ? Ink : WmLocked, 1, 0.5f);
            if (v.CanConfirm) Hit(UiHit.AmountButton, 1, RectAt(br, by, bw, bh), 3);

            ui.Text(CompactHint("KETIK ANGKA   BACKSPACE - HAPUS   ENTER - SIMPAN   ESC - BATAL", CharsFor(innerW)),
                0f, -top + 0.42f, Gold, 1, 0.5f);
            ui.End();
        }

        void AmountColumn(UiPool ui, float x, float width, float top, string label, string now, string after)
        {
            int chars = CharsFor(width - 0.1f);
            ui.Text(Fit(label, chars), x, top, Soft);
            ui.Text(Fit(now, chars), x, top - 0.62f, Ink);
            bool changed = !string.IsNullOrEmpty(after) && after != now;
            ui.Text(Fit(changed ? "> " + after : " ", chars), x, top - 1.24f, Gold);   // "> " = sesudah disimpan
        }
    }
}
