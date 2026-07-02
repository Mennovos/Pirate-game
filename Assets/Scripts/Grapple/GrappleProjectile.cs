using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrappleProjectile : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField, Min(0f)] private float speed;
    [SerializeField] private LineRenderer lineRenderer;

    private PlayerGrappleV2 owningPlayer;
    private IHookPoint hookedPoint;
    
    private bool returning;
    
    private void Awake()
    {
        TryGetComponent(out rb);
    }
    
    public void Init(PlayerGrappleV2 player, Vector2 position, Vector2 direction)
    {
        owningPlayer = player;
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void Update()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, owningPlayer.transform.position);
    }

    void FixedUpdate()
    {
        if (hookedPoint == null)
        {
            if (returning)
            {
                rb.linearVelocity = (owningPlayer.transform.position - transform.position).normalized * speed;
                
                if (Vector3.Distance(transform.position, owningPlayer.transform.position) < 1f)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            
            transform.position = hookedPoint.GetPosition();
            
            if (returning)
            {
                hookedPoint.WhilePulling(owningPlayer, this);
                
                if (Vector3.Distance(transform.position, owningPlayer.transform.position) < 1f)
                {
                    hookedPoint.OnReached(owningPlayer, this);
                    
                    Destroy(gameObject);
                }
            }
        }
    }


    public void StartReturning()
    {
        returning = true;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (hookedPoint == null)
        {
            if (other.TryGetComponent(out hookedPoint))
            {
                hookedPoint.OnHooked(owningPlayer, this);
            }
            else
            {
                returning = true;
            }
        }
    }
}
