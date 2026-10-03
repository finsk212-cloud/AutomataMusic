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

		private struct Star
		{
			public Vector2 Position;
			public float DriftSpeedX;
			public float DriftSpeedY;
			public float BaseAlpha;
			public float TwinkleSpeed;
			public float Phase;
			public float SecondaryPhase;
			public float Size;
			public Color StarColor;
			public int Layer;
		}

		// Textures
		private static Asset<Texture2D> earthTexture = null;

		// Systems
		private static readonly Particle[] particles = new Particle[32];
		private static readonly Star[] stars = new Star[120];
		private static bool initialized = false;

		// FX
		private static float scanLineY = 0f;
		private static float glitchTimer = 0f;
		private static float glitchIntensity = 0f;

		private static void InitializeSystems()
		{
			var rand = new Random(11945);

			// Floating digital embers
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

			// Realistic twinkling & faintly drifting stars across 3 depth layers
			for (int i = 0; i < stars.Length; i++)
			{
				float layerRand = (float)rand.NextDouble();
				int layer = layerRand < 0.65f ? 0 : (layerRand < 0.90f ? 1 : 2);

				float size = layer switch
				{
					0 => 1f,
					1 => 1.5f,
					_ => 2f
				};

				float baseAlpha = layer switch
				{
					0 => 0.18f + (float)rand.NextDouble() * 0.28f,
					1 => 0.38f + (float)rand.NextDouble() * 0.35f,
					_ => 0.65f + (float)rand.NextDouble() * 0.35f
				};

				// Ultra-slow celestial parallax drift (slowly panning across deep space)
				float driftSpeedX = (layer + 1) * (0.015f + (float)rand.NextDouble() * 0.025f);
				float driftSpeedY = (layer + 1) * (-0.006f + (float)rand.NextDouble() * 0.012f);

				// Natural stellar color spectrum (cool diamond blue, pure white, faint golden white, pale silver)
				Color starColor = rand.Next(4) switch
				{
					0 => new Color(210, 228, 255), // Cool diamond blue
					1 => new Color(255, 252, 245), // Pure stellar white
					2 => new Color(255, 242, 218), // Warm golden dwarf
					_ => new Color(225, 232, 250)  // Pale silver
				};

				stars[i] = new Star
				{
					Position = new Vector2((float)rand.NextDouble() * 1920f, (float)rand.NextDouble() * 1080f),
					DriftSpeedX = driftSpeedX,
					DriftSpeedY = driftSpeedY,
					BaseAlpha = baseAlpha,
					TwinkleSpeed = 0.9f + (float)rand.NextDouble() * 2.4f,
					Phase = (float)rand.NextDouble() * MathHelper.TwoPi,
					SecondaryPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
					Size = size,
					StarColor = starColor,
					Layer = layer
				};
			}

			initialized = true;
		}

		private static void EnsureTexturesLoaded()
		{
			try
			{
				if (earthTexture == null)
					earthTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Earth", AssetRequestMode.ImmediateLoad);
			}
			catch
			{
			}
		}

		public static void Draw(SpriteBatch sb, Vector2 logoDrawCenter)
		{
			if (!initialized)
			{
				InitializeSystems();
			}

			EnsureTexturesLoaded();

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			if (pixel == null)
				return;

			int screenW = Main.screenWidth;
			int screenH = Main.screenHeight;
			float time = (float)Main.timeForVisualEffects * 0.02f;

			// Update glitch / terminal jitter timer
			glitchTimer += 0.016f;
			if (glitchTimer > 5.5f)
			{
				glitchTimer = 0f;
				glitchIntensity = 1f;
			}
			if (glitchIntensity > 0f)
			{
				glitchIntensity -= 0.08f;
				if (glitchIntensity < 0f) glitchIntensity = 0f;
			}

			// 1. Deep Space Base Canvas (#07070a)
			sb.Draw(pixel, new Rectangle(0, 0, screenW, screenH), new Color(7, 7, 10));

			// 2. Twinkling Background Stars (Serene, deep space feel)
			DrawStars(sb, pixel, time, screenW, screenH);

			// 3. Planet Earth in Orbit (Not too close! Positioned in lower portion showing curvature & atmosphere)
			DrawEarth(sb, pixel, time, screenW, screenH);

			// 4. Tactical Gridlines (54px spacing) with micro-crosshairs
			DrawTacticalGrid(sb, pixel, screenW, screenH);

			// 5. Floating Digital Atmospheric Dust / Embers drifting upward
			DrawEmbers(sb, pixel, time, screenW, screenH);

			// 7. Subtle CRT Scanlines & Radar Sweep
			DrawScanlinesAndRadar(sb, pixel, screenW, screenH);

			// 8. Tactical YoRHa Military HUD Framing (Unchanged, clean corner alignment)
			DrawTacticalFrame(sb, pixel, screenW, screenH, time);

			// 9. NieR:Automata Stylized Title Card / Logo
			DrawNierTitleCard(sb, pixel, screenW, logoDrawCenter);
		}

		private static void DrawStars(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				// Faint celestial drift (moves slowly across space)
				stars[i].Position.X += stars[i].DriftSpeedX;
				stars[i].Position.Y += stars[i].DriftSpeedY;
				if (stars[i].Position.X > 1920f) stars[i].Position.X -= 1920f;
				if (stars[i].Position.X < 0f) stars[i].Position.X += 1920f;
				if (stars[i].Position.Y > 1080f) stars[i].Position.Y -= 1080f;
				if (stars[i].Position.Y < 0f) stars[i].Position.Y += 1080f;

				// Organic multi-harmonic twinkling (like real stars in space)
				float wave1 = (float)Math.Sin(time * stars[i].TwinkleSpeed + stars[i].Phase);
				float wave2 = (float)Math.Sin(time * (stars[i].TwinkleSpeed * 1.618f) + stars[i].SecondaryPhase);
				float norm = (wave1 * 0.65f + wave2 * 0.35f + 1f) * 0.5f; // [0, 1]
				float shimmer = (float)Math.Pow(norm, 1.65); // Momentary glints, serene rest periods

				float alpha = MathHelper.Clamp(stars[i].BaseAlpha * (0.22f + 0.92f * shimmer), 0f, 1f);

				int sx = (int)(stars[i].Position.X * (screenW / 1920f));
				int sy = (int)(stars[i].Position.Y * (screenH / 1080f));
				int sz = (int)Math.Max(1f, stars[i].Size * (screenW / 1920f));

				// For brighter foreground stars during a bright glint, render a subtle soft glow halo
				if (stars[i].Layer == 2 && shimmer > 0.60f)
				{
					float haloStrength = (shimmer - 0.60f) * 2.5f;
					Color haloCol = stars[i].StarColor * (alpha * haloStrength * 0.22f);
					sb.Draw(pixel, new Rectangle(sx - 1, sy - 1, sz + 2, sz + 2), haloCol);
				}

				sb.Draw(pixel, new Rectangle(sx, sy, sz, sz), stars[i].StarColor * alpha);
			}
		}

		private static void DrawEarth(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			if (earthTexture == null || !earthTexture.IsLoaded || earthTexture.Value == null)
				return;

			Texture2D earth = earthTexture.Value;

			// Orbital drift: Earth slowly shifts gently across the viewport
			float driftX = (float)Math.Sin(time * 0.05f) * 20f;
			float driftY = (float)Math.Cos(time * 0.04f) * 10f;

			// Position Earth so it is "not too close"
			// The curved blue atmospheric horizon rests gracefully across the lower-center of the screen
			int eW = (int)(screenW * 1.15f);
			int eH = (int)(eW * (earth.Height / (float)earth.Width));
			int eX = (int)((screenW - eW) / 2f + driftX);
			// Lowered so the top ~50% of the screen is deep space and the curved planet spans the bottom half
			int eY = (int)(screenH * 0.28f + driftY);

			Rectangle earthDest = new Rectangle(eX, eY, eW, eH);

			// Atmospheric blue glow feather on top of Earth
			Color earthColor = new Color(195, 205, 220) * 0.82f;
			sb.Draw(earth, earthDest, earthColor);
		}

		private static void DrawEmbers(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
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
		}

		private static void DrawScanlinesAndRadar(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			Color scanlineColor = new Color(0, 0, 0, 16);
			for (int y = 0; y < screenH; y += 4)
			{
				sb.Draw(pixel, new Rectangle(0, y, screenW, 1), scanlineColor);
			}

			scanLineY += 1.8f;
			if (scanLineY > screenH) scanLineY = 0f;
			sb.Draw(pixel, new Rectangle(0, (int)scanLineY, screenW, 2), new Color(210, 200, 170) * 0.09f);
			sb.Draw(pixel, new Rectangle(0, (int)scanLineY - 8, screenW, 8), new Color(210, 200, 170) * 0.03f);
		}

		private static void DrawTacticalGrid(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			Color gridColor = new Color(50, 52, 65) * 0.22f;
			Color crosshairColor = new Color(195, 185, 155) * 0.30f;
			int gridSize = 54;

			for (int x = 0; x < screenW; x += gridSize)
			{
				sb.Draw(pixel, new Rectangle(x, 0, 1, screenH), gridColor);
			}
			for (int y = 0; y < screenH; y += gridSize)
			{
				sb.Draw(pixel, new Rectangle(0, y, screenW, 1), gridColor);
			}

			for (int x = gridSize * 2; x < screenW - gridSize; x += gridSize * 3)
			{
				for (int y = gridSize * 2; y < screenH - gridSize; y += gridSize * 3)
				{
					sb.Draw(pixel, new Rectangle(x - 3, y, 7, 1), crosshairColor);
					sb.Draw(pixel, new Rectangle(x, y - 3, 1, 7), crosshairColor);
				}
			}
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

			// Top-Left Corner: Snug within the ┌ bracket
			float textScale = 0.68f;
			float subScale = 0.58f;
			int padX = 14;
			int padY = 8;

			string tlHeader = "[ YoRHa SATELLITE ORBITAL BASE // \"THE BUNKER\" ]";
			string tlSub = "SECURITY: LEVEL 4 // COMM-LINK: STABLE // OPERATOR: 6O";

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

		private static void DrawNierTitleCard(SpriteBatch sb, Texture2D pixel, int screenW, Vector2 logoDrawCenter)
		{
			var fontDeath = FontAssets.DeathText.Value;
			var fontMouse = FontAssets.MouseText.Value;
			if (fontDeath == null || fontMouse == null)
				return;

			float centerX = screenW / 2f;
			float titleY = Math.Max(70f, logoDrawCenter.Y - 60f);

			float jitterX = 0f;
			float jitterY = 0f;
			if (glitchIntensity > 0.05f)
			{
				jitterX = (float)Math.Sin(Main.timeForVisualEffects * 18f) * glitchIntensity * 3.5f;
				jitterY = (float)Math.Cos(Main.timeForVisualEffects * 14f) * glitchIntensity * 1.5f;
			}

			string mainTitle = "T E R R A R I A";
			string katakana = " / テラリア";
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

			// Soft dark backing banner
			sb.Draw(pixel, cardRect, new Color(11, 11, 15) * 0.90f);

			// Faint border
			Color cardBorder = new Color(185, 175, 145) * 0.40f;
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Bottom - 1, cardRect.Width, 1), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.X, cardRect.Y, 1, cardRect.Height), cardBorder);
			sb.Draw(pixel, new Rectangle(cardRect.Right - 1, cardRect.Y, 1, cardRect.Height), cardBorder);

			// YoRHa Corner Brackets for Title Card
			Color bracketColor = new Color(240, 230, 200) * 0.92f;
			int bArm = 12;
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

			float textStartX = centerX - totalTitleW / 2f + jitterX;
			Vector2 titlePos = new Vector2(textStartX, titleY + jitterY);

			// Main Title Shadow & Text
			sb.DrawString(fontDeath, mainTitle, titlePos + new Vector2(2, 2), Color.Black * 0.7f, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);
			sb.DrawString(fontDeath, mainTitle, titlePos, new Color(248, 240, 218), 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

			// Katakana Subtitle
			Vector2 kataPos = new Vector2(textStartX + mainSize.X + 8f, titleY + 12f + jitterY);
			string displayKatakana = MenuLyrics.CheckCjkSupport(fontMouse) ? katakana : " / Terraria";
			sb.DrawString(fontMouse, displayKatakana, kataPos + new Vector2(1, 1), Color.Black * 0.6f, 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);
			sb.DrawString(fontMouse, displayKatakana, kataPos, new Color(210, 195, 160), 0f, Vector2.Zero, katakanaScale, SpriteEffects.None, 0f);

			// Subtitle line (For the Glory of Mankind)
			Vector2 subPos = new Vector2(centerX - subSize.X / 2f + jitterX, titleY + 42f + jitterY);
			Utils.DrawBorderString(sb, subTitle, subPos, new Color(190, 182, 162) * 0.90f, subScale);
		}
	}
}
