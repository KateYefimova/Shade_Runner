using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapCollider2D))]
[DefaultExecutionOrder(-200)]
public class SolidTilemap : MonoBehaviour
{
    private void Awake()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.CompressBounds();

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(cell))
                tilemap.SetColliderType(cell, Tile.ColliderType.Grid);
        }

        GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        Physics2D.SyncTransforms();
    }
}
