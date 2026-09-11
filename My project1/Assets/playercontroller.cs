using UnityEngine;
using UnityEngine.InputSystem;
 
public class PlayerController : MonoBehaviour
{
  public float moveSpeed = 5f;
  private Vector2 moveInput;
 
  void OnMove(InputValue value)
  {
    moveInput = value.Get<Vector2>();
  }
 
  void Update()
  {
    transform.Translate(Vector3.right * moveInput.x * moveSpeed * Time.deltaTime);
  }
}