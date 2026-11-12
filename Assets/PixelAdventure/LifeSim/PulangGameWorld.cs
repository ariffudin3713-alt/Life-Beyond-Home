using System;
using System.Collections;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public partial class PulangGame
    {
        // ================================================================== NPC

        readonly Dictionary<NpcId, LifeNpc> _npcs = new Dictionary<NpcId, LifeNpc>();

        void SpawnNpcs()
        {
            var countAt = new Dictionary<LocationId, int>();
            foreach (var def in PulangCast.All)
            {
                if (def.Id == NpcId.NpcKeluarga) continue;   // hanya lewat telepon
                Region r = PulangWorld.Get(def.Home).Region;
                countAt.TryGetValue(def.Home, out int k);
                countAt[def.Home] = k + 1;
                if (!_world.TryNpcAnchor(def.Home, k, out Vector2 p))
                    p = _world.Spot(def.Home) + new Vector2(2.4f + k * 1.5f, -0.6f);
                _npcs[def.Id] = LifeNpc.Create(_world.RegionRoot(r), PulangNpcLines.Sprite[def.Id],
                    def.Name.ToUpperInvariant(), p.x, p.y);
            }
        }

        void RefreshNpcVisibility()
        {
            if (_story.Data == null) return;
            foreach (var pair in _npcs)
            {
                bool show = true;
                if (pair.Key == NpcId.Ratna) show = !D.Has("ratna_wafat");
                int gi = GirlIndex(pair.Key);
                if (gi >= 0) show = _story.IsGirlUnlocked(gi);
                pair.Value.gameObject.SetActive(show);
            }
        }

        void FaceNearbyNpcs()
        {
            foreach (var npc in _npcs.Values)
                if (npc.gameObject.activeInHierarchy &&
                    (npc.Position - _player.Feet).sqrMagnitude < 9f)
                    npc.FaceTowards(_player.Feet);
        }

        // ================================================================== barang cerita

        struct PickupDef
        {
            public string Id, Label;
            public LocationId Loc;
            public Vector2 Offset;
            public PickupDef(string id, string label, LocationId loc, float x, float y)
            {
                Id = id; Label = label; Loc = loc; Offset = new Vector2(x, y);
            }
        }

        static readonly PickupDef[] Pickups =
        {
            new PickupDef("tas", "TAS", LocationId.RumahZidan, -2.6f, -0.4f),
            new PickupDef("barang_pribadi", "BARANG PRIBADI", LocationId.RumahZidan, -4.2f, -1.0f),
            new PickupDef("buku_catatan_nabila", "BUKU CATATAN KECIL", LocationId.TamanPelangi, 6f, 3f),
            new PickupDef("barang_bawaan", "BARANG BAWAAN", LocationId.KostPelangi, -2.6f, -0.4f),
        };

        readonly Dictionary<string, GameObject> _pickupObjects = new Dictionary<string, GameObject>();

        void SpawnPickups()
        {
            foreach (var p in Pickups)
            {
                Region r = PulangWorld.Get(p.Loc).Region;
                Vector2 pos = _world.Spot(p.Loc) + p.Offset;
                var go = new GameObject("Barang_" + p.Id);
                go.transform.SetParent(_world.RegionRoot(r), false);
                go.transform.position = new Vector3(pos.x, pos.y, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PixelArt.Get("lc_icon_star");
                sr.sharedMaterial = PixelArt.SpriteMaterial;
                sr.sortingOrder = -Mathf.RoundToInt(pos.y * PixelArt.PPU) + 2;
                go.transform.localScale = Vector3.one * 1.6f;
                go.SetActive(false);
                _pickupObjects[p.Id] = go;
            }
        }

        void RefreshPickups()
        {
            foreach (var p in Pickups)
                _pickupObjects[p.Id].SetActive(_story.Data != null && IsPending(StoryEvent.Pickup, p.Id, out _));
            SyncQuestItems();
        }

        // ================================================================== interaksi

        enum Target { None, Npc, Pickup, Location, Decor }
        int _targetDecor;
        Target _target;
        NpcId _targetNpc;
        string _targetPickup;
        LocationId _targetLoc;
        LocationId? _insideLoc;

        static readonly LocationId[] TravelPoints =
        {
            LocationId.TerminalDesa, LocationId.TerminalJogokerto, LocationId.TerminalCempaka,
            LocationId.PasarTradisional, LocationId.PantaiCemara,
        };

        void UpdateInteraction()
        {
            _target = Target.None;
            if (_world.InInterior) { UpdateInteriorInteraction(); return; }
            float best = 1.7f * 1.7f;
            Vector2 me = _player.Feet;

            foreach (var pair in _npcs)
            {
                if (!pair.Value.gameObject.activeInHierarchy) continue;
                float d = (pair.Value.Position - me).sqrMagnitude;
                if (d < best) { best = d; _target = Target.Npc; _targetNpc = pair.Key; }
            }
            foreach (var p in Pickups)
            {
                var go = _pickupObjects[p.Id];
                if (!go.activeInHierarchy) continue;
                float d = ((Vector2)go.transform.position - me).sqrMagnitude;
                if (d < best) { best = d; _target = Target.Pickup; _targetPickup = p.Id; }
            }
            foreach (var loc in _world.LocationsIn(_world.CurrentRegion))
            {
                float d = (_world.Spot(loc) - me).sqrMagnitude;
                if (d >= best || !IsLocationOpen(loc)) continue;
                if (!_world.IsEnterable(loc) && BuildLocationMenu(loc, null, null) == 0) continue;
                best = d; _target = Target.Location; _targetLoc = loc;
            }
            // Bangunan dekorasi (rumah warga, toko tutup, bangunan tua): hanya bisa dilihat.
            var decor = _world.DecorIn(_world.CurrentRegion);
            for (int i = 0; i < decor.Count; i++)
            {
                float d = (decor[i].Door - me).sqrMagnitude;
                if (d < best) { best = d; _target = Target.Decor; _targetDecor = i; }
            }

            switch (_target)
            {
                case Target.Npc:
                    _hud.SetPrompt("E - BICARA DENGAN " + _npcs[_targetNpc].DisplayName); break;
                case Target.Decor:
                    _hud.SetPrompt("E - LIHAT " + _world.DecorIn(_world.CurrentRegion)[_targetDecor].Label); break;
                case Target.Pickup:
                    _hud.SetPrompt("E - AMBIL " + Array.Find(Pickups, p => p.Id == _targetPickup).Label); break;
                case Target.Location:
                    _hud.SetPrompt((_world.IsEnterable(_targetLoc) ? "E - MASUK " : "E - ") + PulangWorld.Get(_targetLoc).Name.ToUpperInvariant()); break;
                default:
                    _hud.SetPrompt(null); break;
            }
        }

        void Interact()
        {
            if (_world.InInterior) { InteractInterior(); return; }
            switch (_target)
            {
                case Target.Npc: TalkTo(_targetNpc); break;
                case Target.Pickup: PickUp(_targetPickup); break;
                case Target.Decor: Say(" ", _world.DecorIn(_world.CurrentRegion)[_targetDecor].Message, null); break;
                case Target.Location:
                    if (_world.IsEnterable(_targetLoc)) EnterBuilding(_targetLoc); else OpenLocation(_targetLoc);
                    break;
            }
        }

        bool IsLocationOpen(LocationId loc)
        {
            // Prolog berlangsung di desa; lokasi kota baru terbuka sesuai bab.
            if (PulangWorld.Get(loc).Region == Region.DesaSukaMakmur) return true;
            return PulangWorld.IsOpen(loc, _story.Chapter);
        }

        void CheckArrival()
        {
            Vector2 me = _player.Feet;
            LocationId? here = null;
            foreach (var loc in _world.LocationsIn(_world.CurrentRegion))
                if ((_world.Spot(loc) - me).sqrMagnitude < 2.2f * 2.2f) { here = loc; break; }

            if (here == _insideLoc) return;
            _insideLoc = here;
            if (here == null) return;

            D.location = here.Value;
            if (IsLocationOpen(here.Value)) _story.Notify(StoryEvent.Arrive, here.Value.ToString());
        }

        void PickUp(string id)
        {
            GameAudio.Play("coin", 0.6f);
            _pickupObjects[id].SetActive(false);
            var item = PulangItems.Get(id);
            if (item != null && !D.inventory.Contains(id))
            {
                D.inventory.Add(id);
                _hud.ShowToast(("DAPAT " + item.Name + " - CEK INVENTORI (I)").ToUpperInvariant());
            }
            _story.Notify(StoryEvent.Pickup, id);
            RefreshPickups();
            TryAutoScene();
        }

        // ================================================================== bicara

        /// <summary>Adegan untuk tujuan "Bicara dengan X" pada state tertentu.</summary>
        static string TalkSceneFor(StoryState s, NpcId id, int objectiveIndex)
        {
            switch (s)
            {
                case StoryState.PrologPersiapan when id == NpcId.Ratna: return "prolog_bicara";
                case StoryState.Ch1MengenalKota when id == NpcId.Dimas: return "ch1_dimas";
                case StoryState.Ch1Kebutuhan when id == NpcId.BuSari: return "ch1_busari";
                case StoryState.Ch1MembantuNabila when id == NpcId.Nabila:
                    return objectiveIndex == 0 ? "ch1_nabila" : "ch1_nabila_selesai";
                case StoryState.Ch2MembantuDimas when id == NpcId.Dimas: return "ch2_dimas";
                case StoryState.Ch2MasalahPasar when id == NpcId.BuSari: return "ch2_busari";
                case StoryState.Ch2MasalahKendaraan when id == NpcId.PakJoko: return "ch2_joko";
                case StoryState.Ch3KesempatanBaru when id == NpcId.PakBudi: return "ch3_budi";
            }
            return null;
        }

        /// <summary>Adegan "SceneDone" yang dipicu dengan berbicara ke NPC tertentu.</summary>
        static readonly Dictionary<string, NpcId> SceneOwner = new Dictionary<string, NpcId>
        {
            { "prolog_pamit", NpcId.Ratna },
            { "ch1_hendra", NpcId.PakHendra },
            { "q4_dimas_kerja", NpcId.Dimas },
            { "urusan_terakhir", NpcId.Dimas },
            // Chapter 3–5
            { "ch3_budi_kursus", NpcId.PakBudi }, { "ch3_melati", NpcId.Melati }, { "ch3_citra", NpcId.Citra },
            { "ch3_darto", NpcId.PakDarto }, { "ch3_sari_usaha", NpcId.BuSari },
            { "ch4_dimas", NpcId.Dimas }, { "ch4_budi", NpcId.PakBudi }, { "ch4_dimas_selesai", NpcId.Dimas },
            { "ch4_hendra", NpcId.PakHendra }, { "ch4_slamet", NpcId.PakSlamet }, { "ch4_hendra_selesai", NpcId.PakHendra },
            { "ch5_hadi", NpcId.PakHadi }, { "ch5_fajar", NpcId.MasFajar }, { "ch5_slamet", NpcId.PakSlamet },
        };

        /// <summary>Adegan yang membutuhkan barang misi (diserahkan saat adegan dimulai).</summary>
        static readonly Dictionary<string, string> SceneItems = new Dictionary<string, string>
        {
            { "ch4_slamet", "surat_hendra" },
            { "ch5_hadi", "oleh_oleh" },
        };

        static string MissingItemLine(string item) =>
            item == "surat_hendra" ? "Suratnya tidak ada padaku. Aku harus mengambilnya dari Pak Hendra dulu."
            : item == "oleh_oleh" ? "Aku belum membeli oleh-oleh untuk Ibu. Toko Pakaian di Pasar Ramai Jaya menjual kain batik."
            : "Aku belum membawa barang yang dibutuhkan.";

        /// <summary>Adegan yang langsung diputar saat menjadi tujuan berikutnya.</summary>
        static readonly HashSet<string> AutoScenes = new HashSet<string> { "pilih_kendaraan" };

        static readonly Dictionary<string, NpcId[]> DeliveryTargets = new Dictionary<string, NpcId[]>
        {
            { "paket_dimas", new[] { NpcId.PakHerman, NpcId.BuRukmini, NpcId.PakHadi } },
            { "barang_pasar", new[] { NpcId.PakDarto, NpcId.PakRudi, NpcId.BuSiti } },
        };

        void TryAutoScene()
        {
            if (_state != State.Playing || _story.Data == null) return;
            int i = NextPendingIndex();
            if (i < 0) return;
            var o = _story.Current.Objectives[i];
            if (o.Event == StoryEvent.SceneDone && AutoScenes.Contains(o.Key))
                RunScene(o.Key, () => _story.Notify(StoryEvent.SceneDone, o.Key));
        }

        void TalkTo(NpcId id)
        {
            _npcs[id].FaceTowards(_player.Feet);
            GameAudio.Play("talk", 0.55f);
            var q = _story.Current;
            string key = id.ToString();

            for (int i = 0; i < q.Objectives.Length; i++)
            {
                if (D.objectiveProgress[i] >= q.Objectives[i].Count) continue;
                var o = q.Objectives[i];

                if (o.Event == StoryEvent.Talk && o.Key == key)
                {
                    // Mengembalikan buku catatan: barang misi diserahkan kepada Nabila.
                    bool returnBook = q.State == StoryState.Ch1MembantuNabila && id == NpcId.Nabila && i > 0;
                    if (returnBook && !D.inventory.Contains("buku_catatan_nabila"))
                    {
                        Say(PulangProfile.Token, "Aku harus menemukan buku catatannya dulu.");
                        return;
                    }
                    if (returnBook)
                    {
                        D.inventory.Remove("buku_catatan_nabila");
                        _hud.ShowToast("BUKU CATATAN DISERAHKAN");
                    }
                    Action finish = () =>
                    {
                        _story.Notify(StoryEvent.Talk, key);
                        ChainOwnedScene(id);
                    };
                    string scene = TalkSceneFor(q.State, id, i);
                    if (scene != null) RunScene(scene, finish);
                    else Say(GreetingFor(id), finish);
                    return;
                }
                if (o.Event == StoryEvent.SceneDone && SceneOwner.TryGetValue(o.Key, out NpcId owner) && owner == id)
                {
                    string sceneId = o.Key;
                    // Adegan yang butuh barang misi: harus dibawa, lalu diserahkan.
                    if (SceneItems.TryGetValue(sceneId, out string needItem))
                    {
                        if (!D.inventory.Contains(needItem))
                        {
                            Say(PulangProfile.Token, MissingItemLine(needItem));
                            return;
                        }
                        D.inventory.Remove(needItem);
                        _hud.ShowToast((PulangItems.Get(needItem).Name + " diserahkan").ToUpperInvariant());
                    }
                    RunScene(sceneId, () => _story.Notify(StoryEvent.SceneDone, sceneId));
                    return;
                }
                if (o.Event == StoryEvent.Deliver && DeliveryTargets.TryGetValue(o.Key, out NpcId[] targets) &&
                    Array.IndexOf(targets, id) >= 0)
                {
                    string flag = "antar:" + o.Key + ":" + key;
                    if (!D.Has(flag))
                    {
                        string from = o.Key == "paket_dimas" ? "Dimas" : "Bu Sari";
                        string deliveryKey = o.Key;
                        // Barang misi: harus ada di inventori, dan berkurang satu per antaran.
                        if (!D.inventory.Contains(deliveryKey))
                        {
                            Say(PulangProfile.Token, "Barang titipannya tidak ada di tasku. Aku harus mengambilnya dari " + from + " dulu.");
                            return;
                        }
                        D.inventory.Remove(deliveryKey);
                        Say(new[]
                        {
                            new Line(PulangProfile.Token, "Permisi, ini titipan dari " + from + "."),
                            new Line(_npcs[id].DisplayName, "Wah, terima kasih! Barangnya sudah sampai."),
                        }, () =>
                        {
                            D.Set(flag);
                            _story.Notify(StoryEvent.Deliver, deliveryKey);
                        });
                        return;
                    }
                }
                if (q.Sequential) break;
            }

            if (GirlIndex(id) >= 0) { RomanceTalk(id); return; }
            Say(GreetingFor(id), null);
        }

        void ChainOwnedScene(NpcId id)
        {
            if (_state != State.Playing) return;
            int i = NextPendingIndex();
            if (i < 0) return;
            var o = _story.Current.Objectives[i];
            if (o.Event == StoryEvent.SceneDone && SceneOwner.TryGetValue(o.Key, out NpcId owner) && owner == id)
                RunScene(o.Key, () => _story.Notify(StoryEvent.SceneDone, o.Key));
        }

        Line[] GreetingFor(NpcId id)
        {
            string name = _npcs.ContainsKey(id) ? _npcs[id].DisplayName : id.ToString();
            if (id == NpcId.Ratna)
                return new[] { new Line("Ratna", "Ibu cuma berharap kamu bisa mendapatkan kehidupan yang lebih baik.") };
            if (PulangNpcLines.Greeting.TryGetValue(id, out string[] lines))
                return new[] { new Line(name, lines[0]), new Line(name, lines[1]) };
            return new[] { new Line(name, "Halo, {nama}.") };
        }

        // ================================================================== PDKT (tanpa ending pasangan)

        static readonly Dictionary<NpcId, string> GirlIntro = new Dictionary<NpcId, string>
        {
            { NpcId.Nabila, "Hai! Namaku Nabila. Senang kenalan denganmu!" },
            { NpcId.Kirana, "Permisi, kamu sedang mencari buku? Namaku Kirana." },
            { NpcId.Anindya, "Hei! Bisa bantu sebentar? Oh iya, namaku Anindya." },
            { NpcId.Melati, "Eh? Oh, tidak apa-apa. Aku hanya sedikit terkejut. Namaku Melati." },
            { NpcId.Citra, "Heiii! Hati-hati, jangan sampai menabrak orang! Namaku Citra." },
        };

        static readonly Dictionary<NpcId, string[]> GirlLines = new Dictionary<NpcId, string[]>
        {
            { NpcId.Nabila, new[] { "Kalau lewat sini lagi, mampir saja, ya!", "Senang juga bisa punya teman baru.",
                "Terima kasih sudah menemani soreku.", "Kamu memang teman yang bisa diandalkan." } },
            { NpcId.Kirana, new[] { "Senang bisa bertemu denganmu lagi, {nama}.", "Ternyata belajar bersama lebih menyenangkan.",
                "Semoga kita sama-sama bisa mencapai impian.", "Aku juga merasa nyaman berbicara denganmu." } },
            { NpcId.Anindya, new[] { "Menurutku kegiatan bersama seperti ini sangat penting.", "Kamu memang teman yang menyenangkan.",
                "Terima kasih sudah mau mendengarkan ceritaku.", "Aku senang kamu ada di sini." } },
            { NpcId.Melati, new[] { "Aku suka bunga-bunga di sini.", "Terkadang aku mendapatkan ide dari kejadian sehari-hari.",
                "Kadang lebih mudah menuliskan perasaan daripada mengatakannya langsung.", "Terima kasih sudah memahami aku." } },
            { NpcId.Citra, new[] { "Hidup itu jangan terlalu serius.", "Kalau bertemu lagi, jangan pura-pura tidak kenal, ya!",
                "Aku senang bisa menjadi diriku sendiri ketika bersamamu.", "Jangan lupa ajak aku jalan lagi!" } },
        };

        void RomanceTalk(NpcId id)
        {
            int gi = GirlIndex(id);
            var g = D.girls[gi];
            string name = _npcs[id].DisplayName;
            var lines = new List<Line>();

            if (!g.met)
            {
                g.met = true;
                lines.Add(new Line(name, GirlIntro[id]));
            }
            else
            {
                string[] pool = GirlLines[id];
                lines.Add(new Line(name, pool[g.episode % pool.Length]));
            }
            if (_today.Add("ngobrol:" + id)) { g.closeness += 5; CheckEpisode(gi); QueueStoryEvent(StoryEvent.GirlTalk, id.ToString()); }

            Say(lines, () =>
            {
                if (_today.Contains("hadiah:" + id)) return;
                var labels = new List<string>();
                var actions = new List<Action>();
                foreach (string itemId in new HashSet<string>(D.inventory))
                {
                    var item = PulangItems.Get(itemId);
                    if (item == null || item.Gift == null) continue;
                    labels.Add("BERI " + item.Name.ToUpperInvariant());
                    actions.Add(() => GiveGift(id, item));
                }
                if (labels.Count == 0) return;
                labels.Add("TIDAK");
                actions.Add(null);
                OpenMenu("BERI HADIAH?", labels, actions);
            });
        }

        void GiveGift(NpcId id, ItemDef item)
        {
            int gi = GirlIndex(id);
            var g = D.girls[gi];
            GiftTaste taste = PulangRomance.Girls[gi].Taste[(int)item.Gift.Value];
            D.inventory.Remove(item.Id);
            _today.Add("hadiah:" + id);
            QueueStoryEvent(StoryEvent.Gift, id.ToString());
            g.closeness += PulangRomance.GiftCloseness(taste);
            GameAudio.Play(taste >= GiftTaste.Suka ? "heart" : "talk", 0.6f);
            Say(_npcs[id].DisplayName, PulangRomance.GiftReaction(taste), () => CheckEpisode(gi));
        }

        void CheckEpisode(int gi)
        {
            var g = D.girls[gi];
            var def = PulangRomance.Girls[gi];
            if (g.episode == 0) g.episode = 1;
            while (g.episode < PulangRomance.EpisodeCount && g.closeness >= (g.episode + 1) * 10)
            {
                g.episode++;
                _hud.ShowToast((def.Id + " - EPISODE " + g.episode + ": " + def.Episodes[g.episode - 1]).ToUpperInvariant());
            }
            if (g.episode >= PulangRomance.EpisodeCount && !D.Has("kenangan_" + def.Id))
            {
                // Episode 10 menjadi kenangan hidup, bukan ending.
                D.Set("kenangan_" + def.Id);
                _hud.ShowToast(("KENANGAN BERSAMA " + def.Id).ToUpperInvariant());
            }
        }

        // ================================================================== lokasi

        void OpenLocation(LocationId loc)
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            if (BuildLocationMenu(loc, labels, actions) == 0) return;
            labels.Add("TUTUP");
            actions.Add(null);

            // Tempat yang menjual makanan / ada bekal: tampilkan energi saat ini di judul.
            bool food = false;
            foreach (var it in PulangItems.SoldAt(loc)) food |= it.Energy > 0;
            foreach (string l in labels) food |= l.StartsWith("MAKAN BEKAL");
            string title = PulangWorld.Get(loc).Name + (food ? "  -  ENERGI " + D.energy + "/100" : "");
            OpenMenu(title, labels, actions);
        }

        /// <summary>Isi menu sebuah lokasi. labels/actions boleh null (hanya menghitung).</summary>
        int BuildLocationMenu(LocationId loc, List<string> labels, List<Action> actions)
        {
            int count = 0;
            void Add(string label, Action action)
            {
                if (count >= 8) return;
                count++;
                labels?.Add(label.ToUpperInvariant());
                actions?.Add(action);
            }

            if (Array.IndexOf(TravelPoints, loc) >= 0 && D.state != StoryState.EndingPulang)
                Add("Naik bus", OpenTravelMenu);

            bool home = (loc == LocationId.KostPelangi && _story.Chapter >= Chapter.Ch1) ||
                        (loc == LocationId.RumahZidan && _story.Chapter == Chapter.Prolog);
            if (home && loc == LocationId.KostPelangi && !D.inventory.Contains("kunci_kost"))
            {
                // Barang misi: kamar kost baru bisa dipakai setelah mendapat kuncinya.
                Add("Kamar terkunci", () => Say(PulangProfile.Token,
                    "Aku belum punya kunci kamar ini. Sebaiknya aku temukan dulu Kost Pelangi sesuai petunjuk."));
                home = false;
            }
            if (home)
            {
                Add("Tidur", TrySleep);
                if (HasEdible()) Add(BekalLabel(), OpenBekalMenu);
                if (D.money > 0) Add("Menabung", OpenSavingsMenu);
                if (D.savings > 0) Add("Ambil tabungan", () => { D.money += D.savings; _hud.ShowToast("TABUNGAN DIAMBIL"); D.savings = 0; _story.Tick(); });
                if (loc == LocationId.KostPelangi && _story.CanCallMother && !_today.Contains("telepon_ibu"))
                    Add("Telepon Ibu", CallMother);
            }

            if (PulangWorld.IsInn(loc))
            {
                int price = PulangWorld.InnPrice[loc];
                LocationId inn = loc;
                Add("Menginap semalam - " + Rp(price), () => StayAtInn(inn));
                if (HasEdible()) Add(BekalLabel(), OpenBekalMenu);
            }

            foreach (var item in PulangItems.SoldAt(loc))
            {
                ItemDef it = item;
                // Barang misi hanya dijual saat dibutuhkan misi (dan belum dimiliki).
                if (it.Quest && (!IsPending(StoryEvent.Buy, it.Id, out _) || D.inventory.Contains(it.Id))) continue;
                bool eatNow = it.Energy > 0 && it.Gift == null;
                string verb = eatNow ? "Beli & makan " : "Beli ";
                // Keterangan energi: dimakan langsung, atau disimpan sebagai bekal.
                string energy = it.Energy <= 0 ? ""
                    : eatNow ? " (+" + it.Energy + " energi)"
                    : " (bekal +" + it.Energy + " energi)";
                Add(verb + it.Name + " - " + Rp(it.Price) + energy, () => BuyItem(it.Id, false));
            }

            foreach (var job in PulangJobs.All)
            {
                if (job.Place != loc) continue;
                JobDef j = job;
                bool held = D.jobMain == j.Id || D.jobSide == j.Id;
                if (held)
                {
                    Add("Kerja: " + j.Name + HoursLabel(j), () => StartShift(j, false));
                    Add("Berhenti: " + j.Name, () => Resign(j));
                }
                else if (D.state >= StoryState.Ch1PekerjaanPertama &&
                         (j.Tier == JobTier.Sederhana || D.state > StoryState.Ch3KesempatanBaru))
                {
                    Add("Lamar: " + j.Name, () => Apply(j));
                }
            }

            // Perpustakaan: kursus dibagi tiga kategori (dibuka di submenu agar daftar tidak terlalu panjang).
            if (loc == LocationId.Perpustakaan)
            {
                Add("Kursus keterampilan", () => OpenCourseMenu(SkillGroup.Keterampilan));
                Add("Kursus alam & petualangan", () => OpenCourseMenu(SkillGroup.Alam));
                Add("Pendidikan profesional", () => OpenCourseMenu(SkillGroup.Profesional));
            }

            if (loc == LocationId.BengkelJayaMotor && D.state == StoryState.Ch2MasalahKendaraan &&
                IsPending(StoryEvent.WorkShift, "bengkel", out _))
                Add("Bantu pekerjaan bengkel", () => StartShift(null, true));

            return count;
        }

        static string HoursLabel(JobDef j)
        {
            if (j.Flexible) return " (fleksibel)";
            return " (" + (j.Start / 60).ToString("00") + "-" + (j.End / 60).ToString("00") + ")";
        }

        void TrySleep()
        {
            if (D.minutes < 18 * 60 && D.energy >= 40)
            {
                Say(PulangProfile.Token, "Masih terlalu sore untuk tidur.");
                return;
            }
            SleepUntilMorning("Selamat pagi", 100);
        }

        /// <summary>Bayar penginapan lalu tidur sampai pagi; bangun di depan penginapan.</summary>
        void StayAtInn(LocationId inn)
        {
            int price = PulangWorld.InnPrice[inn];
            if (D.minutes < 18 * 60 && D.energy >= 40)
            {
                Say(" ", "Kamar baru bisa dipakai mulai pukul 18.00. Datang lagi nanti malam, atau kalau kamu sudah lelah.");
                return;
            }
            if (D.money < price)
            {
                GameAudio.Play("error", 0.5f);
                Say(" ", "Uangmu tidak cukup. Semalam di sini " + Rp(price) + ".");
                return;
            }
            D.money -= price;
            GameAudio.Play("coin", 0.5f);
            SleepUntilMorning("Bangun di " + PulangWorld.Get(inn).Name, 100, inn);
        }

        bool HasEdible()
        {
            foreach (string id in D.inventory)
            {
                var it = PulangItems.Get(id);
                if (it != null && it.Energy > 0) return true;
            }
            return false;
        }

        /// <summary>Makanan bekal di tas: id -> jumlah (urutan pertama kali ditemukan).</summary>
        List<KeyValuePair<ItemDef, int>> BekalInBag()
        {
            var list = new List<KeyValuePair<ItemDef, int>>();
            foreach (string id in D.inventory)
            {
                var it = PulangItems.Get(id);
                if (it == null || it.Energy <= 0) continue;
                int idx = list.FindIndex(p => p.Key.Id == it.Id);
                if (idx < 0) list.Add(new KeyValuePair<ItemDef, int>(it, 1));
                else list[idx] = new KeyValuePair<ItemDef, int>(it, list[idx].Value + 1);
            }
            return list;
        }

        /// <summary>Label menu bekal, mis. "Makan bekal: Roti (+15 energi)" atau "Makan bekal (3 jenis)".</summary>
        string BekalLabel()
        {
            var bekal = BekalInBag();
            if (bekal.Count == 1)
                return "Makan bekal: " + bekal[0].Key.Name + " (+" + bekal[0].Key.Energy + " energi)";
            return "Makan bekal (" + bekal.Count + " jenis)";
        }

        /// <summary>Satu jenis bekal: langsung makan. Beberapa jenis: pilih dari daftar beserta energinya.</summary>
        void OpenBekalMenu()
        {
            var bekal = BekalInBag();
            if (bekal.Count == 0) return;
            if (bekal.Count == 1) { EatFromBag(bekal[0].Key.Id); return; }

            var labels = new List<string>();
            var actions = new List<Action>();
            foreach (var pair in bekal)
            {
                string id = pair.Key.Id;
                labels.Add((pair.Key.Name + " x" + pair.Value + " (+" + pair.Key.Energy + " energi)").ToUpperInvariant());
                actions.Add(() => EatFromBag(id));
            }
            labels.Add("BATAL");
            actions.Add(null);
            OpenMenu("MAKAN BEKAL - ENERGI " + D.energy + "/100", labels, actions);
        }

        void EatFromBag() => EatFromBag(null);

        /// <summary>Makan satu bekal (id tertentu, atau yang pertama bila null).</summary>
        void EatFromBag(string wanted)
        {
            foreach (string id in D.inventory)
            {
                var it = PulangItems.Get(id);
                if (it == null || it.Energy <= 0) continue;
                if (wanted != null && it.Id != wanted) continue;
                D.inventory.Remove(id);
                GameAudio.Play("harvest", 0.5f);
                _hud.ShowToast(("MAKAN " + it.Name + " " + GainEnergy(it.Energy)).ToUpperInvariant());
                return;
            }
        }

        /// <summary>Tambah energi (maks 100) dan kembalikan teks mis. "+15 ENERGI (85/100)".</summary>
        string GainEnergy(int amount)
        {
            int before = D.energy;
            D.energy = Mathf.Min(100, D.energy + amount);
            int gained = D.energy - before;
            return "+" + gained + " ENERGI (" + D.energy + "/100)" + (gained < amount ? " PENUH" : "");
        }

        void OpenSavingsMenu()
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            foreach (int amount in new[] { 10000, 50000, 100000 })
            {
                if (D.money < amount) continue;
                int a = amount;
                labels.Add("SIMPAN " + Rp(a));
                actions.Add(() => Deposit(a));
            }
            labels.Add("SIMPAN SEMUA (" + Rp(D.money) + ")");
            actions.Add(() => Deposit(D.money));
            labels.Add("BATAL");
            actions.Add(null);
            OpenMenu("MENABUNG", labels, actions);
        }

        void Deposit(int amount)
        {
            amount = Mathf.Clamp(amount, 0, D.money);
            D.money -= amount;
            D.savings += amount;
            GameAudio.Play("coin", 0.6f);
            _hud.ShowToast("TABUNGAN " + Rp(D.savings));
            if (amount > 0) QueueStoryEvent(StoryEvent.Save, "*");
            _story.Tick();
        }

        void CallMother()
        {
            _today.Add("telepon_ibu");
            D.familyCloseness += 5;      // Daily Quest "Hubungi Ibu": +5 hubungan keluarga
            string reply = PulangScript.RatnaPhone[D.day % PulangScript.RatnaPhone.Length];
            Say(new[]
            {
                new Line(PulangProfile.Token, "Halo, Bu. Ini " + PulangProfile.Token + "."),
                new Line("Ratna", "Bagaimana keadaanmu di sana, Nak?"),
                new Line(PulangProfile.Token, "Baik, Bu."),
                new Line("Ratna", reply),
            }, null);
        }

        void BuyItem(string id, bool fromStory)
        {
            var it = PulangItems.Get(id);
            if (it == null) return;
            if (D.money < it.Price)
            {
                GameAudio.Play("error", 0.5f);
                _hud.ShowToast("UANGMU TIDAK CUKUP");
                return;
            }
            D.money -= it.Price;
            GameAudio.Play("coin", 0.6f);
            if (it.Energy > 0 && it.Gift == null)
            {
                _hud.ShowToast(GainEnergy(it.Energy));
            }
            else
            {
                D.inventory.Add(it.Id);
                _hud.ShowToast(("DAPAT " + it.Name +
                    (it.Energy > 0 ? " - BEKAL +" + it.Energy + " ENERGI" : "")).ToUpperInvariant());
            }
            QueueStoryEvent(StoryEvent.Buy, it.Id);
        }

        /// <summary>Daftar kursus satu kategori: level sekarang, harga level berikutnya, atau alasan terkunci.</summary>
        void OpenCourseMenu(SkillGroup group)
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            foreach (var skill in PulangSkills.All)
            {
                if (skill.Group != group) continue;
                SkillDef s = skill;
                int lv = PulangSkills.Level(D, s.Id);
                string lvText = s.MaxLevel > 1 ? " LV " + lv + "/" + s.MaxLevel : "";
                if (lv >= s.MaxLevel)
                {
                    labels.Add((s.Name + lvText + " - SELESAI").ToUpperInvariant());
                    actions.Add(() => Say(" ", "Kamu sudah menyelesaikan kursus " + s.Name + "."));
                }
                else if (D.level < s.MinPlayerLevel)
                {
                    labels.Add((s.Name + " - BUTUH LEVEL " + s.MinPlayerLevel).ToUpperInvariant());
                    actions.Add(() => Say(" ", "Kursus " + s.Name + " bisa diikuti mulai level " + s.MinPlayerLevel + ". " + s.Desc));
                }
                else
                {
                    labels.Add((s.Name + lvText + " - " + Rp(PulangSkills.PriceFor(s, lv + 1))).ToUpperInvariant());
                    actions.Add(() => BuyCourse(s));
                }
            }
            labels.Add("KEMBALI");
            actions.Add(() => OpenLocation(LocationId.Perpustakaan));
            string title = group == SkillGroup.Keterampilan ? "KURSUS KETERAMPILAN"
                : group == SkillGroup.Alam ? "KURSUS ALAM & PETUALANGAN" : "PENDIDIKAN PROFESIONAL";
            OpenMenu(title + "  -  " + Rp(D.money), labels, actions);
        }

        void BuyCourse(SkillDef s)
        {
            int next = PulangSkills.Level(D, s.Id) + 1;
            int price = PulangSkills.PriceFor(s, next);
            if (D.money < price) { GameAudio.Play("error", 0.5f); _hud.ShowToast("UANGMU TIDAK CUKUP"); return; }
            D.money -= price;
            PulangSkills.SetLevel(D, s.Id, next);
            GainExp(5 * next);                                  // belajar juga menambah pengalaman
            GameAudio.Play("win", 0.6f);
            _hud.ShowToast((s.MaxLevel > 1 ? "SKILL " + s.Name + " NAIK KE LEVEL " + next : "KURSUS " + s.Name + " SELESAI")
                .ToUpperInvariant());
            QueueStoryEvent(StoryEvent.Skill, s.Id);
        }

        // ================================================================== pekerjaan

        void Apply(JobDef j)
        {
            string fail = null;
            if (D.level < j.MinLevel) fail = "Butuh level " + j.MinLevel + ".";
            else if (j.NeedItem != null && !D.inventory.Contains(j.NeedItem))
                fail = "Butuh " + PulangItems.Get(j.NeedItem).Name.ToLowerInvariant() + ".";
            else if (!PulangSkills.MeetsJob(D, j.Id, out string skillReason))
                fail = skillReason;
            else if (!string.IsNullOrEmpty(D.jobMain) && !string.IsNullOrEmpty(D.jobSide))
                fail = "Kamu sudah punya 1 pekerjaan utama dan 1 sampingan.";

            if (fail != null)
            {
                GameAudio.Play("error", 0.5f);
                Say(" ", "Lamaran ditolak. " + fail);
                return;
            }

            if (string.IsNullOrEmpty(D.jobMain)) D.jobMain = j.Id; else D.jobSide = j.Id;
            UpdateJobTier();
            GameAudio.Play("win", 0.6f);
            Say(" ", "Kamu diterima sebagai " + j.Name + "." + (j.Flexible ? "" : " Jam kerja " + HoursLabel(j).Trim() + "."));
        }

        void Resign(JobDef j)
        {
            if (D.jobMain == j.Id) { D.jobMain = D.jobSide; D.jobSide = ""; }
            else if (D.jobSide == j.Id) D.jobSide = "";
            UpdateJobTier();
            _hud.ShowToast(("BERHENTI DARI " + j.Name).ToUpperInvariant());
        }

        void UpdateJobTier()
        {
            JobTier t = JobTier.None;
            foreach (string id in new[] { D.jobMain, D.jobSide })
            {
                var j = PulangJobs.Get(id);
                if (j != null && j.Tier > t) t = j.Tier;
            }
            D.jobTier = t;
            _story.Tick();
        }

        // Mini-game kerja: tiga kali tekan tepat waktu
        JobDef _workJob;
        bool _workHelp;
        int _workLate, _workRound, _workHits;
        float _workMarker, _workDir = 1f, _workZone;
        const int WorkRounds = 3;
        const float WorkZoneHalf = 0.12f;

        void StartShift(JobDef j, bool helpOnly)
        {
            string key = helpOnly ? "kerja:bantu_bengkel" : "kerja:" + j.Id;
            if (_today.Contains(key)) { Say(" ", "Kamu sudah bekerja di sini hari ini."); return; }
            if (D.energy < 20) { Say(PulangProfile.Token, "Aku terlalu lelah. Sebaiknya makan atau istirahat dulu."); return; }

            int late = 0;
            if (!helpOnly && !j.Flexible)
            {
                if (D.minutes >= j.End) { Say(" ", "Jam kerja sudah selesai. Datang lagi besok pukul " + (j.Start / 60).ToString("00") + ".00."); return; }
                if (D.minutes < j.Start - 60) { Say(" ", "Belum jam masuk. Kerja dimulai pukul " + (j.Start / 60).ToString("00") + ".00."); return; }
                late = Mathf.Max(0, D.minutes - j.Start);
            }

            _workJob = j;
            _workHelp = helpOnly;
            _workLate = late;
            _workRound = 0;
            _workHits = 0;
            _workMarker = 0f;
            _workDir = 1f;
            _workZone = UnityEngine.Random.Range(0.2f, 0.8f);
            _state = State.Work;
            _hud.SetPrompt(null);
            _hud.ShowWork(WorkRounds);
            _inputLock = 0.25f;
        }

        void UpdateWork(float dt)
        {
            float speed = 0.9f + 0.15f * (_workJob == null ? 0 : (int)_workJob.Tier);
            _workMarker += _workDir * speed * dt;
            if (_workMarker > 1f) { _workMarker = 1f; _workDir = -1f; }
            if (_workMarker < 0f) { _workMarker = 0f; _workDir = 1f; }
            _hud.UpdateWork(_workMarker, _workZone, WorkZoneHalf, _workRound, _workHits);

            if (_inputLock > 0f || !LifeInput.InteractPressed) return;
            bool hit = Mathf.Abs(_workMarker - _workZone) <= WorkZoneHalf;
            if (hit) _workHits++;
            GameAudio.Play(hit ? "serve" : "error", 0.55f);
            _workRound++;
            _workZone = UnityEngine.Random.Range(0.2f, 0.8f);
            _inputLock = 0.12f;
            if (_workRound >= WorkRounds) FinishShift();
        }

        void FinishShift()
        {
            _hud.HideWork();
            _state = State.Playing;
            float acc = _workHits / (float)WorkRounds;
            var notes = new List<Line>();
            int pay;

            if (_workHelp)
            {
                pay = 15000;                                   // side quest Pak Joko — Bengkel Sibuk
                _today.Add("kerja:bantu_bengkel");
                _clock = Mathf.Min(1439f, _clock + 120f);
                D.energy = Mathf.Max(0, D.energy - 15);
                GainExp(30);
                notes.Add(new Line("Pak Joko", "Bagus. Kalau mau belajar, kamu harus teliti."));
                QueueStoryEvent(StoryEvent.WorkShift, "bengkel");
            }
            else
            {
                var j = _workJob;
                int units = j.Id == "kurir_makanan" ? 3 : 1;
                pay = Mathf.RoundToInt((j.WageMin + (j.WageMax - j.WageMin) * acc) * units);

                float cut = PulangJobs.LatePenalty(_workLate, out string lateNote);
                pay = Mathf.RoundToInt(pay * (1f - cut));
                if (lateNote != null) notes.Add(new Line("HRD", lateNote));

                if (!j.Flexible)
                {
                    D.onTimeStreak = _workLate <= 0 ? D.onTimeStreak + 1 : 0;
                    int bonus = PulangJobs.AttendanceBonus(D.onTimeStreak, out string bonusNote);
                    if (bonus > 0) { pay += bonus; notes.Add(new Line("HRD", bonusNote + " +" + Rp(bonus))); }
                    _clock = Mathf.Min(1439f, Mathf.Max(_clock, j.Start) + (j.End - j.Start));
                }
                else _clock = Mathf.Min(1439f, _clock + 180f);

                _today.Add("kerja:" + j.Id);
                D.energy = Mathf.Max(0, D.energy - 30);
                GainExp(50 + _workHits * 5);
                QueueStoryEvent(StoryEvent.WorkShift, j.Id);
            }

            D.money += pay;
            D.minutes = (int)_clock;
            GameAudio.Play("coin", 0.7f);
            notes.Insert(0, new Line(" ", "Kerja selesai. Tepat " + _workHits + "/" + WorkRounds + ". Upah " + Rp(pay) + "."));
            Say(notes, () => _story.Tick());
        }

        void GainExp(int amount)
        {
            D.exp += amount;
            while (D.exp >= PulangJobs.ExpForNextLevel(D.level))
            {
                D.exp -= PulangJobs.ExpForNextLevel(D.level);
                D.level++;
                GameAudio.Play("win", 0.6f);
                _hud.ShowToast("NAIK LEVEL " + D.level + "!");
            }
        }

        // ================================================================== perjalanan

        void OpenTravelMenu()
        {
            var labels = new List<string>();
            var actions = new List<Action>();
            var questRegions = new HashSet<Region>();
            foreach (int i in PendingObjectiveIndices())
            {
                var o = _story.Current.Objectives[i];
                if (TryGetObjectiveRegion(o, out Region qr)) questRegions.Add(qr);
                foreach (var place in WorkShiftPlaces(o)) questRegions.Add(PulangWorld.Get(place).Region);
            }
            int questCount = 0;
            foreach (Region r in Enum.GetValues(typeof(Region)))
            {
                if (r == _world.CurrentRegion) continue;
                Region target = r;
                string label = "KE " + PulangWorld.RegionNames[r].ToUpperInvariant() + " - " + Rp(PulangWorld.BusFare[r]);
                foreach (string id in new[] { D.jobMain, D.jobSide })
                {
                    var job = PulangJobs.Get(id);
                    if (job != null && PulangWorld.Get(job.Place).Region == r) { label += "  [KERJA]"; break; }
                }
                if (questRegions.Contains(r))
                {
                    // Tujuan misi aktif: diberi bintang, label MISI, dan ditaruh paling atas.
                    labels.Insert(questCount, LifeHud.QuestMark + " " + label + "  [MISI]");
                    actions.Insert(questCount, () => TryTravel(target));
                    questCount++;
                }
                else
                {
                    labels.Add(label);
                    actions.Add(() => TryTravel(target));
                }
            }
            labels.Add("TIDAK JADI");
            actions.Add(null);
            OpenMenu("TERMINAL - PILIH TUJUAN", labels, actions);
        }

        /// <summary>Wilayah tempat tujuan sebuah misi berada.</summary>
        bool TryGetObjectiveRegion(Objective o, out Region region)
        {
            region = _world.CurrentRegion;
            switch (o.Event)
            {
                case StoryEvent.Travel:
                    return Enum.TryParse(o.Key, out region);
                case StoryEvent.Arrive:
                    if (!Enum.TryParse(o.Key, out LocationId l)) return false;
                    region = PulangWorld.Get(l).Region;
                    return true;
                case StoryEvent.Talk:
                    return Enum.TryParse(o.Key, out NpcId n) && TryNpcRegion(n, out region);
                case StoryEvent.SceneDone:
                    return SceneOwner.TryGetValue(o.Key, out NpcId owner) && TryNpcRegion(owner, out region);
                case StoryEvent.Deliver:
                    if (!DeliveryTargets.TryGetValue(o.Key, out NpcId[] targets)) return false;
                    foreach (var t in targets)
                        if (!D.Has("antar:" + o.Key + ":" + t) && TryNpcRegion(t, out region)) return true;
                    return false;
                case StoryEvent.Skill:
                case StoryEvent.Save:
                case StoryEvent.Buy:
                    var place = ObjectivePlace(o);
                    if (place == null) return false;
                    region = PulangWorld.Get(place.Value).Region;
                    return true;
            }
            return false;
        }

        /// <summary>Tempat untuk tugas kursus (perpustakaan), menabung (kost), atau membeli (toko penjualnya).</summary>
        static LocationId? ObjectivePlace(Objective o)
        {
            switch (o.Event)
            {
                case StoryEvent.Skill: return LocationId.Perpustakaan;
                case StoryEvent.Save: return LocationId.KostPelangi;
                case StoryEvent.Buy:
                    var it = PulangItems.Get(o.Key);
                    return it != null && it.SoldAt.Length > 0 ? it.SoldAt[0] : (LocationId?)null;
            }
            return null;
        }

        static bool TryNpcRegion(NpcId id, out Region region)
        {
            foreach (var def in PulangCast.All)
                if (def.Id == id) { region = PulangWorld.Get(def.Home).Region; return true; }
            region = default;
            return false;
        }

        void TryTravel(Region r)
        {
            int next = NextPendingIndex();
            bool travelNext = next >= 0 && _story.Current.Objectives[next].Event == StoryEvent.Travel;
            int fare = PulangWorld.BusFare[r];

            if (D.state == StoryState.PrologPersiapan)
            {
                if (r != Region.KotaJogokerto || !travelNext)
                {
                    Say(PulangProfile.Token, "Aku harus menyelesaikan persiapan merantau dulu.");
                    return;
                }
                if (D.money < fare) { Say(" ", "Uangmu tidak cukup untuk membeli tiket."); return; }
            }
            else
            {
                if (!_story.CanTravelTo(r, out string reason))
                {
                    GameAudio.Play("error", 0.5f);
                    Say(" ", reason);
                    return;
                }
                if (r == Region.DesaSukaMakmur && D.state == StoryState.Ch6PerjalananTerakhir && !travelNext)
                {
                    Say(PulangProfile.Token, "Masih ada yang harus kuselesaikan di Jogokerto.");
                    return;
                }
                fare = Mathf.Min(fare, D.money);
            }
            StartCoroutine(TravelRoutine(r, fare));
        }

        IEnumerator TravelRoutine(Region r, int fare)
        {
            _state = State.Busy;
            _hud.SetPrompt(null);
            GameAudio.Play("portal", 0.5f);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * 3f) { _hud.SetFade(t); yield return null; }

            _hud.ShowLoading(PulangWorld.RegionNames[r].ToUpperInvariant(), "NAIK BUS", "PERJALANAN MEMBUTUHKAN WAKTU 30 MENIT");
            for (float p = 0f; p < 1f; p += Time.unscaledDeltaTime * 1.2f) { _hud.SetLoading(p, null); yield return null; }

            D.money -= fare;
            _clock = Mathf.Min(1439f, _clock + 30f);
            D.minutes = (int)_clock;
            PlacePlayerAt(r, _world.ArrivalSpot(r));
            _hud.HideLoading();
            _hud.SetFade(1f);
            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * 3f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(0f);

            EnterPlaying();
            _hud.ShowToast(PulangWorld.RegionNames[r].ToUpperInvariant());
            _story.Notify(StoryEvent.Travel, r.ToString());
        }

        // ================================================================== minimap

        readonly List<MapMarker> _markers = new List<MapMarker>();
        static readonly Color MarkerPlace = new Color(0.92f, 0.92f, 0.98f, 0.75f);
        static readonly Color MarkerGoal = new Color(1f, 0.84f, 0.3f, 1f);
        static readonly Color MarkerGirl = new Color(1f, 0.55f, 0.75f, 1f);
        static readonly Color MarkerJob = new Color(0.45f, 0.80f, 1f, 1f);

        void RefreshMinimap()
        {
            _markers.Clear();
            Region here = _world.CurrentRegion;
            foreach (var loc in _world.LocationsIn(here))
                if (IsLocationOpen(loc)) _markers.Add(new MapMarker(_world.Spot(loc), MarkerPlace));

            foreach (var pair in _npcs)
                if (GirlIndex(pair.Key) >= 0 && pair.Value.gameObject.activeInHierarchy)
                    _markers.Add(new MapMarker(pair.Value.Position, MarkerGirl));

            // Setiap tujuan misi yang sedang berjalan mendapat penanda + penunjuk arah.
            _goals.Clear();
            bool needTerminal = false;
            foreach (int i in PendingObjectiveIndices())
            {
                var o = _story.Current.Objectives[i];
                int before = _goals.Count;
                switch (o.Event)
                {
                    case StoryEvent.Arrive:
                        if (Enum.TryParse(o.Key, out LocationId l) && PulangWorld.Get(l).Region == here)
                            AddGoal(_world.Spot(l), PulangWorld.Get(l).Name.ToUpperInvariant());
                        break;
                    case StoryEvent.Talk:
                        if (Enum.TryParse(o.Key, out NpcId n)) AddNpcGoal(n);
                        break;
                    case StoryEvent.SceneDone:
                        if (SceneOwner.TryGetValue(o.Key, out NpcId owner)) AddNpcGoal(owner);
                        break;
                    case StoryEvent.Deliver:
                        if (DeliveryTargets.TryGetValue(o.Key, out NpcId[] targets))
                            foreach (var t in targets)
                                if (!D.Has("antar:" + o.Key + ":" + t)) AddNpcGoal(t);
                        break;
                    case StoryEvent.Pickup:
                        if (_pickupObjects.TryGetValue(o.Key, out GameObject go) && go.activeInHierarchy)
                        {
                            int p = Array.FindIndex(Pickups, x => x.Id == o.Key);
                            AddGoal(go.transform.position, p >= 0 ? Pickups[p].Label.ToUpperInvariant() : "BARANG");
                        }
                        break;
                    case StoryEvent.Travel:
                        needTerminal = true;
                        break;
                    case StoryEvent.Skill:
                    case StoryEvent.Save:
                    case StoryEvent.Buy:
                    {
                        // Kursus -> perpustakaan, menabung -> kost, membeli -> toko penjual (wilayah ini dulu).
                        LocationId? place = null;
                        if (o.Event == StoryEvent.Buy)
                        {
                            var it = PulangItems.Get(o.Key);
                            if (it != null)
                                foreach (var l2 in it.SoldAt)
                                    if (PulangWorld.Get(l2).Region == here && IsLocationOpen(l2)) { place = l2; break; }
                        }
                        else place = ObjectivePlace(o);
                        if (place != null && PulangWorld.Get(place.Value).Region == here)
                            AddGoal(_world.Spot(place.Value), PulangWorld.Get(place.Value).Name.ToUpperInvariant());
                        break;
                    }
                    case StoryEvent.GirlTalk:
                    case StoryEvent.Gift:
                        // Semua teman perempuan yang sudah dikenal & ada di wilayah ini.
                        for (int gi = 0; gi < PulangRomance.Girls.Length; gi++)
                            if (_story.IsGirlUnlocked(gi) && D.girls[gi].met) AddNpcGoal(PulangRomance.Girls[gi].Id);
                        break;
                    case StoryEvent.WorkShift:
                    {
                        // Tempat kerja (mis. pekerjaan awal Quest 4); bila tak ada di wilayah ini -> terminal.
                        bool anyHere = false, anyElse = false;
                        foreach (var place in WorkShiftPlaces(o))
                        {
                            if (!PlaceInWorkHours(place)) continue;   // di luar jam kerja: tanpa penanda
                            if (PulangWorld.Get(place).Region == here)
                            {
                                AddGoal(_world.Spot(place), WorkPlaceLabel(place));
                                anyHere = true;
                            }
                            else anyElse = true;
                        }
                        if (!anyHere && anyElse) needTerminal = true;
                        break;
                    }
                }

                // Tujuan ada di wilayah lain (tempat/NPC di kota lain): arahkan ke terminal.
                if (_goals.Count == before && TryGetObjectiveRegion(o, out Region qr) && qr != here)
                    needTerminal = true;
            }
            if (needTerminal)
                foreach (var loc in _world.LocationsIn(here))
                    if (Array.IndexOf(TravelPoints, loc) >= 0) AddGoal(_world.Spot(loc), "TERMINAL");

            AddJobGoals(here);

            foreach (var g in _goals) _markers.Add(new MapMarker(g.Position, g.IsJob ? MarkerJob : MarkerGoal, g.IsJob, !g.IsJob));
            _hud.RefreshMinimap(here, _player.Feet, _player.Direction, _markers,
                PulangWorld.RegionNames[here].ToUpperInvariant());
            _hud.SetQuestTargets(_goals);
        }

        readonly List<QuestGoal> _goals = new List<QuestGoal>();

        void AddGoal(Vector2 position, string label, bool isJob = false)
        {
            foreach (var g in _goals)
                if ((g.Position - position).sqrMagnitude < 0.25f) return; // hindari penanda ganda
            _goals.Add(new QuestGoal(position, label, isJob));
        }

        /// <summary>
        /// Penunjuk tempat kerja: pekerjaan yang sedang dimiliki selalu ditunjuk;
        /// bila belum punya pekerjaan utama, tempat lowongan yang bisa dilamar ikut ditunjuk.
        /// Tempat kerja di wilayah lain diarahkan ke terminal.
        /// </summary>
        void AddJobGoals(Region here)
        {
            bool otherRegion = false;
            foreach (string id in new[] { D.jobMain, D.jobSide })
            {
                var j = PulangJobs.Get(id);
                if (j == null) continue;
                // Di luar jam kerja atau sudah bekerja hari ini: tidak ada penanda.
                if (!InWorkHours(j) || _today.Contains("kerja:" + j.Id)) continue;
                string label = "KERJA " + j.Name.ToUpperInvariant() + HoursLabel(j).ToUpperInvariant();
                if (PulangWorld.Get(j.Place).Region == here) AddGoal(_world.Spot(j.Place), label, true);
                else otherRegion = true;
            }

            if (string.IsNullOrEmpty(D.jobMain) && D.state >= StoryState.Ch1PekerjaanPertama)
            {
                // Kumpulkan lowongan per tempat agar satu tempat cukup satu penunjuk.
                var openings = new Dictionary<LocationId, List<string>>();
                foreach (var j in PulangJobs.All)
                {
                    if (!CanApplyNow(j) || !InWorkHours(j)) continue;
                    if (PulangWorld.Get(j.Place).Region != here) continue;
                    if (!openings.TryGetValue(j.Place, out List<string> names))
                        openings[j.Place] = names = new List<string>();
                    names.Add(j.Name.ToUpperInvariant());
                }
                foreach (var pair in openings)
                {
                    string label = "LOWONGAN " + pair.Value[0] +
                                   (pair.Value.Count > 1 ? " +" + (pair.Value.Count - 1) : "");
                    AddGoal(_world.Spot(pair.Key), label, true);
                }
            }

            if (otherRegion)
                foreach (var loc in _world.LocationsIn(here))
                    if (Array.IndexOf(TravelPoints, loc) >= 0) AddGoal(_world.Spot(loc), "TERMINAL (KERJA)", true);
        }

        /// <summary>
        /// Jam kerja sedang berlangsung: mulai 1 jam sebelum jam masuk (shift sudah boleh
        /// dimulai, lihat StartShift) sampai jam pulang. Pekerjaan fleksibel selalu buka.
        /// </summary>
        bool InWorkHours(JobDef j) =>
            j.Flexible || (D.minutes >= j.Start - 60 && D.minutes < j.End);

        /// <summary>Ada pekerjaan di tempat ini yang sedang dalam jam kerja.</summary>
        bool PlaceInWorkHours(LocationId place)
        {
            foreach (var j in PulangJobs.All)
                if (j.Place == place && InWorkHours(j)) return true;
            return false;
        }

        /// <summary>
        /// Tempat untuk tugas "selesaikan shift kerja". Key "*" (pekerjaan awal Quest 4):
        /// tempat kerja yang sudah dimiliki; bila belum punya, pekerjaan awal yang bisa
        /// dilamar sekarang (tempat buka, level & barang terpenuhi).
        /// </summary>
        List<LocationId> WorkShiftPlaces(Objective o)
        {
            var places = new List<LocationId>();
            if (o.Event != StoryEvent.WorkShift) return places;

            if (o.Key != "*")
            {
                var job = PulangJobs.Get(o.Key);
                if (job != null) places.Add(job.Place);
                return places;
            }

            foreach (string id in new[] { D.jobMain, D.jobSide })
            {
                var j = PulangJobs.Get(id);
                if (j != null && IsLocationOpen(j.Place) && !places.Contains(j.Place)) places.Add(j.Place);
            }
            if (places.Count > 0) return places;

            foreach (var j in PulangJobs.All)
                if (PulangJobs.IsStarterJob(j.Id) && CanApplyNow(j) && !places.Contains(j.Place))
                    places.Add(j.Place);
            if (places.Count > 0) return places;

            // Cadangan: semua tempat pekerjaan awal yang sudah buka.
            foreach (var j in PulangJobs.All)
                if (PulangJobs.IsStarterJob(j.Id) && IsLocationOpen(j.Place) && !places.Contains(j.Place))
                    places.Add(j.Place);
            return places;
        }

        /// <summary>Label penunjuk tempat kerja, mis. "WARUNG BU SARI - PENJAGA WARUNG".</summary>
        string WorkPlaceLabel(LocationId place)
        {
            foreach (var j in PulangJobs.All)
                if (j.Place == place && (PulangJobs.IsStarterJob(j.Id) || D.jobMain == j.Id || D.jobSide == j.Id))
                    return (PulangWorld.Get(place).Name + " - " + j.Name).ToUpperInvariant();
            return PulangWorld.Get(place).Name.ToUpperInvariant();
        }

        /// <summary>Lowongan yang ditawarkan di tempatnya dan syaratnya sudah terpenuhi.</summary>
        bool CanApplyNow(JobDef j)
        {
            if (D.jobMain == j.Id || D.jobSide == j.Id) return false;
            if (!(j.Tier == JobTier.Sederhana || D.state > StoryState.Ch3KesempatanBaru)) return false;
            if (!IsLocationOpen(j.Place)) return false;
            if (D.level < j.MinLevel) return false;
            if (j.NeedItem != null && !D.inventory.Contains(j.NeedItem)) return false;
            if (!PulangSkills.MeetsJob(D, j.Id, out _)) return false;
            return true;
        }

        void AddNpcGoal(NpcId id)
        {
            if (_npcs.TryGetValue(id, out LifeNpc npc) && npc.gameObject.activeInHierarchy)
                AddGoal(npc.Position, npc.DisplayName.ToUpperInvariant());
        }

        /// <summary>
        /// Tujuan misi yang bisa dikerjakan sekarang: hanya yang berikutnya bila misi
        /// berurutan, atau semua yang belum selesai bila bebas urutan.
        /// </summary>
        IEnumerable<int> PendingObjectiveIndices()
        {
            var q = _story.Current;
            for (int i = 0; i < q.Objectives.Length; i++)
            {
                if (D.objectiveProgress[i] >= q.Objectives[i].Count) continue;
                yield return i;
                if (q.Sequential) yield break;
            }
        }
    }
}
