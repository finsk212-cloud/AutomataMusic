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
		private static bool hasStartedPlaying = false;
		private static int previousMusic = -1;

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		public override void Update(bool isOnTitleScreen)
		{
			if (!isOnTitleScreen)
			{
				hasStartedPlaying = false;
				songStopwatch.Reset();
				previousMusic = -1;
				return;
			}

			int currentMusic = Terraria.Main.curMusic;

			// If music changed away from our menu track, stop
			if (currentMusic != Music)
			{
				hasStartedPlaying = false;
				songStopwatch.Reset();
				previousMusic = currentMusic;
				return;
			}

			// When Alt-Tabbed or game window lost focus, Terraria pauses audio: pause the stopwatch too!
			bool isAudioPaused = !Terraria.Main.hasFocus || SoundEngine.AreSoundsPaused;
			if (isAudioPaused)
			{
				if (songStopwatch.IsRunning)
				{
					songStopwatch.Stop();
				}
				return;
			}

			// Wait until the audio track actually starts producing sound
			if (!hasStartedPlaying)
			{
				bool isTrackReady = false;
				try
				{
					if (Terraria.Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && Music >= 0 && Music < legacy.AudioTracks.Length)
					{
						var track = legacy.AudioTracks[Music];
						if (track != null && track.IsPlaying)
						{
							isTrackReady = true;
						}
					}
					else
					{
						isTrackReady = true;
					}
				}
				catch
				{
					isTrackReady = true;
				}

				if (isTrackReady)
				{
					hasStartedPlaying = true;
					songStopwatch.Restart();
				}
			}
			else
			{
				// Resume if it was paused from an Alt-Tab
				if (!songStopwatch.IsRunning)
				{
					songStopwatch.Start();
				}

				// Loop only when the full 5:44 song ends
				if (songStopwatch.Elapsed.TotalSeconds > 344.607)
				{
					songStopwatch.Restart();
				}
			}

			previousMusic = currentMusic;
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			if (!hasStartedPlaying || !songStopwatch.IsRunning)
				return;

			float elapsedSeconds = (float)songStopwatch.Elapsed.TotalSeconds;
			MenuLyrics.DrawLyrics(spriteBatch, elapsedSeconds, 1f);
		}
	}
}
