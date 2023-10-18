using System.Runtime.InteropServices;

public static class TokenUtilsInternal
{
    [DllImport("__Internal")]
    public static extern void GetToken(int cbIndex);
}

public static class TokenUtils
{
    public static void RequestTokenFromFund(int cbIndex)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        TokenUtilsInternal.GetToken(cbIndex);
#endif
    }
}
