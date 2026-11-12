using UnityEngine;

namespace PixelAdventure
{
    /// <summary>
    /// Starts the whole game when you press Play in any scene. No scene setup is required:
    /// all art, audio, UI and levels are generated at runtime.
    ///
    /// Launches the life-sim "PULANG - Life Beyond Home" (LifeSim.PulangGame).
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            if (Object.FindAnyObjectByType<LifeSim.PulangGame>() != null) return;
            LifeSim.LifeBoot.Launch();
        }
    }
}
