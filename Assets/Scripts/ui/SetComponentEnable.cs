using UnityEngine;

namespace ui {
    public class SetComponentEnable : MonoBehaviour {
        public MonoBehaviour component;
        public void SetEnable(bool value) {
            component.enabled = value;
        }
    }
}
