using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using ReLogic.Graphics;

namespace AutomataMusic.UI
{
	public struct LyricEntry
	{
		public float Start;
		public float End;
		public string Japanese;
		public string Romaji;
		public string English;

		public LyricEntry(float start, float end, string japanese, string romaji, string english)
		{
			Start = start;
			End = end;
			Japanese = japanese;
			Romaji = romaji;
			English = $"({english})";
		}
	}

	public static class MenuLyrics
	{
		public static readonly LyricEntry[] Lyrics = new LyricEntry[]
		{
			// Verse 1
			new LyricEntry(14.0f, 26.5f,
				"息絶えた世界に　一人佇む",
				"Ikitaeta sekai ni hitori tatazumu",
				"I feel like I'm losing hope in what I've been doing"),

			new LyricEntry(27.0f, 39.5f,
				"色を失くした大地　見つめて",
				"Iro o nakushita daichi mitsumete",
				"I feel like each day is gone without any meaning"),

			new LyricEntry(40.5f, 53.5f,
				"冷たい風が吹き抜けてゆく",
				"Tsumetai kaze ga fukinukete yuku",
				"A cold wind pulls out of the dark, and into the sun"),

			new LyricEntry(54.0f, 67.0f,
				"もう二度と戻らない　あの日の温もり",
				"Mou nido to modoranai ano hi no nukumori",
				"It tells me there's no going back from what I've become"),

			// Verse 2
			new LyricEntry(68.0f, 80.5f,
				"壊れた世界で　祈り捧げて",
				"Kowareta sekai de inori sasagete",
				"I feel like I'm losing hope in what I've been doing"),

			new LyricEntry(81.0f, 94.0f,
				"届かぬ願いに　涙こぼれる",
				"Todokanu negai ni namida koboreru",
				"I feel like each day is gone without any meaning"),

			new LyricEntry(94.5f, 107.0f,
				"叫び声さえ　闇に消えてゆく",
				"Sakebigoe sae yami ni kiete yuku",
				"I feel like I'm shouting, but no one can hear me"),

			new LyricEntry(107.5f, 120.0f,
				"救いを求めて　手を伸ばすのに",
				"Sukui o motomete te o nobasu no ni",
				"I feel like I'm falling, and no one can save me"),

			// Chorus 1
			new LyricEntry(121.0f, 134.0f,
				"叫び続ける　意味などなくても",
				"Sakebi tsuzukeru imi nado nakute mo",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(134.5f, 147.5f,
				"終焉の時を　一人で迎えるとしても",
				"Shuuen no toki o hitori de mukaeru to shite mo",
				"It's tough to face the end of the world, when you're all on your own"),

			new LyricEntry(148.0f, 160.5f,
				"叫び続ける　届かぬとしても",
				"Sakebi tsuzukeru todokanu to shite mo",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(161.0f, 176.5f,
				"変わらぬ世界に　抗い続けて",
				"Kawaranu sekai ni aragai tsuzukete",
				"It's tough to make a change to a world that doesn't care anymore"),

			// Interlude: 177 - 191

			// Verse 3
			new LyricEntry(191.5f, 204.0f,
				"息絶えた世界に　一人佇む",
				"Ikitaeta sekai ni hitori tatazumu",
				"I feel like I'm losing hope in what I've been doing"),

			new LyricEntry(204.5f, 217.0f,
				"色を失くした大地　見つめて",
				"Iro o nakushita daichi mitsumete",
				"I feel like each day is gone without any meaning"),

			new LyricEntry(217.5f, 230.0f,
				"叫び声さえ　闇に消えてゆく",
				"Sakebigoe sae yami ni kiete yuku",
				"I feel like I'm shouting, but no one can hear me"),

			new LyricEntry(230.5f, 243.0f,
				"救いを求めて　手を伸ばすのに",
				"Sukui o motomete te o nobasu no ni",
				"I feel like I'm falling, and no one can save me"),

			// Chorus 2
			new LyricEntry(244.0f, 257.0f,
				"叫び続ける　意味などなくても",
				"Sakebi tsuzukeru imi nado nakute mo",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(257.5f, 270.0f,
				"終焉の時を　一人で迎えるとしても",
				"Shuuen no toki o hitori de mukaeru to shite mo",
				"It's tough to face the end of the world, when you're all on your own"),

			new LyricEntry(271.0f, 283.5f,
				"叫び続ける　届かぬとしても",
				"Sakebi tsuzukeru todokanu to shite mo",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(284.0f, 301.0f,
				"変わらぬ世界に　抗い続けて",
				"Kawaranu sekai ni aragai tsuzukete",
				"It's tough to make a change to a world that doesn't care anymore"),

			// Final Chorus / Outro (Choir)
			new LyricEntry(302.0f, 335.0f,
				"共に生きる　希望の光を信じて",
				"Tomo ni ikiru kibou no hikari o shinjite",
				"May you live on... We are not alone.")
		};

		private static bool? isCjkSupported = null;

		public static bool CheckCjkSupport(DynamicSpriteFont font)
		{
			if (isCjkSupported.HasValue)
				return isCjkSupported.Value;

			// Test standard Japanese kanji
			isCjkSupported = font != null && font.IsCharacterSupported('壊') && font.IsCharacterSupported('世');
			return isCjkSupported.Value;
		}

		public static void DrawLyrics(SpriteBatch sb, float elapsedSeconds, float alpha)
		{
			if (alpha <= 0.001f)
				return;

			var font = FontAssets.MouseText.Value;
			if (font == null)
				return;

			// Find active lyric line
			LyricEntry? active = null;
			float lineAlpha = 0f;

			foreach (var entry in Lyrics)
			{
				if (elapsedSeconds >= entry.Start && elapsedSeconds <= entry.End)
				{
					active = entry;
					float duration = entry.End - entry.Start;
					float progress = elapsedSeconds - entry.Start;

					// Smooth 0.6s fade in and 0.6s fade out
					float inAlpha = MathHelper.Clamp(progress / 0.6f, 0f, 1f);
					float outAlpha = MathHelper.Clamp((entry.End - elapsedSeconds) / 0.6f, 0f, 1f);
					lineAlpha = Math.Min(inAlpha, outAlpha) * alpha;
					break;
				}
			}

			if (!active.HasValue || lineAlpha <= 0.001f)
				return;

			LyricEntry lyric = active.Value;

			// Choose Japanese or Romaji depending on whether CJK glyphs are in the font
			string topText = CheckCjkSupport(font) ? lyric.Japanese : lyric.Romaji;
			string bottomText = lyric.English;

			float topScale = 0.88f;
			float bottomScale = 0.76f;

			Vector2 topSize = font.MeasureString(topText) * topScale;
			Vector2 bottomSize = font.MeasureString(bottomText) * bottomScale;

			float maxWidth = Math.Max(topSize.X, bottomSize.X);
			float centerX = Main.screenWidth / 2f;

			// Position below the main buttons, near the bottom of the screen
			float posY = Main.screenHeight - 82f;

			// Draw subtle, soft horizontal vignette backing
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float bannerW = maxWidth + 80f;
			float bannerH = 50f;
			float bannerX = centerX - bannerW / 2f;

			// Soft dark background pill with smooth left/right gradient
			int slices = 20;
			float sliceW = bannerW / slices;
			for (int i = 0; i < slices; i++)
			{
				float norm = i / (float)slices; // 0 to 1
				// Center peak curve: sin(norm * PI)
				float curve = (float)Math.Sin(norm * Math.PI);
				float bgA = curve * 0.75f * lineAlpha;
				Rectangle r = new Rectangle((int)(bannerX + i * sliceW), (int)posY - 4, (int)(sliceW + 1f), (int)bannerH);
				sb.Draw(pixel, r, new Color(10, 10, 14) * bgA);
			}

			// Line 1: Japanese / Romaji (Warm glowing NieR beige)
			Vector2 topPos = new Vector2(centerX - topSize.X / 2f, posY);
			Utils.DrawBorderString(sb, topText, topPos, new Color(235, 222, 192) * lineAlpha, topScale);

			// Line 2: English in parentheses (Soft silver)
			Vector2 bottomPos = new Vector2(centerX - bottomSize.X / 2f, posY + 22f);
			Utils.DrawBorderString(sb, bottomText, bottomPos, new Color(190, 190, 195) * (lineAlpha * 0.9f), bottomScale);
		}
	}
}
