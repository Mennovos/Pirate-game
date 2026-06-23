using System.Collections;
using UnityEngine;

/// <summary>
/// 3D Crab Enemy with state machine (Idle, Patrol, ChargePrepare, ChargeAttack, Hurt, Dead).
/// Features: left/right patrol using offset values, flip on walls/edges, 2-hit health, charge attack on player trigger,
/// full animation integration, and extensible design for future attacks.
/// 
/// Usage: Call ReceiveHit() from player attack code to damage the crab.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Animator))]
public class CrabEnemy : MonoBehaviour
{
    #region State Machine
    private enum State { Idle, Patrol, ChargePrepare, ChargeAttack, Hurt, Dead }
    private State currentState = State.Patrol;
    #endregion

    #region Patrol Settings
    [Header("Patrol Settings")]
    [Tooltip("Left patrol position offset from spawn (X, Z).")]
    [SerializeField] private Vector2 leftPatrolOffset = new Vector2(-3f, 0f);
    [Tooltip("Right patrol position offset from spawn (X, Z).")]
    [SerializeField] private Vector2 rightPatrolOffset = new Vector2(3f, 0f);
    [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
    [SerializeField, Min(0f)] private float pauseAtEndpoint = 1f;
    #endregion

    #region Environment Detection
    [Header("Environment Detection")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private LayerMask obstacleLayer = ~0;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckRadius = 0.3f;
    [SerializeField] private float groundSampleHeight = 2f;
    [SerializeField] private float groundRayDistance = 4f;
    #endregion

    #region Charge Attack Settings
    [Header("Charge Attack Settings")]
    [SerializeField, Min(0f)] private float triggerDetectionRadius = 3f;
    [SerializeField, Min(0f)] private float prepareDuration = 0.5f;
    [SerializeField, Min(0f)] private float chargeSpeed = 8f;
    [SerializeField, Min(0f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0f)] private float chargeCooldown = 3f;
    [SerializeField, Min(0f)] private float chargeStoppingDistance = 1.2f;
    [SerializeField] private int chargeDamage = 1;
    #endregion

    #region Health Settings
    [Header("Health Settings")]
    [SerializeField, Min(1)] private int maxHits = 2;
    [SerializeField, Min(0f)] private float hitInvulnerabilityTime = 0.25f;
    private int currentHits;
    private bool isInvulnerable;
    #endregion

    #region Animation Parameters
    [Header("Animation Parameters")]
    [SerializeField] private string walkBoolName = "Walk";
    [SerializeField] private string idleBoolName = "Idle";
    [SerializeField] private string hurtTriggerName = "Hurt";
    [SerializeField] private string chargeTriggerName = "Charge";
    [SerializeField] private string deathTriggerName = "Die";
    [SerializeField, Min(0f)] private float deathAnimationDelay = 1f;
    #endregion

    #region Misc Settings
    [Header("Misc Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float endpointThreshold = 0.1f;
    #endregion

    #region Runtime Variables
    private Rigidbody rb;
    private Animator animator;
    private Collider mainCollider;

    private Vector3 spawnPosition;
    private Vector3 leftPosition;
    private Vector3 rightPosition;
    private int facingDirection = 1; // 1 = right, -1 = left

    private bool isChargeOnCooldown;
    private bool isCharging;
    private Coroutine patrolCoroutine;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();

        // Lock rotation to prevent tipping
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        InitializePatrolPositions();
        ResetHealth();
        ChangeState(State.Patrol);
    }

    /// <summary>
    /// Initialize patrol positions using offset values from spawn.
    /// </summary>
    private void InitializePatrolPositions()
    {
        // Compute patrol positions from spawn + offsets
        leftPosition = ProjectToGround(spawnPosition + new Vector3(leftPatrolOffset.x, 0f, leftPatrolOffset.y));
        rightPosition = ProjectToGround(spawnPosition + new Vector3(rightPatrolOffset.x, 0f, rightPatrolOffset.y));

        // Place crab at nearest endpoint
        Vector3 startPos = Vector3.Distance(spawnPosition, leftPosition) < Vector3.Distance(spawnPosition, rightPosition)
            ? leftPosition
            : rightPosition;
        transform.position = startPos;
        if (rb != null) rb.position = startPos;
    }

    /// <summary>
    /// Change to a new state and stop any running coroutines.
    /// </summary>
    private void ChangeState(State newState)
    {
        if (currentState == State.Dead) return; // No transitions from Dead
        
        StopAllCoroutines();
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                OnStateIdle();
                break;
            case State.Patrol:
                OnStatePatrol();
                break;
            case State.ChargePrepare:
                OnStateChargePrepare();
                break;
            case State.ChargeAttack:
                OnStateChargeAttack();
                break;
            case State.Hurt:
                OnStateHurt();
                break;
            case State.Dead:
                OnStateDead();
                break;
        }
    }

    #region State Handlers

    private void OnStateIdle()
    {
        animator?.SetBool(idleBoolName, true);
        animator?.SetBool(walkBoolName, false);
        rb.linearVelocity = Vector3.zero;
    }

    private void OnStatePatrol()
    {
        animator?.SetBool(idleBoolName, false);
        animator?.SetBool(walkBoolName, true);
        patrolCoroutine = StartCoroutine(PatrolCoroutine());
    }

    private void OnStateChargePrepare()
    {
        animator?.SetTrigger(chargeTriggerName);
        rb.linearVelocity = Vector3.zero;
        StartCoroutine(ChargePrepareCoroutine());
    }

    private void OnStateChargeAttack()
    {
        StartCoroutine(ChargeAttackCoroutine());
    }

    private void OnStateHurt()
    {
        animator?.SetTrigger(hurtTriggerName);
        StartCoroutine(HurtCoroutine());
    }

    private void OnStateDead()
    {
        animator?.SetTrigger(deathTriggerName);
        StartCoroutine(DeathCoroutine());
    }

    #endregion

    #region Patrol Behavior

    private IEnumerator PatrolCoroutine()
    {
        while (currentState == State.Patrol)
        {
            // Determine target endpoint based on facing direction
            Vector3 targetEndpoint = (facingDirection == 1) ? rightPosition : leftPosition;

            // Move toward endpoint
            while (currentState == State.Patrol && Vector3.Distance(rb.position, targetEndpoint) > endpointThreshold)
            {
                // Check for obstacles or missing ground ahead
                if (IsWallAhead() || !IsGroundAhead())
                {
                    FlipDirection();
                    break;
                }

                // Calculate movement direction (horizontal only)
                Vector3 direction = (targetEndpoint - rb.position).normalized;
                direction.y = 0f;

                // Move crab
                Vector3 nextPosition = rb.position + direction * patrolSpeed * Time.fixedDeltaTime;
                nextPosition = ProjectToGround(nextPosition);
                rb.MovePosition(nextPosition);

                // Rotate toward movement direction smoothly
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));
                }

                // Check if player has entered charge trigger
                if (!isChargeOnCooldown && IsPlayerInChargeTrigger())
                {
                    ChangeState(State.ChargePrepare);
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            // Stop movement and snap to exact endpoint
            rb.linearVelocity = Vector3.zero;
            Vector3 snappedPosition = ProjectToGround(targetEndpoint);
            rb.MovePosition(snappedPosition);

            // Pause at endpoint
            float pauseTimer = 0f;
            while (pauseTimer < pauseAtEndpoint && currentState == State.Patrol)
            {
                if (!isChargeOnCooldown && IsPlayerInChargeTrigger())
                {
                    ChangeState(State.ChargePrepare);
                    yield break;
                }

                pauseTimer += Time.deltaTime;
                yield return null;
            }

            // Flip and continue to other endpoint
            FlipDirection();
            yield return null;
        }
    }

    private bool IsPlayerInChargeTrigger()
    {
        // Check sphere in front of crab for player
        Vector3 triggerCenter = rb.position + transform.right * (facingDirection * triggerDetectionRadius * 0.5f);
        Collider[] hits = Physics.OverlapSphere(triggerCenter, triggerDetectionRadius * 0.5f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(playerTag)) return true;
        }

        return false;
    }

    private bool IsWallAhead()
    {
        Vector3 checkPosition = rb.position + transform.right * facingDirection * wallCheckRadius;
        return Physics.CheckSphere(checkPosition, wallCheckRadius, obstacleLayer);
    }

    private bool IsGroundAhead()
    {
        Vector3 checkPosition = rb.position + transform.right * facingDirection * 0.5f;
        return Physics.Raycast(checkPosition, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void FlipDirection()
    {
        facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    #endregion

    #region Charge Attack Behavior

    private IEnumerator ChargePrepareCoroutine()
    {
        float prepareTimer = 0f;
        while (prepareTimer < prepareDuration)
        {
            prepareTimer += Time.deltaTime;
            yield return null;
        }

        ChangeState(State.ChargeAttack);
    }

    private IEnumerator ChargeAttackCoroutine()
    {
        isChargeOnCooldown = true;
        isCharging = true;

        // Orient toward player if visible
        Transform playerTransform = FindPlayerTransform();
        if (playerTransform != null)
        {
            int playerDirection = (playerTransform.position.x >= transform.position.x) ? 1 : -1;
            if (playerDirection != facingDirection)
                FlipDirection();
        }

        float chargeTimer = 0f;
        while (chargeTimer < chargeDuration && currentState == State.ChargeAttack)
        {
            playerTransform = FindPlayerTransform();
            if (playerTransform == null) break;

            // Calculate direction to player (horizontal only)
            Vector3 directionToPlayer = (playerTransform.position - rb.position);
            directionToPlayer.y = 0f;

            // Stop if close enough
            if (directionToPlayer.magnitude <= chargeStoppingDistance) break;

            directionToPlayer = directionToPlayer.normalized;

            // Move toward player
            Vector3 nextPosition = rb.position + directionToPlayer * chargeSpeed * Time.fixedDeltaTime;
            nextPosition = ProjectToGround(nextPosition);
            rb.MovePosition(nextPosition);

            chargeTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isCharging = false;
        rb.linearVelocity = Vector3.zero;

        // Cooldown before resuming patrol
        float cooldownTimer = 0f;
        while (cooldownTimer < chargeCooldown)
        {
            cooldownTimer += Time.deltaTime;
            yield return null;
        }

        isChargeOnCooldown = false;
        
        // Resume patrol if not dead or hurt
        if (currentState != State.Dead && currentState != State.Hurt)
            ChangeState(State.Patrol);
    }

    #endregion

    #region Damage & Health

    /// <summary>
    /// External method: call from player attack to damage the crab.
    /// </summary>
    public void ReceiveHit(int damageAmount = 1)
    {
        if (isInvulnerable || currentState == State.Dead) return;

        currentHits += damageAmount;
        StartCoroutine(InvulnerabilityCoroutine());

        if (currentHits >= maxHits)
        {
            ChangeState(State.Dead);
        }
        else
        {
            ChangeState(State.Hurt);
        }
    }

    private void ResetHealth()
    {
        currentHits = 0;
        isInvulnerable = false;
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(hitInvulnerabilityTime);
        isInvulnerable = false;
    }

    #endregion

    #region State-Specific Coroutines

    private IEnumerator HurtCoroutine()
    {
        rb.linearVelocity = Vector3.zero;
        float hurtDuration = 0.4f;
        yield return new WaitForSeconds(hurtDuration);

        if (currentState != State.Dead)
            ChangeState(State.Patrol);
    }

    private IEnumerator DeathCoroutine()
    {
        rb.linearVelocity = Vector3.zero;
        if (mainCollider != null) mainCollider.enabled = false;

        yield return new WaitForSeconds(deathAnimationDelay);
        Destroy(gameObject);
    }

    #endregion

    #region Collision with Player During Charge

    private void OnCollisionEnter(Collision collision)
    {
        if (!isCharging) return;

        if (collision.gameObject.CompareTag(playerTag))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.TakeDamage(chargeDamage);

            // End charge early
            rb.linearVelocity = Vector3.zero;
            isCharging = false;
        }
    }

    #endregion

    #region Utilities

    private Transform FindPlayerTransform()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
        return playerGO != null ? playerGO.transform : null;
    }

    /// <summary>
    /// Project a world position down to the ground using raycasts.
    /// Prevents the crab from floating in the air.
    /// </summary>
    private Vector3 ProjectToGround(Vector3 worldPosition)
    {
        Vector3 rayOrigin = worldPosition + Vector3.up * groundSampleHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundSampleHeight + groundRayDistance, groundLayer))
        {
            return hit.point + Vector3.up * heightOffset;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"CrabEnemy: Failed to find ground below {worldPosition}. Check groundLayer settings.", this);
#endif
        return worldPosition;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw patrol positions
        Vector3 spawn = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.color = Color.green;
        Vector3 left = spawn + new Vector3(leftPatrolOffset.x, 0f, leftPatrolOffset.y);
        Vector3 right = spawn + new Vector3(rightPatrolOffset.x, 0f, rightPatrolOffset.y);
        Gizmos.DrawWireSphere(left, 0.5f);
        Gizmos.DrawWireSphere(right, 0.5f);
        Gizmos.DrawLine(left, right);

        // Draw trigger detection zone
        Gizmos.color = Color.yellow;
        Vector3 triggerCenter = spawn + transform.right * (facingDirection * triggerDetectionRadius * 0.5f);
        Gizmos.DrawWireSphere(triggerCenter, triggerDetectionRadius * 0.5f);

        // Draw wall check zone
        Gizmos.color = Color.red;
        Vector3 wallCheckPos = spawn + transform.right * facingDirection * wallCheckRadius;
        Gizmos.DrawWireSphere(wallCheckPos, wallCheckRadius);

        // Draw ground check ray
        Gizmos.color = Color.cyan;
        Vector3 groundCheckPos = spawn + transform.right * facingDirection * 0.5f;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector3.down * groundCheckDistance);
    }

    #endregion
}