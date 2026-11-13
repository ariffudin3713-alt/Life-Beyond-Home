using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Aplikasi tablet KONTAK / TELEPON / PESAN untuk orang yang sudah dikenal.
    ///
    /// - Dikenal: Ibu (sejak awal), NPC yang pernah diajak bicara atau berbicara dalam adegan,
    ///   dan kelima teman perempuan setelah bertemu.
    /// - PROFIL : potret, peran, usia, hobi, tempat tinggal, hari pertama bertemu, hadiah favorit, cerita singkat.
    /// - TELEPON: sekali sehari per orang, tidak diangkat pukul 22.00–06.00. Menelepon Ibu = "Telepon Ibu" harian
    ///   (+5 hubungan keluarga); menelepon teman perempuan +3 kedekatan. Memakan waktu 10 menit.
    /// - PESAN  : pilih pesan siap pakai, langsung dibalas. Pesan pertama per hari ke Ibu / teman perempuan +2.
    /// </summary>
    public partial class PulangGame
    {
        enum ContactTab { Profil, Telepon, Pesan }
        static readonly string[] ContactTabNames = { "PROFIL", "TELEPON", "PESAN" };
        const int MessagesPerContact = 10;

        ContactTab _contactTab;
        int _contactIndex;
        string _contactMsg;

        // ================================================================== siapa yang dikenal

        bool IsKnown(NpcId id)
        {
            if (id == NpcId.NpcKeluarga) return false;
            if (id == NpcId.Ratna) return true;
            int gi = GirlIndex(id);
            if (gi >= 0) return _story.IsGirlUnlocked(gi) && D.girls[gi].met;
            return D.Has("kenal:" + id);
        }

        void MarkKnown(NpcId id)
        {
            if (_story.Data == null || id == NpcId.NpcKeluarga || id == NpcId.Ratna) return;
            if (D.Has("kenal:" + id)) return;
            D.Set("kenal:" + id);
            D.Set("kenalhari:" + id + ":" + D.day);
        }

        /// <summary>Penutur dialog yang namanya sama dengan NPC = sudah berkenalan.</summary>
        void MarkKnownBySpeaker(string speaker)
        {
            if (_story.Data == null || string.IsNullOrEmpty(speaker)) return;
            foreach (var def in PulangCast.All)
                if (string.Equals(def.Name, speaker, StringComparison.OrdinalIgnoreCase)) { MarkKnown(def.Id); return; }
        }

        int KnownDay(NpcId id)
        {
            string prefix = "kenalhari:" + id + ":";
            foreach (string f in D.flags)
                if (f.StartsWith(prefix) && int.TryParse(f.Substring(prefix.Length), out int day)) return day;
            return -1;
        }

        List<NpcId> KnownContacts()
        {
            var list = new List<NpcId>();
            foreach (var def in PulangCast.All)
                if (IsKnown(def.Id)) list.Add(def.Id);
            return list;
        }

        // ================================================================== layar

        void OpenContacts(ContactTab tab)
        {
            _contactTab = tab;
            _pick = PickKind.None;
            _state = State.Contacts;
            _hud.SetPrompt(null);
            RefreshContacts();
            _inputLock = 0.2f;
        }

        void RefreshContacts()
        {
            var ids = KnownContacts();
            _contactIndex = Mathf.Clamp(_contactIndex, 0, Mathf.Max(0, ids.Count - 1));
            var rows = new List<ContactRow>();
            foreach (var id in ids)
                rows.Add(new ContactRow
                {
                    Name = NpcName(id),
                    Role = PulangProfile.Format(RoleOf(id)),
                    Portrait = Portrait(id),
                    Status = RowStatus(id),
                });
            var detail = ids.Count > 0 ? DetailFor(ids[_contactIndex]) : null;
            string header = ids.Count + " KONTAK   " + ClockLabel();
            string hint = "A/D - TAB   W/S - PILIH   E - " +
                          (_contactTab == ContactTab.Telepon ? "TELEPON" : _contactTab == ContactTab.Pesan ? "KIRIM PESAN" : "AKSI") +
                          "   ESC - KEMBALI";
            _hud.ShowContacts(ContactTabNames, (int)_contactTab, rows, _contactIndex, detail, header, _contactMsg, hint,
                BuildPicker());
        }

        // ================================================================== jendela pilihan

        enum PickKind { None, Actions, Message }
        PickKind _pick;
        int _pickSel;
        NpcId _pickWho;

        void OpenPicker(PickKind kind, NpcId who)
        {
            _pick = kind;
            _pickWho = who;
            _pickSel = 0;
            _contactMsg = null;
            GameAudio.Play("menu_open", 0.3f);
            RefreshContacts();
            _inputLock = 0.15f;
        }

        void ClosePicker()
        {
            _pick = PickKind.None;
            RefreshContacts();
        }

        /// <summary>Isi jendela pilihan sesuai jenisnya (null = tidak ada jendela).</summary>
        ContactPicker BuildPicker()
        {
            if (_pick == PickKind.None) return null;
            NpcId id = _pickWho;
            string name = NpcName(id).ToUpperInvariant();
            var p = new ContactPicker { Portrait = Portrait(id), Selected = _pickSel };
            if (_pick == PickKind.Actions)
            {
                p.Title = "HUBUNGI " + name;
                p.Note = RelationOf(id) + "   " + PhoneNumber(id);
                bool canCall = CanCall(id, out string why);
                bool dead = id == NpcId.Ratna && D.Has("ratna_wafat");
                p.Options.Add(new PickOption { Label = "TELEPON " + name, Enabled = canCall,
                    Sub = canCall ? "PANGGILAN SUARA - MEMAKAN WAKTU 10 MENIT" : why });
                p.Options.Add(new PickOption { Label = "KIRIM PESAN", Enabled = !dead,
                    Sub = dead ? "NOMOR TIDAK AKTIF" : "PILIH DARI " + PulangContacts.Templates.Length + " PESAN SIAP PAKAI" });
                p.Options.Add(new PickOption { Label = "BATAL", Enabled = true, Sub = "KEMBALI KE DAFTAR KONTAK" });
            }
            else
            {
                p.Title = "KIRIM PESAN KE " + name;
                bool close = id == NpcId.Ratna || GirlIndex(id) >= 0;
                p.Note = !close ? "PESAN AKAN LANGSUNG DIBALAS"
                    : _today.Contains("pesan:" + id) ? "BONUS HUBUNGAN HARI INI SUDAH DIDAPAT"
                    : "PESAN PERTAMA HARI INI: +2 HUBUNGAN";
                foreach (string t in PulangContacts.Templates)
                    p.Options.Add(new PickOption { Label = "\"" + t.ToUpperInvariant() + "\"", Enabled = true });
                p.Options.Add(new PickOption { Label = "BATAL", Enabled = true });
            }
            p.Selected = Mathf.Clamp(_pickSel, 0, p.Options.Count - 1);
            return p;
        }

        /// <summary>Masukan saat jendela pilihan terbuka. True bila masukan sudah ditangani.</summary>
        bool UpdatePicker()
        {
            if (_pick == PickKind.None) return false;
            var p = BuildPicker();
            int n = p.Options.Count;
            if (LifeInput.CancelPressed || LifeInput.TabletPressed) { GameAudio.Play("click", 0.35f); ClosePicker(); return true; }
            if (LifeInput.UpPressed || LifeInput.DownPressed)
            {
                _pickSel = (_pickSel + (LifeInput.UpPressed ? -1 : 1) + n) % n;
                GameAudio.Play("click", 0.35f);
                RefreshContacts();
                return true;
            }
            if (!LifeInput.InteractPressed) return true;

            var o = p.Options[_pickSel];
            if (!o.Enabled) { GameAudio.Play("error", 0.4f); return true; }
            NpcId id = _pickWho;
            PickKind kind = _pick;
            int sel = _pickSel;
            _pick = PickKind.None;
            if (sel == n - 1) { GameAudio.Play("click", 0.35f); RefreshContacts(); return true; }   // BATAL
            if (kind == PickKind.Actions)
            {
                if (sel == 0) { _contactTab = ContactTab.Telepon; TryCall(id); }
                else { _contactTab = ContactTab.Pesan; OpenMessageMenu(id); }
            }
            else SendContactMessage(id, sel);
            return true;
        }

        void UpdateContacts()
        {
            if (_inputLock > 0f) return;
            if (UpdatePicker()) return;
            if (LifeInput.CancelPressed || LifeInput.TabletPressed)
            {
                _hud.HideContacts();
                _contactMsg = null;
                OpenTabletWith(null);
                return;
            }
            var ids = KnownContacts();
            if (LifeInput.LeftPressed || LifeInput.RightPressed)
            {
                int dir = LifeInput.LeftPressed ? -1 : 1;
                _contactTab = (ContactTab)(((int)_contactTab + dir + 3) % 3);
                _contactMsg = null;
                GameAudio.Play("click", 0.35f);
                RefreshContacts();
                return;
            }
            if (ids.Count > 0 && (LifeInput.UpPressed || LifeInput.DownPressed))
            {
                int dir = LifeInput.UpPressed ? -1 : 1;
                _contactIndex = (_contactIndex + dir + ids.Count) % ids.Count;
                _contactMsg = null;
                GameAudio.Play("click", 0.35f);
                RefreshContacts();
                return;
            }
            if (!LifeInput.InteractPressed || ids.Count == 0) return;

            NpcId who = ids[_contactIndex];
            switch (_contactTab)
            {
                case ContactTab.Telepon: TryCall(who); break;
                case ContactTab.Pesan: OpenMessageMenu(who); break;
                default: OpenContactActions(who); break;
            }
        }

        void ContactError(string message)
        {
            GameAudio.Play("error", 0.4f);
            _contactMsg = message;
            RefreshContacts();
        }

        /// <summary>Tab PROFIL: E membuka pilihan Telepon / Kirim pesan.</summary>
        void OpenContactActions(NpcId id) => OpenPicker(PickKind.Actions, id);

        // ================================================================== telepon

        static bool LateNight(int minutes) => minutes >= 22 * 60 || minutes < 6 * 60;

        bool CanCall(NpcId id, out string why)
        {
            why = null;
            if (id == NpcId.Ratna)
            {
                if (D.Has("ratna_wafat")) { why = "NOMOR INI TIDAK LAGI AKTIF"; return false; }
                return CanPhoneNow(out why);
            }
            if (_today.Contains("telp:" + id)) { why = "SUDAH MENELEPON " + NpcName(id).ToUpperInvariant() + " HARI INI"; return false; }
            if (LateNight(D.minutes)) { why = "SUDAH LARUT - " + NpcName(id).ToUpperInvariant() + " TIDAK MENGANGKAT (22.00-06.00)"; return false; }
            return true;
        }

        void TryCall(NpcId id)
        {
            if (!CanCall(id, out string why))
            {
                ContactError(why);
                return;
            }
            _hud.HideContacts();
            SetLastCall(id);
            Action back = () => { _contactMsg = "PANGGILAN DENGAN " + NpcName(id).ToUpperInvariant() + " SELESAI"; OpenContacts(ContactTab.Telepon); };
            if (id == NpcId.Ratna) { CallMother(back); return; }

            _today.Add("telp:" + id);
            GameAudio.Play("phone", 0.5f);
            _clock = Mathf.Min(1439f, _clock + 10f);
            D.minutes = (int)_clock;
            int gi = GirlIndex(id);
            if (gi >= 0) { D.girls[gi].closeness += 3; CheckEpisode(gi); }

            var info = PulangContacts.Get(id);
            string name = NpcName(id);
            string topic = info.Call.Length > 0 ? info.Call[(D.day + (int)id) % info.Call.Length] : "Senang mendengar suaramu.";
            Say(new[]
            {
                new Line(PulangProfile.Token, "Halo, " + name + "? Ini " + PulangProfile.Token + "."),
                new Line(name, "Oh, halo " + PulangProfile.Token + "! " + topic),
                new Line(PulangProfile.Token, gi >= 0 ? "Senang bisa mengobrol denganmu." : "Terima kasih, nanti kita ngobrol lagi."),
                new Line(name, gi >= 0 ? "Aku juga. Telepon lagi ya!" : "Iya, sampai jumpa!"),
            }, back);
        }

        void SetLastCall(NpcId id)
        {
            string prefix = id + "|";
            D.calls.RemoveAll(c => c.StartsWith(prefix));
            D.calls.Add(prefix + D.day);
        }

        int LastCallDay(NpcId id)
        {
            string prefix = id + "|";
            foreach (string c in D.calls)
                if (c.StartsWith(prefix) && int.TryParse(c.Substring(prefix.Length), out int d)) return d;
            return -1;
        }

        /// <summary>Nomor HP rekaan yang tetap untuk setiap orang, mis. "0812-4471-2093".</summary>
        static string PhoneNumber(NpcId id)
        {
            uint h = (uint)((int)id * 2654435761u + 97u);
            int a = 11 + (int)(h % 9), b = 1000 + (int)((h >> 8) % 9000), c = 1000 + (int)((h >> 16) % 9000);
            return "08" + a + "-" + b + "-" + c;
        }

        // ================================================================== pesan

        /// <summary>Tab PESAN / pilihan "Kirim pesan": buka jendela daftar pesan siap pakai.</summary>
        void OpenMessageMenu(NpcId id)
        {
            if (id == NpcId.Ratna && D.Has("ratna_wafat")) { ContactError("NOMOR INI TIDAK LAGI AKTIF"); return; }
            OpenPicker(PickKind.Message, id);
        }

        void SendContactMessage(NpcId id, int template)
        {
            int gi = GirlIndex(id);
            string reply = PulangContacts.Reply(id, template, D.day, gi >= 0);
            AddMessage(id, true, PulangContacts.Templates[template]);
            AddMessage(id, false, reply);
            GameAudio.Play("notify", 0.5f);

            // Pesan pertama hari ini ke orang terdekat menambah kedekatan.
            if (_today.Add("pesan:" + id))
            {
                if (id == NpcId.Ratna) D.familyCloseness += 2;
                else if (gi >= 0) { D.girls[gi].closeness += 2; CheckEpisode(gi); }
            }
            _contactMsg = NpcName(id).ToUpperInvariant() + " MEMBALAS PESANMU";
            OpenContacts(ContactTab.Pesan);
        }

        void AddMessage(NpcId id, bool mine, string text)
        {
            D.messages.Add(id + "|" + (mine ? 1 : 0) + "|" + D.day + "|" + text.Replace('|', '/'));
            // simpan hanya pesan terbaru per orang
            string prefix = id + "|";
            int count = 0;
            for (int i = D.messages.Count - 1; i >= 0; i--)
                if (D.messages[i].StartsWith(prefix) && ++count > MessagesPerContact) D.messages.RemoveAt(i);
        }

        List<ChatLine> MessagesWith(NpcId id)
        {
            var list = new List<ChatLine>();
            string prefix = id + "|";
            foreach (string m in D.messages)
            {
                if (!m.StartsWith(prefix)) continue;
                string[] p = m.Split(new[] { '|' }, 4);
                if (p.Length < 4) continue;
                list.Add(new ChatLine { Mine = p[1] == "1", Text = PulangProfile.Format(p[3]) });
            }
            return list;
        }

        // ================================================================== isi tampilan

        static string RoleOf(NpcId id)
        {
            foreach (var def in PulangCast.All) if (def.Id == id) return def.Role;
            return "";
        }

        static string Portrait(NpcId id) =>
            PulangNpcLines.Sprite.TryGetValue(id, out string s) ? "lc_" + s + "_down_0" : null;

        string RowStatus(NpcId id)
        {
            switch (_contactTab)
            {
                case ContactTab.Telepon:
                    return _today.Contains("telp:" + id) || (id == NpcId.Ratna && _today.Contains("telepon_ibu")) ? "✓" : "";
                case ContactTab.Pesan:
                    int n = MessagesWith(id).Count;
                    return n > 0 ? n.ToString() : "";
                default:
                    return GirlIndex(id) >= 0 ? "★" : "";
            }
        }

        string RelationOf(NpcId id)
        {
            if (id == NpcId.Ratna) return D.Has("ratna_wafat") ? "KELUARGA - KENANGAN" : "KELUARGA";
            int gi = GirlIndex(id);
            if (gi >= 0)
            {
                int ep = D.girls[gi].episode;
                return ep >= 7 ? "SAHABAT DEKAT" : ep >= 3 ? "TEMAN BAIK" : "TEMAN";
            }
            return id == NpcId.Dimas || id == NpcId.PakHendra ? "TEMAN" : "KENALAN";
        }

        ContactDetail DetailFor(NpcId id)
        {
            var info = PulangContacts.Get(id);
            var d = new ContactDetail
            {
                Name = NpcName(id),
                Role = PulangProfile.Format(RoleOf(id)),
                Relation = RelationOf(id),
                Portrait = Portrait(id),
            };
            int gi = GirlIndex(id);
            if (id == NpcId.Ratna && !D.Has("ratna_wafat"))
            {
                d.HasBond = true;
                d.Bond = D.familyCloseness / 100f;
                d.BondLabel = "HUBUNGAN KELUARGA  " + D.familyCloseness + " POIN";
            }
            else if (gi >= 0)
            {
                d.HasBond = true;
                d.Bond = D.girls[gi].episode / (float)PulangRomance.EpisodeCount;
                d.BondLabel = "KEDEKATAN  EPISODE " + D.girls[gi].episode + "/" + PulangRomance.EpisodeCount;
            }

            void Row(string label, string value) => d.Rows.Add(new JournalStat { Label = label, Value = value });
            switch (_contactTab)
            {
                case ContactTab.Telepon:
                {
                    Row("NOMOR", PhoneNumber(id));
                    bool ok = CanCall(id, out string why);
                    Row("STATUS", ok ? "BISA DITELEPON" : why);
                    int last = LastCallDay(id);
                    Row("TERAKHIR", last < 0 ? "BELUM PERNAH" : last == D.day ? "HARI INI" : "HARI " + last);
                    Row("JAM AKTIF", id == NpcId.Ratna ? "SEKALI SEHARI" : "06.00 - 22.00");
                    d.AboutTitle = "MANFAAT";
                    d.About = id == NpcId.Ratna
                        ? "Menelepon Ibu setiap hari menambah hubungan keluarga +5."
                        : gi >= 0 ? "Menelepon sekali sehari menambah kedekatan +3. Panggilan memakan waktu 10 menit."
                        : "Menjaga silaturahmi dengan warga. Panggilan memakan waktu 10 menit.";
                    d.Action = ok ? "E - TELEPON " + d.Name.ToUpperInvariant() : "TIDAK BISA MENELEPON";
                    d.ActionEnabled = ok;
                    break;
                }
                case ContactTab.Pesan:
                {
                    d.ShowChat = true;
                    d.Chat.AddRange(MessagesWith(id));
                    bool dead = id == NpcId.Ratna && D.Has("ratna_wafat");
                    d.Action = dead ? "NOMOR TIDAK AKTIF" : "E - KIRIM PESAN";
                    d.ActionEnabled = !dead;
                    break;
                }
                default:
                {
                    Row("USIA", info.Age);
                    Row("HOBI", info.Hobby);
                    var home = PulangWorld.Get(HomeOf(id));
                    Row("TINGGAL", home.Name + ", " + PulangWorld.RegionNames[home.Region]);
                    int day = KnownDay(id);
                    Row("BERTEMU", id == NpcId.Ratna ? "SEJAK LAHIR" : day > 0 ? "HARI " + day : "SUDAH LAMA");
                    if (gi >= 0) Row("SUKA", FavoriteGifts(gi));
                    d.AboutTitle = "TENTANG " + d.Name.ToUpperInvariant();
                    d.About = PulangProfile.Format(info.Bio);
                    d.Action = "E - TELEPON / KIRIM PESAN";
                    d.ActionEnabled = true;
                    break;
                }
            }
            return d;
        }

        static LocationId HomeOf(NpcId id)
        {
            foreach (var def in PulangCast.All) if (def.Id == id) return def.Home;
            return LocationId.KostPelangi;
        }

        static string FavoriteGifts(int gi)
        {
            var taste = PulangRomance.Girls[gi].Taste;
            var best = new List<string>();
            for (int i = 0; i < taste.Length; i++)
                if (taste[i] == GiftTaste.SangatSuka) best.Add(((Gift)i).ToString());
            if (best.Count == 0)
                for (int i = 0; i < taste.Length; i++)
                    if (taste[i] == GiftTaste.Suka) best.Add(((Gift)i).ToString());
            return best.Count > 0 ? string.Join(", ", best).ToUpperInvariant() : "-";
        }
    }
}
