using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicVideoLayout : MonoBehaviour
{
    public RectTransform container;
    public GameObject userTilePrefab;
    public int userCount = 3;
    public float spacing = 20f;

    [SerializeField] private Sprite test;

    private List<GameObject> tiles = new List<GameObject>();

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        tiles.Clear();

        for (int i = 0; i < userCount; i++)
        {
            GameObject obj = Instantiate(userTilePrefab, container);
            obj.name = $"User_{i}";
            tiles.Add(obj);
        }

        Layout();
    }

    void Layout()
    {
        float width = container.rect.width;
        float height = container.rect.height;

        float currentAspectRatio = 16f / 9f; // デフォルト値（Spriteがない場合の保険）

        if (test != null)
        {
            // Spriteの幅 ÷ 高さで比率を割り出す
            currentAspectRatio = test.rect.width / test.rect.height;
        }

        int columns = Mathf.CeilToInt(Mathf.Sqrt(userCount));
        int rows = Mathf.CeilToInt((float)userCount / columns);

        // 1マスあたりの最大割り当て領域
        float cellWidth = (width - spacing * (columns + 1)) / columns;
        float cellHeight = (height - spacing * (rows + 1)) / rows;

        // Spriteの比率を維持したまま、マスに収まる最大サイズを計算
        float tileWidth = cellWidth;
        float tileHeight = cellWidth / currentAspectRatio;

        if (tileHeight > cellHeight)
        {
            // 高さが溢れる場合は、高さを基準に横幅を逆算
            tileHeight = cellHeight;
            tileWidth = cellHeight * currentAspectRatio;
        }

        // 全体の縦幅（全行の合計高さ ＋ 行間の隙間）
        float totalHeight = rows * tileHeight + (rows - 1) * spacing;

        for (int i = 0; i < tiles.Count; i++)
        {
            RectTransform rt = tiles[i].GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            int row = i / columns;
            int col = i % columns;

            int currentRowCount = Mathf.Min(columns, userCount - row * columns);

            // この行の横幅
            float currentRowWidth = currentRowCount * tileWidth + (currentRowCount - 1) * spacing;

            float startX = -currentRowWidth / 2f;
            float startY = totalHeight / 2f;

            // 各タイルの中心座標を計算
            float x = startX + col * (tileWidth + spacing) + tileWidth / 2f;
            float y = startY - row * (tileHeight + spacing) - tileHeight / 2f;

            rt.anchoredPosition = new Vector2(x, y);

            // 親のGameObject自体をSpriteと完全に同じ比率の長方形にする
            rt.sizeDelta = new Vector2(tileWidth, tileHeight);

            UpdateInnerUI(tiles[i], tileWidth, tileHeight);
        }
    }

    /// <summary>
    /// タイルのサイズに合わせて内部UIを調整
    /// </summary>
    void UpdateInnerUI(GameObject tile, float tileWidth, float tileHeight)
    {
        Image image = tile.GetComponentInChildren<Image>();
        TMP_Text text = tile.GetComponentInChildren<TMP_Text>();

        if (test != null && image.sprite == null)
        {
            image.sprite = test;
        }

        //  Imageの設定（親のサイズに完全にフィットさせる
        RectTransform imageRT = image.GetComponent<RectTransform>();
        imageRT.anchorMin = new Vector2(0.5f, 0.5f);
        imageRT.anchorMax = new Vector2(0.5f, 0.5f);
        imageRT.pivot = new Vector2(0.5f, 0.5f);


        float imgWidth = tileWidth;
        float imgHeight = tileHeight;

        imageRT.sizeDelta = new Vector2(imgWidth, imgHeight);
        imageRT.anchoredPosition = Vector2.zero;

        //  Textの設定（Imageの下端の内側に重ねる）
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.pivot = new Vector2(0.5f, 0f);

        // タイルの高さに基づいたフォントサイズ
        float fontSize = tileHeight * 0.12f;
        text.fontSize = fontSize;
        textRT.sizeDelta = new Vector2(imgWidth * 0.9f, fontSize * 1.3f);

        // 配置：Imageの下端から少し上にずらして重ねる
        float textPadding = 5f;
        float textTargetY = -(imgHeight / 2f) + textPadding;
        textRT.anchoredPosition = new Vector2(0f, textTargetY);
    }
}