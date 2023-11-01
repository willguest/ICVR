using System.Runtime.InteropServices;

public static class CanisterUtilsInternal
{
    [DllImport("__Internal")]
    public static extern void ICLogin(int cbIndex);

    [DllImport("__Internal")]
    public static extern void ICLogout(int cbIndex);
}

public static class CanisterUtils
{
    public static void StartIIAuth(int cbIndex)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CanisterUtilsInternal.ICLogin(cbIndex);
#endif
    }

    public static void EndIISession(int cbIndex)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CanisterUtilsInternal.ICLogout(cbIndex);
#endif
    }

}