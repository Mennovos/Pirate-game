using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject Map;

    private Vector2 moveInput;


    [SerializeField] private float Speed;
    private Vector3 Movement;

    private Controls Controls;

    private Rigidbody Rb;
    private Animator Anim;
    public bool Grappling;
    private bool Restraint;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Controls = new Controls();
        Controls.Player.Enable();

        Controls.Player.Move.performed += OnMove;
        Controls.Player.Move.canceled += OnMove;
        Controls.Player.Grapple.performed += Grapple;

        Rb = GetComponent<Rigidbody>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Movement.x = context.ReadValue<Vector2>().x;
        Movement.z = context.ReadValue<Vector2>().y;
        bool Walking = input.sqrMagnitude > 0.01f;
        Anim.SetBool("Walking", Walking);
    }
    public void Grapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(GrappleCooldown());
        }
    }

    void Update()
    {
        transform.Translate(Movement * Speed * Time.deltaTime);
        if (cameraTransform != null)
        {
            Vector3 PlayerRotation = transform.eulerAngles;
            PlayerRotation.y = cameraTransform.eulerAngles.y;
            transform.eulerAngles = PlayerRotation;
        }
    }

    IEnumerator GrappleCooldown()
    {
        Anim.SetTrigger("Grapple");
        yield return new WaitForSeconds(1f);
        Grappling = true;
        yield return new WaitForSeconds(0.01f);
        Grappling = false;
    }
    IEnumerator AttackCooldown(string Attackname, float Cooldowntime)
    {
        Anim.SetTrigger(Attackname);
        Restraint = true;
        yield return new WaitForSeconds(Cooldowntime);
        Restraint = false;
    }
}
