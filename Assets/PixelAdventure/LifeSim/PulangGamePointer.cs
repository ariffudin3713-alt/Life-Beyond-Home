namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Bantuan kursor di sisi permainan (zona klik didaftarkan oleh LifeHud, lihat LifeHudPointer):
    /// - bilah tombol HUD (TAS, TUGAS, JURNAL, PETA, TABLET, JEDA);
    /// - klik "di mana saja" untuk permainan kerja yang hanya butuh satu tombol.
    /// </summary>
    public partial class PulangGame
    {
        /// <summary>Tombol bilah HUD diklik (indeks sesuai LifeHud.ToolbarLabels).</summary>
        void RunToolbar(int index)
        {
            GameAudio.Play("click", 0.4f);
            _player.ClearPath();
            switch (index)
            {
                case 0: OpenInventory(false); break;
                case 1: OpenTasks(); break;
                case 2: OpenJournal(); break;
                case 3: OpenWorldMap(false); break;
                case 4: OpenTablet(); break;
                default: OpenPause(); break;
            }
        }

        /// <summary>Klik / ketuk di layar kerja (bukan pada tombol X) = sama dengan menekan E.</summary>
        bool WorkTapped() => LifeInput.PointerTapped && !_hud.PointerOver(UiHit.Close, out _);
    }
}
