using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Misi sampingan di dalam permainan:
    /// - NPC yang punya misi baru bertanda "!" kuning; NPC tujuan misi yang sedang berjalan bertanda "?" biru;
    /// - bicara dengan pemberi misi = tawaran (TERIMA / NANTI SAJA), tidak pernah memaksa;
    /// - beberapa misi bisa berjalan bersamaan dan tidak mengganggu misi utama;
    /// - tujuan misi sampingan ikut ditandai di minimap & daftar tugas (T).
    /// </summary>
    public partial class PulangGame
    {
        static readonly Color MarkerSide = new Color(0.55f, 0.95f, 0.65f, 1f);

        List<SideQuestProgress> SideList
        {
            get
            {
                if (D.side == null) D.side = new List<SideQuestProgress>();
                return D.side;
            }
        }

        SideQuestProgress SideProgress(string id)
        {
            foreach (var p in SideList) if (p.id == id) return p;
            return null;
        }

        /// <summary>Misi sampingan yang sedang berjalan beserta langkahnya saat ini.</summary>
        IEnumerable<KeyValuePair<SideQuestDef, SideQuestProgress>> ActiveSideQuests()
        {
            foreach (var p in SideList)
            {
                if (p.done) continue;
                var q = PulangSideQuests.Get(p.id);
                if (q == null || p.step >= q.Steps.Length) continue;
                yield return new KeyValuePair<SideQuestDef, SideQuestProgress>(q, p);
            }
        }

        /// <summary>Misi baru yang bisa ditawarkan sekarang (belum diambil, bab cukup, sebelum kabar duka).</summary>
        bool SideOfferable(SideQuestDef q)
        {
            if (_story.Data == null || D.state == StoryState.NameEntry) return false;
            if (_story.Chapter < q.MinChapter || _story.Chapter >= Chapter.Ch6) return false;
            if (SideProgress(q.Id) != null) return false;
            return _npcs.ContainsKey(q.Giver);
        }

        // ------------------------------------------------------------------ kejadian

        /// <summary>Dipanggil untuk setiap kejadian cerita (lihat PulangStory.AnyEvent).</summary>
        void OnSideEvent(StoryEvent ev, string key)
        {
            if (_story.Data == null || ev == StoryEvent.Talk || ev == StoryEvent.SceneDone) return;
            bool changed = false;
            foreach (var pair in new List<KeyValuePair<SideQuestDef, SideQuestProgress>>(ActiveSideQuests()))
            {
                var q = pair.Key;
                var p = pair.Value;
                var s = q.Steps[p.step];
                if (s.Event != ev || !SideKeyMatches(s.Key, ev, key)) continue;

                p.count++;
                changed = true;
                if (p.count < s.Count)
                {
                    _hud.ShowToast((q.Title + ": " + s.Text + " (" + p.count + "/" + s.Count + ")").ToUpperInvariant());
                    continue;
                }
                AdvanceSide(q, p);
            }
            if (changed) D.Save();
        }

        static bool SideKeyMatches(string want, StoryEvent ev, string key)
        {
            if (want == "*" || want == key) return true;
            if (want == "#pakaian" && ev == StoryEvent.Buy)
            {
                var it = PulangItems.Get(key);
                return it != null && it.IsOutfit;
            }
            return false;
        }

        void AdvanceSide(SideQuestDef q, SideQuestProgress p)
        {
            var s = q.Steps[p.step];
            p.step++;
            p.count = 0;
            if (!string.IsNullOrEmpty(s.Give)) GiveSideItem(s.Give);
            GameAudio.Play("quest_done", 0.45f);

            if (p.step >= q.Steps.Length) { CompleteSide(q, p); return; }
            _hud.ShowToast(("MISI SAMPINGAN - " + q.Title + ": " + q.Steps[p.step].Text).ToUpperInvariant());
        }

        void CompleteSide(SideQuestDef q, SideQuestProgress p)
        {
            p.done = true;
            if (!string.IsNullOrEmpty(q.GiveOnAccept)) RemoveAll(q.GiveOnAccept);
            D.money += q.Money;
            D.reputation += q.Rep;
            GainExp(q.Exp);
            GameAudio.Play("coin", 0.6f);
            var reward = new List<string>();
            if (q.Money > 0) reward.Add("+" + Rp(q.Money));
            if (q.Exp > 0) reward.Add("+" + q.Exp + " EXP");
            if (q.Rep > 0) reward.Add("+" + q.Rep + " REPUTASI");
            _hud.ShowToast(("MISI SAMPINGAN SELESAI: " + q.Title + "  " + string.Join("  ", reward)).ToUpperInvariant());
            D.Save();
            _story.Tick();
        }

        void GiveSideItem(string id)
        {
            D.inventory.Add(id);
            var it = PulangItems.Get(id);
            if (it == null) return;
            GameAudio.Play("pickup", 0.55f);
            _hud.ShowToast(("BARANG MISI DIDAPAT: " + it.Name + " - LIHAT INVENTORI (I)").ToUpperInvariant());
        }

        void RemoveAll(string id)
        {
            while (D.inventory.Remove(id)) { }
        }

        int CountItem(string id)
        {
            int n = 0;
            foreach (string s in D.inventory) if (s == id) n++;
            return n;
        }

        // ------------------------------------------------------------------ bicara

        /// <summary>
        /// Bicara dengan NPC untuk misi sampingan: langkah "temui X" yang sedang berjalan dulu,
        /// lalu tawaran misi baru. Return true bila percakapan sudah ditangani.
        /// </summary>
        bool TrySideTalk(NpcId id)
        {
            if (_story.Data == null) return false;
            string key = id.ToString();
            string npcName = NpcName(id);

            foreach (var pair in ActiveSideQuests())
            {
                var q = pair.Key;
                var p = pair.Value;
                var s = q.Steps[p.step];
                if (s.Event != StoryEvent.Talk || s.Key != key) continue;

                if (!string.IsNullOrEmpty(s.Consume) && CountItem(s.Consume) < s.ConsumeCount)
                {
                    var need = PulangItems.Get(s.Consume);
                    string what = (s.ConsumeCount > 1 ? s.ConsumeCount + " " : "") + (need != null ? need.Name.ToLowerInvariant() : s.Consume);
                    Say(PulangProfile.Token, "Aku belum membawa " + what + " untuk " + npcName + ".");
                    return true;
                }
                if (!string.IsNullOrEmpty(s.Consume))
                {
                    for (int i = 0; i < s.ConsumeCount; i++) D.inventory.Remove(s.Consume);
                    var it = PulangItems.Get(s.Consume);
                    if (it != null) _hud.ShowToast((it.Name + (s.ConsumeCount > 1 ? " x" + s.ConsumeCount : "") + " diserahkan").ToUpperInvariant());
                }

                bool last = p.step == q.Steps.Length - 1;
                var lines = new List<Line>();
                if (last && id == q.Giver) lines.Add(new Line(npcName, q.Thanks));
                else lines.Add(new Line(npcName, string.IsNullOrEmpty(s.Reply) ? "Baik, terima kasih sudah datang." : s.Reply));

                SideQuestDef quest = q;
                SideQuestProgress prog = p;
                Say(lines, () => { AdvanceSide(quest, prog); D.Save(); });
                return true;
            }

            foreach (var q in PulangSideQuests.All)
            {
                if (q.Giver != id || !SideOfferable(q) || _today.Contains("tolak_sq:" + q.Id)) continue;
                OfferSide(q);
                return true;
            }
            return false;
        }

        void OfferSide(SideQuestDef q)
        {
            string giver = NpcName(q.Giver);
            var lines = new List<Line>();
            foreach (string l in q.Offer) lines.Add(new Line(giver, l));

            Say(lines, () =>
            {
                string reward = "HADIAH " + Rp(q.Money) + " + " + q.Exp + " EXP";
                OpenMenu("MISI SAMPINGAN: " + q.Title + " - " + reward,
                    new List<string> { "TERIMA", "NANTI SAJA" },
                    new List<Action> { () => AcceptSide(q), () => DeclineSide(q) },
                    true, () => DeclineSide(q));
            });
        }

        void AcceptSide(SideQuestDef q)
        {
            if (SideProgress(q.Id) != null) return;
            SideList.Add(new SideQuestProgress { id = q.Id });
            GameAudio.Play("quest_done", 0.5f);
            if (!string.IsNullOrEmpty(q.GiveOnAccept)) GiveSideItem(q.GiveOnAccept);
            Say(NpcName(q.Giver), "Terima kasih, " + PulangProfile.Token + ". " + PulangProfile.Format(q.Steps[0].Text) + ", ya.", () =>
                _hud.ShowToast(("MISI SAMPINGAN DITERIMA: " + q.Title + " - LIHAT DAFTAR TUGAS (T)").ToUpperInvariant()));
            D.Save();
        }

        void DeclineSide(SideQuestDef q)
        {
            _today.Add("tolak_sq:" + q.Id);   // tidak ditawarkan lagi hari ini; besok boleh ditanya lagi
            Say(NpcName(q.Giver), "Tidak apa-apa. Kalau ada waktu luang, mampir saja lagi.");
        }

        // ------------------------------------------------------------------ penanda

        /// <summary>"!" di atas pemberi misi baru, "?" di atas NPC tujuan misi sampingan yang berjalan.</summary>
        void RefreshSideBadges()
        {
            if (_story.Data == null) return;
            var targets = new HashSet<string>();
            foreach (var pair in ActiveSideQuests())
            {
                var s = pair.Key.Steps[pair.Value.step];
                if (s.Event == StoryEvent.Talk) targets.Add(s.Key);
            }
            var offers = new HashSet<NpcId>();
            foreach (var q in PulangSideQuests.All)
                if (SideOfferable(q)) offers.Add(q.Giver);

            foreach (var pair in _npcs)
            {
                string badge = targets.Contains(pair.Key.ToString()) ? "?" : offers.Contains(pair.Key) ? "!" : null;
                pair.Value.SetBadge(badge);
            }
        }

        /// <summary>Tujuan misi sampingan di wilayah ini (penunjuk & minimap).</summary>
        void AddSideGoals(Region here)
        {
            foreach (var pair in ActiveSideQuests())
            {
                var s = pair.Key.Steps[pair.Value.step];
                switch (s.Event)
                {
                    case StoryEvent.Talk:
                        if (Enum.TryParse(s.Key, out NpcId n) && _npcs.TryGetValue(n, out LifeNpc npc) &&
                            npc.gameObject.activeInHierarchy)
                            AddGoal(npc.Position, ("(S) " + npc.DisplayName).ToUpperInvariant());
                        break;
                    case StoryEvent.Arrive:
                        if (Enum.TryParse(s.Key, out LocationId l) && PulangWorld.Get(l).Region == here)
                            AddGoal(_world.Spot(l), ("(S) " + PulangWorld.Get(l).Name).ToUpperInvariant());
                        break;
                    case StoryEvent.Buy:
                        foreach (var loc in SideBuyPlaces(s.Key))
                            if (PulangWorld.Get(loc).Region == here)
                                AddGoal(_world.Spot(loc), ("(S) " + PulangWorld.Get(loc).Name).ToUpperInvariant());
                        break;
                }
            }
        }

        static IEnumerable<LocationId> SideBuyPlaces(string key)
        {
            if (key == "#pakaian") { yield return LocationId.TokoPakaian; yield break; }
            var it = PulangItems.Get(key);
            if (it == null) yield break;
            foreach (var l in it.SoldAt) yield return l;
        }

        // ------------------------------------------------------------------ daftar tugas

        void CollectSideTasks(List<TaskItem> available, List<TaskItem> done)
        {
            Region here = _world.CurrentRegion;
            foreach (var pair in ActiveSideQuests())
            {
                var q = pair.Key;
                var p = pair.Value;
                var s = q.Steps[p.step];
                string progress = s.Count > 1 ? "Progres " + p.count + "/" + s.Count : null;
                available.Add(new TaskItem("(Sampingan) " + q.Title + ": " + PulangProfile.Format(s.Text),
                    Join(SideWhere(s, here), progress)));
            }
            foreach (var q in PulangSideQuests.All)
            {
                if (!SideOfferable(q)) continue;
                var home = NpcHome(q.Giver);
                string where = home != null ? PulangWorld.Get(home.Value).Name : "";
                available.Add(new TaskItem("(Sampingan baru) " + q.Title,
                    "Bicara dengan " + NpcName(q.Giver) + (where.Length > 0 ? " - " + where : "") + " (tanda !)"));
            }
            foreach (var p in SideList)
            {
                if (!p.done) continue;
                var q = PulangSideQuests.Get(p.id);
                if (q != null) done.Add(new TaskItem("(Sampingan) " + q.Title, "Selesai"));
            }
        }

        string SideWhere(SideStep s, Region here)
        {
            LocationId? loc = null;
            switch (s.Event)
            {
                case StoryEvent.Talk:
                    if (Enum.TryParse(s.Key, out NpcId n)) loc = NpcHome(n);
                    break;
                case StoryEvent.Arrive:
                    if (Enum.TryParse(s.Key, out LocationId l)) loc = l;
                    break;
                case StoryEvent.Litter:
                    return "Pungut sampah di jalan, buang ke tong sampah";
                case StoryEvent.WorkShift:
                    return "Bekerja di tempat kerjamu";
                case StoryEvent.Save:
                    return "Kost Pelangi";
                case StoryEvent.Buy:
                    foreach (var b in SideBuyPlaces(s.Key)) { loc = b; if (PulangWorld.Get(b).Region == here) break; }
                    break;
            }
            if (loc == null) return null;
            var def = PulangWorld.Get(loc.Value);
            return def.Region == here ? def.Name : def.Name + ", " + PulangWorld.RegionNames[def.Region];
        }

        /// <summary>Barang misi sampingan: nama misinya (untuk keterangan di inventori).</summary>
        string SideQuestTitleForItem(string itemId)
        {
            foreach (var pair in ActiveSideQuests())
            {
                if (pair.Key.GiveOnAccept == itemId) return pair.Key.Title;
                foreach (var s in pair.Key.Steps)
                    if (s.Give == itemId || s.Consume == itemId) return pair.Key.Title;
            }
            return null;
        }
    }
}
