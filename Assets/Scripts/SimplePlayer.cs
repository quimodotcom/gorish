using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlayer : MonoBehaviour
{
    #region PUBLIC FIELDS

    [Header("Walk / Run Setting")] public float walkSpeed;
    public float runSpeed;
    [Header("Current Player Speed")] public float currentSpeed, targetSpeed;

    [Header("Ground LayerMask name")] public LayerMask groundLayerMask;

    [Header("Player Animator")] public Animator animator;
    
    [Header("Player Orientation")] public Transform orientation;

    #endregion


    #region PRIVATE FIELDS
    [Space]
    private float m_xAxis;
    private float m_zAxis;
    private Rigidbody m_rb;
    private bool m_leftShiftPressed;

    public bool isGrounded = true;

    #endregion
    #region MONODEVELOP ROUTINES

    private void Update()
    {
        #region controller Input [horizontal | vertical ] movement

        m_zAxis = Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1);
        m_xAxis = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        #endregion

        #region adjust player move speed [walk | run]

        if (m_leftShiftPressed && !lerpingSpeed && targetSpeed != runSpeed && (m_zAxis > 0 || m_xAxis > 0))
        {
            targetSpeed = runSpeed;
            StartCoroutine(LerpSpeed(walkSpeed, runSpeed));
        }
        else if (!m_leftShiftPressed && !lerpingSpeed && targetSpeed != walkSpeed && (m_zAxis > 0 || m_xAxis > 0))
        {
            targetSpeed = walkSpeed;
            StartCoroutine(LerpSpeed(currentSpeed, walkSpeed));
        }
        else if (targetSpeed != walkSpeed && targetSpeed != runSpeed)
        {
            targetSpeed = 0;
        }

        #endregion

        #region Shift To Change Speed

		m_leftShiftPressed = Input.GetKey(KeyCode.LeftShift);

        #endregion

        CheckForGround();
    }

    bool lerpingSpeed = false;

	private void CheckForGround()
    {
        Debug.DrawRay(orientation.position, -Vector3.up * 1.5f);
        
        isGrounded = Physics.Raycast(orientation.position, -Vector3.up, 0.5f, groundLayerMask);
    }

    private void DetectAnimation()
    {
        animator.SetFloat("VelocityX", m_xAxis);

        if (!m_leftShiftPressed)
        {
            if (isAnimating)
            {
                StopCoroutine(LerpAnim());
                if(m_zAxis > 0 || m_xAxis > 0)
                {
                    StartCoroutine(LerpAnim(animator.GetFloat("VelocityZ"), 1.0f));
                }
                else
                {
                    StartCoroutine(LerpAnim(animator.GetFloat("VelocityZ"), 0.0f));
                }
            }
            else
            {
                animator.SetFloat("VelocityZ", m_zAxis);
            }
        }
    }

    private bool isAnimating = false;

    private float duration = 1.4f;

    private IEnumerator LerpAnim()
    {
        isAnimating = true;

        float startTime = Time.time;
        float endTime = startTime + duration;
        float startValue = animator.GetFloat("VelocityZ");
        float endValue = m_zAxis * 2;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            animator.SetFloat("VelocityZ", currentValue);

            yield return null;
        }

        animator.SetFloat("VelocityZ", endValue);
        isAnimating = false;
    }

    private IEnumerator LerpAnim(float startValue, float endValue)
    {
        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            animator.SetFloat("VelocityZ", currentValue);

            yield return null;
        }

        animator.SetFloat("VelocityZ", endValue);
    }

    private IEnumerator LerpSpeed(float startSpeed, float endSpeed)
    {
        lerpingSpeed = true;

        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);

            yield return null;
        }

        currentSpeed = endSpeed;
        lerpingSpeed = false;
    }

    private void FixedUpdate()
    {
        m_rb = GetComponent<Rigidbody>();
        
        DetectAnimation();

        #region move player

        Vector3 MoveVector = orientation.TransformDirection(0f, 0f, m_zAxis) * currentSpeed;
        /*if (isGrounded)
        {*/
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + m_xAxis*2.5f, transform.eulerAngles.z);
            m_rb.velocity = new Vector3(MoveVector.x, m_rb.velocity.y, MoveVector.z) * (Time.deltaTime * 50);
        /*}*/

        /*if(m_xAxis != 0 || m_zAxis != 0 && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }*/

        #endregion
    }

    #endregion
}