using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilemapType : MonoBehaviour
{
    public Tilemap tilemap; // Inspectorで割り当て
    public GameObject core;
    public float startX = 54.0f;

    void Start()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap が割り当てられていません。");
            return;
        }

        // タイルマップ全体の範囲を取得
        BoundsInt bounds = tilemap.cellBounds;

        // 各セルを走査
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // タイルが存在するかチェック
                if (tilemap.HasTile(cellPosition))
                {
                    // ワールド座標に変換（必要なら）
                    Vector3 worldPos = tilemap.CellToWorld(cellPosition);

                    Debug.Log($"タイル座標: {cellPosition}, ワールド座標: {worldPos}");
                    GameObject obj = Instantiate(core, worldPos + new Vector3(startX + 0.5f, 0.5f, 0), Quaternion.identity);
                    obj.transform.parent = tilemap.transform;
                }
            }
        }
    }
}