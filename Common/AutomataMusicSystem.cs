using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AutomataMusic.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace AutomataMusic.Common
{
	public class AutomataMusicSystem : ModSystem
	{
		public const float SongDuration = 344.607f;

		private static readonly Stopwatch songStopwatch = new Stopwatch();
		private static double songTimeOffset = 0;
		private static bool hasStartedPlaying = false;

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

		public static void SeekTo(float targetSeconds)
		{
			targetSeconds = MathHelper.Clamp(targetSeconds, 0f, SongDuration);

			try
			{
				int musicId = MusicHelper.GetTrackWithCandidates(ModContent.GetInstance<AutomataMusic>(), "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");
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

		private static bool checkedDefaultMenu = false;

		public override void Load()
		{
			Main.OnPostDraw += OnPostDrawHandler;
			checkedDefaultMenu = false;
		}

		public override void Unload()
		{
			Main.OnPostDraw -= OnPostDrawHandler;
			songStopwatch.Reset();
			hasStartedPlaying = false;
			songTimeOffset = 0;
			checkedDefaultMenu = false;
		}

		private void OnPostDrawHandler(GameTime gameTime)
		{
			if (!checkedDefaultMenu && Main.gameMenu)
			{
				checkedDefaultMenu = true;
				if (AutomataMusicConfig.Instance == null || AutomataMusicConfig.Instance.SetAsDefaultMenuTheme)
				{
					var automataMenu = ModContent.GetInstance<AutomataModMenu>();
					if (automataMenu != null && MenuLoader.CurrentMenu != automataMenu)
					{
						AutomataMusic.ActivateMenuTheme();
					}
				}
			}

			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			// Alt-Tab check: if game lost focus or sounds paused, freeze stopwatch and hide lyrics
			bool isAudioPaused = !Main.hasFocus || SoundEngine.AreSoundsPaused;
			if (isAudioPaused)
			{
				if (songStopwatch.IsRunning)
				{
					songStopwatch.Stop();
				}
				return; // Lyrics completely hidden while Alt-Tabbed
			}

			int currentMusic = Main.curMusic;
			int targetMusic = MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

			if (targetMusic < 0 || currentMusic != targetMusic)
			{
				if (hasStartedPlaying)
				{
					hasStartedPlaying = false;
					songTimeOffset = 0;
					songStopwatch.Reset();
				}
				return;
			}

			// Verify if the audio track is actively producing sound in legacy audio system
			bool isTrackActuallyPlaying = false;
			try
			{
				if (Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && targetMusic >= 0 && targetMusic < legacy.AudioTracks.Length)
				{
					var track = legacy.AudioTracks[targetMusic];
					if (track != null && track.IsPlaying)
					{
						isTrackActuallyPlaying = true;
					}
				}
				else
				{
					isTrackActuallyPlaying = true;
				}
			}
			catch
			{
				isTrackActuallyPlaying = true;
			}

			if (!isTrackActuallyPlaying)
			{
				return;
			}

			if (!hasStartedPlaying)
			{
				hasStartedPlaying = true;
				songTimeOffset = 0;
				songStopwatch.Restart();
			}
			else
			{
				if (!songStopwatch.IsRunning)
				{
					songStopwatch.Start();
				}

				if (songTimeOffset + songStopwatch.Elapsed.TotalSeconds > SongDuration)
				{
					songTimeOffset = 0;
					songStopwatch.Restart();
				}
			}

			float currentTime = CurrentSongTime;

			// Draw lyrics across all menus and gameplay while the track plays
			bool beganOurBatch = false;
			try
			{
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
				beganOurBatch = true;
			}
			catch (InvalidOperationException)
			{
				try
				{
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
					beganOurBatch = true;
				}
				catch
				{
				}
			}

			try
			{
				MenuLyrics.DrawLyrics(Main.spriteBatch, currentTime, 1f);
			}
			finally
			{
				if (beganOurBatch)
				{
					Main.spriteBatch.End();
				}
			}
		}
	}
}
