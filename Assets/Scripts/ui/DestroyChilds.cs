using UnityEngine;

namespace ui {
    public class DestroyChilds : MonoBehaviour {
        public void Destroy() {
            foreach (Transform item in gameObject.transform) {
                Destroy(item.gameObject);
            }
        }
    }
}
