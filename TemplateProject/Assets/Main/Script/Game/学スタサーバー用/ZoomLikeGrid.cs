using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zoomのようにグリッド画面を作ってみる
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
public class ZoomLikeGrid : MonoBehaviour
{
    public RectTransform container;
    public int participantCount = 1;

    public float spacing = 8f;

    private GridLayoutGroup grid;

    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        Refresh();
    }

    /// <summary>
    /// 適当に
    /// </summary>
    public void Refresh()
    {
        float width = container.rect.width;
        float height = container.rect.height;

        int bestRows = 1;
        int bestCols = 1;
        float bestSize = 0;

        // 全パターン試す
        for (int rows = 1; rows <= participantCount; rows++)
        {
            int cols = Mathf.CeilToInt((float)participantCount / rows);

            float cellWidth =
                (width - spacing * (cols - 1)) / cols;

            float cellHeight =
                (height - spacing * (rows - 1)) / rows;

            // Zoomっぽく16:9維持
            float sizeByWidth = cellWidth;
            float sizeByHeight = cellHeight * (16f / 9f);

            float finalWidth = Mathf.Min(sizeByWidth, sizeByHeight);

            if (finalWidth > bestSize)
            {
                bestSize = finalWidth;
                bestRows = rows;
                bestCols = cols;
            }
        }

        float finalCellWidth =
            (width - spacing * (bestCols - 1)) / bestCols;

        float finalCellHeight =
            finalCellWidth * (9f / 16f);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = bestCols;

        grid.cellSize = new Vector2(finalCellWidth, finalCellHeight);
        grid.spacing = new Vector2(spacing, spacing);

        Debug.Log($"Rows:{bestRows} Cols:{bestCols}");
    }
}