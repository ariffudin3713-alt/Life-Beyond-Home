using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu baris di profil. Header = judul bagian. Bar &lt; 0 = tanpa bar.</summary>
    public struct ProfileRow
    {
        public string Label, Value;
        public float Bar;
        public Color BarColor;
        public bool Header;
    }

    /// <summary>Satu teman di bagian bawah profil.</summary>
    public struct ProfileFriend
    {
        public string Sprite, Name, Line1, Line2;
        public float Bar;      // kedekatan 0..1 (hanya bila Known)
        public bool Known;
    }

    /// <summary>
    /// Layar PROFIL (aplikasi tablet), tersusun rapi dalam tiga kartu:
    /// - kiri  : bingkai foto karakter, nama, lencana gelar, level, bar EXP + persen;
    /// - kanan : statistik per bagian (KONDISI, KARIER, SOSIAL, LAINNYA) dengan kolom
    ///           label | bar | nilai yang sejajar dan garis pemisah antar baris;
    /// - bawah : lima kartu teman seragam (ikon, nama, bar kedekatan, episode).
    /// </summary>
    public partial class LifeHud
    {
        const int ProfileRowsMax = 16;
        const int ProfileFriendsMax = 5;
        const float CharW = PixelFont.CharW;
        const float ProfPad = 0.45f;

        static readonly Color ProfLine = new Color(1f, 1f, 1f, 0.08f);
        static readonly Color ProfBarBg = new Color(0.08f, 0.06f, 0.11f, 1f);
        static readonly Color ProfFrame = new Color(0.10f, 0.08f, 0.14f, 1f);

        Transform _profGroup;
        SpriteRenderer _profPanel, _profTitle, _profHeader, _profHint;
        SpriteRenderer _profCardL, _profCardR, _profCardF;
        SpriteRenderer _profAvatarFrame, _profAvatarBg, _profAvatar, _profName, _profRankBadge, _profRank;
        SpriteRenderer _profLevel, _profExpBg, _profExpFill, _profExpText, _profFriendTitle;
        readonly SpriteRenderer[] _profLabels = new SpriteRenderer[ProfileRowsMax];
        readonly SpriteRenderer[] _profValues = new SpriteRenderer[ProfileRowsMax];
        readonly SpriteRenderer[] _profBarBg = new SpriteRenderer[ProfileRowsMax];
        readonly SpriteRenderer[] _profBarFill = new SpriteRenderer[ProfileRowsMax];
        readonly SpriteRenderer[] _profLines = new SpriteRenderer[ProfileRowsMax];
        readonly SpriteRenderer[] _profFriendCard = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendIcon = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendName = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendBarBg = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendBar = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendL1 = new SpriteRenderer[ProfileFriendsMax];
        readonly SpriteRenderer[] _profFriendL2 = new SpriteRenderer[ProfileFriendsMax];
        readonly List<ProfileRow> _profRows = new List<ProfileRow>();
        readonly List<ProfileFriend> _profFriends = new List<ProfileFriend>();
        float _profExpRatio;
        float _profBuiltValueW = -1f;

        void BuildProfile()
        {
            _profGroup = Group("Profile");
            _profPanel = Panel(_profGroup, "Panel", "lc_panel_gold", 4010);
            _profTitle = Text(_profGroup, "Title", "PROFIL", 2, Gold, new Vector2(0f, 0.5f), 4014);
            _profHeader = Text(_profGroup, "Header", " ", 1, Ink, new Vector2(1f, 0.5f), 4014);
            _profHint = Text(_profGroup, "Hint", "ESC / E - KEMBALI", 1, Gold, new Vector2(0.5f, 0.5f), 4014);

            _profCardL = Panel(_profGroup, "CardLeft", "lc_panel_soft", 4011);
            _profCardR = Panel(_profGroup, "CardRight", "lc_panel_soft", 4011);
            _profCardF = Panel(_profGroup, "CardFriends", "lc_panel_soft", 4011);

            // Kartu karakter.
            _profAvatarFrame = Solid(_profGroup, "AvatarFrame", Gold, 4012);
            _profAvatarBg = Solid(_profGroup, "AvatarBg", new Color(0.24f, 0.36f, 0.52f, 1f), 4013);
            _profAvatar = Make(_profGroup, PixelArt.Get("lc_player_down_0"), "Avatar", 4014);
            _profName = Text(_profGroup, "Name", " ", 2, Ink, new Vector2(0.5f, 0.5f), 4014);
            _profRankBadge = Solid(_profGroup, "RankBadge", new Color(0.55f, 0.25f, 0.40f, 1f), 4013);
            _profRank = Text(_profGroup, "Rank", " ", 1, Ink, new Vector2(0.5f, 0.5f), 4014);
            _profLevel = Text(_profGroup, "Level", " ", 2, Gold, new Vector2(0.5f, 0.5f), 4014);
            _profExpBg = Solid(_profGroup, "ExpBg", ProfBarBg, 4013);
            _profExpFill = Solid(_profGroup, "ExpFill", new Color(1f, 0.40f, 0.58f, 1f), 4014);
            _profExpText = Text(_profGroup, "ExpText", " ", 1, Soft, new Vector2(0.5f, 0.5f), 4014);

            // Statistik.
            for (int i = 0; i < ProfileRowsMax; i++)
            {
                _profLines[i] = Solid(_profGroup, "Line" + i, ProfLine, 4012);
                _profLabels[i] = Text(_profGroup, "Label" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4014);
                _profValues[i] = Text(_profGroup, "Value" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4014);
                _profBarBg[i] = Solid(_profGroup, "BarBg" + i, ProfBarBg, 4013);
                _profBarFill[i] = Solid(_profGroup, "BarFill" + i, Color.white, 4014);
            }

            // Teman.
            _profFriendTitle = Text(_profGroup, "FriendTitle", "TEMAN", 1, Gold, new Vector2(0f, 0.5f), 4014);
            for (int i = 0; i < ProfileFriendsMax; i++)
            {
                _profFriendCard[i] = Solid(_profGroup, "FriendCard" + i, ProfFrame, 4012);
                _profFriendIcon[i] = Make(_profGroup, PixelArt.Get("lc_player_down_0"), "FriendIcon" + i, 4014);
                _profFriendName[i] = Text(_profGroup, "FriendName" + i, " ", 1, Ink, new Vector2(0f, 0.5f), 4014);
                _profFriendBarBg[i] = Solid(_profGroup, "FriendBarBg" + i, new Color(0.18f, 0.15f, 0.24f, 1f), 4013);
                _profFriendBar[i] = Solid(_profGroup, "FriendBar" + i, new Color(1f, 0.45f, 0.65f, 1f), 4014);
                _profFriendL1[i] = Text(_profGroup, "FriendL1_" + i, " ", 1, Pink, new Vector2(0f, 0.5f), 4014);
                _profFriendL2[i] = Text(_profGroup, "FriendL2_" + i, " ", 1, Soft, new Vector2(0f, 0.5f), 4014);
            }
            _profGroup.gameObject.SetActive(false);
        }

        public bool ProfileVisible => _profGroup != null && _profGroup.gameObject.activeSelf;

        public void ShowProfile(string name, string rank, int level, int exp, int expNeed, string header,
            List<ProfileRow> rows, List<ProfileFriend> friends)
        {
            SetText(_profName, Safe(name), Ink, 2, new Vector2(0.5f, 0.5f));
            SetText(_profRank, Safe(rank), Ink, 1, new Vector2(0.5f, 0.5f));
            SetText(_profLevel, "LEVEL " + level, Gold, 2, new Vector2(0.5f, 0.5f));
            _profExpRatio = expNeed > 0 ? Mathf.Clamp01(exp / (float)expNeed) : 0f;
            SetText(_profExpText, "EXP " + exp + " / " + expNeed + "  (" + Mathf.RoundToInt(_profExpRatio * 100f) + "%)",
                Soft, 1, new Vector2(0.5f, 0.5f));
            SetText(_profHeader, Safe(header), Ink, 1, new Vector2(1f, 0.5f));

            _profRows.Clear();
            for (int i = 0; i < rows.Count && i < ProfileRowsMax; i++) _profRows.Add(rows[i]);
            for (int i = 0; i < ProfileRowsMax; i++)
            {
                bool show = i < _profRows.Count;
                var r = show ? _profRows[i] : default;
                bool bar = show && !r.Header && r.Bar >= 0f;
                _profLabels[i].gameObject.SetActive(show);
                _profValues[i].gameObject.SetActive(show && !r.Header);
                _profLines[i].gameObject.SetActive(show);
                _profBarBg[i].gameObject.SetActive(bar);
                _profBarFill[i].gameObject.SetActive(bar);
                if (!show) continue;
                SetText(_profLabels[i], Safe(r.Label), r.Header ? Gold : Soft, 1, new Vector2(0f, 0.5f));
                _profLines[i].color = r.Header ? new Color(1f, 0.84f, 0.40f, 0.35f) : ProfLine;
                if (bar) _profBarFill[i].color = r.BarColor;
            }
            _profBuiltValueW = -1f;   // nilai dipotong sesuai lebar kolom saat layout

            _profFriends.Clear();
            for (int i = 0; i < friends.Count && i < ProfileFriendsMax; i++) _profFriends.Add(friends[i]);
            for (int i = 0; i < ProfileFriendsMax; i++)
            {
                bool show = i < _profFriends.Count;
                var f = show ? _profFriends[i] : default;
                _profFriendCard[i].gameObject.SetActive(show);
                _profFriendIcon[i].gameObject.SetActive(show);
                _profFriendName[i].gameObject.SetActive(show);
                _profFriendL1[i].gameObject.SetActive(show);
                _profFriendL2[i].gameObject.SetActive(show);
                _profFriendBarBg[i].gameObject.SetActive(show && f.Known);
                _profFriendBar[i].gameObject.SetActive(show && f.Known);
                if (!show) continue;
                _profFriendIcon[i].sprite = PixelArt.Get(f.Sprite);
                _profFriendIcon[i].color = f.Known ? Color.white : new Color(0.20f, 0.18f, 0.26f, 1f);  // siluet
                SetHint(_profFriendName[i], f.Known ? Safe(f.Name) : "???", f.Known ? Ink : Soft, 0f);
                SetHint(_profFriendL1[i], Safe(f.Line1), f.Known ? Pink : Soft, 0f);
                SetHint(_profFriendL2[i], Safe(f.Line2), Soft, 0f);
            }

            SetDim(0.78f);
            _profGroup.gameObject.SetActive(true);
            HidePrompt();
        }

        public void HideProfile()
        {
            if (_profGroup != null) _profGroup.gameObject.SetActive(false);
            SetDim(0f);
        }

        static void SetBar(SpriteRenderer bg, SpriteRenderer fill, float left, float y, float w, float h, float ratio)
        {
            SetSolid(bg, left + w * 0.5f, y, w, h);
            float fw = (w - 0.08f) * Mathf.Clamp01(ratio);
            SetSolid(fill, left + 0.04f + fw * 0.5f, y, fw, h - 0.08f);
        }

        void LayoutProfile(float halfW, float halfH)
        {
            if (!ProfileVisible) return;
            float w = Mathf.Min(halfW * 2f - 0.8f, 28f);
            float h = halfH * 2f - 0.8f;
            float top = h * 0.5f, left = -w * 0.5f;
            float innerL = left + 0.7f, innerW = w - 1.4f;
            SetRect(_profPanel, 0f, 0f, w, h);
            CloseAt(w * 0.5f, h * 0.5f);
            _profTitle.transform.localPosition = new Vector3(innerL, top - 0.75f, 0f);
            _profHeader.transform.localPosition = new Vector3(innerL + innerW, top - 0.75f, 0f);
            _profHint.transform.localPosition = new Vector3(0f, -top + 0.45f, 0f);

            // ---------------- kartu teman (bawah)
            float fH = 2.7f;
            float fBottom = -top + 0.95f;
            SetRect(_profCardF, 0f, fBottom + fH * 0.5f, innerW, fH);
            _profFriendTitle.transform.localPosition = new Vector3(innerL + ProfPad, fBottom + fH - 0.42f, 0f);
            float gap = 0.25f;
            float slotW = (innerW - ProfPad * 2f - gap * (ProfileFriendsMax - 1)) / ProfileFriendsMax;
            float slotH = fH - 0.95f;
            float slotY = fBottom + ProfPad + slotH * 0.5f - 0.1f;
            for (int i = 0; i < _profFriends.Count; i++)
            {
                float sl = innerL + ProfPad + i * (slotW + gap);
                SetSolid(_profFriendCard[i], sl + slotW * 0.5f, slotY, slotW, slotH);
                // Kartu sempit (layar 4:3): tanpa ikon agar teks tetap muat.
                bool icon = slotW >= 4.2f;
                _profFriendIcon[i].gameObject.SetActive(icon);
                _profFriendIcon[i].transform.localScale = Vector3.one * 0.9f;
                _profFriendIcon[i].transform.localPosition = new Vector3(sl + 0.75f, slotY - slotH * 0.5f + 0.2f, 0f);
                float tx = sl + (icon ? 1.45f : 0.3f);
                float tw = slotW - (tx - sl) - 0.25f;
                FitHint(_profFriendName[i], tw);   // teks kartu teman tidak meluber ke kartu sebelah
                FitHint(_profFriendL1[i], tw);
                FitHint(_profFriendL2[i], tw);
                _profFriendName[i].transform.localPosition = new Vector3(tx, slotY + 0.56f, 0f);
                if (_profFriends[i].Known)
                {
                    SetBar(_profFriendBarBg[i], _profFriendBar[i], tx, slotY + 0.02f, Mathf.Max(0.6f, tw), 0.22f, _profFriends[i].Bar);
                    _profFriendL1[i].transform.localPosition = new Vector3(tx, slotY - 0.56f, 0f);
                    _profFriendL2[i].transform.localPosition = new Vector3(tx, slotY - 1.1f, 0f);
                }
                else
                {
                    _profFriendL1[i].transform.localPosition = new Vector3(tx, slotY + 0.0f, 0f);
                    _profFriendL2[i].transform.localPosition = new Vector3(tx, slotY - 0.56f, 0f);
                }
            }

            // ---------------- kartu kiri (karakter) & kanan (statistik)
            float bodyTop = top - 1.25f;
            float bodyBottom = fBottom + fH + 0.25f;
            float bodyH = bodyTop - bodyBottom;
            float lw = Mathf.Clamp(innerW * 0.3f, 6.2f, 8f);
            float rw = innerW - lw - 0.3f;
            float lcx = innerL + lw * 0.5f;
            float rl = innerL + lw + 0.3f;
            SetRect(_profCardL, lcx, bodyBottom + bodyH * 0.5f, lw, bodyH);
            SetRect(_profCardR, rl + rw * 0.5f, bodyBottom + bodyH * 0.5f, rw, bodyH);

            // Foto berbingkai di atas, lalu nama, lencana gelar, level, EXP — rata tengah, berjarak lega.
            const float gapName = 0.8f, gapRank = 0.95f, gapLevel = 1.05f, gapExp = 0.85f, gapExpText = 0.6f;
            float textBlock = gapName + gapRank + gapLevel + gapExp + gapExpText + 0.4f;
            float photo = Mathf.Clamp(bodyH - ProfPad * 2f - textBlock - 0.3f, 2.2f, 3.6f);
            float photoCy = bodyTop - ProfPad - 0.1f - photo * 0.5f;
            SetSolid(_profAvatarFrame, lcx, photoCy, photo + 0.16f, photo + 0.16f);
            SetSolid(_profAvatarBg, lcx, photoCy, photo, photo);
            float avatarScale = photo / 2.0f;
            _profAvatar.sprite = PixelArt.Get("lc_player_down_0");   // ikut pakaian yang sedang dipakai
            _profAvatar.transform.localScale = Vector3.one * avatarScale;
            _profAvatar.transform.localPosition = new Vector3(lcx, photoCy - photo * 0.5f + 0.12f, 0f);

            float y = photoCy - photo * 0.5f - gapName;
            _profName.transform.localPosition = new Vector3(lcx, y, 0f);
            y -= gapRank;
            float rankW = _profRank.sprite != null ? _profRank.sprite.bounds.size.x + 0.7f : 3f;
            SetSolid(_profRankBadge, lcx, y, Mathf.Min(rankW, lw - 0.6f), 0.55f);
            _profRank.transform.localPosition = new Vector3(lcx, y, 0f);
            y -= gapLevel;
            _profLevel.transform.localPosition = new Vector3(lcx, y, 0f);
            y -= gapExp;
            float expW = lw - ProfPad * 2f;
            SetBar(_profExpBg, _profExpFill, lcx - expW * 0.5f, y, expW, 0.3f, _profExpRatio);
            y -= gapExpText;
            _profExpText.transform.localPosition = new Vector3(lcx, y, 0f);

            // ---------------- statistik: 2 kolom kartu, tiap item bertingkat
            //   LABEL KECIL
            //   NILAI
            //   [bar]          (bila ada)
            // Bagian ke-1 & ke-3 di kolom kiri, ke-2 & ke-4 di kolom kanan.
            const float colGap = 0.6f;
            float colW = (rw - ProfPad * 2f - colGap) * 0.5f;
            float[] colX = { rl + ProfPad, rl + ProfPad + colW + colGap };
            int maxChars = Mathf.Max(6, Mathf.FloorToInt(colW / CharW));

            if (!Mathf.Approximately(rw, _profBuiltValueW))
            {
                _profBuiltValueW = rw;
                for (int i = 0; i < _profRows.Count; i++)
                {
                    var r = _profRows[i];
                    if (r.Header) continue;
                    string v = Safe(r.Value);
                    if (v.Length > maxChars) v = v.Substring(0, maxChars - 2) + "..";
                    SetText(_profValues[i], v, Ink, 1, new Vector2(0f, 0.5f));
                }
            }

            // Jarak antar TEKS selalu tetap (tidak pernah dirapatkan); hanya jarak antar item /
            // bagian yang menyesuaikan bila ruang kurang.
            const float textH = 0.56f;            // tinggi satu baris PixelFont skala 1 (9 px)
            const float labelToValue = 0.6f;      // label -> nilai
            const float valueToBar = 0.44f;       // nilai -> bar
            const float headerH = 0.74f;          // judul bagian + garis + ruang di bawahnya
            float itemGap = 0.4f, sectionGap = 0.55f;

            float[] fixedH = new float[2];
            int[] gaps = new int[2], secGaps = new int[2];
            int section = -1;
            for (int i = 0; i < _profRows.Count; i++)
            {
                var r = _profRows[i];
                if (r.Header)
                {
                    section++;
                    int c = section % 2;
                    if (section >= 2) secGaps[c]++;
                    fixedH[c] += headerH;
                    continue;
                }
                int col = Mathf.Max(0, section) % 2;
                fixedH[col] += textH + labelToValue + (r.Bar >= 0f ? valueToBar : 0f);
                gaps[col]++;
            }
            float availH = bodyH - ProfPad * 2f;
            for (int c = 0; c < 2; c++)
            {
                float need = fixedH[c] + gaps[c] * itemGap + secGaps[c] * sectionGap;
                if (need > availH && gaps[c] + secGaps[c] > 0)
                {
                    float spare = Mathf.Max(0f, availH - fixedH[c]);
                    float s = spare / (gaps[c] * itemGap + secGaps[c] * sectionGap);
                    itemGap = Mathf.Min(itemGap, Mathf.Max(0.12f, itemGap * s));
                    sectionGap = Mathf.Min(sectionGap, Mathf.Max(0.15f, sectionGap * s));
                }
            }

            float[] colY = { bodyTop - ProfPad, bodyTop - ProfPad };
            section = -1;
            for (int i = 0; i < _profRows.Count; i++)
            {
                var r = _profRows[i];
                if (r.Header)
                {
                    section++;
                    int c = section % 2;
                    if (section >= 2) colY[c] -= sectionGap;
                    float hy = colY[c] - textH * 0.5f;
                    _profLabels[i].transform.localPosition = new Vector3(colX[c], hy, 0f);
                    SetSolid(_profLines[i], colX[c] + colW * 0.5f, hy - 0.4f, colW, 0.06f);
                    colY[c] -= headerH;
                    continue;
                }
                int col = Mathf.Max(0, section) % 2;
                float x = colX[col];
                float ly = colY[col] - textH * 0.5f;
                _profLabels[i].transform.localPosition = new Vector3(x, ly, 0f);
                float vy = ly - labelToValue;
                _profValues[i].transform.localPosition = new Vector3(x, vy, 0f);
                float bottom = vy - textH * 0.5f;
                if (r.Bar >= 0f)
                {
                    float by = vy - valueToBar;
                    SetBar(_profBarBg[i], _profBarFill[i], x, by, colW, 0.22f, r.Bar);
                    bottom = by - 0.11f;
                }
                // Garis pemisah tipis di tengah jarak ke item berikutnya.
                SetSolid(_profLines[i], x + colW * 0.5f, bottom - itemGap * 0.5f, colW, 0.03f);
                colY[col] = bottom - itemGap;
            }
        }
    }
}
