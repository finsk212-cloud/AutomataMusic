using AutomataMusic.Common;
using Terraria;
using Terraria.ModLoader;

namespace AutomataMusic.SceneEffects.Biomes
{
	/// <summary>
	/// Forest / Surface Day theme.
	/// Default track candidates: "CityRuins" or "SurfaceDay"
	/// </summary>
	public class SurfaceDaySceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/CityRuins", "Assets/Music/RaysOfLight", "Assets/Music/SurfaceDay" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceSurfaceThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneOverworldHeight
				&& Main.dayTime
				&& !player.ZoneDesert
				&& !player.ZoneSnow
				&& !player.ZoneJungle
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow
				&& !player.ZoneBeach;
		}
	}

	/// <summary>
	/// Forest / Surface Night theme.
	/// Default track candidates: "VoiceOfNoReturn" or "SurfaceNight"
	/// </summary>
	public class SurfaceNightSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/VoiceOfNoReturn", "Assets/Music/SurfaceNight" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceSurfaceThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneOverworldHeight
				&& !Main.dayTime
				&& !player.ZoneDesert
				&& !player.ZoneSnow
				&& !player.ZoneJungle
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow
				&& !player.ZoneBeach;
		}
	}

	/// <summary>
	/// Forest Town / Resistance Camp theme (when near 2+ NPCs in the forest).
	/// Default track candidates: "PeacefulSleep", "ResistanceCamp", "TownDay", or "Town"
	/// </summary>
	public class SurfaceTownSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/PeacefulSleep", "Assets/Music/ResistanceCamp", "Assets/Music/TownDay", "Assets/Music/Town" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceTownThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneOverworldHeight
				&& player.townNPCs >= 2f
				&& !player.ZoneDesert
				&& !player.ZoneSnow
				&& !player.ZoneJungle
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow
				&& !player.ZoneBeach;
		}
	}

	/// <summary>
	/// Forest Rain theme.
	/// Default track candidates: "VagueHope", "VagueHopeColdRain", "ColdRain", "SurfaceRain", or "Rain"
	/// </summary>
	public class SurfaceRainSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/VagueHope", "Assets/Music/VagueHopeColdRain", "Assets/Music/ColdRain", "Assets/Music/SurfaceRain", "Assets/Music/Rain" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceRainThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneOverworldHeight
				&& Main.raining
				&& !player.ZoneDesert
				&& !player.ZoneSnow
				&& !player.ZoneJungle
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow
				&& !player.ZoneBeach;
		}
	}

	/// <summary>
	/// Forest Windy Day theme.
	/// Default track candidates: "ForestKingdom", "WindyDay"
	/// </summary>
	public class SurfaceWindySceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/ForestKingdom", "Assets/Music/WindyDay" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceWindyThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneOverworldHeight
				&& Main.dayTime
				&& Main.IsItAHappyWindyDay
				&& !player.ZoneDesert
				&& !player.ZoneSnow
				&& !player.ZoneJungle
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow
				&& !player.ZoneBeach;
		}
	}

	/// <summary>
	/// Underground / Cavern theme.
	/// Default track candidates: "AmusementPark", "CopiedCity", or "Underground"
	/// </summary>
	public class UndergroundSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/AmusementPark", "Assets/Music/CopiedCity", "Assets/Music/Underground" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceUndergroundThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight)
				&& !player.ZoneUnderworldHeight
				&& !player.ZoneJungle
				&& !player.ZoneSnow
				&& !player.ZoneDesert
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHallow;
		}
	}

	/// <summary>
	/// Desert / Underground Desert theme.
	/// Default track candidates: "MemoriesOfDust" or "Desert"
	/// </summary>
	public class DesertSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/MemoriesOfDust", "Assets/Music/Desert" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceDesertThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneDesert || player.ZoneUndergroundDesert;
		}
	}

	/// <summary>
	/// Snow / Ice Biome theme.
	/// Default track candidates: "KaineSalvation", "PeacefulSleep", or "Snow"
	/// </summary>
	public class SnowSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/KaineSalvation", "Assets/Music/PeacefulSleep", "Assets/Music/Snow" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceSnowThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneSnow;
		}
	}

	/// <summary>
	/// Jungle / Underground Jungle theme.
	/// Default track candidates: "ForestKingdom", "Pascal", or "Jungle"
	/// </summary>
	public class JungleSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/ForestKingdom", "Assets/Music/Pascal", "Assets/Music/Jungle" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceJungleThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneJungle;
		}
	}

	/// <summary>
	/// Corruption / Crimson / Graveyard evil theme.
	/// Default track candidates: "WretchedWeaponry", "EmilDespair", or "EvilBiome"
	/// </summary>
	public class EvilBiomeSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/WretchedWeaponry", "Assets/Music/EmilDespair", "Assets/Music/EvilBiome" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceEvilBiomeThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneCorrupt || player.ZoneCrimson || player.ZoneGraveyard;
		}
	}

	/// <summary>
	/// Underworld (Hell) theme.
	/// Default track candidates: "TheSoundOfTheEnd" or "Underworld"
	/// </summary>
	public class UnderworldSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/TheSoundOfTheEnd", "Assets/Music/Underworld" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceUnderworldThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneUnderworldHeight;
		}
	}

	/// <summary>
	/// Ocean / Beach theme.
	/// Default track candidates: "TreasuredTimes" or "Ocean"
	/// </summary>
	public class OceanSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/TreasuredTimes", "Assets/Music/Ocean" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			var cfg = AutomataMusicConfig.Instance;
			if (!cfg.ReplaceBiomeThemes || !cfg.ReplaceOceanThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return player.ZoneBeach;
		}
	}
}
