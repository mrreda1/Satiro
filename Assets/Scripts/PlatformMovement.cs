using UnityEngine;

public class PlatformMovement : MonoBehaviour {
    public Transform pointA;
    public Transform PointB;
    public float moveSpeed = 2f;
    private Vector3 nextPos;

    void Start() {
        nextPos = PointB.position;
    }

    // Update is called once per frame
    void Update() {
        transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
        
        if (transform.position == nextPos) {
            nextPos = (nextPos == pointA.position) ? PointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.transform.parent = transform;
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.transform.parent = null;
        }
    }
}
