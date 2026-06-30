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
            MainMenu = 0,
            Stage,
        }

        private GameScene PrevScene { get; set; } = GameScene.MainMenu;
        private GameScene CurrentScene { get; set; } = GameScene.MainMenu;

        public void ChangeScene(int sceneIndex)
        {
            ChangeScene((GameScene)sceneIndex);
        }

        public void NextStage()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (GameManager.Instance.isGameOver)
            {
                SceneManager.LoadScene(currentScene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(currentScene.buildIndex + 1); // 나중에 스테이지 씬 마지막 값을 꼭 찾아서 분기처리해야함 안그럼뒤짐
            }
            
        }

        public void ChangeScene(GameScene targetScene)
        {
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
