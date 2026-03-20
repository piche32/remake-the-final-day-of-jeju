using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(PlayerInput))]
public class PlayerMove : MonoBehaviour
{
    PlayerInput m_playerInput;
    InputActionMap m_playerActionMap;
    InputAction m_moveAction;
    InputAction m_jumpAction;

    Rigidbody m_rigidbody;
    Animator m_animator;

    private Vector2 m_moveAmt;
    [SerializeField] float m_speed = 5.0f;
    [SerializeField] float m_rotateSpeed = 100.0f;
    [SerializeField] float m_jumpSpeed = 5.0f;

    [SerializeField] private float m_groundAnimDuration = 0.17f;
    [SerializeField] private float m_maxGroundAnimSpeed = 1.5f;
    private float m_capsuleColliderRadius;
    private bool m_isJumping = false;
    private bool m_falling = false;

    [SerializeField] LayerMask m_groundLayer;
    static private float m_maxGroundDistance = 0f;
    [SerializeField] float m_GroundDistance = 0.1f;
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();

        if (m_maxGroundDistance == 0f)
        {
            m_maxGroundDistance = Mathf.Abs(Physics.gravity.y) * m_groundAnimDuration;
        }

        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        m_capsuleColliderRadius = collider.radius;
    }

    void OnEnable()
    {
        m_playerInput = GetComponent<PlayerInput>();
        m_playerActionMap = m_playerInput.actions.FindActionMap("Player");

        m_moveAction = m_playerActionMap.FindAction("Move");
        m_moveAction.performed += OnMove;
        m_moveAction.canceled += OnMove;

        m_jumpAction = m_playerActionMap.FindAction("Jump");
        m_jumpAction.started += StartJump;
    }
    void OnDisable()
    {
        m_playerInput = GetComponent<PlayerInput>();
        m_playerActionMap = m_playerInput.actions.FindActionMap("Player");

        m_moveAction = m_playerActionMap.FindAction("Move");
        m_moveAction.performed -= OnMove;
        m_moveAction.canceled -= OnMove;

        m_jumpAction = m_playerActionMap.FindAction("Jump");
        m_jumpAction.started -= StartJump;
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            m_moveAmt = Vector2.zero;

            if (m_animator != null)
            {
                m_animator.SetFloat("front", m_moveAmt.y);
            }
            return;
        }
        m_moveAmt = value.ReadValue<Vector2>();

        if (m_animator != null)
        {
            m_animator.SetFloat("front", m_moveAmt.y);
        }
    }

    public void StartJump(InputAction.CallbackContext value)
    {
        if (m_isJumping)
        {
            return;
        }

        m_isJumping = true;
        if (m_animator != null)
        {
            m_animator.SetTrigger("jump");
        }
    }

    public void OnJump()
    {
        if (m_rigidbody != null)
        {
            m_rigidbody.AddForce(Vector3.up * m_jumpSpeed, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if (m_isJumping)
        {
            CheckFalling();
        }
        if (m_falling)
        {
            CheckLanding();
        }

        Walking();
    }

    private void Walking()
    {
        if (m_moveAmt.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 moveDir = transform.forward * m_moveAmt.y;
        m_rigidbody.MovePosition(m_rigidbody.position + m_speed * Time.deltaTime * moveDir);
        float rotationAmount = m_moveAmt.x * m_rotateSpeed * Time.deltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
        m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);

    }

    private void CheckFalling()
    {
        if (m_rigidbody.linearVelocity.y < -0.1f)
        {
            m_falling = true;
            m_isJumping = false;
            m_animator.SetTrigger("falling");
        }

        if (m_falling)
        {
            float v0 = Mathf.Abs(m_rigidbody.linearVelocity.y);
            float g = Mathf.Abs(Physics.gravity.y);

            if (Physics.SphereCast(transform.position, m_capsuleColliderRadius, transform.up * -1, out RaycastHit hit, m_maxGroundDistance, m_groundLayer))
            {
                float d = hit.distance;
                float tLand = (-v0 + Mathf.Sqrt(v0 * v0 + 2 * g * d)) / g;

                if (tLand > 0)
                {
                    float requiredSpeed = m_groundAnimDuration / tLand;
                    float finalSpeed = Mathf.Clamp(requiredSpeed, 1.0f, m_maxGroundAnimSpeed);
                    m_animator.SetFloat("landingAnimSpeed", finalSpeed);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_capsuleColliderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up * -1 * m_maxGroundDistance, m_capsuleColliderRadius);
    }
    private void CheckLanding()
    {
        if (Physics.SphereCast(transform.position, m_capsuleColliderRadius, transform.up * -1, out RaycastHit hit, m_GroundDistance, m_groundLayer))
        {
            Landing();
        }
    }


    private void Landing()
    {
        m_falling = false;
        if (m_animator != null)
        {
            m_animator.SetTrigger("landing");
        }
    }
}
