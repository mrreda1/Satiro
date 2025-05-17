using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private GameObject virtualCamera;

    void Start() {
        foreach (Transform obj in transform.parent.transform) {
            if (obj.CompareTag("Camera")) {
                virtualCamera = obj.gameObject;
                return;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && !collision.isTrigger) {
            virtualCamera.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player") && !collision.isTrigger) {
            virtualCamera.SetActive(false);
        }
    }

}