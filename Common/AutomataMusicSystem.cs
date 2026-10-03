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
using Terraria.ID;
using Terraria.ModLoader;

namespace AutomataMusic.Common
{
	public class AutomataMusicSystem : ModSystem
	{
		public const float SongDuration = 344.607f;

		private static readonly Stopwatch songStopwatch = new Stopwatch();
		private static double songTimeOffset = 0;
		private static bool hasStartedPlaying = false;
		private static bool checkedDefaultMenu = false;

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

		public static void UpdatePlaybackState()
		{
			// Alt-Tab check: if game lost focus or sounds paused, freeze stopwatch
			bool isAudioPaused = !Main.hasFocus || SoundEngine.AreSoundsPaused;
			if (isAudioPaused)
			{
				if (songStopwatch.IsRunning)
				{
					songStopwatch.Stop();
				}
				return;
			}

			int currentMusic = Main.curMusic;
			int targetMusic = MusicHelper.GetTrackWithCandidates(ModContent.GetInstance<AutomataMusic>(), "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

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
				return;

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
		}

		public static bool ShouldDrawLyrics(out float currentTime)
		{
			currentTime = 0f;

			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return false;

			bool isAudioPaused = !Main.hasFocus || SoundEngine.AreSoundsPaused;
			if (isAudioPaused)
				return false;

			if (!hasStartedPlaying || !songStopwatch.IsRunning)
				return false;

			currentTime = CurrentSongTime;
			return currentTime > 0.001f;
		}

		private static float savedMusicVolume = 1f;

		public override void Load()
		{
			Main.OnPostDraw += OnPostDrawHandler;
			Terraria.On_Main.DrawVersionNumber += Hook_DrawVersionNumber;
			Terraria.On_Main.DrawSocialMediaButtons += Hook_DrawSocialMediaButtons;
			Terraria.On_Main.DrawtModLoaderSocialMediaButtons += Hook_DrawtModLoaderSocialMediaButtons;
			Terraria.On_Main.UpdateAudio += Hook_UpdateAudio;
			checkedDefaultMenu = false;

			// If music volume was zeroed out by Terraria's musicError bug, restore it
			if (Main.musicVolume <= 0.001f)
			{
				Main.musicVolume = 1f;
			}
			savedMusicVolume = Main.musicVolume;
		}

		public override void Unload()
		{
			Main.OnPostDraw -= OnPostDrawHandler;
			Terraria.On_Main.DrawVersionNumber -= Hook_DrawVersionNumber;
			Terraria.On_Main.DrawSocialMediaButtons -= Hook_DrawSocialMediaButtons;
			Terraria.On_Main.DrawtModLoaderSocialMediaButtons -= Hook_DrawtModLoaderSocialMediaButtons;
			Terraria.On_Main.UpdateAudio -= Hook_UpdateAudio;
			songStopwatch.Reset();
			hasStartedPlaying = false;
			songTimeOffset = 0;
			checkedDefaultMenu = false;

			// Critical: When unloading mod, reset curMusic if it points to a modded slot to prevent IndexOutOfRangeException in UpdateAudio
			if (Main.curMusic >= Main.maxMusic || Main.curMusic < 0)
			{
				Main.curMusic = 0;
			}
			if (Main.newMusic >= Main.maxMusic || Main.newMusic < 0)
			{
				Main.newMusic = 0;
			}
			Main.musicError = 0;
		}

		private static void Hook_UpdateAudio(Terraria.On_Main.orig_UpdateAudio orig, Main self)
		{
			// 1. Keep track of user's intended music volume
			if (Main.musicVolume > 0.001f)
			{
				savedMusicVolume = Main.musicVolume;
			}

			// 2. Prevent IndexOutOfRangeException in Main.UpdateAudio()
			if (Main.musicFade != null)
			{
				if (Main.curMusic < 0 || Main.curMusic >= Main.musicFade.Length)
				{
					Main.curMusic = 0;
				}
				if (Main.newMusic < 0 || Main.newMusic >= Main.musicFade.Length)
				{
					Main.newMusic = 0;
				}
			}

			// 3. Keep musicError suppressed so Terraria never triggers the musicVolume = 0 reset
			Main.musicError = 0;

			orig(self);

			// 4. If musicVolume was somehow zeroed out by audio errors, automatically restore it
			if (Main.musicVolume <= 0.001f && savedMusicVolume > 0.001f && Main.soundVolume > 0.001f)
			{
				Main.musicVolume = savedMusicVolume;
			}
		}

		private static void Hook_DrawVersionNumber(Terraria.On_Main.orig_DrawVersionNumber orig, Color menuColor, float upBump)
		{
			if (MenuLoader.CurrentMenu is AutomataModMenu && (AutomataMusicConfig.Instance?.BunkerMenuTheme ?? true))
			{
				return; // Suppress corner white version and news text
			}

			orig(menuColor, upBump);
		}

		private static void Hook_DrawSocialMediaButtons(Terraria.On_Main.orig_DrawSocialMediaButtons orig, Color menuColor, float upBump)
		{
			if (MenuLoader.CurrentMenu is AutomataModMenu && (AutomataMusicConfig.Instance?.BunkerMenuTheme ?? true))
			{
				return;
			}

			orig(menuColor, upBump);
		}

		private static void Hook_DrawtModLoaderSocialMediaButtons(Terraria.On_Main.orig_DrawtModLoaderSocialMediaButtons orig, Color menuColor, float upBump)
		{
			if (MenuLoader.CurrentMenu is AutomataModMenu && (AutomataMusicConfig.Instance?.BunkerMenuTheme ?? true))
			{
				return;
			}

			orig(menuColor, upBump);
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

			UpdatePlaybackState();

			// In menus, lyrics are rendered via AutomataModMenu.PostDrawLogo so they sit on the background layer behind UI panels.
			// Only render in OnPostDraw when in active gameplay (e.g. music box playing Weight of the World).
			if (Main.gameMenu)
				return;

			if (ShouldDrawLyrics(out float currentTime))
			{
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
}
