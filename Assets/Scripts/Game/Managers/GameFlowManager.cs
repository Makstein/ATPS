using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Parameters")] [Tooltip("Duration of the fade-to-black at the end of the game")]
        public float EndSceneLoadDelay = 3f;

        [Tooltip("The canvas group of the fade-to-black screen")]
        public CanvasGroup EndGameFadeCanvasGroup;

        [Header("Win")] [Tooltip("The scene will be loaded when wining")]
        public string WinSceneName = "SimpleScene";

        [Tooltip("Duration of delay before fade-to-black, if wining")]
        public float DelayBeforeFadeToBlack = 4f;

        [Tooltip("Win game message")] public string WinGameMessage;

        [Tooltip("Duration of delay before the win message")]
        public float DelayBeforeWinMessage;

        [Tooltip("Sound played on win")] public AudioClip VictorySound;

        [Header("Lose")] [Tooltip("The scene name of which will be loaded when losing")]
        public string LoseSceneName = "SimpleScene";

        private string m_SceneToLoad;

        private float m_TimeLoadEndGameScene;

        public bool GameIsEnding { get; private set; }

        private void Awake()
        {
            EventManager.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        private void Start()
        {
            //init settings
        }

        private void Update()
        {
            if (!GameIsEnding) return;

            //todo: make an EndGameFadeCanvasGroup and enable the lines below
            // var timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / EndSceneLoadDelay;
            // EndGameFadeCanvasGroup.alpha = timeRatio;

            // todo: audio
            //AudioUtility.SetMasterVolume(1 - timeRatio);

            if (Time.time <= m_TimeLoadEndGameScene) return;

            SceneManager.LoadScene(m_SceneToLoad);
            GameIsEnding = false;
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        private void OnAllObjectivesCompleted(AllObjectivesCompletedEvent evt)
        {
            EndGame(true);
        }

        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            EndGame(false);
        }

        private void EndGame(bool win)
        {
            //Unlock the cursor to enable click
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //Load the appropriate end scene after a delay
            GameIsEnding = true;

            //todo: make an EndGameFadeCanvasGroup and enable the line below
            //EndGameFadeCanvasGroup.gameObject.SetActive(true);

            if (win)
            {
                m_SceneToLoad = WinSceneName;
                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay + DelayBeforeFadeToBlack;

                // todo: play a sound on win and enable the lines below
                // var audioSource = gameObject.AddComponent<AudioSource>();
                // audioSource.clip = VictorySound;
                // audioSource.playOnAwake = false;
                // audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                // audioSource.PlayScheduled(AudioSettings.dspTime + DelayBeforeWinMessage);

                var displayMessage = Events.DisplayMessageEvent;
                displayMessage.message = WinGameMessage;
                displayMessage.DelayBeforeDisplay = DelayBeforeWinMessage;
                EventManager.Broadcast(displayMessage);
            }
            else
            {
                m_SceneToLoad = LoseSceneName;
                m_TimeLoadEndGameScene = Time.time + EndSceneLoadDelay;
            }
        }
    }
}