using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace AutomataMusic.UI
{
	public static class BunkerMenuTheme
	{
		// FX
		private static float titleTimer = 0f;

		public static void Unload()
		{
			OrbitalBackdrop.Unload();
			titleTimer = 0f;
		}

		public static void Draw(SpriteBatch sb, Vector2 logoDrawCenter)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
				return;

			int screenW = Main.screenWidth;
			int screenH = Main.screenHeight;
			float time = (float)Main.timeForVisualEffects * 0.02f;

			// Deterministic title transition timer (5.6s total loop)
			titleTimer += 0.01667f;
			if (titleTimer >= 5.6f)
				titleTimer -= 5.6f;

			// 1. Fully procedural orbital scene (rotating Earth, sunrise, nebula, stars, moon)
			OrbitalBackdrop.Draw(sb, pixel, screenW, screenH);

			// 2. Tactical YoRHa Military Corner Brackets
			DrawTacticalFrame(sb, pixel, screenW, screenH, time);

			// 3. NieR:Automata Stylized Title Card / Logo
			DrawNierTitleCard(sb, pixel, screenW, logoDrawCenter, time);
		}

		private static void DrawScanlines(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			Color scanlineColor = new Color(0, 0, 0, 14);
			for (int y = 0; y < screenH; y += 4)
			{
				sb.Draw(pixel, new Rectangle(0, y, screenW, 1), scanlineColor);
			}
		}

		private static void DrawTacticalFrame(SpriteBatch sb, Texture2D pixel, int screenW, int screenH, float time)
		{
			var font = FontAssets.MouseText.Value;
			int inset = 22;
			int bracketArm = 48;
			int thick = 2;

			Color frameBeige = new Color(230, 220, 190) * 0.90f;

			// Corner brackets ┌ ┐ └ ┘
			// Top-Left ┌
			sb.Draw(pixel, new Rectangle(inset, inset, bracketArm, thick), frameBeige);
			sb.Draw(pixel, new Rectangle(inset, inset, thick, bracketArm), frameBeige);
			sb.Draw(pixel, new Rectangle(inset + bracketArm, inset - 1, 4, 4), frameBeige);

			// Top-Right ┐
			sb.Draw(pixel, new Rectangle(screenW - inset - bracketArm, inset, bracketArm, thick), frameBeige);
			sb.Draw(pixel, new Rectangle(screenW - inset - thick, inset, thick, bracketArm), frameBeige);
			sb.Draw(pixel, new Rectangle(screenW - inset - bracketArm - 4, inset - 1, 4, 4), frameBeige);

			// Bottom-Left └
			sb.Draw(pixel, new Rectangle(inset, screenH - inset - thick, bracketArm, thick), frameBeige);
			sb.Draw(pixel, new Rectangle(inset, screenH - inset - bracketArm, thick, bracketArm), frameBeige);
			sb.Draw(pixel, new Rectangle(inset + bracketArm, screenH - inset - 3, 4, 4), frameBeige);

			// Bottom-Right ┘
			sb.Draw(pixel, new Rectangle(screenW - inset - bracketArm, screenH - inset - thick, bracketArm, thick), frameBeige);
			sb.Draw(pixel, new Rectangle(screenW - inset - thick, screenH - inset - bracketArm, thick, bracketArm), frameBeige);
			sb.Draw(pixel, new Rectangle(screenW - inset - bracketArm - 4, screenH - inset - 3, 4, 4), frameBeige);

			if (font == null)
				return;

			// Top-Left Corner: Snug within the ┌ bracket
			float textScale = 0.68f;
			float subScale = 0.58f;
			int padX = 14;
			int padY = 8;

			string tlHeader = "[ YoRHa ORBITAL PATROL // BUNKER PERIMETER ]";
			string tlSub = "TARGET: THE BUNKER [IN VIEW] // COMM: STABLE // OPERATOR: 6O";

			Utils.DrawBorderString(sb, tlHeader, new Vector2(inset + padX, inset + padY), new Color(245, 236, 212), textScale);
			Utils.DrawBorderString(sb, tlSub, new Vector2(inset + padX, inset + padY + 18), new Color(180, 175, 160) * 0.90f, subScale);

			// Top-Right Corner: Snug within the ┐ bracket (Right-Aligned)
			DateTime now = DateTime.Now;
			string trHeader = $"SYS TIME: {now:HH:mm:ss} // CYCLE: 11945.03.10";
			string trSub = "FFCS: ONLINE // LINK QUALITY: 99.8% // STATUS: NOMINAL";
			Vector2 trHeaderSize = font.MeasureString(trHeader) * textScale;
			Vector2 trSubSize = font.MeasureString(trSub) * subScale;

			Utils.DrawBorderString(sb, trHeader, new Vector2(screenW - inset - padX - trHeaderSize.X, inset + padY), new Color(240, 232, 210), textScale);
			Utils.DrawBorderString(sb, trSub, new Vector2(screenW - inset - padX - trSubSize.X, inset + padY + 18), new Color(180, 175, 160) * 0.90f, subScale);

			// Bottom-Left Corner: Snug within the └ bracket
			string blHeader = "POD 042: STANDBY [NORMAL] // FFCS: ENGAGED";
			string blSub = "PROGRAMS: LASER [ONLINE] // GATLING [ARMED] // SHIELD [READY]";

			Utils.DrawBorderString(sb, blHeader, new Vector2(inset + padX, screenH - inset - padY - 30), new Color(242, 234, 212), textScale);
			Utils.DrawBorderString(sb, blSub, new Vector2(inset + padX, screenH - inset - padY - 12), new Color(175, 170, 155) * 0.90f, subScale);

			// Bottom-Right Corner: Audio visualizer
			DrawAudioVisualizer(sb, pixel, screenW - inset - padX, screenH - inset - padY, time);
		}

		private static void DrawAudioVisualizer(SpriteBatch sb, Texture2D pixel, int rightX, int bottomY, float time)
		{
			var font = FontAssets.MouseText.Value;
			int barCount = 14;
			int barW = 3;
			int barGap = 2;
			int totalW = barCount * (barW + barGap);
			int startX = rightX - totalW;

			string label = "AUDIO FREQUENCY // CH-01 [ACTIVE]";

			if (font != null)
			{
				Vector2 lSize = font.MeasureString(label) * 0.58f;
				Utils.DrawBorderString(sb, label, new Vector2(rightX - lSize.X, bottomY - 26), new Color(188, 178, 155) * 0.90f, 0.58f);
			}

			Color barColor = new Color(240, 226, 192) * 0.92f;
			for (int b = 0; b < barCount; b++)
			{
				float wave = (float)Math.Sin(time * 1.5f + b * 0.45f) * 0.5f + 0.5f;
				float wave2 = (float)Math.Cos(time * 0.9f - b * 0.30f) * 0.35f + 0.35f;
				float hRatio = MathHelper.Clamp(wave * 0.65f + wave2 * 0.35f, 0.15f, 1f);
				int barH = (int)(hRatio * 15f);

				int bx = startX + b * (barW + barGap);
				int by = bottomY - 2 - barH;
				sb.Draw(pixel, new Rectangle(bx, by, barW, barH), barColor);
			}
		}

		private static readonly string[] TerrariaLetters = { "T", "E", "R", "R", "A", "R", "I", "A" };
		private static readonly string[] AutomataLetters = { "A", "U", "T", "O", "M", "A", "T", "A" };

		private static string GetTitleText(float timer, float time, out string katakanaText, out float glitchJitter, out bool isGhosting)
		{
			glitchJitter = 0f;
			isGhosting = false;

			// Phase 0: Stable "T E R R A R I A" (0.0s -> 3.8s)
			if (timer < 3.8f)
			{
				katakanaText = " / テラリア";
				return "T E R R A R I A";
			}

			// Phase 1: Letters start to switch to Automata (3.8s -> 4.2s, 0.4s transition)
			if (timer < 4.2f)
			{
				float prog = (timer - 3.8f) / 0.4f; // 0.0 -> 1.0
				int lettersToSwitch = (int)(prog * 8); // 0 -> 8 letters
				glitchJitter = (float)Math.Sin(time * 65f) * 2.2f;
				isGhosting = true;

				int seed = (int)(time * 26f);
				Random rand = new Random(seed);
				const string glyphs = "01X_#?*[]/\\";

				string[] current = new string[8];
				for (int i = 0; i < 8; i++)
				{
					if (i < lettersToSwitch)
					{
						current[i] = AutomataLetters[i];
					}
					else if (i == lettersToSwitch && rand.NextDouble() < 0.65)
					{
						current[i] = glyphs[rand.Next(glyphs.Length)].ToString();
					}
					else
					{
						current[i] = TerrariaLetters[i];
					}
				}

				katakanaText = lettersToSwitch >= 4 ? " / オートマタ" : " / テラリア";
				return string.Join(" ", current);
			}

			// Phase 2: Hold "A U T O M A T A" for 1.0 full second (4.2s -> 5.2s)
			if (timer < 5.2f)
			{
				katakanaText = " / オートマタ";
				return "A U T O M A T A";
			}

			// Phase 3: Glitches back to "T E R R A R I A" (5.2s -> 5.55s, 0.35s transition)
			if (timer < 5.55f)
			{
				float prog = (timer - 5.2f) / 0.35f; // 0.0 -> 1.0
				int lettersBack = (int)(prog * 8); // 0 -> 8 letters back to Terraria
				glitchJitter = (float)Math.Sin(time * 65f) * 2.2f;
				isGhosting = true;

				int seed = (int)(time * 26f) + 37;
				Random rand = new Random(seed);
				const string glyphs = "01X_#?*[]/\\";

				string[] current = new string[8];
				for (int i = 0; i < 8; i++)
				{
					if (i < lettersBack)
					{
						current[i] = TerrariaLetters[i];
					}
					else if (i == lettersBack && rand.NextDouble() < 0.65)
					{
						current[i] = glyphs[rand.Next(glyphs.Length)].ToString();
					}
					else
					{
						current[i] = AutomataLetters[i];
					}
				}

				katakanaText = lettersBack >= 4 ? " / テラリア" : " / オートマタ";
				return string.Join(" ", current);
			}

			// Phase 4: Settle back to "T E R R A R I A" (5.55s -> 5.6s)
			katakanaText = " / テラリア";
			return "T E R R A R I A";
		}

		private static void DrawNierTitleCard(SpriteBatch sb, Texture2D pixel, int screenW, Vector2 logoDrawCenter, float time)
		{
			var fontDeath = FontAssets.DeathText.Value;
			var fontMouse = FontAssets.MouseText.Value;
			if (fontDeath == null || fontMouse == null)
				return;

			float centerX = screenW / 2f;
			float titleY = Math.Max(70f, logoDrawCenter.Y - 60f);

			string subTitle = MenuLyrics.CheckCjkSupport(fontMouse)
				? "— YoRHa OS v4.02 // 人類に栄光あれ —"
				: "— YoRHa OS v4.02 // For the Glory of Mankind —";

			float titleScale = 0.90f;
			float katakanaScale = 0.78f;
			float subScale = 0.68f;

			// Dynamic letter-by-letter switch to Automata, 1.0s hold, and glitch back (monochrome only)
			string displayTitle = GetTitleText(titleTimer, time, out string katakanaRaw, out float glitchJitter, out bool isGhosting);
			string displayKatakana = MenuLyrics.CheckCjkSupport(fontMouse)
				? katakanaRaw
				: (katakanaRaw.Contains("オートマタ") ? " / Automata" : " / Terraria");

			Vector2 mainSize = fontDeath.MeasureString("T E R R A R I A") * titleScale;
			Vector2 kataSize = fontMouse.MeasureString(" / テラリア") * katakanaScale;
			Vector2 subSize = fontMouse.MeasureString(subTitle) * subScale;

			float totalTitleW = mainSize.X + kataSize.X + 8f;
			float cardW = Math.Max(totalTitleW, subSize.X) + 84f;
			float cardH = 88f;

			Rectangle cardRect = new Rectangle((int)(centerX - cardW / 2f), (int)(titleY - 14f), (int)cardW, (int)cardH);

			// 1. Clean dark translucent backing banner (no lines or moving slice noise)
			sb.Draw(pixel, cardRect, new Color(12, 13, 17) * 0.92f);

			// 2. Faint border outline
			Color cardBorder = new Color(185, 175, 145) * 0.40f;
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - 1, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, 1, cardRect.Height), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.Right - 1, cardRect.Y, 1, cardRect.Height), cardBorder);

			// 3. YoRHa Corner Brackets for Title Card
			Color bracketColor = new Color(240, 230, 200) * 0.92f;
			int bArm = 14;
			int bThick = 2;

			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, bThick, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bArm, cardRect.Y, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bThick, cardRect.Y, bThick, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - bThick, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - bArm, bThick, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bArm, cardRect.Bottom - bThick, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bThick, cardRect.Bottom - bArm, bThick, bArm), bracketColor);

			int midY = cardRect.Y + cardRect.Height / 2;
			sb.Draw(pixel, new Rectangle(cardRect.X - 1, midY - 2, 3, 5), bracketColor * 0.7f);
			sb.Draw(pixel, new Rectangle(cardRect.Right - 2, midY - 2, 3, 5), bracketColor * 0.7f);

			// 4. Top Classification Bar inside the card (Clean, no NOMINAL badge)
			float hdrScale = 0.52f;
			string headerText = "[ YoRHa FFCS // PROJECT: TERRARIA // VER. 1.1945 ]";
			Vector2 hdrPos = new Vector2(cardRect.X + 16, cardRect.Y + 7);
			Utils.DrawBorderString(sb, headerText, hdrPos, new Color(175, 168, 150) * 0.85f, hdrScale);

			// 5. Main Title & Katakana (Monochrome only: authentic bone-ivory, no red/cyan glitch)
			float textStartX = centerX - totalTitleW / 2f + glitchJitter;
			Vector2 titlePos = new Vector2(textStartX, titleY + 6f);
			Vector2 kataPos = new Vector2(textStartX + mainSize.X + 8f, titleY + 18f);

			// Subtle monochrome CRT ghosting during transition glitch (pure white, NO colored split)
			if (isGhosting)
			{
				Vector2 ghostOffset = new Vector2(glitchJitter * 0.75f, 0f);
				sb.DrawString(fontDeath, displayTitle, titlePos + ghostOffset, Color.White * 0.28f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
				sb.DrawString(fontMouse, displayKatakana, kataPos + ghostOffset, Color.White * 0.22f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			}

			// Clean drop shadows
			sb.DrawString(fontDeath, displayTitle, titlePos + new Vector2(2, 2), Color.Black * 0.85f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
			sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(1, 1), Color.Black * 0.7f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// Core Title Texts (pure authentic NieR bone-ivory, no red tints)
			Color mainTitleCol = new Color(248, 242, 222);
			sb.DrawString(fontDeath, displayTitle, titlePos, mainTitleCol, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

			Color kataCol = new Color(212, 198, 168);
			sb.DrawString(fontMouse, displayKatakana, kataPos, kataCol, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// 6. Tactical horizontal divider below the main title
			int divY = (int)(titleY + mainSize.Y + 4f);
			int divW = cardRect.Width - 32;
			int divX = cardRect.X + 16;
			Color divColor = new Color(210, 200, 175) * 0.50f;

			sb.Draw(pixel, new Rectangle(divX, divY, divW, 1), divColor);
			sb.Draw(pixel, new Rectangle(cardRect.Center.X - 3, divY - 2, 6, 5), new Color(245, 235, 205) * 0.80f);
			sb.Draw(pixel, new Rectangle(divX, divY - 2, 2, 5), divColor);
			sb.Draw(pixel, new Rectangle(divX + divW - 2, divY - 2, 2, 5), divColor);
			sb.Draw(pixel, new Rectangle(divX + (int)(divW * 0.25f), divY - 1, 2, 3), divColor * 0.7f);
			sb.Draw(pixel, new Rectangle(divX + (int)(divW * 0.75f), divY - 1, 2, 3), divColor * 0.7f);

			// 7. Subtitle line (For the Glory of Mankind)
			Vector2 subPos = new Vector2(centerX - subSize.X / 2f, divY + 6f);
			Color subColor = new Color(195, 186, 165) * 0.92f;
			Utils.DrawBorderString(sb, subTitle, subPos, subColor, subScale);
		}
	}
}
