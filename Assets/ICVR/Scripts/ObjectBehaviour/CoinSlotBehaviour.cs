using System.Collections;
using UnityEngine;



public class CoinSlotBehaviour : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Light;
    [SerializeField] private CapsuleCollider detector;
    [SerializeField] private GameObject toggleSign;

    private bool hasCoin = false;
    private IslandCoinObject currentCoin;

    public bool HasCoin
    {
        get { return hasCoin; }
        private set { hasCoin = value; }
    }

    public void DoWithCoin(System.Action callback)
    {
        if (hasCoin && currentCoin != null)
        {
            callback();
        }
    }

    private IEnumerator CheckForCoin(Collider coinCollider)
    {
        yield return new WaitForSeconds(1.0f);
        hasCoin = coinCollider.bounds.Intersects(detector.bounds);

        toggleSign.SetActive(!hasCoin);
        coinCollider.gameObject.TryGetComponent(out currentCoin);

        Color c = hasCoin ? Color.green : Color.red;
        m_Light.material.SetColor("_EmissionColor", c);

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IslandCoinObject>())
        {
            StartCoroutine(CheckForCoin(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IslandCoinObject>())
        {
            StartCoroutine(CheckForCoin(other));
        }
    }
}
