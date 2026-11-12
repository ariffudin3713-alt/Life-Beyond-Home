using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu tujuan misi yang ditunjuk di layar.</summary>
    public struct QuestGoal
    {
        public Vector2 Position;
        public string Label;
        public bool IsJob;      // penunjuk tempat kerja (warna biru), bukan misi
        public QuestGoal(Vector2 position, string label, bool isJob = false)
        {
            Position = position; Label = label; IsJob = isJob;
        }
    }

    /// <summary>
    /// Penunjuk arah misi:
    /// - panah yang mengelilingi pemain menunjuk tujuan terdekat;
    /// - setiap tujuan di luar layar mendapat panah di tepi layar (nama + jarak);
    /// - setiap tujuan yang terlihat mendapat penanda memantul di atasnya (nama + jarak).
    /// </summary>
    public partial class LifeHud
    {
        const int QuestMax = 8;
        static readonly Color QuestLabelColor = new Color(1f, 0.84f, 0.40f);
        static readonly Color JobLabelColor = new Color(0.55f, 0.85f, 1f);
        static readonly Color JobArrowTint = new Color(0.60f, 0.90f, 1f);

        Transform _questGroup;
        SpriteRenderer _questArrow, _questDist;
        readonly SpriteRenderer[] _questMarks = new SpriteRenderer[QuestMax];
        readonly SpriteRenderer[] _questLabels = new SpriteRenderer[QuestMax];
        readonly string[] _questLabelCache = new string[QuestMax];
        readonly List<QuestGoal> _questGoals = new List<QuestGoal>();
        int _lastQuestDist = -1;

        void BuildQuestCompass()
        {
            _questGroup = Group("QuestCompass");
            _questArrow = Make(_questGroup, PixelArt.Get("lc_icon_direction"), "QuestArrow", 3985);
            _questArrow.transform.localScale = Vector3.one * 1.1f;
            _questDist = Text(_questGroup, "QuestDist", " ", 1, Gold, new Vector2(0.5f, 0.5f), 3986);

            for (int i = 0; i < QuestMax; i++)
            {
                _questMarks[i] = Make(_questGroup, PixelArt.Get("lc_icon_direction"), "QuestMark" + i, 3985);
                _questMarks[i].transform.localScale = Vector3.one * 0.9f;
                _questLabels[i] = Text(_questGroup, "QuestLabel" + i, " ", 1, QuestLabelColor,
                    new Vector2(0.5f, 0.5f), 3986);
                _questMarks[i].gameObject.SetActive(false);
                _questLabels[i].gameObject.SetActive(false);
            }
            _questGroup.gameObject.SetActive(true);
        }

        /// <summary>Semua tujuan misi aktif di wilayah ini (posisi dunia).</summary>
        public void SetQuestTargets(List<QuestGoal> goals)
        {
            _questGoals.Clear();
            if (goals == null) return;
            for (int i = 0; i < goals.Count && _questGoals.Count < QuestMax; i++)
                _questGoals.Add(goals[i]);
        }

        static void Show(SpriteRenderer sr, bool on)
        {
            if (sr.gameObject.activeSelf != on) sr.gameObject.SetActive(on);
        }

        void LayoutQuestCompass(float halfW, float halfH)
        {
            if (_questGroup == null) return;
            bool show = _questGoals.Count > 0
                        && _hudGroup.gameObject.activeSelf
                        && !_talkGroup.gameObject.activeSelf
                        && !_menuGroup.gameObject.activeSelf
                        && !_workGroup.gameObject.activeSelf
                        && !(_journalGroup != null && _journalGroup.gameObject.activeSelf)
                        && !TasksVisible && !InventoryVisible && !TabletVisible && !ProfileVisible && !GuideVisible
                        && !(_loadingGroup != null && _loadingGroup.gameObject.activeSelf);

            Vector2 cam = _cam.transform.position;
            Vector2 center = _miniPlayerPos + new Vector2(0f, 0.75f) - cam;
            float bob = Mathf.Sin(_time * 5f) * 0.12f;

            // ---------------- panah di sekitar pemain -> tujuan misi terdekat
            // (tempat kerja hanya dipakai bila tidak ada tujuan misi)
            bool anyQuest = false;
            for (int i = 0; i < _questGoals.Count; i++) anyQuest |= !_questGoals[i].IsJob;
            int nearest = -1;
            float best = float.MaxValue;
            for (int i = 0; i < _questGoals.Count; i++)
            {
                if (anyQuest && _questGoals[i].IsJob) continue;
                float d = (_questGoals[i].Position - _miniPlayerPos).sqrMagnitude;
                if (d < best) { best = d; nearest = i; }
            }
            float nearDist = nearest >= 0 ? Mathf.Sqrt(best) : 0f;
            bool orbit = show && nearest >= 0 && nearDist > 1.6f;
            Show(_questArrow, orbit);
            Show(_questDist, orbit);
            if (orbit)
            {
                Vector2 dir = (_questGoals[nearest].Position - _miniPlayerPos) / nearDist;
                Vector2 p = center + dir * (1.75f + bob);
                _questArrow.transform.localPosition = new Vector3(p.x, p.y, 0f);
                _questArrow.transform.localRotation =
                    Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                int meters = Mathf.RoundToInt(nearDist);
                if (meters != _lastQuestDist)
                {
                    _lastQuestDist = meters;
                    SetText(_questDist, meters + " M", Gold, 1, new Vector2(0.5f, 0.5f));
                }
                Vector2 q = center + dir * 2.85f;
                _questDist.transform.localPosition = new Vector3(q.x, q.y, 0f);
            }

            // ---------------- penanda untuk setiap tujuan misi
            // Area aman: di bawah panel status atas, di atas teks prompt bawah.
            float minX = -halfW + 0.9f, maxX = halfW - 0.9f;
            float minY = -halfH + 1.45f, maxY = halfH - 3.75f;
            float miniRight = _miniEdgeRight + 0.6f, miniTop = _miniEdgeTop + 0.6f; // kotak minimap kiri bawah

            for (int i = 0; i < QuestMax; i++)
            {
                bool active = show && i < _questGoals.Count;
                Vector2 delta = active ? _questGoals[i].Position - _miniPlayerPos : Vector2.zero;
                float dist = delta.magnitude;
                active &= dist > 1.2f;
                Show(_questMarks[i], active);
                Show(_questLabels[i], active);
                if (!active) continue;

                bool job = _questGoals[i].IsJob;
                string text = (job ? QuestMark + " " : "") + _questGoals[i].Label + " " +
                              Mathf.RoundToInt(dist) + "M";
                string key = (job ? "J" : "Q") + text;
                if (key != _questLabelCache[i])
                {
                    _questLabelCache[i] = key;
                    SetText(_questLabels[i], text, job ? JobLabelColor : QuestLabelColor, 1,
                        new Vector2(0.5f, 0.5f));
                }
                // Posisi penanda: memantul di atas tujuan.
                Vector2 t = _questGoals[i].Position - cam + new Vector2(0f, 2.0f + bob);
                bool inMini = t.x < miniRight && t.y < miniTop;
                bool onScreen = t.x > minX && t.x < maxX && t.y > minY && t.y < maxY && !inMini;

                // Lokasi pekerjaan yang terlihat memakai logo bintang; selain itu panah.
                bool star = job && onScreen;
                Sprite want = PixelArt.Get(star ? "lc_icon_star" : "lc_icon_direction");
                if (_questMarks[i].sprite != want) _questMarks[i].sprite = want;
                _questMarks[i].color = star ? Color.white : job ? JobArrowTint : Color.white;

                Transform mark = _questMarks[i].transform;
                mark.localScale = Vector3.one * (star ? 1.25f + 0.12f * Mathf.Sin(_time * 4f) : 0.9f);
                Vector2 labelPos;
                if (onScreen)
                {
                    mark.localPosition = new Vector3(t.x, t.y, 0f);
                    // bintang tegak; panah misi menunjuk ke bawah
                    mark.localRotation = Quaternion.Euler(0f, 0f, star ? 0f : 180f);
                    labelPos = t + new Vector2(0f, star ? 1.0f : 0.85f);
                }
                else
                {
                    // Di luar layar: jepit ke tepi area aman, menunjuk ke arah tujuan.
                    Vector2 from = new Vector2(0f, (minY + maxY) * 0.5f);
                    Vector2 dir = t - from;
                    if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
                    float sx = dir.x > 0f ? (maxX - from.x) / dir.x : dir.x < 0f ? (minX - from.x) / dir.x : float.MaxValue;
                    float sy = dir.y > 0f ? (maxY - from.y) / dir.y : dir.y < 0f ? (minY - from.y) / dir.y : float.MaxValue;
                    Vector2 e = from + dir * Mathf.Min(1f, Mathf.Min(sx, sy));
                    if (e.x < miniRight && e.y < miniTop) e.y = miniTop;
                    Vector2 nd = dir.normalized;
                    e -= nd * bob; // denyut kecil ke arah tujuan
                    mark.localPosition = new Vector3(e.x, e.y, 0f);
                    mark.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(nd.y, nd.x) * Mathf.Rad2Deg - 90f);

                    // Label di sisi dalam panah, dijaga tetap di dalam layar.
                    float halfLabel = _questLabels[i].sprite != null ? _questLabels[i].sprite.bounds.size.x * 0.5f : 1f;
                    labelPos = e - nd * 1.0f;
                    labelPos.x = Mathf.Clamp(labelPos.x, -halfW + 0.3f + halfLabel, halfW - 0.3f - halfLabel);
                }
                _questLabels[i].transform.localPosition = new Vector3(labelPos.x, labelPos.y, 0f);
            }
        }
    }
}
