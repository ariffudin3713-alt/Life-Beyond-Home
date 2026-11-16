using System.Globalization;
using PixelAdventure.LifeSim.Pulang;
using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// MENABUNG di kost: pemain mengetik sendiri jumlah uang yang ingin ditabung.
    /// - Keyboard: angka 0-9 (baris atas atau numpad), BACKSPACE / DELETE hapus, ENTER / E simpan, ESC batal.
    /// - Mouse / sentuh: papan angka di layar, tombol cepat (10RB ... SEMUA), tombol BATAL & SIMPAN.
    /// - Pratinjau langsung uang & tabungan setelah disimpan; jumlah melebihi uang ditolak dengan pesan.
    /// Tampilan: LifeHudAmount.
    /// </summary>
    public partial class PulangGame
    {
        const int AmountMaxDigits = 10;
        static readonly int[] DepositChips = { 10000, 50000, 100000, 500000 };

        string _amtDigits = "";
        string _amtWarning;

        /// <summary>Dipanggil dari pilihan "Menabung" di kost.</summary>
        void OpenSavingsMenu()
        {
            _amtDigits = "";
            _amtWarning = null;
            _state = State.Amount;
            _hud.SetPrompt(null);
            GameAudio.Play("menu_open", 0.3f);
            RefreshAmount();
            _inputLock = 0.2f;
        }

        long TypedAmount => _amtDigits.Length == 0 ? 0L : long.Parse(_amtDigits, CultureInfo.InvariantCulture);

        static string RpLong(long value) => "RP" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".");

        static string ShortRp(int value) => value >= 1000000 ? value / 1000000 + "JT" : value / 1000 + "RB";

        void RefreshAmount()
        {
            long a = TypedAmount;
            int money = D.money;
            bool over = a > money;
            bool valid = a > 0 && !over;

            var v = new AmountView
            {
                Title = "MENABUNG",
                Amount = _amtDigits.Length == 0 ? "" : RpLong(a),
                LeftLabel = "UANGMU", LeftNow = Rp(money),
                RightLabel = "TABUNGAN", RightNow = Rp(D.savings),
                CanConfirm = valid,
            };
            if (valid)
            {
                v.LeftAfter = Rp(money - (int)a);
                v.RightAfter = RpLong((long)D.savings + a);
            }

            if (_amtWarning != null) { v.Message = _amtWarning; v.MessageError = true; }
            else if (over) { v.Message = "UANGMU TIDAK CUKUP - MAKSIMAL " + Rp(money); v.MessageError = true; }
            else if (a == 0) v.Message = "KETIK JUMLAH UANG YANG INGIN DITABUNG";
            else
            {
                long after = D.savings + a;
                long target = PulangBalance.SuccessSavings;
                v.Message = after >= target
                    ? "TABUNGAN MENCAPAI TARGET " + RpLong(target) + "!"
                    : "KURANG " + RpLong(target - after) + " LAGI MENUJU TARGET " + RpLong(target);
            }

            foreach (int c in DepositChips)
            {
                v.Chips.Add(ShortRp(c));
                v.ChipOn.Add(money >= c);
            }
            v.Chips.Add("SEMUA");
            v.ChipOn.Add(money > 0);

            _hud.ShowAmount(v);
        }

        void UpdateAmount()
        {
            if (_inputLock > 0f) return;

            // Tombol layar: BATAL / SIMPAN.
            if (_hud.PointerClick(UiHit.AmountButton, out int btn))
            {
                if (btn == 0) CloseAmount();
                else ConfirmAmount();
                return;
            }

            bool back = LifeInput.BackspacePressed;
            if (LifeInput.CancelPressed && !back) { CloseAmount(); return; }   // ESC / X (backspace = hapus angka)
            if (LifeInput.InteractPressed) { ConfirmAmount(); return; }        // ENTER / E / SPASI

            bool changed = false;
            int digit = LifeInput.DigitPressed;
            if (digit >= 0) changed |= TypeDigits(digit.ToString(CultureInfo.InvariantCulture));
            if (back) changed |= EraseDigit();

            if (_hud.PointerClick(UiHit.AmountKey, out int key) && key >= 0 && key < LifeHud.AmountKeys.Length)
                changed |= key == 11 ? EraseDigit() : TypeDigits(LifeHud.AmountKeys[key]);

            if (_hud.PointerClick(UiHit.AmountChip, out int chip))
            {
                int value = chip < DepositChips.Length ? DepositChips[chip] : D.money;
                _amtDigits = value > 0 ? value.ToString(CultureInfo.InvariantCulture) : "";
                changed = true;
            }

            if (changed)
            {
                _amtWarning = null;
                GameAudio.Play("click", 0.3f);
                RefreshAmount();
            }
        }

        bool TypeDigits(string digits)
        {
            bool any = false;
            foreach (char c in digits)
            {
                if (_amtDigits.Length >= AmountMaxDigits) break;
                if (c == '0' && _amtDigits.Length == 0) continue;   // tanpa nol di depan
                _amtDigits += c;
                any = true;
            }
            return any;
        }

        bool EraseDigit()
        {
            if (_amtDigits.Length == 0) return false;
            _amtDigits = _amtDigits.Substring(0, _amtDigits.Length - 1);
            return true;
        }

        void ConfirmAmount()
        {
            long a = TypedAmount;
            if (a <= 0 || a > D.money)
            {
                GameAudio.Play("error", 0.45f);
                _amtWarning = a <= 0 ? "KETIK JUMLAHNYA DULU" : "UANGMU TIDAK CUKUP - MAKSIMAL " + Rp(D.money);
                RefreshAmount();
                return;
            }
            _hud.HideAmount();
            _state = State.Playing;
            _inputLock = 0.15f;
            Deposit((int)a);          // uang -> tabungan, toast, event cerita "menabung"
            AfterReturnToPlaying();   // proses event cerita yang diantrekan
        }

        void CloseAmount()
        {
            _hud.HideAmount();
            GameAudio.Play("click", 0.35f);
            _state = State.Playing;
            _inputLock = 0.15f;
        }
    }
}
