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
    [SerializeField] private GameObject Damage;
    private bool chargingAttack;
    void Start()
    {
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
                transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / waitTime);
                elapsedTime += Time.deltaTime * moveSpeed;
                yield return null;
            }

            // Swap start and end positions
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
        }
    }
    public override void attack(Vector2 impulse)
    {
        enemyHealth -= 1; // Example damage value
        
        if (enemyHealth <= 0)
        {
            kill();
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
        yield return new WaitForSeconds(chargeAttackAnimDuration);
        // Activate damage hitbox
        Damage.SetActive(true);
        yield return new WaitForSeconds(chargeAttackSpeed);
        Damage.SetActive(false);
        // Resume moving after attack
        moveSpeed = 4;
        chargingAttack = false;

    }
}