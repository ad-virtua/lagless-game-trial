using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilemapType : MonoBehaviour
{
    private Tilemap tilemap; // Inspectorで割り当て
    public GameObject core;
    public float startX = 54.0f;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();

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

                    if (SceneManager.instance.sceneType == SceneManager.SceneType.Stage1)
                    {
                        GameObject obj = Instantiate(core, worldPos + new Vector3(startX + 0.5f, 0.5f, 0), Quaternion.identity);
                        obj.transform.parent = tilemap.transform;
                    }
                    if (SceneManager.instance.sceneType == SceneManager.SceneType.Stage2)
                    {
                        GameObject obj = Instantiate(core, worldPos + new Vector3(startX + 0.3f, 0.35f, 0), Quaternion.identity);
                        obj.transform.parent = tilemap.transform;
                    }
                }
            }
        }
    }
}