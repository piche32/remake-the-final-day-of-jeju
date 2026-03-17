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

    private bool m_isJumping;

    private void Awake()
    {
        m_playerInput = GetComponent<PlayerInput>();
        m_playerActionMap = m_playerInput.actions.FindActionMap("Player");

        m_moveAction = m_playerActionMap.FindAction("Move");
        m_moveAction.performed += OnMove;
        m_moveAction.canceled += OnMove;

        m_jumpAction = m_playerActionMap.FindAction("Jump");
        m_jumpAction.started += OnJump;


        m_rigidbody = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            m_moveAmt = Vector2.zero;

            if (m_animator != null)
            {
                m_animator.SetFloat("front", m_moveAmt.y);
                m_animator.SetFloat("right", m_moveAmt.x);
            }
            return;
        }
        m_moveAmt = value.ReadValue<Vector2>();

        if (m_animator != null)
        {
            m_animator.SetFloat("front", m_moveAmt.y);
            m_animator.SetFloat("right", m_moveAmt.x);
        }
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        if (m_isJumping) return;
        if (m_rigidbody != null)
        {
            m_rigidbody.AddForce(Vector3.up * m_jumpSpeed, ForceMode.Impulse);
            m_isJumping = true;
        }
    }

    private void FixedUpdate()
    {
        Walking();
    }

    private void Walking()
    {
        if (m_moveAmt.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 moveDir = transform.forward * m_moveAmt.y + transform.right * m_moveAmt.x;
        m_rigidbody.MovePosition(m_rigidbody.position + m_speed * Time.deltaTime * moveDir);

    }

    void OnCollisionEnter(Collision collision)
    {
        if (m_isJumping)
        {
            m_isJumping = false;
        }
    }
}
