using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AutomataMusic.Common;
using AutomataMusic.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataModMenu : ModMenu
	{
		public const float SongDuration = 344.607f;

		private static readonly Stopwatch songStopwatch = new Stopwatch();
		private static double songTimeOffset = 0;
		private static bool hasStartedPlaying = false;
		private static int previousMusic = -1;

		public static float CurrentSongTime
		{
			get
			{
				if (!hasStartedPlaying)
					return 0f;

				double total = songTimeOffset + songStopwatch.Elapsed.TotalSeconds;
				if (total > SongDuration)
				{
					total %= SongDuration;
				}
				return (float)MathHelper.Clamp((float)total, 0f, SongDuration);
			}
		}

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		public static void SeekTo(float targetSeconds)
		{
			targetSeconds = MathHelper.Clamp(targetSeconds, 0f, SongDuration);

			try
			{
				int musicId = ModContent.GetInstance<AutomataModMenu>().Music;
				if (Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && musicId >= 0 && musicId < legacy.AudioTracks.Length)
				{
					var track = legacy.AudioTracks[musicId];
					if (track != null)
					{
						var trackType = track.GetType();
						var mp3StreamField = trackType.GetField("_mp3Stream", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
						var soundInstField = trackType.GetField("_soundEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
						var prepareBufferMethod = trackType.GetMethod("PrepareBuffer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

						if (mp3StreamField?.GetValue(track) is Stream mp3Stream)
						{
							float ratio = MathHelper.Clamp(targetSeconds / SongDuration, 0f, 1f);
							long targetPos = (long)(ratio * mp3Stream.Length);
							// Align to 4 bytes (16-bit stereo frame)
							targetPos -= (targetPos % 4);
							if (targetPos < 0) targetPos = 0;
							if (targetPos > mp3Stream.Length) targetPos = mp3Stream.Length;

							if (soundInstField?.GetValue(track) is DynamicSoundEffectInstance soundInst)
							{
								soundInst.Stop();
								mp3Stream.Position = targetPos;
								prepareBufferMethod?.Invoke(track, null);
								soundInst.Play();
							}
							else
							{
								mp3Stream.Position = targetPos;
							}
						}
					}
				}
			}
			catch
			{
			}

			songTimeOffset = targetSeconds;
			songStopwatch.Restart();
		}

		public override void Update(bool isOnTitleScreen)
		{
			if (!isOnTitleScreen)
			{
				hasStartedPlaying = false;
				songTimeOffset = 0;
				songStopwatch.Reset();
				previousMusic = -1;
				return;
			}

			int currentMusic = Main.curMusic;

			// If music changed away from our menu track, stop
			if (currentMusic != Music)
			{
				hasStartedPlaying = false;
				songTimeOffset = 0;
				songStopwatch.Reset();
				previousMusic = currentMusic;
				return;
			}

			// When Alt-Tabbed or game window lost focus, Terraria pauses audio: pause stopwatch too!
			bool isAudioPaused = !Main.hasFocus || SoundEngine.AreSoundsPaused;
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
					if (Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && Music >= 0 && Music < legacy.AudioTracks.Length)
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
					songTimeOffset = 0;
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

				// Loop when reaching end
				if (songTimeOffset + songStopwatch.Elapsed.TotalSeconds > SongDuration)
				{
					songTimeOffset = 0;
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

			float currentTime = CurrentSongTime;
			MenuLyrics.DrawLyrics(spriteBatch, currentTime, 1f);
		}
	}
}
