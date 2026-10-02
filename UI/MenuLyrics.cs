using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

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
			// Verse 1 (Guitar solo intro ends at 27.2s)
			new LyricEntry(27.2f, 33.8f,
				"心と体の　希望さえ失いかけている",
				"Kokoro to karada no kibou sae ushinaikakete iru",
				"I feel like I'm losing hope in my body and my soul"),

			new LyricEntry(34.0f, 39.5f,
				"見上げる空は　不吉な影を落とし",
				"Miageru sora wa fukitsuna kage o otoshi",
				"And the sky, it looks so ominous"),

			new LyricEntry(39.8f, 46.0f,
				"そして時は止まり　静寂が満ちてゆく",
				"Soshite toki wa tomari seijaku ga michite yuku",
				"And as time comes to a halt, silence starts to overflow"),

			new LyricEntry(46.2f, 53.2f,
				"私の叫びさえ　誰にも届かない",
				"Watashi no sakebi sae dare ni mo todokanai",
				"My cries are inconspicuous"),

			new LyricEntry(53.5f, 60.5f,
				"教えて神よ、私を罰しているのですか？",
				"Oshiete kami yo, watashi o basshite iru no desu ka?",
				"Tell me, God, are you punishing me?"),

			new LyricEntry(61.0f, 67.5f,
				"過去の過ちへの代償なのでしょうか",
				"Kako no ayamachi e no daishou na no deshou ka",
				"Is this the price I'm paying for my past mistakes?"),

			new LyricEntry(68.0f, 74.5f,
				"これは私の贖罪の歌",
				"Kore wa watashi no shokuzai no uta",
				"This is my redemption song"),

			new LyricEntry(74.8f, 80.5f,
				"今、あなたが必要… 私の声が聞こえますか？",
				"Ima, anata ga hitsuyou... Watashi no koe ga kikoemasu ka?",
				"I need you more than ever right now / Can you hear me now?"),

			// Chorus 1
			new LyricEntry(81.0f, 87.5f,
				"意味などないとしても、大声で叫び続ける",
				"Imi nado nai to shite mo, oogoe de sakebitsuzukeru",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(88.0f, 94.5f,
				"まるで世界の重荷を背負っているかのように",
				"Marude sekai no omoni o seotte iru ka no you ni",
				"It's like I'm carrying the weight of the world"),

			new LyricEntry(95.0f, 101.5f,
				"どうにかして、私たち全員を救えたらいいのに",
				"Dounika shite, watashitachi zen'in o sukuetara ii no ni",
				"I wish that someway, somehow that I could save every one of us"),

			new LyricEntry(102.0f, 108.0f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(108.5f, 116.0f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(116.5f, 122.0f,
				"叶うのだろうか…",
				"Kanau no darou ka...",
				"Come to life..."),

			// Verse 2
			new LyricEntry(134.0f, 139.8f,
				"笑い声が消え去り、生命の痕跡は洗い流され",
				"Waraigoe ga kiesari, inochi no konseki wa arainagasare",
				"After all the laughter fades, signs of life all washed away"),

			new LyricEntry(140.2f, 146.5f,
				"それでもまだ、穏やかな風を感じることができる",
				"Soredemo mada, odayakana kaze o kanjiru koto ga dekiru",
				"I can still, still feel a gentle breeze"),

			new LyricEntry(146.8f, 153.2f,
				"どんなに熱心に祈ろうとも、警告の兆しは消えず",
				"Donna ni nesshin ni inorou tomo, keikoku no kizashi wa kiezu",
				"No matter how hard I pray, signs of warning still remain"),

			new LyricEntry(153.5f, 160.5f,
				"命そのものが私の敵となってしまった",
				"Inochi sono mono ga watashi no teki to natte shimatta",
				"And life has become my enemy"),

			new LyricEntry(160.8f, 167.5f,
				"教えて神よ、私を罰しているのですか？",
				"Oshiete kami yo, watashi o basshite iru no desu ka?",
				"Tell me, God, are you punishing me?"),

			new LyricEntry(167.8f, 174.0f,
				"過去の過ちへの代償なのでしょうか",
				"Kako no ayamachi e no daishou na no deshou ka",
				"Is this the price I'm paying for my past mistakes?"),

			new LyricEntry(174.2f, 180.5f,
				"これは私の贖罪の歌",
				"Kore wa watashi no shokuzai no uta",
				"This is my redemption song"),

			new LyricEntry(180.8f, 187.8f,
				"今、あなたが必要… 私の声が聞こえますか？",
				"Ima, anata ga hitsuyou... Watashi no koe ga kikoemasu ka?",
				"I need you more than ever right now / Can you hear me now?"),

			// Chorus 2
			new LyricEntry(188.0f, 194.5f,
				"意味などないとしても、大声で叫び続ける",
				"Imi nado nai to shite mo, oogoe de sakebitsuzukeru",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(195.0f, 201.5f,
				"まるで世界の重荷を背負っているかのように",
				"Marude sekai no omoni o seotte iru ka no you ni",
				"It's like I'm carrying the weight of the world"),

			new LyricEntry(202.0f, 208.5f,
				"どうにかして、私たち全員を救えたらいいのに",
				"Dounika shite, watashitachi zen'in o sukuetara ii no ni",
				"I wish that someway, somehow that I could save every one of us"),

			new LyricEntry(209.0f, 215.0f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			// Post-Chorus 2 / Bridge
			new LyricEntry(215.5f, 222.0f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(222.5f, 227.5f,
				"叶うのだろうか…",
				"Kanau no darou ka...",
				"Come to life..."),

			new LyricEntry(256.5f, 264.0f,
				"どうにかして、私たち全員を救えたらいいのに",
				"Dounika shite, watashitachi zen'in o sukuetara ii no ni",
				"I wish that someway, somehow that I could save every one of us"),

			new LyricEntry(264.5f, 272.0f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(272.5f, 281.5f,
				"それでも、大声で叫び続ける",
				"Soredemo, oogoe de sakebitsuzukeru",
				"Still, we're gonna shout it loud, even if our words seem meaningless"),

			// Climax Chorus
			new LyricEntry(282.0f, 289.0f,
				"意味などないとしても、大声で叫び続ける",
				"Imi nado nai to shite mo, oogoe de sakebitsuzukeru",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(289.5f, 296.0f,
				"世界の重荷を背負っているかのように",
				"Sekai no omoni o seotte iru ka no you ni",
				"Like I'm carrying the weight of the world"),

			new LyricEntry(296.5f, 303.0f,
				"どうか私たち全員を救えますように",
				"Douka watashitachi zen'in o sukuemasu you ni",
				"I hope that someway, somehow that I could save every one of us"),

			new LyricEntry(303.5f, 309.0f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(309.5f, 316.0f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(316.5f, 325.0f,
				"命よ、蘇れ…",
				"Inochi yo, yomigaere...",
				"Come to life...")
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

					// Smooth 0.4s fade in and 0.4s fade out
					float inAlpha = MathHelper.Clamp(progress / 0.4f, 0f, 1f);
					float outAlpha = MathHelper.Clamp((entry.End - elapsedSeconds) / 0.4f, 0f, 1f);
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
			float bottomScale = 0.78f;

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
