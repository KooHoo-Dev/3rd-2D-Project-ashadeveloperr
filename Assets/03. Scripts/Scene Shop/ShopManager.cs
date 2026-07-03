using TMPro;
using UnityEngine;

namespace Babel
{

    public class ShopManager : MonoBehaviour
    {
        public TMP_Text userGoldText;

        private void Start()
        {
            userGoldText.SetText($"{TotalGameManager.Instance.PlayerGold}");
        }
    }



}