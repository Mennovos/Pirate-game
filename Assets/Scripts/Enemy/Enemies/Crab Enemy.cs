using UnityEngine;
using System.Collections;

public class CrabEnemy : Enemy
{
    [SerializeField] private float enemyHealth;
    [SerializeField] private float chargeAttackSpeed;
    [SerializeField] private float chargeAttackAnimDuration;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float waitTime;
    [SerializeField] private float impulseReduction;
    [SerializeField] private float pauseAtPoint = 1f;

    [SerializeField] private GameObject Damage;

    private bool chargingAttack;
    private bool hit;
  
    private new void Awake()
    {
        base.Awake();
        transform.position = startPos;
        StartCoroutine(MoveBetweenPoints());
    }

    private IEnumerator MoveBetweenPoints()
    {
   
        while (true)
            {
           
                // Move from startPos to endPos
                float elapsedTime = 0;
                while (elapsedTime < waitTime)
                {
                 animator.SetBool("Walking", true);
                if (hit) 
                {
                    yield return null;
                    transform.position = Vector3.Lerp(transform.position, startPos, 1);
                    continue;
                }
                
                transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / waitTime);
                    elapsedTime += Time.deltaTime * moveSpeed;
                    yield return null;
                }
                animator.SetBool("Walking", false);
            yield return new WaitForSeconds(pauseAtPoint);
            // Swap start and end positions
            Vector3 temp = startPos;
                startPos = endPos;
                endPos = temp;
            }
    }
    public override void attack(Vector2 impulse)
    {
        StartCoroutine(TimeHit());
        base.attack(impulse / impulseReduction);

        if (!chargingAttack)
        {
            enemyHealth -= 1; // Example damage value

            if (enemyHealth <= 0)
            {
                kill();
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            moveSpeed = 0; // Stop moving when player is in range
            StartCoroutine(ChargeAttack());
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
         other.GetComponent<Health>().TakeDamage(10); // Example damage value
        }
    }
    private IEnumerator ChargeAttack()
    {
        chargingAttack = true;
        //charge atttack animation here
        animator.SetTrigger("Attacking");
        yield return new WaitForSeconds(chargeAttackAnimDuration);
        // Activate damage hitbox
        Damage.SetActive(true);
        yield return new WaitForSeconds(chargeAttackSpeed);
        Damage.SetActive(false);
        // Resume moving after attack
        moveSpeed = 4;
        chargingAttack = false;

    }
    private IEnumerator TimeHit()
    {
        hit = true;
        yield return new WaitForSeconds(0.5f);
        hit = false;
    }

}