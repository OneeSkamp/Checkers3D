using UnityEngine;

namespace controller {

    public class CheckersController : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;
        public Transform rightBottom;
        public float cellSize;
        public float leftTopX;
        public float rightBottomX;

        private void Awake() {
            leftTopX = leftTop.position.x;
            rightBottomX = rightBottom.position.x;
            cellSize = (Mathf.Abs(leftTopX) + Mathf.Abs(rightBottomX)) / 8;
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {
                return;
            }
            var size = (Mathf.Abs(leftTopX) + Mathf.Abs(rightBottomX)) / 8;
            var offset = leftTop.position.x - size / 2;
            var localHit = boardTransform.InverseTransformPoint(hit.point);


            var hitOffcet = (localHit - leftTop.position) / size;
            var pos = new Vector2Int(Mathf.Abs((int)hitOffcet.x), Mathf.Abs((int)hitOffcet.z));

            // var hitOffcet = (localHit + leftTop.position) / 2;
            // Debug.Log(hitOffcet);
            // var pos = new Vector2Int(Mathf.Abs((int)hitOffcet.x), Mathf.Abs((int)hitOffcet.z));

             Debug.Log(pos.x + " " + pos.y);
        }
    }
}
