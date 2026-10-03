using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
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
			public float BaseAlpha;
			public float TwinkleSpeed;
			public float Phase;
			public float Size;
		}

		private struct AlienDrop
		{
			public Vector2 Start;
			public Vector2 Current;
			public Vector2 Target;
			public float Progress;
			public float Speed;
			public float TrailLength;
			public float ImpactTimer;
			public bool Active;
		}

		// Textures
		private static Asset<Texture2D> earthTexture = null;
		private static Asset<Texture2D> bunkerTexture = null;
		private static Asset<Texture2D> moonTexture = null;
		private static Asset<Texture2D> flightUnitTexture = null;
		private static Asset<Texture2D> emilTexture = null;

		// Systems & Particles
		private static readonly Particle[] particles = new Particle[36];
		private static readonly Star[] stars = new Star[60];
		private static readonly AlienDrop[] alienDrops = new AlienDrop[4];
		private static bool initialized = false;

		// Dynamics & FX
		private static float scanLineY = 0f;
		private static float glitchTimer = 0f;
		private static float glitchIntensity = 0f;
		private static float alienSpawnTimer = 0f;
		private static string alienWarningText = "";
		private static float alienWarningTimer = 0f;

		// YoRHa Flight Unit Patrol
		private static float flightUnitProgress = -0.5f;
		private static float flightUnitTimer = 0f;

		// Easter Egg State
		private static string podDialogTitle = "";
		private static string podDialogText = "";
		private static float podDialogTimer = 0f;
		private static int easterEggClicks = 0;
		private static float emilFlyProgress = -1f;
		private static float emilFlyY = 200f;
		private static bool wasMouseDown = false;

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

			// Background twinkling stars
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i] = new Star
				{
					Position = new Vector2((float)rand.NextDouble() * 1920f, (float)rand.NextDouble() * 1080f),
					BaseAlpha = 0.25f + (float)rand.NextDouble() * 0.60f,
					TwinkleSpeed = 1.5f + (float)rand.NextDouble() * 3.5f,
					Phase = (float)rand.NextDouble() * MathHelper.TwoPi,
					Size = (rand.NextDouble() > 0.85) ? 2f : 1f
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
				if (bunkerTexture == null)
					bunkerTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Bunker", AssetRequestMode.ImmediateLoad);
				if (moonTexture == null)
					moonTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Moon", AssetRequestMode.ImmediateLoad);
				if (flightUnitTexture == null)
					flightUnitTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/FlightUnit", AssetRequestMode.ImmediateLoad);
				if (emilTexture == null)
					emilTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Emil", AssetRequestMode.ImmediateLoad);
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

			// Handle Mouse Easter Eggs & Interactions
			HandleInteractions(screenW, screenH);

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

			// 1. Deep Space Base Canvas (#08080c)
			sb.Draw(pixel, new Rectangle(0, 0, screenW, screenH), new Color(8, 8, 12));

			// 2. Twinkling Deep Space Stars
			DrawStars(sb, pixel, time, screenW, screenH);

			// 3. Distant Moving Moon (Humanity Server)
			DrawMovingMoon(sb, pixel, time, screenW, screenH);

			// 4. Moving Planet Earth in Low Orbit (Smooth orbital pan / drift)
			DrawMovingEarth(sb, pixel, time, screenW, screenH);

			// 5. Alien Dropships Landing on Earth (Fiery atmospheric re-entry streaks)
			UpdateAndDrawAlienDrops(sb, pixel, screenW, screenH);

			// 6. Tactical Gridlines (54px spacing) with micro-crosshairs
			DrawTacticalGrid(sb, pixel, screenW, screenH);

			// 7. The Bunker (YoRHa 13th Orbital Base) & Escort Flight Units
			DrawYoRHaBunker(sb, pixel, time, screenW, screenH);

			// 8. YoRHa Flight Unit Patrol (Periodic orbital pass)
			DrawFlightUnitPatrol(sb, pixel, screenW, screenH);

			// 9. Secret Easter Egg Flyby (Emil's Head)
			DrawEmilEasterEgg(sb, screenW, screenH);

			// 10. Floating Digital Atmospheric Dust / Embers drifting upward
			DrawEmbers(sb, pixel, time, screenW, screenH);

			// 11. Subtle CRT Scanlines & Radar Sweep
			DrawScanlinesAndRadar(sb, pixel, screenW, screenH);

			// 12. Interactive Pod 042 Tactical Log Pop-up
			DrawPodDialog(sb, pixel, screenW, screenH);

			// 13. Tactical YoRHa Military HUD Framing (clean corner alignment)
			DrawTacticalFrame(sb, pixel, screenW, screenH, time);

			// 14. NieR:Automata Stylized Title Card / Logo
			DrawNierTitleCard(sb, pixel, screenW, logoDrawCenter);
		}

		private static void DrawStars(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				float twinkle = (float)Math.Sin(time * stars[i].TwinkleSpeed + stars[i].Phase) * 0.4f + 0.6f;
				float alpha = stars[i].BaseAlpha * twinkle;
				int sx = (int)(stars[i].Position.X * (screenW / 1920f));
				int sy = (int)(stars[i].Position.Y * (screenH / 1080f));
				int sz = (int)stars[i].Size;

				sb.Draw(pixel, new Rectangle(sx, sy, sz, sz), new Color(220, 225, 240) * alpha);
			}
		}

		private static void DrawMovingMoon(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			if (moonTexture == null || !moonTexture.IsLoaded || moonTexture.Value == null)
				return;

			Texture2D moon = moonTexture.Value;
			var font = FontAssets.MouseText.Value;

			// Distant Moon orbits gently in the upper right space quadrant
			float moonBaseX = screenW * 0.80f;
			float moonBaseY = screenH * 0.22f;
			float moonDriftX = (float)Math.Sin(time * 0.15f) * 35f;
			float moonDriftY = (float)Math.Cos(time * 0.12f) * 14f;
			Vector2 moonPos = new Vector2(moonBaseX + moonDriftX, moonBaseY + moonDriftY);

			int moonSize = Math.Max(56, (int)(screenW * 0.052f));
			Rectangle moonRect = new Rectangle((int)(moonPos.X - moonSize / 2f), (int)(moonPos.Y - moonSize / 2f), moonSize, moonSize);

			// Subtle lunar atmospheric halo
			Color haloColor = new Color(130, 160, 200) * 0.12f;
			sb.Draw(pixel, new Rectangle(moonRect.X - 6, moonRect.Y - 6, moonRect.Width + 12, moonRect.Height + 12), haloColor);

			// Draw Moon Sphere
			sb.Draw(moon, moonRect, new Color(225, 235, 250) * 0.88f);

			// Micro tactical targeting telemetry beside Moon
			if (font != null)
			{
				float tagScale = 0.52f;
				string moonTag1 = "[ LUNA // HUMANITY SERVER ]";
				string moonTag2 = "DIST: 384,400 KM // LINK: 99.8%";
				Vector2 tagPos = new Vector2(moonRect.Right + 8, moonRect.Y + 4);

				Utils.DrawBorderString(sb, moonTag1, tagPos, new Color(190, 205, 225) * 0.70f, tagScale);
				Utils.DrawBorderString(sb, moonTag2, tagPos + new Vector2(0, 14), new Color(150, 170, 195) * 0.55f, tagScale * 0.90f);
			}
		}

		private static void DrawMovingEarth(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			if (earthTexture == null || !earthTexture.IsLoaded || earthTexture.Value == null)
				return;

			Texture2D earth = earthTexture.Value;

			// Smooth orbital drift and slight breathing to simulate floating above Earth
			float driftX = (float)Math.Sin(time * 0.08f) * 22f;
			float driftY = (float)Math.Cos(time * 0.06f) * 12f;
			float scalePulse = 1.04f + (float)Math.Sin(time * 0.04f) * 0.015f;

			int eW = (int)(screenW * scalePulse);
			int eH = (int)(screenH * scalePulse);
			int eX = (int)((screenW - eW) / 2f + driftX);
			int eY = (int)((screenH - eH) / 2f + driftY);

			Rectangle earthDest = new Rectangle(eX, eY, eW, eH);

			// Darker moodier tint for crisp tactical HUD contrast
			sb.Draw(earth, earthDest, new Color(200, 208, 220) * 0.84f);
		}

		private static void UpdateAndDrawAlienDrops(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			var font = FontAssets.MouseText.Value;
			var rand = Main.rand ?? new Terraria.Utilities.UnifiedRandom();

			// Spawn periodic alien dropships plunging into Earth
			alienSpawnTimer += 0.016f;
			if (alienSpawnTimer > 3.8f)
			{
				alienSpawnTimer = 0f;
				for (int i = 0; i < alienDrops.Length; i++)
				{
					if (!alienDrops[i].Active)
					{
						// Drop trajectory: starts high above Earth in orbit and plunges down-left or down-right into Earth
						float startX = (float)(screenW * (0.20f + rand.NextDouble() * 0.65f));
						float startY = (float)(screenH * (0.05f + rand.NextDouble() * 0.20f));
						float targetX = startX - 160f - (float)(rand.NextDouble() * 140f);
						float targetY = (float)(screenH * (0.60f + rand.NextDouble() * 0.35f));

						alienDrops[i] = new AlienDrop
						{
							Start = new Vector2(startX, startY),
							Current = new Vector2(startX, startY),
							Target = new Vector2(targetX, targetY),
							Progress = 0f,
							Speed = 0.006f + (float)rand.NextDouble() * 0.007f,
							TrailLength = 45f + (float)rand.NextDouble() * 35f,
							ImpactTimer = 0f,
							Active = true
						};

						// Trigger tactical alert
						alienWarningText = $"! [ ALERT: MACHINE LIFEFORM DESCENT DETECTED // SECTOR {rand.Next(1, 15):D2} ]";
						alienWarningTimer = 3.5f;
						break;
					}
				}
			}

			// Render and update each drop
			for (int i = 0; i < alienDrops.Length; i++)
			{
				if (!alienDrops[i].Active)
					continue;

				alienDrops[i].Progress += alienDrops[i].Speed;
				alienDrops[i].Current = Vector2.Lerp(alienDrops[i].Start, alienDrops[i].Target, alienDrops[i].Progress);

				Vector2 dir = alienDrops[i].Target - alienDrops[i].Start;
				if (dir.LengthSquared() > 0.001f)
					dir.Normalize();

				// Draw blazing plasma re-entry trail
				int segments = 16;
				float trailLen = alienDrops[i].TrailLength;
				for (int s = 0; s < segments; s++)
				{
					float f = s / (float)segments;
					Vector2 segPos = alienDrops[i].Current - dir * (f * trailLen);
					float segAlpha = (1f - f) * 0.90f;
					int segThick = Math.Max(1, (int)((1f - f) * 4f));

					// Re-entry fire gradient: White -> Gold/Orange -> Fiery Crimson
					Color trailColor = Color.Lerp(new Color(255, 230, 160), new Color(255, 60, 20), f) * segAlpha;
					sb.Draw(pixel, new Rectangle((int)segPos.X - segThick / 2, (int)segPos.Y - segThick / 2, segThick, segThick), trailColor);
				}

				// Glowing plasma head (alien landing pod)
				sb.Draw(pixel, new Rectangle((int)alienDrops[i].Current.X - 2, (int)alienDrops[i].Current.Y - 2, 5, 5), new Color(255, 245, 210));

				// Check atmospheric entry / impact completion
				if (alienDrops[i].Progress >= 1f)
				{
					alienDrops[i].ImpactTimer += 0.05f;
					// Expanding shockwave ring upon atmospheric entry
					float shockRadius = alienDrops[i].ImpactTimer * 38f;
					float shockAlpha = MathHelper.Clamp(1f - alienDrops[i].ImpactTimer, 0f, 1f);
					if (shockAlpha > 0f)
					{
						int pts = 12;
						for (int p = 0; p < pts; p++)
						{
							float ang = p * MathHelper.TwoPi / pts;
							Vector2 pt = alienDrops[i].Target + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * shockRadius;
							sb.Draw(pixel, new Rectangle((int)pt.X, (int)pt.Y, 2, 2), new Color(255, 120, 50) * shockAlpha);
						}
					}
					else
					{
						alienDrops[i].Active = false;
					}
				}
			}

			// Draw tactical alien descent warning ticker
			if (alienWarningTimer > 0f && font != null)
			{
				alienWarningTimer -= 0.016f;
				float flash = (float)Math.Sin(Main.timeForVisualEffects * 0.2f) * 0.3f + 0.7f;
				float warnScale = 0.62f;
				Vector2 warnSize = font.MeasureString(alienWarningText) * warnScale;
				Vector2 warnPos = new Vector2(screenW / 2f - warnSize.X / 2f, screenH - 58f);

				sb.Draw(pixel, new Rectangle((int)warnPos.X - 8, (int)warnPos.Y - 2, (int)warnSize.X + 16, (int)warnSize.Y + 4), new Color(40, 10, 10) * (0.85f * flash));
				Utils.DrawBorderString(sb, alienWarningText, warnPos, new Color(255, 80, 60) * flash, warnScale);
			}
		}

		private static void DrawYoRHaBunker(SpriteBatch sb, Texture2D pixel, float time, int screenW, int screenH)
		{
			if (bunkerTexture == null || !bunkerTexture.IsLoaded || bunkerTexture.Value == null)
				return;

			Texture2D bunker = bunkerTexture.Value;
			var font = FontAssets.MouseText.Value;

			// Station orbital coordinates: situated majestically in low Earth orbit
			float baseCenterX = screenW * 0.50f;
			float baseCenterY = screenH * 0.38f;

			// Station dynamics: gentle altitude bobbing & tiny attitude drift
			float bobY = (float)Math.Sin(time * 0.35f) * 6f;
			float swayX = (float)Math.Cos(time * 0.25f) * 4f;
			float rot = (float)Math.Sin(time * 0.15f) * 0.012f;

			Vector2 bunkerPos = new Vector2(baseCenterX + swayX, baseCenterY + bobY);

			// Scale the Bunker station to fit comfortably in orbit
			float baseW = screenW * 0.42f;
			float scale = baseW / bunker.Width;
			float drawW = bunker.Width * scale;
			float drawH = bunker.Height * scale;

			Rectangle bunkerDest = new Rectangle((int)(bunkerPos.X - drawW / 2f), (int)(bunkerPos.Y - drawH / 2f), (int)drawW, (int)drawH);

			// Check mouse hover over Bunker
			Point mouse = new Point(Main.mouseX, Main.mouseY);
			bool hovered = bunkerDest.Contains(mouse);

			// Draw station shadow / soft ambient backing
			sb.Draw(pixel, new Rectangle(bunkerDest.X + 6, bunkerDest.Y + 6, bunkerDest.Width - 12, bunkerDest.Height - 12), new Color(4, 4, 8) * 0.45f);

			// Draw Bunker Station
			Color stationTint = hovered ? new Color(255, 250, 240) : new Color(230, 235, 245) * 0.95f;
			sb.Draw(bunker, bunkerDest, null, stationTint, rot, new Vector2(bunker.Width / 2f, bunker.Height / 2f), SpriteEffects.None, 0f);

			// Blinking Navigation & Array LEDs (Green / Gold / Cyan beacons)
			DrawStationBeacon(sb, pixel, bunkerPos + new Vector2(-drawW * 0.22f, -drawH * 0.18f), Color.LimeGreen, time * 4.5f);
			DrawStationBeacon(sb, pixel, bunkerPos + new Vector2(drawW * 0.32f, -drawH * 0.12f), Color.Gold, time * 3.8f);
			DrawStationBeacon(sb, pixel, bunkerPos + new Vector2(-drawW * 0.38f, drawH * 0.05f), Color.Cyan, time * 5.0f);
			DrawStationBeacon(sb, pixel, bunkerPos + new Vector2(drawW * 0.28f, drawH * 0.25f), Color.LimeGreen, time * 4.0f);

			// Tactical Targeting Brackets around the Bunker
			Color bracketColor = hovered ? new Color(255, 220, 130) * 0.90f : new Color(210, 200, 175) * 0.45f;
			int bArm = 16;
			int bPad = 12;
			Rectangle targetBox = new Rectangle(bunkerDest.X - bPad, bunkerDest.Y - bPad, bunkerDest.Width + bPad * 2, bunkerDest.Height + bPad * 2);

			// Corner brackets ┌ ┐ └ ┘
			sb.Draw(pixel, new Rectangle(targetBox.X, targetBox.Y, bArm, 1), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.X, targetBox.Y, 1, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.Right - bArm, targetBox.Y, bArm, 1), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.Right - 1, targetBox.Y, 1, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.X, targetBox.Bottom - 1, bArm, 1), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.X, targetBox.Bottom - bArm, 1, bArm), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.Right - bArm, targetBox.Bottom - 1, bArm, 1), bracketColor);
			sb.Draw(pixel, new Rectangle(targetBox.Right - 1, targetBox.Bottom - bArm, 1, bArm), bracketColor);

			// Tactical HUD Station Label
			if (font != null)
			{
				float tagScale = 0.54f;
				string bunkerTag = MenuLyrics.CheckCjkSupport(font)
					? "[ 軌道衛星バンカー // YoRHa 13th BASE \"BUNKER\" ]"
					: "[ YoRHa 13th ORBITAL BASE // \"BUNKER\" ]";
				string statusTag = "ORBIT: 420 KM // STATUS: ALL SYSTEMS NOMINAL // [CLICK TO COMMS]";

				Vector2 bTagSize = font.MeasureString(bunkerTag) * tagScale;
				Vector2 sTagSize = font.MeasureString(statusTag) * (tagScale * 0.88f);

				Vector2 tagPos = new Vector2(targetBox.Center.X - bTagSize.X / 2f, targetBox.Bottom + 4);
				Utils.DrawBorderString(sb, bunkerTag, tagPos, hovered ? new Color(255, 235, 170) : new Color(230, 220, 195) * 0.85f, tagScale);
				Utils.DrawBorderString(sb, statusTag, new Vector2(targetBox.Center.X - sTagSize.X / 2f, tagPos.Y + 14), new Color(175, 170, 155) * 0.70f, tagScale * 0.88f);
			}
		}

		private static void DrawStationBeacon(SpriteBatch sb, Texture2D pixel, Vector2 pos, Color color, float pulseTime)
		{
			float pulse = (float)Math.Sin(pulseTime) * 0.5f + 0.5f;
			if (pulse > 0.4f)
			{
				sb.Draw(pixel, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 3, 3), color * pulse);
			}
		}

		private static void DrawFlightUnitPatrol(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			if (flightUnitTexture == null || !flightUnitTexture.IsLoaded || flightUnitTexture.Value == null)
				return;

			flightUnitTimer += 0.016f;
			if (flightUnitProgress < 0f && flightUnitTimer > 18f)
			{
				flightUnitTimer = 0f;
				flightUnitProgress = 0f;
			}

			if (flightUnitProgress >= 0f)
			{
				flightUnitProgress += 0.0022f;
				if (flightUnitProgress > 1.25f)
				{
					flightUnitProgress = -0.5f;
				}

				Texture2D ship = flightUnitTexture.Value;
				var font = FontAssets.MouseText.Value;

				float startX = -150f;
				float endX = screenW + 150f;
				float currentX = MathHelper.Lerp(startX, endX, flightUnitProgress);
				float currentY = screenH * 0.28f + (float)Math.Sin(flightUnitProgress * 12f) * 16f;

				int shipW = (int)(screenW * 0.16f);
				int shipH = (int)(shipW * (ship.Height / (float)ship.Width));
				Rectangle shipRect = new Rectangle((int)currentX, (int)currentY, shipW, shipH);

				// Draw glowing cyan ion thruster trail
				for (int t = 1; t <= 12; t++)
				{
					float f = t / 12f;
					Vector2 trailPos = new Vector2(shipRect.X - t * 7f, shipRect.Center.Y);
					int tSize = Math.Max(2, (int)((1f - f) * 10f));
					sb.Draw(pixel, new Rectangle((int)trailPos.X, (int)trailPos.Y - tSize / 2, tSize * 2, tSize), new Color(120, 230, 255) * ((1f - f) * 0.65f));
				}

				// Draw Flight Unit Ho229
				sb.Draw(ship, shipRect, new Color(245, 248, 255));

				// Tactical reticle tracking 2B
				if (font != null)
				{
					string pilotTag = "[ UNIT: Ho229 // PILOT: 2B // PATROL ROUTE ]";
					Utils.DrawBorderString(sb, pilotTag, new Vector2(shipRect.X, shipRect.Bottom + 4), new Color(180, 225, 255) * 0.75f, 0.50f);
				}
			}
		}

		private static void DrawEmilEasterEgg(SpriteBatch sb, int screenW, int screenH)
		{
			if (emilFlyProgress < 0f || emilTexture == null || !emilTexture.IsLoaded || emilTexture.Value == null)
				return;

			emilFlyProgress += 0.0035f;
			if (emilFlyProgress > 1.2f)
			{
				emilFlyProgress = -1f;
			}

			Texture2D emil = emilTexture.Value;
			var font = FontAssets.MouseText.Value;

			float startX = screenW + 100f;
			float endX = -120f;
			float currentX = MathHelper.Lerp(startX, endX, emilFlyProgress);
			float currentY = emilFlyY + (float)Math.Sin(emilFlyProgress * 14f) * 25f;
			float rot = -emilFlyProgress * 18f;

			int sz = 74;
			Rectangle emilRect = new Rectangle((int)currentX, (int)currentY, sz, sz);

			sb.Draw(emil, emilRect, null, Color.White, rot, new Vector2(emil.Width / 2f, emil.Height / 2f), SpriteEffects.None, 0f);

			if (font != null)
			{
				string emilSong = "♪ Every day's a sale! Every sale's a win! Buy stuff now or it'll be gone! ♪";
				Vector2 songSize = font.MeasureString(emilSong) * 0.60f;
				Utils.DrawBorderString(sb, emilSong, new Vector2(currentX - songSize.X / 2f, currentY + 46), new Color(255, 235, 140), 0.60f);
			}
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

		private static void HandleInteractions(int screenW, int screenH)
		{
			bool isMouseDown = Main.mouseLeft;
			bool clicked = isMouseDown && !wasMouseDown;
			wasMouseDown = isMouseDown;

			if (clicked)
			{
				Point mPos = new Point(Main.mouseX, Main.mouseY);

				// 1. Click Bunker Area -> Pod 042 Report
				Rectangle bunkerArea = new Rectangle((int)(screenW * 0.30f), (int)(screenH * 0.22f), (int)(screenW * 0.40f), (int)(screenH * 0.32f));
				if (bunkerArea.Contains(mPos))
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
					podDialogTitle = "[ POD 042 // TACTICAL TRANSMISSION ]";
					string[] reports = new string[]
					{
						"Report: Orbital Bunker operational. All YoRHa android units deployed to surface sectors.",
						"Report: Humanity's server communication frequency confirmed. Glory to Mankind.",
						"Proposal: Maintain orbital observation. Surface machine activity remains elevated.",
						"Report from Commander White: All personnel are required to perform routine diagnostic checks."
					};
					podDialogText = reports[Main.rand.Next(reports.Length)];
					podDialogTimer = 6.0f;
					return;
				}

				// 2. Click Moon Area -> Humanity Server Transmission
				Rectangle moonArea = new Rectangle((int)(screenW * 0.72f), (int)(screenH * 0.12f), (int)(screenW * 0.16f), (int)(screenH * 0.20f));
				if (moonArea.Contains(mPos))
				{
					SoundEngine.PlaySound(SoundID.MaxMana);
					podDialogTitle = "[ TRANSMISSION: COUNCIL OF HUMANITY ]";
					podDialogText = "To all YoRHa personnel: Humanity's prayers are with you on the front lines. Glory to Mankind.";
					podDialogTimer = 6.5f;
					return;
				}

				// 3. Secret Easter Egg: Click 4 times anywhere in empty space -> Emil flyby!
				easterEggClicks++;
				if (easterEggClicks >= 4)
				{
					easterEggClicks = 0;
					emilFlyProgress = 0f;
					emilFlyY = (float)(screenH * (0.15f + Main.rand.NextDouble() * 0.35f));
					SoundEngine.PlaySound(SoundID.Item29);
					podDialogTitle = "[ UNIDENTIFIED VEHICLE DETECTED ]";
					podDialogText = "Emil: 'Hey there! Check out my shop! Low prices guaranteed!'";
					podDialogTimer = 5.5f;
				}
			}
		}

		private static void DrawPodDialog(SpriteBatch sb, Texture2D pixel, int screenW, int screenH)
		{
			if (podDialogTimer <= 0f)
				return;

			podDialogTimer -= 0.016f;
			var font = FontAssets.MouseText.Value;
			if (font == null)
				return;

			float dialogAlpha = MathHelper.Clamp(podDialogTimer, 0f, 1f);
			float titleScale = 0.65f;
			float textScale = 0.58f;

			Vector2 tSize = font.MeasureString(podDialogTitle) * titleScale;
			Vector2 bSize = font.MeasureString(podDialogText) * textScale;

			float boxW = Math.Max(tSize.X, bSize.X) + 36f;
			float boxH = 58f;
			float boxX = screenW / 2f - boxW / 2f;
			float boxY = screenH * 0.58f;

			Rectangle boxRect = new Rectangle((int)boxX, (int)boxY, (int)boxW, (int)boxH);

			// Dark matte tactical backing
			sb.Draw(pixel, boxRect, new Color(12, 12, 16) * (0.92f * dialogAlpha));

			// Border & brackets
			Color brkColor = new Color(245, 235, 205) * dialogAlpha;
			sb.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, boxRect.Width, 1), brkColor * 0.4f);
			sb.Draw(pixel, new Rectangle(boxRect.X, boxRect.Bottom - 1, boxRect.Width, 1), brkColor * 0.4f);
			sb.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, 1, boxRect.Height), brkColor * 0.4f);
			sb.Draw(pixel, new Rectangle(boxRect.Right - 1, boxRect.Y, 1, boxRect.Height), brkColor * 0.4f);

			// Corner notches
			sb.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, 10, 2), brkColor);
			sb.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, 2, 10), brkColor);
			sb.Draw(pixel, new Rectangle(boxRect.Right - 10, boxRect.Y, 10, 2), brkColor);
			sb.Draw(pixel, new Rectangle(boxRect.Right - 2, boxRect.Y, 2, 10), brkColor);

			// Text
			Utils.DrawBorderString(sb, podDialogTitle, new Vector2(boxRect.X + 16, boxRect.Y + 8), new Color(255, 225, 140) * dialogAlpha, titleScale);
			Utils.DrawBorderString(sb, podDialogText, new Vector2(boxRect.X + 16, boxRect.Y + 28), new Color(230, 225, 215) * dialogAlpha, textScale);
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
