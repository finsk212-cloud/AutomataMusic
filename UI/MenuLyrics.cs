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

			new LyricEntry(53.0f, 58.5f,
				"教えて神よ、私を罰しているのですか？",
				"Oshiete kami yo, watashi o basshite iru no desu ka?",
				"Tell me, God, why? Are you punishing me?"),

			new LyricEntry(58.8f, 64.5f,
				"過去の過ちへの代償なのでしょうか",
				"Kako no ayamachi e no daishou na no deshou ka",
				"Is this the price I'm paying for my past mistakes?"),

			new LyricEntry(65.0f, 71.0f,
				"これは私の贖罪の歌",
				"Kore wa watashi no shokuzai no uta",
				"This is my redemption song"),

			new LyricEntry(71.5f, 80.5f,
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

			new LyricEntry(102.0f, 106.5f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(107.0f, 115.5f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(116.8f, 121.5f,
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

			new LyricEntry(160.2f, 165.5f,
				"教えて神よ、私を罰しているのですか？",
				"Oshiete kami yo, watashi o basshite iru no desu ka?",
				"Tell me, God, why? Are you punishing me?"),

			new LyricEntry(165.8f, 171.5f,
				"過去の過ちへの代償なのでしょうか",
				"Kako no ayamachi e no daishou na no deshou ka",
				"Is this the price I'm paying for my past mistakes?"),

			new LyricEntry(172.0f, 178.0f,
				"これは私の贖罪の歌",
				"Kore wa watashi no shokuzai no uta",
				"This is my redemption song"),

			new LyricEntry(178.5f, 187.5f,
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

			new LyricEntry(209.0f, 213.5f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(214.0f, 222.5f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(223.0f, 227.5f,
				"叶うのだろうか…",
				"Kanau no darou ka...",
				"Come to life..."),

			// Instrumental Solo (227.5s - 251.0s / 3:48 - 4:11) - Guitar & orchestral solo, no lyrics

			// Bridge (Vocals resume softly at 4:11 / 251.0s)
			new LyricEntry(251.0f, 259.0f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(260.0f, 266.5f,
				"叶うのだろうか…",
				"Kanau no darou ka...",
				"Come to life..."),

			// 267.0s - 270.5s: Clean musical silence / drop before explosion (no lyrics)

			new LyricEntry(270.5f, 277.5f,
				"それでも、大声で叫び続ける",
				"Soredemo, oogoe de sakebitsuzukeru",
				"Still, we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(278.0f, 282.0f,
				"まるで世界の重荷を背負っているかのように",
				"Marude sekai no omoni o seotte iru ka no you ni",
				"It's like I'm carrying the weight of the world"),

			// Climax (Choir & full orchestra)
			new LyricEntry(282.5f, 289.0f,
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

			new LyricEntry(309.5f, 317.0f,
				"信じ続ければ、いつか夢は叶うのだろうか",
				"Shinjitsuzukereba, itsuka yume wa kanau no darou ka",
				"Maybe if I keep believing, my dreams will come to life"),

			new LyricEntry(319.5f, 327.0f,
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

					// Smooth 0.35s fade in and 0.35s fade out
					float inAlpha = MathHelper.Clamp(progress / 0.35f, 0f, 1f);
					float outAlpha = MathHelper.Clamp((entry.End - elapsedSeconds) / 0.35f, 0f, 1f);
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

			// Clean seamless NieR HUD panel backing (no sliced seams/bars)
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float bannerW = maxWidth + 70f;
			float bannerH = 50f;
			Rectangle bannerRect = new Rectangle((int)(centerX - bannerW / 2f), (int)posY - 4, (int)bannerW, (int)bannerH);

			// Solid translucent slate background
			sb.Draw(pixel, bannerRect, new Color(12, 12, 16) * (0.82f * lineAlpha));

			// Fine NieR beige border
			Color borderColor = new Color(185, 175, 150) * (0.45f * lineAlpha);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 1, bannerRect.Height), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.Right - 1, bannerRect.Y, 1, bannerRect.Height), borderColor);

			// Line 1: Japanese / Romaji (Warm glowing NieR beige)
			Vector2 topPos = new Vector2(centerX - topSize.X / 2f, posY);
			Utils.DrawBorderString(sb, topText, topPos, new Color(235, 222, 192) * lineAlpha, topScale);

			// Line 2: English in parentheses (Soft silver)
			Vector2 bottomPos = new Vector2(centerX - bottomSize.X / 2f, posY + 22f);
			Utils.DrawBorderString(sb, bottomText, bottomPos, new Color(190, 190, 195) * (lineAlpha * 0.9f), bottomScale);
		}
	}
}
