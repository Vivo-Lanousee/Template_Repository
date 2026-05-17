using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Student
{
    /// <summary>
    /// 適当にスクリーンショットを取る技術
    /// </summary>
    public class CanvasCapture : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        private readonly string capturePath = "Screenshots";
        public void Awake()
        {

            button.onClick.AddListener(()=>Capture());
        }

        /// <summary>
        /// セーブファイル有効化
        /// </summary>
        public void ActiveSaveFile ()
        {
            if (!Directory.Exists(Application.dataPath+"/"+capturePath))
            {
                Debug.Log("フォルダがない。");
                Directory.CreateDirectory(Application.dataPath + "/" + capturePath);
            }
        }


        public void Capture()
        {
            ActiveSaveFile();   

            var fileName = $"{capturePath}/screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

            // 保存先
            var path = Path.Combine(Application.dataPath, fileName);

            ScreenCapture.CaptureScreenshot(path);

            Debug.Log($"保存した: {path}");
        }
        /// <summary>
        /// スクリーンショット削除
        /// </summary>
        public void DeleteScreenshot()
        {

        }
    }
}