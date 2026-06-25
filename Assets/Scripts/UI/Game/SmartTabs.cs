using Board.States;
using DataStorage;
using UnityEngine;

namespace UI.Game
{
    public class SmartTabs : MonoBehaviour
    {
        private GameState.MovingMode lastMovingUserMode;
        private dynamic lastSelectedElement;
        private TabsUINavigation.ActiveContent lastActiveContent;
        
        private bool canReturnToDefaultTab;
        
       
        private bool wasActiveLastFrame;
        
       
        private PlayerUIController playerUI;

        void Start()
        {
            playerUI = GetComponentInParent<PlayerUIController>();

            //Destiny: Saving last set elements to check if variable has changed
            lastMovingUserMode = GameManager.State.MovingUserMode;
            lastSelectedElement = GameManager.Selected.Element;
            lastActiveContent = GetComponent<TabsUINavigation>().activeContent;
            canReturnToDefaultTab = false;
            wasActiveLastFrame = (playerUI != null && playerUI.playerId == GameManager.State.CurrentPlayerId);

            //Destiny: First invoke of smart tabs
            InvokeSmartActions();
        }

        void Update()
        {
            // Chỉ cho phép SmartTabs hoạt động nếu đây là người chơi đang trong lượt
            if (playerUI == null)
            {
                playerUI = GetComponentInParent<PlayerUIController>();
            }
            
            bool isActiveTurn = (playerUI != null && playerUI.playerId == GameManager.State.CurrentPlayerId);
            if (!isActiveTurn)
            {
                wasActiveLastFrame = false;
                return;
            }

            if (!wasActiveLastFrame)
            {
                wasActiveLastFrame = true;
                // Force a different value for lastMovingUserMode to trigger update on new turn activation
                lastMovingUserMode = GameManager.State.MovingUserMode == GameState.MovingMode.ThrowDice ? 
                    GameState.MovingMode.Normal : GameState.MovingMode.ThrowDice;
            }

            //Destiny: Lowest priority - smart tabs on user moving mode
            if (lastMovingUserMode != GameManager.State.MovingUserMode)
            {
                lastMovingUserMode = GameManager.State.MovingUserMode;
                InvokeSmartActions();
            }
            
            //Destiny: Average priority - smart tabs on selecting element to build
            if (lastSelectedElement != GameManager.Selected.Element)
            {
                lastSelectedElement = GameManager.Selected.Element;
                if(GameManager.Selected.Element != null)
                {
                    InvokeSmartElementInteraction();
                }
            }
            
            //Destiny: Highest priority - smart tabs on popups
            if(!GameManager.PopupManager.CheckIfWindowShown())
            {
                //Destiny: If it is set, tabs returns to content which was set before
                if (canReturnToDefaultTab)
                {
                    GetComponent<TabsUINavigation>().activeContent = TabsUINavigation.ActiveContent.None;
                    switch(lastActiveContent)
                    {
                        case TabsUINavigation.ActiveContent.Actions:
                            GetComponent<TabsUINavigation>().OnActionButtonClick();
                            break;
                        case TabsUINavigation.ActiveContent.Cards:
                            GetComponent<TabsUINavigation>().OnCardsButtonClick();
                            break;
                    }
                    canReturnToDefaultTab = false;
                }
            }
        }

        /// <summary>
        /// Smart tabs opening actions tab on user mode
        /// </summary>
        private void InvokeSmartActions()
        {
            if (GameManager.State.MovingUserMode is
                GameState.MovingMode.ThrowDice or GameState.MovingMode.MovingThief or
                GameState.MovingMode.OnePathForFree or GameState.MovingMode.TwoPathsForFree or
                GameState.MovingMode.BuildPath or GameState.MovingMode.BuildVillage or GameState.MovingMode.EndTurn ||
                (GameManager.State.MovingUserMode == GameState.MovingMode.Normal &&
                 (GameManager.State.Mode == GameState.CatanMode.Advanced ||
                  GameManager.State.BasicMovingUserMode is GameState.BasicMovingMode.TradePhase or GameState.BasicMovingMode.BuildPhase)))
            {
                GetComponent<TabsUINavigation>().activeContent = TabsUINavigation.ActiveContent.None;
                GetComponent<TabsUINavigation>().OnActionButtonClick();
            }
        }

        /// <summary>
        /// Smart tabs opening actions tab on selecting element
        /// </summary>
        private void InvokeSmartElementInteraction()
        {
            GetComponent<TabsUINavigation>().activeContent = TabsUINavigation.ActiveContent.None;
            GetComponent<TabsUINavigation>().OnActionButtonClick();
        }
    }
}
