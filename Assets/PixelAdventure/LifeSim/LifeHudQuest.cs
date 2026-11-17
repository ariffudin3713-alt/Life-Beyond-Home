using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Satu tujuan misi yang ditunjuk di minimap.</summary>
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
    /// Tujuan misi aktif. Arah ke tujuan terdekat hanya ditampilkan di minimap
    /// (LifeHudMinimap.cs: garis putus-putus, panah kecil, dan baris "tujuan + jarak").
    /// Penanda di area permainan (panah memantul di atas tujuan dan panah di tepi layar)
    /// sudah dihapus agar tengah layar bersih.
    /// </summary>
    public partial class LifeHud
    {
        const int QuestMax = 8;
        static readonly Color JobArrowTint = new Color(0.60f, 0.90f, 1f);   // warna tujuan kerja di minimap

        readonly List<QuestGoal> _questGoals = new List<QuestGoal>();

        /// <summary>Semua tujuan misi aktif di wilayah ini (posisi dunia).</summary>
        public void SetQuestTargets(List<QuestGoal> goals)
        {
            _questGoals.Clear();
            if (goals == null) return;
            for (int i = 0; i < goals.Count && _questGoals.Count < QuestMax; i++)
                _questGoals.Add(goals[i]);
        }

        /// <summary>
        /// Tujuan terdekat dari pemain (tempat kerja hanya dipakai bila tidak ada tujuan misi).
        /// Mengembalikan -1 bila tidak ada tujuan.
        /// </summary>
        int NearestQuestGoal(out float distance)
        {
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
            distance = nearest >= 0 ? Mathf.Sqrt(best) : 0f;
            return nearest;
        }

        static void Show(SpriteRenderer sr, bool on)
        {
            if (sr.gameObject.activeSelf != on) sr.gameObject.SetActive(on);
        }
    }
}
