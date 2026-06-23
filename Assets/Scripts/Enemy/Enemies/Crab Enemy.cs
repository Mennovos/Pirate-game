using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CrabEnemy : Enemy
{
    [Header("Patrol (relative to spawn position)")]
    [Tooltip("Local XZ offset from spawn for patrol point A.")]
    [SerializeField] private Vector2 patrolOffsetA = new Vector2(-2f, 0f);
    [Tooltip("Local XZ offset from spawn for patrol point B.")]
    [SerializeField] private Vector2 patrolOffsetB = new Vector2(2f, 0f);
    [SerializeField, Min(0f)] private float patrolSpeed = 1.2f;
    [SerializeField, Min(0f)] private float pauseAtPoint = 1f;

    [Header("Charge Attack")]
    [SerializeField, Min(0f)] private float detectionRadius = 6f;
    [SerializeField, Min(0f)] private float windupTime = 0.25f;
    [SerializeField, Min(0f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0f)] private float chargeMaxSpeed = 6f;
    [SerializeField, Min(0f)] private float chargeAcceleration = 20f;
    [SerializeField, Min(0f)] private float chargeCooldown = 1.2f;
    [SerializeField, Min(0f)] private float stoppingDistance = 1.2f;
    [SerializeField] private GameObject Damage;

    [Header("Ground Sampling")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField, Min(0.1f)] private float groundSampleHeight = 2f;
    [SerializeField, Min(0.5f)] private float groundRayDistance = 4f;
    [SerializeField] private float heightOffset = 0.0f;

    private Vector3 spawnPosition;
    private Vector3 aPos;
    private Vector3 bPos;

    private Rigidbody bodyRb;
    private bool goingToB = true;

    private enum State { Patrolling, Windup, Charging, Cooldown, Stunned }
    private State state = State.Patrolling;
    private Coroutine patrolCoroutine;

    private void Start()
    {
        bodyRb = rb ?? GetComponent<Rigidbody>();
        if (bodyRb == null)
        {
            Debug.LogError("CrabEnemy requires a Rigidbody.", this);
            enabled = false;
            return;
        }

        bodyRb.isKinematic = false;
        bodyRb.constraints = RigidbodyConstraints.FreezeRotation;

        // store spawn position and compute grounded patrol points from offsets
        spawnPosition = transform.position;
        aPos = ProjectToGround(spawnPosition + new Vector3(patrolOffsetA.x, 0f, patrolOffsetA.y));
        bPos = ProjectToGround(spawnPosition + new Vector3(patrolOffsetB.x, 0f, patrolOffsetB.y));

        // place crab on ground at nearest endpoint
        Vector3 start = Vector3.Distance(transform.position, aPos) < Vector3.Distance(transform.position, bPos) ? aPos : bPos;
        transform.position = start;
        bodyRb.position = start;

        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private void FixedUpdate()
    {
        if (state == State.Patrolling && target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= detectionRadius)
            {
                StopCoroutineIfRunning(patrolCoroutine);
                StartCoroutine(ChargeRoutine());
            }
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (state != State.Patrolling)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            Vector3 target = goingToB ? bPos : aPos;

            // Move until within small distance
            while (state == State.Patrolling && Vector3.Distance(bodyRb.position, target) > 0.06f)
            {
                Vector3 dir = (target - bodyRb.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) break;
                dir = dir.normalized;

                Vector3 next = bodyRb.position + dir * patrolSpeed * Time.fixedDeltaTime;
                next = ProjectToGround(next);

                bodyRb.MovePosition(next);

                // Smoothly face movement direction
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                bodyRb.MoveRotation(Quaternion.Slerp(bodyRb.rotation, look, 10f * Time.fixedDeltaTime));

                yield return new WaitForFixedUpdate();
            }

            // snap and ensure grounded
            Vector3 snap = ProjectToGround(target);
            bodyRb.MovePosition(snap);

            // pause while staying grounded
            float t = 0f;
            while (t < pauseAtPoint && state == State.Patrolling)
            {
                bodyRb.MovePosition(ProjectToGround(bodyRb.position));
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // swap target
            goingToB = !goingToB;
        }
    }

    private IEnumerator ChargeRoutine()
    {
        state = State.Windup;
        animator.SetTrigger("Attacking");
        yield return new WaitForSeconds(windupTime);

        state = State.Charging;
        float timer = 0f;
        float currentSpeed = patrolSpeed;

        if (Damage != null) Damage.SetActive(true);

        while (timer < chargeDuration && state == State.Charging)
        {
            if (target == null) break;

            Vector3 toPlayer = target.position - bodyRb.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;
            if (dist <= stoppingDistance) break;

            Vector3 dir = toPlayer.normalized;
            currentSpeed = Mathf.MoveTowards(currentSpeed, chargeMaxSpeed, chargeAcceleration * Time.fixedDeltaTime);

            Vector3 next = bodyRb.position + dir * currentSpeed * Time.fixedDeltaTime;
            next = ProjectToGround(next);

            bodyRb.MovePosition(next);

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                bodyRb.MoveRotation(Quaternion.Slerp(bodyRb.rotation, look, 14f * Time.fixedDeltaTime));
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (Damage != null) Damage.SetActive(false);
        if (bodyRb != null) bodyRb.linearVelocity = Vector3.zero;

        state = State.Cooldown;
        yield return new WaitForSeconds(chargeCooldown);

        state = State.Patrolling;
        if (patrolCoroutine == null) patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state == State.Charging && collision.gameObject.CompareTag("Player"))
        {
            var health = collision.gameObject.GetComponent<Health>();
            if (health != null) health.TakeDamage((int)getAttackDamage());

            StopAllCoroutines();
            if (Damage != null) Damage.SetActive(false);
            if (bodyRb != null) bodyRb.linearVelocity = Vector3.zero;
            state = State.Cooldown;
            StartCoroutine(ResumeAfterCooldown());
        }
    }

    private IEnumerator ResumeAfterCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        state = State.Patrolling;
        if (patrolCoroutine == null) patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private Vector3 ProjectToGround(Vector3 candidate)
    {
        Vector3 origin = new Vector3(candidate.x, candidate.y + groundSampleHeight, candidate.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundSampleHeight + groundRayDistance, groundMask))
        {
            Vector3 g = hit.point;
            g.y += heightOffset;
            return g;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"CrabEnemy: ground not found near {candidate}. Increase groundRayDistance or set groundMask.", this);
#endif
        // fallback: preserve current Y to avoid teleporting
        return new Vector3(candidate.x, transform.position.y, candidate.z);
    }

    private void StopCoroutineIfRunning(Coroutine c)
    {
        if (c != null) StopCoroutine(c);
        c = null;
    }

    private void OnDisable()
    {
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
    }
}