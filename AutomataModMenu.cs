using AutomataMusic.Common;
using AutomataMusic.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataModMenu : ModMenu
	{
		private static int songFrame = 0;

		public override string DisplayName => "Automata: Music (Weight of the World)";

		public override int Music => MusicHelper.GetTrackWithCandidates(Mod, "Assets/Music/WeightOfTheWorld", "Assets/Music/Menu", "Assets/Music/Title");

		public override void Update(bool isOnTitleScreen)
		{
			if (isOnTitleScreen)
			{
				songFrame++;
				// Reset loop after 340 seconds (5 min 40s)
				if (songFrame > 20400)
					songFrame = 0;
			}
			else
			{
				songFrame = 0;
			}
		}

		public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
		{
			if (!AutomataMusicConfig.Instance.ShowMenuLyrics)
				return;

			float elapsedSeconds = songFrame / 60f;
			MenuLyrics.DrawLyrics(spriteBatch, elapsedSeconds, 1f);
		}
	}
}
