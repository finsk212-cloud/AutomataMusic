using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AutomataMusic.UI
{
	public static class NierMenuButtons
	{
		private static bool hooksRegistered = false;

		public static void Load()
		{
			if (hooksRegistered)
				return;

			Terraria.On_Utils.DrawBorderStringBig += Hook_DrawBorderStringBig;
			hooksRegistered = true;
		}

		public static void Unload()
		{
			if (!hooksRegistered)
				return;

			Terraria.On_Utils.DrawBorderStringBig -= Hook_DrawBorderStringBig;
			hooksRegistered = false;
		}

		private static bool IsThemeActive()
		{
			if (!Main.gameMenu)
				return false;

			try
			{
				if (MenuLoader.CurrentMenu == ModContent.GetInstance<AutomataModMenu>())
					return true;
			}
			catch
			{
			}

			return false;
		}

		private static Vector2 Hook_DrawBorderStringBig(
			Terraria.On_Utils.orig_DrawBorderStringBig orig,
			SpriteBatch sb,
			string text,
			Vector2 pos,
			Color color,
			float scale,
			float anchorx,
			float anchory,
			int maxchar)
		{
			if (!IsThemeActive() || string.IsNullOrEmpty(text))
			{
				return orig(sb, text, pos, color, scale, anchorx, anchory, maxchar);
			}

			// Uppercase text for authentic NieR YoRHa military UI
			string upperText = text.ToUpperInvariant();

			var font = FontAssets.DeathText.Value;
			if (font == null)
			{
				return orig(sb, text, pos, color, scale, anchorx, anchory, maxchar);
			}

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 textSize = font.MeasureString(upperText) * scale;

			// Calculate top-left of the text based on anchor
			Vector2 textPos = pos - new Vector2(textSize.X * anchorx, textSize.Y * anchory);

			// Detect if this button is hovered / selected:
			// In vanilla Terraria, hovered buttons use bright yellow/gold (high R & G, low B)
			bool isHovered = (color.R > 200 && color.G > 180 && color.B < 100);

			if (pixel != null)
			{
				// NieR button bar layout
				float bannerPadX = 28f;
				float bannerPadY = 6f;
				float bannerW = Math.Max(textSize.X + bannerPadX * 2f, 260f);
				float bannerH = textSize.Y + bannerPadY * 2f;
				float bannerX = textPos.X + (textSize.X - bannerW) / 2f;
				float bannerY = textPos.Y - bannerPadY;

				Rectangle bannerRect = new Rectangle((int)bannerX, (int)bannerY, (int)bannerW, (int)bannerH);

				if (isHovered)
				{
					// 1. Solid YoRHa bone-ivory highlight banner
					Color bannerBg = new Color(238, 232, 212);
					sb.Draw(pixel, bannerRect, bannerBg);

					// 2. Dark charcoal borders and corner accents
					Color bannerBorder = new Color(30, 32, 36);
					sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), bannerBorder * 0.8f);
					sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), bannerBorder * 0.8f);

					// 3. Signature NieR square pip on the left of the button
					int pipSize = 8;
					int pipX = (int)(textPos.X - 18f);
					int pipY = (int)(textPos.Y + (textSize.Y - pipSize) / 2f);
					sb.Draw(pixel, new Rectangle(pipX, pipY, pipSize, pipSize), new Color(25, 26, 30));

					// 4. Subtle YoRHa bracket notch on the right
					int notchH = 10;
					int notchX = bannerRect.Right - 6;
					int notchY = (int)(bannerRect.Y + (bannerRect.Height - notchH) / 2f);
					sb.Draw(pixel, new Rectangle(notchX, notchY, 2, notchH), new Color(50, 52, 58));

					// 5. Draw text in deep NieR charcoal on the light banner
					Color textDark = new Color(22, 24, 28);
					sb.DrawString(font, upperText, textPos, textDark, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				}
				else
				{
					// Unselected state: subtle translucent backing bar
					Color idleBg = new Color(12, 13, 17) * 0.65f;
					sb.Draw(pixel, bannerRect, idleBg);

					// Faint top and bottom guideline
					Color idleBorder = new Color(175, 168, 145) * 0.25f;
					sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), idleBorder);
					sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), idleBorder);

					// Clean drop shadow
					sb.DrawString(font, upperText, textPos + new Vector2(2, 2), Color.Black * 0.85f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

					// Muted YoRHa bone-silver text
					Color idleText = new Color(215, 208, 190) * 0.90f;
					sb.DrawString(font, upperText, textPos, idleText, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				}

				return textSize;
			}

			return orig(sb, text, pos, color, scale, anchorx, anchory, maxchar);
		}
	}
}
