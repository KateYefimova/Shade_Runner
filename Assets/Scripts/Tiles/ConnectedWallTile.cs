using UnityEngine;
using UnityEngine.Tilemaps;

public class ConnectedWallTile : TileBase
{
    public Sprite[] sprites = new Sprite[16];
    public Tile.ColliderType colliderType = Tile.ColliderType.Grid;

    private static readonly Vector3Int[] Neighbours =
    {
        Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left
    };

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        tilemap.RefreshTile(position);
        foreach (Vector3Int offset in Neighbours)
        {
            if (IsWall(tilemap, position + offset))
                tilemap.RefreshTile(position + offset);
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        int mask = 0;
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (IsWall(tilemap, position + Neighbours[i]))
                mask |= 1 << i;
        }

        tileData.sprite = mask < sprites.Length ? sprites[mask] : null;
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
        tileData.colliderType = colliderType;
    }

    private static bool IsWall(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) is ConnectedWallTile;
    }
}
