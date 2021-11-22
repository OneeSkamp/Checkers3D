using UnityEngine;
using controller;

namespace ui {
    public class OpenMenu : MonoBehaviour {
        public CheckersController chController;
        private void Awake() {
            chController.gameOver += Open;
        }

        public void Open() {
            gameObject.SetActive(!gameObject.activeSelf);
            chController.enabled = !gameObject.activeSelf;
        }
    }
}
