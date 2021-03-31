using System.Linq;
using UnityEngine;

namespace VeinPlanter.Presenters
{
    public class PresenterBase
    {
        public static void AttachGameObject(GameObject go, Transform parent)
        {
            VeinPlanter.DontDestroyOnLoad(go);
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(0, 0, 0);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.SetActive(true);
        }

        public T GetNamedComponentInChildren<T>(GameObject go, string name) where T : MonoBehaviour { return go.GetComponentsInChildren<T>().Where(k => k.gameObject.name == name).FirstOrDefault(); }
    }
}
