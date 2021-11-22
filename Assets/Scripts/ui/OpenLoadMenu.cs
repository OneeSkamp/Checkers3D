using UnityEngine;
using controller;
using UnityEngine.UI;
using option;

namespace ui {
    // public class OpenLoadMenu : MonoBehaviour {
    //     public CheckersController chController;
    //     public GameObject content;

    //     public void OpenLoadPanel() {
    //         foreach (Transform item in content.transform) {
    //             Destroy(item.gameObject);
    //         }

    //         var saveInfos = chController.GetSaveInfos(Application.persistentDataPath);
    //         foreach (var saveInfo in saveInfos) {
    //             var loaderObj = Instantiate(loadItem);
    //             loaderObj.transform.SetParent(content.transform);
    //             loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

    //             var textTransform = loaderObj.transform.GetChild(0);
    //             var text = textTransform.GetComponent<Text>();
    //             text.text = saveInfo.date;

    //             var imageTransform = loaderObj.transform.GetChild(1);
    //             var image = imageTransform.GetComponent<RawImage>();
    //             FillImageBoard(image, saveInfo.board);

    //             var loadTransform = loaderObj.transform.GetChild(2);
    //             var loadBtn = loadTransform.GetComponent<Button>();
    //             loadBtn.onClick.AddListener(() => {
    //                 chController.LoadGame(saveInfo.savePath);
    //                 menu.SetActive(false);
    //                 loadPanel.SetActive(false);
    //                 chController.enabled = true;
    //             });

    //             var deleteTransform = loaderObj.transform.GetChild(3);
    //             var deleteBtn = deleteTransform.GetComponent<Button>();

    //             deleteBtn.onClick.AddListener(() => {
    //                 Destroy(loaderObj);
    //                 try {
    //                     File.Delete(saveInfo.savePath);
    //                 } catch (Exception e) {
    //                     Debug.LogError(e);
    //                 }
    //             });

    //             var moveColorTransform = loaderObj.transform.GetChild(5);
    //             var moveClrText = moveColorTransform.GetComponent<Text>();
    //             if (saveInfo.moveColor == ChColor.White) {
    //                 moveClrText.text = "WHITE";
    //             }
    //         }
    //     }

    //     public void FillImageBoard(RawImage image, Option<Checker>[,] board) {
    //         for (int i = 0; i < board.GetLength(0); i++) {
    //             for (int j = 0; j < board.GetLength(1); j++) {
    //                 if (board[i, j].IsNone()) continue;
    //                 var ch = board[i, j].Peel();
    //                 RawImage checker = null;
    //                 if (ch.color == ChColor.White) {
    //                     checker = whiteCh;
    //                     if (ch.type == ChType.Lady) {
    //                         checker = whiteLady;
    //                     }
    //                 }

    //                 if (ch.color == ChColor.Black) {
    //                     checker = blackCh;
    //                     if (ch.type == ChType.Lady) {
    //                         checker = blackLady;
    //                     }
    //                 }

    //                 var img = Instantiate(checker);
    //                 img.transform.SetParent(image.transform);
    //                 var cell = new Vector2Int(i, j);
    //                 img.transform.localPosition = chController.ToCellOnImage(cell);
    //             }
    //         }
    //     }
    // }
}
