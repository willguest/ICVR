using System;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class NodeInputData
{
    [JsonProperty("data")]
    public string Data { get; set; }

    [JsonProperty("userid")]
    public string Userid { get; set; }

    [JsonProperty("extra")]
    public Extra Extra { get; set; }

    [JsonProperty("latency")]
    public long Latency { get; set; }
}

[Serializable]
public partial class Extra
{
    [JsonProperty("_room")]
    public Room Room { get; set; }
}

[Serializable]
public class Room
{
    [JsonProperty("isFull")]
    public bool IsFull { get; set; }
}

[Serializable]
public class NodeDataFrame
{
    [JsonProperty("id")]
    public string Id = "";

    [JsonProperty("headPosition")]
    public SVector3 HeadPosition = Vector3.zero;

    [JsonProperty("headRotation")]
    public SQuaternion HeadRotation = Quaternion.identity;

    [JsonProperty("leftHandPosition")]
    public SVector3 LeftHandPosition = Vector3.zero;

    [JsonProperty("rightHandPosition")]
    public SVector3 RightHandPosition = Vector3.zero;

    [JsonProperty("leftHandRotation")]
    public SQuaternion LeftHandRotation = Quaternion.identity;

    [JsonProperty("rightHandRotation")]
    public SQuaternion RightHandRotation = Quaternion.identity;
    
    [JsonProperty("leftHandPointer")]
    public SVector3 LeftHandPointer = Vector3.zero;

    [JsonProperty("rightHandPointer")]
    public SVector3 RightHandPointer = Vector3.zero;

    [JsonProperty("eventType")]
    public AvatarEventType EventType = AvatarEventType.None;

    [JsonProperty("eventData")]
    public string EventData = "";

}

[Serializable]
public enum AvatarEventType
{
    None = 0,
    Interaction = 1,
    Chat = 2
}

[Serializable]
public enum ControllerHand
{
    NONE = 0,
    LEFT = 1,
    RIGHT = 2,
    BOTH = 3
}


[Serializable]
public partial class ConnectionData
{
    [JsonProperty("isTrusted")]
    public bool IsTrusted { get; set; }

    [JsonProperty("userid")]
    public string Userid { get; set; }

    [JsonProperty("extra")]
    public Extra Extra { get; set; }
}

[Serializable]
public partial class AudioStreamLight
{
    public string Url { get; set; }
}


[Serializable]
public partial class AudioStream
{
    [JsonProperty("stream")]
    public Stream Stream { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("userid")]
    public string Userid { get; set; }

    [JsonProperty("extra")]
    public Extra Extra { get; set; }

    [JsonProperty("mediaElement")]
    public MediaElement MediaElement { get; set; }

    [JsonProperty("streamid")]
    public string Streamid { get; set; }
}

public partial class MediaElement
{
    [JsonProperty("URL")]
    public string Url { get; set; }
}

public partial class Stream
{
    [JsonProperty("isAudio")]
    public bool IsAudio { get; set; }

    [JsonProperty("isVideo")]
    public bool IsVideo { get; set; }

    [JsonProperty("isScreen")]
    public bool IsScreen { get; set; }

    [JsonProperty("streamid")]
    public string Streamid { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("alreadySetEndHandler")]
    public bool AlreadySetEndHandler { get; set; }
}



