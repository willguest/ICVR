using Newtonsoft.Json;
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
public class CallbackResponse
{
    [Preserve] public int cbIndex;
    [Preserve] public string error;
}


[Preserve]
public class CanisterResponse
{
    [JsonProperty("Error")]
    public CallFailedResponse ErrorDetails;
}

[Preserve]
public partial class CallFailedResponse
{
    [JsonProperty("Canister")]
    public string Canister;

    [JsonProperty("Method")]
    public string Method;

    [JsonProperty("Request ID")]
    public string RequestId;

    [JsonProperty("Reject code")]
    public int RejectCode;

    [JsonProperty("Reject message")]
    public string RejectMessage;
    
}


