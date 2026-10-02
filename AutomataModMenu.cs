using System;
using System.Diagnostics;
using AutomataMusic.Common;
using AutomataMusic.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataModMenu : ModMenu
	{
		private static readonly Stopwatch songStopwatch = new Stopwatch();

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		public static bool IsTrackActivelyPlaying(int musicId)
		{
			try
			{
				if (Terraria.Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && musicId >= 0 && musicId < legacy.AudioTracks.Length)
				{
					var track = legacy.AudioTracks[musicId];
					return track != null && track.IsPlaying;
				}
			}
			catch { }

			return Terraria.Main.curMusic == musicId;
		}

		public override void Update(bool isOnTitleScreen)
		{
			if (!isOnTitleScreen)
			{
				if (songStopwatch.IsRunning)
					songStopwatch.Reset();
				return;
			}

			bool isPlaying = IsTrackActivelyPlaying(Music);

			if (isPlaying)
			{
				if (!songStopwatch.IsRunning)
				{
					songStopwatch.Restart();
				}
				else if (songStopwatch.Elapsed.TotalSeconds > 344.607)
				{
					// Song finished and looped; restart stopwatch
					songStopwatch.Restart();
				}
			}
			else
			{
				if (songStopwatch.IsRunning)
				{
					songStopwatch.Reset();
				}
			}
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			if (!IsTrackActivelyPlaying(Music) || !songStopwatch.IsRunning)
				return;

			float elapsedSeconds = (float)songStopwatch.Elapsed.TotalSeconds;
			MenuLyrics.DrawLyrics(spriteBatch, elapsedSeconds, 1f);
		}
	}
}
