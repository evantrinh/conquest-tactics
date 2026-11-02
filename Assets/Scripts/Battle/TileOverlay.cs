using System.Collections.Generic;
using UnityEngine;

// Draws coloured squares on top of tiles. Squares are pooled and reused.
public class TileOverlay : MonoBehaviour
{
    public static TileOverlay Instance { get; private set; }

    public Color moveColor = new Color(0.4f, 0.85f, 1f, 0.55f);
    public Color attackColor = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Tooltip("Optional. Leave empty to use Unity's built-in Sprites/Default shader.")]
    public Material overlayMaterial;

    readonly List<GameObject> pool = new List<GameObject>();
    int used;
    Material material;
    MaterialPropertyBlock block;

    void Awake()
    {
        Instance = this;
        block = new MaterialPropertyBlock();
        material = overlayMaterial != null ? overlayMaterial : new Material(Shader.Find("Sprites/Default"));
    }

    void OnEnable() { GameEvents.OnLevelBuilt += OnLevelBuilt; }
    void OnDisable() { GameEvents.OnLevelBuilt -= OnLevelBuilt; }
    void OnLevelBuilt(Bounds _) { Clear(); }

    public void Show(IEnumerable<Vector2Int> moveTiles, IEnumerable<Vector2Int> attackTiles)
    {
        Clear();
        foreach (var p in attackTiles) Add(p, attackColor, 0.02f);
        foreach (var p in moveTiles) Add(p, moveColor, 0.04f);
    }

    public void Clear()
    {
        for (int i = 0; i < used; i++) pool[i].SetActive(false);
        used = 0;
    }

    void Add(Vector2Int p, Color color, float lift)
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
            quad.transform.localScale = new Vector3(0.92f, 0.92f, 1f);
            var r = quad.GetComponent<Renderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            pool.Add(quad);
        }
        used++;

        quad.SetActive(true);
        quad.transform.position = GridManager.Instance.TileToWorld(p) + Vector3.up * lift;

        block.SetColor("_Color", color);
        quad.GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
