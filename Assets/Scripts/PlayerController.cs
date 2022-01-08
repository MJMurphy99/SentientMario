using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce, quickFall, jumpDelay, vaultJumpModifier, maxVaultChain;
    public float acceleration, maxWalkSpeed, maxRunSpeed;

    private Rigidbody2D rb;
    private float lastDirection = 0;
    private bool isJumpDelayed = false;
    private int vaultJumpChain = 0;
    private CapsuleCollider2D col;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Jump();
        Walk();
    }

    private void Jump()
    {
        VaultJump();
        if (Input.GetKey(KeyCode.Space) && CheckIfGrounded() && !isJumpDelayed)
            rb.velocity += Vector2.up * jumpForce * (1 + (vaultJumpChain * vaultJumpModifier));
        else if(!Input.GetKey(KeyCode.Space)) vaultJumpChain = 0;


        if (!CheckIfGrounded())
            QuickFall();
    }

    private void QuickFall()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics.gravity.y * quickFall * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics.gravity.y * quickFall * Time.deltaTime;
        }
    }

    private bool CheckIfGrounded()
    {
        RaycastHit2D downHit = Physics2D.Raycast(transform.position, Vector2.down);

        return  downHit.distance <= col.size.y / 2 + 0.1f;
    }

    private void VaultJump()
    {
        if (rb.velocity.y < 0 && CheckIfGrounded())
        {
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                vaultJumpChain = vaultJumpChain != maxVaultChain ? vaultJumpChain + 1 : 0;
            }
            else StartCoroutine("JumpDelay");
        }
    }

    private IEnumerator JumpDelay()
    {
        vaultJumpChain = 0;
        isJumpDelayed = true;
        yield return new WaitForSeconds(jumpDelay);
        isJumpDelayed = false;
    }

    private void Walk()
    {
        GetLastDirection();
        float maxMoveSpeed = MaxSpeed();

        if(Input.GetAxisRaw("Horizontal") != 0)
        {
            rb.velocity += Vector2.right * Input.GetAxisRaw("Horizontal") * acceleration;

            if (Mathf.Abs(rb.velocity.x) > maxMoveSpeed)
            {
                rb.velocity = rb.velocity.y * Vector2.up + Vector2.right * Input.GetAxisRaw("Horizontal") * maxMoveSpeed;
            }
        }
        else
        {
            if (Mathf.Abs(rb.velocity.x) > acceleration)
            {
                rb.velocity -= Vector2.right * lastDirection * acceleration;
            }
            else
            {
                rb.velocity = rb.velocity.y * Vector2.up;
            }
        }
    }

    private void GetLastDirection()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 && Input.GetAxisRaw("Horizontal") != lastDirection)
            lastDirection = Input.GetAxisRaw("Horizontal");
    }

    private float MaxSpeed()
    {
        return Input.GetKey(KeyCode.LeftControl) ? maxRunSpeed : maxWalkSpeed;
    }
}
