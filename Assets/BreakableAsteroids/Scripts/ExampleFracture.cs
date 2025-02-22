using UnityEngine;

public class ExampleFracture : MonoBehaviour
{
    public GameObject[] asteroids;
    public GameObject chonker;
    private int counter;

    private void Update()
    {
        //Code loops through asteroids and fractures them on space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            asteroids[counter].GetComponent<Fracture>().FractureObject();
            counter++;
        }

        if (Input.GetKey(KeyCode.I)) chonker.gameObject.SetActive(true);
    }
}