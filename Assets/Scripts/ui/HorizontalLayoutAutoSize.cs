using UnityEngine;
using UnityEngine.UI;

namespace ui {
    [ExecuteInEditMode, RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class HorizontalLayoutAutoSize : MonoBehaviour {
        private void Update() {
            var width = 0f;
            var height = 0f;

            foreach (Transform item in gameObject.transform) {
                var rectItem = item.gameObject.GetComponent<RectTransform>().rect;
                width += rectItem.width;
                if (rectItem.height > height) {
                    height = rectItem.height;
                }
            }

            var rectObj = gameObject.GetComponent<RectTransform>().rect;
            var size = new Vector2(width, height);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}
