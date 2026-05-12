using UnityEngine;

namespace Common
{
    /// <summary>
    /// Singleton
    /// </summary>
    public class InputSystemManager : SingletonBase<InputSystemManager>
    {
        public InputSystem_Actions inputActions { get; private set; }

        InputSystemManager ()
        {
            inputActions = new InputSystem_Actions();
        }
        /// <summary>
        /// 全Inputを遮断
        /// </summary>
        public void DisableInput()
        {
            inputActions.Disable();
        }
    }
}