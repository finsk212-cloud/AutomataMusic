using System;
using System.IO;
using System.Reflection;
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
		private static int songFrame = 0;
		private static bool wasPlaying = false;

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		private static readonly FieldInfo mp3StreamField = typeof(MP3AudioTrack).GetField("_mp3Stream", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo baseStreamField = typeof(MP3AudioTrack).GetField("_stream", BindingFlags.NonPublic | BindingFlags.Instance);
		private static PropertyInfo currentTimeProp = null;
		private static PropertyInfo positionProp = null;
		private static PropertyInfo lengthProp = null;
		private static bool reflectionInitialized = false;

		/// <summary>
		/// Returns the real-time audio playback timestamp directly from the audio stream if available.
		/// </summary>
		public static float? GetAudioTimeSeconds(int musicId)
		{
			try
			{
				if (Terraria.Main.audioSystem is LegacyAudioSystem legacy && legacy.AudioTracks != null && musicId >= 0 && musicId < legacy.AudioTracks.Length)
				{
					var track = legacy.AudioTracks[musicId];
					if (track == null || !track.IsPlaying)
						return null;

					if (track is MP3AudioTrack mp3 && mp3StreamField != null)
					{
						object streamObj = mp3StreamField.GetValue(mp3);
						if (streamObj != null)
						{
							if (!reflectionInitialized)
							{
								reflectionInitialized = true;
								var stType = streamObj.GetType();
								currentTimeProp = stType.GetProperty("CurrentTime");
								positionProp = stType.GetProperty("Position");
								lengthProp = stType.GetProperty("Length");
							}

							// 1. Direct CurrentTime (TimeSpan)
							if (currentTimeProp != null)
							{
								object val = currentTimeProp.GetValue(streamObj);
								if (val is TimeSpan ts)
									return (float)ts.TotalSeconds;
							}

							// 2. Position / Length ratio
							if (positionProp != null && lengthProp != null)
							{
								object pVal = positionProp.GetValue(streamObj);
								object lVal = lengthProp.GetValue(streamObj);
								if (pVal is long pos && lVal is long len && len > 0)
								{
									return ((float)pos / len) * 344.607f;
								}
							}
						}

						// 3. Fallback: Base file stream Position / Length
						if (baseStreamField != null)
						{
							if (baseStreamField.GetValue(mp3) is Stream s && s.Length > 0)
							{
								return ((float)s.Position / s.Length) * 344.607f;
							}
						}
					}
				}
			}
			catch { }

			return null;
		}

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
				songFrame = 0;
				wasPlaying = false;
				return;
			}

			bool isPlaying = IsTrackActivelyPlaying(Music);

			// If the track just started playing, reset frame counter to 0 so we stay in sync
			if (isPlaying && !wasPlaying)
			{
				songFrame = 0;
			}
			wasPlaying = isPlaying;

			// Only advance frames when the audio is actually playing (does not drift while loading)
			if (isPlaying)
			{
				songFrame++;
				// Loop after 344.6s (20676 frames @ 60 FPS)
				if (songFrame > 20676)
					songFrame = 0;
			}
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			// Don't show lyrics if menu music is not playing
			if (!IsTrackActivelyPlaying(Music))
				return;

			// Try to get exact hardware audio position first; fallback to frame counter
			float elapsedSeconds = GetAudioTimeSeconds(Music) ?? (songFrame / 60f);

			MenuLyrics.DrawLyrics(spriteBatch, elapsedSeconds, 1f);
		}
	}
}
