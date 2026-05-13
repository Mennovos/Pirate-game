using UnityEngine;
using System.Collections;

public class CrabEnemy : Enemy
{
    [SerializeField] private float enemyHealth;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float waitTime;
    private bool PlayerInRange;
    void Start()
    {
        transform.position = startPos;
        if (PlayerInRange)
        {
            StartCoroutine(MoveBetweenPoints());
        }
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = true;
            StartCoroutine(ChargeAttack());
        }

    }
    private IEnumerator ChargeAttack()
    {
        //charge atttack animation here
        yield return new WaitForSeconds(1f);
      
    }
}