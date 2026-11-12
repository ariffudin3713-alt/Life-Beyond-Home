using System.Collections;
using UnityEngine;

namespace PixelAdventure
{
    public enum GameState
    {
        Title,
        Playing,
        Paused,
        Dying,
        LevelClear,
        GameOver,
        Victory
    }

    /// <summary>Owns the run: builds levels, drives the state machine, HUD and audio.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        /// <summary>Drop the stale static reference between Play sessions when Domain Reload is off.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticState() => I = null;

        public GameState State { get; private set; } = GameState.Title;
        public CameraRig CameraRig { get; private set; }
        public Player PlayerInstance => _player;
        public LevelDef CurrentLevel => _level;
        public int LevelIndex => _levelIndex;
        public int GemsCollected => _gems;
        public Theme CurrentTheme => _level != null ? _level.Theme : Levels.Grove;

        Camera _camera;
        Hud _hud;
        LevelDef _level;
        LevelDef[] _levels;
        int _levelIndex;
        Player _player;
        Transform _world;

        int _health;
        int _gems;
        int _bankedGems;
        int _totalGems;

        public void Boot(Camera cam)
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            _camera = cam;
            PixelArt.BuildAll();

            Physics2D.IgnoreLayerCollision(GameLayers.Player, GameLayers.EnemyBody, true);
            Physics2D.IgnoreLayerCollision(GameLayers.EnemyBody, GameLayers.EnemyBody, true);

            FxManager.Ensure(gameObject);
            GameAudio.Init(gameObject);

            CameraRig = _camera.GetComponent<CameraRig>();
            if (CameraRig == null) CameraRig = _camera.gameObject.AddComponent<CameraRig>();

            _hud = gameObject.AddComponent<Hud>();
            _hud.Init(_camera);

            _levels = Levels.All;
            _totalGems = 0;
            for (int i = 0; i < _levels.Length; i++) _totalGems += _levels[i].GemCount;

            LoadLevel(0, false);
        }

        public void LoadLevel(int index, bool play)
        {
            StopAllCoroutines();
            Time.timeScale = 1f;

            if (_world != null)
            {
                _world.gameObject.SetActive(false);
                Destroy(_world.gameObject);
            }
            FxManager.ClearAll();

            _levelIndex = Mathf.Clamp(index, 0, _levels.Length - 1);
            _level = _levels[_levelIndex];
            _gems = 0;
            _health = Player.MaxHealth;

            BuildWorld();

            _hud.SetTheme(_level.Theme);
            _hud.SetLevel(_levelIndex, _level);
            _hud.SetHealth(_health);
            _hud.SetGems(_gems, _level.GemCount);
            _hud.SetMuted(GameAudio.Muted);
            GameAudio.PlayMusic(_level.Theme.MusicIndex);

            if (play)
            {
                State = GameState.Playing;
                _player.SetFrozen(false);
                _hud.HideOverlays();
            }
            else
            {
                State = GameState.Title;
                _player.SetFrozen(true);
                _hud.ShowTitle(Progress.BestRun, _totalGems);
            }
        }

        void Update()
        {
            if (GameInput.MutePressed)
            {
                GameAudio.ToggleMute();
                _hud.SetMuted(GameAudio.Muted);
                _hud.ShowToast(GameAudio.Muted ? "SOUND OFF" : "SOUND ON");
            }

            switch (State)
            {
                case GameState.Title:
                    if (GameInput.ConfirmPressed) StartRun();
                    break;

                case GameState.Playing:
                    if (GameInput.RestartPressed) { LoadLevel(_levelIndex, true); break; }
                    if (GameInput.PausePressed) { Pause(); break; }
                    if (_player != null && _player.IsAlive && _player.transform.position.y < -1.6f)
                        _player.Fell();
                    break;

                case GameState.Paused:
                    if (GameInput.PausePressed || GameInput.ConfirmPressed) Resume();
                    else if (GameInput.RestartPressed) LoadLevel(_levelIndex, true);
                    break;

                case GameState.GameOver:
                    if (GameInput.ConfirmPressed || GameInput.RestartPressed)
                        LoadLevel(_levelIndex, true);
                    break;

                case GameState.LevelClear:
                    if (GameInput.ConfirmPressed) LoadLevel(_levelIndex + 1, true);
                    break;

                case GameState.Victory:
                    if (GameInput.ConfirmPressed || GameInput.RestartPressed) StartNewRun();
                    break;
            }
        }

        void StartRun()
        {
            if (State != GameState.Title) return;
            State = GameState.Playing;
            _player.SetFrozen(false);
            _hud.HideOverlays();
            GameAudio.Play("click", 0.7f);
        }

        void StartNewRun()
        {
            _bankedGems = 0;
            LoadLevel(0, true);
        }

        void Pause()
        {
            State = GameState.Paused;
            Time.timeScale = 0f;
            if (_player != null) _player.SetFrozen(true);
            _hud.ShowPause();
            GameAudio.Play("click", 0.6f);
        }

        void Resume()
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
            if (_player != null) _player.SetFrozen(false);
            _hud.HideOverlays();
            GameAudio.Play("click", 0.6f);
        }

        // ------------------------------------------------------------------ callbacks

        public void OnPlayerHealthChanged(int health)
        {
            _health = health;
            if (_hud != null) _hud.SetHealth(health);
        }

        public void OnPlayerDied()
        {
            if (State != GameState.Playing) return;
            State = GameState.Dying;
            GameAudio.Play("gameover", 0.9f);
            StartCoroutine(DeathSequence());
        }

        IEnumerator DeathSequence()
        {
            yield return new WaitForSecondsRealtime(1.15f);
            State = GameState.GameOver;
            if (_hud != null) _hud.ShowGameOver();
        }

        public void CollectGem(Vector3 position)
        {
            if (State != GameState.Playing) return;

            _gems++;
            Color accent = CurrentTheme.Accent;
            GameAudio.Play("gem", 0.85f);
            FxManager.Burst(position, accent, 12, 1.2f, 4f, 0.45f, 0.1f, 2f);

            if (_hud != null) _hud.SetGems(_gems, _level.GemCount);
            if (_gems >= _level.GemCount) _hud.ShowToast("ALL GEMS!");
        }

        public void SetCheckpoint(Vector3 position)
        {
            if (_player != null)
            {
                _player.RespawnPoint = position;
                if (_player.Health < Player.MaxHealth) _player.Heal(1);
            }
            if (_hud != null) _hud.ShowToast("CHECKPOINT");
        }

        public void ReachGoal()
        {
            if (State != GameState.Playing) return;

            State = GameState.LevelClear;
            _player.SetFrozen(true);

            Color accent = CurrentTheme.Accent;
            GameAudio.Play("win", 0.9f);
            FxManager.Burst(_player.transform.position + new Vector3(0f, 0.8f, 0f), accent,
                30, 2f, 7f, 0.9f, 0.14f, 6f);
            CameraRig?.Shake(0.18f, 0.4f);

            _bankedGems += _gems;
            int lastGems = _gems;
            int lastTotal = _level.GemCount;
            bool newLevelBest = Progress.ReportLevel(_levelIndex, lastGems);

            if (_levelIndex >= _levels.Length - 1)
            {
                State = GameState.Victory;
                bool newRunBest = Progress.ReportRun(_bankedGems);
                _hud.ShowVictory(_bankedGems, _totalGems, Progress.BestRun, newRunBest);
            }
            else
            {
                _hud.ShowLevelClear(lastGems, lastTotal, Progress.BestGems(_levelIndex), newLevelBest);
            }
        }

        // ------------------------------------------------------------------ world building

        void BuildWorld()
        {
            var root = new GameObject("World - " + _level.Theme.Title);
            _world = root.transform;

            BuildTiles();
            CreatePlayer();
            CameraRig.Bind(_camera, _player, _player.transform, _level.Width, _level.Height);
            BuildBackground();
            BuildEntities();

            var motes = root.AddComponent<AmbientMotes>();
            motes.ColorA = _level.Theme.Ambient;
            motes.ColorB = Color.Lerp(_level.Theme.Ambient, Color.white, 0.45f);
            motes.Rate = _level.Theme.HasStars ? 5f : 2f;
            motes.Rise = 0.25f;
            motes.Drift = 0.4f;
            motes.SizeMin = 0.05f;
            motes.SizeMax = 0.13f;
        }

        void CreatePlayer()
        {
            var go = new GameObject("Player");
            go.transform.SetParent(_world, false);
            Vector3 position = new Vector3(_level.SpawnX + 0.5f,
                _level.Height - 1 - _level.SpawnRow, 0f);
            go.transform.position = position;

            _player = go.AddComponent<Player>();
            _player.RespawnPoint = position;
        }

        void BuildTiles()
        {
            var tiles = new GameObject("Tiles").transform;
            tiles.SetParent(_world, false);
            var colliders = new GameObject("Colliders").transform;
            colliders.SetParent(_world, false);
            colliders.gameObject.layer = GameLayers.Ground;

            string prefix = "tile_" + _level.Theme.SpritePrefix;

            for (int x = 0; x < _level.Width; x++)
            {
                for (int row = 0; row < _level.Height; row++)
                {
                    char c = _level.Tiles[x, row];
                    if (c != '#' && c != '=') continue;

                    string key;
                    if (c == '=') key = prefix + "_platform";
                    else key = _level.IsSolid(x, row - 1) ? prefix + "_fill" : prefix + "_top";

                    var sr = MakeWorldSprite("Tile", PixelArt.Get(key), c == '=' ? 1 : 0, tiles);
                    sr.transform.position = new Vector3(x + 0.5f,
                        _level.Height - 1 - row + 0.5f, 0f);
                }
            }

            for (int row = 0; row < _level.Height; row++)
            {
                int x = 0;
                while (x < _level.Width)
                {
                    if (!_level.IsSolid(x, row)) { x++; continue; }
                    int start = x;
                    while (x < _level.Width && _level.IsSolid(x, row)) x++;
                    int length = x - start;

                    var go = new GameObject("Solid");
                    go.transform.SetParent(colliders, false);
                    go.layer = GameLayers.Ground;
                    go.transform.position = new Vector3(start + length * 0.5f,
                        _level.Height - 1 - row + 0.5f, 0f);
                    var box = go.AddComponent<BoxCollider2D>();
                    box.size = new Vector2(length, 1f);
                }
            }
        }

        void BuildBackground()
        {
            Theme theme = _level.Theme;
            float width = _level.Width;
            float height = _level.Height;

            var sky = MakeWorldSprite("Sky", PixelArt.Get(theme.Sky), -100);
            sky.transform.position = new Vector3(width * 0.5f, height * 0.5f, 0f);
            Vector2 skySize = sky.sprite.bounds.size;
            sky.transform.localScale = new Vector3((width + 44f) / skySize.x,
                26f / skySize.y, 1f);

            bool ruins = theme.Id == "ruins";
            var sun = MakeWorldSprite("Sun", PixelArt.Get(theme.Sun), -95);
            sun.transform.position = new Vector3(ruins ? width * 0.72f : width * 0.18f,
                ruins ? 10.6f : 13.2f, 0f);
            sun.transform.localScale = Vector3.one * (ruins ? 2.2f : 1.7f);

            if (theme.HasClouds)
            {
                int count = 5;
                for (int i = 0; i < count; i++)
                {
                    var cloud = MakeWorldSprite("Cloud", PixelArt.Get(theme.Cloud), -90);
                    cloud.transform.position = new Vector3(
                        (i + 0.5f) * width / count + Random.Range(-3f, 3f),
                        12.4f + Random.Range(-0.7f, 1.4f), 0f);
                    cloud.transform.localScale = Vector3.one * Random.Range(1.4f, 2.4f);
                    cloud.color = new Color(1f, 1f, 1f, Random.Range(0.65f, 0.9f));

                    var drift = cloud.gameObject.AddComponent<CloudDrift>();
                    drift.Speed = Random.Range(0.10f, 0.26f);
                    drift.MinX = -14f;
                    drift.MaxX = width + 14f;
                }
            }

            if (theme.HasStars)
            {
                for (int i = 0; i < 36; i++)
                {
                    string key = Random.value < 0.6f ? "star_a" : "star_b";
                    var star = MakeWorldSprite("Star", PixelArt.Get(key), -92);
                    star.transform.position = new Vector3(Random.Range(0f, width),
                        Random.Range(8.5f, 17f), 0f);
                    star.transform.localScale = Vector3.one * Random.Range(0.7f, 1.6f);
                    star.color = new Color(1f, 1f, 1f, Random.Range(0.45f, 0.95f));

                    var twinkle = star.gameObject.AddComponent<Twinkle>();
                    twinkle.Renderer = star;
                    twinkle.Speed = Random.Range(0.8f, 2.6f);
                    twinkle.Phase = Random.Range(0f, 6.28f);
                    twinkle.MinAlpha = 0.18f;
                    twinkle.MaxAlpha = 1f;
                }
            }

            MakeParallax(theme.HillsFar, 0.2f, -80, 0.26f);
            MakeParallax(theme.HillsNear, -0.15f, -70, 0.46f);
        }

        void MakeParallax(string key, float y, int order, float factor)
        {
            Sprite sprite = PixelArt.Get(key);
            var root = new GameObject("Parallax " + key).transform;
            root.SetParent(_world, false);
            root.position = new Vector3(_level.Width * 0.5f, y, 0f);

            for (int i = -1; i <= 1; i++)
            {
                var sr = MakeWorldSprite("Layer", sprite, order, root);
                sr.transform.localPosition = new Vector3(i * 48f, 0f, 0f);
            }

            var parallax = root.gameObject.AddComponent<Parallax>();
            parallax.Init(_camera.transform, factor);
        }

        void BuildEntities()
        {
            Theme theme = _level.Theme;

            for (int i = 0; i < _level.Entities.Count; i++)
            {
                EntitySpawn e = _level.Entities[i];
                float bottom = _level.Height - 1 - e.Row;
                float x = e.X + 0.5f;

                switch (e.Type)
                {
                    case 'o':
                        WorldFactory.SpawnGem(new Vector3(x, bottom + 0.6f, 0f), _world);
                        break;
                    case 's':
                        EnemyFactory.SpawnSlime(new Vector3(x, bottom, 0f), _world);
                        break;
                    case 'f':
                        EnemyFactory.SpawnBat(new Vector3(x, bottom + 0.6f, 0f), _world);
                        break;
                    case '^':
                        EnemyFactory.SpawnSpike(new Vector3(x, bottom, 0f), _world);
                        break;
                    case 'b':
                        WorldFactory.SpawnSpring(new Vector3(x, bottom, 0f), _world);
                        break;
                    case 'C':
                        WorldFactory.SpawnCheckpoint(new Vector3(x, bottom, 0f), _world);
                        break;
                    case 'G':
                        WorldFactory.SpawnPortal(new Vector3(x, bottom, 0f), _world);
                        break;
                    case 'T':
                        SpawnDecor(((e.X + e.Row) & 1) == 0 ? theme.TreeA : theme.TreeB,
                            x, bottom, e);
                        break;
                    case 'B':
                        SpawnDecor(theme.Bush, x, bottom, e);
                        break;
                    case 'R':
                        SpawnDecor(theme.Rock, x, bottom, e);
                        break;
                    case 'M':
                        SpawnDecor(theme.Mushroom, x, bottom, e);
                        break;
                    case 'F':
                        SpawnDecor(theme.Flower, x, bottom, e);
                        break;
                    case 'Y':
                        SpawnDecor(CrystalKey(theme, e), x, bottom, e);
                        break;
                    case 'P':
                        SpawnDecor(theme.Pillar, x, bottom, e);
                        break;
                }
            }
        }

        static string CrystalKey(Theme theme, EntitySpawn e)
        {
            if (theme.Id == "cave" && (e.X & 1) == 0)
            {
                const string alt = "crystal_cave_b";
                if (PixelArt.Sprites.ContainsKey(alt)) return alt;
            }
            return theme.Crystal;
        }

        void SpawnDecor(string key, float x, float bottom, EntitySpawn e)
        {
            if (string.IsNullOrEmpty(key)) return;

            float scale = 0.92f + ((e.X * 37 + e.Row * 11) % 17) * 0.01f;
            var sr = MakeWorldSprite("Decor", PixelArt.Get(key), 3);
            sr.transform.position = new Vector3(x, bottom, 0f);
            sr.transform.localScale = Vector3.one * scale;
            sr.flipX = ((e.X + e.Row) & 3) == 0;
        }

        SpriteRenderer MakeWorldSprite(string name, Sprite sprite, int order, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : _world, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
