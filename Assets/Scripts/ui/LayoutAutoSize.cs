using UnityEngine;
using UnityEngine.UI;

namespace ui {
    [ExecuteInEditMode]
    public class LayoutAutoSize : MonoBehaviour {
        public HorizontalOrVerticalLayoutGroup layout;

        private void Update() {
            if (layout == null) return;

            var isVert = layout is VerticalLayoutGroup;

            var size = Vector2.zero;
            foreach (RectTransform item in transform) {
                var x = item.rect.width;
                var y = item.rect.height;

                if (isVert) {
                    (x, y) = (y, x);
                }

                size.x += x;
                if (y > size.y) {
                    size.y = y;
                }
            }

            var rectTransform = (RectTransform)transform;
            if (size != rectTransform.sizeDelta) {
                if (isVert) {
                    (size.x, size.y) = (size.y, size.x);
                }

                rectTransform.sizeDelta = size;
            }
        }
    }
}
