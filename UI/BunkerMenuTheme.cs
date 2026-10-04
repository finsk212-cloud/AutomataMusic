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
		private static float glitchTimer = 0f;
		private static float nextGlitchInterval = 3.2f;
		private static float glitchIntensity = 0f;
		private static float microGlitchTimer = 0f;
		private static float nextMicroInterval = 1.1f;

		public static void Unload() => OrbitalBackdrop.Unload();

		public static void Draw(SpriteBatch sb, Vector2 logoDrawCenter)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
				return;

			int screenW = Main.screenWidth;
			int screenH = Main.screenHeight;
			float time = (float)Main.timeForVisualEffects * 0.02f;

			// Multi-frequency organic NieR terminal glitch generator
			glitchTimer += 0.016f;
			microGlitchTimer += 0.016f;

			// Frequent micro-twitches every 0.9 - 2.0 seconds
			if (microGlitchTimer > nextMicroInterval)
			{
				microGlitchTimer = 0f;
				nextMicroInterval = 0.9f + (float)Main.rand.NextDouble() * 1.1f;
				if (glitchIntensity < 0.45f)
					glitchIntensity = 0.55f;
			}

			// Major system desync & "AUTOMATA" title glitch every 2.8 - 4.6 seconds
			if (glitchTimer > nextGlitchInterval)
			{
				glitchTimer = 0f;
				nextGlitchInterval = 2.8f + (float)Main.rand.NextDouble() * 1.8f;
				glitchIntensity = 1.0f;
			}

			// Organic recovery decay
			if (glitchIntensity > 0f)
			{
				glitchIntensity -= 0.035f;
				if (glitchIntensity < 0f) glitchIntensity = 0f;
			}

			// 1. Fully procedural orbital scene (rotating Earth, sunrise, nebula, stars, moon)
			OrbitalBackdrop.Draw(sb, pixel, screenW, screenH);

			// 2. Tactical YoRHa Military Corner Brackets
			DrawTacticalFrame(sb, pixel, screenW, screenH, time);

			// 4. NieR:Automata Stylized Title Card / Logo
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

		private static string GetGlitchTitle(string original, float intensity, float time, out bool isAutomata)
		{
			isAutomata = false;
			if (intensity < 0.15f)
				return original;

			int seed = (int)(time * 22f);
			Random gRand = new Random(seed);

			// Glitch into "A U T O M A T A" for split seconds during peak glitch
			string target = original;
			if (intensity > 0.45f && (seed % 5 == 0 || seed % 7 == 0))
			{
				isAutomata = true;
				target = "A U T O M A T A";
			}

			char[] chars = target.ToCharArray();
			const string glitchPool = "01X_#@!$?%*[]/\\";

			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] == ' ')
					continue;

				// Chance to corrupt individual character into cyber glyph
				if (gRand.NextDouble() < intensity * 0.50f)
				{
					chars[i] = glitchPool[gRand.Next(glitchPool.Length)];
				}
			}

			return new string(chars);
		}

		private static string GetGlitchKatakana(string original, float intensity, float time, bool isAutomata)
		{
			if (intensity < 0.20f)
				return original;

			int seed = (int)(time * 20f) + 107;
			Random gRand = new Random(seed);

			string target = isAutomata ? " / オートマタ" : original;
			char[] chars = target.ToCharArray();

			const string kataGlitch = "01_#X/テラリアオートマタ";
			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] == ' ' || chars[i] == '/')
					continue;

				if (gRand.NextDouble() < intensity * 0.55f)
				{
					chars[i] = kataGlitch[gRand.Next(kataGlitch.Length)];
				}
			}

			return new string(chars);
		}

		private static void DrawNierTitleCard(SpriteBatch sb, Texture2D pixel, int screenW, Vector2 logoDrawCenter, float time)
		{
			var fontDeath = FontAssets.DeathText.Value;
			var fontMouse = FontAssets.MouseText.Value;
			if (fontDeath == null || fontMouse == null)
				return;

			// Multi-harmonic glitch crackle for rapid, erratic digital stutter
			float effectiveGlitch = glitchIntensity;
			if (effectiveGlitch > 0.04f)
			{
				float crackle = (float)(Math.Sin(time * 48f) * 0.40 + Math.Sin(time * 75f) * 0.30 + 0.70);
				effectiveGlitch = MathHelper.Clamp(effectiveGlitch * crackle, 0f, 1f);
			}

			float centerX = screenW / 2f;
			float titleY = Math.Max(70f, logoDrawCenter.Y - 60f);

			float jitterX = 0f;
			float jitterY = 0f;
			if (effectiveGlitch > 0.05f)
			{
				jitterX = (float)Math.Sin(time * 42f) * effectiveGlitch * 7.5f;
				jitterY = (float)Math.Cos(time * 34f) * effectiveGlitch * 3.5f;
			}

			string mainTitle = "T E R R A R I A";
			string katakana = " / テラリア";
			string subTitle = MenuLyrics.CheckCjkSupport(fontMouse)
				? "— YoRHa OS v4.02 // 人類に栄光あれ —"
				: "— YoRHa OS v4.02 // For the Glory of Mankind —";

			float titleScale = 0.90f;
			float katakanaScale = 0.78f;
			float subScale = 0.68f;

			Vector2 mainSize = fontDeath.MeasureString(mainTitle) * titleScale;
			Vector2 kataSize = fontMouse.MeasureString(katakana) * katakanaScale;
			Vector2 subSize = fontMouse.MeasureString(subTitle) * subScale;

			float totalTitleW = mainSize.X + kataSize.X + 8f;
			float cardW = Math.Max(totalTitleW, subSize.X) + 84f;
			float cardH = 88f;

			Rectangle cardRect = new Rectangle((int)(centerX - cardW / 2f + jitterX), (int)(titleY - 14f + jitterY), (int)cardW, (int)cardH);

			// 1. Soft dark translucent backing banner
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

			// 4. Horizontal CRT slice scanline displacement across the card during glitches
			if (effectiveGlitch > 0.18f)
			{
				int sliceCount = (int)(3 + effectiveGlitch * 4);
				for (int s = 0; s < sliceCount; s++)
				{
					int sliceY = cardRect.Y + 4 + (int)(((s + 0.5f) / sliceCount) * (cardRect.Height - 10));
					int sliceH = 4 + (s * 3) % 7;
					float shiftDir = (float)Math.Sin(time * 42f + s * 2.7f);
					int shiftPx = (int)(shiftDir * (8f + effectiveGlitch * 20f));

					Rectangle sliceRect = new Rectangle(cardRect.X + shiftPx, sliceY, cardRect.Width, sliceH);
					sb.Draw(pixel, sliceRect, new Color(14, 15, 20) * 0.95f);

					Color sliceColor = (s % 2 == 0)
						? new Color(255, 60, 90, 0) * (effectiveGlitch * 0.75f)
						: new Color(60, 220, 255, 0) * (effectiveGlitch * 0.75f);
					sb.Draw(pixel, new Rectangle(sliceRect.X, sliceY, sliceRect.Width, 1), sliceColor);
				}
			}

			// 5. Digital noise rectangles (bursting machine data corruption)
			if (effectiveGlitch > 0.28f)
			{
				Random nRand = new Random((int)(time * 28f));
				int numBlocks = (int)(4 + effectiveGlitch * 6);
				for (int b = 0; b < numBlocks; b++)
				{
					int bw = nRand.Next(18, 65);
					int bh = nRand.Next(2, 6);
					int bx = cardRect.X + nRand.Next(Math.Max(1, cardRect.Width - bw));
					int by = cardRect.Y + nRand.Next(Math.Max(1, cardRect.Height - bh));

					Color nCol = (nRand.Next(3) == 0)
						? new Color(255, 70, 90, 0) * (effectiveGlitch * 0.90f)
						: ((nRand.Next(2) == 0)
							? new Color(80, 230, 255, 0) * (effectiveGlitch * 0.90f)
							: new Color(250, 245, 230) * (effectiveGlitch * 0.85f));

					sb.Draw(pixel, new Rectangle(bx, by, bw, bh), nCol);
				}
			}

			// 6. Top Classification Bar inside the card
			float hdrScale = 0.52f;
			string headerText = "[ YoRHa FFCS // PROJECT: TERRARIA // VER. 1.1945 ]";
			Vector2 hdrPos = new Vector2(cardRect.X + 16, cardRect.Y + 7);
			Utils.DrawBorderString(sb, headerText, hdrPos, new Color(175, 168, 150) * 0.85f, hdrScale);

			// 7. Main Title & Katakana with Character Corruption & Chromatic Split
			string displayTitle = GetGlitchTitle(mainTitle, effectiveGlitch, time, out bool isAutomata);
			string baseKatakana = MenuLyrics.CheckCjkSupport(fontMouse) ? katakana : " / Terraria";
			string displayKatakana = GetGlitchKatakana(baseKatakana, effectiveGlitch, time, isAutomata);

			float textStartX = centerX - totalTitleW / 2f + jitterX;
			Vector2 titlePos = new Vector2(textStartX, titleY + 6f + jitterY);
			Vector2 kataPos = new Vector2(textStartX + mainSize.X + 8f, titleY + 18f + jitterY);

			// Chromatic aberration RGB split under glitch
			if (effectiveGlitch > 0.05f)
			{
				float splitX = (4.5f + 7.5f * effectiveGlitch) * (float)Math.Sin(time * 36f);
				float splitY = (float)Math.Cos(time * 28f) * (2.2f * effectiveGlitch);

				// Red / Magenta channel shifted left
				Color redGlitch = new Color(255, 45, 80, 0) * (effectiveGlitch * 0.92f);
				Vector2 redPos = titlePos + new Vector2(-splitX, -splitY);
				sb.DrawString(fontDeath, displayTitle, redPos, redGlitch, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

				// Cyan / Sky-blue channel shifted right
				Color cyanGlitch = new Color(45, 220, 255, 0) * (effectiveGlitch * 0.92f);
				Vector2 cyanPos = titlePos + new Vector2(splitX, splitY);
				sb.DrawString(fontDeath, displayTitle, cyanPos, cyanGlitch, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

				// Katakana chromatic split
				sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(-splitX * 0.7f, 0f), redGlitch * 0.85f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
				sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(splitX * 0.7f, 0f), cyanGlitch * 0.85f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			}

			// Drop shadows
			sb.DrawString(fontDeath, displayTitle, titlePos + new Vector2(2, 2), Color.Black * 0.85f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
			sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(1, 1), Color.Black * 0.7f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// Core Title Texts (flashes red/magenta tint during Automata glitch shift)
			Color mainTitleCol = isAutomata
				? new Color(255, 110, 110)
				: Color.Lerp(new Color(248, 242, 222), new Color(255, 130, 130), effectiveGlitch * 0.40f);
			sb.DrawString(fontDeath, displayTitle, titlePos, mainTitleCol, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

			Color kataCol = isAutomata
				? new Color(255, 130, 130)
				: Color.Lerp(new Color(212, 198, 168), new Color(255, 140, 140), effectiveGlitch * 0.40f);
			sb.DrawString(fontMouse, displayKatakana, kataPos, kataCol, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// 8. Tactical horizontal divider below the main title
			int divY = (int)(titleY + mainSize.Y + 4f + jitterY);
			int divW = cardRect.Width - 32;
			int divX = cardRect.X + 16;
			Color divColor = new Color(210, 200, 175) * 0.50f;

			sb.Draw(pixel, new Rectangle(divX, divY, divW, 1), divColor);
			sb.Draw(pixel, new Rectangle(cardRect.Center.X - 3, divY - 2, 6, 5), new Color(245, 235, 205) * 0.80f);
			sb.Draw(pixel, new Rectangle(divX, divY - 2, 2, 5), divColor);
			sb.Draw(pixel, new Rectangle(divX + divW - 2, divY - 2, 2, 5), divColor);
			sb.Draw(pixel, new Rectangle(divX + (int)(divW * 0.25f), divY - 1, 2, 3), divColor * 0.7f);
			sb.Draw(pixel, new Rectangle(divX + (int)(divW * 0.75f), divY - 1, 2, 3), divColor * 0.7f);

			// 9. Subtitle line (For the Glory of Mankind)
			Vector2 subPos = new Vector2(centerX - subSize.X / 2f + jitterX, divY + 6f);
			bool isDesync = effectiveGlitch > 0.25f;
			Color subColor = isDesync ? new Color(245, 130, 120) : new Color(195, 186, 165) * 0.92f;
			Utils.DrawBorderString(sb, subTitle, subPos, subColor, subScale);
		}
	}
}
