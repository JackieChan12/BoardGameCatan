using System.Collections;
using Assets.Scripts.DataStorage.Managers;
using DataStorage;
using UnityEngine;

namespace UI.Game.Popups
{
    public class PopupWindowsController : MonoBehaviour
    {
        [Header("Popups")][Space(5)]
        [Tooltip("Monopol Popup")][SerializeField]
        private GameObject monopolPopup;
        [Tooltip("Invention Popup")][SerializeField] 
        private GameObject inventionPopup;
        [Tooltip("Thief Pay Popup")][SerializeField] 
        private GameObject thiefPayPopup;
        [Tooltip("Thief Player Choice Popup")][SerializeField]
        private GameObject thiefPlayerChoicePopup;
        [Tooltip("Obligatory Action Info Popup")][SerializeField]
        private GameObject obligatoryActionInfoPopup;
        [Tooltip("Bought Card Popup")][SerializeField] 
        private GameObject boughtCardPopup;
        [Tooltip("Land Trade Popup")][SerializeField] 
        private GameObject landTradePopup;
        [Tooltip("Land Trade Accept Popup")][SerializeField] 
        private GameObject landTradeAcceptPopup;
        [Tooltip("Land Trade Accept Popup")][SerializeField] 
        private GameObject seaTradePopup;
        [Tooltip("End Game Popup")][SerializeField] 
        private GameObject endGamePopup;
        
        void Start()
        {

        }

        void Update()
        {
            SetPopupActivity(thiefPlayerChoicePopup, 
                GameManager.PopupManager.PopupsShown[PopupManager.THIEF_PLAYER_CHOICE_POPUP], 0.3f);
            SetPopupActivity(monopolPopup, GameManager.PopupManager.PopupsShown[PopupManager.MONOPOL_POPUP]);
            SetPopupActivity(inventionPopup, GameManager.PopupManager.PopupsShown[PopupManager.INVENTION_POPUP]);
            SetPopupActivity(thiefPayPopup, GameManager.PopupManager.PopupsShown[PopupManager.THIEF_PAY_POPUP]);
            SetPopupActivity(boughtCardPopup, GameManager.PopupManager.PopupsShown[PopupManager.BOUGHT_CARD_POPUP]);
            SetPopupActivity(landTradePopup, GameManager.PopupManager.PopupsShown[PopupManager.LAND_TRADE_POPUP]);
            SetPopupActivity(landTradeAcceptPopup, 
                GameManager.PopupManager.PopupsShown[PopupManager.LAND_TRADE_ACCEPT_POPUP]);
            SetPopupActivity(seaTradePopup, GameManager.PopupManager.PopupsShown[PopupManager.SEA_TRADE_POPUP]);
            SetPopupActivity(endGamePopup, GameManager.PopupManager.PopupsShown[PopupManager.END_GAME_POPUP]);
            SetPopupActivity(obligatoryActionInfoPopup, true);
        }

        private void SetPopupActivity(GameObject popup, bool doShow, float delay = 0.0f)
        {
            if (popup != null)
            {
                int playerId = GameManager.State.CurrentPlayerId;
                if (popup == landTradeAcceptPopup)
                {
                    playerId = GameManager.TradeManager.LandTradeOfferTarget;
                }
                else if (popup == thiefPayPopup)
                {
                    var thiefPayController = popup.GetComponent<ThiefPayPopupController>();
                    if (thiefPayController != null && thiefPayController.ActivePlayerId != -1)
                    {
                        playerId = thiefPayController.ActivePlayerId;
                    }
                }

                float targetZ = (playerId == 1 || playerId == 2) ? 180f : 0f;
                Vector3 currentEuler = popup.transform.localEulerAngles;
                popup.transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, targetZ);
            }

            if (delay == 0.0f) popup.SetActive(doShow);
            else
            {
                if (doShow && !popup.activeSelf) StartCoroutine(ShowPopupWithDelay(popup, delay));
                else popup.SetActive(doShow);
            }
        }
        private IEnumerator ShowPopupWithDelay(GameObject popup, float delay)
        {
            yield return new WaitForSeconds(delay);
            popup.SetActive(true);
        }
    }
}
