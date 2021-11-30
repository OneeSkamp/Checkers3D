using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class TextSmoothOpacity : MonoBehaviour {
        public float duration;

        private float timeElapsed;
        private Text text;

        private void Awake() {
            if (duration <= 0f) {
                Debug.LogError("Duration must be positive float");
                this.enabled = false;
                return;
            }

            text = GetComponent<Text>();
            if (text == null) {
                Debug.LogError("This component requires text");
                this.enabled = false;
                return;
            }
        }

        private void Update() {
            if (timeElapsed > duration) {
                Destroy(gameObject);
            }

            var clr = text.color;
            if (timeElapsed < duration) {
                text.color = new Vector4(clr.r, clr.g, clr.b, duration /timeElapsed);
                timeElapsed += Time.deltaTime;
            }
        }
    }
}