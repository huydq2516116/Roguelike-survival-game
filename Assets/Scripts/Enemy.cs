using SmallHedge.SoundManager;
using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    private int m_CurrentHealth;
    public int damage = 3;
    private bool e_IsMoving = false;
    private Vector3 e_MoveTarget;
    public float MoveSpeed = 5.0f;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }

        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        //remove enemy from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        //add it to the next cell
        targetCell.ContainedObject = this;
        m_Cell = coord;
        e_IsMoving = true;
        e_MoveTarget = board.CellToWorld(coord);
        animator.SetBool("Moving", e_IsMoving);
        

        return true;
    }

    void TurnHappened()
    {
        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = (int)playerCell.x - m_Cell.x;
        int yDist = (int)playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            //we are adjacent to the player, attack!
            GameManager.Instance.ChangeFood(-damage);
            GameManager.Instance.PlayerController.animator.SetTrigger("Hurt");
            animator.SetTrigger("Attack");
            SoundManager.PlaySound(SoundType.ENEMY, GameManager.Instance.audioSource);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    //if our move was not successful (so no move and not attack)
                    //we try to move along Y
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        //try to get closer in x

        //player to our right
        if (xDist > 0)
        {
            spriteRenderer.flipX = true;
            return MoveTo(m_Cell + Vector2Int.right);
        }

        //player to our left
        spriteRenderer.flipX = false;
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        //try to get closer in y

        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }

        //player below
        return MoveTo(m_Cell + Vector2Int.down);
    }
    private void Update()
    {
        if (e_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, e_MoveTarget, MoveSpeed * Time.deltaTime);
            if (transform.position == e_MoveTarget)
            {
                e_IsMoving = false;
                animator.SetBool("Moving", e_IsMoving);
            }
        }
    }
}
