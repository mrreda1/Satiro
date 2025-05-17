using UnityEngine;

public class MonsterMovement : MonoBehaviour {
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
}
