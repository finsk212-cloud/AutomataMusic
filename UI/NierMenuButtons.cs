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

			// In vanilla DrawMenu, each button is drawn in a 5-pass loop (k = 0..4).
			// Passes 0..3 are border outline passes (R, G, B are low: 0 for idle, ~51 for hovered).
			// Pass 4 is the front text pass (R >= 75 for idle, R >= 200 for hovered).
			// Suppress all dark border passes so only the front pass renders cleanly without ghosting.
			if (color.R <= 60 && color.G <= 60 && color.B <= 60)
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

			float centerX = Main.screenWidth / 2f;
			float centerY = position.Y;

			// Check if the button is hovered:
			// In vanilla Terraria, hovered buttons use bright yellow/gold (high R & G, low B)
			bool isHovered = (color.R > 180 && color.G > 140 && color.B < 125);

			// Compact, sleek scale for refined military terminal typography (smaller & cleaner)
			float drawScale = 0.60f;
			Vector2 textSize = font.MeasureString(upperText) * drawScale;

			// Shift down by +6px to precisely align Andy Bold uppercase glyphs with the center line
			Vector2 textPos = new Vector2(
				(float)Math.Round(centerX - textSize.X / 2f),
				(float)Math.Round(centerY - textSize.Y / 2f + 6f)
			);

			if (isHovered)
			{
				// Slim, compact highlight banner (24px tall, 240px wide)
				float bannerW = 240f;
				float bannerH = 24f;
				float bannerX = (float)Math.Round(centerX - bannerW / 2f);
				float bannerY = (float)Math.Round(centerY - bannerH / 2f);
				Rectangle bannerRect = new Rectangle((int)bannerX, (int)bannerY, (int)bannerW, (int)bannerH);

				// 1. Solid YoRHa bone-ivory highlight banner (#EAE5D4)
				Color bannerBg = new Color(234, 229, 212);
				sb.Draw(pixel, bannerRect, bannerBg);

				// 2. Dark charcoal top & bottom borders (#202226)
				Color borderCol = new Color(32, 34, 38);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), borderCol * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), borderCol * 0.9f);

				// 3. Left and right 2px vertical end-caps
				sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 2, bannerRect.Height), borderCol * 0.9f);
				sb.Draw(pixel, new Rectangle(bannerRect.Right - 2, bannerRect.Y, 2, bannerRect.Height), borderCol * 0.9f);

				// 4. Signature NieR square cursor pip (■) to the left of the text
				int pipSize = 5;
				int pipX = (int)(textPos.X - 14f);
				int pipY = (int)Math.Round(centerY - pipSize / 2f + 1f);
				sb.Draw(pixel, new Rectangle(pipX, pipY, pipSize, pipSize), new Color(24, 26, 30));

				// 5. High-contrast deep charcoal text (#16181C)
				Color textDark = new Color(22, 24, 28);
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos, textDark, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			}
			else
			{
				// Unselected buttons: pure, clean, minimalist text with no background boxes or borders
				// 1. Subtle 1px drop shadow for crisp legibility against orbital background
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos + new Vector2(1f, 1f), Color.Black * 0.85f, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

				// 2. Muted YoRHa bone-silver / platinum text (#D4CEBF)
				Color idleText = new Color(212, 206, 191);
				DynamicSpriteFontExtensionMethods.DrawString(sb, font, upperText, textPos, idleText, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			}
		}
	}
}
