using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AutomataMusic.Common
{
	public class AutomataMusicConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static AutomataMusicConfig Instance => Terraria.ModLoader.ModContent.GetInstance<AutomataMusicConfig>();

		[Header("GeneralSettings")]

		[DefaultValue(true)]
		public bool ReplaceBossThemes;

		[DefaultValue(true)]
		public bool ReplaceBiomeThemes;

		[DefaultValue(true)]
		public bool ShowNowPlayingNotification;

		[Header("IndividualOverrides")]

		[DefaultValue(true)]
		public bool ReplaceSurfaceThemes;

		[DefaultValue(true)]
		public bool ReplaceTownThemes;

		[DefaultValue(true)]
		public bool ReplaceRainThemes;

		[DefaultValue(true)]
		public bool ReplaceWindyThemes;

		[DefaultValue(true)]
		public bool ReplaceUndergroundThemes;

		[DefaultValue(true)]
		public bool ReplaceDesertThemes;

		[DefaultValue(true)]
		public bool ReplaceSnowThemes;

		[DefaultValue(true)]
		public bool ReplaceJungleThemes;

		[DefaultValue(true)]
		public bool ReplaceEvilBiomeThemes;

		[DefaultValue(true)]
		public bool ReplaceUnderworldThemes;

		[DefaultValue(true)]
		public bool ReplaceOceanThemes;
	}
}
