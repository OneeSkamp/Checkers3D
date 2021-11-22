using UnityEngine;
using controller;

namespace ui {
    public class SetActiveObject : MonoBehaviour {
        public GameObject obj;
        public CheckersController chController;
        private void Awake() {
            chController.saveGameOn += () => SetActiveValue(true);
            chController.saveGameOff += () => SetActiveValue(false);
        }
        public void SetActiveValue(bool value) {
            obj.SetActive(value);
        }
    }
}
