using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;


[Preserve]
public class AuthResponse
{
    [Preserve] public int cbIndex;
    [Preserve] public bool result;
    [Preserve] public string principal;
    [Preserve] public string accountId;
}

[Preserve]
public class CoinRequestResponse
{
    [Preserve] public int cbIndex;
    [Preserve] public bool result;
    [Preserve] public int fundCount;
}

[Preserve]
public class CallbackResponse
{
    [Preserve] public int cbIndex;
    [Preserve] public string error;
}

[Preserve]
public class UserProfile
{
    public string principal;
    public string accountId;
    public string status;
}
