using System.Collections.Generic;
using UnityEngine;

// Draws diagonal hatched squares on top of tiles. Squares are pooled and reused.
// Move tiles are blue, attack tiles red, enemy move tiles gray, and a tile in both ranges alternates the two colours.
public class TileOverlay : MonoBehaviour
{
    public static TileOverlay Instance { get; private set; }

    public Color moveColor = new Color(0.4f, 0.85f, 1f, 0.55f);
    public Color attackColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    public Color enemyMoveColor = new Color(0.65f, 0.65f, 0.65f, 0.6f);

    [Tooltip("Optional. Leave empty to use Unity's built-in Sprites/Default shader.")]
    public Material overlayMaterial;

    readonly List<GameObject> pool = new List<GameObject>();
    int used;
    Material material;
    MaterialPropertyBlock block;
    Texture2D moveTex, enemyMoveTex, attackTex, bothTex, enemyBothTex;

    const int TexSize = 64;
    const int StripeWidth = 8;

    void Awake()
    {
        Instance = this;
        block = new MaterialPropertyBlock();
        material = overlayMaterial != null ? overlayMaterial : new Material(Shader.Find("Sprites/Default"));
        moveTex = Hatch(moveColor, Color.clear);
        enemyMoveTex = Hatch(enemyMoveColor, Color.clear);
        attackTex = Hatch(attackColor, Color.clear);
        bothTex = Hatch(moveColor, attackColor);
        enemyBothTex = Hatch(enemyMoveColor, attackColor);
    }

    // Diagonal stripes that line up across neighbouring tiles. A clear second colour leaves gaps between stripes.
    static Texture2D Hatch(Color a, Color b)
    {
        var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        var pixels = new Color[TexSize * TexSize];
        for (int y = 0; y < TexSize; y++)
        for (int x = 0; x < TexSize; x++)
            pixels[y * TexSize + x] = ((x + y) / StripeWidth) % 2 == 0 ? a : b;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void OnEnable() { GameEvents.OnLevelBuilt += OnLevelBuilt; }
    void OnDisable() { GameEvents.OnLevelBuilt -= OnLevelBuilt; }
    void OnLevelBuilt(Bounds _) { Clear(); }

    public void Show(IEnumerable<Vector2Int> moveTiles, IEnumerable<Vector2Int> attackTiles, bool enemy = false)
    {
        Clear();
        const int Move = 1, Attack = 2;
        var kinds = new Dictionary<Vector2Int, int>();
        foreach (var p in moveTiles) kinds[p] = Move;
        foreach (var p in attackTiles) kinds[p] = kinds.TryGetValue(p, out var k) ? k | Attack : Attack;

        foreach (var kv in kinds)
        {
            Texture2D tex;
            if (kv.Value == Move) tex = enemy ? enemyMoveTex : moveTex;
            else if (kv.Value == Attack) tex = attackTex;
            else tex = enemy ? enemyBothTex : bothTex;
            Add(kv.Key, tex, kv.Value == Attack ? 0.02f : 0.04f);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < used; i++) pool[i].SetActive(false);
        used = 0;
    }

    void Add(Vector2Int p, Texture2D tex, float lift)
    {
        GameObject quad;
        if (used < pool.Count) quad = pool[used];
        else
        {
            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<Collider>());   // overlays must never block mouse clicks
            quad.name = "OverlayTile";
            quad.transform.SetParent(transform, false);
            quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = Vector3.one;
            var r = quad.GetComponent<Renderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            pool.Add(quad);
        }
        used++;

        quad.SetActive(true);
        quad.transform.position = GridManager.Instance.TileToWorld(p) + Vector3.up * lift;

        block.SetTexture("_MainTex", tex);
        block.SetColor("_Color", Color.white);
        quad.GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
