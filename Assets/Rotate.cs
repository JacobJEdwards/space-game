using UnityEngine;

public class Rotate : MonoBehaviour
{

    void Update()
    {
        transform.Rotate(new Vector3(15, 2, 2) * Time.deltaTime);
    }
}
