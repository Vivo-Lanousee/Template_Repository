using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Common
{
    /// <summary>
    /// キーバインド設定について
    /// </summary>

    public class KeyConfig
    {
        public void Initialize()
        {
            //初期化時
            this.actions = InputSystemManager.Instance().inputActions;
        }

        InputSystem_Actions actions;
        //キーコンフィグのマジックナンバー
        private　readonly string pathKeyConfig = "/KeyConfig.json";

        /// <summary>
        /// ゲーム起動時
        /// </summary>

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void KeyConfigInitialize()
        {
            KeyConfig keyConfig = new KeyConfig();
            keyConfig.Initialize();
            //コンフィグ初期化設定
            keyConfig.LoadJsonData();
        }



        /// <summary>
        /// バインディングタイプ
        /// </summary>
        public enum KeyBindType
        {
            Keyboard,
            Gamepad,
            Mouse
        }
        public struct Binding
        {
            public InputAction inputAction;
            public int indexBinding;

            /// <summary>
            /// 変更後パス取得
            /// </summary>
            /// <returns></returns>
            public string GetEffectivePath()
            {
                return inputAction.bindings[indexBinding].effectivePath;
            }
            /// <summary>
            /// 現在のキー情報のみを取得。
            /// </summary>
            /// <returns></returns>
            public string GetEffectiveKey()
            {
                string path = inputAction.bindings[indexBinding].effectivePath;
                string[] parts = path.Split('/');
                return parts[parts.Length - 1]; // 最後の要素を返す
            }
            /// <summary>
            /// デフォルトパス取得
            /// </summary>
            /// <returns></returns>
            public string GetPath()
            {
                return inputAction.bindings[indexBinding].path;
            }

        }
        /// <summary>
        /// Jsonデータを保存する
        /// </summary>
        public void SaveJsonData()
        {

            if( actions == null) { Debug.LogWarning("InputSystem管理に問題があります。"); return; }
            string _json = actions.SaveBindingOverridesAsJson();
            //データを書き込み（これは特に危険性もない類のものなので簡単に
            using (var sw = new StreamWriter(Application.dataPath + pathKeyConfig,
                false, System.Text.Encoding.UTF8))
            {
                sw.Write(_json);
                Debug.Log("セーブ完了");
            }
        }
        /// <summary>
        /// キーコンフィグをロードする。
        /// </summary>
        /// <param name="data"></param>
        public void LoadJsonData()
        {
            if (File.Exists(Application.dataPath + pathKeyConfig))
            {
                using (var stream = new StreamReader(Application.dataPath + pathKeyConfig))
                {
                    //JsonファイルをString文に。
                    string _file = stream.ReadToEnd();
                    //ロード
                    actions.asset.LoadBindingOverridesFromJson(_file);
                }
            }
            else
            {
                Debug.Log("ファイルが存在しません");
            }
        }

        /// <summary>
        /// 同じキーバインドを探す。
        /// </summary>
        public Binding? OnSearchKeyBind(InputActionMap _actionMap, string _path, InputAction _inputAction)
        {
            foreach (var _action in _actionMap)
            {
                for (int i = 0; i < _action.bindings.Count; i++)
                {
                    var binding = _action.bindings[i];
                    //Pathの設定が同じ場合、処理を終了する。
                    if (binding.effectivePath == _path)
                    {
                        //Actionが同じではない場合のみ取得する。
                        if (_inputAction != _action)
                        {
                            //決定。
                            return new Binding { inputAction = _action, indexBinding = i };
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// バインディングタイプがどれか探す
        /// </summary>
        /// <returns></returns>
        public int? OnSearchBindingType(InputAction _action, KeyBindType _keyBindType)
        {
            string devicePrefix = _keyBindType switch
            {
                KeyBindType.Keyboard => "<Keyboard>",
                KeyBindType.Gamepad => "<Gamepad>",
                KeyBindType.Mouse => "<Mouse>",
                _ => null
            };
            for (int i = 0; i < _action.bindings.Count; i++)
            {
                var binding = _action.bindings[i];
                // 空パス対策
                if (string.IsNullOrEmpty(binding.path))
                    continue;
                Debug.Log($"Index {i} | Path: {binding.path} | Groups: {binding.groups}");
                if (binding.path.StartsWith(devicePrefix))
                {
                    Debug.Log($"これは {_keyBindType} 用のバインディングです");
                    return i;
                }
            }
            return null;
        }

        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        public void Cancel()
        {
            if (rebindOperation != null)
            {
                Debug.Log("Rebind キャンセル");
                rebindOperation?.Cancel();  // 中断
                rebindOperation?.Dispose(); // 後片付け
                rebindOperation = null;
            }
        }

        /// <summary>
        /// キーバインド設定/キーボードorMouse
        /// </summary>
        public Binding? OnEventChangedKeyboard(InputAction _action, Action _complete = null)
        {
            //どちらか片方のみしか受け付けないようにする。
            int? _keyboardNum = OnSearchBindingType(_action, KeyBindType.Keyboard);
            int? _mouseboardNum = OnSearchBindingType(_action, KeyBindType.Mouse);

            int num = 0;
            if (_keyboardNum == null && _mouseboardNum == null)
            {
                Debug.LogError(_action.name + "が設定されていません。ただちに設定をお願いします。");
                return null;
            }
            else if (_keyboardNum != null && _mouseboardNum != null)
            {
                Debug.LogError(_action.name + "がマウス、キーボード両方に設定されています。ただちに設定をお願いします。");
                return null;
            }
            else if (_keyboardNum != null)
            {
                num = (int)_keyboardNum;
            }
            else if (_mouseboardNum != null)
            {
                num = (int)_mouseboardNum;
            }

            Cancel();

            //特定アクションマップを無効化する
            _action.Disable();


            //事前にパスを保存しておく
            string beforePath = _action.bindings[num].effectivePath;
            Binding? bind = new Binding { inputAction = _action, indexBinding = (int)num };

            //設定
            rebindOperation = _action.PerformInteractiveRebinding(num)
                .WithControlsExcluding("Gamepad")//ゲームパッドは除外する
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(callback =>
                {
                    //他にバインディングしているものが存在している場合交換する
                    Binding? binding = OnSearchKeyBind(_action.actionMap, _action.bindings[(int)num].effectivePath, _action);
                    if (binding != null)
                    {
                        binding.Value.inputAction.ApplyBindingOverride(binding.Value.indexBinding, beforePath);
                    }
                    _complete?.Invoke();

                    SaveJsonData();
                    callback.Dispose();
                    _action.Enable();
                    rebindOperation = null;


                }).Start();

            return bind;
        }
        /// <summary>
        /// キーバインド設定/GamePad用 , 第一引数:どのアクションか。
        /// 第二引数:どのアクションマップか
        /// </summary>
        /// <param name="_action"></param>
        public Binding? OnEventChangedGamePad(InputAction _action,Action _complete = null)
        {
            //キャンセル処理をしておく
            Cancel();

            int? num = OnSearchBindingType(_action, KeyBindType.Gamepad);
            if (num == null)
            {
                Debug.LogError("GamePadのアクションタイプが登録されていません。");
                return null;
            }

            //事前にパスを保存しておく
            string beforePath = _action.bindings[(int)num].effectivePath;
            //Binding情報を返す為に保存
            Binding? bind = new Binding { inputAction = _action, indexBinding = (int)num };

            _action.Disable();
            rebindOperation = _action.PerformInteractiveRebinding((int)num)
                .WithControlsExcluding("Keyboard,Mouse")//キーボードを入力受付拒否
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(callback =>
                {
                    Binding? binding = OnSearchKeyBind(_action.actionMap, _action.bindings[(int)num].effectivePath, _action);
                    //他にバインディングしているものが存在している場合。
                    if (binding != null)
                    {
                        binding.Value.inputAction.ApplyBindingOverride(binding.Value.indexBinding, beforePath);
                    }
                    _complete?.Invoke();

                    SaveJsonData();
                    callback.Dispose();
                    _action.Enable();
                    rebindOperation = null;
                }).Start();
            return bind;
        }
        
    }
}