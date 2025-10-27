using UnityEngine;
using NaivePhysics;

public class MarioController : AlignedBox
{
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private float gravity = -9.8f;
    private bool isGrounded = true;

    private NaiveEngine engine; 

    private void Awake()
    {
        engine = FindObjectOfType<NaiveEngine>();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity = new Vector2(5f, jumpForce);
            isGrounded = false;
        }

        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            OnMoved();

            foreach (var shape in engine.Shapes)
            {
                if (shape != this && NaiveEngine.Overlaps(this, shape))
                {
                    isGrounded = true;
                    velocity = Vector2.zero;
                    break;
                }
            }
        }
    }
}
