using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    public string playerName = "야루렁";
    public int hp = 100;
    public int lives = 3;
    public float moveSpeed = 5f;
    public float jumpPower = 8f;
    public float fallLimit =-10f;
    private float facing = 1f;
    public Vector3 startPosition;
    public Vector2 airScale = new Vector2(0.8f, 1.2f);
    public Transform visual;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isGameOver = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = startPosition;
        Debug.Log(playerName + " 시작. 체력 " + hp);
                if(hp >= 70)
        {
            Debug.Log("건강");
        }
        else if(hp >= 30)
        {
            Debug.Log("주의");
        }
        else
        {
            Debug.Log("위험");
        }

    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.x > 0) {facing=1f;}
       
        else if (moveInput.x < 0) {facing=-1f;}
       
    
       // if (moveInput.x > 0 &&)
       //오른쪽만 가능
       // else if (moveInput.x < 0 ||)
      
    }
    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isGameOver)
        {
            Debug.Log("점프!");
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x, jumpPower);
        }
    }
    void Update()
    {
        if(!isGameOver)
        {
            transform.Translate(Vector3.right * moveInput.x
             * moveSpeed * Time.deltaTime);
        }

        if(transform.position.y<fallLimit)
        {
            lives-=1;
            transform.position=startPosition;
            rb.linearVelocity=Vector2.zero;
            Debug.Log("낙사.남은 목숨"+lives);
            
            if(lives<=0)
            {
                isGameOver=true;
                Debug.Log("게임 오버");
            }
        }
        if (isGrounded)
        {
            visual.localScale=new Vector3(facing,1f,1f);
        }
        else
        {
            visual.localScale=new Vector3(
                facing*airScale.x,airScale.y, 1f);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
         if(collision.gameObject.CompareTag("Ground"))
         {
            isGrounded = true;
            Debug.Log("착지");
         }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
         if(collision.gameObject.CompareTag("Ground"))
         {
            isGrounded = false;
            Debug.Log("공중");
         }
    }
}