using System;
using System.Collections.Generic;
using AutomataMusic.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutomataMusic.UI
{
	public class NowPlayingSystem : ModSystem
	{
		private static int lastMusicSlot = -1;
		private static int displayTimer = 0;
		private const int TotalDuration = 450; // 7.5 seconds at 60 FPS
		private static string currentTitle = "";
		private static string currentSubtitle = "";

		public override void UpdateUI(GameTime gameTime)
		{
			if (Main.dedServ)
				return;

			int cur = Main.curMusic;
			if (cur != lastMusicSlot)
			{
				lastMusicSlot = cur;

				if (MusicHelper.TryGetTrackInfo(cur, out var info))
				{
					if (AutomataMusicConfig.Instance.ShowNowPlayingNotification)
					{
						currentTitle = info.Title;
						currentSubtitle = info.Subtitle;
						displayTimer = TotalDuration;
					}
				}
			}

			if (displayTimer > 0)
			{
				displayTimer--;
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"AutomataMusic: Now Playing",
					DrawNowPlayingToast,
					InterfaceScaleType.UI
				));
			}
		}

		private static bool DrawNowPlayingToast()
		{
			if (displayTimer <= 0 || !AutomataMusicConfig.Instance.ShowNowPlayingNotification)
				return true;

			// Smooth animation calculations
			float alpha;
			float slideOffsetX;

			if (displayTimer > TotalDuration - 40) // Fade in (~0.65s): gentle slide in from right
			{
				float t = (TotalDuration - displayTimer) / 40f;
				alpha = MathHelper.SmoothStep(0f, 1f, t);
				slideOffsetX = (1f - alpha) * 22f;
			}
			else if (displayTimer > 65) // Sustained (~5.8s): Full opacity
			{
				alpha = 1f;
				slideOffsetX = 0f;
			}
			else // Fade out (~1.1s): smooth dissolve + gentle drift right
			{
				float t = displayTimer / 65f;
				alpha = MathHelper.SmoothStep(0f, 1f, t);
				slideOffsetX = (1f - alpha) * 16f;
			}

			alpha = MathHelper.Clamp(alpha, 0f, 1f);
			if (alpha <= 0.001f)
				return true;

			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			var font = FontAssets.MouseText.Value;

			// Exact font scales for MouseText font
			float headerScale = 0.65f;
			float titleScale = 0.88f;
			float subScale = 0.72f;

			string headerText = "NOW PLAYING";

			// Measure each string with the exact font used by DrawBorderString
			Vector2 headerSize = font.MeasureString(headerText) * headerScale;
			Vector2 titleSize = font.MeasureString(currentTitle) * titleScale;
			Vector2 subSize = font.MeasureString(currentSubtitle) * subScale;

			float maxTextWidth = Math.Max(headerSize.X, Math.Max(titleSize.X, subSize.X));

			// Generous horizontal padding so text never touches edges or exceeds background
			float leftFadePadding = 55f;
			float rightPadding = 22f;
			float totalWidth = maxTextWidth + leftFadePadding + rightPadding;

			// Vertical layout heights with clean line spacing (no overlapping!)
			// Header (18px) -> Gap (4px) -> Title (24px) -> Gap (4px) -> Subtitle (18px) -> Underline
			float totalHeight = 78f;

			// Position in the bottom-right corner, comfortably above the bottom edge
			float marginX = 26f;
			float marginY = 36f;
			float posX = Main.screenWidth - totalWidth - marginX + slideOffsetX;
			float posY = Main.screenHeight - totalHeight - marginY;

			// 1. Soft horizontal gradient background (fades from 0% on left to dark on right)
			int slices = 32;
			float sliceW = totalWidth / slices;
			for (int i = 0; i < slices; i++)
			{
				float t = (i + 1) / (float)slices; // 0 to 1
				float curve = (float)Math.Pow(t, 1.8);
				float bgAlpha = curve * 0.78f * alpha;
				Rectangle r = new Rectangle((int)(posX + i * sliceW), (int)posY, (int)(sliceW + 1f), (int)totalHeight);
				sb.Draw(pixel, r, new Color(10, 10, 14) * bgAlpha);
			}

			// 2. NieR gold underline accent that fades softly to the left
			int underlineY = (int)(posY + totalHeight - 4);
			for (int i = 0; i < slices; i++)
			{
				float t = (i + 1) / (float)slices;
				float curve = t * t * t;
				float lineAlpha = curve * 0.85f * alpha;
				Rectangle r = new Rectangle((int)(posX + i * sliceW), underlineY, (int)(sliceW + 1f), 2);
				sb.Draw(pixel, r, new Color(218, 186, 110) * lineAlpha);
			}

			// 3. Crisp vertical accent cap at the right edge
			sb.Draw(pixel, new Rectangle((int)(posX + totalWidth - 4), (int)posY + 8, 2, (int)totalHeight - 14), new Color(218, 186, 110) * (0.9f * alpha));

			// 4. Right-anchored text positions with clean margins
			float rightAnchorX = posX + totalWidth - rightPadding;

			// Line 1: Header ("NOW PLAYING")
			Vector2 headerPos = new Vector2(rightAnchorX - headerSize.X, posY + 8f);
			Utils.DrawBorderString(sb, headerText, headerPos, new Color(205, 195, 170) * alpha, headerScale);

			// Line 2: Track Title (e.g. "Peaceful Sleep")
			Vector2 titlePos = new Vector2(rightAnchorX - titleSize.X, posY + 27f);
			Utils.DrawBorderString(sb, currentTitle, titlePos, new Color(255, 255, 252) * alpha, titleScale);

			// Line 3: Subtitle (e.g. "NieR:Automata OST (Resistance Camp)")
			Vector2 subPos = new Vector2(rightAnchorX - subSize.X, posY + 52f);
			Utils.DrawBorderString(sb, currentSubtitle, subPos, new Color(165, 160, 150) * alpha, subScale);

			return true;
		}
	}
}
