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

			// Deterministic title transition timer (6.5s total loop)
			titleTimer += 0.01667f;
			if (titleTimer >= 6.5f)
				titleTimer -= 6.5f;

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
		private static readonly int[] SwitchOrder = { 2, 5, 1, 6, 3, 0, 4, 7 };
		private static readonly char[] GlitchGlyphs = { '0', '1', 'u', 'x', 'm', 't', '_', '-', '/' };

		private static void GetTitleGlitchState(
			float timer, float time,
			out string[] displayLetters,
			out string katakanaRaw,
			out float ghostAlpha,
			out float sliceLineAlpha,
			out int whiteBlockSlot,
			out float[] slotJitterX,
			out float[] slotJitterY)
		{
			displayLetters = new string[8];
			slotJitterX = new float[8];
			slotJitterY = new float[8];
			whiteBlockSlot = -1;
			ghostAlpha = 0f;
			sliceLineAlpha = 0f;

			// Phase 0: Pure, stable "T E R R A R I A" (0.0s -> 3.2s)
			if (timer < 3.2f)
			{
				for (int i = 0; i < 8; i++)
					displayLetters[i] = TerrariaLetters[i];

				katakanaRaw = " / テラリア";
				return;
			}

			// Phase 1: Progressive NieR digital decode transition into Automata (3.2s -> 4.2s, 1.0s duration)
			if (timer < 4.2f)
			{
				float prog = (timer - 3.2f) / 1.0f; // 0.0 -> 1.0
				int switchedCount = (int)(prog * 8f);
				ghostAlpha = 0.45f * ((float)Math.Sin(time * 45f) * 0.3f + 0.7f);
				sliceLineAlpha = 0.65f * ((float)Math.Sin(time * 65f) * 0.4f + 0.6f);

				int seed = (int)(time * 24f);
				Random rand = new Random(seed);

				// Default all to Terraria
				for (int i = 0; i < 8; i++)
					displayLetters[i] = TerrariaLetters[i];

				// Switch letters according to NieR decoding sequence
				for (int s = 0; s < switchedCount && s < 8; s++)
				{
					int slot = SwitchOrder[s];
					displayLetters[slot] = AutomataLetters[slot];
				}

				// The currently decoding slot flickers with glyphs
				if (switchedCount < 8)
				{
					int activeSlot = SwitchOrder[switchedCount];
					displayLetters[activeSlot] = GlitchGlyphs[rand.Next(GlitchGlyphs.Length)].ToString();
					slotJitterX[activeSlot] = (float)(rand.NextDouble() * 3.0 - 1.5);
					slotJitterY[activeSlot] = (float)(rand.NextDouble() * 2.0 - 1.0);
				}

				// White data block artifact on 1 random glitched slot
				if (switchedCount > 0 && rand.NextDouble() < 0.75)
				{
					whiteBlockSlot = SwitchOrder[rand.Next(Math.Min(switchedCount + 1, 8))];
				}

				katakanaRaw = switchedCount >= 4 ? " / オートマタ" : " / テラリア";
				return;
			}

			// Phase 2: Full Hold on "A U T O M A T A" for 1.0 full second (4.2s -> 5.2s)
			if (timer < 5.2f)
			{
				for (int i = 0; i < 8; i++)
					displayLetters[i] = AutomataLetters[i];

				katakanaRaw = " / オートマタ";

				// Vertical ghost duplicate faintly visible underneath like title screen in video
				ghostAlpha = 0.36f + (float)Math.Sin(time * 8f) * 0.08f;

				// Subtle occasional white data block artifact on letter corner (e.g. slot 4 'M' or slot 6 'T')
				int cyclePulse = (int)(timer * 6f) % 4;
				if (cyclePulse == 1)
					whiteBlockSlot = 4; // 'M'
				else if (cyclePulse == 3)
					whiteBlockSlot = 6; // 'T'

				return;
			}

			// Phase 3: Glitch / digital decode back to "T E R R A R I A" (5.2s -> 5.7s, 0.5s duration)
			if (timer < 5.7f)
			{
				float prog = (timer - 5.2f) / 0.5f; // 0.0 -> 1.0
				int revertedCount = (int)(prog * 8f);
				ghostAlpha = 0.40f * ((float)Math.Sin(time * 50f) * 0.3f + 0.7f);
				sliceLineAlpha = 0.70f * ((float)Math.Sin(time * 70f) * 0.4f + 0.6f);

				int seed = (int)(time * 24f) + 42;
				Random rand = new Random(seed);

				// Start with Automata
				for (int i = 0; i < 8; i++)
					displayLetters[i] = AutomataLetters[i];

				// Revert letters back to Terraria in reverse sequence
				for (int s = 0; s < revertedCount && s < 8; s++)
				{
					int slot = SwitchOrder[7 - s];
					displayLetters[slot] = TerrariaLetters[slot];
				}

				if (revertedCount < 8)
				{
					int activeSlot = SwitchOrder[7 - revertedCount];
					displayLetters[activeSlot] = GlitchGlyphs[rand.Next(GlitchGlyphs.Length)].ToString();
					slotJitterX[activeSlot] = (float)(rand.NextDouble() * 3.0 - 1.5);
					slotJitterY[activeSlot] = (float)(rand.NextDouble() * 2.0 - 1.0);
				}

				if (revertedCount > 0 && rand.NextDouble() < 0.70)
				{
					whiteBlockSlot = SwitchOrder[rand.Next(8)];
				}

				katakanaRaw = revertedCount >= 4 ? " / テラリア" : " / オートマタ";
				return;
			}

			// Phase 4: Settle back to stable "T E R R A R I A" (5.7s -> 6.5s)
			for (int i = 0; i < 8; i++)
				displayLetters[i] = TerrariaLetters[i];

			katakanaRaw = " / テラリア";
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

			// NieR:Automata Title Glitch State
			GetTitleGlitchState(
				titleTimer, time,
				out string[] displayLetters,
				out string katakanaRaw,
				out float ghostAlpha,
				out float sliceLineAlpha,
				out int whiteBlockSlot,
				out float[] jitterX,
				out float[] jitterY);

			string displayKatakana = MenuLyrics.CheckCjkSupport(fontMouse)
				? katakanaRaw
				: (katakanaRaw.Contains("オートマタ") ? " / Automata" : " / Terraria");

			// Fixed slot layout so typography stays rock-solid
			float slotWidth = 33f;
			float slotsTotalW = slotWidth * 8f;
			Vector2 kataSize = fontMouse.MeasureString(displayKatakana) * katakanaScale;
			Vector2 subSize = fontMouse.MeasureString(subTitle) * subScale;

			float totalTitleW = slotsTotalW + kataSize.X + 12f;
			float cardW = Math.Max(totalTitleW, subSize.X) + 84f;
			float cardH = 88f;

			Rectangle cardRect = new Rectangle((int)(centerX - cardW / 2f), (int)(titleY - 14f), (int)cardW, (int)cardH);

			// 1. Clean dark translucent backing banner
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

			// 4. Top Classification Bar inside the card
			float hdrScale = 0.52f;
			string headerText = "[ YoRHa FFCS // PROJECT: TERRARIA // VER. 1.1945 ]";
			Vector2 hdrPos = new Vector2(cardRect.X + 16, cardRect.Y + 7);
			Utils.DrawBorderString(sb, headerText, hdrPos, new Color(175, 168, 150) * 0.85f, hdrScale);

			// 5. Main Title Letters & Katakana Sub-Logo
			float textStartX = centerX - totalTitleW / 2f;
			Color mainTitleCol = new Color(248, 242, 222);
			Color ghostCol = new Color(176, 168, 148);

			// Draw 8 letter slots individually
			for (int i = 0; i < 8; i++)
			{
				string letter = displayLetters[i];
				Vector2 lSize = fontDeath.MeasureString(letter) * titleScale;
				float slotX = textStartX + i * slotWidth;
				float drawX = slotX + (slotWidth - lSize.X) / 2f + jitterX[i];
				float drawY = titleY + 6f + jitterY[i];
				Vector2 charPos = new Vector2(drawX, drawY);

				// NieR vertical ghost duplicate underneath (Slide 7 in reference video)
				if (ghostAlpha > 0f)
				{
					Vector2 ghostPos = charPos + new Vector2(0f, 11f);
					sb.DrawString(fontDeath, letter, ghostPos, ghostCol * ghostAlpha, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
				}

				// Clean drop shadow
				sb.DrawString(fontDeath, letter, charPos + new Vector2(2, 2), Color.Black * 0.85f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

				// Main letter glyph
				sb.DrawString(fontDeath, letter, charPos, mainTitleCol, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

				// NieR solid white data block glitch artifact (Slide 1, 3, 7 in reference video)
				if (i == whiteBlockSlot)
				{
					int blockW = 14;
					int blockH = 6;
					int bx = (int)(drawX + lSize.X / 2f - blockW / 2f);
					int by = (int)(drawY + lSize.Y * 0.45f);
					sb.Draw(pixel, new Rectangle(bx, by, blockW, blockH), Color.White * 0.95f);
				}
			}

			// Katakana subtitle (drawn to the right of the 8 letters)
			Vector2 kataPos = new Vector2(textStartX + slotsTotalW + 10f, titleY + 18f);

			if (ghostAlpha > 0f)
			{
				Vector2 kataGhostPos = kataPos + new Vector2(0f, 9f);
				sb.DrawString(fontMouse, displayKatakana, kataGhostPos, ghostCol * (ghostAlpha * 0.70f), 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			}

			sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(1, 1), Color.Black * 0.70f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			sb.DrawString(fontMouse, displayKatakana, kataPos, new Color(212, 198, 168), 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// Center razor slice line across the title (Slide 0 & 3 in reference video)
			if (sliceLineAlpha > 0f)
			{
				int lineY = (int)(titleY + 24f);
				int lineX = (int)(textStartX - 8f);
				int lineW = (int)(totalTitleW + 16f);
				sb.Draw(pixel, new Rectangle(lineX, lineY, lineW, 1), new Color(245, 240, 225) * sliceLineAlpha);
			}

			// 6. Tactical horizontal divider below the main title
			int divY = (int)(titleY + 48f);
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
