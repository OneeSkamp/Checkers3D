using UnityEngine;

namespace ui {
    public class SpawnObject : MonoBehaviour {
        public GameObject obj;
        public Transform parent;

        public void Spawn() {
            var instObj = Instantiate(obj);
            instObj.transform.SetParent(parent);
        }
    }
}
