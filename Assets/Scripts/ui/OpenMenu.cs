using UnityEngine;
using controller;

namespace ui {
    public class OpenMenu : MonoBehaviour {
        public CheckersController chController;

        private void Awake() {
            if (chController == null) {
                Debug.LogError("Checkers controller isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void Open() {
            gameObject.SetActive(!gameObject.activeSelf);
            chController.enabled = !gameObject.activeSelf;
        }
    }
}
