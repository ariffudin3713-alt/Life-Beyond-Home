using System;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Layar PETA (tombol F, ketuk minimap, atau aplikasi PETA di tablet):
    /// melihat semua wilayah berurutan kiri ke kanan, memilih wilayah dengan A/D atau ketuk,
    /// lalu melihat peta wilayah itu, daftar tempatnya, dan arah ke sana dari posisimu
    /// (keluar lewat ujung kiri/kanan peta, wilayah yang dilewati, waktu jalan kaki, ongkos bus).
    /// Juga mengisi petunjuk jalan keluar di minimap.
    /// </summary>
    public partial class PulangGame
    {
        static readonly Dictionary<Region, string> RegionShort = new Dictionary<Region, string>
        {
            { Region.DesaSukaMakmur, "DESA" },
            { Region.KampungCempaka, "CEMPAKA" },
            { Region.KotaJogokerto,  "KOTA" },
            { Region.PasarRamaiJaya, "PASAR" },
            { Region.PantaiCemara,   "PANTAI" },
        };

        static List<Region> _regionChain;
        int _mapSelected;

        /// <summary>Wilayah berurutan dari ujung paling kiri ke ujung paling kanan (mengikuti PulangWorld.EdgeRight).</summary>
        static List<Region> RegionChain()
        {
            if (_regionChain != null) return _regionChain;
            var chain = new List<Region>();
            Region start = Region.DesaSukaMakmur;
            foreach (Region r in Enum.GetValues(typeof(Region)))
                if (!PulangWorld.EdgeLeft.ContainsKey(r)) { start = r; break; }
            for (Region r = start; !chain.Contains(r); )
            {
                chain.Add(r);
                if (!PulangWorld.EdgeRight.TryGetValue(r, out r)) break;
            }
            foreach (Region r in Enum.GetValues(typeof(Region)))   // wilayah tanpa sambungan tetap tampil di ujung
                if (!chain.Contains(r)) chain.Add(r);
            _regionChain = chain;
            return chain;
        }

        bool RegionWalkOpen(Region r) => PulangWorld.CanWalkTo(r, _story.Chapter, out _);

        // ================================================================== minimap

        void SetMinimapExits(Region here)
        {
            string l = null, r = null;
            bool lo = false, ro = false;
            if (PulangWorld.EdgeLeft.TryGetValue(here, out Region lr)) { l = RegionShort[lr]; lo = RegionWalkOpen(lr); }
            if (PulangWorld.EdgeRight.TryGetValue(here, out Region rr)) { r = RegionShort[rr]; ro = RegionWalkOpen(rr); }
            _hud.SetMinimapExits(l, lo, r, ro);
        }

        // ================================================================== layar peta

        void OpenWorldMap(bool fromTablet)
        {
            _returnToTablet = fromTablet;
            _state = State.WorldMap;
            _hud.SetPrompt(null);
            _mapSelected = Mathf.Max(0, RegionChain().IndexOf(_world.CurrentRegion));
            _mapPlace = -1;
            GameAudio.Play("menu_open", 0.35f);
            RefreshWorldMap();
            _inputLock = 0.2f;
        }

        void UpdateWorldMap()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.CancelPressed || LifeInput.MapPressed || LifeInput.TabletPressed)
            {
                _hud.HideWorldMap();
                GameAudio.Play("click", 0.35f);
                CloseSubScreen();
                return;
            }

            // Pilih wilayah: A/D, ketuk kotak wilayah, atau E = kembali ke wilayahmu.
            int n = RegionChain().Count;
            int pick = _hud.WorldMapTappedRegion();
            if (pick < 0 && LifeInput.LeftPressed) pick = Mathf.Max(0, _mapSelected - 1);
            if (pick < 0 && LifeInput.RightPressed) pick = Mathf.Min(n - 1, _mapSelected + 1);
            if (pick < 0 && LifeInput.InteractPressed) pick = RegionChain().IndexOf(_world.CurrentRegion);
            if (pick >= 0 && pick != _mapSelected)
            {
                _mapSelected = pick;
                _mapPlace = -1;   // pilih ulang tempat bawaan untuk wilayah baru
                GameAudio.Play("click", 0.35f);
                RefreshWorldMap();
                return;
            }

            // Pilih tempat: W/S atau ketuk baris daftar / titik di peta.
            int places = _mapPlaceCount;
            if (places == 0) return;
            int place = _hud.WorldMapTappedPlace();
            if (place < 0 && _hud.PointerHover(UiHit.MapPlace, out int hoverPlace)) place = hoverPlace;   // arahkan ke daftar = pilih
            float wheel = LifeInput.ScrollWheel;
            if (place < 0 && wheel != 0f) place = Mathf.Clamp(_mapPlace + (wheel > 0f ? -1 : 1), 0, places - 1);
            if (place < 0 && LifeInput.UpPressed) place = (_mapPlace - 1 + places) % places;
            if (place < 0 && LifeInput.DownPressed) place = (_mapPlace + 1) % places;
            if (place >= 0 && place != _mapPlace)
            {
                _mapPlace = place;
                GameAudio.Play("click", 0.3f);
                RefreshWorldMap();
            }
        }

        int _mapPlace = -1, _mapPlaceCount;

        void RefreshWorldMap()
        {
            var chain = RegionChain();
            Region here = _world.CurrentRegion;
            Region sel = chain[Mathf.Clamp(_mapSelected, 0, chain.Count - 1)];
            int hi = chain.IndexOf(here), si = chain.IndexOf(sel);
            var v = new WorldMapView { Here = hi, Selected = si, Region = sel, RegionName = PulangWorld.RegionNames[sel] };

            // Wilayah yang berisi misi & tempat tujuan misi.
            var questRegions = new HashSet<Region>();
            var goalPlaces = new HashSet<LocationId>();
            // Fokus satu misi: selama misi sampingan diambil, peta hanya menandai tujuan misi itu.
            var focus = FocusedSideQuest(out SideQuestProgress focusProgress);
            if (focus != null)
            {
                var s = focus.Steps[focusProgress.step];
                LocationId? fl = null;
                if (s.Event == StoryEvent.Arrive && Enum.TryParse(s.Key, out LocationId al)) fl = al;
                else if (s.Event == StoryEvent.Talk && Enum.TryParse(s.Key, out NpcId fn)) fl = NpcHome(fn);
                else if (s.Event == StoryEvent.Save) fl = LocationId.KostPelangi;
                if (fl != null)
                {
                    questRegions.Add(PulangWorld.Get(fl.Value).Region);
                    if (s.Event != StoryEvent.Talk) goalPlaces.Add(fl.Value);
                }
            }
            else foreach (int i in PendingObjectiveIndices())
            {
                var o = _story.Current.Objectives[i];
                if (TryGetObjectiveRegion(o, out Region qr)) questRegions.Add(qr);
                if (o.Event == StoryEvent.Arrive && Enum.TryParse(o.Key, out LocationId l)) goalPlaces.Add(l);
                else if ((o.Event == StoryEvent.Skill || o.Event == StoryEvent.Save) && ObjectivePlace(o) is LocationId p) goalPlaces.Add(p);
            }
            foreach (var g in _goals) if (!g.IsJob) questRegions.Add(here);

            foreach (var r in chain)
                v.Regions.Add(new WorldMapRegion
                {
                    Name = PulangWorld.RegionNames[r],
                    Here = r == here,
                    Locked = r != here && !RegionWalkOpen(r),
                    Quest = questRegions.Contains(r),
                });

            // Posisi pemain (di dalam bangunan = pintu bangunan itu).
            bool atHere = sel == here;
            Vector2 me = _world.InInterior && _insideLoc.HasValue ? _world.Spot(_insideLoc.Value) : _player.Feet;
            if (atHere)
            {
                v.ShowPlayer = true;
                v.PlayerDir = _player.Direction;
                v.Player = me;
                foreach (var g in _goals) (g.IsJob ? v.Jobs : v.Goals).Add(g.Position);
                foreach (var pair in _npcs)
                    if (GirlIndex(pair.Key) >= 0 && pair.Value.gameObject.activeInHierarchy) v.Friends.Add(pair.Value.Position);
            }

            // Tempat yang sudah terbuka: tujuan misi dulu, lalu terminal, lalu sisanya (di wilayah ini: terdekat dulu).
            var places = new List<WorldMapPlace>();
            foreach (var loc in _world.LocationsIn(sel))
            {
                if (!IsLocationOpen(loc)) { v.LockedPlaces++; continue; }
                Vector2 pos = _world.Spot(loc);
                places.Add(new WorldMapPlace
                {
                    Name = PulangWorld.Get(loc).Name,
                    Position = pos,
                    Terminal = Array.IndexOf(TravelPoints, loc) >= 0,
                    Goal = goalPlaces.Contains(loc),
                    Meters = atHere ? Mathf.RoundToInt(Vector2.Distance(me, pos)) : -1,
                });
            }
            int Rank(WorldMapPlace p) => p.Goal ? 0 : p.Terminal ? 1 : 2;
            places.Sort((a, b) =>
            {
                int c = Rank(a).CompareTo(Rank(b));
                if (c != 0) return c;
                return atHere ? a.Meters.CompareTo(b.Meters) : string.CompareOrdinal(a.Name, b.Name);
            });
            v.Places.AddRange(places);
            _mapPlaceCount = places.Count;
            if (_mapPlace < 0 || _mapPlace >= places.Count) _mapPlace = places.Count > 0 ? 0 : -1;
            v.SelectedPlace = _mapPlace;

            if (PulangWorld.EdgeLeft.TryGetValue(sel, out Region lr)) { v.ExitLeft = RegionShort[lr]; v.ExitLeftOpen = RegionWalkOpen(lr); }
            if (PulangWorld.EdgeRight.TryGetValue(sel, out Region rr)) { v.ExitRight = RegionShort[rr]; v.ExitRightOpen = RegionWalkOpen(rr); }

            BuildRoute(v, chain, hi, si);
            _hud.ShowWorldMap(v);
        }

        void BuildRoute(WorldMapView v, List<Region> chain, int hi, int si)
        {
            Region sel = chain[si];
            if (hi < 0 || si == hi)
            {
                v.RouteHead = "KAMU BERADA DI WILAYAH INI.";
                if (v.ExitLeft != null)
                    v.Route.Add("UJUNG KIRI PETA: KE " + PulangWorld.RegionNames[chain[si - 1]] + (v.ExitLeftOpen ? "" : " (TERKUNCI)"));
                if (v.ExitRight != null)
                    v.Route.Add("UJUNG KANAN PETA: KE " + PulangWorld.RegionNames[chain[si + 1]] + (v.ExitRightOpen ? "" : " (TERKUNCI)"));
                v.Route.Add("BUS ANTARWILAYAH ADA DI TERMINAL (TANDA BIRU).");
                v.Route.Add("PILIH WILAYAH LAIN UNTUK MELIHAT ARAHNYA.");
                return;
            }

            bool right = si > hi;
            int dir = right ? 1 : -1;
            int steps = Mathf.Abs(si - hi);
            var path = new List<string>();
            string blocked = null;
            for (int i = hi + dir; ; i += dir)
            {
                path.Add(RegionShort[chain[i]]);
                if (blocked == null && !CanWalkTo(chain[i], out string reason)) blocked = reason;
                if (i == si) break;
            }

            if (blocked == null)
            {
                v.RouteOk = true;
                v.RouteHead = right ? "JALAN KE UJUNG KANAN PETA ▶" : "◀ JALAN KE UJUNG KIRI PETA";
            }
            else
            {
                v.RouteOk = false;
                v.RouteHead = "BELUM BISA BERJALAN KE SANA";
                v.Route.Add(blocked);
            }
            v.Route.Add("LEWAT: " + string.Join(" > ", path));
            v.Route.Add("JALAN KAKI: SEKITAR " + steps * EdgeWalkMinutes + " MENIT (" + steps + " PERBATASAN)");
            if (PulangWorld.BusFare.TryGetValue(sel, out int fare))
                v.Route.Add("ATAU NAIK BUS DARI TERMINAL: " + Rp(fare));
        }
    }
}
