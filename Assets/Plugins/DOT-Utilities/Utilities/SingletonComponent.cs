using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace DOT.Utilities
{
    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        /* ------------------------------------------ */

        public static T instance
        {
            get { return _instance; }
        }

        private static T _instance;

        public bool IsPermanent;

        /* ------------------------------------------ */

        private string initiationScene;

        /* ------------------------------------------ */

        protected virtual void Awake()
        {
            // If there is an instance already, we'll destroy the new instance here
            if (_instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = GetComponent<T>();
            initiationScene = SceneManager.GetActiveScene().name;

            // If we want to keep the object through scene changes, we don't subscribe to the scene changes
            // and do nothing when it's occur.
            if (IsPermanent)
                DontDestroyOnLoad(this.gameObject);
            else
                SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
        }

        protected virtual void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            StopAllCoroutines();

            _instance = null;
        }

        /* ------------------------------------------ */

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            // If it's not a new scene, do not clear the instance data
            if (initiationScene == scene.name)
                return;

            // Clear the data and unsubscribe the event
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            _instance = null;
        }

        /* ------------------------------------------ */
    }
}