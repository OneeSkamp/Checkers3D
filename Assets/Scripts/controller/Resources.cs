using UnityEngine;

namespace controller {
    [System.Serializable]
    public struct BoardPositions {
        public Vector3 posFor8x8;
        public Vector3 posFor10x10;
    }

    public class Resources : MonoBehaviour {
        public GameObject board8x8;
        public GameObject board10x10;
        public Transform leftTop;
        public Transform leftTop10x10;
        public Transform offset;
        public BoardPositions boardPositions;

        public GameObject whiteChecker;
        public GameObject blackChecker;
        public GameObject whiteLady;
        public GameObject blackLady;
        public GameObject moveHighlight;
        public GameObject selectedHighlight;
    }
}