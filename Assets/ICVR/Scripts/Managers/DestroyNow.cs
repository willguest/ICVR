using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyNow : MonoBehaviour
{
    public GameObject DefaultRespawnLocation;
    public Transform BallReturnLocation;
    public GameObject PinRespawnLocation;

    private GameObject respawnObject;
    private bool readyToSpawn = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (readyToSpawn)
        {
            new WaitForEndOfFrame();
            if (respawnObject != null)
            {
                Respawn(respawnObject);
            }
            readyToSpawn = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        respawnObject = collision.collider.gameObject;
        readyToSpawn = true;
    }

    private void Respawn(GameObject orig)
    {
        string name = orig.name;
        Vector3 scale = orig.transform.localScale;


        if (name.Contains("MixedRealityPlayspace"))
        {
            orig.transform.position = DefaultRespawnLocation.transform.position;
            orig.transform.rotation = Quaternion.identity;
            return;
        }

        GameObject newOne = Instantiate(orig, DefaultRespawnLocation.transform.position, 
                                        Quaternion.identity, orig.transform.parent);
        newOne.transform.localScale = scale;

        Destroy(orig);

        newOne.name = name;

        if (newOne.name.Contains("Cube"))
        {
            newOne.transform.position = new Vector3(0.65f, 2f, 1.1f);
        }
        else if (newOne.name.Contains("LookingGlass"))
        {
            newOne.transform.position = new Vector3(-0.5f, 1.5f, 1.0f);
        }
        else if (newOne.name.Contains("Pin"))
        {
            newOne.transform.position = PinRespawnLocation.transform.position;
        }
        else if (newOne.name.Contains("sceneReset"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (newOne.name.Contains("lb"))
        {
            newOne.transform.position = BallReturnLocation.position;
        }
    }

}
