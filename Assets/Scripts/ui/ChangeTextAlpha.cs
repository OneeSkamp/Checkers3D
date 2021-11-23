using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using controller;

namespace ui {
    public class ChangeTextAlpha : MonoBehaviour {
        public Text text;
        public CheckersController chController;
        public AnimationCurve curve;
        public float speed;

        private void Awake() {
            if (text == null) {
                Debug.LogError("Text isn't provided");
                this.enabled = false;
                return;
            }

            if (chController == null) {
                Debug.LogError("Checkers controller isn't provided");
                this.enabled = false;
                return;
            }

            if (curve == null) {
                Debug.LogError("Curve isn't provided");
                this.enabled = false;
                return;
            }

            if (chController == null) {
                Debug.LogError("chController isn't provided");
                this.enabled = false;
                return;
            }

            chController.savedSuccessfully += async () => await ChangeAlpha();
        }

        public async Task ChangeAlpha() {
            var color = gameObject.GetComponent<Text>().color;
            for (float time = 0f; time <= 1f; time += Time.deltaTime * speed) {
                await Task.Yield();
                text.color = new Color(color.r, color.g, color.b, curve.Evaluate(time));
            }
        }
    }
}
