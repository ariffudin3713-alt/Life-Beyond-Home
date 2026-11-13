# Life Beyond Home

Game simulasi kehidupan 2D pixel art. Tokoh utama (default **Zidan**, nama bisa
diganti di awal) merantau dari **Desa Suka Makmur** ke **Kota Jogokerto** untuk
membangun masa depan. Cerita berjalan dari Prolog sampai Chapter 6 dan hanya
punya satu ending: **PULANG**.

Semua sprite, UI, musik, dan efek suara dibuat saat runtime, tanpa aset eksternal.

## Cara menjalankan

1. Buka proyek dengan **Unity 6** (`6000.6.8f1`).
2. Buka `Assets/Scenes/SampleScene.unity`, lalu tekan **Play**.
3. Di layar judul tekan **Enter**, lalu ketik nama tokoh utama.
   **Enter** tanpa mengetik atau **Esc** memakai nama **Zidan**.

## Kontrol

| Aksi | Tombol |
| --- | --- |
| Jalan | `WASD` / panah |
| Bicara, pilih, lanjut | `E`, `Spasi`, `Enter` |
| Jurnal | `J` |
| Panggil / simpan kendaraan | `V` |
| Jeda | `P` / `Esc` |
| Suara | `M` |
| Mulai baru (di judul) | `N` dua kali |

## Alur cerita

| Bagian | Isi |
| --- | --- |
| Prolog | Bersama Ibu (Ratna), keputusan merantau, Quest 1 Persiapan Merantau |
| Chapter 1 | Tiba di Jogokerto, Pak Hendra, Dimas, Bu Sari, pekerjaan pertama, Nabila (Quest 2–5) |
| Chapter 2 | Membantu Dimas, masalah di pasar, bengkel Pak Joko (Quest 6–8) |
| Chapter 3 | Tawaran Pak Budi (Quest 9), kursus keterampilan (Quest 10), Melati & Citra (Quest 11), dunia bisnis bersama Pak Darto & Bu Sari (Quest 12), lalu capai level 5 |
| Chapter 4 | Menemani Dimas mencari kerja (Quest 13), surat Pak Hendra & cerita masa lalu (Quest 14), waktu bersama teman (Quest 15), lalu capai level 10 |
| Chapter 5 | Pesan dari Ibu & kiriman kain batik (Quest 16), kerja bakti Kampung Cempaka (Quest 17), lalu kerja profesional & tabungan Rp2.000.000 |
| Chapter 6 | Telepon kabar duka, Perjalanan Terakhir |
| Ending | **PULANG** — satu-satunya ending |

Desa Suka Makmur terkunci sejak Chapter 1 sampai kabar duka. Lima karakter
perempuan (Nabila, Kirana, Anindya, Melati, Citra) punya 10 episode kedekatan
masing-masing, tanpa ending pasangan.

## Wilayah

Desa Suka Makmur, Kota Jogokerto, Kampung Cempaka, Pasar Ramai Jaya, dan Pantai
Cemara, terhubung lewat terminal/halte bus (Rp4.000–Rp5.000, 30 menit di dalam game).

## Sistem

- **Waktu**: pagi, siang, sore, malam. Tidur di Kost Pelangi setelah 18.00.
- **Uang & tabungan**: menabung di kost. Tabungan dipakai sebagai target sukses.
- **Energi**: berkurang saat bekerja, pulih dengan makan dan tidur.
- **Pekerjaan**: 16 pekerjaan dari dokumen (sederhana, keterampilan, alam, profesional),
  1 pekerjaan utama + 1 sampingan, jam masuk, aturan terlambat, bonus kehadiran.
  Pekerjaan non-sederhana butuh kursus skill di perpustakaan.
- **Level**: naik dari pengalaman kerja.
- **Telepon Ibu**: sekali sehari dari kost, sampai kabar duka.
- **Kendaraan pribadi**: sepeda, skuter matic, sepeda motor, dan mobil dari Dealer Kendaraan Maju.
  Disimpan di inventori (tab KENDARAAN); tekan `V` di luar ruangan untuk memanggil atau
  menyimpannya. Bisa dikendarai sendiri ke wilayah lain dari terminal (bensin / gratis untuk sepeda).

## Struktur kode

```
Assets/PixelAdventure/LifeSim/
  PulangGame.cs          Pengendali utama: judul, input nama, dialog, menu, waktu, HUD
  PulangGameWorld.cs     NPC, interaksi, lokasi, toko, pekerjaan, perjalanan, PDKT, minimap
  LifeWorld.cs           Membangun peta 5 wilayah dan titik lokasi
  Pulang/
    PulangProfile.cs     Nama pemain ({nama}) dan layar input nama
    PulangWorld.cs       Database wilayah, lokasi, tarif bus, tokoh, 5 karakter
    PulangStory.cs       State machine cerita, data simpanan, angka penyeimbang
    PulangScript*.cs     Naskah adegan dari dokumen
    PulangJobs.cs        Pekerjaan, barang, sapaan NPC
  LifeHud*.cs, LifePlayer.cs, LifeNpc.cs, LifeCamera.cs, LifeInput.cs, LifeArt*.cs
                         Tampilan, pemain, NPC, kamera, input, dan art prosedural
```

Simpanan otomatis di `PlayerPrefs` dengan kunci `Pulang.Save.v1`.
