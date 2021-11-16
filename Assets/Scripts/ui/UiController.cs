using UnityEngine;
using controller;
using System;
using System.IO;
using UnityEngine.UI;

namespace ui {
    public class UiController : MonoBehaviour {
        public CheckersController chController;
        public GameObject loadPanel;
        public GameObject content;
        public GameObject menu;
        public GameObject loadItem;

        public void OpenMenu() {
            loadPanel.SetActive(false);
            menu.SetActive(!menu.activeSelf);
            chController.enabled = true;
            if (menu.activeSelf) {
                chController.enabled = false;
            }
        }

        public void OpenLoadPanel() {
            menu.SetActive(false);
            loadPanel.SetActive(true);
            this.enabled = false;

            var pathToFolder = Application.persistentDataPath;

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            string[] allfiles;
            try {
                allfiles = Directory.GetFiles(pathToFolder, "*.csv");
            } catch (Exception e) {
                allfiles = default;
                Debug.LogError(e);
            }

            foreach (string filename in allfiles) {
                if (filename == Path.Combine(pathToFolder, "newgame.csv")) continue;
                var loaderObj = Instantiate(loadItem);
                loaderObj.transform.SetParent(content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                var textObj = loaderObj.transform.GetChild(0);
                var text = textObj.GetComponent<Text>();
                var saveName = filename.Replace(pathToFolder, "");
                saveName = saveName.Replace(".csv", "");
                text.text = saveName;

                var imageObj = loaderObj.transform.GetChild(1);
                var image = imageObj.GetComponent<RawImage>();
                try {
                    byte[] data = File.ReadAllBytes(filename.Replace(".csv", ".png"));
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    image.texture = tex;
                } catch (Exception e ) {
                    Debug.LogError(e);
                    continue;
                }

                var loadObj = loaderObj.transform.GetChild(2);
                var loadBtn = loadObj.GetComponent<Button>();
                loadBtn.onClick.AddListener(() => chController.LoadGame(filename));

                var deleteObj = loaderObj.transform.GetChild(3);
                var deleteBtn = deleteObj.GetComponent<Button>();
                deleteBtn.onClick.AddListener(() => {
                    Destroy(loaderObj);
                    try {
                        File.Delete(filename);
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }

                    try {
                        File.Delete(filename.Replace(".csv", ".png"));
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }
                });
            }
        }
    }
}
