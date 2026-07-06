using Study.Utilities;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Babel
{

    public class SceneController : MonoBehaviour
    {

        public enum GameScene
        {
            Boot = 0,
            MainMenu = 1,
            Stage = 2,
            BankruptcyEnding = 3,
        }

        private GameScene PrevScene { get; set; } = GameScene.MainMenu;
        private GameScene CurrentScene { get; set; } = GameScene.MainMenu;

        public void ChangeScene(int sceneIndex)
        {
            ChangeScene((GameScene)sceneIndex);
        }

        public void NextStage()
        {
            Time.timeScale = 1.0f;

            Scene currentScene = SceneManager.GetActiveScene();

            if (GameManager.Instance.isGameOver)
            {
                SceneManager.LoadScene(currentScene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(currentScene.buildIndex + 1);
            }
        }

        public void ChangeScene(GameScene targetScene)
        {
            Time.timeScale = 1.0f;

            PrevScene = CurrentScene;
            CurrentScene = targetScene;

            SceneManager.LoadScene((int)CurrentScene);
        }

        public void MovePrevScene()
        {
            ChangeScene(PrevScene);
        }




    }


}
