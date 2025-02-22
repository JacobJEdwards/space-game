using UnityEngine;

public class Fracture : MonoBehaviour
{
    [Tooltip("\"Fractured\" is the object that this will break into")]
    public GameObject fractured;

    public void FractureObject()
    {
        var fracturedInstance = Instantiate(fractured, transform.position, transform.rotation); //Spawn in the broken
        fracturedInstance.transform.localScale =
            transform.localScale; //Set the scale of the broken object to the scale of the original object

        foreach (var rb in
                 fracturedInstance.GetComponentsInChildren<Rigidbody>()) //Get all the rigidbodies in the object
            rb.AddExplosionForce(Random.Range(200f, 500f), transform.position, Random.Range(25f, 50f)); //Add an

        Destroy(fracturedInstance, 5f); //Destroy the broken version after 5 seconds

        transform.parent?.gameObject.SetActive(false); //Disable the parent object
    }
}