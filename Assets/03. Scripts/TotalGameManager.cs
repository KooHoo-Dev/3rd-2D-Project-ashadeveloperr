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

        public int CurrentLevelShip { get; private set; } = 0;
        public int CurrentLevelPump { get; private set; } = 0;
        public int CurrentLevelTimer { get; private set; } = 0;
        public int CurrentLevelInsurance { get; private set; } = 0;

        public int MaintenanceCost { get; private set; }

        public int currentStageLevel = 0;



        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public int TotalMaintenanceCost()
        {
            MaintenanceCost = (CurrentLevelPump * 100) + (CurrentLevelInsurance * 250);
            return MaintenanceCost;
        }

        public void AddGold(int amount)
        {
            PlayerGold += amount;
        }








        


    }


}
