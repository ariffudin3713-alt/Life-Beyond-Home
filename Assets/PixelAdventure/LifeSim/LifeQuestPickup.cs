using UnityEngine;

namespace PixelAdventure.LifeSim
{
    /// <summary>
    /// Barang misi yang tergeletak di dunia: ikon barangnya melayang naik-turun di atas cahaya emas
    /// yang berdenyut, dengan pilar cahaya, gelembung "!", dan nama barang. Efeknya digambar di atas
    /// lapisan malam agar tetap terlihat jelas saat gelap.
    /// </summary>
    public sealed class LifeQuestPickup : MonoBehaviour
    {
        SpriteRenderer _icon, _glow, _beam, _bang, _label;
        float _t;

        public static LifeQuestPickup Create(Transform parent, string itemId, string label, Vector2 pos)
        {
            var go = new GameObject("BarangMisi_" + itemId);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            var p = go.AddComponent<LifeQuestPickup>();
            p._t = Random.value * 3f;

            string icon = PixelArt.Sprites.ContainsKey("lc_qi_" + itemId) ? "lc_qi_" + itemId : "lc_item_quest";
            p._glow = Part(go.transform, "lc_quest_glow", new Vector3(0f, 0.05f, 0f), LifeWorld.LightOrder);
            p._beam = Part(go.transform, "lc_quest_beam", Vector3.zero, LifeWorld.LightOrder);
            p._icon = Part(go.transform, icon, new Vector3(0f, 0.75f, 0f), LifeWorld.LightOrder + 2);
            p._icon.transform.localScale = Vector3.one * 1.5f;
            p._bang = Part(go.transform, "lc_quest_bang", new Vector3(0f, 1.55f, 0f), LifeWorld.LightOrder + 3);

            var tag = new GameObject("Nama");
            tag.transform.SetParent(go.transform, false);
            tag.transform.localPosition = new Vector3(0f, 2.65f, 0f);
            p._label = tag.AddComponent<SpriteRenderer>();
            p._label.sprite = PixelFont.Render("BARANG MISI: " + label.ToUpperInvariant(),
                new Color(1f, 0.86f, 0.4f), new Color(0.08f, 0.06f, 0.12f), 1, new Vector2(0.5f, 0.5f));
            p._label.sharedMaterial = PixelArt.SpriteMaterial;
            p._label.sortingOrder = LifeWorld.LightOrder + 4;
            return p;
        }

        static SpriteRenderer Part(Transform parent, string sprite, Vector3 local, int order)
        {
            var go = new GameObject(sprite);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Get(sprite);
            sr.sharedMaterial = PixelArt.SpriteMaterial;
            sr.sortingOrder = order;
            return sr;
        }

        void Update()
        {
            bool visible = LifeView.Sees(transform.position, 2f, 3.5f);
            if (_icon.enabled != visible)
                _icon.enabled = _glow.enabled = _beam.enabled = _bang.enabled = _label.enabled = visible;
            if (!visible) return;

            _t += Time.unscaledDeltaTime;
            float bob = Mathf.Sin(_t * 3f) * 0.12f;
            _icon.transform.localPosition = new Vector3(0f, 0.75f + bob, 0f);
            _bang.transform.localPosition = new Vector3(0f, 1.55f + bob, 0f);
            float pulse = 0.5f + 0.5f * Mathf.Sin(_t * 4f);
            _glow.transform.localScale = Vector3.one * (1f + pulse * 0.25f);
            _glow.color = new Color(1f, 1f, 1f, 0.6f + pulse * 0.4f);
            _beam.color = new Color(1f, 1f, 1f, 0.45f + pulse * 0.35f);
        }
    }
}
