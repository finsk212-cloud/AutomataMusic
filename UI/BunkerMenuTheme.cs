using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AutomataMusic.UI
{
	public static class BunkerMenuTheme
	{
		private struct Particle
		{
			public Vector2 Position;
			public float SpeedY;
			public float SpeedX;
			public float Size;
			public float BaseAlpha;
			public float Phase;
		}

		private static readonly Particle[] particles = new Particle[36];
		private static bool particlesInitialized = false;
		private static float scanLineY = 0f;
		private static float glitchTimer = 0f;
		private static float glitchIntensity = 0f;
		private static Asset<Texture2D> earthTexture = null;

		private static void InitializeParticles()
		{
			var rand = new Random(11945);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i] = new Particle
				{
					Position = new Vector2((float)rand.NextDouble() * 1920f, (float)rand.NextDouble() * 1080f),
					SpeedY = 0.20f + (float)rand.NextDouble() * 0.40f,
					SpeedX = -0.1f + (float)rand.NextDouble() * 0.2f,
					Size = 1.5f + (float)rand.NextDouble() * 2.0f,
					BaseAlpha = 0.15f + (float)rand.NextDouble() * 0.35f,
					Phase = (float)rand.NextDouble() * MathHelper.TwoPi
				};
			}
			particlesInitialized = true;
		}

		public static void Draw(SpriteBatch sb, Vector2 logoDrawCenter)
		{
			if (!particlesInitialized)
			{
				InitializeParticles();
			}

			if (earthTexture == null)
			{
				try
				{
					earthTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Earth", AssetRequestMode.ImmediateLoad);
				}
				catch
				{
				}
			}

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
				return;

			int screenW = Main.screenWidth;
			int screenH = Main.screenHeight;
			float time = (float)Main.timeForVisualEffects * 0.02f;

			// Update glitch / terminal jitter timer
			glitchTimer += 0.016f;
			if (glitchTimer > 5.2f)
			{
				glitchTimer = 0f;
				glitchIntensity = 1f;
			}
			if (glitchIntensity > 0f)
			{
				glitchIntensity -= 0.08f;
				if (glitchIntensity < 0f) glitchIntensity = 0f;
			}

			// 1. Deep Space Base Canvas (#0c0c10)
			sb.Draw(pixel, new Rectangle(0, 0, screenW, screenH), new Color(12, 12, 16));

			// 2. Planet Earth in Low Orbit as seen from the YoRHa Bunker
			if (earthTexture != null && earthTexture.IsLoaded && earthTexture.Value != null)
			{
				Texture2D earth = earthTexture.Value;
				Rectangle earthDest = new Rectangle(0, 0, screenW, screenH);
				sb.Draw(earth, earthDest, Color.White * 0.92f);
			}

			// 3. Tactical Gridlines (54px spacing) with micro-crosshairs
			Color gridColor = new Color(50, 52, 65) * 0.26f;
			Color crosshairColor = new Color(195, 185, 155) * 0.35f;
			int gridSize = 54;

			for (int x = 0; x < screenW; x += gridSize)
			{
				sb.Draw(pixel, new Rectangle(x, 0, 1, screenH), gridColor);
			}
			for (int y = 0; y < screenH; y += gridSize)
			{
				sb.Draw(pixel, new Rectangle(0, y, screenW, 1), gridColor);
			}

			// Draw small '+' crosshair marks at selected grid intersections
			for (int x = gridSize * 2; x < screenW - gridSize; x += gridSize * 3)
			{
				for (int y = gridSize * 2; y < screenH - gridSize; y += gridSize * 3)
				{
					sb.Draw(pixel, new Rectangle(x - 3, y, 7, 1), crosshairColor);
					sb.Draw(pixel, new Rectangle(x, y - 3, 1, 7), crosshairColor);
				}
			}

			// 4. Floating Digital Atmospheric Dust / Embers drifting upward
			Color emberColor = new Color(230, 222, 195);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position.Y -= particles[i].SpeedY;
				particles[i].Position.X += (float)Math.Sin(time + particles[i].Phase) * 0.3f + particles[i].SpeedX;

				if (particles[i].Position.Y < -10f)
				{
					particles[i].Position.Y = screenH + 10f;
					particles[i].Position.X = (float)(new Random(i + (int)Main.timeForVisualEffects).NextDouble() * screenW);
				}
				if (particles[i].Position.X < -10f) particles[i].Position.X = screenW + 10f;
				if (particles[i].Position.X > screenW + 10f) particles[i].Position.X = -10f;

				float pulse = (float)Math.Sin(time * 2f + particles[i].Phase) * 0.25f + 0.75f;
				float alpha = particles[i].BaseAlpha * pulse;
				int pSize = (int)particles[i].Size;

				sb.Draw(pixel, new Rectangle((int)particles[i].Position.X, (int)particles[i].Position.Y, pSize, pSize), emberColor * alpha);
			}

			// 5. Subtle CRT Scanlines
			Color scanlineColor = new Color(0, 0, 0, 16);
			for (int y = 0; y < screenH; y += 4)
			{
				sb.Draw(pixel, new Rectangle(0, y, screenW, 1), scanlineColor);
			}

			// Sweeping radar / terminal refresh beam
			scanLineY += 1.8f;
			if (scanLineY > screenH) scanLineY = 0f;
			sb.Draw(pixel, new Rectangle(0, (int)scanLineY, screenW, 2), new Color(210, 200, 170) * 0.10f);
			sb.Draw(pixel, new Rectangle(0, (int)scanLineY - 8, screenW, 8), new Color(210, 200, 170) * 0.03f);

			// 6. Tactical YoRHa Military HUD Framing (tight, pixel-perfect corner alignment)
			DrawTacticalFrame(sb, pixel, screenW, screenH, time);

			// 7. NieR:Automata Stylized Title Card / Logo (without "Weight of the World")
			DrawNierTitleCard(sb, pixel, screenW, logoDrawCenter);
		}

		private static void DrawTacticalFrame(SpriteBatch sb, Texture2D pixel, int screenW, int screenH, float time)
		{
			var font = FontAssets.MouseText.Value;
			int inset = 22;
			int bracketArm = 48;
			int thick = 2;

			Color frameBeige = new Color(230, 220, 190) * 0.90f;
			Color faintBorder = new Color(185, 175, 150) * 0.20f;

			// Connecting faint boundary line
			sb.Draw(pixel, new Rectangle(inset, inset, screenW - inset * 2, 1), faintBorder);
			sb.Draw(pixel, new Rectangle(inset, screenH - inset, screenW - inset * 2, 1), faintBorder);
			sb.Draw(pixel, new Rectangle(inset, inset, 1, screenH - inset * 2), faintBorder);
			sb.Draw(pixel, new Rectangle(screenW - inset, inset, 1, screenH - inset * 2), faintBorder);

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

			// ── Top-Left Corner: Snug within the ┌ bracket ──
			float textScale = 0.68f;
			float subScale = 0.58f;
			int padX = 14;
			int padY = 8;

			string tlHeader = "[ YoRHa SATELLITE ORBITAL BASE // \"THE BUNKER\" ]";
			string tlSub = "SECURITY: LEVEL 4 // COMM-LINK: STABLE // OPERATOR: 6O";
			Vector2 tlHeaderSize = font.MeasureString(tlHeader) * textScale;

			// Subtle protective backing panel so HUD text is 100% legible against Earth
			Rectangle tlPanel = new Rectangle(inset + padX - 4, inset + padY - 2, (int)tlHeaderSize.X + 8, 36);
			sb.Draw(pixel, tlPanel, new Color(10, 10, 15) * 0.75f);

			Utils.DrawBorderString(sb, tlHeader, new Vector2(inset + padX, inset + padY), new Color(242, 232, 208), textScale);
			Utils.DrawBorderString(sb, tlSub, new Vector2(inset + padX, inset + padY + 18), new Color(175, 170, 155) * 0.85f, subScale);

			// ── Top-Right Corner: Snug within the ┐ bracket (Right-Aligned) ──
			DateTime now = DateTime.Now;
			string trHeader = $"SYS TIME: {now:HH:mm:ss} // CYCLE: 11945.03.10";
			string trSub = "FFCS: ONLINE // LINK QUALITY: 99.8% // STATUS: NOMINAL";
			Vector2 trHeaderSize = font.MeasureString(trHeader) * textScale;
			Vector2 trSubSize = font.MeasureString(trSub) * subScale;
			float trMaxW = Math.Max(trHeaderSize.X, trSubSize.X);

			Rectangle trPanel = new Rectangle((int)(screenW - inset - padX - trMaxW - 4), inset + padY - 2, (int)trMaxW + 8, 36);
			sb.Draw(pixel, trPanel, new Color(10, 10, 15) * 0.75f);

			Utils.DrawBorderString(sb, trHeader, new Vector2(screenW - inset - padX - trHeaderSize.X, inset + padY), new Color(235, 226, 204), textScale);
			Utils.DrawBorderString(sb, trSub, new Vector2(screenW - inset - padX - trSubSize.X, inset + padY + 18), new Color(175, 170, 155) * 0.85f, subScale);

			// ── Bottom-Left Corner: Snug within the └ bracket ──
			string blHeader = "POD 042: STANDBY [NORMAL] // FFCS: ENGAGED";
			string blSub = "PROGRAMS: LASER [ONLINE] // GATLING [ARMED] // SHIELD [READY]";
			Vector2 blHeaderSize = font.MeasureString(blHeader) * textScale;
			Vector2 blSubSize = font.MeasureString(blSub) * subScale;
			float blMaxW = Math.Max(blHeaderSize.X, blSubSize.X);

			Rectangle blPanel = new Rectangle(inset + padX - 4, screenH - inset - padY - 34, (int)blMaxW + 8, 36);
			sb.Draw(pixel, blPanel, new Color(10, 10, 15) * 0.75f);

			Utils.DrawBorderString(sb, blHeader, new Vector2(inset + padX, screenH - inset - padY - 32), new Color(238, 228, 204), textScale);
			Utils.DrawBorderString(sb, blSub, new Vector2(inset + padX, screenH - inset - padY - 14), new Color(170, 165, 150) * 0.85f, subScale);

			// ── Bottom-Right Corner: Snug within the ┘ bracket (Audio Equalizer) ──
			DrawAudioVisualizer(sb, pixel, screenW - inset - padX, screenH - inset - padY, time);

			// Left border altitude ruler
			int rulerYStart = screenH / 2 - 100;
			for (int r = 0; r < 7; r++)
			{
				int ry = rulerYStart + r * 28;
				int rw = (r % 2 == 0) ? 8 : 4;
				sb.Draw(pixel, new Rectangle(inset + 4, ry, rw, 1), frameBeige * 0.50f);
			}
			Utils.DrawBorderString(sb, "ELEV: 35,420M", new Vector2(inset + 16, rulerYStart - 16), new Color(175, 165, 145) * 0.70f, 0.55f);
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
			Vector2 lSize = font != null ? font.MeasureString(label) * 0.58f : Vector2.Zero;
			float blockW = Math.Max(totalW, lSize.X);

			// Dark backing panel
			Rectangle brPanel = new Rectangle((int)(rightX - blockW - 4), bottomY - 34, (int)blockW + 8, 36);
			sb.Draw(pixel, brPanel, new Color(10, 10, 15) * 0.75f);

			if (font != null)
			{
				Utils.DrawBorderString(sb, label, new Vector2(rightX - lSize.X, bottomY - 30), new Color(185, 175, 150) * 0.85f, 0.58f);
			}

			Color barColor = new Color(238, 222, 185) * 0.90f;
			for (int b = 0; b < barCount; b++)
			{
				float wave = (float)Math.Sin(time * 6f + b * 0.85f) * 0.5f + 0.5f;
				float wave2 = (float)Math.Cos(time * 3.5f - b * 0.45f) * 0.3f + 0.3f;
				float hRatio = MathHelper.Clamp(wave * 0.7f + wave2 * 0.3f, 0.15f, 1f);
				int barH = (int)(hRatio * 16f);

				int bx = startX + b * (barW + barGap);
				int by = bottomY - 4 - barH;
				sb.Draw(pixel, new Rectangle(bx, by, barW, barH), barColor);
			}
		}

		private static void DrawNierTitleCard(SpriteBatch sb, Texture2D pixel, int screenW, Vector2 logoDrawCenter)
		{
			var fontDeath = FontAssets.DeathText.Value;
			var fontMouse = FontAssets.MouseText.Value;
			if (fontDeath == null || fontMouse == null)
				return;

			float centerX = screenW / 2f;
			float titleY = Math.Max(70f, logoDrawCenter.Y - 60f);

			// Jitter/glitch micro-offset
			float jitterX = 0f;
			float jitterY = 0f;
			if (glitchIntensity > 0.05f)
			{
				jitterX = (float)Math.Sin(Main.timeForVisualEffects * 18f) * glitchIntensity * 3.5f;
				jitterY = (float)Math.Cos(Main.timeForVisualEffects * 14f) * glitchIntensity * 1.5f;
			}

			// Main Title String: T E R R A R I A
			string mainTitle = "T E R R A R I A";
			string katakana = " / テラリア";
			// Clean YoRHa slogan instead of Weight of the World
			string subTitle = MenuLyrics.CheckCjkSupport(fontMouse)
				? "— YoRHa OS v4.02 // 人類に栄光あれ —"
				: "— YoRHa OS v4.02 // For the Glory of Mankind —";

			float titleScale = 0.92f;
			float katakanaScale = 0.78f;
			float subScale = 0.70f;

			Vector2 mainSize = fontDeath.MeasureString(mainTitle) * titleScale;
			Vector2 kataSize = fontMouse.MeasureString(katakana) * katakanaScale;
			Vector2 subSize = fontMouse.MeasureString(subTitle) * subScale;

			float totalTitleW = mainSize.X + kataSize.X + 8f;
			float cardW = Math.Max(totalTitleW, subSize.X) + 72f;
			float cardH = 74f;

			Rectangle cardRect = new Rectangle((int)(centerX - cardW / 2f + jitterX), (int)(titleY - 8f + jitterY), (int)cardW, (int)cardH);

			// 1. Soft dark backing banner
			sb.Draw(pixel, cardRect, new Color(11, 11, 15) * 0.90f);

			// 2. Faint border
			Color cardBorder = new Color(185, 175, 145) * 0.40f;
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - 1, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, 1, cardRect.Height), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.Right - 1, cardRect.Y, 1, cardRect.Height), cardBorder);

			// 3. YoRHa Corner Brackets for Title Card
			Color bracketColor = new Color(240, 230, 200) * 0.92f;
			int bArm = 12;
			int bThick = 2;

			// Top-Left ┌
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, bThick, bArm), bracketColor);
			// Top-Right ┐
			sb.Draw(pixel, new Rectangle(cardRect.Right - bArm, cardRect.Y, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bThick, cardRect.Y, bThick, bArm), bracketColor);
			// Bottom-Left └
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - bThick, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - bArm, bThick, bArm), bracketColor);
			// Bottom-Right ┘
			sb.Draw(pixel, new Rectangle(cardRect.Right - bArm, cardRect.Bottom - bThick, bArm, bThick), bracketColor);
			sb.Draw(pixel, new Rectangle(cardRect.Right - bThick, cardRect.Bottom - bArm, bThick, bArm), bracketColor);

			// Decorative center notches
			int midY = cardRect.Y + cardRect.Height / 2;
			sb.Draw(pixel, new Rectangle(cardRect.X - 1, midY - 2, 3, 5), bracketColor * 0.7f);
			sb.Draw(pixel, new Rectangle(cardRect.Right - 2, midY - 2, 3, 5), bracketColor * 0.7f);

			// 4. Draw Main Title: T E R R A R I A
			float textStartX = centerX - totalTitleW / 2f + jitterX;
			Vector2 titlePos = new Vector2(textStartX, titleY + jitterY);

			// Shadow
			sb.DrawString(fontDeath, mainTitle, titlePos + new Vector2(2, 2), Color.Black * 0.7f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
			// Main text in warm pale sand
			sb.DrawString(fontDeath, mainTitle, titlePos, new Color(248, 240, 218), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

			// 5. Draw Katakana subtitle
			Vector2 kataPos = new Vector2(textStartX + mainSize.X + 8f, titleY + 12f + jitterY);
			string displayKatakana = MenuLyrics.CheckCjkSupport(fontMouse) ? katakana : " / Terraria";
			sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(1, 1), Color.Black * 0.6f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			sb.DrawString(fontMouse, displayKatakana, kataPos, new Color(210, 195, 160), 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// 6. Subtitle line (For the Glory of Mankind)
			Vector2 subPos = new Vector2(centerX - subSize.X / 2f + jitterX, titleY + 42f + jitterY);
			Utils.DrawBorderString(sb, subTitle, subPos, new Color(190, 182, 162) * 0.90f, subScale);
		}
	}
}
