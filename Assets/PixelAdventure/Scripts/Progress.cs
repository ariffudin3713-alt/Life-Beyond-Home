using UnityEngine;

namespace PixelAdventure
{
    /// <summary>
    /// Tiny save layer: remembers the best gem count per level and the best full run.
    /// Uses PlayerPrefs so it works in the editor and in standalone builds.
    /// </summary>
    public static class Progress
    {
        const string LevelKey = "PixelQuest.BestGems.";
        const string RunKey = "PixelQuest.BestRun";

        public static int BestGems(int levelIndex)
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(LevelKey + levelIndex, 0));
        }

        /// <summary>Stores a level record and returns true when it is a new personal best.</summary>
        public static bool ReportLevel(int levelIndex, int gems)
        {
            if (gems <= BestGems(levelIndex)) return false;
            PlayerPrefs.SetInt(LevelKey + levelIndex, gems);
            PlayerPrefs.Save();
            return true;
        }

        public static int BestRun => Mathf.Max(0, PlayerPrefs.GetInt(RunKey, 0));

        /// <summary>Stores a full-run record and returns true when it is a new personal best.</summary>
        public static bool ReportRun(int gems)
        {
            if (gems <= BestRun) return false;
            PlayerPrefs.SetInt(RunKey, gems);
            PlayerPrefs.Save();
            return true;
        }
    }
}
