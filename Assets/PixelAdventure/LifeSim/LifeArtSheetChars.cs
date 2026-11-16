using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Mengganti sprite karakter prosedural dengan spritesheet "74 karakter bernama"
    /// (Resources/Characters/karakter_74.bytes, PNG 64x2072, frame 16x28).
    /// Tiap baris = satu karakter; kolom = down, left, right, up (urutan sheet).
    /// Sheet hanya punya satu pose per arah, jadi frame jalan 1 & 2 dibuat dengan
    /// mengangkat kaki kiri / kanan bergantian. Bila aset tidak ada, sprite prosedural
    /// lama tetap dipakai.
    /// </summary>
    public static partial class LifeArt
    {
        const string SheetResource = "Characters/karakter_74";
        const int SheetFrameW = 16;
        const int SheetFrameH = 28;
        const int SheetLegRows = 6;

        // Kolom pada sheet untuk tiap arah game (game: down, up, left, right).
        static readonly int[] SheetColumnForDir = { 0, 3, 1, 2 };

        /// <summary>
        /// Kunci sprite game -> nomor baris di sheet (0 = karakter nomor 1).
        /// Dicocokkan berdasarkan peran di game (pakaian & profesi), lalu nama.
        /// Semua baris yang tidak disebut di sini otomatis menjadi warga pejalan kaki
        /// ("warga1".."wargaN"), jadi seluruh 74 karakter terpakai.
        /// </summary>
        static readonly Dictionary<string, int> SheetRows = new Dictionary<string, int>
        {
            // Tokoh utama & keluarga
            { "player", 0 },        // 1. Zidan (tokoh utama)
            { "ratna", 1 },         // 2. Ratna (ibu Zidan)
            { "keluarga", 4 },      // 5. Pak Joko (kakek Zidan) — penelepon dari keluarga

            // Lima karakter perempuan
            { "nabila", 5 },        // 6. Nabila
            { "kirana", 6 },        // 7. Kirana
            { "anindya", 7 },       // 8. Anindya
            { "melati", 8 },        // 9. Melati
            { "citra", 9 },         // 10. Citra

            // NPC pendukung
            { "hendra", 2 },        // 3. Hendra (paman, memperkenalkan kota)
            { "dimas", 3 },         // 4. Dimas (sepupu, teman pertama di kota)
            { "busari", 11 },       // 12. Bu Sari (warung)
            { "joko", 53 },         // 54. Pak Yanto (pemilik bengkel) — Bengkel Jaya Motor
            { "andi", 68 },         // 69. Dokter Arman — petugas rumah sakit
            { "budi", 38 },         // 39. Pak Bima (manajer) — urusan pekerjaan
            { "rina", 25 },         // 26. Rina (warga kota)
            { "darto", 50 },        // 51. Pak Darto (penjual sayur) — pedagang pasar
            { "riko", 51 },         // 52. Riko

            // NPC warga
            { "sastro", 10 },       // 11. Pak Slamet (petani bercaping) — petani sawah
            { "rt", 18 },           // 19. Pak Lurah Surya (kepala desa) — ketua RT
            { "herman", 48 },       // 49. Pak Mulyono (pemilik toko) — Toko Anugerah
            { "slamet", 45 },       // 46. Yudha (satpam) — penjaga pos ronda
            { "fajar", 21 },        // 22. Fajar (pemuda)
            { "karman", 62 },       // 63. Pak Basuki (topi lapangan) — peternak
            { "rukmini", 46 },      // 47. Bu Wati (pemilik warung) — toko kelontong
            { "hadi", 64 },         // 65. Hadi — petugas loket terminal
            { "rudi", 14 },         // 15. Rudi (pemuda desa) — pedagang sayur
            { "surya", 58 },        // 59. Joko (baju oranye) — petugas kebersihan
            { "siti", 49 },         // 50. Bu Tini (penjual makanan) — warung makan
            { "pustakawan", 42 },   // 43. Sinta (admin) — penjaga perpustakaan
            { "tukangkebun", 12 },  // 13. Pak Karto (sesepuh desa) — tukang kebun

            // Penjaga ruangan dalam (lihat StaffSprite)
            { "staff_shop", 54 },        // 55. Aldi (penjual minuman)
            { "staff_cafe", 55 },        // 56. Seno (pemilik kafe)
            { "staff_inn", 73 },         // 74. Pak Darwis (pemilik penginapan)
            { "staff_office", 40 },      // 41. Fikri (rekan kerja)
            { "staff_office2", 39 },     // 40. Bu Rani (supervisor)
            { "staff_library", 61 },     // 62. Dewi
            { "staff_workshop", 47 },    // 48. Pak Herman (montir)
            { "staff_dealer", 43 },      // 44. Rendi (marketing)
            { "staff_electronics", 41 }, // 42. Dito (programmer)
            { "staff_books", 35 },       // 36. Farhan
            { "staff_clothes", 26 },     // 27. Maya
            { "staff_flowers", 65 },     // 66. Lestari
            { "staff_grocery", 63 },     // 64. Bu Fitri
            { "staff_fishing", 23 },     // 24. Bagas
            { "staff_hospital", 69 },    // 70. Perawat Sella
            { "staff_school", 67 },      // 68. Bu Ratih (guru)
            { "staff_balai", 66 },       // 67. Pak Wiryo (tokoh masyarakat)
            { "staff_mosque", 56 },      // 57. Pak Surono
            { "staff_church", 44 },      // 45. Pak Anton

            // Anak-anak di taman
            { "anak1", 16 },        // 17. Budi (anak desa)
            { "anak2", 59 },        // 60. Rafi (anak)
            { "anak3", 60 },        // 61. Nanda (anak)
        };

        /// <summary>Jumlah sprite warga pejalan kaki ("warga1".."wargaN").</summary>
        public static int WargaCount { get; private set; } = 6;

        /// <summary>Kunci sprite warga acak (tanpa awalan "lc_").</summary>
        public static string RandomWarga() => "warga" + Random.Range(1, WargaCount + 1);

        /// <summary>Sprite penjaga ruangan dalam (kunci lengkap, "_down_0").</summary>
        public static string StaffSprite(InteriorKind kind, int fallbackIndex, bool second = false)
        {
            string key = kind == InteriorKind.Office && second ? "staff_office2"
                : "staff_" + kind.ToString().ToLowerInvariant();
            string full = Prefix + key + "_down_0";
            if (PixelArt.Sprites.ContainsKey(full)) return full;
            return Prefix + "warga" + (1 + (fallbackIndex % 6)) + "_down_0";
        }

        static void ApplyCharacterSheet()
        {
            var asset = Resources.Load<TextAsset>(SheetResource);
            if (asset == null)
            {
                Debug.LogWarning("[LifeArt] Spritesheet karakter tidak ditemukan: " + SheetResource);
                return;
            }

            var sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!sheet.LoadImage(asset.bytes))
            {
                Debug.LogWarning("[LifeArt] Gagal membaca spritesheet karakter.");
                return;
            }

            Color32[] src = sheet.GetPixels32();
            int sheetW = sheet.width;
            int sheetH = sheet.height;
            int rows = sheetH / SheetFrameH;
            if (Application.isPlaying) Object.Destroy(sheet); else Object.DestroyImmediate(sheet);
            _sheetPixels = src;
            _sheetW = sheetW;
            _sheetH = sheetH;

            // Semua karakter yang belum punya peran menjadi warga pejalan kaki.
            var cast = new List<KeyValuePair<string, int>>(SheetRows);
            var used = new HashSet<int>(SheetRows.Values);
            int warga = 0;
            for (int r = 0; r < rows; r++)
            {
                if (used.Contains(r)) continue;
                warga++;
                cast.Add(new KeyValuePair<string, int>("warga" + warga, r));
            }
            if (warga >= 6) WargaCount = warga;

            foreach (var pair in cast)
            {
                int row = pair.Value;
                if (row < 0 || row >= rows) continue;

                for (int dir = 0; dir < 4; dir++)
                {
                    int col = SheetColumnForDir[dir];
                    // Baris 0 ada di atas gambar; tekstur Unity mulai dari bawah.
                    int ox = col * SheetFrameW;
                    int oy = sheetH - (row + 1) * SheetFrameH;

                    for (int frame = 0; frame < 3; frame++)
                    {
                        var c = new PixelCanvas(SheetFrameW, SheetFrameH);
                        for (int y = 0; y < SheetFrameH; y++)
                            for (int x = 0; x < SheetFrameW; x++)
                                c.Set(x, y, src[(oy + y) * sheetW + ox + x]);

                        if (frame == 1) LiftLeg(c, 0, SheetFrameW / 2);
                        else if (frame == 2) LiftLeg(c, SheetFrameW / 2, SheetFrameW);

                        Add(c, new Vector2(0.5f, 0f), pair.Key + "_" + DirNames[dir] + "_" + frame);
                    }
                }
            }
        }

        // ------------------------------------------------------------------ pakaian tokoh utama

        static Color32[] _sheetPixels;
        static int _sheetW, _sheetH;
        static int _playerOutfitRow = -1;

        /// <summary>Baris kepala tokoh utama di sheet (1. Zidan).</summary>
        const int PlayerSheetRow = 0;
        /// <summary>Baris gambar (dari atas) terakhir yang termasuk kepala & leher; di bawahnya badan.</summary>
        const int HeadLastImageRow = 12;

        static Color32 SheetPixel(int row, int col, int x, int imageY)
        {
            int ox = col * SheetFrameW;
            int oy = _sheetH - (row + 1) * SheetFrameH;
            int y = SheetFrameH - 1 - imageY;   // tekstur Unity mulai dari bawah
            return _sheetPixels[(oy + y) * _sheetW + ox + x];
        }

        static bool Same(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

        /// <summary>
        /// Kepala tokoh utama + badan (pakaian) dari karakter lain. Warna kulit badan donor
        /// (tangan, kaki bercelana pendek) diganti warna kulit tokoh utama.
        /// </summary>
        static PixelCanvas OutfitCanvas(int bodyRow, int col)
        {
            var c = new PixelCanvas(SheetFrameW, SheetFrameH);
            // Kulit diambil dari leher tampak depan (kolom 0), sama untuk semua karakter sheet.
            Color32 skin = SheetPixel(PlayerSheetRow, 0, 7, HeadLastImageRow);
            Color32 skinShade = SheetPixel(PlayerSheetRow, 0, 6, HeadLastImageRow);
            Color32 donorSkin = SheetPixel(bodyRow, 0, 7, HeadLastImageRow);
            Color32 donorShade = SheetPixel(bodyRow, 0, 6, HeadLastImageRow);

            for (int iy = 0; iy < SheetFrameH; iy++)
            {
                for (int x = 0; x < SheetFrameW; x++)
                {
                    Color32 p;
                    if (iy <= HeadLastImageRow) p = SheetPixel(PlayerSheetRow, col, x, iy);
                    else
                    {
                        p = SheetPixel(bodyRow, col, x, iy);
                        if (Same(p, donorSkin)) p = skin;
                        else if (Same(p, donorShade)) p = skinShade;
                    }
                    c.Set(x, SheetFrameH - 1 - iy, p);
                }
            }
            return c;
        }

        /// <summary>
        /// Ganti pakaian tokoh utama: sprite "lc_player_*" dibuat ulang dari kepala tokoh utama
        /// dan badan baris <paramref name="bodyRow"/> (-1 = baju harian). Return false bila sheet tidak ada.
        /// </summary>
        public static bool ApplyPlayerOutfit(int bodyRow)
        {
            if (_sheetPixels == null) return false;
            int rows = _sheetH / SheetFrameH;
            if (bodyRow < 0 || bodyRow >= rows) bodyRow = PlayerSheetRow;
            if (bodyRow == _playerOutfitRow) return true;
            _playerOutfitRow = bodyRow;

            for (int dir = 0; dir < 4; dir++)
            {
                int col = SheetColumnForDir[dir];
                for (int frame = 0; frame < 3; frame++)
                {
                    var c = OutfitCanvas(bodyRow, col);
                    if (frame == 1) LiftLeg(c, 0, SheetFrameW / 2);
                    else if (frame == 2) LiftLeg(c, SheetFrameW / 2, SheetFrameW);
                    Add(c, new Vector2(0.5f, 0f), "player_" + DirNames[dir] + "_" + frame);
                }
            }
            return true;
        }

        /// <summary>Ikon pakaian (tokoh utama tampak depan memakai baju itu), dibuat sekali.</summary>
        public static string OutfitIcon(int bodyRow)
        {
            string key = Prefix + "outfit_" + bodyRow;
            if (PixelArt.Sprites.ContainsKey(key)) return key;
            if (_sheetPixels == null || bodyRow < 0 || bodyRow >= _sheetH / SheetFrameH) return null;
            Add(OutfitCanvas(bodyRow, 0), new Vector2(0.5f, 0.5f), "outfit_" + bodyRow);
            return key;
        }

        /// <summary>Mengangkat satu kaki 1 piksel (kolom x0..x1) untuk frame langkah.</summary>
        static void LiftLeg(PixelCanvas c, int x0, int x1)
        {
            for (int x = x0; x < x1; x++)
            {
                for (int y = SheetLegRows - 1; y >= 1; y--)
                    c.Set(x, y, c.Get(x, y - 1));
                c.Set(x, 0, new Color32(0, 0, 0, 0));
            }
        }
    }
}
