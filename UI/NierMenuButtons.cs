using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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

			Terraria.IL_Main.DrawMenu += Hook_IL_DrawMenu;
			hooksRegistered = true;
		}

		public static void Unload()
		{
			if (!hooksRegistered)
				return;

			Terraria.IL_Main.DrawMenu -= Hook_IL_DrawMenu;
			hooksRegistered = false;
		}

		private static void Hook_IL_DrawMenu(ILContext il)
		{
			try
			{
				var targetMethod = typeof(DynamicSpriteFontExtensionMethods).GetMethod(
					"DrawString",
					new Type[] {
						typeof(SpriteBatch),
						typeof(DynamicSpriteFont),
						typeof(string),
						typeof(Vector2),
						typeof(Color),
						typeof(float),
						typeof(Vector2),
						typeof(float),
						typeof(SpriteEffects),
						typeof(float)
					});

				var replacementMethod = typeof(NierMenuButtons).GetMethod(
					nameof(CustomDrawString),
					BindingFlags.Public | BindingFlags.Static);

				if (targetMethod == null || replacementMethod == null)
					return;

				var cursor = new ILCursor(il);
				int redirected = 0;

				while (cursor.TryGotoNext(MoveType.Before, i => i.MatchCall(targetMethod)))
				{
					cursor.Next.OpCode = OpCodes.Call;
					cursor.Next.Operand = il.Import(replacementMethod);
					redirected++;
					cursor.Index++;
				}

				AutomataMusic.Instance?.Logger.Info($"[NierMenuButtons] Successfully redirected {redirected} DrawString calls in DrawMenu!");
			}
			catch (Exception ex)
			{
				AutomataMusic.Instance?.Logger.Warn("[NierMenuButtons] IL hook error: " + ex);
			}
		}

		public static bool IsThemeActive()
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

		public static void CustomDrawString(
			SpriteBatch sb,
			DynamicSpriteFont font,
			string text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth)
		{
			// If NieR theme is not active, or this is not the main menu button font, render normally
			if (!IsThemeActive() || string.IsNullOrEmpty(text) || font != FontAssets.DeathText.Value)
			{
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, position, color, rotation, origin, scale, effects, layerDepth);
				return;
			}

			// In vanilla DrawMenu, each button is drawn in a loop 5 times:
			// 4 times in Color.Black to create a thick cartoon border, then 1 time in the actual color.
			// In NieR, we suppress the thick cartoon black border passes:
			if (color.R == 0 && color.G == 0 && color.B == 0)
			{
				return;
			}

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
			{
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, position, color, rotation, origin, scale, effects, layerDepth);
				return;
			}

			// Uppercase formatting for authentic NieR YoRHa military UI
			string upperText = text.ToUpperInvariant();
			Vector2 textSize = font.MeasureString(upperText) * scale;

			// Check if the button is hovered:
			// In vanilla Terraria, hovered buttons use bright yellow/gold (high R & G, low B)
			bool isHovered = (color.R > 210 && color.G > 160 && color.B < 120);

			// Calculate button banner dimensions
			float padX = 28f * scale;
			float padY = 6f * scale;
			float bannerW = Math.Max(textSize.X + padX * 2f, 280f * scale);
			float bannerH = textSize.Y + padY * 2f;
			float bannerX = position.X - origin.X * scale + (textSize.X - bannerW) / 2f;
			float bannerY = position.Y - origin.Y * scale - padY;

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
				int pipX = (int)(bannerRect.X + 12f * scale);
				int pipY = (int)(bannerRect.Y + (bannerRect.Height - pipSize) / 2f);
				sb.Draw(pixel, new Rectangle(pipX, pipY, pipSize, pipSize), new Color(22, 24, 28));

				// 4. Subtle YoRHa bracket notch indicator on the right edge
				int notchH = (int)(10f * scale);
				int notchX = bannerRect.Right - (int)(10f * scale);
				int notchY = (int)(bannerRect.Y + (bannerRect.Height - notchH) / 2f);
				sb.Draw(pixel, new Rectangle(notchX, notchY, 2, notchH), new Color(35, 38, 42));

				// 5. High-contrast deep charcoal text on the light ivory banner
				Color textDark = new Color(22, 24, 28);
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, position, textDark, rotation, origin, scale, effects, layerDepth);
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
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, position + new Vector2(1.5f, 1.5f), Color.Black * 0.85f, rotation, origin, scale, effects, layerDepth);

				// 5. Muted YoRHa bone-silver text
				Color idleText = new Color(218, 210, 192) * 0.90f;
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, position, idleText, rotation, origin, scale, effects, layerDepth);
			}
		}
	}
}
