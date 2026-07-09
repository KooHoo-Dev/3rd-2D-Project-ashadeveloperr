using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Babel
{
    public class ShopManager : MonoBehaviour
    {
        [Header("골드 UI")]
        [SerializeField] private TMP_Text userGoldText;

        [Header("선박 UI")]
        [SerializeField] private TMP_Text shipNameText;
        [SerializeField] private TMP_Text shipPriceText;
        [SerializeField] private TMP_Text shipSpecText;
        [SerializeField] private TMP_Text shipMaintenanceText;
        [SerializeField] private Button shipButton;

        [Header("업그레이드 UI")]
        [SerializeField] private TMP_Text pumpPriceText;
        [SerializeField] private TMP_Text timerPriceText;
        [SerializeField] private TMP_Text insurancePriceText;
        [SerializeField] private TMP_Text pumpLevelText;
        [SerializeField] private TMP_Text timerLevelText;
        [SerializeField] private TMP_Text insuranceLevelText;

        [Header("유지비 UI")]
        [SerializeField] private TMP_Text totalMaintenanceText;

        [Header("상점 이미지")]
        [SerializeField] private Image canoeImage;
        [SerializeField] private Image boatImage;
        [SerializeField] private Image cargoImage;

        [Header("알림 UI")]
        [SerializeField] private Image notEnoughGold;

        private int shipPrice;
        private int pumpPrice;
        private int timerPrice;
        private int insurancePrice;
        private Coroutine countCoroutine;
        private int currentDisplayedLoot;
        private event System.Action<int> OnLootChanged;

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
        }

        private void OnEnable()
        {
            UpdateLootValue(TotalGameManager.Instance.PlayerGold);
            RefreshUIAndPrice();
        }

        private void OnDestroy()
        {
            OnLootChanged -= UpdateLootValue;
        }

        private void RefreshUIAndPrice()
        {
            RefreshShipUI();
            pumpPrice = 300 + (TotalGameManager.Instance.CurrentLevelPump * 300);
            pumpPriceText.SetText($"{pumpPrice}");
            pumpLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelPump}");
            timerPrice = 500 + (TotalGameManager.Instance.CurrentLevelTimer * 500);
            timerPriceText.SetText($"{timerPrice}");
            timerLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelTimer}");
            insurancePrice = 200 + (TotalGameManager.Instance.CurrentLevelInsurance * 200);
            insurancePriceText.SetText($"{insurancePrice}");
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

            if (!TryGetNextShipType(manager.CurrentShipType, out TotalGameManager.ShipType nextShipType))
            {
                RefreshUIAndPrice();
                return;
            }

            shipPrice = GetShipUpgradePrice(manager.CurrentShipType);

            if (manager.PlayerGold < shipPrice)
            {
                Debug.Log("골드가 부족합니다.");
                return;
            }

            manager.AddGold(-shipPrice);
            manager.CurrentShipType = nextShipType;
            OnLootChanged?.Invoke(manager.PlayerGold);
            RefreshUIAndPrice();
        }

        private void RefreshShipUI()
        {
            TotalGameManager manager = TotalGameManager.Instance;
            TotalGameManager.ShipType currentShipType = manager.CurrentShipType;

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
                    shipSpecText.SetText($"Your {GetShipDisplayName(currentShipType)}'s displacement : {GetShipMass(currentShipType)}kg\nNo more ships available.");
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
                shipMaintenanceText.SetText($"Current Maintenance : {currentShipMaintenanceCost}\nNext Maintenance : {nextShipMaintenanceCost}");
            }

            if (shipSpecText != null)
            {
                shipSpecText.SetText($"Your {GetShipDisplayName(currentShipType)}'s displacement : {GetShipMass(currentShipType)}kg\nThis {GetShipDisplayName(nextShipType)}'s displacement : {GetShipMass(nextShipType)}kg");
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
            if (TotalGameManager.Instance.PlayerGold >= pumpPrice)
            {
                TotalGameManager.Instance.CurrentLevelPump++;
                TotalGameManager.Instance.AddGold(-pumpPrice);
                OnLootChanged?.Invoke(TotalGameManager.Instance.PlayerGold);
                RefreshUIAndPrice();
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
                RefreshUIAndPrice();
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
                RefreshUIAndPrice();
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
    }
}
