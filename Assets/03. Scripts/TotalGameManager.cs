using Study.Utilities;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace Babel
{

    public class TotalGameManager : SingletonBase<TotalGameManager>
    {
        public int PlayerGold { get; private set; }

        public int CurrntLevelShip { get; private set; } = 0;
        public int CurrntLevelPump { get; private set; } = 0;
        public int CurrntLevelTimer { get; private set; } = 0;
        public int CurrntLevelInsurance { get; private set; } = 0;

        public int CurrentStageLevel { get; private set; } = 0;


        public void AddGold(int amount)
        {
            PlayerGold += amount;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DontDestroyOnLoad(gameObject);
        }






        


    }


}
