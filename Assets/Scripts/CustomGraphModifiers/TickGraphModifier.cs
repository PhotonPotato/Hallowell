using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class TickGraphModifier : GraphModifier
{
    [Header("Refs")]
    public Tilemap worldColTiles;

    [Header("Settings")]
    public int maxVertJumpLen = 5;

    // Indicies that have been marked to set as not walkable
    private List<int> dirtyNodeIndices = new List<int>();

    public static readonly Dictionary<(GraphNode from, GraphNode to), TickGraphConnectionType> SpecialConnections = new Dictionary<(GraphNode, GraphNode), TickGraphConnectionType>();

    public override void OnPostScan()
    {
        GridGraph grid = AstarPath.active.data.gridGraph;
        if (grid == null) return;

        // Pass to flag invalid tick position tiles in the graph
        for (int y = 0; y < grid.depth; y++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                GridNode curNode = grid.nodes[y * grid.width + x];

                // Only check the node if its already marked as walkable
                if (curNode.Walkable)
                {
                    if (!IsTileWalkable(grid, new Vector2Int(x, y)))
                    {
                        // Flag it to be removed
                        dirtyNodeIndices.Add(y * grid.width + x);
                    }
                }
            }
        }

        // Before we set stu

        // Pass to actually remove the flagged nodes
        foreach (var index in dirtyNodeIndices)
        {
            grid.nodes[index].Walkable = false;
        }

        // Pass to rebuild node connections
        for (int y = 0; y < grid.depth; y++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                grid.CalculateConnections(x, y);

                AddTickJumpConnectionsForPos(grid, new Vector2Int(x, y));
            }
        }

        dirtyNodeIndices.Clear();
    }

    /// <summary>
    /// Checks to see if the given tile is walkable for the tick
    /// </summary>
    private bool IsTileWalkable(GridGraph grid, Vector2Int pos)
    {
        // If any of the surrounding tiles are solid, then the tick can cling here
        return IsSolid(grid, pos + Vector2Int.up) ||
               IsSolid(grid, pos + new Vector2Int(-1, 1)) ||
               IsSolid(grid, pos + new Vector2Int(1, 1)) ||
               IsSolid(grid, pos + Vector2Int.down) ||
               IsSolid(grid, pos + new Vector2Int(-1, -1)) ||
               IsSolid(grid, pos + new Vector2Int(1, -1)) ||
               IsSolid(grid, pos + Vector2Int.right) ||
               IsSolid(grid, pos + Vector2Int.left);
    }

    /// <summary>
    /// Returns whether the node at a specified position within a specified grid is Walkable
    /// </summary>
    private bool IsSolid(GridGraph grid, Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= grid.width || pos.y >= grid.depth) return false;

        return !grid.nodes[pos.y * grid.width + pos.x].Walkable;
    }

    private void AddTickJumpConnectionsForPos(GridGraph grid, Vector2Int pos)
    {
        GridNode cur = grid.nodes[pos.y * grid.width + pos.x];

        if (!cur.Walkable) return;

        // Make sure its not just a wall (check for non-walkable tile above & below)
        if (!IsSolid(grid, pos + Vector2Int.up) || !IsSolid(grid, pos + Vector2Int.down)) return;

        for (int dy = -maxVertJumpLen; dy <= maxVertJumpLen; dy++)
        {
            int newY = pos.y + dy;

            if (newY >= grid.depth || newY < 0 || Mathf.Abs(dy) <= 1) continue;

            GridNode scanNode = grid.nodes[newY * grid.width + pos.x];

            if (scanNode.Walkable)
            {
                uint cost = (uint)(Mathf.Abs(dy) * 1000);

                cur.AddConnection(scanNode, cost);

                SpecialConnections.Add((cur, scanNode), dy > 0 ? TickGraphConnectionType.JumpUp : TickGraphConnectionType.DropDown);

                Debug.Log("Connection Made!");
            }
        }
    }
}
