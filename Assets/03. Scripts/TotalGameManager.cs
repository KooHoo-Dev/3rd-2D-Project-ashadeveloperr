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
        public int PlayerGold { get; private set; } = 1000;

        [SerializeField] private ShipType currentShipType = ShipType.Cargo;
        public int CurrentLevelPump { get; private set; } = 0;
        public int CurrentLevelTimer { get; private set; } = 0;
        public int CurrentLevelInsurance { get; private set; } = 0;

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
            MaintenanceCost = (CurrentLevelPump * 100) + (CurrentLevelInsurance * 250) + GetShipMaintenanceCost(currentShipType);
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
                ShipType.Raft => 100,
                ShipType.Canoe => 250,
                ShipType.Cargo => 500,
                _ => 0
            };
        }









    }


}
