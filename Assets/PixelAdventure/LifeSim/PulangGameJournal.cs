using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Isi layar JURNAL: misi & langkahnya, catatan diri, dan hubungan.</summary>
    public partial class PulangGame
    {
        static readonly Color BondFamily = new Color(1f, 0.74f, 0.45f);
        static readonly Color BondGirl = new Color(1f, 0.55f, 0.72f);

        static string ChapterLabel(Chapter c) =>
            c == Chapter.Prolog ? "PROLOG" : c == Chapter.Ending ? "ENDING" : "CHAPTER " + (int)c;

        string ClockLabel()
        {
            int hh = D.minutes / 60, mm = D.minutes % 60;
            return "HARI " + D.day + "  " + hh.ToString("00") + ":" + mm.ToString("00") + "  " + LifeHud.PhaseFor(D.minutes);
        }

        void OpenJournal()
        {
            var q = _story.Current;
            var v = new JournalView
            {
                Chapter = ChapterLabel(q.Chapter),
                Title = PulangProfile.Format(q.Title),
                Task = CurrentHint(),
                Clock = ClockLabel(),
            };

            // Langkah misi: selesai, yang sedang dikerjakan (pertama yang belum selesai), lalu berikutnya.
            bool current = false;
            for (int i = 0; i < q.Objectives.Length; i++)
            {
                var o = q.Objectives[i];
                if (string.IsNullOrEmpty(o.Text)) continue;
                int have = i < D.objectiveProgress.Length ? D.objectiveProgress[i] : 0;
                bool done = have >= o.Count;
                string count = o.Count > 1 ? " (" + Mathf.Min(have, o.Count) + "/" + o.Count + ")" : "";
                int state = done ? 0 : current ? 2 : 1;
                if (!done) current = true;
                v.Steps.Add(new JournalStep { Text = PulangProfile.Format(o.Text) + count, State = state });
            }
            if (q.GateText != null) v.Steps.Add(new JournalStep { Text = q.GateText, State = current ? 2 : 1 });

            // Catatan diri
            void Stat(string label, string value) => v.Stats.Add(new JournalStat { Label = label, Value = value });
            Stat("LEVEL", D.level + "   EXP " + D.exp + "/" + PulangJobs.ExpForNextLevel(D.level));
            Stat("ENERGI", D.energy + "/100");
            Stat("UANG", Rp(D.money));
            Stat("TABUNGAN", Rp(D.savings));
            Stat("PEKERJAAN", JobLabel(D.jobMain));
            Stat("SAMPINGAN", JobLabel(D.jobSide));
            var ride = RidingVehicle;
            int owned = OwnedVehicles().Count;
            Stat("KENDARAAN", ride != null ? ride.Name + " (DIPAKAI)" : owned > 0 ? owned + " DI INVENTORI" : "BELUM PUNYA");
            Stat("REPUTASI", D.reputation.ToString());

            // Hubungan
            if (!D.Has("ratna_wafat"))
                v.Bonds.Add(new JournalBond
                {
                    Name = "IBU (RATNA)", Detail = D.familyCloseness + " POIN",
                    Fill = D.familyCloseness / 100f, Color = BondFamily,
                });
            for (int i = 0; i < PulangRomance.Girls.Length; i++)
            {
                if (!_story.IsGirlUnlocked(i) || !D.girls[i].met) continue;
                var gd = PulangRomance.Girls[i];
                int ep = D.girls[i].episode;
                v.Bonds.Add(new JournalBond
                {
                    Name = NpcName(gd.Id), Detail = "EPISODE " + ep + "/" + PulangRomance.EpisodeCount,
                    Fill = ep / (float)PulangRomance.EpisodeCount, Color = BondGirl,
                });
            }

            _state = State.Journal;
            _hud.SetPrompt(null);
            _hud.ShowJournal(v);
            _inputLock = 0.2f;
        }

        static string NpcName(NpcId id)
        {
            foreach (var def in PulangCast.All) if (def.Id == id) return def.Name;
            return id.ToString();
        }
    }
}
