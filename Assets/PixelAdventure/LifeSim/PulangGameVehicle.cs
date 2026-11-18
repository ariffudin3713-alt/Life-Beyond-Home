using System.Collections;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Kendaraan pribadi (sepeda, skuter matic, sepeda motor, mobil) dari Dealer Kendaraan Maju.
    ///
    /// - Kendaraan yang dibeli disimpan di inventori (tab KENDARAAN).
    /// - Tekan V (atau pilih di inventori lalu E) untuk MEMANGGIL kendaraan di mana saja di luar
    ///   ruangan; tekan V lagi untuk MEMARKIR / menyimpannya kembali ke inventori.
    /// - Punya lebih dari satu kendaraan: V membuka menu pilihan.
    /// - Masuk bangunan: kendaraan otomatis diparkir di luar dan dinaiki lagi saat keluar.
    /// - Di terminal: "Berkendara sendiri" ke wilayah lain (bensin / gratis untuk sepeda).
    /// </summary>
    public partial class PulangGame
    {
        const float BaseWalkSpeed = 4.6f;
        string _vehicleShown;

        /// <summary>Kendaraan yang sedang dinaiki (null = jalan kaki).</summary>
        ItemDef RidingVehicle
        {
            get
            {
                if (_story.Data == null || string.IsNullOrEmpty(D.vehicle)) return null;
                var it = PulangItems.Get(D.vehicle);
                // Simpanan rusak / kendaraan tidak dimiliki lagi: turun.
                if (it == null || !it.IsVehicle || !D.inventory.Contains(it.Id)) { D.vehicle = ""; return null; }
                return it;
            }
        }

        /// <summary>Kendaraan milik pemain, urut sesuai daftar dealer.</summary>
        List<ItemDef> OwnedVehicles()
        {
            var list = new List<ItemDef>();
            foreach (var it in PulangItems.All)
                if (it.IsVehicle && D.inventory.Contains(it.Id)) list.Add(it);
            return list;
        }

        /// <summary>
        /// Atur kecepatan & gambar pemain sesuai kendaraan (hanya di luar ruangan).
        /// True bila sedang berkendara (tidak bisa berlari, tidak menguras energi).
        /// </summary>
        bool ApplyVehicleSpeed()
        {
            var v = _world.InInterior ? null : RidingVehicle;
            _player.SetSpeed(BaseWalkSpeed * (v != null ? v.Speed : 1f));
            string visual = v != null ? v.Id : "";
            if (visual != _vehicleShown)
            {
                _vehicleShown = visual;
                _player.SetVehicleVisual(visual);
            }
            return v != null;
        }

        /// <summary>Tombol V: parkir bila sedang berkendara, atau panggil kendaraan.</summary>
        void ToggleVehicle()
        {
            if (RidingVehicle != null) { ParkVehicle(); return; }

            var owned = OwnedVehicles();
            if (owned.Count == 0)
            {
                GameAudio.Play("error", 0.4f);
                _hud.ShowToast("BELUM PUNYA KENDARAAN - BELI DI DEALER KENDARAAN MAJU (KOTA JOGOKERTO)");
                return;
            }
            if (!CanSummonVehicle(out string reason))
            {
                GameAudio.Play("error", 0.4f);
                _hud.ShowToast(reason);
                return;
            }
            if (owned.Count == 1) { SummonVehicle(owned[0]); return; }

            var labels = new List<string>();
            var actions = new List<System.Action>();
            foreach (var it in owned)
            {
                ItemDef v = it;
                labels.Add((v.Name + "  -  x" + v.Speed.ToString("0.##") + " KECEPATAN").ToUpperInvariant());
                actions.Add(() => SummonVehicle(v));
            }
            labels.Add("TIDAK JADI");
            actions.Add(null);
            OpenMenu("PANGGIL KENDARAAN", labels, actions);
        }

        bool CanSummonVehicle(out string reason)
        {
            reason = null;
            if (_world.InInterior) reason = "TIDAK BISA MEMANGGIL KENDARAAN DI DALAM RUANGAN";
            else if (_player != null && _player.SitBusy) reason = "BERDIRI DULU SEBELUM MEMANGGIL KENDARAAN";
            return reason == null;
        }

        void SummonVehicle(ItemDef v)
        {
            if (v == null) return;
            if (!CanSummonVehicle(out string reason))
            {
                GameAudio.Play("error", 0.4f);
                _hud.ShowToast(reason);
                return;
            }
            D.vehicle = v.Id;
            ApplyVehicleSpeed();
            GameAudio.Play(v.Engine ? "engine_start" : "bike_bell", 0.55f);
            SpawnPuff(_player.Feet);
            _hud.ShowToast(("NAIK " + v.Name + " - TEKAN V UNTUK MENYIMPAN").ToUpperInvariant());
        }

        void ParkVehicle()
        {
            var v = RidingVehicle;
            if (v == null) return;
            D.vehicle = "";
            ApplyVehicleSpeed();
            GameAudio.Play("pickup", 0.45f);
            SpawnPuff(_player.Feet);
            _hud.ShowToast((v.Name + " disimpan ke inventori").ToUpperInvariant());
        }

        /// <summary>Kepulan asap kecil saat kendaraan muncul / disimpan.</summary>
        void SpawnPuff(Vector2 at)
        {
            if (_world.InInterior) return;
            StartCoroutine(PuffRoutine(at));
        }

        IEnumerator PuffRoutine(Vector2 at)
        {
            var go = new GameObject("AsapKendaraan");
            go.transform.position = new Vector3(at.x, at.y - 0.1f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = -Mathf.RoundToInt((at.y - 0.4f) * PixelArt.PPU);
            const float dur = 0.45f;
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                int f = Mathf.Min(2, (int)(t / dur * 3f));
                sr.sprite = PixelArt.Get("lc_puff_" + f);
                go.transform.localScale = Vector3.one * (1f + t * 1.6f);
                yield return null;
            }
            Destroy(go);
        }

        // ================================================================== perjalanan antarwilayah

        /// <summary>Label tarif perjalanan sendiri, mis. "BENSIN Rp4.000, 20 MENIT".</summary>
        static string OwnTripCost(ItemDef v) =>
            (v.Engine ? "BENSIN " + Rp(v.Fuel) : "GRATIS") + ", " + v.TravelMinutes + " MENIT";

        /// <summary>Biaya, lama, dan nama moda perjalanan ke wilayah lain.</summary>
        void TripInfo(Region r, bool own, out int fare, out int minutes, out string mode)
        {
            var v = own ? RidingVehicle : null;
            if (v != null)
            {
                fare = v.Engine ? v.Fuel : 0;
                minutes = v.TravelMinutes;
                mode = (v.Engine ? "BERKENDARA " : "BERSEPEDA ") + "DENGAN " + v.Name.ToUpperInvariant();
            }
            else
            {
                fare = PulangWorld.BusFare[r];
                minutes = 30;
                mode = "NAIK BUS";
            }
        }
    }
}
