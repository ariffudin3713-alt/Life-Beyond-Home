using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Profil karakter (aplikasi tablet): level, EXP, statistik hidup, pekerjaan, skill, teman.</summary>
    public partial class PulangGame
    {
        static readonly Color BarEnergy = new Color(0.99f, 0.80f, 0.30f);
        static readonly Color BarSavings = new Color(0.50f, 0.92f, 0.55f);
        static readonly Color BarFamily = new Color(1f, 0.55f, 0.65f);
        static readonly Color BarRep = new Color(0.55f, 0.78f, 1f);

        void OpenProfile(bool fromTablet)
        {
            _returnToTablet = fromTablet;
            _state = State.Profile;
            _hud.SetPrompt(null);

            var q = _story.Current;
            string chapter = q.Chapter == Chapter.Prolog ? "PROLOG"
                : q.Chapter == Chapter.Ending ? "ENDING" : "CHAPTER " + (int)q.Chapter;
            string header = "HARI " + D.day + "  " + GameCalendar.ShortDate(D.day) + "   " + chapter;

            int savePct = Mathf.RoundToInt(Mathf.Clamp01(D.savings / (float)PulangBalance.SuccessSavings) * 100f);
            var rows = new List<ProfileRow>
            {
                Header("KONDISI"),
                Row("ENERGI", D.energy + " / 100", D.energy / 100f, BarEnergy),
                Row("UANG", Rp(D.money)),
                Row("TABUNGAN", Rp(D.savings) + "  (" + savePct + "%)",
                    D.savings / (float)PulangBalance.SuccessSavings, BarSavings),

                Header("KARIER"),
                Row("PEKERJAAN UTAMA", JobLabel(D.jobMain)),
                Row("PEKERJAAN SAMPINGAN", JobLabel(D.jobSide)),
                Row("SKILL", SkillList()),

                // Urutan bagian = kolom kiri, kanan, kiri, kanan (bar dibagi seimbang di dua kolom).
                Header("LAINNYA"),
                Row("BARANG DI TAS", D.inventory.Count + " BARANG"),
                Row("MISI SAAT INI", PulangProfile.Format(q.Title)),

                Header("SOSIAL"),
                Row("REPUTASI WARGA", D.reputation.ToString(), Mathf.Clamp01(D.reputation / 30f), BarRep),
                D.Has("ratna_wafat")
                    ? Row("HUBUNGAN KELUARGA", "IBU TELAH TIADA")
                    : Row("HUBUNGAN KELUARGA", D.familyCloseness.ToString(), Mathf.Clamp01(D.familyCloseness / 40f), BarFamily),
            };

            var friends = new List<ProfileFriend>();
            for (int i = 0; i < PulangRomance.Girls.Length; i++)
            {
                var gd = PulangRomance.Girls[i];
                var g = D.girls[i];
                bool unlocked = _story.IsGirlUnlocked(i);
                string sprite = "lc_" + PulangNpcLines.Sprite[gd.Id] + "_down_0";
                if (unlocked && g.met)
                    friends.Add(new ProfileFriend
                    {
                        Known = true, Sprite = sprite, Name = gd.Id.ToString(),
                        Bar = Mathf.Clamp01(g.closeness / 100f),          // bar = kedekatan
                        Line1 = "EP " + g.episode + "/" + PulangRomance.EpisodeCount,
                        Line2 = " ",
                    });
                else
                    friends.Add(new ProfileFriend
                    {
                        Known = false, Sprite = sprite, Name = "???",
                        Line1 = unlocked ? "BELUM" : "BUKA",
                        Line2 = unlocked ? "KENAL" : "BAB " + (int)gd.Unlocks,
                    });
            }

            int need = PulangJobs.ExpForNextLevel(D.level);
            _hud.ShowProfile(PulangProfile.Name, RankFor(D.jobTier), D.level, D.exp, need, header, rows, friends);
            _inputLock = 0.2f;
        }

        void UpdateProfile()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.CancelPressed || LifeInput.InteractPressed || LifeInput.TabletPressed)
            {
                _hud.HideProfile();
                CloseSubScreen();
            }
        }

        static ProfileRow Header(string title) => new ProfileRow { Label = title, Header = true, Bar = -1f };

        static ProfileRow Row(string label, string value, float bar = -1f, Color color = default) =>
            new ProfileRow { Label = label, Value = value, Bar = bar, BarColor = color };

        static string RankFor(JobTier t)
        {
            switch (t)
            {
                case JobTier.Sederhana: return "PEKERJA PEMULA";
                case JobTier.Keterampilan: return "TENAGA TERAMPIL";
                case JobTier.Alam: return "PEKERJA ALAM";
                case JobTier.Profesional: return "PROFESIONAL";
                default: return "PERANTAU BARU";
            }
        }

        string SkillList() => PulangSkills.Summary(D);
    }
}
