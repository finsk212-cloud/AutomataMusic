using AutomataMusic.Common;
using AutomataMusic.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			if (AutomataMusicConfig.Instance == null || AutomataMusicConfig.Instance.BunkerMenuTheme)
			{
				BunkerMenuTheme.Draw(spriteBatch, logoDrawCenter);
				return false; // Suppress vanilla Terraria logo; Bunker title card rendered instead
			}

			return true;
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			AutomataMusicSystem.UpdatePlaybackState();

			if (AutomataMusicSystem.ShouldDrawLyrics(out float currentTime))
			{
				// In PostDrawLogo, spriteBatch is already active with Main.UIScaleMatrix.
				// Drawing here renders lyrics on the background layer, behind all UI panels and buttons.
				MenuLyrics.DrawLyrics(spriteBatch, currentTime, 1f);
			}
		}

		public override void Unload()
		{
			BunkerMenuTheme.Unload();
		}
	}
}
