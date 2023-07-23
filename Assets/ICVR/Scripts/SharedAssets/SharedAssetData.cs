using System;
using Newtonsoft.Json;

namespace ICVR.SharedAssets
{
    [Serializable]
    public enum AvatarInteractionEventType
    {
        None = 0,
        AcquireData = 1,
        ReleaseData = 2
    }

    [Serializable]
    public enum ManipulationDistance
    {
        None = 0,
        Near = 1,
        Far = 2
    }

    [Serializable]
    public partial class AvatarHandlingData
    {
        [JsonProperty("hand")]
        public ControllerHand Hand = ControllerHand.NONE;

        [JsonProperty("targetId")]
        public string TargetId = "";

        [JsonProperty("distance")]
        public ManipulationDistance Distance = ManipulationDistance.None;

        [JsonProperty("eventType")]
        public AvatarInteractionEventType EventType = AvatarInteractionEventType.None;

        [JsonProperty("acquisitionEvent")]
        public AcquireData AcquisitionEvent { get; set; }

        [JsonProperty("releaseEvent")]
        public ReleaseData ReleaseEvent { get; set; }
    }

    [Serializable]
    public partial class AcquireData
    {
        [JsonProperty("acqTime")]
        public long AcqTime = 0;

        [JsonProperty("objectPosition")]
        public SVector3 ObjectPosition = new SVector3(0f, 0f, 0f);
        [JsonProperty("objectRotation")]
        public SQuaternion ObjectRotation = new SQuaternion(0f, 0f, 0f, 1f);
    }

    [Serializable]
    public partial class ReleaseData
    {
        [JsonProperty("releaseTime")]
        public long ReleaseTime = 0;

        [JsonProperty("releasePosition")]
        public SVector3 ReleasePosition = new SVector3(0f, 0f, 0f);
        [JsonProperty("releaseRotation")]
        public SQuaternion ReleaseRotation = new SQuaternion(0f, 0f, 0f, 1f);

        [JsonProperty("forceData")]
        public ThrowData ForceData;
    }
}
