using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void LateUpdate()
    {
        transform.LookAt(transform.position + GameManager.instance.arCamera.transform.forward);
        // Vector3 position = transform.position;
        // Vector3 cameraPosition = GameManager.instance.arCamera.transform.position;
        // Vector3 direction = cameraPosition - position;
        // Vector3 targetRotationEuler = Quaternion.LookRotation(-direction).eulerAngles;
        // Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, transform.up.normalized);
        // Quaternion targetRotation = Quaternion.Euler(scaledEuler);
        // transform.rotation = targetRotation;
    }
}
