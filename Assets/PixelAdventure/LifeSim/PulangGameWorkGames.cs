using System.Collections.Generic;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Mini-game kerja yang berbeda untuk tiap pekerjaan (tema di PulangWorkThemes):
    /// - Pilihan  : warung, kafe, kantor, guru, dokter. Pilih jawaban benar sebelum waktu habis.
    /// - Kombo    : kurir, montir, koki, desainer, perawat. Tekan urutan panah; salah sekali = gagal.
    /// - Tekan    : petani, tukang kebun. Tekan E berulang sampai bar penuh (bar terus menyusut).
    /// - Tepat    : tukang cukur, nelayan (zona ikut bergerak seperti ikan melawan).
    /// - Bidik    : fotografer, polisi. Gerakkan bidikan ke sasaran bergerak lalu tekan E.
    /// Makin tinggi jenjang pekerjaan, makin cepat & sulit. Kombo beruntun dicatat; kerja sempurna dapat bonus.
    /// </summary>
    public partial class PulangGame
    {
        WorkTheme _wgTheme;
        int _wgRounds, _wgRound, _wgStreak, _wgBestStreak, _wgTier;
        float _wgTimer, _wgTimerMax, _wgPause, _wgFeedbackT;
        bool _wgResolved, _wgShiftQuit;

        // Tepat waktu
        float _wgMarker, _wgDir = 1f, _wgZone, _wgZoneVel;
        // Pilihan
        string[] _wgOpts;
        int _wgAnswer, _wgSel;
        readonly List<int> _wgQuestionBag = new List<int>();
        // Kombo
        int[] _wgSeq;
        int _wgSeqPos;
        bool _wgSeqFailed;
        // Tekan berulang
        float _wgGauge;
        int _wgNeed;
        // Bidik
        Vector2 _wgCross, _wgTarget, _wgTargetVel;
        float _wgTargetTurn;
        readonly List<int> _wgTaskBag = new List<int>();

        void BeginWorkGame(string jobId, int tier)
        {
            _wgTheme = PulangWorkThemes.For(jobId);
            _wgTier = Mathf.Clamp(tier, 0, 4);
            _wgRounds = _wgTheme.Rounds;
            _wgRound = 0;
            _wgStreak = _wgBestStreak = 0;
            _wgShiftQuit = false;
            _wgQuestionBag.Clear();
            _wgTaskBag.Clear();
            _hud.ShowWorkGame(_wgTheme.Title, _wgTheme.Instruction, _wgTheme.Icon, _wgTheme.Game, _wgTheme.Target);
            StartWorkRound();
        }

        static int Draw(List<int> bag, int count)
        {
            if (count <= 0) return 0;
            if (bag.Count == 0) for (int i = 0; i < count; i++) bag.Add(i);
            int k = Random.Range(0, bag.Count);
            int v = bag[k];
            bag.RemoveAt(k);
            return v;
        }

        string RoundLabel() => _wgTheme.RoundName + " " + (_wgRound + 1) + " / " + _wgRounds;

        void StartWorkRound()
        {
            _wgResolved = false;
            _wgPause = 0f;
            string task = _wgTheme.Tasks.Length > 0 ? _wgTheme.Tasks[Draw(_wgTaskBag, _wgTheme.Tasks.Length)] : " ";

            switch (_wgTheme.Game)
            {
                case WorkGame.Timing:
                    _wgMarker = 0f; _wgDir = 1f;
                    _wgZone = Random.Range(0.2f, 0.8f);
                    _wgZoneVel = _wgTheme.MovingZone ? (Random.value < 0.5f ? -1f : 1f) * (0.25f + 0.05f * _wgTier) : 0f;
                    _wgTimerMax = -1f;
                    break;

                case WorkGame.Choice:
                {
                    var q = _wgTheme.Questions[Draw(_wgQuestionBag, _wgTheme.Questions.Length)];
                    task = q.Ask;
                    var opts = new List<string>(q.Options);
                    string right = opts[0];
                    for (int i = opts.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (opts[i], opts[j]) = (opts[j], opts[i]); }
                    _wgOpts = opts.ToArray();
                    _wgAnswer = opts.IndexOf(right);
                    _wgSel = 0;
                    _wgTimerMax = Mathf.Max(3.5f, 7.5f - _wgTier * 0.8f);
                    break;
                }

                case WorkGame.Combo:
                {
                    int len = Mathf.Clamp(4 + _wgTier + _wgRound / 2, 4, 8);
                    _wgSeq = new int[len];
                    for (int i = 0; i < len; i++) _wgSeq[i] = Random.Range(0, 4);
                    _wgSeqPos = 0;
                    _wgSeqFailed = false;
                    _wgTimerMax = 1.6f + len * Mathf.Max(0.5f, 0.85f - _wgTier * 0.07f);
                    break;
                }

                case WorkGame.Mash:
                    _wgGauge = 0f;
                    _wgNeed = 14 + _wgTier * 2 + _wgRound * 2;
                    _wgTimerMax = 4.2f;
                    break;

                case WorkGame.Aim:
                    _wgCross = new Vector2(0.5f, 0.5f);
                    _wgTarget = new Vector2(Random.Range(0.15f, 0.85f), Random.Range(0.2f, 0.8f));
                    if ((_wgTarget - _wgCross).magnitude < 0.25f) _wgTarget.x = _wgTarget.x < 0.5f ? 0.15f : 0.85f;
                    _wgTargetVel = Random.insideUnitCircle.normalized * (0.22f + 0.05f * _wgTier + 0.03f * _wgRound);
                    _wgTargetTurn = Random.Range(0.6f, 1.4f);
                    _wgTimerMax = Mathf.Max(3.5f, 6f - _wgTier * 0.4f);
                    break;
            }
            _wgTimer = _wgTimerMax;
            _hud.WorkTask(RoundLabel(), task);
            RefreshWorkStatus();
        }

        void RefreshWorkStatus() =>
            _hud.WorkStatus("TEPAT " + _workHits + " / " + _wgRounds + (_wgStreak >= 2 ? "    KOMBO x" + _wgStreak : ""));

        void UpdateWork(float dt)
        {
            // Umpan balik memudar; jeda singkat antar ronde.
            if (_wgFeedbackT > 0f) _wgFeedbackT = Mathf.Max(0f, _wgFeedbackT - dt);
            _hud.WorkFeedback(_lastFeedback ?? " ", _lastFeedbackGood, Mathf.Clamp01(_wgFeedbackT / 0.6f));

            // ESC / tombol X: berhenti kerja lebih awal. Ronde yang belum dimainkan dihitung gagal.
            if (LifeInput.CancelPressed && _inputLock <= 0f)
            {
                GameAudio.Play("click", 0.4f);
                if (_workHelp)
                {
                    // Bantuan bengkel (side quest) tidak selesai bila ditinggal; bisa diulang nanti.
                    _hud.HideWork();
                    _state = State.Playing;
                    Say(" ", "Kamu berhenti membantu di bengkel. Bisa dicoba lagi nanti.");
                    return;
                }
                _wgShiftQuit = true;
                FinishShift();
                return;
            }

            if (_wgResolved)
            {
                _wgPause -= dt;
                PushWorkVisuals();
                if (_wgPause <= 0f)
                {
                    _wgRound++;
                    if (_wgRound >= _wgRounds) { FinishShift(); return; }
                    StartWorkRound();
                }
                return;
            }

            if (_wgTimerMax > 0f)
            {
                _wgTimer -= dt;
                if (_wgTimer <= 0f) { _wgTimer = 0f; ResolveWorkRound(false, "WAKTU HABIS!"); PushWorkVisuals(); return; }
            }

            bool locked = _inputLock > 0f;
            switch (_wgTheme.Game)
            {
                case WorkGame.Timing:
                {
                    float speed = 0.9f + 0.15f * _wgTier + 0.05f * _wgRound;
                    _wgMarker += _wgDir * speed * dt;
                    if (_wgMarker > 1f) { _wgMarker = 1f; _wgDir = -1f; }
                    if (_wgMarker < 0f) { _wgMarker = 0f; _wgDir = 1f; }
                    if (_wgTheme.MovingZone)
                    {
                        _wgZone += _wgZoneVel * dt;
                        if (_wgZone < 0.15f || _wgZone > 0.85f) { _wgZoneVel = -_wgZoneVel; _wgZone = Mathf.Clamp(_wgZone, 0.15f, 0.85f); }
                        if (Random.value < dt * 0.6f) _wgZoneVel = -_wgZoneVel;      // ikan berbalik arah
                    }
                    if (!locked && (LifeInput.InteractPressed || WorkTapped()))
                        ResolveWorkRound(Mathf.Abs(_wgMarker - _wgZone) <= _wgTheme.ZoneHalf, null);
                    break;
                }

                case WorkGame.Choice:
                    if (!locked)
                    {
                        if (LifeInput.UpPressed) { _wgSel = (_wgSel + _wgOpts.Length - 1) % _wgOpts.Length; GameAudio.Play("click", 0.3f); }
                        if (LifeInput.DownPressed) { _wgSel = (_wgSel + 1) % _wgOpts.Length; GameAudio.Play("click", 0.3f); }
                        // Kursor: arahkan = pilih jawaban, klik = jawab.
                        if (_hud.PointerHover(UiHit.WorkChoice, out int hv) && hv != _wgSel && hv < _wgOpts.Length)
                        { _wgSel = hv; GameAudio.Play("click", 0.25f); }
                        bool clicked = _hud.PointerClick(UiHit.WorkChoice, out int pick) && pick < _wgOpts.Length;
                        if (clicked) _wgSel = pick;
                        if (LifeInput.InteractPressed || clicked) ResolveWorkRound(_wgSel == _wgAnswer, null);
                    }
                    break;

                case WorkGame.Combo:
                    if (!locked)
                    {
                        int pressed = LifeInput.UpPressed ? 0 : LifeInput.RightPressed ? 1 : LifeInput.DownPressed ? 2 : LifeInput.LeftPressed ? 3 : -1;
                        if (pressed < 0 && _hud.PointerClick(UiHit.WorkArrow, out int arrow)) pressed = arrow;   // tombol panah di layar
                        if (pressed >= 0)
                        {
                            if (pressed == _wgSeq[_wgSeqPos])
                            {
                                _wgSeqPos++;
                                GameAudio.Play("click", 0.4f, 1f + _wgSeqPos * 0.06f);
                                if (_wgSeqPos >= _wgSeq.Length) ResolveWorkRound(true, null);
                            }
                            else { _wgSeqFailed = true; ResolveWorkRound(false, null); }
                        }
                    }
                    break;

                case WorkGame.Mash:
                    _wgGauge = Mathf.Max(0f, _wgGauge - dt * 0.12f);
                    if (!locked && (LifeInput.InteractPressed || WorkTapped()))
                    {
                        _wgGauge += 1f / _wgNeed;
                        GameAudio.Play("step_hard", 0.3f, Random.Range(0.9f, 1.15f));
                        if (_wgGauge >= 1f) { _wgGauge = 1f; ResolveWorkRound(true, null); }
                    }
                    break;

                case WorkGame.Aim:
                {
                    // Sasaran bergerak & berbelok acak, memantul di tepi area.
                    _wgTargetTurn -= dt;
                    if (_wgTargetTurn <= 0f)
                    {
                        _wgTargetVel = Random.insideUnitCircle.normalized * _wgTargetVel.magnitude;
                        _wgTargetTurn = Random.Range(0.6f, 1.4f);
                    }
                    _wgTarget += _wgTargetVel * dt;
                    if (_wgTarget.x < 0.06f || _wgTarget.x > 0.94f) { _wgTargetVel.x = -_wgTargetVel.x; _wgTarget.x = Mathf.Clamp(_wgTarget.x, 0.06f, 0.94f); }
                    if (_wgTarget.y < 0.15f || _wgTarget.y > 0.9f) { _wgTargetVel.y = -_wgTargetVel.y; _wgTarget.y = Mathf.Clamp(_wgTarget.y, 0.15f, 0.9f); }

                    var move = new Vector2(LifeInput.X, LifeInput.Y);
                    _wgCross += new Vector2(move.x * 0.55f, move.y * 1.5f) * dt;
                    _wgCross = new Vector2(Mathf.Clamp01(_wgCross.x), Mathf.Clamp01(_wgCross.y));
                    // Mouse: bidikan langsung mengikuti kursor di dalam area bidik.
                    if (_hud.PointerMoved && _hud.PointerIn(UiHit.WorkAim, out Vector2 aim)) _wgCross = aim;
                    if (!locked && (LifeInput.InteractPressed || WorkTapped()))
                    {
                        Vector2 d = _wgCross - _wgTarget;
                        d.y *= 0.3f;                       // area lebih lebar daripada tinggi
                        GameAudio.Play("page", 0.5f, 1.6f);
                        ResolveWorkRound(d.magnitude < 0.06f, null);
                    }
                    break;
                }
            }
            PushWorkVisuals();
        }

        void PushWorkVisuals()
        {
            _hud.WorkTimer(_wgTimerMax > 0f ? Mathf.Clamp01(_wgTimer / _wgTimerMax) : -1f);
            switch (_wgTheme.Game)
            {
                case WorkGame.Timing: _hud.WorkTiming(_wgMarker, _wgZone, _wgTheme.ZoneHalf); break;
                case WorkGame.Choice: _hud.WorkChoices(_wgOpts, _wgSel); break;
                case WorkGame.Combo: _hud.WorkCombo(_wgSeq, _wgSeqPos, _wgSeqFailed); break;
                case WorkGame.Mash: _hud.WorkGauge(_wgGauge); break;
                case WorkGame.Aim: _hud.WorkAim(_wgCross, _wgTarget); break;
            }
        }

        string _lastFeedback;
        bool _lastFeedbackGood;

        void ResolveWorkRound(bool hit, string missOverride)
        {
            _wgResolved = true;
            _wgPause = 0.85f;
            if (hit)
            {
                _workHits++;
                _wgStreak++;
                _wgBestStreak = Mathf.Max(_wgBestStreak, _wgStreak);
                _lastFeedback = _wgTheme.HitLines[Random.Range(0, _wgTheme.HitLines.Length)] + (_wgStreak >= 2 ? "  x" + _wgStreak : "");
                GameAudio.Play(_wgStreak >= 3 ? "buy" : "serve", 0.55f, 1f + Mathf.Min(_wgStreak, 5) * 0.04f);
            }
            else
            {
                _wgStreak = 0;
                _lastFeedback = missOverride ?? _wgTheme.MissLines[Random.Range(0, _wgTheme.MissLines.Length)];
                if (_wgTheme.Game == WorkGame.Choice && _wgOpts != null)
                    _hud.WorkTask(RoundLabel(), "JAWABAN BENAR: " + _wgOpts[_wgAnswer]);
                GameAudio.Play("error", 0.5f);
            }
            _lastFeedbackGood = hit;
            _wgFeedbackT = 0.85f;
            RefreshWorkStatus();
        }
    }
}
