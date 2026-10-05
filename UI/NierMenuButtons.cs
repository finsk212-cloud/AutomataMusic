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

			// Clean uppercase formatting for authentic NieR YoRHa military UI
			string upperText = text.ToUpperInvariant();

			float screenW = Main.screenWidth;
			float centerX = screenW / 2f;

			// Check if the button is hovered:
			// In vanilla Terraria, hovered buttons use bright yellow/gold (high R & G, low B)
			bool isHovered = (color.R > 210 && color.G > 160 && color.B < 120);

			// Proportional, elegant text scale that fits comfortably in a 30px row
			float drawScale = isHovered ? 0.82f : 0.78f;
			Vector2 textSize = font.MeasureString(upperText) * drawScale;

			// Sleek, uniform dimensions so all buttons align into a perfect military block
			float bannerW = 340f;
			float bannerH = 28f;
			float bannerX = centerX - bannerW / 2f;

			// Center the 28px banner on the text line (Terraria spaces lines ~42px, giving ~14px clean margins)
			float centerY = position.Y + 14f;
			float bannerY = centerY - bannerH / 2f;

			Rectangle bannerRect = new Rectangle((int)bannerX, (int)bannerY, (int)bannerW, (int)bannerH);
			Vector2 textPos = new Vector2(
				(float)Math.Round(centerX - textSize.X / 2f),
				(float)Math.Round(centerY - textSize.Y / 2f)
			);

			if (isHovered)
			{
				// 1. Solid YoRHa bone-ivory highlight banner (#EEE8D4)
				Color bannerBg = new Color(238, 232, 212);
				sb.Draw(pixel, bannerRect, bannerBg);

				// 2. Dark charcoal top & bottom borders
				Color borderCol = new Color(28, 30, 34);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), borderCol * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), borderCol * 0.9f);

				// 3. Left and right 2px end-caps
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 2, bannerRect.Height), borderCol * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 2, bannerRect.Y, 2, bannerRect.Height), borderCol * 0.9f);

				// 4. Subtle corner bracket ticks (matching title card style)
				int arm = 8;
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, arm, 2), borderCol);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - arm, bannerRect.Y, arm, 2), borderCol);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 2, arm, 2), borderCol);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - arm, bannerRect.Bottom - 2, arm, 2), borderCol);

				// 5. Signature NieR square cursor pip (■) on the left side of text
				int pipSize = 6;
				int pipX = (int)(textPos.X - 16f);
				int pipY = (int)(centerY - pipSize / 2f);
				sb.Draw(pixel, new Rectangle(pipX, pipY, pipSize, pipSize), borderCol);

				// 6. High-contrast deep charcoal text
				Color textDark = new Color(22, 24, 28);
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos, textDark, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			}
			else
			{
				// 1. Unselected state: subtle, clean dark translucent backing (#0C0D11 at 40% opacity)
				Color idleBg = new Color(12, 13, 17) * 0.40f;
				sb.Draw(pixel, bannerRect, idleBg);

				// 2. Faint guideline border
				Color idleBorder = new Color(185, 178, 155) * 0.22f;
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), idleBorder);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), idleBorder);

				// 3. Crisp corner ticks (matching Title Card YoRHa brackets)
				int tickArm = 6;
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, tickArm, 1), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 1, 3), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - tickArm, bannerRect.Y, tickArm, 1), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 1, bannerRect.Y, 1, 3), idleBorder * 0.9f);

				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, tickArm, 1), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 3, 1, 3), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - tickArm, bannerRect.Bottom - 1, tickArm, 1), idleBorder * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 1, bannerRect.Bottom - 3, 1, 3), idleBorder * 0.9f);

				// 4. Clean drop shadow
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos + new Vector2(1.5f, 1.5f), Color.Black * 0.85f, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

				// 5. Muted YoRHa bone-silver text
				Color idleText = new Color(218, 210, 192) * 0.90f;
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos, idleText, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			}
		}
	}
}
