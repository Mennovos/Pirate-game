using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrapple : MonoBehaviour
{
    LayerMask grappleLayerMask;
    private LineRenderer lineRenderer;
    [SerializeField] private float GrappleSpeed = 5f;
    [SerializeField] private Transform Grapplepoint;
    [SerializeField] private List<Transform> grapplePoints;
    [SerializeField] public List<GameObject> PickupsPosition;
    private Controls Controls;
    private bool grappling;
    private void Awake()
    {
        Controls = new Controls();
        grappleLayerMask = LayerMask.GetMask("Grappling");
        Controls.Player.Grapple.performed += Grapple;
    }

    public void Grapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Grapple performed");
            StartCoroutine(GrappleCooldown());
        }
    }
    private void FixedUpdate()
    {
        Debug.DrawRay(Grapplepoint.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);


        if (Physics.Raycast(Grapplepoint.position, transform.forward, out RaycastHit hit, Mathf.Infinity, grappleLayerMask))
        {
            Debug.Log($"Grapple hit: {hit.collider.name}");
            if (grappling == true && !hit.collider.CompareTag("Pickup"))
            {
                grapplePoints.Add(hit.transform);
                Vector3 EndPoint = hit.point;
            }
            //if (grappling == true && hit.collider.CompareTag("Pickup"))
            //{
            //    PickupsPosition.Add(hit.collider.gameObject);
            //}

        }


        for (int i = 0; i < grapplePoints.Count; i++)
        {
             transform.position = Vector3.Lerp(transform.position, grapplePoints[i].position, Time.deltaTime * GrappleSpeed);
            if (Vector3.Distance(transform.position, grapplePoints[i].position) < 4f)
            {
                grapplePoints.RemoveAt(i);
            }
        }
        for (int i = 0; i < PickupsPosition.Count; i++)
        {
            PickupsPosition[i].transform.position = Vector3.Lerp(PickupsPosition[i].transform.position, transform.position + new Vector3(0, 5, 0), Time.deltaTime * 2);
        }
    }
    IEnumerator GrappleCooldown()
    {
        //Anim.SetTrigger("Grapple");
        yield return new WaitForSeconds(1f);
        grappling = true;
        yield return new WaitForSeconds(0.01f);
        grappling = false;
    }
}
