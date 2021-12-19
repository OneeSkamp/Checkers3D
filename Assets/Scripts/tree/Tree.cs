using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {
    public struct Node {
        public List<Node> childs;
        public Vector2Int pos;
    }
}
