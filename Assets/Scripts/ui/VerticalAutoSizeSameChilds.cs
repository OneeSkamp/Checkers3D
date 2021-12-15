using UnityEngine;
using UnityEngine.UI;

namespace ui {
    [ExecuteInEditMode, RequireComponent(typeof(VerticalLayoutGroup))]
    public class VerticalAutoSizeSameChilds : MonoBehaviour {
        public GameObject defoultObj;
        private Rect rect;
        private void Awake() {
            rect = defoultObj.GetComponent<RectTransform>().rect;
        }

        private void Update() {
            var childCount = gameObject.transform.childCount;
            var size = new Vector2(rect.width, rect.height * childCount);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}
