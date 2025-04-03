using UnityEngine;

public class SpawnFood : MonoBehaviour
{
    public GameObject FoodGameObject;
    public int FoodCount = 10;
    public int Scale = 10;

    void Start()
    {
        // Spawn n food sources
        for (int i = 0; i < FoodCount; i++)
        {
            // Distribute the food on random points in circle
            var x = Random.Range(-1f, 1f) * Scale;
            var z = Random.Range(-1f, 1f) * Scale;

            FoodGameObject.transform.position = new Vector3(x, 0.05f, z);
            Instantiate(FoodGameObject);
        }
    }
}
