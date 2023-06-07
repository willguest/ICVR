using System;
using Unity.Plastic.Newtonsoft.Json;

namespace ICVR
{
    [Serializable]
    public class NavigatorData
    {
        [JsonProperty("data")]
        public string ua { get; set; }

        [JsonProperty("browser")]
        public UA_Browser Browser { get; set; }

        [JsonProperty("engine")]
        public UA_Engine Engine { get; set; }

        [JsonProperty("os")]
        public UA_OS OS { get; set; }

        [JsonProperty("device")]
        public UA_Device Device { get; set; }

        [JsonProperty("cpu")]
        public UA_CPU CPU { get; set; }
    }

    [Serializable]
    public class UA_Browser
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("major")]
        public string Major { get; set; }
    }

    [Serializable]
    public class UA_Engine
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    [Serializable]
    public class UA_OS
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    [Serializable]
    public class UA_Device
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }
    }

    [Serializable]
    public class UA_CPU
    {
        [JsonProperty("architecture")]
        public string Architecture { get; set; }
    }
}