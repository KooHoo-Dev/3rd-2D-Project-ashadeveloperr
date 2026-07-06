using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Babel
{

    public class ShopManager : MonoBehaviour
    {
        public TMP_Text userGoldText;
        public TMP_Text shipNameText;
        public TMP_Text shipPriceText;
        public TMP_Text pumpPriceText;
        public TMP_Text timerPriceText;
        public TMP_Text insurancePriceText;
        public TMP_Text pumpLevelText;
        public TMP_Text timerLevelText;
        public TMP_Text insuranceLevelText;
        public TMP_Text shipSpecText;
        public TMP_Text shipMaintenanceText;
        public TMP_Text totalMaintenanceText;

        public Button shipButton;

        public Image notEnoughGold;

        public Image canoeImage;
        public Image boatImage;
        public Image cargoImage;

        private int shipPrice;
        private int pumpPrice;
        private int timerPrice;
        private int insurancePrice;

        private Coroutine countCoroutine;
        private int currentDisplayedLoot = 0;

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
        }

        private void OnEnable()
        {
            UpdateLootValue(TotalGameManager.Instance.PlayerGold);
            refreshUIandPrice();
        }

        private void OnDestroy()
        {
            OnLootChanged -= UpdateLootValue;
        }




        private void refreshUIandPrice()
        {
            RefreshShipUI();

            pumpPrice = 300 + (TotalGameManager.Instance.CurrentLevelPump * 300);
            pumpPriceText.SetText($"{300 + (TotalGameManager.Instance.CurrentLevelPump * 300)}");
            pumpLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelPump}");

            timerPrice = 500 + (TotalGameManager.Instance.CurrentLevelTimer * 500);
            timerPriceText.SetText($"{500 + (TotalGameManager.Instance.CurrentLevelTimer * 500)}");
            timerLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelTimer}");

            insurancePrice = 200 + (TotalGameManager.Instance.CurrentLevelInsurance * 200);
            insurancePriceText.SetText($"{200 + (TotalGameManager.Instance.CurrentLevelInsurance * 200)}");
            insuranceLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelInsurance}");

            RefreshTotalMaintenanceUI();
        }

        private void RefreshTotalMaintenanceUI()
        {
            if (totalMaintenanceText == null)
            {
                return;
            }

            int totalMaintenanceCost = TotalGameManager.Instance.TotalMaintenanceCost();
            totalMaintenanceText.SetText($"{totalMaintenanceCost}");
        }


        public void ShipUpgrade()
        {
            TotalGameManager manager = TotalGameManager.Instance;

            if (!TryGetNextShipType(manager.currentShipType, out TotalGameManager.ShipType nextShipType))
            {
                refreshUIandPrice();
                return;
            }

            shipPrice = GetShipUpgradePrice(manager.currentShipType);

            if (manager.PlayerGold < shipPrice)
            {
                Debug.Log("골드가 부족합니다.");
                return;
            }

            manager.AddGold(-shipPrice);
            manager.currentShipType = nextShipType;

            OnLootChanged?.Invoke(manager.PlayerGold);
            refreshUIandPrice();
        }


        private void RefreshShipUI()
        {
            TotalGameManager manager = TotalGameManager.Instance;
            TotalGameManager.ShipType currentShipType = manager.currentShipType;

            if (!TryGetNextShipType(currentShipType, out TotalGameManager.ShipType nextShipType))
            {
                shipPrice = 0;

                SetShopShipImage(null);

                if (shipNameText != null)
                {
                    shipNameText.SetText("Max Level");
                }

                if (shipPriceText != null)
                {
                    shipPriceText.SetText("-");
                }

                if (shipMaintenanceText != null)
                {
                    shipMaintenanceText.SetText("Maintenance : -");
                }

                if (shipSpecText != null)
                {
                    shipSpecText.SetText(
                        $"Your {GetShipDisplayName(currentShipType)}'s displacement : {GetShipMass(currentShipType)}kg\n" +
                        $"No more ships available."
                    );
                }

                if (shipButton != null)
                {
                    shipButton.interactable = false;
                }

                return;
            }

            shipPrice = GetShipUpgradePrice(currentShipType);

            int currentShipMaintenanceCost = manager.GetShipMaintenanceCost(currentShipType);
            int nextShipMaintenanceCost = manager.GetShipMaintenanceCost(nextShipType);

            SetShopShipImage(nextShipType);

            if (shipNameText != null)
            {
                shipNameText.SetText(GetShipDisplayName(nextShipType));
            }

            if (shipPriceText != null)
            {
                shipPriceText.SetText($"{shipPrice}");
            }

            if (shipMaintenanceText != null)
            {
                shipMaintenanceText.SetText(
                    $"Current Maintenance : {currentShipMaintenanceCost}\n" +
                    $"Next Maintenance : {nextShipMaintenanceCost}"
                );
            }

            if (shipSpecText != null)
            {
                shipSpecText.SetText(
                    $"Your {GetShipDisplayName(currentShipType)}'s displacement : {GetShipMass(currentShipType)}kg\n" +
                    $"This {GetShipDisplayName(nextShipType)}'s displacement : {GetShipMass(nextShipType)}kg"
                );
            }

            if (shipButton != null)
            {
                shipButton.interactable = true;
            }
        }

        private void SetShopShipImage(TotalGameManager.ShipType? nextShipType)
        {
            if (canoeImage != null)
            {
                canoeImage.gameObject.SetActive(nextShipType == TotalGameManager.ShipType.Canoe);
            }

            if (boatImage != null)
            {
                boatImage.gameObject.SetActive(nextShipType == TotalGameManager.ShipType.Boat);
            }

            if (cargoImage != null)
            {
                cargoImage.gameObject.SetActive(nextShipType == TotalGameManager.ShipType.Cargo);
            }
        }


        private int GetShipUpgradePrice(TotalGameManager.ShipType currentShipType)
        {
            return currentShipType switch
            {
                TotalGameManager.ShipType.Raft => 1000,
                TotalGameManager.ShipType.Canoe => 3000,
                TotalGameManager.ShipType.Boat => 5000,
                _ => 0
            };
        }

        private string GetShipDisplayName(TotalGameManager.ShipType shipType)
        {
            return shipType switch
            {
                TotalGameManager.ShipType.Raft => "Raft",
                TotalGameManager.ShipType.Canoe => "Canoe",
                TotalGameManager.ShipType.Boat => "Boat",
                TotalGameManager.ShipType.Cargo => "Cargo",
                _ => "Unknown Ship"
            };
        }

        private int GetShipMass(TotalGameManager.ShipType shipType)
        {
            return shipType switch
            {
                TotalGameManager.ShipType.Raft => 150,
                TotalGameManager.ShipType.Canoe => 200,
                TotalGameManager.ShipType.Boat => 250,
                TotalGameManager.ShipType.Cargo => 300,
                _ => 0
            };
        }

        private bool TryGetNextShipType(TotalGameManager.ShipType currentShipType, out TotalGameManager.ShipType nextShipType)
        {
            switch (currentShipType)
            {
                case TotalGameManager.ShipType.Raft:
                    nextShipType = TotalGameManager.ShipType.Canoe;
                    return true;

                case TotalGameManager.ShipType.Canoe:
                    nextShipType = TotalGameManager.ShipType.Boat;
                    return true;

                case TotalGameManager.ShipType.Boat:
                    nextShipType = TotalGameManager.ShipType.Cargo;
                    return true;

                default:
                    nextShipType = currentShipType;
                    return false;
            }
        }


        public void PumpUpgrade()
        {
            if(TotalGameManager.Instance.PlayerGold >= pumpPrice)
            {
                TotalGameManager.Instance.CurrentLevelPump++;
                TotalGameManager.Instance.AddGold(-pumpPrice);
                OnLootChanged?.Invoke(TotalGameManager.Instance.PlayerGold);
                refreshUIandPrice();
            }
            else
            {
                Debug.Log("골드가 부족합니다.");
            }
        }
        public void TimerUpgrade()
        {
            if (TotalGameManager.Instance.PlayerGold >= timerPrice)
            {
                TotalGameManager.Instance.CurrentLevelTimer++;
                TotalGameManager.Instance.AddGold(-timerPrice);

                OnLootChanged?.Invoke(TotalGameManager.Instance.PlayerGold);
                refreshUIandPrice();
            }
            else
            {
                Debug.Log("골드가 부족합니다.");
            }
        }
        public void InsuranceUpgrade()
        {
            if (TotalGameManager.Instance.PlayerGold >= insurancePrice)
            {
                TotalGameManager.Instance.CurrentLevelInsurance++;
                TotalGameManager.Instance.AddGold(-insurancePrice);

                OnLootChanged?.Invoke(TotalGameManager.Instance.PlayerGold);
                refreshUIandPrice();
            }
            else
            {
                Debug.Log("골드가 부족합니다.");
            }
        }


        private void UpdateLootValue(int targetLoot)
        {
            if (countCoroutine != null) StopCoroutine(countCoroutine);
            countCoroutine = StartCoroutine(CountLootRoutine(targetLoot));
        }

        private IEnumerator CountLootRoutine(int targetLoot)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            int startLoot = currentDisplayedLoot;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentDisplayedLoot = (int)Mathf.Lerp(startLoot, targetLoot, elapsed / duration);

                userGoldText.SetText($"{currentDisplayedLoot}");
                yield return null;
            }

            currentDisplayedLoot = targetLoot;
            userGoldText.SetText($"{currentDisplayedLoot}");
            countCoroutine = null;
        }



        public event System.Action<int> OnLootChanged;
    }
}