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

        public ShipType currentShipType = (ShipType)2;
        public int CurrentLevelPump { get; set; } = 0;
        public int CurrentLevelTimer { get; set; } = 0;
        public int CurrentLevelInsurance { get; set; } = 0;

        public int MaintenanceCost { get; private set; }

        public int currentStageLevel = 0;

        public enum ShipType
        {
            Raft,
            Canoe,
            Cargo,
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public int TotalMaintenanceCost()
        {
            MaintenanceCost = (CurrentLevelPump * 50) + (CurrentLevelInsurance * 50) + GetShipMaintenanceCost(currentShipType);
            return MaintenanceCost;
        }

        public void AddGold(int amount)
        {
            PlayerGold += amount;
        }

        public int GetShipMaintenanceCost(ShipType type)
        {
            return type switch
            {
                ShipType.Raft => 0,
                ShipType.Canoe => 50,
                ShipType.Cargo => 100,
                _ => 0
            };
        }









    }


}
