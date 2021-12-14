using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class Test : MonoBehaviour {
        public int col;
        public int row;

        private HorizontalLayoutGroup horizontal;
        private VerticalLayoutGroup vertical;
        private GridLayoutGroup grid;

        private void Awake() {
            horizontal = gameObject.GetComponent<HorizontalLayoutGroup>();
            vertical = gameObject.GetComponent<VerticalLayoutGroup>();
            grid = gameObject.GetComponent<GridLayoutGroup>();

            if (horizontal == null && vertical == null && grid == null) {
                Debug.LogError("This object not have HorizontalLayoutGroup");
                this.enabled = false;
            }
        }

        private void Update() {
            if (horizontal != null) {
                HorizontalScaler();
            } else if (vertical != null) {
                VerticalScaler();
            } else if (grid != null) {
                GridScaler();
            }
        }

        private void HorizontalScaler() {
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

        private void VerticalScaler() {
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

        private void GridScaler() {
            var width = 0f;
            var height = 0f;

            var count = gameObject.transform.childCount;
            var obj = gameObject.transform.GetChild(0);
            var rect = obj.gameObject.GetComponent<RectTransform>().rect;
            if (count >= row) {
                height = rect.height * row;
            } else {
                height = rect.height;
            }

            width = rect.width * Mathf.CeilToInt(count / (float)row);
            var size = new Vector2(width, height);
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}
