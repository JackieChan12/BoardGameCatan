using UnityEngine;
using UnityEngine.UI;
using DataStorage;
using Board;
using Assets.Scripts.Board.States;
using static Assets.Scripts.Board.States.FieldState;
using static Assets.Scripts.Board.States.JunctionState;

namespace UI.Game
{
    /// <summary>
    /// Class for coordinating PlayerUIController instances and managing their interactability based on turn,
    /// as well as displaying shared Selected Element UI.
    /// </summary>
    public class StaticUIUpdater : MonoBehaviour
    {
        [Tooltip("ID của người chơi sở hữu bộ UI này. Để -1 nếu hiển thị theo người chơi hiện tại.")]
        public int playerId = -1;

        private PlayerUIController[] playerUIs;

        [Header("Selected Element UI")][Space(5)]
        [Tooltip("Selected element owner text")][SerializeField] 
        private Text selectedElementAdditionalInfo;
        [Tooltip("Selected element name text")][SerializeField]
        private Text selectedElementName;

        // Board elements names shown in UI
        private string junctionEmptyName;
        private string junctionVillageName;
        private string junctionCityName;
        private string pathName;
        private string clayFieldName;
        private string desertFieldName;
        private string fieldFieldName;
        private string forestFieldName;
        private string mountainsFieldName;
        private string pastureFieldName;

        // When we point a field, it gives info which resource is supplied by this field
        private string suppliedPrefix;
        private string suppliedWood;
        private string suppliedWheat;
        private string suppliedClay;
        private string suppliedIron;
        private string suppliedWool;
        private string suppliedNothing;
        
        private string ownerPrefix;
        private string noOwner;

        void Start()
        {
            junctionEmptyName = "Giao lộ trống";
            junctionVillageName = "Làng";
            junctionCityName = "Thành phố";
            pathName = "Đường đi";
            clayFieldName = "Mỏ đất sét";
            desertFieldName = "Sa mạc";
            fieldFieldName = "Đồng lúa mì";
            forestFieldName = "Rừng gỗ";
            mountainsFieldName = "Mỏ quặng";
            pastureFieldName = "Đồng cỏ chăn cừu";

            suppliedPrefix = "Cung cấp: ";
            suppliedWood = "gỗ";
            suppliedWheat = "lúa mì";
            suppliedClay = "đất sét";
            suppliedIron = "quặng";
            suppliedWool = "lông cừu";
            suppliedNothing = "không có gì";

            ownerPrefix = "Chủ sở hữu: ";
            noOwner = "Chưa có chủ sở hữu";

            // Find all PlayerUIController instances in the scene
            playerUIs = FindObjectsOfType<PlayerUIController>();
        }

        void Update()
        {
            // Manage player UI interactability based on turn
            if (playerUIs != null)
            {
                foreach (var playerUI in playerUIs)
                {
                    playerUI.SetInteractable(playerUI.playerId == GameManager.State.CurrentPlayerId);
                }
            }

            UpdateSelectedElement();
        }

        /// <summary>
        /// Updates UI information about selected element
        /// </summary>
        private void UpdateSelectedElement()
        {
            if (selectedElementName == null || selectedElementAdditionalInfo == null)
            {
                return;
            }

            if (GameManager.Selected.Pointed == null)
            {
                selectedElementName.text = "";
                selectedElementAdditionalInfo.text = "";
                return;
            }
            
            // Setting the object text
            if (GameManager.Selected.Pointed as FieldElement != null)
            {
                var element = (FieldElement)GameManager.Selected.Pointed;
                selectedElementName.text = ((FieldState)element.State).type switch
                {
                    FieldType.Forest => forestFieldName,
                    FieldType.Pasture => pastureFieldName,
                    FieldType.Field => fieldFieldName,
                    FieldType.Hills => clayFieldName,
                    FieldType.Mountains => mountainsFieldName,
                    FieldType.Desert => desertFieldName,
                    _ => selectedElementName.text
                };
            }
            else if (GameManager.Selected.Pointed as JunctionElement != null)
            {
                var element = (JunctionElement)GameManager.Selected.Pointed;
                selectedElementName.text = ((JunctionState)element.State).type switch
                {
                    JunctionType.None => junctionEmptyName,
                    JunctionType.Village => junctionVillageName,
                    JunctionType.City => junctionCityName,
                    _ => selectedElementName.text
                };
            }
            else if (GameManager.Selected.Pointed as PathElement != null)
            {
                selectedElementName.text = pathName;
            }
            
            // Setting additional info
            selectedElementAdditionalInfo.text = "";
            foreach (var player in GameManager.State.Players)
            {
                // For field additional info is which resource it supplies
                if (GameManager.Selected.Pointed as FieldElement != null)
                {
                    var element = (FieldElement)GameManager.Selected.Pointed;
                    selectedElementAdditionalInfo.text = ((FieldState)element.State).type switch
                    {
                        FieldType.Forest => suppliedPrefix + suppliedWood,
                        FieldType.Pasture => suppliedPrefix + suppliedWool,
                        FieldType.Field => suppliedPrefix + suppliedWheat,
                        FieldType.Hills => suppliedPrefix + suppliedClay,
                        FieldType.Mountains => suppliedPrefix + suppliedIron,
                        FieldType.Desert => suppliedNothing,
                        _ => selectedElementAdditionalInfo.text
                    };
                    break;
                }
                
                // For path and junction additional info is an owner
                if (GameManager.Selected.Pointed as JunctionElement != null)
                {
                    var element = (JunctionElement)GameManager.Selected.Pointed;
                    if (!player.OwnsBuilding(element.State.id))
                    {
                        selectedElementAdditionalInfo.text = noOwner;
                        continue;
                    }
                    selectedElementAdditionalInfo.text = ownerPrefix + player.name;
                    break;
                }

                if (GameManager.Selected.Pointed as PathElement != null)
                {
                    var element = (PathElement)GameManager.Selected.Pointed;
                    if (!player.OwnsPath(element.State.id))
                    {
                        selectedElementAdditionalInfo.text = noOwner;
                        continue;
                    }
                    selectedElementAdditionalInfo.text = ownerPrefix + player.name;
                    break;
                }
            }
        }
    }
}