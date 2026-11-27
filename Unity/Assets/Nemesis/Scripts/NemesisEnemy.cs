using UnityEngine;

public class NemesisEnemy : MonoBehaviour
{
    public int health = 65;
    public int damage = 12;
    public float aggroRadius = 3.6f;
    public float attackRadius = 0.85f;
    public float speed = 1.55f;

    private bool alerted;
    private bool isDead;
    private float nextHit;
    private float nextPath;
    private Rigidbody2D body;
    private NemesisEnemyAnimator animator;
    private NemesisDungeonNavigation navigation;
    private Vector2[] path = new Vector2[0];
    private int pathIndex;
    private Vector2Int lastTargetCell = new Vector2Int(int.MinValue, int.MinValue);

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<NemesisEnemyAnimator>();
        if (animator == null) animator = gameObject.AddComponent<NemesisEnemyAnimator>();
        navigation = FindFirstObjectByType<NemesisDungeonNavigation>();
    }

    void LateUpdate()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 100f) + 7;
    }

    void Update()
    {
        if (isDead || NemesisPlayer.Instance == null || body == null) return;
        Vector2 target = NemesisPlayer.Instance.transform.position;
        float distance = Vector2.Distance(transform.position, target);
        if (!alerted && distance <= aggroRadius)
        {
            alerted = true;
            NemesisHUD.Instance?.SetMessage("ПРОТИВНИК ЗАМЕТИЛ ВАС");
        }

        if (!alerted)
        {
            body.linearVelocity = Vector2.zero;
            animator?.SetMovement(Vector2.zero);
            return;
        }

        if (distance <= attackRadius)
        {
            body.linearVelocity = Vector2.zero;
            animator?.SetMovement(Vector2.zero);
            if (Time.time >= nextHit)
            {
                nextHit = Time.time + 1.25f;
                animator?.PlayAttack(target - (Vector2)transform.position);
                NemesisPlayer.Instance.TakeDamage(damage);
            }
            return;
        }

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        if (navigation != null)
        {
            Vector2Int targetCell = navigation.WorldToCell(target);
            if (Time.time >= nextPath || targetCell != lastTargetCell)
            {
                path = navigation.FindPath(transform.position, target);
                pathIndex = 0;
                lastTargetCell = targetCell;
                nextPath = Time.time + 0.55f;
            }
            if (pathIndex < path.Length)
            {
                Vector2 waypoint = path[pathIndex];
                if (Vector2.Distance(transform.position, waypoint) < 0.16f && pathIndex < path.Length - 1)
                {
                    pathIndex++;
                    waypoint = path[pathIndex];
                }
                direction = (waypoint - (Vector2)transform.position).normalized;
            }
        }

        animator?.SetMovement(direction);
        body.linearVelocity = direction * speed;
    }

    public void TakeDamage(int value)
    {
        if (isDead) return;
        health = Mathf.Max(0, health - value);
        animator?.PlayHurt();

        if (health <= 0)
        {
            isDead = true;
            body.linearVelocity = Vector2.zero;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            NemesisHUD.Instance?.AddXP(name.Contains("Hollow Knight") ? 250 : 25);
            if (name.Contains("Hollow Knight")) NemesisHUD.Instance?.SetMessage("ПОЛЫЙ РЫЦАРЬ ПАЛ. ПУТЬ ОТКРЫТ.");
            if (animator != null) animator.PlayDeath();
            else Destroy(gameObject);
        }
    }
}
