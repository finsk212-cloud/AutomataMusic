using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using AutomataMusic.Common;

namespace AutomataMusic.UI
{
	public static class NierMenuButtons
	{
		private static bool hooksRegistered = false;

		public static void Load()
		{
			if (hooksRegistered)
				return;

			Terraria.On_Utils.DrawBorderStringFourWay += Hook_DrawBorderStringFourWay;
			hooksRegistered = true;
		}

		public static void Unload()
		{
			if (!hooksRegistered)
				return;

			Terraria.On_Utils.DrawBorderStringFourWay -= Hook_DrawBorderStringFourWay;
			hooksRegistered = false;
		}

		private static bool IsThemeActive()
		{
			if (!Main.gameMenu)
				return false;

			if (AutomataMusicConfig.Instance != null && !AutomataMusicConfig.Instance.BunkerMenuTheme)
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

		private static void Hook_DrawBorderStringFourWay(
			Terraria.On_Utils.orig_DrawBorderStringFourWay orig,
			SpriteBatch sb,
			DynamicSpriteFont font,
			string text,
			float x,
			float y,
			Color color,
			Color borderColor,
			Vector2 origin,
			float scale)
		{
			// Only hook big menu buttons when the NieR theme is active on the main menu
			if (!IsThemeActive() || string.IsNullOrEmpty(text) || font != FontAssets.DeathText.Value)
			{
				orig(sb, font, text, x, y, color, borderColor, origin, scale);
				return;
			}

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
			{
				orig(sb, font, text, x, y, color, borderColor, origin, scale);
				return;
			}

			// Clean uppercase typography for NieR:Automata YoRHa UI
			string upperText = text.ToUpperInvariant();
			Vector2 textSize = font.MeasureString(upperText) * scale;

			// In vanilla DrawMenu, (x, y) is the top-left of the text
			Vector2 textPos = new Vector2(x - origin.X * scale, y - origin.Y * scale);

			// Detect if this button is hovered / selected (vanilla sets bright yellow/gold with high R & G)
			bool isHovered = (color.R > 220 && color.G > 160 && color.B < 120);

			// NieR button banner sizing
			float padX = 28f * scale;
			float padY = 6f * scale;
			float bannerW = Math.Max(textSize.X + padX * 2f, 280f * scale);
			float bannerH = textSize.Y + padY * 2f;
			float bannerX = textPos.X + (textSize.X - bannerW) / 2f;
			float bannerY = textPos.Y - padY;

			Rectangle bannerRect = new Rectangle((int)bannerX, (int)bannerY, (int)bannerW, (int)bannerH);

			if (isHovered)
			{
				// 1. Solid YoRHa bone-ivory highlight banner (#EEE8D4)
				Color bannerBg = new Color(238, 232, 212);
				sb.Draw(pixel, bannerRect, bannerBg);

				// 2. Dark charcoal border frame
				Color bannerBorder = new Color(30, 32, 36);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), bannerBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), bannerBorder * 0.9f);

				// 3. Signature NieR square cursor pip on the left of the button (■)
				int pipSize = Math.Max(7, (int)(8f * scale));
				int pipX = (int)(textPos.X - 18f * scale);
				int pipY = (int)(textPos.Y + (textSize.Y - pipSize) / 2f);
				sb.Draw(pixel, new Rectangle(pipX, pipY, pipSize, pipSize), new Color(22, 24, 28));

				// 4. Subtle YoRHa bracket notch indicator on the right edge
				int notchH = (int)(10f * scale);
				int notchX = bannerRect.Right - (int)(10f * scale);
				int notchY = (int)(bannerRect.Y + (bannerRect.Height - notchH) / 2f);
				sb.Draw(pixel, new Rectangle(notchX, notchY, 2, notchH), new Color(35, 38, 42));

				// 5. High-contrast deep charcoal text on the light ivory banner
				Color textDark = new Color(22, 24, 28);
				sb.DrawString(font, upperText, textPos, textDark, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
			else
			{
				// 1. Unselected state: subtle dark translucent backing bar (#0C0D11 * 0.65f)
				Color idleBg = new Color(12, 13, 17) * 0.65f;
				sb.Draw(pixel, bannerRect, idleBg);

				// 2. Faint guideline border
				Color idleBorder = new Color(180, 172, 150) * 0.28f;
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), idleBorder);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), idleBorder);

				// 3. Corner tick brackets
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 2, 4), idleBorder * 0.8f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 2, bannerRect.Y, 2, 4), idleBorder * 0.8f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 4, 2, 4), idleBorder * 0.8f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 2, bannerRect.Bottom - 4, 2, 4), idleBorder * 0.8f);

				// 4. Drop shadow
				sb.DrawString(font, upperText, textPos + new Vector2(2f, 2f), Color.Black * 0.85f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

				// 5. Muted YoRHa bone-silver text
				Color idleText = new Color(218, 210, 192) * 0.90f;
				sb.DrawString(font, upperText, textPos, idleText, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}
	}
}
