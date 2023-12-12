using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public LayerMask playerLayer;

    public int patrolRange = 2;
    public float angelSpeedWalking = 2f;
    public float angelSpeedAttack = 10f;
    public float lookDistance = 5f;
    public float killDistance = 1.5f;
    public int health = 100;

    public AnimationClip idleSprite;
    public AnimationClip walkSprite;
    public AnimationClip deathSprite;
    public AnimationClip attackSprite;

    private bool isFrozen = false;
    private bool canSeePlayer = false;
    private bool isRightOfPlayer;
    private bool isDead = false;
    private float oldPosition;
    private Vector3 spawnPosition;

    private Rigidbody2D rb;
    private PlayerController player;
    private SpriteRenderer renderer;
    private Animator animator;

    private AnimatorClipInfo[] currentClipInfo;
    private string currentClipName;

    private EnemyState currentState = EnemyState.Patrolling;
    private int randomInt;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        spawnPosition = this.transform.position;
        randomInt = Random.Range(-1, 2);
    }

    void FixedUpdate()
    {
        currentClipInfo = animator.GetCurrentAnimatorClipInfo(0); //Fetch the current Animation clip information for the base layer
        currentClipName = currentClipInfo[0].clip.name; //Access the Animation clip name
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (transform.position.x > oldPosition)
        {
            renderer.flipX = false;
        } else
        {
            renderer.flipX = true;
        }

        if ( transform.position.x > player.transform.position.x ) 
        { 
            isRightOfPlayer = true; 
        } 
        else 
        { 
            isRightOfPlayer = false;
        }

        if (Physics2D.Linecast(transform.position, player.transform.position, ~playerLayer))
        {
            canSeePlayer = false;
        }
        else
        {
            canSeePlayer = true;
        }

        if ((player.isFacingRight && !isRightOfPlayer) || (!player.isFacingRight && isRightOfPlayer)) 
        { 
            isFrozen = false;
        }
        else 
        { 
            isFrozen = true;
        }

        if (!isFrozen && canSeePlayer && !isDead && distance < lookDistance)
        {
            currentState = EnemyState.Attack;
        }
        else if (!isFrozen && !canSeePlayer && !isDead)
        {
            currentState = EnemyState.Patrolling;
        }
        else if (isFrozen && !isDead)
        {
            currentState = EnemyState.Frozen;
        }

        if (health <= 0)
        {
            currentState = EnemyState.Dead;
        }
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case EnemyState.Frozen:
                animator.Play(idleSprite.name); //no pause cause it should be instant
                break;
            case EnemyState.Dead:
                if(isDead == false)
                {
                    isDead = true;
                    if (currentClipName != deathSprite.name)
                    {
                        animator.Play(deathSprite.name);
                    }
                    //after animation finnished send off death event
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
                    {
                        StartCoroutine(Dying());
                    }
                }
                break;
            case EnemyState.Patrolling:
                if (currentClipName != walkSprite.name
                    && (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) 
                    && !animator.IsInTransition(0))
                {
                    Vector2 walk = new Vector2(randomInt, transform.position.y); //between 2 spots
                    float distanceWalk = Vector2.Distance(walk, spawnPosition);
                    
                    if(distanceWalk <= patrolRange)
                    {
                        transform.position = spawnPosition;
                        animator.Play(walkSprite.name);
                    }
                    else
                    {
                        randomInt = Random.Range(-1, 2);
                    }
                }
            break;
            case EnemyState.Attack:
                if (distance < killDistance)
                {
                    player.TakeDamage(10);
                }
                if (distance < lookDistance)
                {
                    rb.position = Vector2.Lerp(rb.position, player.transform.position, Time.deltaTime * angelSpeedAttack / 10f);
                    if (currentClipName != attackSprite.name 
                        && currentClipName != deathSprite.name
                        && (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) 
                        && !animator.IsInTransition(0))
                    {
                        animator.Play(attackSprite.name);
                    }
                }
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    void LateUpdate()
    {
        oldPosition = transform.position.x;
    }

    IEnumerator Dying()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    public enum EnemyState
    {
        Frozen,
        Patrolling,
        Attack,
        Dead
    }
}
