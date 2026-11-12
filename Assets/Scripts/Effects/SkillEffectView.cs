using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Outlines a skill's area. A glowing line follows the edge of the area along the terrain, a soft wall fades
// up into the air and down the cliff beside it where the terrain steps down, and the tiles inside glow faintly.
public class SkillEffectView : MonoBehaviour
{
    public float duration = 0.9f;
    public float outlineWidth = 0.14f;
    public float outlineAlpha = 1f;
    public float wallUp = 0.5f;
    public float wallDown = 0.5f;
    public float wallStrength = 0.35f;
    public float fillAlpha = 0.18f;

    [Tooltip("Optional. Leave empty to build the material from the shader by name.")]
    public Material effectMaterial;

    static readonly Vector2Int[] Dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    void OnEnable() { GameEvents.OnSkillUsed += Play; }
    void OnDisable() { GameEvents.OnSkillUsed -= Play; }

    void Play(Unit caster, SkillData skill, List<Vector2Int> tiles)
    {
        StartCoroutine(Run(skill.effectColor, tiles));
    }

    IEnumerator Run(Color color, List<Vector2Int> tiles)
    {
        // Left at the scene root. The mesh is built in world space, so a parent that is moved would shift it.
        var go = new GameObject("SkillEffect");
        var mesh = BuildMesh(color, tiles);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        var material = effectMaterial != null ? new Material(effectMaterial) : new Material(Shader.Find("Contour/Skill Effect"));
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // Fades in quickly, holds, then fades out
            float envelope = Mathf.Sin(Mathf.PI * t / duration);
            material.color = new Color(1f, 1f, 1f, Mathf.Clamp01(envelope * 1.6f));
            yield return null;
        }

        Destroy(mesh);
        Destroy(material);
        Destroy(go);
    }

    Mesh BuildMesh(Color color, List<Vector2Int> tiles)
    {
        var grid = GridManager.Instance;
        var set = new HashSet<Vector2Int>(tiles);
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        var clear = new Color(color.r, color.g, color.b, 0f);
        var fill = new Color(color.r, color.g, color.b, fillAlpha);
        var line = new Color(color.r, color.g, color.b, outlineAlpha);
        var wall = new Color(color.r, color.g, color.b, wallStrength);

        foreach (var p in tiles)
        {
            // Just above the tile surface, so it sits over the range overlay
            var c = grid.TileToWorld(p) + Vector3.up * 0.06f;

            AddQuad(vertices, colors, triangles,
                c + new Vector3(-0.5f, 0, -0.5f), c + new Vector3(0.5f, 0, -0.5f),
                c + new Vector3(0.5f, 0, 0.5f), c + new Vector3(-0.5f, 0, 0.5f),
                fill, fill, fill, fill);

            foreach (var d in Dirs)
            {
                if (set.Contains(p + d)) continue;

                var outward = new Vector3(d.x, 0, d.y);
                var along = new Vector3(d.y, 0, d.x) * 0.5f;
                var edge = c + outward * 0.5f;

                // Glowing line on the terrain, bright on the edge and fading inwards
                var inward = -outward * outlineWidth;
                AddQuad(vertices, colors, triangles,
                    edge - along, edge + along, edge + along + inward, edge - along + inward,
                    line, line, clear, clear);

                // Where the terrain steps beside this edge, the outline climbs the side of the taller tile
                foreach (var n in new[] { new Vector2Int(d.y, d.x), new Vector2Int(-d.y, -d.x) })
                {
                    var q = p + n;
                    if (!set.Contains(q) || set.Contains(q + d)) continue;

                    float step = c.y - (grid.TileToWorld(q).y + 0.06f);
                    if (step <= 0.01f) continue;

                    var corner = c + new Vector3(n.x, 0, n.y) * 0.5f + outward * 0.5f;
                    var drop = Vector3.down * step;
                    AddQuad(vertices, colors, triangles,
                        corner + drop, corner, corner + inward, corner + inward + drop,
                        line, line, clear, clear);
                }

                // Wall that is strongest at the line and fades up. It also fades down, but only where the
                // terrain steps down next to the area, and never lower than the tile on the other side.
                // At the edge of the map and beside taller tiles it starts on top of the tile.
                float surface = c.y - 0.06f;
                float downLength = 0f;
                if (grid.InBounds(p + d))
                    downLength = Mathf.Min(wallDown, surface - grid.TileToWorld(p + d).y);
                var up = Vector3.up * wallUp;
                AddQuad(vertices, colors, triangles,
                    edge - along, edge + along, edge + along + up, edge - along + up,
                    wall, wall, clear, clear);

                if (downLength > 0.01f)
                {
                    var down = Vector3.down * downLength;
                    var bottom = Color.Lerp(wall, clear, downLength / wallDown);
                    AddQuad(vertices, colors, triangles,
                        edge - along + down, edge + along + down, edge + along, edge - along,
                        bottom, bottom, wall, wall);
                }
            }
        }

        var mesh = new Mesh { name = "SkillEffect" };
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    static void AddQuad(List<Vector3> vertices, List<Color> colors, List<int> triangles,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color ca, Color cb, Color cc, Color cd)
    {
        int i = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
        colors.Add(ca); colors.Add(cb); colors.Add(cc); colors.Add(cd);
        triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
        triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
    }
}
