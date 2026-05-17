using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Student
{
    /// <summary>
    /// 適当なキャンパスの作成
    /// </summary>
    public class DrawCanvas : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private static readonly int MainTex = Shader.PropertyToID("_SourceTex");
        private static readonly int Coordinate = Shader.PropertyToID("_Coordinate");
        private static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
        private static readonly int Color = Shader.PropertyToID("_Color");

        private readonly string capturePath = "Screenshots";


        [SerializeField] private Shader _drawShader;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private float _radius = 0.1f;
        [SerializeField] private Color _color = UnityEngine.Color.black;

        private RenderTexture _texture;
        private Material _drawMaterial;

        private Vector2 _screenPointMax;
        private Vector2 _screenPointMin;
        private Vector2 _lastPosition;

        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        [SerializeField] private Button resetButton;
        [SerializeField] private Button endButton;
        private void Start()
        {
            resetButton.onClick.AddListener(() => Clear());
            endButton.onClick.AddListener(() => SaveAsSprite());


            _rectTransform = transform as RectTransform;
            _parentCanvas = transform.GetComponentInParent<Canvas>().rootCanvas;

            _texture = new RenderTexture(
                (int)_rectTransform.rect.width,
                (int)_rectTransform.rect.height,
                0,
                RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                enableRandomWrite = true
            };
            _texture.Create();

            _drawMaterial = new Material(_drawShader);
            _drawMaterial.SetTexture(MainTex, _texture);
            _drawMaterial.SetVector(TextureSize, new Vector4(_texture.width, _texture.height, 0, 0));
            _drawMaterial.SetColor(Color, _color);

            _rawImage.texture = _texture;

            // 初回クリア（初期状態を透明に）
            Clear();
        }

        private void OnDestroy()
        {
            _rawImage.texture = null;
            if (_texture != null)
            {
                _texture.Release();
                Destroy(_texture);
                _texture = null;
            }
            if (_drawMaterial != null)
            {
                Destroy(_drawMaterial);
                _drawMaterial = null;
            }
        }

        /// <summary>
        /// 最新の画面座標範囲を更新する（解像度変更やUIズレ対策）
        /// </summary>
        private void UpdateScreenBounds()
        {
            var corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            var screenPoints = new Vector3[4];
            for (var i = 0; i < corners.Length; i++)
            {
                screenPoints[i] = RectTransformUtility.WorldToScreenPoint(_parentCanvas.worldCamera, corners[i]);
            }

            _screenPointMax = new Vector2(screenPoints.Max(v => v.x), screenPoints.Max(v => v.y));
            _screenPointMin = new Vector2(screenPoints.Min(v => v.x), screenPoints.Min(v => v.y));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UpdateScreenBounds(); // ドラッグ開始時に最新の枠組みを計算
            Draw(eventData.position);
            _lastPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            DrawInterpolate(eventData.position);
            _lastPosition = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        private void DrawInterpolate(Vector2 position)
        {
            Draw(position);
            var delta = position - _lastPosition;
            var distance = delta.magnitude;
            const float step = 10f;

            if (distance > step)
            {
                var count = (int)(distance / step);
                for (var i = 0; i < count; i++)
                {
                    var t = (float)i / count;
                    var interpolatePosition = Vector2.Lerp(_lastPosition, position, t);
                    Draw(interpolatePosition);
                }
            }
        }

        private void Draw(Vector2 screenPosition)
        {
            var u = Mathf.InverseLerp(_screenPointMin.x, _screenPointMax.x, screenPosition.x);
            var v = Mathf.InverseLerp(_screenPointMin.y, _screenPointMax.y, screenPosition.y);

            _drawMaterial.SetVector(Coordinate, new Vector4(u, v, _radius, 0));
            var temp = RenderTexture.GetTemporary(_texture.width, _texture.height, 0, _texture.format);
            Graphics.Blit(_texture, temp, _drawMaterial);
            Graphics.Blit(temp, _texture);
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// キャンバスに描かれた内容をすべて消去
        /// </summary>
        public void Clear()
        {
            if (_texture == null) return;

            var temp = RenderTexture.GetTemporary(_texture.width, _texture.height, 0, _texture.format);

            RenderTexture.active = temp;
            GL.Clear(true, true, UnityEngine.Color.clear); 

            Graphics.Blit(temp, _texture); 

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// 現在描かれている内容を新しい Sprite として取得します。
        /// </summary>
        public void SaveAsSprite()
        {

            // RenderTextureの中身を読み取るための Texture2D を生成
            Texture2D tex2D = new Texture2D(_texture.width, _texture.height, TextureFormat.ARGB32, false);

            // アクティブなRenderTextureを切り替えてピクセルをコピー
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = _texture;

            tex2D.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0);
            tex2D.Apply();

            RenderTexture.active = previousRT;

            // Pivotは中央 (0.5, 0.5) に設定しています
            Sprite generatedSprite = Sprite.Create(
                tex2D,
                new Rect(0, 0, tex2D.width, tex2D.height),
                new Vector2(0.5f, 0.5f)
            );

            Save(generatedSprite);
        }

        /// <summary>
        /// セーブファイル有効化
        /// </summary>
        public void ActiveSaveFile()
        {
            if (!Directory.Exists(Application.dataPath + "/" + capturePath))
            {
                Debug.Log("フォルダがない。");
                Directory.CreateDirectory(Application.dataPath + "/" + capturePath);
            }
        }
        /// <summary>
        /// 実際に保存
        /// </summary>
        public void Save(Sprite _sprite)
        {
            if (_sprite == null || _sprite.texture == null)
            {
                Debug.LogError("保存するSprite、またはTextureが空です。");
                return;
            }

            // 保存先フォルダの存在チェックと生成
            ActiveSaveFile();

            // ファイル名の生成
            var fileName = $"screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

            // 実際のフルパス（Application.dataPath + /Screenshots/ + ファイル名）
            var path = Path.Combine(Application.dataPath, capturePath, fileName);

            // --------------------------------------------------
            // 【重要】SpriteからTexture2Dを取り出してPNGバイトデータに変換
            // --------------------------------------------------
            Texture2D tex = _sprite.texture;

            // Texture2DをPNG形式のバイナリ(byte配列)に変換するUnityの標準機能
            byte[] bytes = tex.EncodeToPNG();

            // ディスクにファイルを書き出し
            File.WriteAllBytes(path, bytes);

            Debug.Log($"PNG画像を保存しました: {path}");
        }
    }
}