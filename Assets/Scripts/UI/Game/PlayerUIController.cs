using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DataStorage;
using Board;
using Assets.Scripts.Board.States;
using static Assets.Scripts.Board.States.FieldState;
using static Assets.Scripts.Board.States.JunctionState;

namespace UI.Game
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayerUIController : MonoBehaviour
    {
        [Tooltip("ID của người chơi sở hữu bộ UI này (0, 1, 2, 3...)")]
        public int playerId;

        [Header("Player Info")][Space(5)]
        [Tooltip("Current player color image")][SerializeField]
        private Image playerColorImage;
        [Tooltip("Current player name text")][SerializeField]
        private Text playerNameText;
        [Tooltip("Current player score text (Optional)")][SerializeField]
        private Text scoreText;

        [Header("Resources")][Space(5)]
        [Tooltip("Wood resource text")][SerializeField]
        private Text woodResourceText;
        [Tooltip("Clay resource text")][SerializeField]
        private Text clayResourceText;
        [Tooltip("Wool resource text")][SerializeField]
        private Text woolResourceText;
        [Tooltip("Iron resource text")][SerializeField]
        private Text ironResourceText;
        [Tooltip("Wheat resource text")][SerializeField] 
        private Text wheatResourceText;

        [Header("Child UI Components")][Space(5)]
        [Tooltip("Actions tab component")]
        public ActionsContentNavigation actionsNav;
        [Tooltip("Cards tab component")]
        public CardsContentNavigation cardsNav;
        
        [Header("Incoming Resources")] [Space(5)] 
        [Tooltip("Incoming resource motion speed")] [SerializeField]
        private float incomingResourceMotionSpeed = 250f;
        [Tooltip("Incoming resource text start height")] [SerializeField]
        private Vector3 incomingResourceEndPosition = new Vector3(0, 150f, 0);
        [Tooltip("Incoming wood text")][SerializeField]
        private Text incomingWoodText;
        [Tooltip("Incoming clay text")][SerializeField]
        private Text incomingClayText;
        [Tooltip("Incoming wool text")][SerializeField]
        private Text incomingWoolText;
        [Tooltip("Incoming iron text")][SerializeField]
        private Text incomingIronText;
        [Tooltip("Incoming wheat text")][SerializeField]
        private Text incomingWheatText;

        private CanvasGroup canvasGroup;

        void Awake()
        {
            // Đảm bảo CanvasGroup gốc trên Player luôn hoạt động
            var rootCanvasGroup = GetComponent<CanvasGroup>();
            if (rootCanvasGroup != null)
            {
                rootCanvasGroup.interactable = true;
            }

            // Tìm CanvasGroup trên SlidingUI để chỉ khóa các nút chức năng, giữ các nút chuyển Tab click được
            var slidingUITrans = transform.Find("Tabs/SlidingUI");
            if (slidingUITrans != null)
            {
                canvasGroup = slidingUITrans.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = slidingUITrans.gameObject.AddComponent<CanvasGroup>();
                }
            }
            else
            {
                canvasGroup = rootCanvasGroup;
            }

            // Tự động tìm và gán các component con
            var staticUI = GetComponentInChildren<StaticUIUpdater>(true);
            if (staticUI != null)
            {
                staticUI.playerId = playerId;
            }

            actionsNav = GetComponentInChildren<ActionsContentNavigation>(true);
            if (actionsNav != null)
            {
                actionsNav.playerId = playerId;
            }

            cardsNav = GetComponentInChildren<CardsContentNavigation>(true);
            if (cardsNav != null)
            {
                cardsNav.playerId = playerId;
            }

            var limits = GetComponentInChildren<BuildingsLimits>(true);
            if (limits != null)
            {
                limits.playerId = playerId;
            }

            // Tự động tìm các Text hoạt ảnh tài nguyên
            if (incomingWoodText == null) incomingWoodText = transform.Find("Resources/Wood/Info")?.GetComponent<Text>();
            if (incomingClayText == null) incomingClayText = transform.Find("Resources/Clay/Info")?.GetComponent<Text>();
            if (incomingWoolText == null) incomingWoolText = transform.Find("Resources/Wool/Info")?.GetComponent<Text>();
            if (incomingIronText == null) incomingIronText = transform.Find("Resources/Iron/Info")?.GetComponent<Text>();
            if (incomingWheatText == null) incomingWheatText = transform.Find("Resources/Wheat/Info")?.GetComponent<Text>();

            // Tự động tìm Text điểm số nếu có con tên "Score" hoặc "Points"
            if (scoreText == null)
            {
                var scoreGo = transform.Find("Score") ?? transform.Find("Points") ?? transform.Find("CurrentPlayerColor/Score");
                if (scoreGo != null)
                {
                    scoreText = scoreGo.GetComponent<Text>();
                }
            }
        }

        void Update()
        {
            if (GameManager.State == null || GameManager.State.Players == null)
            {
                return;
            }

            if (playerId < 0 || playerId >= GameManager.State.Players.Length || GameManager.State.Players[playerId] == null)
            {
                gameObject.SetActive(false);
                return;
            }

            UpdatePlayerInfo();
            UpdateResources();

            // Quản lý và kích hoạt cập nhật các component con tương ứng
            if (actionsNav != null)
            {
                actionsNav.UpdateUI();
            }

            if (cardsNav != null)
            {
                cardsNav.UpdateUI();
            }

            // Mỗi người chơi tự quản lý hiệu ứng tài nguyên bay lên của mình
            CheckIncomingResources();
        }

        /// <summary>
        /// Bật/tắt khả năng tương tác trên toàn bộ bộ UI này
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
            }
        }

        private void UpdatePlayerInfo()
        {
            if (GameManager.State.Players == null || playerId >= GameManager.State.Players.Length || GameManager.State.Players[playerId] == null)
            {
                return;
            }

            var player = GameManager.State.Players[playerId];
            var points = player.score.GetPoints();

            if (scoreText != null)
            {
                if (playerNameText != null)
                {
                    playerNameText.text = player.name;
                }
                scoreText.text = $"{points}đ";
            }
            else
            {
                if (playerNameText != null)
                {
                    playerNameText.text = $"{player.name} ({points}đ)";
                }
            }

            if (playerColorImage != null)
            {
                playerColorImage.color = player.color switch
                {
                    Player.Player.Color.Blue => Color.blue,
                    Player.Player.Color.Red => Color.red,
                    Player.Player.Color.White => Color.white,
                    Player.Player.Color.Yellow => Color.yellow,
                    _ => playerColorImage.color
                };
            }
        }

        private void UpdateResources()
        {
            if (GameManager.State.Players == null || playerId >= GameManager.State.Players.Length || GameManager.State.Players[playerId] == null)
            {
                return;
            }

            var resources = GameManager.State.Players[playerId].resources.GetResourcesNumber();

            if (woodResourceText != null) woodResourceText.text = resources[Player.Resources.ResourceType.Wood].ToString();
            if (clayResourceText != null) clayResourceText.text = resources[Player.Resources.ResourceType.Clay].ToString();
            if (woolResourceText != null) woolResourceText.text = resources[Player.Resources.ResourceType.Wool].ToString();
            if (ironResourceText != null) ironResourceText.text = resources[Player.Resources.ResourceType.Iron].ToString();
            if (wheatResourceText != null) wheatResourceText.text = resources[Player.Resources.ResourceType.Wheat].ToString();
        }

        private void CheckIncomingResources()
        {
            int i = 0;
            while (i < GameManager.ResourceManager.IncomingResourcesRequests.Count)
            {
                var req = GameManager.ResourceManager.IncomingResourcesRequests[i];
                if (req.playerId == playerId)
                {
                    StartCoroutine(ShowIncomingResource(req.type, req.number));
                    GameManager.ResourceManager.IncomingResourcesRequests.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private IEnumerator ShowIncomingResource(FieldType type, int number)
        {
            var sign = number >= 0 ? "+" : "";
            switch(type)
            {
                case FieldType.Forest:
                    if (incomingWoodText == null) break;
                    incomingWoodText.text = $"{sign}{number.ToString()}";
                    incomingWoodText.gameObject.transform.localPosition = new Vector3(0,0,0);
                    incomingWoodText.gameObject.SetActive(true);
                    while (incomingWoodText.gameObject.transform.localPosition.y < incomingResourceEndPosition.y)
                    {
                        incomingWoodText.gameObject.transform.localPosition +=
                            new Vector3(0, incomingResourceMotionSpeed * Time.deltaTime, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    incomingWoodText.gameObject.SetActive(false);
                    break;
                case FieldType.Pasture:
                    if (incomingWoolText == null) break;
                    incomingWoolText.text = $"{sign}{number.ToString()}";
                    incomingWoolText.gameObject.transform.localPosition = new Vector3(0,0,0);
                    incomingWoolText.gameObject.SetActive(true);
                    while (incomingWoolText.gameObject.transform.localPosition.y < incomingResourceEndPosition.y)
                    {
                        incomingWoolText.gameObject.transform.localPosition +=
                            new Vector3(0, incomingResourceMotionSpeed * Time.deltaTime, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    incomingWoolText.gameObject.SetActive(false);
                    break;
                case FieldType.Field:
                    if (incomingWheatText == null) break;
                    incomingWheatText.text = $"{sign}{number.ToString()}";
                    incomingWheatText.gameObject.transform.localPosition = new Vector3(0,0,0);
                    incomingWheatText.gameObject.SetActive(true);
                    while (incomingWheatText.gameObject.transform.localPosition.y < incomingResourceEndPosition.y)
                    {
                        incomingWheatText.gameObject.transform.localPosition +=
                            new Vector3(0, incomingResourceMotionSpeed * Time.deltaTime, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    incomingWheatText.gameObject.SetActive(false);
                    break;
                case FieldType.Hills:
                    if (incomingClayText == null) break;
                    incomingClayText.text = $"{sign}{number.ToString()}";
                    incomingClayText.gameObject.transform.localPosition = new Vector3(0,0,0);
                    incomingClayText.gameObject.SetActive(true);
                    while (incomingClayText.gameObject.transform.localPosition.y < incomingResourceEndPosition.y)
                    {
                        incomingClayText.gameObject.transform.localPosition +=
                            new Vector3(0, incomingResourceMotionSpeed * Time.deltaTime, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    incomingClayText.gameObject.SetActive(false);
                    break;
                case FieldType.Mountains:
                    if (incomingIronText == null) break;
                    incomingIronText.text = $"{sign}{number.ToString()}";
                    incomingIronText.gameObject.transform.localPosition = new Vector3(0,0,0);
                    incomingIronText.gameObject.SetActive(true);
                    while (incomingIronText.gameObject.transform.localPosition.y < incomingResourceEndPosition.y)
                    {
                        incomingIronText.gameObject.transform.localPosition +=
                            new Vector3(0, incomingResourceMotionSpeed * Time.deltaTime, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    incomingIronText.gameObject.SetActive(false);
                    break;
            }
        }
    }
}

