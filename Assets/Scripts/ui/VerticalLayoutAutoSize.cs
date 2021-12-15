using UnityEngine;
using UnityEngine.UI;

namespace ui {
    [ExecuteInEditMode, RequireComponent(typeof(VerticalLayoutGroup))]
    public class VerticalLayoutAutoSize : MonoBehaviour {
        private void Update() {
            var width = 0f;
            var height = 0f;

            foreach (Transform item in gameObject.transform) {
                var rectItem = item.gameObject.GetComponent<RectTransform>().rect;
                height += rectItem.height;
                if (rectItem.width > width) {
                    width = rectItem.width;
                }
            }

            var rectObj = gameObject.GetComponent<RectTransform>().rect;
            var size = new Vector2(width, height);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}
