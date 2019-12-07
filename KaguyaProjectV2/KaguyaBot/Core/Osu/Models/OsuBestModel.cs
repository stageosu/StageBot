﻿using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Models
{
    public class OsuBestModel : OsuScoreableModel
    {
        public int play_number { get; set; }
        public int beatmap_id { get; set; }
        public int score { get; set; }
        public double starrating { get; set; }
        public int maxcombo { get; set; }
        public int perfect { get; set; }
        public int enabled_mods { get; set; }
        public string string_mods { get; set; }
        public string user_id { get; set; }
        public string rank { get; set; }
        public string rankemote { get; set; }
        public double pp { get; set; }
        public double accuracy { get; set; }
        public DateTime date { get; set; }
        public OsuBeatmapModel beatmap { get; set; }
    }
}
