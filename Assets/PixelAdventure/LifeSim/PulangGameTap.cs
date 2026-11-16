using System;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Kontrol ketuk / klik untuk menjalankan karakter:
    /// - ketuk tanah: berjalan ke sana (A*, memutari bangunan & perabot);
    /// - ketuk NPC / pintu / barang / kasir: berjalan ke sana lalu langsung berinteraksi;
    /// - ketuk dua kali dengan cepat: berlari;
    /// - tombol arah (WASD) kapan pun membatalkan jalur ketukan.
    /// Penanda kuning memantul menunjukkan tujuan.
    /// </summary>
    public partial class PulangGame
    {
        const float TapPickRadius = 1.3f;     // ketukan sedekat ini dengan objek = pilih objek itu
        const float DoubleTapTime = 0.35f;

        float _lastTapTime = -10f;
        Vector2 _lastTapWorld;
        bool _tapInteract;
        Transform _tapMarker;
        SpriteRenderer _tapArrow, _tapRing;

        /// <summary>Proses ketukan. True bila ketukan dipakai untuk membuka sesuatu (keluar dari update).</summary>
        bool HandleTap()
        {
            if (!LifeInput.PointerTapped || _inputLock > 0f) return false;
            if (_hud.GuideButtonClicked()) { OpenGuide(false); return true; }
            if (_hud.MinimapClicked()) { OpenWorldMap(false); return true; }
            if (_hud.PointerClick(UiHit.Toolbar, out int tool)) { RunToolbar(tool); return true; }
            if (_hud.PointerOverUi) return false;   // klik di atas panel HUD tidak menjadi "jalan ke sini"

            Vector2 tap = _cam.ScreenToWorldPoint(LifeInput.PointerPosition);
            bool run = Time.unscaledTime - _lastTapTime < DoubleTapTime &&
                       (tap - _lastTapWorld).sqrMagnitude < 2.25f;
            _lastTapTime = Time.unscaledTime;
            _lastTapWorld = tap;

            // Ketukan dekat objek interaktif: tuju objek itu, lalu interaksi saat sampai.
            Vector2 goal = tap;
            _tapInteract = TryPickInteractable(tap, out Vector2 objPos);
            if (_tapInteract)
            {
                goal = objPos;
                // Sudah cukup dekat: langsung interaksi tanpa berjalan.
                if ((objPos - _player.Feet).sqrMagnitude < 1.4f * 1.4f)
                {
                    _tapInteract = false;
                    _player.ClearPath();
                    UpdateInteraction();
                    Interact();
                    return true;
                }
            }

            var path = LifePathfinder.Find(_world, _player.Feet, goal);
            if (path.Count == 0)
            {
                _tapInteract = false;
                GameAudio.Play("error", 0.2f);
                return false;
            }
            _player.SetPath(path, run);
            GameAudio.Play("click", 0.2f);
            return false;
        }

        /// <summary>Objek interaktif terdekat dari titik ketukan (NPC, barang, pintu, bangunan, titik di dalam ruangan).</summary>
        bool TryPickInteractable(Vector2 tap, out Vector2 pos)
        {
            Vector2 bestPos = tap;
            float best = TapPickRadius * TapPickRadius;
            bool found = false;
            void Try(Vector2 p) { float d = (p - tap).sqrMagnitude; if (d < best) { best = d; bestPos = p; found = true; } }

            if (_world.InInterior)
            {
                foreach (var s in _world.InteriorSpots) Try(s.Pos);
                pos = bestPos;
                return found;
            }
            foreach (var pair in _npcs)
                if (pair.Value.gameObject.activeInHierarchy) Try(pair.Value.Position);
            foreach (var p in Pickups)
            {
                var go = _pickupObjects[p.Id];
                if (go.activeInHierarchy) Try(go.transform.position);
            }
            foreach (var loc in _world.LocationsIn(_world.CurrentRegion))
                if (IsLocationOpen(loc) && (_world.IsEnterable(loc) || BuildLocationMenu(loc, null, null) > 0))
                    Try(_world.Spot(loc));
            foreach (var d in _world.DecorIn(_world.CurrentRegion)) Try(d.Door);
            foreach (var l in _world.LitterIn(_world.CurrentRegion)) if (l.activeSelf) Try(l.transform.position);
            // Mengetuk badan bangunan (bukan pintunya) juga memilih pintunya.
            if (!found)
            {
                float wide = 3.2f * 3.2f;
                foreach (var loc in _world.LocationsIn(_world.CurrentRegion))
                {
                    if (!IsLocationOpen(loc) || !_world.IsEnterable(loc)) continue;
                    Vector2 door = _world.Spot(loc);
                    Vector2 body = door + new Vector2(0f, 3.5f);
                    float d = (body - tap).sqrMagnitude;
                    if (d < wide) { wide = d; bestPos = door; found = true; }
                }
            }
            pos = bestPos;
            return found;
        }

        /// <summary>Setelah karakter bergerak: interaksi otomatis saat tiba, penanda tujuan.</summary>
        void AfterTapMove()
        {
            if (_player.PathArrived && _tapInteract)
            {
                _tapInteract = false;
                UpdateInteraction();
                Interact();
            }
            else if (!_player.HasPath && !_player.PathArrived) _tapInteract = false;
            UpdateTapMarker();
        }

        /// <summary>Berjalan (lewat ketukan) ke keset pintu = keluar ruangan.</summary>
        bool TapReachedExit() =>
            _player.HasPath && _world.InteriorExitZone.Contains(_player.PathGoal) &&
            _world.InteriorExitZone.Contains(_player.Feet);

        void UpdateTapMarker()
        {
            if (_tapMarker == null)
            {
                _tapMarker = new GameObject("TapMarker").transform;
                _tapMarker.SetParent(transform, false);
                _tapRing = new GameObject("Ring").AddComponent<SpriteRenderer>();
                _tapRing.transform.SetParent(_tapMarker, false);
                _tapRing.sprite = PixelArt.Get("lc_shadow");
                _tapRing.sharedMaterial = PixelArt.SpriteMaterial;
                _tapRing.color = new Color(1f, 0.84f, 0.3f, 0.75f);
                _tapArrow = new GameObject("Arrow").AddComponent<SpriteRenderer>();
                _tapArrow.transform.SetParent(_tapMarker, false);
                _tapArrow.sprite = PixelArt.Get("lc_icon_direction");
                _tapArrow.sharedMaterial = PixelArt.SpriteMaterial;
                _tapArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                _tapArrow.transform.localScale = Vector3.one * 0.6f;
            }
            bool show = _player.HasPath && _state == State.Playing;
            if (_tapMarker.gameObject.activeSelf != show) _tapMarker.gameObject.SetActive(show);
            if (!show) return;
            Vector2 g = _player.PathGoal;
            float t = Time.unscaledTime;
            _tapMarker.position = new Vector3(g.x, g.y, 0f);
            _tapRing.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t * 6f));
            _tapArrow.transform.localPosition = new Vector3(0f, 0.75f + 0.15f * Mathf.Abs(Mathf.Sin(t * 5f)), 0f);
            int order = -Mathf.RoundToInt(g.y * PixelArt.PPU);
            _tapRing.sortingOrder = order - 2;
            _tapArrow.sortingOrder = order + 3;
        }
    }
}
