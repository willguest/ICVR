using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

[Serializable]
public class Peers
{
}

[Serializable]
public class SocketOptions
{
    [JsonProperty("transport")]
    public string Transport { get; set; }
}

[Serializable]
public class PeersBackup
{
}



[Serializable]
public class Session
{
    [JsonProperty("audio")]
    public bool Audio { get; set; }

    [JsonProperty("video")]
    public bool Video { get; set; }

    [JsonProperty("data")]
    public bool Data { get; set; }
}

[Serializable]
public class Bandwidth
{
    [JsonProperty("screen")]
    public bool Screen { get; set; }

    [JsonProperty("audio")]
    public bool Audio { get; set; }

    [JsonProperty("video")]
    public bool Video { get; set; }
}

[Serializable]
public class Codecs
{
    [JsonProperty("audio")]
    public string Audio { get; set; }

    [JsonProperty("video")]
    public string Video { get; set; }
}

[Serializable]
public class CodecsHandler
{
}

[Serializable]
public class BandwidthHandler
{
}

[Serializable]
public class MediaConstraints
{
    [JsonProperty("audio")]
    public bool Audio { get; set; }

    [JsonProperty("video")]
    public bool Video { get; set; }
}

[Serializable]
public class Mandatory
{
    [JsonProperty("OfferToReceiveAudio")]
    public bool OfferToReceiveAudio { get; set; }

    [JsonProperty("OfferToReceiveVideo")]
    public bool OfferToReceiveVideo { get; set; }
}

[Serializable]
public class Optional
{
    [JsonProperty("VoiceActivityDetection")]
    public bool VoiceActivityDetection { get; set; }

    [JsonProperty("DtlsSrtpKeyAgreement")]
    public bool DtlsSrtpKeyAgreement { get; set; }

    [JsonProperty("googImprovedWifiBwe")]
    public bool? GoogImprovedWifiBwe { get; set; }

    [JsonProperty("googScreencastMinBitrate")]
    public int? GoogScreencastMinBitrate { get; set; }

    [JsonProperty("googIPv6")]
    public bool? GoogIPv6 { get; set; }

    [JsonProperty("googDscp")]
    public bool? GoogDscp { get; set; }

    [JsonProperty("googCpuUnderuseThreshold")]
    public int? GoogCpuUnderuseThreshold { get; set; }

    [JsonProperty("googCpuOveruseThreshold")]
    public int? GoogCpuOveruseThreshold { get; set; }

    [JsonProperty("googSuspendBelowMinBitrate")]
    public bool? GoogSuspendBelowMinBitrate { get; set; }

    [JsonProperty("googCpuOveruseDetection")]
    public bool? GoogCpuOveruseDetection { get; set; }
}

[Serializable]
public class SdpConstraints
{
    [JsonProperty("mandatory")]
    public Mandatory Mandatory { get; set; }

    [JsonProperty("optional")]
    public List<Optional> Optional { get; set; }
}

[Serializable]
public class OptionalArgument
{
    [JsonProperty("optional")]
    public List<Optional> Optional { get; set; }

    [JsonProperty("mandatory")]
    public Mandatory Mandatory { get; set; }
}

[Serializable]
public class IceServer
{
    //[JsonProperty("urls")]
    //public List<string> Urls { get; set; }

    [JsonProperty("urls")]
    public object Urls { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("credential")]
    public string Credential { get; set; }
}

[Serializable]
public class Candidates
{
    [JsonProperty("host")]
    public bool Host { get; set; }

    [JsonProperty("stun")]
    public bool Stun { get; set; }

    [JsonProperty("turn")]
    public bool Turn { get; set; }
}

[Serializable]
public class IceProtocols
{
    [JsonProperty("tcp")]
    public bool Tcp { get; set; }

    [JsonProperty("udp")]
    public bool Udp { get; set; }
}

[Serializable]
public class VideosContainer
{
}

[Serializable]
public class FilesContainer
{
}

[Serializable]
public class Translator
{
}

[Serializable]
public class StreamsHandler
{
}

[Serializable]
public class StreamEvents
{
}

[Serializable]
public class Browser
{
    [JsonProperty("fullVersion")]
    public string FullVersion { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("isPrivateBrowsing")]
    public bool IsPrivateBrowsing { get; set; }

    [JsonProperty("isFirefox")]
    public bool IsFirefox { get; set; }
}

[Serializable]
public class DetectRTC
{
    [JsonProperty("browser")]
    public Browser Browser { get; set; }

    [JsonProperty("osName")]
    public string OsName { get; set; }

    [JsonProperty("osVersion")]
    public string OsVersion { get; set; }

    [JsonProperty("isWebRTCSupported")]
    public bool IsWebRTCSupported { get; set; }

    [JsonProperty("isORTCSupported")]
    public bool IsORTCSupported { get; set; }

    [JsonProperty("isScreenCapturingSupported")]
    public bool IsScreenCapturingSupported { get; set; }

    [JsonProperty("isAudioContextSupported")]
    public bool IsAudioContextSupported { get; set; }

    [JsonProperty("isCreateMediaStreamSourceSupported")]
    public bool IsCreateMediaStreamSourceSupported { get; set; }

    [JsonProperty("isRtpDataChannelsSupported")]
    public bool IsRtpDataChannelsSupported { get; set; }

    [JsonProperty("isSctpDataChannelsSupported")]
    public bool IsSctpDataChannelsSupported { get; set; }

    [JsonProperty("isMobileDevice")]
    public bool IsMobileDevice { get; set; }

    [JsonProperty("isGetUserMediaSupported")]
    public bool IsGetUserMediaSupported { get; set; }

    [JsonProperty("displayResolution")]
    public string DisplayResolution { get; set; }

    [JsonProperty("displayAspectRatio")]
    public string DisplayAspectRatio { get; set; }

    [JsonProperty("isCanvasSupportsStreamCapturing")]
    public bool IsCanvasSupportsStreamCapturing { get; set; }

    [JsonProperty("isVideoSupportsStreamCapturing")]
    public bool IsVideoSupportsStreamCapturing { get; set; }

    [JsonProperty("isWebSocketsSupported")]
    public bool IsWebSocketsSupported { get; set; }

    [JsonProperty("isWebSocketsBlocked")]
    public bool IsWebSocketsBlocked { get; set; }

    [JsonProperty("MediaDevices")]
    public List<object> MediaDevices { get; set; }

    [JsonProperty("hasMicrophone")]
    public bool HasMicrophone { get; set; }

    [JsonProperty("hasSpeakers")]
    public bool HasSpeakers { get; set; }

    [JsonProperty("hasWebcam")]
    public bool HasWebcam { get; set; }

    [JsonProperty("isWebsiteHasWebcamPermissions")]
    public bool IsWebsiteHasWebcamPermissions { get; set; }

    [JsonProperty("isWebsiteHasMicrophonePermissions")]
    public bool IsWebsiteHasMicrophonePermissions { get; set; }

    [JsonProperty("audioInputDevices")]
    public List<object> AudioInputDevices { get; set; }

    [JsonProperty("audioOutputDevices")]
    public List<object> AudioOutputDevices { get; set; }

    [JsonProperty("videoInputDevices")]
    public List<object> VideoInputDevices { get; set; }

    [JsonProperty("isSetSinkIdSupported")]
    public bool IsSetSinkIdSupported { get; set; }

    [JsonProperty("isRTPSenderReplaceTracksSupported")]
    public bool IsRTPSenderReplaceTracksSupported { get; set; }

    [JsonProperty("isRemoteStreamProcessingSupported")]
    public bool IsRemoteStreamProcessingSupported { get; set; }

    [JsonProperty("isApplyConstraintsSupported")]
    public bool IsApplyConstraintsSupported { get; set; }

    [JsonProperty("isMultiMonitorScreenCapturingSupported")]
    public bool IsMultiMonitorScreenCapturingSupported { get; set; }

    [JsonProperty("isPromisesSupported")]
    public bool IsPromisesSupported { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("MediaStream")]
    public List<string> MediaStream { get; set; }

    [JsonProperty("MediaStreamTrack")]
    public bool MediaStreamTrack { get; set; }

    [JsonProperty("RTCPeerConnection")]
    public List<string> RTCPeerConnection { get; set; }
}

[Serializable]
public class MultiPeersHandler
{
}

[Serializable]
public class Errors
{
    [JsonProperty("ROOM_NOT_AVAILABLE")]
    public string ROOMNOTAVAILABLE { get; set; }

    [JsonProperty("INVALID_PASSWORD")]
    public string INVALIDPASSWORD { get; set; }

    [JsonProperty("USERID_NOT_AVAILABLE")]
    public string USERIDNOTAVAILABLE { get; set; }

    [JsonProperty("ROOM_PERMISSION_DENIED")]
    public string ROOMPERMISSIONDENIED { get; set; }

    [JsonProperty("ROOM_FULL")]
    public string ROOMFULL { get; set; }

    [JsonProperty("DID_NOT_JOIN_ANY_ROOM")]
    public string DIDNOTJOINANYROOM { get; set; }

    [JsonProperty("INVALID_SOCKET")]
    public string INVALIDSOCKET { get; set; }

    [JsonProperty("PUBLIC_IDENTIFIER_MISSING")]
    public string PUBLICIDENTIFIERMISSING { get; set; }

    [JsonProperty("INVALID_ADMIN_CREDENTIAL")]
    public string INVALIDADMINCREDENTIAL { get; set; }
}

[Serializable]
public class RtcMultiConnection
{
    [JsonProperty("sessionid")]
    public string Sessionid { get; set; }

    [JsonProperty("channel")]
    public string Channel { get; set; }

    [JsonProperty("peers")]
    public Peers Peers { get; set; }

    [JsonProperty("socketOptions")]
    public SocketOptions SocketOptions { get; set; }

    [JsonProperty("waitingForLocalMedia")]
    public bool WaitingForLocalMedia { get; set; }

    [JsonProperty("peersBackup")]
    public PeersBackup PeersBackup { get; set; }

    [JsonProperty("publicRoomIdentifier")]
    public string PublicRoomIdentifier { get; set; }

    [JsonProperty("closeBeforeUnload")]
    public bool CloseBeforeUnload { get; set; }

    [JsonProperty("userid")]
    public string Userid { get; set; }

    [JsonProperty("extra")]
    public Extra Extra { get; set; }

    [JsonProperty("attachStreams")]
    public List<object> AttachStreams { get; set; }

    [JsonProperty("session")]
    public Session Session { get; set; }

    [JsonProperty("enableFileSharing")]
    public bool EnableFileSharing { get; set; }

    [JsonProperty("bandwidth")]
    public Bandwidth Bandwidth { get; set; }

    [JsonProperty("codecs")]
    public Codecs Codecs { get; set; }

    [JsonProperty("CodecsHandler")]
    public CodecsHandler CodecsHandler { get; set; }

    [JsonProperty("BandwidthHandler")]
    public BandwidthHandler BandwidthHandler { get; set; }

    [JsonProperty("mediaConstraints")]
    public MediaConstraints MediaConstraints { get; set; }

    [JsonProperty("sdpConstraints")]
    public SdpConstraints SdpConstraints { get; set; }

    [JsonProperty("sdpSemantics")]
    public object SdpSemantics { get; set; }

    [JsonProperty("iceCandidatePoolSize")]
    public object IceCandidatePoolSize { get; set; }

    [JsonProperty("bundlePolicy")]
    public object BundlePolicy { get; set; }

    [JsonProperty("rtcpMuxPolicy")]
    public object RtcpMuxPolicy { get; set; }

    [JsonProperty("iceTransportPolicy")]
    public object IceTransportPolicy { get; set; }

    [JsonProperty("optionalArgument")]
    public OptionalArgument OptionalArgument { get; set; }

    [JsonProperty("iceServers")]
    public List<IceServer> IceServers { get; set; }

    [JsonProperty("candidates")]
    public Candidates Candidates { get; set; }

    [JsonProperty("iceProtocols")]
    public IceProtocols IceProtocols { get; set; }

    [JsonProperty("direction")]
    public string Direction { get; set; }

    [JsonProperty("autoCloseEntireSession")]
    public bool AutoCloseEntireSession { get; set; }

    [JsonProperty("videosContainer")]
    public VideosContainer VideosContainer { get; set; }

    [JsonProperty("filesContainer")]
    public FilesContainer FilesContainer { get; set; }

    [JsonProperty("isInitiator")]
    public bool IsInitiator { get; set; }

    [JsonProperty("autoTranslateText")]
    public bool AutoTranslateText { get; set; }

    [JsonProperty("language")]
    public string Language { get; set; }

    [JsonProperty("googKey")]
    public string GoogKey { get; set; }

    [JsonProperty("Translator")]
    public Translator Translator { get; set; }

    [JsonProperty("StreamsHandler")]
    public StreamsHandler StreamsHandler { get; set; }

    [JsonProperty("streamEvents")]
    public StreamEvents StreamEvents { get; set; }

    [JsonProperty("socketURL")]
    public string SocketURL { get; set; }

    [JsonProperty("socketMessageEvent")]
    public string SocketMessageEvent { get; set; }

    [JsonProperty("socketCustomEvent")]
    public string SocketCustomEvent { get; set; }

    [JsonProperty("DetectRTC")]
    public DetectRTC DetectRTC { get; set; }

    [JsonProperty("multiPeersHandler")]
    public MultiPeersHandler MultiPeersHandler { get; set; }

    [JsonProperty("enableLogs")]
    public bool EnableLogs { get; set; }

    [JsonProperty("chunkSize")]
    public int ChunkSize { get; set; }

    [JsonProperty("maxParticipantsAllowed")]
    public int MaxParticipantsAllowed { get; set; }

    [JsonProperty("enableScalableBroadcast")]
    public bool EnableScalableBroadcast { get; set; }

    [JsonProperty("maxRelayLimitPerUser")]
    public int MaxRelayLimitPerUser { get; set; }

    [JsonProperty("dontCaptureUserMedia")]
    public bool DontCaptureUserMedia { get; set; }

    [JsonProperty("dontAttachStream")]
    public bool DontAttachStream { get; set; }

    [JsonProperty("dontGetRemoteStream")]
    public bool DontGetRemoteStream { get; set; }

    [JsonProperty("isOnline")]
    public bool IsOnline { get; set; }

    [JsonProperty("isLowBandwidth")]
    public bool IsLowBandwidth { get; set; }

    [JsonProperty("trickleIce")]
    public bool TrickleIce { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("autoCreateMediaElement")]
    public bool AutoCreateMediaElement { get; set; }

    [JsonProperty("password")]
    public object Password { get; set; }

    [JsonProperty("errors")]
    public Errors Errors { get; set; }
}

public partial class RoomObject
{
    [JsonProperty("maxParticipantsAllowed")]
    public long MaxParticipantsAllowed { get; set; }

    [JsonProperty("owner")]
    public string Owner { get; set; }

    [JsonProperty("participants")]
    public string[] Participants { get; set; }

    [JsonProperty("extra")]
    public Extra Extra { get; set; }

    [JsonProperty("session")]
    public Session Session { get; set; }

    [JsonProperty("sessionid")]
    public string Sessionid { get; set; }

    [JsonProperty("isRoomFull")]
    public bool IsRoomFull { get; set; }

    [JsonProperty("isPasswordProtected")]
    public bool IsPasswordProtected { get; set; }
}

