using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class TextSmoothOpacity : MonoBehaviour {
        public AnimationCurve curve;
        public float duration;

        private float timer;
        private Text text;

        private void Awake() {
            if (curve == null) {
                curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }

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

        private void OnEnable() {
            timer = 0;
        }

        private void Update() {
            if (timer > duration) {
                this.enabled = false;
                return;
            }

            timer += Time.deltaTime;
            var t = Mathf.Clamp(timer / duration, 0f, 1f);

            var clr = text.color;
            text.color = new Color(clr.r, clr.g, clr.b, curve.Evaluate(t));
        }
    }
}