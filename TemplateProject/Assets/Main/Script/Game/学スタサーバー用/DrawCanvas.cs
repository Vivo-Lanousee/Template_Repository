using System.Linq;
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
        [SerializeField]
        private Shader _drawShader;
        [SerializeField]
        private RawImage _rawImage;

        [SerializeField]
        private float _radius = 0.1f;
        [SerializeField]
        private Color _color = UnityEngine.Color.black;
        private RenderTexture _texture;
        private Material _drawMaterial;

        private Vector2 _screenPointMax;
        private Vector2 _screenPointMin;

        private Vector2 _lastPosition;
        private void Start()
        {
            var rectTransform = transform as RectTransform;
            _texture = new RenderTexture(
                (int)rectTransform.rect.width,
                (int)rectTransform.rect.height,
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

            var canvas = transform.GetComponentInParent<Canvas>().rootCanvas;
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            var screenPoints = new Vector3[4];
            for (var i = 0; i < corners.Length; i++)
            {
                var screenPoint = UnityEngine.RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[i]);
                screenPoints[i] = screenPoint;
            }
            _screenPointMax = new Vector2(
                screenPoints.Max(v => v.x),
                screenPoints.Max(v => v.y)
            );

            _screenPointMin = new Vector2(
                screenPoints.Min(v => v.x),
                screenPoints.Min(v => v.y)
            );
            _rawImage.texture = _texture;
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
        public void OnBeginDrag(PointerEventData eventData)
        {
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
            // 一定以上離れていたら補完
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
        public void Clear()
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = _texture;
            GL.Clear(true, true, UnityEngine.Color.clear);
            RenderTexture.active = currentRT;
        }
    }
}