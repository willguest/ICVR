using UnityEngine;

public class TokenObject : MonoBehaviour
{
    public System.Guid CoinId
    {
        get { return id; }
        set { id = value; }
    }

    public string Holder
    {
        get { return holder; }
        set { holder = value; }
    }

    private System.Guid id;
    private string holder;
}