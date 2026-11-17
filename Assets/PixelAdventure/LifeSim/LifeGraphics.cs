using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Pengaturan grafik yang bisa disesuaikan untuk berbagai perangkat (HP kecil, tablet, PC).
    /// Disimpan di PlayerPrefs. Saat pertama kali dijalankan, nilai awal dipilih otomatis
    /// berdasarkan jenis perangkat dan ukuran layar.
    ///
    /// - FPS          : 30 / 60 / 120 / VSYNC
    /// - RESOLUSI     : 100% / 85% / 70% / 50% dari resolusi layar (lebih rendah = lebih ringan)
    /// - MODE LAYAR   : LAYAR PENUH / JENDELA (hanya PC)
    /// - ZOOM KAMERA  : DEKAT / NORMAL / JAUH / SANGAT JAUH
    /// - UKURAN UI    : 80% - 150%
    /// - EFEK         : PENUH / HEMAT (hewan kecil & debu langkah dimatikan)
    /// </summary>
    public static class LifeGraphics
    {
        const string Prefix = "Pulang.Gfx.";
        const int Version = 1;

        public static readonly int[] FpsValues = { 30, 60, 120, 0 };            // 0 = VSync
        public static readonly float[] ResolutionValues = { 1f, 0.85f, 0.7f, 0.5f };
        public static readonly float[] ZoomValues = { 6.5f, 8f, 9.5f, 11f };    // ukuran kamera ortografis
        public static readonly string[] ZoomNames = { "DEKAT", "NORMAL", "JAUH", "SANGAT JAUH" };
        public static readonly float[] UiValues = { 0.8f, 0.9f, 1f, 1.1f, 1.2f };

        public const float BaseCameraSize = 8f;   // ukuran kamera tempat tata letak HUD dirancang

        public static int FpsIndex { get; private set; }
        public static int ResolutionIndex { get; private set; }
        public static int ZoomIndex { get; private set; }
        public static int UiIndex { get; private set; }
        public static bool Fullscreen { get; private set; }
        public static bool LowEffects { get; private set; }

        public static float CameraSize => ZoomValues[ZoomIndex];
        /// <summary>
        /// Ukuran UI yang dipakai: pilihan pemain, dibatasi agar panel HUD (dirancang untuk area
        /// 16 x 28 unit) tetap muat di layar sempit seperti tablet 4:3.
        /// </summary>
        public static float UiScale => SnapToPixels(RawUiScale, UiScaleLimit);

        /// <summary>Pilihan pemain setelah dibatasi ukuran layar (belum dibulatkan ke piksel).</summary>
        static float RawUiScale => Mathf.Min(UiValues[UiIndex], UiScaleLimit);

        static float UiScaleLimit
        {
            get
            {
                float aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 16f / 9f;
                float maxByHeight = BaseCameraSize / 6.6f;            // tinggi area HUD minimal 13,2 unit
                float maxByWidth = BaseCameraSize * aspect / 10.5f;   // lebar area HUD minimal 21 unit
                return Mathf.Min(maxByHeight, Mathf.Max(0.8f, maxByWidth));
            }
        }

        /// <summary>
        /// Bulatkan skala UI agar satu piksel font (1/PPU unit HUD) = bilangan bulat piksel layar.
        /// Tanpa ini (mis. 1080p, UI 100% = 4,22 piksel layar per piksel font) goresan huruf
        /// berukuran tidak sama - sebagian 4 piksel, sebagian 5 - sehingga teks tampak bergerigi
        /// dan melelahkan dibaca. Dibulatkan ke yang terdekat, tetapi tidak melewati batas layar.
        /// </summary>
        static float SnapToPixels(float scale, float limit)
        {
            int h = Screen.height;
            if (h <= 0) return scale;
            float unitPx = 2f * BaseCameraSize * PixelArt.PPU;   // piksel font setinggi area HUD pada skala 1
            float k = h * scale / unitPx;                        // piksel layar per piksel font
            if (k < 1.5f) return scale;                          // layar sangat kecil: biarkan
            int kr = Mathf.RoundToInt(k);
            if (kr * unitPx / h > limit + 0.0001f) kr = Mathf.FloorToInt(k);
            return kr >= 1 ? kr * unitPx / h : scale;
        }

        /// <summary>PC / laptop: mode layar bisa diganti. Di HP & tablet selalu layar penuh.</summary>
        public static bool CanChangeWindowMode => !Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer;

        static bool _loaded;
        static int _nativeW, _nativeH;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntime() { _loaded = false; }

        // ------------------------------------------------------------------ muat & simpan

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            _nativeW = Display.main != null ? Display.main.systemWidth : Screen.currentResolution.width;
            _nativeH = Display.main != null ? Display.main.systemHeight : Screen.currentResolution.height;
            if (_nativeW <= 0 || _nativeH <= 0) { _nativeW = Screen.width; _nativeH = Screen.height; }

            if (PlayerPrefs.GetInt(Prefix + "Version", 0) != Version) ApplyDeviceDefaults();
            else
            {
                FpsIndex = Clamp(PlayerPrefs.GetInt(Prefix + "Fps", 1), FpsValues.Length);
                ResolutionIndex = Clamp(PlayerPrefs.GetInt(Prefix + "Res", 0), ResolutionValues.Length);
                ZoomIndex = Clamp(PlayerPrefs.GetInt(Prefix + "Zoom", 1), ZoomValues.Length);
                UiIndex = Clamp(PlayerPrefs.GetInt(Prefix + "Ui", 2), UiValues.Length);
                Fullscreen = PlayerPrefs.GetInt(Prefix + "Full", 1) == 1;
                LowEffects = PlayerPrefs.GetInt(Prefix + "Low", 0) == 1;
            }
        }

        static void Save()
        {
            PlayerPrefs.SetInt(Prefix + "Version", Version);
            PlayerPrefs.SetInt(Prefix + "Fps", FpsIndex);
            PlayerPrefs.SetInt(Prefix + "Res", ResolutionIndex);
            PlayerPrefs.SetInt(Prefix + "Zoom", ZoomIndex);
            PlayerPrefs.SetInt(Prefix + "Ui", UiIndex);
            PlayerPrefs.SetInt(Prefix + "Full", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "Low", LowEffects ? 1 : 0);
            PlayerPrefs.Save();
        }

        static int Clamp(int i, int count) => Mathf.Clamp(i, 0, count - 1);

        /// <summary>Ukuran layar fisik (inci, diagonal). -1 bila perangkat tidak melaporkan DPI.</summary>
        static float ScreenInches()
        {
            float dpi = Screen.dpi;
            if (dpi <= 1f) return -1f;
            float w = Screen.width, h = Screen.height;
            return Mathf.Sqrt(w * w + h * h) / dpi;
        }

        /// <summary>Nilai awal yang cocok untuk perangkat ini (juga dipakai tombol "SETEL OTOMATIS").</summary>
        public static void ApplyDeviceDefaults()
        {
            if (!_loaded) Load();
            bool mobile = Application.isMobilePlatform;
            float inches = ScreenInches();
            bool phone = mobile && (inches < 0f || inches < 7.5f);
            int pixels = Mathf.Max(_nativeW, _nativeH);

            if (mobile)
            {
                FpsIndex = 1;                                                  // 60 FPS
                ResolutionIndex = pixels > 2400 ? 2 : pixels > 1800 ? 1 : 0;   // layar sangat tajam: turunkan resolusi
                ZoomIndex = phone ? 0 : 1;                                     // HP: kamera lebih dekat
                UiIndex = phone ? 4 : 3;                                       // HP: UI lebih besar agar mudah diketuk
                LowEffects = SystemInfo.systemMemorySize > 0 && SystemInfo.systemMemorySize < 3000;
                Fullscreen = true;
            }
            else
            {
                FpsIndex = 3;                                                  // VSync
                ResolutionIndex = 0;
                ZoomIndex = 1;
                UiIndex = pixels >= 3000 ? 3 : 2;
                LowEffects = false;
                Fullscreen = Screen.fullScreen;
            }
            Save();
        }

        // ------------------------------------------------------------------ ubah nilai

        /// <param name="step">+1 = pilihan berikutnya, -1 = pilihan sebelumnya (berputar).</param>
        public static void CycleFps(int step = 1) { FpsIndex = Step(FpsIndex, step, FpsValues.Length); Changed(true); }
        public static void CycleResolution(int step = 1) { ResolutionIndex = Step(ResolutionIndex, step, ResolutionValues.Length); Changed(true); }
        public static void CycleZoom(int step = 1) { ZoomIndex = Step(ZoomIndex, step, ZoomValues.Length); Changed(false); }
        public static void CycleUi(int step = 1) { UiIndex = Step(UiIndex, step, UiValues.Length); Changed(false); }

        static int Step(int index, int step, int count) => ((index + step) % count + count) % count;
        public static void ToggleFullscreen() { Fullscreen = !Fullscreen; Changed(true); }
        public static void ToggleEffects() { LowEffects = !LowEffects; Changed(false); }

        public static void ResetToDevice()
        {
            ApplyDeviceDefaults();
            Apply(true);
        }

        static void Changed(bool display)
        {
            Save();
            Apply(display);
        }

        // ------------------------------------------------------------------ terapkan

        /// <param name="display">Juga terapkan resolusi & mode layar (bisa membuat layar berkedip sesaat).</param>
        public static void Apply(bool display)
        {
            if (!_loaded) Load();

            int fps = FpsValues[FpsIndex];
            QualitySettings.vSyncCount = fps == 0 ? 1 : 0;
            Application.targetFrameRate = fps == 0 ? -1 : fps;

            Camera cam = Camera.main;
            if (cam != null) cam.orthographicSize = CameraSize;

            if (!display || Application.isEditor) return;

            float s = ResolutionValues[ResolutionIndex];
            if (CanChangeWindowMode && !Fullscreen)
            {
                // Jendela: 75% layar (dikali skala resolusi tidak perlu; jendela tetap tajam).
                int w = Mathf.RoundToInt(_nativeW * 0.75f), h = Mathf.RoundToInt(_nativeH * 0.75f);
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
            }
            else
            {
                int w = Mathf.Max(320, Mathf.RoundToInt(_nativeW * s));
                int h = Mathf.Max(240, Mathf.RoundToInt(_nativeH * s));
                Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
            }
        }

        // ------------------------------------------------------------------ teks label

        public static string FpsLabel => FpsValues[FpsIndex] == 0 ? "VSYNC" : FpsValues[FpsIndex] + " FPS";
        public static string ResolutionLabel
        {
            get
            {
                if (CanChangeWindowMode && !Fullscreen) return "IKUT JENDELA";
                float s = ResolutionValues[ResolutionIndex];
                return Mathf.RoundToInt(s * 100f) + "% (" + Mathf.RoundToInt(_nativeW * s) + "X" + Mathf.RoundToInt(_nativeH * s) + ")";
            }
        }
        public static string ZoomLabel => ZoomNames[ZoomIndex];
        public static string UiLabel
        {
            get
            {
                int want = Mathf.RoundToInt(UiValues[UiIndex] * 100f);
                int max = Mathf.RoundToInt(RawUiScale * 100f);
                int used = Mathf.RoundToInt(UiScale * 100f);
                if (max < want) return want + "% (MAKS " + used + "%)";
                return used != want ? want + "% (TAJAM " + used + "%)" : want + "%";
            }
        }
        /// <summary>Jenis perangkat + resolusi asli layar, mis. "PC 1920X1080".</summary>
        public static string DeviceLabel
        {
            get
            {
                if (!_loaded) Load();
                float inches = ScreenInches();
                string kind = !Application.isMobilePlatform ? "PC" : inches < 7.5f ? "HP" : "TABLET";   // tanpa DPI (-1) = HP
                return kind + " " + _nativeW + "X" + _nativeH;
            }
        }
        public static string WindowLabel => Fullscreen ? "LAYAR PENUH" : "JENDELA";
        public static string EffectsLabel => LowEffects ? "HEMAT" : "PENUH";
    }
}
