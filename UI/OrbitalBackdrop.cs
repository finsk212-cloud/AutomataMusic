using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace AutomataMusic.UI
{
	/// <summary>
	/// Fully procedural "view from the Bunker" backdrop. No image assets are used:
	/// the planet surface, clouds, nebula and moon are generated with 3D Perlin noise,
	/// and the planet is re-shaded on the CPU every other frame so it slowly rotates.
	///
	/// Lore note: no city lights on the night side — Earth in NieR:Automata is a
	/// machine-overrun, overgrown ruin. The night side only shows faint moonlight and
	/// occasional lightning inside storm systems.
	/// </summary>
	public static class OrbitalBackdrop
	{
		// ───────────────────────── Tunables ─────────────────────────
		private const int MapW = 1024;               // equirectangular surface map (power of two)
		private const int MapH = 512;
		private const float HorizonTop = 0.62f;      // horizon height at screen centre (fraction of screen height)
		private const float RadiusFactor = 1.05f;    // planet radius as fraction of screen width
		private const float AtmThickness = 0.026f;   // atmosphere glow thickness, in planet radii
		private const float LandSpin = 2.4f;         // map columns per second (smooth, synced orbital rotation)
		private const float CloudSpin = 3.0f;        // clouds drift slightly faster than the ground
		private const int MoonSize = 160;
		private const int NebW = 480, NebH = 270;

		private static readonly Vector3 SunDir = Vector3.Normalize(new Vector3(-0.85f, -0.30f, -0.43f));
		private static readonly Vector3 Axis = Vector3.Normalize(new Vector3(0f, -0.55f, 0.84f));
		private static readonly Vector3 AxisX = Vector3.UnitX;
		private static readonly Vector3 AxisZ = Vector3.Cross(Vector3.UnitX, Vector3.Normalize(new Vector3(0f, -0.55f, 0.84f)));

		// ───────────────────────── Generated data ─────────────────────────
		private static volatile bool dataReady;
		private static bool dataStarted;
		private static bool texturesBuilt;
		private static float[] landR, landG, landB, cloudMap;
		private static Color[] nebulaData, moonData;

		private static Texture2D glowTex, gradientTex, nebulaTex, moonTex, planetTex;
		private static Asset<Texture2D> bunkerTexture;

		// ───────────────────────── Planet geometry cache ─────────────────────────
		private static int cachedW = -1, cachedH = -1;
		private static int bufW, bufH, bufDiv, bufTop;
		private static float gCx, gCy, gR;
		private static Color[] buffer;
		private static int planetCount;
		private static int[] pIndex, pRowOff;
		private static float[] pLon, pKr, pKg, pKb, pAr, pAg, pAb;
		private static readonly List<Vector2> nightSpots = new List<Vector2>();

		// ───────────────────────── Animation state ─────────────────────────
		private static readonly Stopwatch clock = Stopwatch.StartNew();
		private static double lastTime;
		private static float totalTime;
		private static float landRot, cloudRot;
		private static float planetFade;
		private static readonly Random fxRand = new Random(2027);

		private struct Star
		{
			public Vector2 Pos;      // normalised [0,1]
			public Vector2 Drift;    // normalised units / second
			public float BaseAlpha, TwinkleSpeed, Phase, Phase2;
			public int Size;
			public bool Spikes;
			public Color Col;
		}
		private static Star[] stars;

		private struct WarFlash
		{
			public bool Active;
			public Vector2 Pos;
			public float Life;
			public float MaxLife;
			public float BaseSize;
			public int Type; // 0 = Plasma Detonation, 1 = Crimson Machine Laser Strike, 2 = Warzone Embers, 3 = EMP Shock Discharge
			public Color PrimaryCol;
			public Color CoreCol;
			public float Angle;
		}

		private const int MaxWarFlashes = 24;
		private static readonly WarFlash[] warFlashes = new WarFlash[MaxWarFlashes];
		private static float warSpawnTimer = 1.6f;
		private static Vector2 lastWarPos;

		private static float shootTimer = 5f, shootLife, shootMax;
		private static Vector2 shootPos, shootVel;

		// ═════════════════════════════════════════════════════════════
		//  Public API
		// ═════════════════════════════════════════════════════════════

		public static void Draw(SpriteBatch sb, Texture2D pixel, int w, int h)
		{
			float dt = AdvanceClock();
			EnsureBasics();

			if (!dataStarted)
			{
				dataStarted = true;
				Task.Run(GenerateData);
			}
			if (dataReady && !texturesBuilt)
				BuildDataTextures();

			if (w != cachedW || h != cachedH)
				BuildGeometry(w, h);

			// 1. Deep-space gradient (slight airglow towards the horizon)
			sb.Draw(gradientTex, new Rectangle(0, 0, w, h), Color.White);

			// 2. Milky-way / nebula band
			if (nebulaTex != null)
				sb.Draw(nebulaTex, new Rectangle(0, 0, w, h), Color.White * 0.9f);

			// 3. Stars + shooting stars
			DrawStars(sb, pixel, w, h, dt);
			DrawShootingStar(sb, pixel, w, h, dt);

			// 4. Moon (the Human Council's server lives up there...)
			DrawMoon(sb, w, h);

			// 5. Planet
			if (texturesBuilt)
			{
				planetFade = Math.Min(1f, planetFade + dt * 0.6f);
				landRot += dt * LandSpin;
				cloudRot += dt * CloudSpin;
				if (landRot > MapW) landRot -= MapW;
				if (cloudRot > MapW) cloudRot -= MapW;

				UpdatePlanet();

				sb.Draw(planetTex, new Rectangle(0, bufTop, bufW * bufDiv, bufH * bufDiv), Color.White * planetFade);

				DrawWarDestruction(sb, h, dt);
			}

			// 6. Sunrise on the limb + lens flare
			DrawSun(sb, w, h);

			// 7. YoRHa Orbital Satellite Base // "The Bunker"
			DrawBunker(sb, w, h);
		}

		public static void Unload()
		{
			Texture2D[] all = { glowTex, gradientTex, nebulaTex, moonTex, planetTex };
			glowTex = gradientTex = nebulaTex = moonTex = planetTex = null;
			bunkerTexture = null;
			texturesBuilt = false;
			cachedW = cachedH = -1;
			Array.Clear(warFlashes, 0, warFlashes.Length);
			Main.QueueMainThreadAction(() =>
			{
				foreach (var t in all)
					t?.Dispose();
			});
		}

		// ═════════════════════════════════════════════════════════════
		//  Setup
		// ═════════════════════════════════════════════════════════════

		private static float AdvanceClock()
		{
			double now = clock.Elapsed.TotalSeconds;
			float dt = (float)Math.Min(0.1, Math.Max(0.0, now - lastTime));
			lastTime = now;
			totalTime += dt;
			return dt;
		}

		private static void EnsureBasics()
		{
			GraphicsDevice gd = Main.instance.GraphicsDevice;

			if (glowTex == null || glowTex.IsDisposed)
			{
				const int S = 128;
				var data = new Color[S * S];
				for (int y = 0; y < S; y++)
				{
					for (int x = 0; x < S; x++)
					{
						float dx = (x + 0.5f) / S * 2f - 1f;
						float dy = (y + 0.5f) / S * 2f - 1f;
						float d = (float)Math.Sqrt(dx * dx + dy * dy);
						float f = d >= 1f ? 0f : (float)Math.Exp(-d * d * 4.5f) * (1f - d);
						byte b = (byte)(MathHelper.Clamp(f, 0f, 1f) * 255f);
						data[y * S + x] = new Color(b, b, b, b);
					}
				}
				glowTex = new Texture2D(gd, S, S);
				glowTex.SetData(data);
			}

			if (gradientTex == null || gradientTex.IsDisposed)
			{
				const int H = 256;
				var data = new Color[H];
				Vector3 top = new Vector3(1f, 2f, 6f);
				Vector3 mid = new Vector3(5f, 11f, 22f);
				for (int y = 0; y < H; y++)
				{
					float t = y / (float)(H - 1);
					float k = SS(0f, HorizonTop, t);
					Vector3 c = Vector3.Lerp(top, mid, k * k);
					data[y] = new Color((int)c.X, (int)c.Y, (int)c.Z, 255);
				}
				gradientTex = new Texture2D(gd, 1, H);
				gradientTex.SetData(data);
			}

			if (stars == null)
				InitStars();
		}

		private static void InitStars()
		{
			var rand = new Random(11945);
			stars = new Star[360];
			Vector2 a = new Vector2(0f, 0.62f), b = new Vector2(1f, 0.02f);
			Vector2 dir = Vector2.Normalize(b - a);
			Vector2 perp = new Vector2(-dir.Y, dir.X);

			for (int i = 0; i < stars.Length; i++)
			{
				Vector2 pos;
				if (rand.NextDouble() < 0.38)
				{
					// Concentrate some stars along the galactic band
					float t = (float)rand.NextDouble();
					float g = Gaussian(rand) * 0.07f;
					pos = Vector2.Lerp(a, b, t) + perp * g;
				}
				else
				{
					pos = new Vector2((float)rand.NextDouble(), (float)rand.NextDouble() * 0.8f);
				}

				float l = (float)rand.NextDouble();
				int size = l < 0.70 ? 1 : (l < 0.95 ? 2 : 3);
				float baseAlpha = size switch
				{
					1 => 0.20f + (float)rand.NextDouble() * 0.35f,
					2 => 0.45f + (float)rand.NextDouble() * 0.35f,
					_ => 0.75f + (float)rand.NextDouble() * 0.25f
				};

				Color col = rand.Next(5) switch
				{
					0 => new Color(200, 222, 255),
					1 => new Color(255, 250, 240),
					2 => new Color(255, 236, 205),
					3 => new Color(255, 214, 180),
					_ => new Color(225, 232, 250)
				};

				// Ever-so-slight drift: roughly 0.2–0.6 px/sec at 1080p
				float speed = 0.00011f * size;
				stars[i] = new Star
				{
					Pos = pos,
					Drift = new Vector2(speed, -speed * 0.25f),
					BaseAlpha = baseAlpha,
					TwinkleSpeed = 0.25f + (float)rand.NextDouble() * 0.6f,
					Phase = (float)rand.NextDouble() * MathHelper.TwoPi,
					Phase2 = (float)rand.NextDouble() * MathHelper.TwoPi,
					Size = size,
					Spikes = size == 3 && rand.NextDouble() < 0.5,
					Col = col
				};
			}
		}

		private static void BuildDataTextures()
		{
			GraphicsDevice gd = Main.instance.GraphicsDevice;
			nebulaTex = new Texture2D(gd, NebW, NebH);
			nebulaTex.SetData(nebulaData);
			moonTex = new Texture2D(gd, MoonSize, MoonSize);
			moonTex.SetData(moonData);
			texturesBuilt = true;
		}

		// ═════════════════════════════════════════════════════════════
		//  Procedural generation (background thread)
		// ═════════════════════════════════════════════════════════════

		private static void GenerateData()
		{
			try
			{
				GenerateSurface();
				GenerateNebula();
				GenerateMoon();
				dataReady = true;
			}
			catch
			{
				// If generation fails for any reason, the scene just stays starfield-only.
			}
		}

		private static void GenerateSurface()
		{
			int n = MapW * MapH;
			var lr = new float[n];
			var lg = new float[n];
			var lb = new float[n];
			var cl = new float[n];

			Vector3 deep = new Vector3(0.015f, 0.055f, 0.11f);
			Vector3 shallow = new Vector3(0.04f, 0.14f, 0.21f);
			Vector3 shelf = new Vector3(0.07f, 0.22f, 0.27f);
			Vector3 dry = new Vector3(0.44f, 0.38f, 0.27f);
			Vector3 wet = new Vector3(0.13f, 0.20f, 0.11f);   // overgrown, reclaimed by nature
			Vector3 high = new Vector3(0.36f, 0.34f, 0.31f);
			Vector3 snow = new Vector3(0.86f, 0.88f, 0.90f);
			Vector3 ice = new Vector3(0.84f, 0.89f, 0.93f);

			for (int y = 0; y < MapH; y++)
			{
				double lat = (0.5 - (y + 0.5) / MapH) * Math.PI;
				float cLat = (float)Math.Cos(lat), sLat = (float)Math.Sin(lat);
				float absLatDeg = (float)(Math.Abs(lat) * 180.0 / Math.PI);

				for (int x = 0; x < MapW; x++)
				{
					double lon = (x + 0.5) / MapW * Math.PI * 2.0;
					float px = cLat * (float)Math.Cos(lon);
					float py = sLat;
					float pz = cLat * (float)Math.Sin(lon);

					// Continents (domain warped for organic coastlines)
					float warp = Fbm(px * 1.2f + 5.2f, py * 1.2f + 1.3f, pz * 1.2f - 2.7f, 3) * 0.7f;
					float hgt = Fbm(px * 1.6f + warp, py * 1.6f - warp, pz * 1.6f + warp, 6);
					float moist = Fbm(px * 2.4f + 11.1f, py * 2.4f, pz * 2.4f - 4.4f, 4);

					Vector3 col;
					if (hgt < 0.03f)
					{
						float t = SS(-0.30f, 0.03f, hgt);
						col = Vector3.Lerp(deep, shallow, t);
						col = Vector3.Lerp(col, shelf, SS(-0.015f, 0.03f, hgt) * 0.8f);
					}
					else
					{
						float e = MathHelper.Clamp((hgt - 0.03f) / 0.33f, 0f, 1f);
						col = Vector3.Lerp(dry, wet, SS(-0.12f, 0.14f, moist + absLatDeg * 0.002f));
						col = Vector3.Lerp(col, high, (float)Math.Pow(e, 1.4));
						col = Vector3.Lerp(col, snow, SS(0.72f, 0.95f, e));
						float detail = Fbm(px * 14f, py * 14f, pz * 14f, 3);
						col *= 0.88f + detail * 0.5f;
					}

					// Polar ice caps
					float iceEdge = 67f + Fbm(px * 4f + 3f, py * 4f, pz * 4f, 3) * 14f;
					col = Vector3.Lerp(col, ice, SS(iceEdge - 2f, iceEdge + 2f, absLatDeg));

					// Clouds: banded along latitude, swirled by a warp field
					float cw = Fbm(px * 2.0f + 31f, py * 2.0f, pz * 2.0f, 3);
					float c = Fbm(px * 2.6f + cw * 1.6f, py * 4.4f + cw, pz * 2.6f - cw * 1.6f, 6);
					float fine = Fbm(px * 11f, py * 13f, pz * 11f, 3);
					float cloud = SS(0.00f, 0.30f, c + fine * 0.25f);
					cloud *= 0.92f;

					int i = y * MapW + x;
					lr[i] = col.X; lg[i] = col.Y; lb[i] = col.Z;
					cl[i] = cloud;
				}
			}

			landR = lr; landG = lg; landB = lb; cloudMap = cl;
		}

		private static void GenerateNebula()
		{
			var data = new Color[NebW * NebH];
			float aspect = 16f / 9f;
			Vector2 a = new Vector2(0f, 0.62f), b = new Vector2(aspect, 0.02f);
			Vector2 dir = Vector2.Normalize(b - a);
			Vector2 perp = new Vector2(-dir.Y, dir.X);

			Vector3 teal = new Vector3(0.14f, 0.28f, 0.34f);
			Vector3 violet = new Vector3(0.28f, 0.19f, 0.34f);
			Vector3 core = new Vector3(0.44f, 0.38f, 0.30f);

			for (int y = 0; y < NebH; y++)
			{
				for (int x = 0; x < NebW; x++)
				{
					Vector2 p = new Vector2((x + 0.5f) / NebW * aspect, (y + 0.5f) / NebH);
					float d = Vector2.Dot(p - a, perp);

					float n1 = Fbm(p.X * 2.4f, p.Y * 2.4f, 3.3f, 5);
					float n2 = Fbm(p.X * 6.5f, p.Y * 6.5f, 9.1f, 4);

					float band = (float)Math.Exp(-Math.Pow((d + n1 * 0.06f) / 0.15f, 2));
					float cloud = MathHelper.Clamp(0.55f + n1 * 1.7f, 0f, 1.3f);
					float lane = (float)Math.Exp(-Math.Pow((d - 0.012f + n1 * 0.05f) / 0.035f, 2)) * SS(-0.12f, 0.18f, n2);
					float total = band * cloud * (1f - 0.8f * lane) + 0.05f * SS(0f, 0.5f, n1 + 0.2f);

					Vector3 col = Vector3.Lerp(teal, violet, SS(-0.25f, 0.30f, n2 * 0.8f + n1));
					col = Vector3.Lerp(col, core, (float)Math.Exp(-Math.Pow(d / 0.06f, 2)) * 0.6f);

					float alpha = MathHelper.Clamp(total * 0.42f, 0f, 0.55f);
					data[y * NebW + x] = new Color(col.X * alpha, col.Y * alpha, col.Z * alpha, alpha);
				}
			}
			nebulaData = data;
		}

		private static void GenerateMoon()
		{
			var data = new Color[MoonSize * MoonSize];
			var rand = new Random(4242);
			const int CraterCount = 46;
			var craters = new Vector3[CraterCount];
			for (int i = 0; i < CraterCount; i++)
			{
				float r = 0.04f + (float)Math.Pow(rand.NextDouble(), 2.2) * 0.22f;
				craters[i] = new Vector3((float)rand.NextDouble() * 2f - 1f, (float)rand.NextDouble() * 2f - 1f, r);
			}

			Vector3 L = Vector3.Normalize(new Vector3(-0.9f, -0.2f, 0.38f));
			float half = MoonSize / 2f;

			for (int y = 0; y < MoonSize; y++)
			{
				for (int x = 0; x < MoonSize; x++)
				{
					float dx = (x + 0.5f - half) / (half - 1f);
					float dy = (y + 0.5f - half) / (half - 1f);
					float r2 = dx * dx + dy * dy;
					float r = (float)Math.Sqrt(r2);
					float cover = MathHelper.Clamp((1f - r) * (half - 1f), 0f, 1f);
					if (cover <= 0f)
					{
						data[y * MoonSize + x] = Color.Transparent;
						continue;
					}

					float z = (float)Math.Sqrt(Math.Max(0f, 1f - r2));
					Vector3 nrm = new Vector3(dx, dy, z);

					float maria = SS(0.0f, 0.25f, Fbm(nrm.X * 1.8f + 7f, nrm.Y * 1.8f, nrm.Z * 1.8f, 4));
					float albedo = 0.72f - maria * 0.26f + Fbm(nrm.X * 9f, nrm.Y * 9f, nrm.Z * 9f, 3) * 0.10f;

					for (int c = 0; c < CraterCount; c++)
					{
						float cdx = dx - craters[c].X, cdy = dy - craters[c].Y;
						float cd = (float)Math.Sqrt(cdx * cdx + cdy * cdy) / craters[c].Z;
						if (cd < 1.3f)
						{
							if (cd < 1f) albedo -= 0.07f * (1f - cd * cd);
							albedo += 0.06f * (float)Math.Exp(-Math.Pow((cd - 1f) / 0.13f, 2));
						}
					}

					float shade = (float)Math.Pow(Math.Max(0f, Vector3.Dot(nrm, L)), 0.85) + 0.035f;
					Vector3 col = new Vector3(0.88f, 0.86f, 0.82f) * albedo * shade;
					float a = cover;
					data[y * MoonSize + x] = new Color(col.X * a, col.Y * a, col.Z * a, a);
				}
			}
			moonData = data;
		}

		// ═════════════════════════════════════════════════════════════
		//  Planet
		// ═════════════════════════════════════════════════════════════

		private static float HorizonY(float x)
		{
			float dx = x - gCx;
			return gCy - (float)Math.Sqrt(Math.Max(0f, gR * gR - dx * dx));
		}

		private static void BuildGeometry(int w, int h)
		{
			cachedW = w;
			cachedH = h;
			gR = RadiusFactor * w;
			gCx = w * 0.5f;
			gCy = HorizonTop * h + gR;

			float atmPx = AtmThickness * gR;
			bufDiv = Math.Max(2, (int)Math.Ceiling(w / 960f));
			bufTop = Math.Max(0, (int)(HorizonTop * h - atmPx * 3.6f));
			bufW = (w + bufDiv - 1) / bufDiv;
			bufH = Math.Max(1, (h - bufTop + bufDiv - 1) / bufDiv);

			int n = bufW * bufH;
			buffer = new Color[n];
			pIndex = new int[n]; pRowOff = new int[n]; pLon = new float[n];
			pKr = new float[n]; pKg = new float[n]; pKb = new float[n];
			pAr = new float[n]; pAg = new float[n]; pAb = new float[n];
			planetCount = 0;
			nightSpots.Clear();

			Vector3 dayAtm = new Vector3(0.28f, 0.52f, 0.92f);
			Vector3 duskAtm = new Vector3(1.0f, 0.50f, 0.22f);
			Vector3 rimCol = new Vector3(0.62f, 0.84f, 1.0f);
			Vector3 moonlight = new Vector3(0.045f, 0.06f, 0.085f);
			Vector3 duskTint = new Vector3(1.0f, 0.55f, 0.32f);

			for (int j = 0; j < bufH; j++)
			{
				float sy = bufTop + (j + 0.5f) * bufDiv;
				for (int i = 0; i < bufW; i++)
				{
					float sx = (i + 0.5f) * bufDiv;
					float dx = (sx - gCx) / gR;
					float dy = (sy - gCy) / gR;
					float r2 = dx * dx + dy * dy;
					float r = (float)Math.Sqrt(r2);
					int bi = j * bufW + i;

					if (r < 1f)
					{
						float z = (float)Math.Sqrt(Math.Max(0f, 1f - r2));
						Vector3 nrm = new Vector3(dx, dy, z);
						float ndl = Vector3.Dot(nrm, SunDir);

						float ly = Vector3.Dot(nrm, Axis);
						float lx = Vector3.Dot(nrm, AxisX);
						float lz = Vector3.Dot(nrm, AxisZ);
						double lat = Math.Asin(MathHelper.Clamp(ly, -1f, 1f));
						double lon = Math.Atan2(lz, lx);
						int row = Math.Clamp((int)((0.5 - lat / Math.PI) * MapH), 0, MapH - 1);
						float lonCol = (float)((lon / (Math.PI * 2.0) + 0.5) * MapW);

						float day = SS(-0.10f, 0.30f, ndl);
						float light = day * (0.30f + 1.05f * (float)Math.Pow(Math.Max(0f, ndl), 0.75));
						float term = (float)Math.Exp(-Math.Pow((ndl - 0.03f) / 0.10f, 2));
						Vector3 k = Vector3.Lerp(Vector3.One, duskTint, term * 0.85f) * light + moonlight * (1f - day);

						float haze = (float)Math.Pow(1f - z, 2.4);
						float scatter = SS(-0.30f, 0.45f, ndl);
						Vector3 atm = Vector3.Lerp(dayAtm, duskAtm, term * 0.9f) * scatter;
						k *= 1f - haze * 0.8f;
						Vector3 add = atm * haze * 0.85f + new Vector3(0.004f, 0.009f, 0.02f);

						float edgePx = (1f - r) * gR;
						float rim = (float)Math.Exp(-edgePx / 3.5f);
						add += rimCol * rim * (0.12f + 0.88f * scatter);

						int p = planetCount++;
						pIndex[p] = bi;
						pRowOff[p] = row * MapW;
						pLon[p] = lonCol;
						pKr[p] = k.X; pKg[p] = k.Y; pKb[p] = k.Z;
						pAr[p] = add.X; pAg[p] = add.Y; pAb[p] = add.Z;

						if (ndl < -0.12f && (i % 6 == 0) && (j % 6 == 0) && sy < h - 40)
							nightSpots.Add(new Vector2(sx, sy));
					}
					else
					{
						// Atmosphere glow outside the limb (static, premultiplied)
						float t = (r - 1f) / AtmThickness;
						Vector3 limbN = new Vector3(dx / r, dy / r, 0f);
						float ndl = Vector3.Dot(limbN, SunDir);
						float scatter = SS(-0.40f, 0.55f, ndl);
						float term = (float)Math.Exp(-Math.Pow((ndl - 0.05f) / 0.18f, 2));

						float alpha = (float)Math.Exp(-t * 1.7f) * (0.18f + 0.82f * scatter);
						Vector3 col = Vector3.Lerp(new Vector3(0.35f, 0.62f, 1.0f), new Vector3(0.75f, 0.90f, 1.0f), (float)Math.Exp(-t * 3f));
						col = Vector3.Lerp(col, new Vector3(1.0f, 0.62f, 0.35f), term * 0.55f);

						if (alpha < 0.004f)
						{
							buffer[bi] = Color.Transparent;
						}
						else
						{
							float a = MathHelper.Clamp(alpha, 0f, 1f);
							buffer[bi] = new Color(col.X * a, col.Y * a, col.Z * a, a * 0.55f);
						}
					}
				}
			}

			planetTex?.Dispose();
			planetTex = new Texture2D(Main.instance.GraphicsDevice, bufW, bufH, false, SurfaceFormat.Color);
			planetTex.SetData(buffer);
		}

		private static void UpdatePlanet()
		{
			int l0 = (int)Math.Floor(landRot);
			float lFrac = landRot - l0;
			int l1 = (l0 + 1) & (MapW - 1);

			int c0 = (int)Math.Floor(cloudRot);
			float cFrac = cloudRot - c0;
			int c1 = (c0 + 1) & (MapW - 1);

			const int mask = MapW - 1;
			float[] lr = landR, lg = landG, lb = landB, cm = cloudMap;
			const float cR = 0.93f, cG = 0.94f, cB = 0.96f;

			Parallel.For(0, planetCount, p =>
			{
				int row = pRowOff[p];
				int baseCol = (int)pLon[p];

				int li0 = row + ((baseCol + l0) & mask);
				int li1 = row + ((baseCol + l1) & mask);
				float landR_val = lr[li0] + (lr[li1] - lr[li0]) * lFrac;
				float landG_val = lg[li0] + (lg[li1] - lg[li0]) * lFrac;
				float landB_val = lb[li0] + (lb[li1] - lb[li0]) * lFrac;

				int ci0 = row + ((baseCol + c0) & mask);
				int ci1 = row + ((baseCol + c1) & mask);
				float cloud = cm[ci0] + (cm[ci1] - cm[ci0]) * cFrac;
				float inv = 1f - cloud;

				float sr = landR_val * inv + cR * cloud;
				float sg = landG_val * inv + cG * cloud;
				float sb = landB_val * inv + cB * cloud;

				int r = (int)((sr * pKr[p] + pAr[p]) * 255f);
				int g = (int)((sg * pKg[p] + pAg[p]) * 255f);
				int b = (int)((sb * pKb[p] + pAb[p]) * 255f);
				buffer[pIndex[p]] = new Color(r > 255 ? 255 : (r < 0 ? 0 : r),
				                              g > 255 ? 255 : (g < 0 ? 0 : g),
				                              b > 255 ? 255 : (b < 0 ? 0 : b), 255);
			});

			planetTex.SetData(buffer);
		}

		private static void SpawnWarFlash(int h, bool cluster = false)
		{
			if (nightSpots.Count == 0)
				return;

			int slot = -1;
			for (int i = 0; i < MaxWarFlashes; i++)
			{
				if (!warFlashes[i].Active)
				{
					slot = i;
					break;
				}
			}
			if (slot == -1)
				slot = fxRand.Next(MaxWarFlashes);

			Vector2 pos;
			if (cluster && lastWarPos != Vector2.Zero)
			{
				float offsetDist = (float)(fxRand.NextDouble() * h * 0.035f + h * 0.005f);
				float offsetAngle = (float)(fxRand.NextDouble() * MathHelper.TwoPi);
				pos = lastWarPos + new Vector2((float)Math.Cos(offsetAngle), (float)Math.Sin(offsetAngle)) * offsetDist;
			}
			else
			{
				pos = nightSpots[fxRand.Next(nightSpots.Count)];
			}
			lastWarPos = pos;

			double roll = fxRand.NextDouble();
			int type;
			float maxLife;
			float baseSize;
			Color primCol;
			Color coreCol;

			if (roll < 0.40)
			{
				// Type 0: Plasma Detonation / Heavy Bombardment
				type = 0;
				maxLife = 0.35f + (float)fxRand.NextDouble() * 0.25f;
				baseSize = h * (0.026f + (float)fxRand.NextDouble() * 0.025f);
				primCol = new Color(255, 135, 35, 0);
				coreCol = new Color(255, 235, 180, 0);
			}
			else if (roll < 0.70)
			{
				// Type 1: Crimson Machine Laser Strike
				type = 1;
				maxLife = 0.20f + (float)fxRand.NextDouble() * 0.20f;
				baseSize = h * (0.028f + (float)fxRand.NextDouble() * 0.025f);
				primCol = new Color(255, 30, 45, 0);
				coreCol = new Color(255, 190, 200, 0);
			}
			else if (roll < 0.90)
			{
				// Type 2: Burning Ruined Warzone Fires / Embers
				type = 2;
				maxLife = 0.80f + (float)fxRand.NextDouble() * 0.70f;
				baseSize = h * (0.016f + (float)fxRand.NextDouble() * 0.018f);
				primCol = new Color(255, 95, 20, 0);
				coreCol = new Color(255, 205, 80, 0);
			}
			else
			{
				// Type 3: High-Altitude EMP / Lightning Arc
				type = 3;
				maxLife = 0.22f + (float)fxRand.NextDouble() * 0.22f;
				baseSize = h * (0.028f + (float)fxRand.NextDouble() * 0.028f);
				primCol = new Color(110, 190, 255, 0);
				coreCol = new Color(235, 245, 255, 0);
			}

			warFlashes[slot] = new WarFlash
			{
				Active = true,
				Pos = pos,
				Life = maxLife,
				MaxLife = maxLife,
				BaseSize = baseSize,
				Type = type,
				PrimaryCol = primCol,
				CoreCol = coreCol,
				Angle = (float)(fxRand.NextDouble() * 0.5 - 0.25)
			};
		}

		private static void DrawWarDestruction(SpriteBatch sb, int h, float dt)
		{
			if (nightSpots.Count == 0)
				return;

			// Spawn war events across Earth's dark side and frontline (subtle, spaced-out cadence)
			warSpawnTimer -= dt;
			if (warSpawnTimer <= 0f)
			{
				warSpawnTimer = 1.4f + (float)fxRand.NextDouble() * 2.2f;
				SpawnWarFlash(h);

				// Rare chance for a rapid consecutive cluster strike nearby
				if (fxRand.NextDouble() < 0.20)
				{
					SpawnWarFlash(h, cluster: true);
				}
			}

			for (int i = 0; i < MaxWarFlashes; i++)
			{
				if (!warFlashes[i].Active)
					continue;

				ref WarFlash flash = ref warFlashes[i];
				flash.Life -= dt;
				if (flash.Life <= 0f)
				{
					flash.Active = false;
					continue;
				}

				float norm = 1f - (flash.Life / flash.MaxLife); // [0..1] progress

				switch (flash.Type)
				{
					case 0: // Expanding plasma detonation fireball
					{
						float expand = flash.BaseSize * (0.5f + 1.25f * norm);
						float intensity = (norm < 0.15f ? (norm / 0.15f) : (float)Math.Pow(1f - norm, 1.8)) * planetFade;
						DrawGlow(sb, flash.Pos, expand * 1.5f, expand * 1.2f, flash.PrimaryCol * (0.65f * intensity));
						DrawGlow(sb, flash.Pos, expand * 0.45f, expand * 0.35f, flash.CoreCol * (0.95f * intensity));
						break;
					}
					case 1: // Crimson Machine Laser Strike
					{
						float intensity = (float)Math.Sin(norm * Math.PI) * planetFade;

						// Downward laser strike trace from atmosphere
						if (norm < 0.65f)
						{
							float beamFade = (1f - norm / 0.65f) * planetFade;
							float beamLen = h * 0.055f;
							Vector2 beamCenter = flash.Pos - new Vector2((float)Math.Sin(flash.Angle), (float)Math.Cos(flash.Angle)) * (beamLen * 0.45f);
							DrawGlow(sb, beamCenter, 3.5f, beamLen, flash.PrimaryCol * (0.80f * beamFade), flash.Angle);
						}

						// Surface impact detonation
						DrawGlow(sb, flash.Pos, flash.BaseSize * 1.35f, flash.BaseSize * 1.35f, flash.PrimaryCol * (0.85f * intensity));
						DrawGlow(sb, flash.Pos, flash.BaseSize * 0.35f, flash.BaseSize * 0.35f, flash.CoreCol * (1.0f * intensity));
						break;
					}
					case 2: // Burning warzone fires / ruined city embers
					{
						float flicker = (float)(0.65 + 0.35 * Math.Sin(totalTime * 24.0 + flash.Pos.X)) * (1f - norm) * planetFade;
						DrawGlow(sb, flash.Pos, flash.BaseSize * 1.2f, flash.BaseSize * 0.95f, flash.PrimaryCol * (0.55f * flicker));
						DrawGlow(sb, flash.Pos, flash.BaseSize * 0.32f, flash.BaseSize * 0.25f, flash.CoreCol * (0.85f * flicker));
						break;
					}
					case 3: // EMP / Shock discharge
					{
						float jitter = (float)(0.50 + 0.50 * Math.Sin(norm * 44.0)) * (1f - norm) * planetFade;
						DrawGlow(sb, flash.Pos, flash.BaseSize * 1.6f, flash.BaseSize * 1.2f, flash.PrimaryCol * (0.75f * jitter));
						DrawGlow(sb, flash.Pos, flash.BaseSize * 0.38f, flash.BaseSize * 0.28f, flash.CoreCol * (0.95f * jitter));
						break;
					}
				}
			}
		}

		// ═════════════════════════════════════════════════════════════
		//  Sky elements
		// ═════════════════════════════════════════════════════════════

		private static void DrawStars(SpriteBatch sb, Texture2D pixel, int w, int h, float dt)
		{
			Rectangle src = new Rectangle(0, 0, 1, 1);
			for (int i = 0; i < stars.Length; i++)
			{
				ref Star s = ref stars[i];
				s.Pos += s.Drift * dt;
				if (s.Pos.X > 1f) s.Pos.X -= 1f;
				if (s.Pos.Y < 0f) s.Pos.Y += 0.8f;

				float sx = s.Pos.X * w;
				float sy = s.Pos.Y * h;
				if (cachedW > 0 && sy > HorizonY(sx) - AtmThickness * gR * 0.5f)
					continue;

				float w1 = (float)Math.Sin(totalTime * s.TwinkleSpeed + s.Phase);
				float w2 = (float)Math.Sin(totalTime * s.TwinkleSpeed * 1.618f + s.Phase2);
				float shimmer = (w1 * 0.65f + w2 * 0.35f) * 0.5f + 0.5f; // [0,1], smooth
				// Super subtle: brightness only breathes by about ±8%
				float alpha = MathHelper.Clamp(s.BaseAlpha * (0.92f + 0.16f * (shimmer - 0.5f)), 0f, 1f);

				int sz = s.Size;

				// Sub-pixel rendering: split brightness across the 4 neighbouring pixels so the
				// ultra-slow drift glides smoothly instead of snapping 1px at a time.
				int ix = (int)Math.Floor(sx), iy = (int)Math.Floor(sy);
				float fx = sx - ix, fy = sy - iy;
				Color c = s.Col * alpha;
				sb.Draw(pixel, new Rectangle(ix, iy, sz, sz), src, c * ((1f - fx) * (1f - fy)));
				sb.Draw(pixel, new Rectangle(ix + 1, iy, sz, sz), src, c * (fx * (1f - fy)));
				sb.Draw(pixel, new Rectangle(ix, iy + 1, sz, sz), src, c * ((1f - fx) * fy));
				sb.Draw(pixel, new Rectangle(ix + 1, iy + 1, sz, sz), src, c * (fx * fy));

				if (s.Size >= 2)
					sb.Draw(pixel, new Rectangle(ix - 1, iy - 1, sz + 2, sz + 2), src, s.Col * (alpha * 0.10f));

				if (s.Spikes)
				{
					float spike = alpha * 0.18f;
					const int len = 6;
					sb.Draw(pixel, new Rectangle(ix - len, iy + sz / 2, len * 2 + sz, 1), src, s.Col * spike);
					sb.Draw(pixel, new Rectangle(ix + sz / 2, iy - len, 1, len * 2 + sz), src, s.Col * spike);
				}
			}
		}

		private static void DrawShootingStar(SpriteBatch sb, Texture2D pixel, int w, int h, float dt)
		{
			if (shootLife > 0f)
			{
				shootLife -= dt;
				shootPos += shootVel * dt;
				float t = 1f - shootLife / shootMax;
				float fade = (float)Math.Sin(t * Math.PI);
				Vector2 dirN = Vector2.Normalize(shootVel);
				float angle = (float)Math.Atan2(dirN.Y, dirN.X);
				Rectangle src = new Rectangle(0, 0, 1, 1);
				const int Segs = 14;
				float segLen = 13f;
				for (int k = 0; k < Segs; k++)
				{
					Vector2 p = shootPos - dirN * (k * segLen);
					float a = fade * (1f - k / (float)Segs);
					sb.Draw(pixel, p, src, new Color(235, 240, 255, 0) * (a * 0.8f), angle, Vector2.Zero, new Vector2(segLen, k < 3 ? 2f : 1f), SpriteEffects.None, 0f);
				}
				DrawGlow(sb, shootPos, 6f, 6f, new Color(220, 235, 255, 0) * (fade * 0.6f));
				return;
			}

			shootTimer -= dt;
			if (shootTimer <= 0f)
			{
				shootTimer = 7f + (float)fxRand.NextDouble() * 10f;
				shootMax = shootLife = 0.55f + (float)fxRand.NextDouble() * 0.35f;
				shootPos = new Vector2(w * (0.30f + (float)fxRand.NextDouble() * 0.65f), h * (0.04f + (float)fxRand.NextDouble() * 0.30f));
				float ang = MathHelper.ToRadians(150f + (float)fxRand.NextDouble() * 25f);
				float speed = w * (0.45f + (float)fxRand.NextDouble() * 0.25f);
				shootVel = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * speed;
			}
		}

		private static void DrawMoon(SpriteBatch sb, int w, int h)
		{
			if (moonTex == null)
				return;

			float size = h * 0.080f;
			Vector2 c = new Vector2(w * 0.865f, h * 0.16f);
			float fade = Math.Min(1f, planetFade * 1.5f);
			DrawGlow(sb, c, size * 1.3f, size * 1.3f, new Color(170, 185, 210, 0) * (0.12f * fade));

			// Slow axial rotation of the Moon (ever so slightly)
			float moonRotation = totalTime * 0.010f;
			Vector2 origin = new Vector2(MoonSize * 0.5f, MoonSize * 0.5f);
			float scale = size / (float)MoonSize;
			sb.Draw(moonTex, c, null, Color.White * fade, moonRotation, origin, scale, SpriteEffects.None, 0f);
		}

		private static void EnsureBunkerTexture()
		{
			if (bunkerTexture == null)
			{
				try
				{
					bunkerTexture = ModContent.Request<Texture2D>("AutomataMusic/Assets/Textures/Bunker", AssetRequestMode.ImmediateLoad);
				}
				catch
				{
				}
			}
		}

		private static void DrawBunker(SpriteBatch sb, int w, int h)
		{
			EnsureBunkerTexture();
			if (bunkerTexture == null || !bunkerTexture.IsLoaded || bunkerTexture.Value == null)
				return;

			Texture2D bunker = bunkerTexture.Value;
			if (bunker.IsDisposed)
				return;

			// Microgravity orbital hover & sway (floating gracefully on the left side of orbit)
			float bobY = (float)Math.Sin(totalTime * 0.45f) * (h * 0.008f);
			float swayX = (float)Math.Cos(totalTime * 0.32f) * (w * 0.005f);
			Vector2 center = new Vector2(w * 0.23f + swayX, h * 0.38f + bobY);

			// Majestic axial rotation in orbit (smooth, synced with orbital drift)
			float rotation = totalTime * 0.038f;

			// Subtle 3D perspective breathing
			float scaleWobble = 1f + 0.015f * (float)Math.Sin(totalTime * 0.5f);
			float baseSize = h * 0.30f;
			float drawSize = baseSize * scaleWobble;
			float scale = drawSize / (float)bunker.Width;

			Vector2 origin = new Vector2(bunker.Width * 0.5f, bunker.Height * 0.5f);

			// 1. Soft atmospheric back-glow / Earth-shine directly behind the station
			float glowSize = drawSize * 0.90f;
			DrawGlow(sb, center, glowSize, glowSize, new Color(90, 150, 220, 0) * (0.14f * planetFade));

			// 2. Main YoRHa Bunker Station Body
			// All station lights (spire beacon, docking bays, hull lamps) are authentically integrated into the 3D model
			Color bunkerColor = new Color(245, 248, 255) * planetFade;
			sb.Draw(bunker, center, null, bunkerColor, rotation, origin, scale, SpriteEffects.None, 0f);
		}

		private static void DrawSun(SpriteBatch sb, int w, int h)
		{
			if (cachedW <= 0)
				return;

			float sxp = w * 0.10f;
			Vector2 sun = new Vector2(sxp, HorizonY(sxp) - h * 0.004f);
			float pulse = 1f + 0.035f * (float)Math.Sin(totalTime * 1.3f);
			float fade = 0.35f + 0.65f * planetFade;

			// Tangent of the horizon at the sun (so the atmospheric flare hugs the curve)
			Vector2 radial = sun - new Vector2(gCx, gCy);
			float tangentAngle = (float)Math.Atan2(radial.X, -radial.Y);

			DrawGlow(sb, sun, h * 0.60f * pulse, h * 0.60f * pulse, new Color(255, 150, 80, 0) * (0.14f * fade));
			DrawGlow(sb, sun, w * 0.55f, h * 0.045f, new Color(110, 165, 255, 0) * (0.30f * fade), tangentAngle);
			DrawGlow(sb, sun, w * 0.30f, h * 0.014f, new Color(255, 175, 95, 0) * (0.65f * fade), tangentAngle);
			DrawGlow(sb, sun, h * 0.17f * pulse, h * 0.17f * pulse, new Color(255, 205, 160, 0) * (0.42f * fade));
			DrawGlow(sb, sun, w * 0.75f, h * 0.0035f, new Color(170, 200, 255, 0) * (0.40f * fade));
			DrawGlow(sb, sun, h * 0.035f, h * 0.035f, new Color(255, 248, 235, 0) * fade);
			DrawGlow(sb, sun, h * 0.013f, h * 0.013f, new Color(255, 255, 255, 0) * fade);

			// Lens-flare ghosts along the sun → screen-centre axis
			Vector2 axis = new Vector2(w * 0.5f, h * 0.5f) - sun;
			float[] dist = { 0.45f, 0.80f, 1.25f, 1.60f, 1.95f };
			float[] rad = { 0.020f, 0.045f, 0.016f, 0.075f, 0.030f };
			Color[] cols =
			{
				new Color(255, 200, 140, 0), new Color(120, 210, 190, 0), new Color(200, 160, 255, 0),
				new Color(110, 160, 230, 0), new Color(255, 180, 120, 0)
			};
			for (int i = 0; i < dist.Length; i++)
			{
				float r = h * rad[i];
				DrawGlow(sb, sun + axis * dist[i], r, r, cols[i] * (0.07f * fade));
			}
		}

		private static void DrawGlow(SpriteBatch sb, Vector2 center, float rx, float ry, Color color, float rotation = 0f)
		{
			// Colour alpha of 0 with premultiplied blending = additive light.
			sb.Draw(glowTex, center, null, color, rotation, new Vector2(64f, 64f), new Vector2(rx / 64f, ry / 64f), SpriteEffects.None, 0f);
		}

		// ═════════════════════════════════════════════════════════════
		//  Math helpers / Perlin noise
		// ═════════════════════════════════════════════════════════════

		private static float SS(float e0, float e1, float x)
		{
			float t = MathHelper.Clamp((x - e0) / (e1 - e0), 0f, 1f);
			return t * t * (3f - 2f * t);
		}

		private static float Gaussian(Random r)
		{
			double u1 = 1.0 - r.NextDouble();
			double u2 = r.NextDouble();
			return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
		}

		private static readonly int[] perm = BuildPerm(1194503);

		private static int[] BuildPerm(int seed)
		{
			var r = new Random(seed);
			int[] p = new int[256];
			for (int i = 0; i < 256; i++) p[i] = i;
			for (int i = 255; i > 0; i--)
			{
				int j = r.Next(i + 1);
				(p[i], p[j]) = (p[j], p[i]);
			}
			int[] pp = new int[512];
			for (int i = 0; i < 512; i++) pp[i] = p[i & 255];
			return pp;
		}

		private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
		private static float Lerp(float t, float a, float b) => a + t * (b - a);

		private static float Grad(int hash, float x, float y, float z)
		{
			int h = hash & 15;
			float u = h < 8 ? x : y;
			float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
		}

		private static float Noise(float x, float y, float z)
		{
			int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y), zi = (int)Math.Floor(z);
			float xf = x - xi, yf = y - yi, zf = z - zi;
			xi &= 255; yi &= 255; zi &= 255;
			float u = Fade(xf), v = Fade(yf), w = Fade(zf);

			int a = perm[xi] + yi, aa = perm[a] + zi, ab = perm[a + 1] + zi;
			int b = perm[xi + 1] + yi, ba = perm[b] + zi, bb = perm[b + 1] + zi;

			return Lerp(w,
				Lerp(v,
					Lerp(u, Grad(perm[aa], xf, yf, zf), Grad(perm[ba], xf - 1, yf, zf)),
					Lerp(u, Grad(perm[ab], xf, yf - 1, zf), Grad(perm[bb], xf - 1, yf - 1, zf))),
				Lerp(v,
					Lerp(u, Grad(perm[aa + 1], xf, yf, zf - 1), Grad(perm[ba + 1], xf - 1, yf, zf - 1)),
					Lerp(u, Grad(perm[ab + 1], xf, yf - 1, zf - 1), Grad(perm[bb + 1], xf - 1, yf - 1, zf - 1))));
		}

		private static float Fbm(float x, float y, float z, int octaves)
		{
			float sum = 0f, amp = 0.5f, freq = 1f;
			for (int i = 0; i < octaves; i++)
			{
				sum += amp * Noise(x * freq, y * freq, z * freq);
				freq *= 2.03f;
				amp *= 0.5f;
			}
			return sum;
		}
	}
}
