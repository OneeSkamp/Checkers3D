using UnityEngine;

namespace controller {
    [System.Serializable]
    public struct BoardPositions {
        public Vector3 posFor8x8;
        public Vector3 posFor10x10;
    }

    public class Resources : MonoBehaviour {
        public Transform board8x8Transform;
        public Transform board10x10Transform;
        public Transform leftTop;
        public Transform leftTop10x10;
        public Transform offset;
        public BoardPositions boardPositions;
        // public Transform cameraPos8x8;
        // public Transform cameraPos10x10;

        public GameObject whiteChecker;
        public GameObject blackChecker;
        public GameObject whiteLady;
        public GameObject blackLady;
        public GameObject moveHighlight;
        public GameObject selectedHighlight;
    }
}