using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    private Vector2 m_moveAmt;
    private Vector2 m_lookAmt;

    [SerializeField] float m_speed = 5.0f;
    [SerializeField] float m_rotateSpeed = 100.0f;
    [SerializeField] float m_jumpSpeed = 5.0f;

    Rigidbody m_rigidbody;
    Animator m_animator;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        m_moveAmt = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        m_lookAmt = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (m_rigidbody != null)
        {
            m_rigidbody.AddForce(Vector3.up * m_jumpSpeed, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Walking();
        Rotating();
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

    private void Rotating()
    {
        if (m_lookAmt.sqrMagnitude < 0.01f)
        {
            return;
        }

        float rotationAmount = m_lookAmt.x * m_rotateSpeed * Time.deltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0, rotationAmount, 0);
        m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
    }
}
