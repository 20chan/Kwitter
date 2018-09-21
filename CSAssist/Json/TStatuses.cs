using System.Collections.Generic;
using Newtonsoft.Json;

namespace CSAssist.Json
{
    public class TStatuses : List<TStatus>
    {
    }

    public class TStatus
    {
        [JsonProperty(PropertyName = "id")]
        public long ID;
        [JsonProperty(PropertyName = "full_text")]
        public string Text;
        [JsonProperty(PropertyName = "user")]
        public TStatusUser User;
    }

    public class TStatusUser
    {
        [JsonProperty(PropertyName = "id")]
        public long ID;
        [JsonProperty(PropertyName = "name")]
        public string Nickname;
        [JsonProperty(PropertyName = "screen_name")]
        public string UserName;
    }
}
