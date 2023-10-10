using System.Runtime.InteropServices;

public static class TokenUtilsInternal
{
    [DllImport("__Internal")]
    public static extern void GetCoin(int cbIndex);
}

public static class TokenUtils
{
    public static void GetSomeIslandCoin(int cbIndex)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TokenUtilsInternal.GetCoin(cbIndex);
#endif
    }
}
