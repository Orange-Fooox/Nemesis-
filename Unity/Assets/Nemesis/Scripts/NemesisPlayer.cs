using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NemesisHeroAnimator))]
public class NemesisPlayer : MonoBehaviour
{
    public float moveSpeed = 4.2f;
    public float runSpeed = 6.3f;
    public int maxHealth = 100;
    public int health = 100;
    public int damage = 28;
    public float attackRange = 1.25f;
    public float attackCooldown = 0.42f;
    public float dodgeSpeed = 10f;
    public float dodgeDuration = 0.18f;

    private Rigidbody2D body;
    private NemesisHeroAnimator heroAnimator;
    private Vector2 move;
    private Vector2 lastFacing = Vector2.down;
    private Vector3 spawnPosition;
    private float nextAttack;
    private bool dodging;
    private bool running;
    private bool dead;
    private float dodgeUntil;
    private Vector2 dodgeDir;
    private SpriteRenderer sr;

    public static NemesisPlayer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        body = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        heroAnimator = GetComponent<NemesisHeroAnimator>();
        health = maxHealth;
        spawnPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (sr != null)
            sr.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 100f) + 7;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || dead) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        move = new Vector2(horizontal + vertical * 2f, -horizontal + vertical * 2f).normalized;
        running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (move.sqrMagnitude > 0.025f) lastFacing = move;

        if (Input.GetKeyDown(KeyCode.Space) && !dodging && move.sqrMagnitude > 0.01f)
        {
            dodging = true;
            dodgeDir = move;
            dodgeUntil = Time.time + dodgeDuration;
            heroAnimator?.PlayDodge();
            StartCoroutine(DodgeFlash());
        }

        if (dodging && Time.time >= dodgeUntil) dodging = false;
        heroAnimator?.SetMovement(dodging ? dodgeDir : move, running);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) Attack();
        if (Input.GetKeyDown(KeyCode.E)) Interact();
    }

    private void FixedUpdate()
    {
        if (body == null) return;
        float speed = running ? runSpeed : moveSpeed;
        body.linearVelocity = dead ? Vector2.zero : (dodging ? dodgeDir * dodgeSpeed : move * speed);
    }

    private IEnumerator DodgeFlash()
    {
        while (dodging && !dead)
        {
            if (sr != null) sr.color = new Color(0.65f, 0.58f, 0.78f, 0.78f);
            yield return null;
        }
        if (sr != null) sr.color = Color.white;
    }

    private void Attack()
    {
        if (dead || Time.time < nextAttack) return;
        nextAttack = Time.time + attackCooldown;
        heroAnimator?.PlayAttack();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            NemesisEnemy enemy = hit.GetComponent<NemesisEnemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
        NemesisHUD.Instance?.SetMessage("УДАР");
    }

    private void Interact()
    {
        if (dead) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            NemesisChest chest = hit.GetComponent<NemesisChest>();
            if (chest != null) { chest.Open(); return; }
            NemesisShrine shrine = hit.GetComponent<NemesisShrine>();
            if (shrine != null) { shrine.Activate(); return; }
            NemesisDungeonPortal portal = hit.GetComponent<NemesisDungeonPortal>();
            if (portal != null) { portal.Enter(); return; }
            NemesisNPC npc = hit.GetComponent<NemesisNPC>();
            if (npc != null) { npc.Talk(); return; }
        }
        NemesisHUD.Instance?.SetMessage("РЯДОМ НИЧЕГО НЕТ");
    }

    public void ResetForNewGame()
    {
        dead = false;
        dodging = false;
        running = false;
        health = maxHealth;
        transform.position = spawnPosition;
        body.linearVelocity = Vector2.zero;
        heroAnimator?.ResetAnimation();
        NemesisHUD.Instance?.SetHealth(health, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (dodging || dead) return;
        health = Mathf.Max(0, health - amount);
        NemesisHUD.Instance?.SetHealth(health, maxHealth);
        if (health <= 0)
        {
            dead = true;
            dodging = false;
            move = Vector2.zero;
            heroAnimator?.PlayDeath();
            body.linearVelocity = Vector2.zero;
            StartCoroutine(RespawnAfterDeath());
        }
        else heroAnimator?.PlayHurt();
    }

    private IEnumerator RespawnAfterDeath()
    {
        yield return new WaitForSeconds(1.35f);
        health = maxHealth;
        transform.position = spawnPosition;
        body.linearVelocity = Vector2.zero;
        dead = false;
        heroAnimator?.ResetAnimation();
        NemesisHUD.Instance?.SetHealth(health, maxHealth);
        NemesisHUD.Instance?.SetMessage("Вы пробуждаетесь у последнего святилища.");
    }
}
