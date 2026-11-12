using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Daftar tugas (tombol T): misi yang bisa dikerjakan sekarang, yang sudah
    /// dikerjakan, dan yang belum bisa dikerjakan beserta alasannya.
    /// </summary>
    public partial class PulangGame
    {
        void OpenTasks()
        {
            var available = new List<TaskItem>();
            var done = new List<TaskItem>();
            var locked = new List<TaskItem>();
            CollectTasks(available, done, locked, out int doneCount, out int totalCount);

            var q = _story.Current;
            string chapter = q.Chapter == Chapter.Prolog ? "PROLOG"
                : q.Chapter == Chapter.Ending ? "ENDING" : "CHAPTER " + (int)q.Chapter;
            string subtitle = chapter + " - " + PulangProfile.Format(q.Title);

            _state = State.Tasks;
            _hud.SetPrompt(null);
            _hud.ShowTasks(subtitle, doneCount, totalCount, available, done, locked);
            _inputLock = 0.2f;
        }

        void UpdateTasks()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.TasksPressed || LifeInput.CancelPressed || LifeInput.InteractPressed)
            {
                _hud.HideTasks();
                CloseSubScreen();
                return;
            }

            // A/D: pilih kartu. W/S (tahan) atau roda mouse: gulir kartu yang dipilih.
            if (LifeInput.LeftPressed) { _hud.FocusTaskCard(-1); GameAudio.Play("click", 0.35f); }
            if (LifeInput.RightPressed) { _hud.FocusTaskCard(1); GameAudio.Play("click", 0.35f); }

            float y = LifeInput.Y;
            if (Mathf.Abs(y) > 0.2f) _hud.ScrollTasks(-y * 9f * Time.unscaledDeltaTime);

            float wheel = LifeInput.ScrollWheel;
            if (wheel > 0f) _hud.ScrollTasksStep(-1);
            else if (wheel < 0f) _hud.ScrollTasksStep(1);
        }

        void CollectTasks(List<TaskItem> available, List<TaskItem> done, List<TaskItem> locked,
            out int doneCount, out int totalCount)
        {
            var q = _story.Current;
            Region here = _world.CurrentRegion;
            doneCount = 0;
            totalCount = 0;

            // ---------------- misi utama saat ini
            var actionable = new HashSet<int>(PendingObjectiveIndices());
            bool nightWait = q.SceneAtNight && !D.sceneShown && D.minutes < PulangBalance.NightStartMinutes;
            for (int i = 0; i < q.Objectives.Length; i++)
            {
                var o = q.Objectives[i];
                if (string.IsNullOrEmpty(o.Text)) continue;
                totalCount++;
                string text = PulangProfile.Format(o.Text);
                string progress = o.Count > 1 ? "Progres " + D.objectiveProgress[i] + "/" + o.Count : null;

                if (D.objectiveProgress[i] >= o.Count)
                {
                    doneCount++;
                    done.Add(new TaskItem(text, "Misi saat ini"));
                }
                else if (nightWait && o.Event == StoryEvent.SceneDone && o.Key == q.EnterScene)
                    locked.Add(new TaskItem(text, "Mulai pukul 19.00"));
                else if (actionable.Contains(i))
                    available.Add(new TaskItem(text, Join(WhereHint(o, here), progress)));
                else
                    locked.Add(new TaskItem(text, "Selesaikan tugas sebelumnya"));
            }
            if (q.Gate != null && q.GateText != null)
            {
                totalCount++;
                if (q.Gate(D)) { doneCount++; done.Add(new TaskItem(q.GateText, "Misi saat ini")); }
                else available.Add(new TaskItem(q.GateText, "Level sekarang " + D.level));
            }

            // ---------------- tugas harian & pekerjaan
            if (_story.CanCallMother && _story.Chapter >= Chapter.Ch1)
            {
                if (_today.Contains("telepon_ibu")) done.Add(new TaskItem("Telepon Ibu", "Harian - hari ini"));
                else available.Add(new TaskItem("Telepon Ibu", "Harian - di Kost Pelangi"));
            }

            foreach (string id in new[] { D.jobMain, D.jobSide })
            {
                var j = PulangJobs.Get(id);
                if (j == null) continue;
                string title = "Kerja: " + j.Name;
                string place = PulangWorld.Get(j.Place).Name;
                string hours = j.Flexible ? "Jam fleksibel" : Hour(j.Start) + "-" + Hour(j.End);
                if (_today.Contains("kerja:" + j.Id)) done.Add(new TaskItem(title, "Sudah bekerja hari ini"));
                else if (D.energy < 20) locked.Add(new TaskItem(title, "Energi kurang - makan atau istirahat"));
                else if (!j.Flexible && D.minutes >= j.End) locked.Add(new TaskItem(title, "Besok pukul " + Hour(j.Start)));
                else if (!j.Flexible && D.minutes < j.Start - 60) locked.Add(new TaskItem(title, "Mulai pukul " + Hour(j.Start)));
                else available.Add(new TaskItem(title, hours + " - " + place));
            }

            // ---------------- misi sebelumnya yang sudah tamat (terbaru dulu)
            for (var s = D.state - 1; s > StoryState.NameEntry; s--)
            {
                var past = PulangStory.QuestFor(s);
                if (past == null || !HasTasks(past)) continue;
                done.Add(new TaskItem(PulangProfile.Format(past.Title), "Tamat"));
            }

            // ---------------- misi berikutnya yang masih terkunci
            int upcoming = 0;
            for (var s = D.state + 1; s < StoryState.TheEnd && upcoming < 3; s++)
            {
                var next = PulangStory.QuestFor(s);
                if (next == null || !HasTasks(next)) continue;
                locked.Add(new TaskItem(PulangProfile.Format(next.Title),
                    upcoming == 0 ? "Terbuka setelah misi ini selesai" : "Terkunci"));
                upcoming++;
            }
        }

        static bool HasTasks(QuestDef q)
        {
            if (q.Gate != null) return true;
            foreach (var o in q.Objectives)
                if (!string.IsNullOrEmpty(o.Text)) return true;
            return false;
        }

        static string Join(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b;
            if (string.IsNullOrEmpty(b)) return a;
            return a + " - " + b;
        }

        /// <summary>Lokasi tugas, mis. "Warung Bu Sari" atau "Kampung Cempaka (wilayah lain)".</summary>
        string WhereHint(Objective o, Region here)
        {
            LocationId? loc = null;
            switch (o.Event)
            {
                case StoryEvent.Arrive:
                    if (Enum.TryParse(o.Key, out LocationId l)) loc = l;
                    break;
                case StoryEvent.Talk:
                    if (Enum.TryParse(o.Key, out NpcId n)) loc = NpcHome(n);
                    break;
                case StoryEvent.SceneDone:
                    if (SceneOwner.TryGetValue(o.Key, out NpcId owner)) loc = NpcHome(owner);
                    break;
                case StoryEvent.Travel:
                    return "Naik bus dari terminal";
                case StoryEvent.Skill:
                case StoryEvent.Save:
                case StoryEvent.Buy:
                    loc = ObjectivePlace(o);
                    break;
                case StoryEvent.GirlTalk:
                    return "Nabila, Kirana, Anindya, Melati, atau Citra - sekali sehari per orang";
                case StoryEvent.Gift:
                    return "Beli hadiah di toko, lalu berikan saat mengobrol";
                case StoryEvent.WorkShift:
                {
                    var places = WorkShiftPlaces(o);
                    if (places.Count == 0) return null;
                    var names = new List<string>();
                    foreach (var p in places) names.Add(PulangWorld.Get(p).Name);
                    return string.Join(" / ", names);
                }
            }
            if (loc == null)
            {
                if (TryGetObjectiveRegion(o, out Region r) && r != here)
                    return "Di " + PulangWorld.RegionNames[r];
                return null;
            }
            var def = PulangWorld.Get(loc.Value);
            return def.Region == here ? def.Name : def.Name + ", " + PulangWorld.RegionNames[def.Region];
        }

        static LocationId? NpcHome(NpcId id)
        {
            foreach (var def in PulangCast.All)
                if (def.Id == id) return def.Home;
            return null;
        }

        static string Hour(int minutes) => (minutes / 60).ToString("00") + ".00";
    }
}
