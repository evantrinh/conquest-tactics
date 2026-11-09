using System.Collections.Generic;
using UnityEngine;

public static class MovementRange
{
    static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    // Returns every reachable tile with the movement points spent to reach it.
    // A climb of more than 1 level is blocked, and each step uphill costs extra.
    public static Dictionary<Vector2Int, int> GetReachable(
        IGrid grid, Vector2Int start, int movePoints, HashSet<Vector2Int> blocked = null) =>
        GetReachable(grid, start, movePoints, blocked, out _);

    // Also returns, for each reached tile, the tile it was reached from
    public static Dictionary<Vector2Int, int> GetReachable(
        IGrid grid, Vector2Int start, int movePoints, HashSet<Vector2Int> blocked,
        out Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var cost = new Dictionary<Vector2Int, int> { [start] = 0 };
        var frontier = new List<Vector2Int> { start };

        while (frontier.Count > 0)
        {
            int best = 0;  // take the cheapest tile first
            for (int i = 1; i < frontier.Count; i++)
                if (cost[frontier[i]] < cost[frontier[best]]) best = i;
            var current = frontier[best];
            frontier.RemoveAt(best);

            foreach (var d in Dirs)
            {
                var next = current + d;
                if (!grid.InBounds(next) || !grid.IsWalkable(next)) continue;
                if (blocked != null && blocked.Contains(next)) continue;

                int climb = grid.GetHeight(next) - grid.GetHeight(current);
                if (climb > 1) continue;
                int step = 1 + Mathf.Max(0, climb);

                int total = cost[current] + step;
                if (total > movePoints) continue;
                if (!cost.ContainsKey(next) || total < cost[next])
                {
                    cost[next] = total;
                    cameFrom[next] = current;
                    frontier.Add(next);
                }
            }
        }
        return cost;
    }
}
