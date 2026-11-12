using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shows a skill's area: a glow on the tiles and a see-through wall that rises from the outline and fades into the air.
public class SkillEffectView : MonoBehaviour
{
    public float duration = 0.9f;
    public float wallHeight = 1.4f;
    public float glowAlpha = 0.5f;
    public float wallAlpha = 0.85f;

    static readonly Vector2Int[] Dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    void OnEnable() { GameEvents.OnSkillUsed += Play; }
    void OnDisable() { GameEvents.OnSkillUsed -= Play; }

    void Play(Unit caster, SkillData skill, List<Vector2Int> tiles)
    {
        StartCoroutine(Run(skill.effectColor, tiles));
    }

    IEnumerator Run(Color color, List<Vector2Int> tiles)
    {
        var go = new GameObject("SkillEffect");
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = BuildMesh(color, tiles);
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

        Destroy(go.GetComponent<MeshFilter>().sharedMesh);
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

        var edgeColor = new Color(color.r, color.g, color.b, glowAlpha * 0.6f);
        var centerColor = new Color(color.r, color.g, color.b, glowAlpha);
        var wallBottom = new Color(color.r, color.g, color.b, wallAlpha);
        var wallTop = new Color(color.r, color.g, color.b, 0f);

        foreach (var p in tiles)
        {
            var c = grid.TileToWorld(p) + Vector3.up * 0.06f;

            // Tile glow: brighter in the middle
            int b = vertices.Count;
            vertices.Add(c);
            colors.Add(centerColor);
            vertices.Add(c + new Vector3(-0.5f, 0, -0.5f));
            vertices.Add(c + new Vector3(0.5f, 0, -0.5f));
            vertices.Add(c + new Vector3(0.5f, 0, 0.5f));
            vertices.Add(c + new Vector3(-0.5f, 0, 0.5f));
            for (int i = 0; i < 4; i++) colors.Add(edgeColor);
            for (int i = 0; i < 4; i++)
            {
                triangles.Add(b);
                triangles.Add(b + 1 + i);
                triangles.Add(b + 1 + (i + 1) % 4);
            }

            // Wall on every side that faces outside the area
            foreach (var d in Dirs)
            {
                if (set.Contains(p + d)) continue;
                var along = new Vector3(d.y, 0, d.x) * 0.5f;      // half an edge, sideways
                var edge = c + new Vector3(d.x, 0, d.y) * 0.5f;   // middle of that edge
                var up = Vector3.up * wallHeight;

                int w = vertices.Count;
                vertices.Add(edge - along);
                vertices.Add(edge + along);
                vertices.Add(edge + along + up);
                vertices.Add(edge - along + up);
                colors.Add(wallBottom);
                colors.Add(wallBottom);
                colors.Add(wallTop);
                colors.Add(wallTop);
                triangles.AddRange(new[] { w, w + 1, w + 2, w, w + 2, w + 3 });
            }
        }

        var mesh = new Mesh { name = "SkillEffect" };
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
