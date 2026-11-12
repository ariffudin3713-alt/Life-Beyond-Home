using UnityEngine;

namespace PixelAdventure.LifeSim
{
    public static partial class LifeArt
    {
        static readonly string[] DirNames = { "down", "up", "left", "right" };

        // ------------------------------------------------------------------ characters

        static void BuildCharacters()
        {
            // Tokoh utama (nama bebas, default Zidan): kaos hijau, celana biru tua.
            Cast("player", 0, "2f2a44", "4a4266", "4fae62", "3a8a4c", "3f4a6b", "f2c49b");

            // Keluarga
            Cast("ratna", 5, "8c8a94", "b8b6c0", "b0705a", "8a4f3e", "4a3a44", "e6bd97");
            Cast("keluarga", 4, "3a3530", "6b5c4c", "6f8f9a", "4c6a74", "3f3a36", "dca781");

            // Lima karakter perempuan
            Cast("nabila", 1, "3b2233", "5a3448", "ff9fc0", "e0749a", "8a4a63", "f6cfa8");   // ceria
            Cast("kirana", 5, "2c2038", "4b3560", "6f8fd6", "4c6bb0", "3c3f56", "f0c7a2");   // kacamata, suka membaca
            Cast("anindya", 3, "6b3a22", "a0603a", "f6b85e", "e18b47", "523044", "e8b98c");  // aktif
            Cast("melati", 1, "1f1a24", "3a3040", "f2e6cf", "c9bfa8", "5a4a5e", "f0c9a4");   // pendiam
            Cast("citra", 2, "3a1f2c", "6b3048", "87d1bc", "4e9d8d", "393c59", "efc29c");    // kreatif

            // NPC pendukung (Bagian 3)
            Cast("hendra", 4, "5a5550", "8a857e", "8a6e52", "6b5038", "3f3a36", "e0b188");
            Cast("dimas", 0, "292c36", "b65f3a", "7cbd75", "43805e", "464b6c", "d8a17b");
            Cast("busari", 5, "b0aab8", "dcd6e2", "d1636b", "a94550", "5a2f36", "e6bd97");
            Cast("joko", 4, "302d28", "7a563a", "5f6a78", "3f4a56", "2f2a2a", "dca781");
            Cast("andi", 0, "322b23", "765638", "eef2f6", "b8c4d0", "39435c", "e8b98c");
            Cast("budi", 4, "4a4038", "6b5c4c", "3a4a6b", "2a3550", "2a2a36", "e0b188");
            Cast("rina", 3, "56353a", "98604b", "8c9fe2", "5d6bb4", "604458", "dca781");
            Cast("darto", 4, "6b6660", "9a958e", "c7a25a", "9a7a3a", "4a4038", "d9a97e");
            Cast("riko", 0, "3f2832", "713c48", "69bdd1", "377b9a", "3c3f56", "ebc49e");

            // NPC warga yang disebut di dokumen
            Cast("sastro", 4, "8c8a94", "b8b6c0", "8a7a4a", "6b5c34", "4a4038", "d9a97e");
            Cast("rt", 4, "302d28", "5a5550", "f0f0e8", "c8c8c0", "2f2a2a", "e0b188");
            Cast("herman", 4, "453252", "705178", "c85c4a", "963a34", "393c59", "f0c9a4");
            Cast("slamet", 4, "5a5550", "8a857e", "4a6b4a", "34503a", "2f2a2a", "dca781");
            Cast("fajar", 0, "292c36", "4a4266", "ef7f5a", "c25a3d", "2f3b52", "dda274");
            Cast("karman", 4, "6b6660", "9a958e", "7a5a3a", "5a4028", "3f3a36", "d9a97e");
            Cast("rukmini", 5, "8c8a94", "b8b6c0", "6f9f8a", "4a7263", "433027", "e8bb92");
            Cast("hadi", 4, "302d28", "5a5550", "3b82b8", "2a5f88", "2a2a36", "e0b188");
            Cast("rudi", 0, "322b23", "765638", "6cbd5c", "4a8a3f", "39435c", "d8a17b");
            Cast("surya", 4, "4a4038", "6b5c4c", "f39c3a", "c97a24", "3f3a36", "dca781");
            Cast("siti", 5, "3b2a22", "6b4c38", "e3cf6a", "b9a246", "523044", "efc29c");
            Cast("pustakawan", 5, "3b2a22", "6b4c38", "8a63ad", "6a4a8a", "433027", "e8bb92");
            Cast("tukangkebun", 4, "4a4038", "6b5c4c", "5f7a63", "41573f", "4a4038", "e0b188");
        }

        static void Cast(string name, int style, string hair, string hairLight,
            string shirt, string shirtDark, string pants, string skin)
        {
            MakeCharacter(name, style, PixelArt.Hex(hair), PixelArt.Hex(hairLight),
                PixelArt.Hex(shirt), PixelArt.Hex(shirtDark), PixelArt.Hex(pants), PixelArt.Hex(skin));
        }

        static void MakeCharacter(string name, int style, Color32 hair, Color32 hairLight,
            Color32 shirt, Color32 shirtDark, Color32 pants, Color32 skin)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                for (int frame = 0; frame < 3; frame++)
                {
                    PixelCanvas canvas = CharacterCanvas(style, hair, hairLight, shirt, shirtDark,
                        pants, skin, dir, frame);
                    Add(canvas, new Vector2(0.5f, 0f), name + "_" + DirNames[dir] + "_" + frame);
                }
            }
        }

        static void Part(PixelCanvas c, int x, int y, int w, int h, Color32 fill)
        {
            c.Rect(x - 1, y - 1, w + 2, h + 2, Outline);
            c.Rect(x, y, w, h, fill);
        }

        static PixelCanvas CharacterCanvas(int style, Color32 hair, Color32 hairLight,
            Color32 shirt, Color32 shirtDark, Color32 pants, Color32 skin,
            int dir, int frame)
        {
            var c = new PixelCanvas(16, 26);
            Color32 eye = PixelArt.Hex("2a1d2e");
            Color32 shoe = PixelArt.Hex("2b2233");
            Color32 blush = new Color32(255, 150, 160, 180);

            bool side = dir == 2 || dir == 3;
            bool back = dir == 1;

            int bob = frame == 1 ? 1 : 0;
            int legA = frame == 1 ? -1 : 0;
            int legB = frame == 2 ? 1 : 0;
            int legHA = frame == 1 ? 4 : 5;
            int legHB = frame == 2 ? 4 : 5;

            // legs / shoes
            if (style == 1)
            {
                // dress: only shoes peek out
                c.Rect(4, 1, 3, 3, Outline);
                c.Rect(5, 2, 2, 2, skin);
                c.Rect(5, 0, 3, 2, Outline);
                c.Rect(6, 1, 2, 1, shoe);
                c.Rect(9, 1, 3, 3, Outline);
                c.Rect(9, 2, 2, 2, skin);
                c.Rect(9, 0, 3, 2, Outline);
                c.Rect(9, 1, 2, 1, shoe);
            }
            else
            {
                c.Rect(4 + legA, 1, 4, legHA + 2, Outline);
                c.Rect(5 + legA, 2, 2, legHA, pants);
                c.Rect(4 + legA, 0, 4, 2, Outline);
                c.Rect(5 + legA, 1, 3, 1, shoe);
                c.Rect(9 + legB, 1, 4, legHB + 2, Outline);
                c.Rect(9 + legB, 2, 2, legHB, pants);
                c.Rect(9 + legB, 0, 4, 2, Outline);
                c.Rect(9 + legB, 1, 3, 1, shoe);
            }

            // torso
            int bodyX = side ? 5 : 4;
            int bodyW = side ? 7 : 8;
            int bodyY = 8 + bob;
            int bodyH = 9;
            Part(c, bodyX, bodyY, bodyW, bodyH, shirt);
            c.Rect(bodyX + 1, bodyY + 1, 2, bodyH - 2, shirtDark);

            if (style == 1)
            {
                // flared skirt
                c.Rect(2, 5 + bob, 12, 2, Outline);
                c.Rect(3, 6 + bob, 10, 3, shirt);
                c.Rect(2, 4 + bob, 12, 2, shirtDark);
                c.Rect(2, 4 + bob, 12, 1, PixelArt.Hex("b85a80"));
                c.Rect(3, 7 + bob, 2, 2, shirtDark);
                c.Rect(11, 7 + bob, 2, 2, shirtDark);
            }
            else if (style == 2)
            {
                // apron over dress
                c.Rect(6, bodyY + 1, 5, bodyH - 3, PixelArt.Hex("fff7df"));
                c.Rect(6, bodyY + 1, 1, bodyH - 3, PixelArt.Hex("e0d2b8"));
                c.Rect(6, bodyY, 5, 3, PixelArt.Hex("c85c4a"));
                c.Rect(4, 5 + bob, 8, 4, Outline);
                c.Rect(5, 6 + bob, 6, 3, shirtDark);
            }
            else if (style == 3)
            {
                // little vest
                c.Rect(bodyX + 1, bodyY + 3, 2, bodyH - 5, shirtDark);
                c.Rect(bodyX + bodyW - 3, bodyY + 3, 2, bodyH - 5, shirtDark);
            }

            // belt
            c.Rect(bodyX, bodyY - 1, bodyW, 2, pants);

            // arms
            int armY = bodyY + 2;
            int armH = 6;
            if (style == 1)
            {
                c.Rect(bodyX - 2, armY - 1, 2, armH + 2, Outline);
                c.Rect(bodyX - 2, armY, 1, armH, skin);
                c.Rect(bodyX + bodyW, armY - 1, 2, armH + 2, Outline);
                c.Rect(bodyX + bodyW + 1, armY, 1, armH, skin);
            }
            else
            {
                c.Rect(bodyX - 2, armY - 1, 2, armH + 2, Outline);
                c.Rect(bodyX - 2, armY, 1, armH, shirt);
                c.Rect(bodyX - 2, armY - 1, 1, 2, skin);
                c.Rect(bodyX + bodyW, armY - 1, 2, armH + 2, Outline);
                c.Rect(bodyX + bodyW + 1, armY, 1, armH, shirt);
                c.Rect(bodyX + bodyW + 1, armY - 1, 1, 2, skin);
            }

            // neck
            c.Rect(7, 16 + bob, 3, 2, Outline);
            c.Rect(7, 16 + bob, 2, 1, skin);

            // head
            float hx = 8f;
            float hy = 21f + bob;
            c.Ellipse(hx, hy, 5.7f, 5.3f, Outline);

            // hair mass
            if (style == 1)
            {
                c.Ellipse(hx, hy + 1.2f, 5.4f, 4.8f, hair);
                c.Rect(2, 12 + bob, 3, 8, hair);
                c.Rect(11, 12 + bob, 3, 8, hair);
                c.Rect(2, 12 + bob, 1, 8, Outline);
                c.Rect(13, 12 + bob, 1, 8, Outline);
                if (back)
                {
                    c.Rect(4, 14 + bob, 8, 9, hair);
                    c.Rect(4, 14 + bob, 8, 1, Outline);
                }
            }
            else if (style == 2)
            {
                c.Ellipse(hx, hy + 1.1f, 5.3f, 4.6f, hair);
                c.Disc(8f, hy + 5f, 2.4f, hair);
                c.Disc(8f, hy + 5f, 1.4f, hairLight);
                c.Rect(3, 18 + bob, 1, 4, hair);
                c.Rect(12, 18 + bob, 1, 4, hair);
                if (back) c.Rect(4, 15 + bob, 8, 9, hair);
            }
            else if (style == 4)
            {
                // short grey hair under a flat cap
                c.Ellipse(hx, hy + 1.1f, 5.2f, 4.5f, hair);
                c.Rect(3, 19 + bob, 2, 3, hair);
                c.Rect(11, 19 + bob, 2, 3, hair);
                if (back) c.Rect(4, 17 + bob, 8, 6, hair);
                c.Rect(2, 24 + bob, 12, 3, pants);
                c.Rect(3, 27 + bob, 10, 1, pants);
                c.Rect(2, 23 + bob, 12, 1, PixelArt.Hex("2b2233"));
            }
            else if (style == 5)
            {
                // silver hair gathered into a bun
                c.Ellipse(hx, hy + 1.1f, 5.2f, 4.5f, hair);
                c.Disc(8f, hy + 5.2f, 2.6f, hair);
                c.Disc(8f, hy + 5.2f, 1.6f, hairLight);
                c.Rect(2, 12 + bob, 3, 8, hair);
                c.Rect(11, 12 + bob, 3, 8, hair);
                c.Rect(2, 12 + bob, 1, 8, Outline);
                c.Rect(13, 12 + bob, 1, 8, Outline);
                if (back) c.Rect(4, 14 + bob, 8, 9, hair);
            }
            else
            {
                c.Ellipse(hx, hy + 1.1f, 5.2f, 4.5f, hair);
                c.Rect(3, 19 + bob, 2, 3, hair);
                c.Rect(11, 19 + bob, 2, 3, hair);
                if (back) c.Rect(4, 17 + bob, 8, 6, hair);
            }

            // hair highlight
            c.Rect(5, 24 + bob, 3, 1, hairLight);
            if (!back) c.Rect(10, 23 + bob, 2, 1, hairLight);

            // face
            if (!back)
            {
                float fx = side ? 7.3f : 8f;
                c.Ellipse(fx, hy - 0.8f, 4.15f, 3.9f, skin);
                // bangs
                c.Rect(4, 23 + bob, 8, 2, hair);
                c.Set(6, 22 + bob, hair);
                c.Set(9, 22 + bob, hair);
                c.Set(11, 21 + bob, hair);
                if (side) c.Rect(10, 20 + bob, 3, 4, hair);
            }
            else
            {
                c.Ellipse(hx, hy - 0.6f, 4.3f, 4.0f, hair);
                c.Rect(5, 18 + bob, 6, 2, hairLight);
            }

            // eyes / blush / mouth
            if (!back)
            {
                if (side)
                {
                    int ex = dir == 3 ? 11 : 5;
                    c.Rect(ex, 20 + bob, 1, 2, eye);
                    if (dir == 3) c.Set(12, 19 + bob, blush);
                    else c.Set(4, 19 + bob, blush);
                    c.Set(dir == 3 ? 13 : 3, 19 + bob, skin);
                }
                else
                {
                    c.Rect(6, 20 + bob, 1, 2, eye);
                    c.Rect(10, 20 + bob, 1, 2, eye);
                    c.Set(5, 19 + bob, blush);
                    c.Set(11, 19 + bob, blush);
                    c.Rect(7, 18 + bob, 2, 1, PixelArt.Hex("c98383"));

                    if (style == 4)
                    {
                        // grey moustache and beard
                        c.Rect(5, 17 + bob, 6, 2, hairLight);
                        c.Rect(6, 16 + bob, 4, 1, hair);
                        c.Set(7, 18 + bob, hair);
                        c.Set(8, 18 + bob, hair);
                    }
                    else if (style == 5)
                    {
                        // small round spectacles
                        c.Rect(5, 21 + bob, 3, 1, Outline);
                        c.Rect(9, 21 + bob, 3, 1, Outline);
                        c.Set(8, 21 + bob, Outline);
                        c.Set(5, 20 + bob, Outline);
                        c.Set(7, 20 + bob, Outline);
                        c.Set(9, 20 + bob, Outline);
                        c.Set(11, 20 + bob, Outline);
                    }
                }
            }

            return dir == 3 ? Mirror(c) : c;
        }

        static PixelCanvas Mirror(PixelCanvas src)
        {
            var dst = new PixelCanvas(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    dst.Set(src.Width - 1 - x, y, src.Get(x, y));
            return dst;
        }
    }
}
