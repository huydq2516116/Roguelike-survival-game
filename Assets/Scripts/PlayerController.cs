using SmallHedge.SoundManager;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private bool m_IsGameOver;
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private InputAction moveAction;
    private bool movedLastFrame = false;
    public float MoveSpeed = 5.0f;

    private bool m_IsMoving;
    private Vector3 m_MoveTarget;

    private SpriteRenderer spriteRenderer;
    public Animator animator;
    public Vector2Int Cell;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    public void GameOver()
    {
        SoundManager.PlaySound(SoundType.DOWN);
        m_IsGameOver = true;
    }

    void Start()
    {
        m_IsGameOver = true;
        // Kiểm tra xem Input System có hoạt động không
        if (InputSystem.actions == null)
        {
            Debug.LogError("LỖI: Chưa bật Input System! Xem hướng dẫn bên dưới.");
            return;
        }

        moveAction = InputSystem.actions.FindAction("Move");

        if (moveAction == null)
        {
            Debug.LogError("LỖI: Không tìm thấy hành động 'Move'.");
        }
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell,true);
        Cell = cell;
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        m_CellPosition = cell;
        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
            Cell = m_CellPosition;
        }
        else
        {
            m_IsMoving = true;
            SoundManager.PlaySound(SoundType.FOOTSTEP,GameManager.Instance.audioSource);
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
            Cell = m_CellPosition;
        }
        animator.SetBool("Moving", m_IsMoving);
    }

    public void Init()
    {
        m_IsMoving = false;
        m_IsGameOver = false;
    }
    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }
        // Nếu bị lỗi ở Start, dừng Update để không báo lỗi đỏ liên tục
        if (moveAction == null || m_Board == null) return;
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }
            animator.SetBool("Moving", m_IsMoving);
            
            return;
        }
        // Logic di chuyển
        if (!movedLastFrame && moveValue != Vector2.zero)
        {
            Vector2Int newCellTarget = m_CellPosition;
            bool hasMoved = false;

            if (moveValue.y > 0) { newCellTarget.y += 1; hasMoved = true; }
            else if (moveValue.y < 0) { newCellTarget.y -= 1; hasMoved = true; }
            else if (moveValue.x > 0) { newCellTarget.x += 1; hasMoved = true; spriteRenderer.flipX = false; }
            else if (moveValue.x < 0) { newCellTarget.x -= 1; hasMoved = true; spriteRenderer.flipX = true; }

            if (hasMoved)
            {
                BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);
                if (cellData != null && cellData.Passable)
                {
                    if (GameManager.Instance != null) GameManager.Instance.TurnManager.Tick();

                    if (cellData.ContainedObject == null)
                    {
                        MoveTo(newCellTarget,false);
                    }
                    else if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        MoveTo(newCellTarget,false);
                    }
                    movedLastFrame = true;
                }
            }
        }
        else if (moveValue == Vector2.zero)
        {
            movedLastFrame = false;
        }
    }
}