using System;
using System.Collections.Generic;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Layar PENGATURAN (grafik + suara). Dibuka dari aplikasi GRAFIK di tablet
    /// atau tombol PENGATURAN di menu utama. Tampilan: lihat LifeHudSettings.
    /// - W/S atau ketuk: pilih baris; A/D atau ketuk ◀ ▶: ganti nilai (langsung diterapkan & disimpan);
    /// - E pada baris = nilai berikutnya; E pada tombol = jalankan tombol;
    /// - ESC / X / KEMBALI: tutup.
    /// </summary>
    public partial class PulangGame
    {
        Action _settingsClose;
        int _settingsSel;
        readonly List<Action<int>> _settingsChange = new List<Action<int>>();

        void OpenSettingsMenu(Action onClose, int selected = 0)
        {
            _settingsClose = onClose;
            _settingsSel = selected;
            _state = State.Settings;
            _hud.SetPrompt(null);
            GameAudio.Play("menu_open", 0.3f);
            RefreshSettings();
            _inputLock = 0.2f;
        }

        void RefreshSettings()
        {
            var rows = new List<SettingRow>();
            _settingsChange.Clear();

            void Row(string section, string label, string value, int count, int index, string help, Action<int> change)
            {
                rows.Add(new SettingRow { Section = section, Label = label, Value = value, Count = count, Index = index, Help = help });
                _settingsChange.Add(change);
            }

            const string screen = "LAYAR", view = "TAMPILAN", perf = "PERFORMA & SUARA";
            Row(screen, "FPS", LifeGraphics.FpsLabel, LifeGraphics.FpsValues.Length, LifeGraphics.FpsIndex,
                "Gambar per detik. Lebih tinggi = gerakan lebih halus, tetapi baterai lebih boros. VSYNC mengikuti layar.",
                LifeGraphics.CycleFps);
            Row(screen, "RESOLUSI", LifeGraphics.ResolutionLabel, LifeGraphics.ResolutionValues.Length, LifeGraphics.ResolutionIndex,
                "Resolusi lebih rendah = lebih ringan untuk HP lama, tetapi gambar sedikit kurang tajam.",
                LifeGraphics.CycleResolution);
            if (LifeGraphics.CanChangeWindowMode)
                Row(screen, "MODE LAYAR", LifeGraphics.WindowLabel, 2, LifeGraphics.Fullscreen ? 0 : 1,
                    "Layar penuh atau jendela (hanya di PC / laptop).",
                    _ => LifeGraphics.ToggleFullscreen());

            Row(view, "ZOOM KAMERA", LifeGraphics.ZoomLabel, LifeGraphics.ZoomValues.Length, LifeGraphics.ZoomIndex,
                "Seberapa jauh kamera melihat dunia. DEKAT cocok untuk HP, JAUH untuk layar besar.",
                step => { LifeGraphics.CycleZoom(step); _cameraRig?.Snap(); });
            Row(view, "UKURAN UI", LifeGraphics.UiLabel, LifeGraphics.UiValues.Length, LifeGraphics.UiIndex,
                "Besar panel dan tulisan. Dibatasi otomatis agar semua panel tetap muat di layar ini.",
                LifeGraphics.CycleUi);

            Row(perf, "EFEK", LifeGraphics.EffectsLabel, 2, LifeGraphics.LowEffects ? 1 : 0,
                "HEMAT mematikan hewan kecil dan debu langkah agar permainan lebih ringan.",
                _ => LifeGraphics.ToggleEffects());
            Row(perf, "SUARA", GameAudio.Muted ? "MATI" : "NYALA", 2, GameAudio.Muted ? 1 : 0,
                "Musik dan efek suara. Bisa juga dengan tombol M kapan saja.",
                _ => GameAudio.ToggleMute());

            _settingsSel = UnityEngine.Mathf.Clamp(_settingsSel, 0, rows.Count + 1);
            _hud.ShowSettings(rows, _settingsSel, LifeGraphics.DeviceLabel);
        }

        void UpdateSettings()
        {
            if (_inputLock > 0f) return;
            int rows = _settingsChange.Count;
            int auto = rows, back = rows + 1;

            if (LifeInput.CancelPressed) { CloseSettings(); return; }

            // Kursor: arahkan = pilih baris / tombol.
            if (_hud.PointerHover(UiHit.SettingsRow, out int hover) && hover != _settingsSel && hover <= back)
            {
                _settingsSel = hover;
                GameAudio.Play("click", 0.25f);
                RefreshSettings();
                return;
            }

            int tapped = _hud.SettingsTapped(out int tapStep);
            if (tapped >= 0)
            {
                // Ketuk baris yang sudah dipilih / panah = ganti nilai; ketuk tombol = jalankan.
                bool act = tapped >= rows || tapped == _settingsSel || tapStep < 0;
                _settingsSel = tapped;
                if (act) ApplySetting(tapped, tapStep);
                else { GameAudio.Play("click", 0.35f); RefreshSettings(); }
                return;
            }

            int move = LifeInput.UpPressed ? -1 : LifeInput.DownPressed ? 1 : 0;
            if (move != 0)
            {
                // Kedua tombol sejajar dianggap satu baris di bawah daftar:
                //   naik : baris i -> i-1, baris pertama -> tombol SETEL OTOMATIS, tombol -> baris terakhir;
                //   turun: baris i -> i+1, baris terakhir -> tombol SETEL OTOMATIS, tombol -> baris pertama.
                if (move < 0) _settingsSel = _settingsSel >= rows ? rows - 1 : _settingsSel == 0 ? auto : _settingsSel - 1;
                else _settingsSel = _settingsSel >= rows ? 0 : _settingsSel + 1;
                GameAudio.Play("click", 0.35f);
                RefreshSettings();
                return;
            }

            int step = LifeInput.LeftPressed ? -1 : LifeInput.RightPressed ? 1 : 0;
            if (step != 0)
            {
                if (_settingsSel < rows) ApplySetting(_settingsSel, step);
                else
                {
                    _settingsSel = _settingsSel == auto ? back : auto;
                    GameAudio.Play("click", 0.35f);
                    RefreshSettings();
                }
                return;
            }

            if (LifeInput.InteractPressed) ApplySetting(_settingsSel, 1);
        }

        void ApplySetting(int index, int step)
        {
            int rows = _settingsChange.Count;
            if (index == rows + 1) { CloseSettings(); return; }
            if (index == rows) LifeGraphics.ResetToDevice();
            else _settingsChange[index](step);
            GameAudio.Play("confirm", 0.4f);
            RefreshSettings();
            _inputLock = 0.08f;
        }

        void CloseSettings()
        {
            _hud.HideSettings();
            GameAudio.Play("click", 0.4f);
            _state = State.Playing;
            _inputLock = 0.15f;
            var back = _settingsClose;
            _settingsClose = null;
            back?.Invoke();
        }
    }
}
