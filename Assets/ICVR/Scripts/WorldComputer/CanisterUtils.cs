using System.Runtime.InteropServices;

public static class CanisterUtilsInternal
{
    [DllImport("__Internal")]
    public static extern void ICLogin(int cbIndex);
}

public static class CanisterUtils
{
    public static void StartIIAuth(int cbIndex)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CanisterUtilsInternal.ICLogin(cbIndex);
#endif
    }

}