using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace AutomataMusic.Common
{
	public struct TrackInfo
	{
		public string Title;
		public string Subtitle;

		public TrackInfo(string title, string subtitle)
		{
			Title = title;
			Subtitle = subtitle;
		}
	}

	public static class MusicHelper
	{
		private static readonly Dictionary<string, int> slotCache = new();
		private static readonly Dictionary<int, TrackInfo> slotToInfo = new();

		private static readonly Dictionary<string, (string Title, string Subtitle)> KnownMetadata = new(StringComparer.OrdinalIgnoreCase)
		{
			// Forest / Surface
			{ "Assets/Music/CityRuins", ("City Ruins - Rays of Light", "NieR:Automata OST") },
			{ "Assets/Music/RaysOfLight", ("City Ruins - Rays of Light", "NieR:Automata OST") },
			{ "Assets/Music/SurfaceDay", ("City Ruins - Rays of Light", "NieR:Automata OST") },
			{ "Assets/Music/VoiceOfNoReturn", ("Voice of No Return", "NieR:Automata OST") },
			{ "Assets/Music/SurfaceNight", ("Voice of No Return", "NieR:Automata OST") },
			{ "Assets/Music/PeacefulSleep", ("Peaceful Sleep", "NieR:Automata OST (Resistance Camp)") },
			{ "Assets/Music/ResistanceCamp", ("Peaceful Sleep", "NieR:Automata OST (Resistance Camp)") },
			{ "Assets/Music/TownDay", ("Peaceful Sleep", "NieR:Automata OST (Resistance Camp)") },
			{ "Assets/Music/Town", ("Peaceful Sleep", "NieR:Automata OST (Resistance Camp)") },
			{ "Assets/Music/VagueHope", ("Vague Hope - Cold Rain", "NieR:Automata OST") },
			{ "Assets/Music/VagueHopeColdRain", ("Vague Hope - Cold Rain", "NieR:Automata OST") },
			{ "Assets/Music/ColdRain", ("Vague Hope - Cold Rain", "NieR:Automata OST") },
			{ "Assets/Music/SurfaceRain", ("Vague Hope - Cold Rain", "NieR:Automata OST") },
			{ "Assets/Music/Rain", ("Vague Hope - Cold Rain", "NieR:Automata OST") },
			{ "Assets/Music/ForestKingdom", ("Forest Kingdom", "NieR:Automata OST") },
			{ "Assets/Music/WindyDay", ("Forest Kingdom", "NieR:Automata OST") },

			// Bosses
			{ "Assets/Music/BirthOfAWish", ("Birth of a Wish", "NieR:Automata OST") },
			{ "Assets/Music/Boss1", ("Birth of a Wish", "NieR:Automata OST") },
			{ "Assets/Music/ABeautifulSong", ("A Beautiful Song", "NieR:Automata OST (Simone)") },
			{ "Assets/Music/QueenBee", ("A Beautiful Song", "NieR:Automata OST (Simone)") },
			{ "Assets/Music/GrandmaDestruction", ("Grandma - Destruction", "NieR:Automata OST") },
			{ "Assets/Music/WallOfFlesh", ("Grandma - Destruction", "NieR:Automata OST") },
			{ "Assets/Music/DependentWeakling", ("Dependent Weakling", "NieR:Automata OST") },
			{ "Assets/Music/WarAndWar", ("War & War", "NieR:Automata OST") },
			{ "Assets/Music/MechBoss", ("Dependent Weakling", "NieR:Automata OST") },
			{ "Assets/Music/AlienManifestation", ("Alien Manifestation", "NieR:Automata OST") },
			{ "Assets/Music/Plantera", ("Alien Manifestation", "NieR:Automata OST") },
			{ "Assets/Music/PossessedByDisease", ("Possessed by Disease", "NieR:Automata OST") },
			{ "Assets/Music/LateBoss", ("Possessed by Disease", "NieR:Automata OST") },
			{ "Assets/Music/DarkColossusKaiju", ("Dark Colossus - Kaiju", "NieR:Automata OST") },
			{ "Assets/Music/TheEndOfTheUnknown", ("The End of the Unknown", "NieR:Automata OST") },
			{ "Assets/Music/WeightOfTheWorld", ("Weight of the World (the End of YoRHa)", "NieR:Automata OST") },
			{ "Assets/Music/MoonLord", ("Weight of the World (the End of YoRHa)", "NieR:Automata OST") },

			// Biomes
			{ "Assets/Music/MemoriesOfDust", ("Memories of Dust", "NieR:Automata OST") },
			{ "Assets/Music/Desert", ("Memories of Dust", "NieR:Automata OST") },
			{ "Assets/Music/AmusementPark", ("Amusement Park", "NieR:Automata OST") },
			{ "Assets/Music/CopiedCity", ("Copied City", "NieR:Automata OST") },
			{ "Assets/Music/Underground", ("Amusement Park", "NieR:Automata OST") },
			{ "Assets/Music/KaineSalvation", ("Kainé - Salvation", "NieR Replicant / Automata") },
			{ "Assets/Music/Snow", ("Kainé - Salvation", "NieR Replicant / Automata") },
			{ "Assets/Music/Pascal", ("Pascal", "NieR:Automata OST") },
			{ "Assets/Music/Jungle", ("Forest Kingdom", "NieR:Automata OST") },
			{ "Assets/Music/WretchedWeaponry", ("Wretched Weaponry", "NieR:Automata OST") },
			{ "Assets/Music/EmilDespair", ("Emil - Despair", "NieR:Automata OST") },
			{ "Assets/Music/EvilBiome", ("Wretched Weaponry", "NieR:Automata OST") },
			{ "Assets/Music/TheSoundOfTheEnd", ("The Sound of the End", "NieR:Automata OST") },
			{ "Assets/Music/Underworld", ("The Sound of the End", "NieR:Automata OST") },
			{ "Assets/Music/TreasuredTimes", ("Treasured Times", "NieR:Automata OST") },
			{ "Assets/Music/Ocean", ("Treasured Times", "NieR:Automata OST") }
		};

		/// <summary>
		/// Checks whether an audio file exists in the mod at the given path (e.g. "Assets/Music/CityRuins").
		/// Supports .ogg, .mp3, and .wav automatically.
		/// </summary>
		public static bool HasTrack(Mod mod, string relativePath)
		{
			return MusicLoader.MusicExists(mod, relativePath);
		}

		/// <summary>
		/// Safely retrieves or caches the music slot index for a given path.
		/// Returns 0 if the track is not present in the mod assets.
		/// </summary>
		public static int GetTrack(Mod mod, string relativePath)
		{
			if (slotCache.TryGetValue(relativePath, out int cached))
				return cached;

			if (HasTrack(mod, relativePath))
			{
				int slot = MusicLoader.GetMusicSlot(mod, relativePath);
				slotCache[relativePath] = slot;

				if (KnownMetadata.TryGetValue(relativePath, out var meta))
				{
					slotToInfo[slot] = new TrackInfo(meta.Title, meta.Subtitle);
				}
				else
				{
					string name = Path.GetFileNameWithoutExtension(relativePath);
					slotToInfo[slot] = new TrackInfo(name, "NieR:Automata OST");
				}

				return slot;
			}

			return 0;
		}

		/// <summary>
		/// Checks multiple candidate file names and returns the music slot of the first one that exists.
		/// Returns 0 if none exist.
		/// </summary>
		public static int GetTrackWithCandidates(Mod mod, params string[] candidates)
		{
			foreach (string candidate in candidates)
			{
				if (HasTrack(mod, candidate))
					return GetTrack(mod, candidate);
			}
			return 0;
		}

		public static bool HasAnyTrack(Mod mod, params string[] candidates)
		{
			foreach (string candidate in candidates)
			{
				if (HasTrack(mod, candidate))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Pre-registers and caches all present tracks so that Now Playing detection is instant.
		/// </summary>
		public static void PreWarmAll(Mod mod)
		{
			foreach (var path in KnownMetadata.Keys)
			{
				if (HasTrack(mod, path))
				{
					GetTrack(mod, path);
				}
			}
		}

		/// <summary>
		/// Checks whether a given music slot belongs to this mod and outputs its metadata.
		/// </summary>
		public static bool TryGetTrackInfo(int slot, out TrackInfo info)
		{
			return slotToInfo.TryGetValue(slot, out info);
		}

		public static void Unload()
		{
			slotCache.Clear();
			slotToInfo.Clear();
		}
	}
}
