using UnityEngine;

namespace controller {
    public class Resources : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;
        public Transform moveHighlights;

        public GameObject whiteChecker;
        public GameObject blackChecker;
        public GameObject moveHighlight;

        private void Awake() {
            if (boardTransform == null) {
                Debug.LogError("Board transform isn't provided");
                this.enabled = false;
                return;
            }

            if (leftTop == null) {
                Debug.LogError("Left top isn't provided");
                this.enabled = false;
                return;
            }

            if (moveHighlights == null) {
                Debug.LogError("Move highlights isn't provided");
                this.enabled = false;
                return;
            }

            if (whiteChecker == null) {
                Debug.LogError("White checker isn't provided");
                this.enabled = false;
                return;
            }

            if (blackChecker == null) {
                Debug.LogError("Black checker isn't provided");
                this.enabled = false;
                return;
            }

            if (moveHighlight == null) {
                Debug.LogError("Move highlight isn't provided");
                this.enabled = false;
                return;
            }
        }
    }
}

