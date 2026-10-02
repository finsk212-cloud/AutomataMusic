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

		private static KeyboardState prevKeyState;
		private static bool isDraggingScrubber = false;
		private static bool wasMouseLeftDown = false;

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
				isDraggingScrubber = false;
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
				isDraggingScrubber = false;
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

				// Keyboard timeline seeking controls
				var keyState = Main.keyState;
				if (!Main.drawingPlayerChat)
				{
					if (keyState.IsKeyDown(Keys.Left) && !prevKeyState.IsKeyDown(Keys.Left))
					{
						SeekTo(CurrentSongTime - 5f);
					}
					else if (keyState.IsKeyDown(Keys.Right) && !prevKeyState.IsKeyDown(Keys.Right))
					{
						SeekTo(CurrentSongTime + 5f);
					}
					else if (keyState.IsKeyDown(Keys.Down) && !prevKeyState.IsKeyDown(Keys.Down))
					{
						SeekTo(CurrentSongTime - 15f);
					}
					else if (keyState.IsKeyDown(Keys.Up) && !prevKeyState.IsKeyDown(Keys.Up))
					{
						SeekTo(CurrentSongTime + 15f);
					}
					else if (keyState.IsKeyDown(Keys.Home) && !prevKeyState.IsKeyDown(Keys.Home))
					{
						SeekTo(0f);
					}
				}
				prevKeyState = keyState;
			}

			previousMusic = currentMusic;
		}

		private static string FormatTime(float seconds)
		{
			int totalSec = (int)Math.Max(0, seconds);
			int min = totalSec / 60;
			int sec = totalSec % 60;
			return $"{min:D2}:{sec:D2}";
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			if (!hasStartedPlaying || !songStopwatch.IsRunning)
				return;

			float currentTime = CurrentSongTime;

			// Draw Interactive Timeline Bar above the lyrics
			DrawTimelineBar(spriteBatch, currentTime);

			// Draw Lyrics
			MenuLyrics.DrawLyrics(spriteBatch, currentTime, 1f);
		}

		private void DrawTimelineBar(SpriteBatch sb, float currentTime)
		{
			var font = FontAssets.MouseText.Value;
			if (font == null)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float centerX = Main.screenWidth / 2f;
			float barY = Main.screenHeight - 114f;

			float trackWidth = 340f;
			float trackHeight = 6f;
			float trackX = centerX - trackWidth / 2f;
			float trackY = barY + 4f;

			// Hitbox for the scrubbing area
			Rectangle trackHitbox = new Rectangle((int)trackX - 6, (int)barY - 8, (int)trackWidth + 12, 26);
			bool isHoveringTrack = trackHitbox.Contains(Main.mouseX, Main.mouseY);

			// Handle mouse click/drag to seek
			bool isMouseDown = Main.mouseLeft;
			if (isMouseDown)
			{
				if (isHoveringTrack || isDraggingScrubber)
				{
					isDraggingScrubber = true;
					float mouseRatio = MathHelper.Clamp((Main.mouseX - trackX) / trackWidth, 0f, 1f);
					SeekTo(mouseRatio * SongDuration);
					currentTime = CurrentSongTime;
				}
			}
			else
			{
				isDraggingScrubber = false;
			}

			// Quick Jump Button -5s
			Rectangle prevBtn = new Rectangle((int)(trackX - 44), (int)barY - 2, 34, 18);
			bool isHoverPrev = prevBtn.Contains(Main.mouseX, Main.mouseY);
			if (isHoverPrev && isMouseDown && !wasMouseLeftDown)
			{
				SeekTo(currentTime - 5f);
				currentTime = CurrentSongTime;
			}

			// Quick Jump Button +5s
			Rectangle nextBtn = new Rectangle((int)(trackX + trackWidth + 10), (int)barY - 2, 34, 18);
			bool isHoverNext = nextBtn.Contains(Main.mouseX, Main.mouseY);
			if (isHoverNext && isMouseDown && !wasMouseLeftDown)
			{
				SeekTo(currentTime + 5f);
				currentTime = CurrentSongTime;
			}

			wasMouseLeftDown = isMouseDown;

			// Background panel for the timeline
			int panelW = 580;
			int panelH = 30;
			Rectangle panelRect = new Rectangle((int)(centerX - panelW / 2f), (int)barY - 9, panelW, panelH);
			sb.Draw(pixel, panelRect, new Color(12, 12, 16, 195));

			// Border around panel (NieR beige)
			Color borderColor = new Color(180, 170, 145, 120);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 1, panelRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 1, panelRect.Height), borderColor);
			sb.Draw(pixel, new Rectangle(panelRect.Right - 1, panelRect.Y, 1, panelRect.Height), borderColor);

			// Buttons visual
			Color btnBg = new Color(30, 30, 35, 200);
			Color btnHoverBg = new Color(75, 70, 60, 240);
			sb.Draw(pixel, prevBtn, isHoverPrev ? btnHoverBg : btnBg);
			sb.Draw(pixel, nextBtn, isHoverNext ? btnHoverBg : btnBg);

			Utils.DrawBorderString(sb, "-5s", new Vector2(prevBtn.X + 4, prevBtn.Y + 1), isHoverPrev ? Color.White : new Color(210, 205, 190), 0.65f);
			Utils.DrawBorderString(sb, "+5s", new Vector2(nextBtn.X + 4, nextBtn.Y + 1), isHoverNext ? Color.White : new Color(210, 205, 190), 0.65f);

			// Track bar background
			Rectangle trackBg = new Rectangle((int)trackX, (int)trackY, (int)trackWidth, (int)trackHeight);
			sb.Draw(pixel, trackBg, new Color(35, 35, 42, 230));

			// Track bar progress fill
			float progress = MathHelper.Clamp(currentTime / SongDuration, 0f, 1f);
			int fillW = (int)(trackWidth * progress);
			if (fillW > 0)
			{
				Rectangle trackFill = new Rectangle((int)trackX, (int)trackY, fillW, (int)trackHeight);
				sb.Draw(pixel, trackFill, new Color(225, 205, 155, 245));
			}

			// Scrubber handle (square/diamond)
			float handleX = trackX + fillW;
			Rectangle handleRect = new Rectangle((int)handleX - 4, (int)trackY - 3, 8, (int)trackHeight + 6);
			Color handleColor = (isHoveringTrack || isDraggingScrubber) ? new Color(255, 245, 220) : new Color(220, 205, 160);
			sb.Draw(pixel, handleRect, handleColor);

			// Time text
			string currentStr = FormatTime(currentTime);
			string totalStr = FormatTime(SongDuration);

			Vector2 currentTextPos = new Vector2(prevBtn.X - 44, barY - 1);
			Utils.DrawBorderString(sb, currentStr, currentTextPos, new Color(230, 220, 195), 0.72f);

			Vector2 totalTextPos = new Vector2(nextBtn.Right + 8, barY - 1);
			Utils.DrawBorderString(sb, totalStr, totalTextPos, new Color(170, 170, 175), 0.72f);

			// Tooltip when hovering the scrub bar showing target time
			if (isHoveringTrack)
			{
				float hoverRatio = MathHelper.Clamp((Main.mouseX - trackX) / trackWidth, 0f, 1f);
				string hoverTimeStr = FormatTime(hoverRatio * SongDuration);
				Vector2 tipPos = new Vector2(Main.mouseX - 15, barY - 26);
				Utils.DrawBorderString(sb, hoverTimeStr, tipPos, new Color(255, 240, 200), 0.75f);
			}
		}
	}
}
