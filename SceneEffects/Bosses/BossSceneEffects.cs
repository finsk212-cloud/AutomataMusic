using AutomataMusic.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AutomataMusic.SceneEffects.Bosses
{
	/// <summary>
	/// Early game bosses: Eye of Cthulhu, King Slime, Eater of Worlds, Brain of Cthulhu, Skeletron, Deerclops.
	/// Default track candidates: "BirthOfAWish", "Boss1", falling back to "PossessedByDisease" or "AlienManifestation"
	/// </summary>
	public class EarlyBossSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/BirthOfAWish", "Assets/Music/Boss1", "Assets/Music/PossessedByDisease", "Assets/Music/AlienManifestation" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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
	/// Default track candidates: "ABeautifulSong", "QueenBee", falling back to "AlienManifestation" or "PossessedByDisease"
	/// </summary>
	public class QueenBeeSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/ABeautifulSong", "Assets/Music/QueenBee", "Assets/Music/AlienManifestation", "Assets/Music/PossessedByDisease" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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
	/// Default track candidates: "GrandmaDestruction", "WallOfFlesh", falling back to "PossessedByDisease" or "AlienManifestation"
	/// </summary>
	public class WallOfFleshSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/GrandmaDestruction", "Assets/Music/WallOfFlesh", "Assets/Music/PossessedByDisease", "Assets/Music/AlienManifestation" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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
	/// Default track candidates: "DependentWeakling", "WarAndWar", "MechBoss", falling back to "PossessedByDisease" or "AlienManifestation"
	/// </summary>
	public class MechBossSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/DependentWeakling", "Assets/Music/WarAndWar", "Assets/Music/MechBoss", "Assets/Music/PossessedByDisease", "Assets/Music/AlienManifestation" };

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, Tracks);

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

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
	/// Default track candidates: "WeightOfTheWorld", "DarkColossusKaiju", "TheEndOfTheUnknown", "MoonLord"
	/// </summary>
	public class MoonLordSceneEffect : ModSceneEffect
	{
		private static readonly string[] Tracks = { "Assets/Music/WeightOfTheWorld", "Assets/Music/DarkColossusKaiju", "Assets/Music/TheEndOfTheUnknown", "Assets/Music/MoonLord" };

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
