using AutomataMusic.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AutomataMusic.SceneEffects.Bosses
{
	/// <summary>
	/// Early game bosses: Eye of Cthulhu, King Slime, Eater of Worlds, Brain of Cthulhu, Skeletron, Deerclops.
	/// Default track candidates: "BirthOfAWish" or "Boss1"
	/// </summary>
	public class EarlyBossSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/BirthOfAWish", "Assets/Music/Boss1" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.EyeofCthulhu)
				|| NPC.AnyNPCs(NPCID.KingSlime)
				|| NPC.AnyNPCs(NPCID.EaterofWorldsHead)
				|| NPC.AnyNPCs(NPCID.BrainofCthulhu)
				|| NPC.AnyNPCs(NPCID.SkeletronHead)
				|| NPC.AnyNPCs(NPCID.Deerclops);
		}
	}

	/// <summary>
	/// Queen Bee boss theme.
	/// Default track candidates: "ABeautifulSong" or "QueenBee"
	/// </summary>
	public class QueenBeeSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/ABeautifulSong", "Assets/Music/QueenBee" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.QueenBee);
		}
	}

	/// <summary>
	/// Wall of Flesh boss theme.
	/// Default track candidates: "GrandmaDestruction" or "WallOfFlesh"
	/// </summary>
	public class WallOfFleshSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/GrandmaDestruction", "Assets/Music/WallOfFlesh" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.WallofFlesh);
		}
	}

	/// <summary>
	/// Mechanical bosses: The Destroyer, The Twins (Retinazer / Spazmatism), Skeletron Prime.
	/// Default track candidates: "DependentWeakling", "WarAndWar", or "MechBoss"
	/// </summary>
	public class MechBossSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/DependentWeakling", "Assets/Music/WarAndWar", "Assets/Music/MechBoss" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.TheDestroyer)
				|| NPC.AnyNPCs(NPCID.Retinazer)
				|| NPC.AnyNPCs(NPCID.Spazmatism)
				|| NPC.AnyNPCs(NPCID.SkeletronPrime);
		}
	}

	/// <summary>
	/// Plantera boss theme.
	/// Default track candidates: "AlienManifestation" or "Plantera"
	/// </summary>
	public class PlanteraSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/AlienManifestation", "Assets/Music/Plantera" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.Plantera);
		}
	}

	/// <summary>
	/// Post-Plantera bosses: Golem, Duke Fishron, Empress of Light, Lunatic Cultist.
	/// Default track candidates: "PossessedByDisease" or "LateBoss"
	/// </summary>
	public class LateBossSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/PossessedByDisease", "Assets/Music/LateBoss" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.Golem)
				|| NPC.AnyNPCs(NPCID.DukeFishron)
				|| NPC.AnyNPCs(NPCID.HallowBoss)
				|| NPC.AnyNPCs(NPCID.CultistBoss);
		}
	}

	/// <summary>
	/// Moon Lord boss theme.
	/// Default track candidates: "DarkColossusKaiju", "TheEndOfTheUnknown", "WeightOfTheWorld", or "MoonLord"
	/// </summary>
	public class MoonLordSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/DarkColossusKaiju", "Assets/Music/TheEndOfTheUnknown", "Assets/Music/WeightOfTheWorld", "Assets/Music/MoonLord" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

		public override bool IsSceneEffectActive(Player player)
		{
			if (!AutomataMusicConfig.Instance.ReplaceBossThemes)
				return false;

			if (!MusicHelper.HasAnyTrack(Mod, Tracks))
				return false;

			return NPC.AnyNPCs(NPCID.MoonLordCore);
		}
	}
}
