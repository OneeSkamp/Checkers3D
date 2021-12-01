using UnityEngine;

namespace ui {
    public class TimerForDestroy : MonoBehaviour {
        public float duration;

        private float timer;

        private void Awake() {
            if (duration <= 0f) {
                Debug.LogError("Duration must be positive float");
                this.enabled = false;
                return;
            }
        }

        private void Update() {
            if (timer > duration) {
                Destroy(gameObject);
            }

            timer += Time.deltaTime;
        }
    }
}