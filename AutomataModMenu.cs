using AutomataMusic.Common;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataModMenu : ModMenu
	{
		public const float SongDuration = AutomataMusicSystem.SongDuration;

		public static float CurrentSongTime => AutomataMusicSystem.CurrentSongTime;

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		public static void SeekTo(float targetSeconds) => AutomataMusicSystem.SeekTo(targetSeconds);
	}
}
