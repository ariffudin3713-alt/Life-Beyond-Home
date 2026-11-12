

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Buku panduan cara bermain (tombol G, klik tombol pojok kanan bawah, atau aplikasi tablet).
    /// 8 topik, dua per halaman buku; dibalik dengan A/D.
    /// </summary>
    public partial class PulangGame
    {
        // Format entri: "JUDUL POIN|keterangan singkat". Halaman Table = tabel dua kolom.
        static readonly GuidePage[] GuidePages =
        {
            new GuidePage { Title = "KONTROL", Icon = "lc_app_guide", Table = true, Entries = new[]
            {
                "WASD|Berjalan",
                "KETUK / KLIK|Jalan ke titik itu",
                "KETUK 2X|Berlari ke titik itu",
                "SHIFT|Berlari (tahan)",
                "E / SPASI|Bicara, masuk, pilih",
                "TAB|Buka tablet",
                "I|Inventori",
                "T|Daftar tugas",
                "J|Jurnal",
                "G|Buku panduan",
                "P / ESC|Jeda permainan",
                "M|Suara nyala / mati",
            } },
            new GuidePage { Title = "MISI & ARAH", Icon = "lc_app_tasks", Entries = new[]
            {
                "Tujuan misi|Tertulis di panel kiri atas layar.",
                "Panah kuning|Menunjuk arah tujuan misi terdekat.",
                "Minimap|Kuning = misi, bintang = kerja, pink = teman.",
                "Daftar tugas (T)|Lihat misi yang bisa, sudah, dan belum bisa dikerjakan.",
            } },
            new GuidePage { Title = "WAKTU & ENERGI", Icon = "lc_item_food", Entries = new[]
            {
                "Waktu|Pagi, siang, sore, lalu malam. Lewat tengah malam kamu tertidur.",
                "Energi|Berkurang saat bekerja dan berlari.",
                "Makan|Beli makanan atau makan bekal untuk menambah energi.",
                "Tidur|Di kost setelah 18.00, atau di penginapan.",
            } },
            new GuidePage { Title = "KERJA & UANG", Icon = "lc_item_gear", Entries = new[]
            {
                "Melamar|Datangi tempat kerja yang ditandai bintang biru.",
                "Jam kerja|Datang tepat waktu agar upah tidak dipotong.",
                "Level|Bekerja memberi EXP. Naik level membuka pekerjaan baru.",
                "Kursus|Pekerjaan tinggi butuh kursus di Perpustakaan.",
            } },
            new GuidePage { Title = "TEMPAT & BUS", Icon = "lc_app_home", Entries = new[]
            {
                "Masuk bangunan|Berdiri di depan pintu lalu tekan E.",
                "Di dalam|Datangi kasir, kasur, atau resepsionis.",
                "Keluar|Berjalan ke keset pintu di bawah.",
                "Bus|Naik dari terminal. Tujuan misi bertanda [MISI].",
            } },
            new GuidePage { Title = "TEMAN", Icon = "lc_item_gift", Entries = new[]
            {
                "Mengobrol|Sekali sehari dengan setiap teman.",
                "Hadiah|Berikan saat mengobrol. Selera tiap teman berbeda.",
                "Keluarga|Telepon Ibu sekali sehari lewat tablet.",
                "Reputasi|Bertambah saat membantu warga.",
            } },
            new GuidePage { Title = "TAS & TABLET", Icon = "lc_app_bag", Entries = new[]
            {
                "Inventori (I)|Makanan, hadiah, perlengkapan, dan barang misi.",
                "Barang misi|Dipakai otomatis saat misi berjalan.",
                "Tablet (TAB)|Profil, tugas, jurnal, telepon, dan simpan.",
                "Profil|Level, EXP, pekerjaan, skill, dan teman.",
            } },
            new GuidePage { Title = "SIMPAN & TIPS", Icon = "lc_app_save", Entries = new[]
            {
                "Simpan|Otomatis, atau manual lewat tablet.",
                "Slot|Tersedia 3 slot simpanan di menu utama.",
                "Menu utama|Jeda, lalu tekan H.",
                "Pesan|Jangan lupa pulang. Ibu selalu menunggumu.",
            } },
        };

        void OpenGuide(bool fromTablet)
        {
            _returnToTablet = fromTablet;
            _state = State.Guide;
            _hud.SetPrompt(null);
            GameAudio.Play("click", 0.4f);
            _hud.ShowGuide(GuidePages);    // halaman disusun otomatis (topik panjang bersambung)
            _inputLock = 0.2f;
        }

        void UpdateGuide()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.CancelPressed || LifeInput.GuidePressed || LifeInput.InteractPressed)
            {
                _hud.HideGuide();
                CloseSubScreen();
                return;
            }
            int move = LifeInput.LeftPressed ? -1 : LifeInput.RightPressed ? 1 : 0;
            if (move != 0 && _hud.GuideFlip(move)) GameAudio.Play("click", 0.35f);
        }
    }
}
