using Newtonsoft.Json;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

internal struct HeadhunterBuffEntry
{
    [JsonProperty("property")]
    public string Property { get; set; }

    [JsonProperty("added")]
    public bool Added { get; set; }

    [JsonProperty("increased")]
    public bool Increased { get; set; }

    [JsonProperty("max_added")]
    public int MaxAdded { get; set; }

    [JsonProperty("max_increased")]
    public int MaxIncreased { get; set; }
}
