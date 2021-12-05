using UnityEngine;
using controller;

public class NewGame : MonoBehaviour {
    public CheckersController checkersController;

    public void Load(TextAsset gameAsset) {
        checkersController.LoadGame(gameAsset.text);
    }

}
