using UnityEngine;

public class Unit : MonoBehaviour
{
    private new Rigidbody2D rigidbody2D;
    private new Camera camera;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        camera = Camera.main;
        //mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);    
    }
    
    Vector3 mouseWorldPosition;
    private void FixedUpdate()
    {
        mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

        rigidbody2D.MovePosition(Vector2.MoveTowards(transform.position, mouseWorldPosition,
            Time.fixedDeltaTime * 50f));

    }
}
