using UnityEngine;


namespace Common
{
    /// <summary>
    /// Singleton継承。
    /// Instanceメソッドで呼び出し。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
    {
        protected static T instance;

        /// <summary>
        /// シーンまたぎはInstanceメソッドの返り値をtrueに。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T Instance(bool destroy = false)
        {
            if (instance == null)
            {
                var gameObject = new GameObject(typeof(T).Name);
                instance = gameObject.AddComponent<T>();
                if (destroy == false)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            return instance;
        }
    }
}