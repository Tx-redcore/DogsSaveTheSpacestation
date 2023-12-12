using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer renderer;
    private Animator animator;
    private AnimatorClipInfo[] currentClipInfo;
    private string currentClipName;

    public LayerMask IgnoreMask;

    public int health = 100;
    public int speed = 3;
    public int jumpStrength = 1;
    public bool isFacingRight;
    public float attackRange = 5f;

    private int maxHealth = 100;
    private Vector3 spawnPosition;
    private int sprintBoost = 1;

    public AnimationClip idleSprite;
    public AnimationClip walkSprite;
    public AnimationClip deathSprite;
    public AnimationClip barkSprite;
    public AnimationClip hurtSprite;

    public delegate void OnHealthChange(int health);
    public static event OnHealthChange onHealthChange;

    public delegate void Died();
    public static event Died died;
    public bool isDead = false;

    public CurrentState state = CurrentState.Idle;
    public float speedRunSeconds = 0;
    private Vector2 tempVelocity = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 40;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        spawnPosition = this.transform.position;

        GameController.onRestart += OnRestart;
        PickUp.onPickUp += OnPickUp;
    }

    void FixedUpdate()
     {
        if (!isDead)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                if (sprintBoost == 1)
                    {
                    sprintBoost = 2;
                    }
                }
            if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                sprintBoost = 1;
                }

            var horizontalInput = Input.GetAxisRaw("Horizontal");
            tempVelocity.x = horizontalInput * speed * sprintBoost;

            if (Input.GetKeyDown(KeyCode.A))
            {
                isFacingRight = false;
                renderer.flipX = true;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                isFacingRight = true;
                renderer.flipX = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray2D ray = new Ray2D(mousePosition, Vector2.zero);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null && hit.collider.tag == "EnemyCollider")
                {
                    EnemyController ctrl = hit.collider.gameObject.GetComponentInParent<EnemyController>();
                    ctrl.TakeDamage(50);
                }

            }

            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
                {
                tempVelocity.y = jumpStrength * 100;
                state = CurrentState.Jump; 
                }

            rb.velocity = tempVelocity;
            Vector2 noVelocity = new Vector2(0, 0);

            if (rb.velocity == noVelocity)
                {
                state = CurrentState.Idle; 
                }

            if (rb.velocity != noVelocity)
                {
                state = CurrentState.Moving;
                }
            }
        else
        {
            state = CurrentState.Dead;
        }
    }

    void Update()
        {
        speedRunSeconds += Time.deltaTime;
        currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        currentClipName = currentClipInfo [0].clip.name;

        if (state == CurrentState.Moving)
        {
            //character is moving play walk
            if (currentClipName != walkSprite.name
                && (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) && !animator.IsInTransition(0))
                {
                animator.Play(walkSprite.name);
                }
        }

        if (state == CurrentState.Idle)
        {
            //character is standing still play idle
            if (currentClipName != idleSprite.name
                && (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) && !animator.IsInTransition(0))
                {
                animator.Play(idleSprite.name);
                }
        }

        if (state == CurrentState.Attack)
            {
            //character is moving play walk
            if (currentClipName != barkSprite.name
                && (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) && !animator.IsInTransition(0))
                {
                animator.Play(barkSprite.name);
                }
            }
        tempVelocity = new Vector2 (0, 0);
        }

    public void LateUpdate()
        {
        //Dead check
        if (health <= 0)
            {
            if (isDead == false)
                {
                isDead = true;
                //play death animation
                if (currentClipName != deathSprite.name)
                    {
                    animator.Play(deathSprite.name);
                    }
                }
            else
                {
                //after animation finnished send off death event
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                    {
                    //end of anim send event
                    died?.Invoke();
                    }
                }
            }
        }

    bool IsGrounded()
    {
        var groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 1.7f, ~IgnoreMask);
        return groundCheck.collider != null && groundCheck.collider.CompareTag("Ground");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        onHealthChange?.Invoke(health);
        if (currentClipName != hurtSprite.name)
        {
            animator.Play(hurtSprite.name);
        }
    }

    void OnPickUp(int amount, PickUpType type)
    {
        switch (type)
        {
            case PickUpType.Heal:
                health += amount;
                if (health > maxHealth) { health = maxHealth; } //reset health to not overflow
                onHealthChange?.Invoke(health);
                break;
            case PickUpType.Speed:
                speed += amount;
                //start corutine to remove speed amount after a certain time
                StartCoroutine(StartBoost(amount, type, 5));
                break;
            case PickUpType.Jump:
                jumpStrength += amount;
                //start corutine to remove jump amount after a certain time
                StartCoroutine(StartBoost(amount, type, 5));
                break;
        }
    }

    void OnRestart()
    {
        //reset health
        isDead = false;
        state = CurrentState.Idle;
        health = maxHealth;
        onHealthChange?.Invoke(health);
        //reset position
        transform.position = spawnPosition;
        //reset scene
        //done from other scripts

        //reset speedrunsec
        speedRunSeconds = 0;
    }

    IEnumerator StartBoost(int amount, PickUpType type, int seconds)
    {
        yield return new WaitForSeconds(seconds);
        //remove boost after seconds
        switch (type)
        {
            case PickUpType.Speed:
                speed -= amount;
                break;
            case PickUpType.Jump:
                jumpStrength -= amount;
                break;
        }

        yield break;
    }

    public enum CurrentState
    {
        Idle,
        Moving,
        Jump,
        Attack,
        Dead
    } 
}