using System;
using System.Collections;
using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>Penanda minimap.</summary>
    public struct MapMarker
    {
        public Vector2 Position;
        public Color Color;
        public bool Star;       // digambar sebagai logo bintang (lokasi pekerjaan)
        public bool Important;  // tujuan misi: lebih besar & berdenyut di minimap
        public MapMarker(Vector2 position, Color color, bool star = false, bool important = false)
        {
            Position = position; Color = color; Star = star; Important = important;
        }
    }

    /// <summary>
    /// Pengendali utama PULANG – Life Beyond Home.
    /// Judul -> input nama -> Prolog -> Chapter 1–6 -> ending tunggal PULANG.
    /// Alur cerita ada di <see cref="PulangStory"/>; kelas ini menghubungkannya ke dunia, HUD, dan input.
    /// </summary>
    public partial class PulangGame : MonoBehaviour
    {
        enum State { Title, NameEntry, Playing, Dialogue, Menu, Work, Pause, Journal, Tasks, Inventory, Tablet, Profile, Guide, Busy, Ending, Contacts, WorldMap, Settings, Amount }

        public static PulangGame Instance { get; private set; }
        public bool CanAmbientMove => _state == State.Playing;
        public Vector2 PlayerFeet => _player != null ? _player.Feet : new Vector2(-999f, -999f);

        Camera _cam;
        LifeWorld _world;
        LifePlayer _player;
        LifeCamera _cameraRig;
        LifeHud _hud;
        readonly PulangStory _story = new PulangStory();
        PulangSaveData D => _story.Data;

        State _state = State.Busy;
        float _clock;
        float _inputLock;
        int _lastTickMinute = -1;

        // Layar nama
        NameEntry _nameEntry;
        NameEntryInput _nameInput;
        float _caretTime;
        string _lastNameShown;

        // Hal yang hanya berlaku hari ini (tidak disimpan)
        readonly HashSet<string> _today = new HashSet<string>();

        // ------------------------------------------------------------------ boot

        public void Boot(Camera cam)
        {
            Instance = this;
            _cam = cam;
            LifeView.SetCamera(cam);

            LifeArt.BuildAll();
            GameAudio.Init(gameObject);

            var worldGo = new GameObject("Dunia");
            worldGo.transform.SetParent(transform, false);
            _world = worldGo.AddComponent<LifeWorld>();
            _world.Build();

            _player = LifePlayer.Create(_world, 0f, 0f);
            _player.transform.SetParent(transform, false);
            SpawnNpcs();
            SpawnPickups();

            _cameraRig = cam.GetComponent<LifeCamera>();
            if (_cameraRig == null) _cameraRig = cam.gameObject.AddComponent<LifeCamera>();
            _cameraRig.Init(cam, _player.transform);

            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(transform, false);
            _hud = hudGo.AddComponent<LifeHud>();
            _hud.Init(cam);

            _nameInput = gameObject.AddComponent<NameEntryInput>();
            _nameInput.enabled = false;

            _story.PlayScene += OnStoryScene;
            _story.StateChanged += OnStoryStateChanged;
            _story.ObjectiveCompleted += text => { _hud.ShowToast(text.ToUpperInvariant()); GameAudio.Play("quest_done", 0.55f); SyncQuestItems(); };
            _story.GameFinished += OnGameFinished;
            _story.AnyEvent += OnSideEvent;

            PlacePlayerAt(Region.DesaSukaMakmur, _world.Spot(LocationId.RumahZidan));
            ShowTitle();
        }

        // ------------------------------------------------------------------ judul

        void StartNewGame()
        {
            _today.Clear();
            _questItemsBaseline = false;
            _story.NewGame();                 // -> NameEntry (lewat StateChanged)
            ApplyOutfit();
            _clock = D.minutes;
            PlacePlayerAt(Region.DesaSukaMakmur, _world.Spot(LocationId.RumahZidan));
            RefreshNpcVisibility();
        }

        void ContinueGame(PulangSaveData data)
        {
            _today.Clear();
            _questItemsBaseline = false;
            _clock = data.minutes;
            Vector2 pos = data.posX >= 0f ? new Vector2(data.posX, data.posY) : _world.ArrivalSpot(data.region);
            PlacePlayerAt(data.region, pos);
            _hud.HideTitle();
            _story.Resume(data);
            ApplyOutfit();
            if (data.state != StoryState.NameEntry) EnterPlaying();
            RefreshNpcVisibility();
        }

        // ------------------------------------------------------------------ input nama

        void BeginNameEntry()
        {
            _state = State.NameEntry;
            _hud.SetGameplayVisible(false);
            _nameEntry = new NameEntry();
            _nameEntry.Confirmed += name =>
            {
                _hud.HideTitle();
                _hud.ResetTitleTexts();
                GameAudio.Play("win", 0.6f);
                _inputLock = 0.3f;
                EnterPlaying();
                _story.ConfirmName(name);      // -> Prolog, adegan pertama diputar
            };
            _nameInput.Begin(_nameEntry);
            _lastNameShown = null;
        }

        void UpdateNameEntry(float dt)
        {
            if (_nameEntry == null) return;
            _caretTime += dt;
            bool caret = (_caretTime % 1f) < 0.55f;
            string key = _nameEntry.Display + caret;
            if (key == _lastNameShown) return;
            _lastNameShown = key;
            _hud.ShowNameEntry(_nameEntry.Display, !_nameEntry.IsEmpty, caret);
        }

        // ------------------------------------------------------------------ loop

        void Update()
        {
            if (_hud == null) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            _inputLock = Mathf.Max(0f, _inputLock - dt);

            if (_state != State.NameEntry && LifeInput.MutePressed)
            {
                GameAudio.ToggleMute();
                if (_state == State.Pause) _hud.ShowPause(GameAudio.Muted);
            }

            // Tombol X di pojok layar yang terbuka = ESC (menutup / kembali).
            bool closable = CanCloseScreen();
            _hud.SetCloseAllowed(closable);
            _hud.SetPlayInput(_state == State.Playing);   // bilah tombol HUD hanya saat bermain bebas
            if (closable && _inputLock <= 0f && _hud.CloseButtonClicked())
            {
                GameAudio.Play("click", 0.4f);
                LifeInput.InjectCancel();
            }

            switch (_state)
            {
                case State.Title: UpdateTitle(); break;
                case State.NameEntry: UpdateNameEntry(dt); break;
                case State.Playing: UpdatePlaying(dt); break;
                case State.Dialogue: UpdateDialogue(); break;
                case State.Menu: UpdateMenu(); break;
                case State.Work: UpdateWork(dt); break;
                case State.Pause: UpdatePause(); break;
                case State.Journal: UpdateJournal(); break;
                case State.Tasks: UpdateTasks(); break;
                case State.Inventory: UpdateInventory(); break;
                case State.Tablet: UpdateTablet(); break;
                case State.Profile: UpdateProfile(); break;
                case State.Guide: UpdateGuide(); break;
                case State.Contacts: UpdateContacts(); break;
                case State.WorldMap: UpdateWorldMap(); break;
                case State.Settings: UpdateSettings(); break;
                case State.Amount: UpdateAmount(); break;
                case State.Ending: UpdateEnding(); break;
            }

            if (_story.Data != null) SyncNight();
        }

        /// <summary>Layar yang sedang terbuka bisa ditutup dengan tombol X (sama dengan ESC)?</summary>
        bool CanCloseScreen()
        {
            switch (_state)
            {
                case State.Menu: return _menuCancellable;
                case State.Pause:
                case State.Journal:
                case State.Tasks:
                case State.Inventory:
                case State.Tablet:
                case State.Profile:
                case State.Guide:
                case State.Contacts:
                case State.WorldMap:
                case State.Settings:
                case State.Amount:
                case State.Work:      // mini-game kerja: X / ESC = berhenti lebih awal
                    return true;
                case State.Title: return _homePage == HomePage.Slots;   // slot simpanan -> kembali ke menu utama
                default: return false;
            }
        }

        void EnterPlaying()
        {
            _state = State.Playing;
            _hud.SetGameplayVisible(true);
            _inputLock = Mathf.Max(_inputLock, 0.15f);
            SyncHud();
            if (_queuedScene != null)
            {
                string s = _queuedScene;
                _queuedScene = null;
                OnStoryScene(s);
                return;
            }
            TryAutoScene();
        }

        /// <summary>
        /// Kecepatan jam permainan (menit game per detik nyata). Siang 06.00–20.00 ±8,75 menit nyata,
        /// malam 20.00–06.00 ±4,5 menit nyata: satu hari penuh ±13 menit (sebelumnya ±8 menit).
        /// </summary>
        const float DayMinutesPerSecond = 1.6f;
        const float NightMinutesPerSecond = 2.2f;

        void UpdatePlaying(float dt)
        {
            _clock += dt * ((_clock < 360f || _clock >= 1200f) ? NightMinutesPerSecond : DayMinutesPerSecond);
            if (_clock >= 1440f)
            {
                PassOut();
                return;
            }
            D.minutes = Mathf.Clamp((int)_clock, 0, 1439);

            if (HandleTap()) return;                      // ketuk / klik: jalan ke titik atau objek
            bool onMotor = ApplyVehicleSpeed();
            _player.Tick(dt, !onMotor && D.energy > RunMinEnergy);   // di atas motor tidak perlu berlari
            AfterTapMove();
            UpdateRunEnergy(dt);
            if (!_world.InInterior)
            {
                D.posX = _player.Feet.x;
                D.posY = _player.Feet.y;
                FaceNearbyNpcs();
                _world.UpdateSigns(_player.Feet);
                CheckArrival();
                if (CheckEdgeTravel()) return;    // berjalan ke ujung peta -> wilayah sebelah
            }
            else if (CheckInteriorExit()) return;

            if (D.minutes != _lastTickMinute)
            {
                _lastTickMinute = D.minutes;
                _story.Tick();
                if (_state != State.Playing) return;
            }

            UpdateInteraction();
            SyncHud();

            if (LifeInput.JournalPressed) { OpenJournal(); return; }
            if (LifeInput.TasksPressed) { OpenTasks(); return; }
            if (LifeInput.InventoryPressed) { OpenInventory(false); return; }
            if (LifeInput.TabletPressed) { OpenTablet(); return; }
            if (LifeInput.GuidePressed) { OpenGuide(false); return; }
            if (LifeInput.MapPressed) { OpenWorldMap(false); return; }
            if (LifeInput.PausePressed) { OpenPause(); return; }
            if (LifeInput.VehiclePressed && _inputLock <= 0f) { ToggleVehicle(); return; }
            if (LifeInput.InteractPressed && _inputLock <= 0f) Interact();
        }

        // ------------------------------------------------------------------ cerita

        string _queuedScene;

        void OnStoryScene(string id)
        {
            if (_state != State.Playing)
            {
                _queuedScene = id;
                return;
            }
            RunScene(id, () => _story.Notify(StoryEvent.SceneDone, id));
        }

        void OnStoryStateChanged(StoryState s)
        {
            _insideLoc = null;
            if (s == StoryState.NameEntry)
            {
                BeginNameEntry();
                return;
            }
            if (D.region != _world.CurrentRegion)
                PlacePlayerAt(D.region, _world.ArrivalSpot(D.region));
            RefreshNpcVisibility();
            RefreshPickups();
            TryAutoScene();
        }

        void OnGameFinished()
        {
            _state = State.Ending;
            _hud.SetGameplayVisible(false);
            _hud.HideDialogue();
            _hud.ShowPulangEnding(PulangProfile.Name.ToUpperInvariant(), D.day);
            GameAudio.PlayMusic(5);
            _inputLock = 1.5f;
        }

        void UpdateEnding()
        {
            if (_inputLock > 0f || !(LifeInput.InteractPressed || LifeInput.PointerTapped)) return;
            PulangSaveData.Delete();         // tidak ada ending lain; mulai lagi dari awal
            ShowTitle();
        }

        // ------------------------------------------------------------------ dialog

        readonly List<Line> _lines = new List<Line>();
        int _lineIndex;
        Action _linesDone;

        void Say(IList<Line> lines, Action done)
        {
            _lines.Clear();
            if (lines != null) _lines.AddRange(lines);
            if (_lines.Count == 0) { done?.Invoke(); return; }
            _lineIndex = 0;
            _linesDone = done;
            _state = State.Dialogue;
            _hud.SetPrompt(null);
            ShowLine();
            _inputLock = 0.2f;
        }

        void Say(string speaker, string text, Action done = null) => Say(new[] { new Line(speaker, text) }, done);

        void ShowLine()
        {
            Line l = _lines[_lineIndex];
            string speaker = string.IsNullOrEmpty(l.Speaker) ? " " : PulangProfile.Format(l.Speaker).ToUpperInvariant();
            _hud.ShowDialogue(speaker, PulangProfile.Format(l.Text).ToUpperInvariant());
            ShowLinePortraits(l.Speaker);
            MarkKnownBySpeaker(PulangProfile.Format(l.Speaker));
        }

        // Potret percakapan ala Citampi: lawan bicara di kiri, tokoh utama di kanan.
        string _portraitNpc;

        void ShowLinePortraits(string rawSpeaker)
        {
            if (_lineIndex == 0) _portraitNpc = null;

            bool isPlayer = rawSpeaker != null && rawSpeaker.Contains("{nama}");
            bool narration = string.IsNullOrWhiteSpace(rawSpeaker);
            string npcKey = isPlayer || narration ? null : NpcPortraitKey(PulangProfile.Format(rawSpeaker));

            // Pembicara bernama tanpa potret (mis. "HRD"): lawan bicara sebelumnya disembunyikan.
            if (npcKey != null) _portraitNpc = npcKey;
            else if (!isPlayer && !narration) _portraitNpc = null;

            string playerKey = isPlayer || _portraitNpc != null ? "lc_player_down_0" : null;
            int speaking = isPlayer ? 1 : npcKey != null ? 0 : -1;
            _hud.SetDialoguePortraits(_portraitNpc, playerKey, speaking);
        }

        static string NpcPortraitKey(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return null;
            foreach (var def in PulangCast.All)
                if (string.Equals(def.Name, speaker, StringComparison.OrdinalIgnoreCase)
                    && PulangNpcLines.Sprite.TryGetValue(def.Id, out string s))
                    return "lc_" + s + "_down_0";
            return null;
        }

        void UpdateDialogue()
        {
            if (_inputLock > 0f) return;
            if (!LifeInput.InteractPressed && !LifeInput.CancelPressed && !LifeInput.PointerTapped) return;   // ketuk = lanjut

            _lineIndex++;
            if (_lineIndex < _lines.Count)
            {
                GameAudio.Play("talk", 0.55f, 1.15f);
                ShowLine();
                _inputLock = 0.08f;
                return;
            }

            _hud.HideDialogue();
            var done = _linesDone;
            _linesDone = null;
            _state = State.Playing;
            _hud.SetGameplayVisible(true);
            _inputLock = 0.15f;
            done?.Invoke();
            if (_state == State.Playing) AfterReturnToPlaying();
        }

        /// <summary>Putar satu adegan: baris, pilihan, balasan, lalu After.</summary>
        void RunScene(string id, Action done)
        {
            if (!PulangScript.Scenes.TryGetValue(id, out SceneDef scene))
            {
                Debug.LogWarning("Adegan tidak ditemukan: " + id);
                done?.Invoke();
                return;
            }

            Say(scene.Lines, () =>
            {
                if (scene.Choices.Length == 0)
                {
                    Say(scene.After, () => FinishScene(id, done));
                    return;
                }
                var labels = new List<string>();
                var actions = new List<Action>();
                foreach (var c in scene.Choices)
                {
                    Choice choice = c;
                    labels.Add(PulangProfile.Format(choice.Label).ToUpperInvariant());
                    actions.Add(() =>
                    {
                        ApplyEffect(choice.Effect);
                        Say(choice.Reply, () => Say(scene.After, () => FinishScene(id, done)));
                    });
                }
                OpenMenu("PILIH JAWABAN", labels, actions, false);
            });
        }

        void FinishScene(string id, Action done)
        {
            if (id == "ch1_nabila") GirlOf(NpcId.Nabila).met = true;
            if (id == "ch3_melati") GirlOf(NpcId.Melati).met = true;
            if (id == "ch3_citra") GirlOf(NpcId.Citra).met = true;
            if (id == "ch1_nabila_selesai") GirlOf(NpcId.Nabila).closeness += 5; // kedekatan naik setelah berhasil
            done?.Invoke();
        }

        // Kejadian cerita dari efek pilihan, dikirim setelah adegan selesai.
        readonly List<KeyValuePair<StoryEvent, string>> _pendingEvents = new List<KeyValuePair<StoryEvent, string>>();

        void QueueStoryEvent(StoryEvent ev, string key) => _pendingEvents.Add(new KeyValuePair<StoryEvent, string>(ev, key));

        /// <summary>Setelah dialog/menu tertutup: kirim kejadian tertunda, lalu putar adegan yang antre.</summary>
        void AfterReturnToPlaying()
        {
            FlushStoryEvents();
            if (_state != State.Playing) return;
            if (_queuedScene != null)
            {
                string s = _queuedScene;
                _queuedScene = null;
                OnStoryScene(s);
                return;
            }
            TryAutoScene();
        }

        void FlushStoryEvents()
        {
            if (_pendingEvents.Count == 0) return;
            var copy = new List<KeyValuePair<StoryEvent, string>>(_pendingEvents);
            _pendingEvents.Clear();
            foreach (var e in copy) _story.Notify(e.Key, e.Value);
        }

        void ApplyEffect(string effect)
        {
            if (string.IsNullOrEmpty(effect)) return;
            if (effect.StartsWith("flag:")) { D.Set(effect.Substring(5)); return; }
            if (effect.StartsWith("keluarga+")) { D.familyCloseness += int.Parse(effect.Substring(9)); return; }
            if (effect.StartsWith("rep+")) { D.reputation += int.Parse(effect.Substring(4)); return; }
            if (effect.StartsWith("buy:")) { BuyItem(effect.Substring(4), true); return; }
            if (effect.StartsWith("dekat:"))
            {
                string[] parts = effect.Substring(6).Split('+');
                var id = (NpcId)Enum.Parse(typeof(NpcId), parts[0]);
                var g = GirlOf(id);
                g.met = true;
                g.closeness += int.Parse(parts[1]);
            }
        }

        // ------------------------------------------------------------------ menu

        readonly List<Action> _menuActions = new List<Action>();
        int _menuSelected, _menuCount;
        bool _menuCancellable;

        Action _menuOnCancel;

        /// <param name="onCancel">Dipanggil saat menu dibatalkan (ESC / X), mis. kembali ke tablet.</param>
        void OpenMenu(string title, List<string> labels, List<Action> actions, bool cancellable = true, Action onCancel = null)
        {
            _menuActions.Clear();
            _menuActions.AddRange(actions);
            _menuCount = Mathf.Min(labels.Count, 9);
            _menuSelected = 0;
            _menuCancellable = cancellable;
            _menuOnCancel = onCancel;
            _state = State.Menu;
            _hud.ShowMenu(title.ToUpperInvariant(), labels, 0);
            GameAudio.Play("menu_open", 0.3f);
            _inputLock = 0.2f;
        }

        void UpdateMenu()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.UpPressed) { _menuSelected = (_menuSelected + _menuCount - 1) % _menuCount; _hud.SetMenuSelection(_menuSelected); GameAudio.Play("click", 0.4f); }
            if (LifeInput.DownPressed) { _menuSelected = (_menuSelected + 1) % _menuCount; _hud.SetMenuSelection(_menuSelected); GameAudio.Play("click", 0.4f); }

            // Kursor: arahkan = pilih, klik = jalankan.
            if (_hud.PointerHover(UiHit.Menu, out int hover) && hover != _menuSelected && hover < _menuCount)
            {
                _menuSelected = hover;
                _hud.SetMenuSelection(hover);
                GameAudio.Play("click", 0.25f);
            }
            bool clicked = _hud.PointerClick(UiHit.Menu, out int pick) && pick < _menuCount;
            if (clicked) { _menuSelected = pick; _hud.SetMenuSelection(pick); }

            if (_menuCancellable && LifeInput.CancelPressed)
            {
                _hud.HideMenu();
                _state = State.Playing;
                _inputLock = 0.15f;
                var back = _menuOnCancel;
                _menuOnCancel = null;
                back?.Invoke();
                if (_state == State.Playing) AfterReturnToPlaying();   // kirim kejadian tertunda (mis. beli)
                return;
            }
            if (!LifeInput.InteractPressed && !clicked) return;

            Action action = _menuActions[_menuSelected];
            _hud.HideMenu();
            _state = State.Playing;
            _inputLock = 0.15f;
            GameAudio.Play("click", 0.6f);
            _menuOnCancel = null;
            action?.Invoke();
            if (_state == State.Playing) AfterReturnToPlaying();
        }

        // ------------------------------------------------------------------ jeda & jurnal

        void OpenPause()
        {
            D.Save();
            _state = State.Pause;
            _hud.SetPrompt(null);
            _hud.ShowPause(GameAudio.Muted);
            _inputLock = 0.2f;
        }

        void UpdatePause()
        {
            if (_inputLock > 0f) return;
            bool clicked = _hud.PointerClick(UiHit.Pause, out int pick);
            if (clicked && pick == 2)
            {
                GameAudio.ToggleMute();
                _hud.ShowPause(GameAudio.Muted);
                return;
            }
            if (LifeInput.HomePressed || (clicked && pick == 1))
            {
                _hud.HidePause();
                ReturnToHome();
                return;
            }
            if (LifeInput.PausePressed || LifeInput.CancelPressed || LifeInput.InteractPressed || (clicked && pick == 0))
            {
                _hud.HidePause();
                _state = State.Playing;
                _inputLock = 0.15f;
            }
        }


        void UpdateJournal()
        {
            if (_inputLock > 0f) return;
            if (LifeInput.JournalPressed || LifeInput.CancelPressed || LifeInput.InteractPressed)
            {
                _hud.HideJournal();
                CloseSubScreen();
            }
        }

        // ------------------------------------------------------------------ waktu

        /// <param name="wakeAt">Tempat bangun (mis. penginapan); null = rumah/kost.</param>
        void SleepUntilMorning(string message, int energy, LocationId? wakeAt = null)
        {
            StartCoroutine(SleepRoutine(message, energy, wakeAt));
        }

        IEnumerator SleepRoutine(string message, int energy, LocationId? wakeAt)
        {
            _state = State.Busy;
            _hud.SetPrompt(null);
            GameAudio.Play("sleep", 0.6f);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * 2f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(1f);

            D.day++;
            D.minutes = 6 * 60;
            _clock = D.minutes;
            D.energy = Mathf.Clamp(energy, 0, 100);
            _today.Clear();
            _questItemsBaseline = false;

            Region home = _story.Chapter == Chapter.Prolog ? Region.DesaSukaMakmur : Region.KotaJogokerto;
            LocationId bed = home == Region.DesaSukaMakmur ? LocationId.RumahZidan : LocationId.KostPelangi;
            if (wakeAt.HasValue)
            {
                bed = wakeAt.Value;
                home = PulangWorld.Get(bed).Region;
            }
            PlacePlayerAt(home, _world.Spot(bed));
            D.Save();
            yield return new WaitForSecondsRealtime(0.35f);

            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * 2f) { _hud.SetFade(t); yield return null; }
            _hud.SetFade(0f);
            EnterPlaying();
            _hud.ShowToast(("HARI " + D.day + " - " + message).ToUpperInvariant());
        }

        void PassOut() => SleepUntilMorning("Kamu tertidur kelelahan", 60);

        // ------------------------------------------------------------------ lari

        /// <summary>Berlari menguras 1 energi setiap RunDrainSeconds detik (berjalan tidak menguras).</summary>
        const float RunDrainSeconds = 2.5f;
        /// <summary>Di bawah energi ini pemain terlalu lelah untuk berlari.</summary>
        const int RunMinEnergy = 10;

        float _runDrain, _tiredToastCooldown;

        void UpdateRunEnergy(float dt)
        {
            _tiredToastCooldown = Mathf.Max(0f, _tiredToastCooldown - dt);
            if (_player.IsRunning)
            {
                _runDrain += dt;
                if (_runDrain >= RunDrainSeconds)
                {
                    _runDrain -= RunDrainSeconds;
                    D.energy = Mathf.Max(0, D.energy - 1);
                }
                return;
            }

            // Menahan SHIFT sambil bergerak tapi energi habis: beri tahu sekali-sekali.
            bool tryingToRun = LifeInput.RunHeld && (Mathf.Abs(LifeInput.X) > 0.1f || Mathf.Abs(LifeInput.Y) > 0.1f);
            if (tryingToRun && D.energy <= RunMinEnergy && _tiredToastCooldown <= 0f)
            {
                _hud.ShowToast("TERLALU LELAH UNTUK BERLARI - MAKAN ATAU ISTIRAHAT");
                GameAudio.Play("error", 0.3f);
                _tiredToastCooldown = 4f;
            }
        }

        void SyncNight()
        {
            float m = _clock;
            // Malam lebih gelap, makin pekat menjelang tengah malam (21.00 -> 0.74, 00.00-03.00 -> 0.86).
            float deep = m >= 1260f ? Mathf.InverseLerp(1260f, 1440f, m) : m < 180f ? 1f : m < 300f ? 1f - (m - 180f) / 120f : 0f;
            Color night = new Color(0.02f, 0.03f, 0.11f, Mathf.Lerp(0.74f, 0.86f, deep));
            Color dusk = new Color(0.42f, 0.16f, 0.24f, 0.30f);
            Color dawn = new Color(0.30f, 0.20f, 0.36f, 0.32f);
            Color clear = new Color(0f, 0f, 0f, 0f);
            float dark = 0f;
            Color tint = clear;

            if (m < 300f) { dark = 1f; tint = night; }
            else if (m < 390f) { float t = (m - 300f) / 90f; dark = 1f - t; tint = Color.Lerp(night, dawn, t); }
            else if (m < 450f) { tint = Color.Lerp(dawn, clear, (m - 390f) / 60f); }
            else if (m < 990f) { tint = clear; }
            else if (m < 1110f) { tint = Color.Lerp(clear, dusk, (m - 990f) / 120f); }
            else if (m < 1230f) { float t = (m - 1110f) / 120f; dark = t; tint = Color.Lerp(dusk, night, t); }
            else { dark = 1f; tint = night; }

            float lamps = Mathf.Clamp01(dark + tint.a * 0.55f);
            if (_world != null && _world.InInterior) tint.a *= 0.3f;   // di dalam ruangan: lampu menyala, gelap malam berkurang
            _hud.SetNight(tint, lamps);
            _world.SetNight(lamps);

            if (_state == State.Title || _state == State.NameEntry || _state == State.Ending) { GameAudio.PlayAmbience(null); return; }
            GameAudio.PlayAmbience(AmbienceKey(dark));
            bool sad = _story.Data.state >= StoryState.Ch6KabarDuka;
            GameAudio.PlayMusic(sad ? 5 : MusicFor(m));
        }

        // ------------------------------------------------------------------ HUD

        void SyncHud()
        {
            int need = PulangJobs.ExpForNextLevel(D.level);
            int expPct = Mathf.Clamp(Mathf.RoundToInt(D.exp * 100f / need), 0, 100);
            _hud.Refresh(D.day, D.minutes, D.money, D.energy, expPct, "LV " + D.level, CurrentHint());
            _hud.SetSubline("TABUNGAN " + Rp(D.savings));
            if (_world.InInterior) { _hud.SetQuestTargets(null); _hud.SetMinimapVisible(false); }   // di dalam bangunan: tanpa minimap & penunjuk
            else RefreshMinimap();
        }

        string CurrentHint()
        {
            var q = _story.Current;
            for (int i = 0; i < q.Objectives.Length; i++)
                if (D.objectiveProgress[i] < q.Objectives[i].Count && !string.IsNullOrEmpty(q.Objectives[i].Text))
                    return Short(PulangProfile.Format(q.Objectives[i].Text));
            if (q.GateText != null) return Short(q.GateText);
            return Short(PulangProfile.Format(q.Title));
        }

        static string Short(string s)
        {
            s = s.ToUpperInvariant();
            return s.Length > 80 ? s.Substring(0, 79) + "." : s;   // HUD membungkus jadi 2 baris
        }

        public static string Rp(int value) => "RP" + value.ToString("N0").Replace(",", ".");

        static string JobLabel(string id)
        {
            var j = PulangJobs.Get(id);
            return j == null ? "-" : j.Name.ToUpperInvariant();
        }

        // ------------------------------------------------------------------ util

        void PlacePlayerAt(Region region, Vector2 pos)
        {
            LeaveInteriorSilently();
            _world.SetRegion(region);
            ScatterNpcs(region);
            if (_story.Data != null) D.region = region;
            if (!_world.CanStand(pos.x, pos.y)) pos = _world.NearestStandable(pos) ?? _world.ArrivalSpot(region);
            _player.Teleport(pos.x, pos.y);
            _cameraRig?.Snap();
            _world.UpdateSigns(pos);
            _insideLoc = null;
        }

        GirlProgress GirlOf(NpcId id)
        {
            for (int i = 0; i < PulangRomance.Girls.Length; i++)
                if (PulangRomance.Girls[i].Id == id) return D.girls[i];
            return null;
        }

        int GirlIndex(NpcId id)
        {
            for (int i = 0; i < PulangRomance.Girls.Length; i++)
                if (PulangRomance.Girls[i].Id == id) return i;
            return -1;
        }

        /// <summary>Tujuan berikutnya yang belum selesai (dengan aturan berurutan).</summary>
        int NextPendingIndex()
        {
            var q = _story.Current;
            for (int i = 0; i < q.Objectives.Length; i++)
                if (D.objectiveProgress[i] < q.Objectives[i].Count) return i;
            return -1;
        }

        bool IsPending(StoryEvent ev, string key, out int index)
        {
            var q = _story.Current;
            index = -1;
            for (int i = 0; i < q.Objectives.Length; i++)
            {
                if (D.objectiveProgress[i] >= q.Objectives[i].Count) continue;
                var o = q.Objectives[i];
                if (o.Event == ev && (o.Key == key || o.Key == "*")) { index = i; return true; }
                if (q.Sequential) return false;
            }
            return false;
        }
    }
}
