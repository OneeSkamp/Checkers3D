using UnityEngine;

namespace ui {
    public class SetComponentEnable : MonoBehaviour {
        public MonoBehaviour component;

        private void Awake() {
            if (component == null) {
                Debug.LogError("Component isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void SetEnable(bool value) {
            component.enabled = value;
        }
    }
}
