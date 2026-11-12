using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Outlines a skill's area. A glowing line follows the edge of the area along the terrain, a soft wall fades
// up into the air and down the side of the terrain from that line, and the tiles inside glow faintly.
public class SkillEffectView : MonoBehaviour
{
    public float duration = 0.9f;
    public float outlineWidth = 0.14f;
    public float outlineAlpha = 1f;
    public float wallUp = 0.9f;
    public float wallDown = 0.9f;
    public float wallAlpha = 0.45f;
    public float fillAlpha = 0.18f;

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
        var material = new Material(Shader.Find("Sprites/Default"));
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
        var wall = new Color(color.r, color.g, color.b, wallAlpha);

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

                // Wall that is strongest at the line and fades both up and down
                var up = Vector3.up * wallUp;
                var down = Vector3.down * wallDown;
                AddQuad(vertices, colors, triangles,
                    edge - along, edge + along, edge + along + up, edge - along + up,
                    wall, wall, clear, clear);
                AddQuad(vertices, colors, triangles,
                    edge - along + down, edge + along + down, edge + along, edge - along,
                    clear, clear, wall, wall);
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
