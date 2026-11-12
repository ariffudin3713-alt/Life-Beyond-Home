using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Entry point for the life-sim. Generates all art, sets up the camera and
    /// hands control to LifeGame, so pressing Play in any scene just works.
    /// </summary>
    public static class LifeBoot
    {
        public static void Launch()
        {
            if (Object.FindAnyObjectByType<PulangGame>() != null) return;

            PixelArt.BuildAll();
            LifeArt.BuildAll();

            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.transform.position = new Vector3(0f, 0f, -10f);
                cam = camGo.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.12f, 0.20f);

            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();

            var host = new GameObject("PULANG");
            var game = host.AddComponent<PulangGame>();
            game.Boot(cam);
        }
    }
}
