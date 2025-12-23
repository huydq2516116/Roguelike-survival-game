using SmallHedge.SoundManager;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public BoardManager BoardManager;
	public PlayerController PlayerController;

	public TurnManager TurnManager { get; private set;}

	private int m_FoodAmount;

	public UIDocument UIDoc;
	private Label m_FoodLabel;
    private Label m_LevelLabel;

    public int m_CurrentLevel = 1;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    public AudioSource audioSource;
    public AudioSource backgroundAudioSource;

    private void Awake()
	{
        backgroundAudioSource = GetComponent<AudioSource>();
	   if (Instance != null)
	   {
		   Destroy(gameObject);
		   return;
	   }
	  
	   Instance = this;
	}

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        SoundManager.PlaySound(SoundType.MUSIC,backgroundAudioSource);

        
        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_LevelLabel = UIDoc.rootVisualElement.Q<Label>("LevelLabel");

        m_GameOverMessage.text = "Press Enter To Start";
        
    }
    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 50;
        BoardManager.minFood = 3;
        BoardManager.maxFood = 7;
        BoardManager.minWall = 6;
        BoardManager.maxWall = 10;
        BoardManager.minEnemy = 1;
        BoardManager.maxEnemy = 3;
        BoardManager.Width = 8;
        BoardManager.Height = 8;
        BoardManager.cam.orthographicSize = 5;

        m_FoodLabel.text = "Food : " + m_FoodAmount;
        m_LevelLabel.text = m_CurrentLevel.ToString("D2");

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    void OnTurnHappen()
	{
        ChangeFood(-1);
    }
    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        if (m_FoodAmount < 0) {m_FoodAmount=0;}
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels\n\n" + "Press Enter to restart";

        }
    }
    public void NewLevel()
    {
        
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        StartCoroutine(RunNextFrameNextLevel());


    }
    IEnumerator RunNextFrameNextLevel()
    {
        yield return null;
        m_CurrentLevel++;
        m_LevelLabel.text = m_CurrentLevel.ToString("D2");
    }

}


