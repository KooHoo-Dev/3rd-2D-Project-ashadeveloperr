using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Babel
{

    public class ShopManager : MonoBehaviour
    {
        public TMP_Text userGoldText;
        public TMP_Text shipPriceText;
        public TMP_Text pumpPriceText;
        public TMP_Text timerPriceText;
        public TMP_Text insurancePriceText;
        public TMP_Text pumpLevelText;
        public TMP_Text timerLevelText;
        public TMP_Text insuranceLevelText;

        public Image notEnoughGold;

        private int shipPrice = 200;
        private int pumpPrice = 200;
        private int timerPrice = 200;
        private int insurancePrice = 200;

        private Coroutine countCoroutine;
        private int currentDisplayedLoot = 0;

        private void Start()
        {
            OnLootChanged += UpdateLootValue;
            currentDisplayedLoot = TotalGameManager.Instance.PlayerGold;
            userGoldText.SetText($"{TotalGameManager.Instance.PlayerGold}");

            refreshUIandPrice();
        }

        private void Update()
        {
        }

        private void OnDestroy()
        {
            OnLootChanged -= UpdateLootValue;
        }

        private void refreshUIandPrice()
        {

            shipPriceText.SetText($"{1000}");

            pumpPrice = 300 + (TotalGameManager.Instance.CurrentLevelPump * 300);
            pumpPriceText.SetText($"{300 + (TotalGameManager.Instance.CurrentLevelPump * 300)}");
            pumpLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelPump}");

            timerPrice = 500 + (TotalGameManager.Instance.CurrentLevelTimer * 500);
            timerPriceText.SetText($"{500 + (TotalGameManager.Instance.CurrentLevelTimer * 500)}");
            timerLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelTimer}");

            insurancePrice = 200 + (TotalGameManager.Instance.CurrentLevelInsurance * 200);
            insurancePriceText.SetText($"{200 + (TotalGameManager.Instance.CurrentLevelInsurance * 200)}");
            insuranceLevelText.SetText($"Lv.{TotalGameManager.Instance.CurrentLevelInsurance}");
        }


        public void ShipUpgrade()
        {
            
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
            }
        }
        public void TimerUpgrade()
        {

        }
        public void InsuranceUpgrade()
        {

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