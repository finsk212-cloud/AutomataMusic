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
		public bool IsTitleCard;

		public LyricEntry(float start, float end, string japanese, string romaji, string english, bool isTitleCard = false)
		{
			Start = start;
			End = end;
			Japanese = japanese;
			Romaji = romaji;
			English = isTitleCard ? english : $"({english})";
			IsTitleCard = isTitleCard;
		}
	}

	public static class MenuLyrics
	{
		public static readonly LyricEntry[] Lyrics = new LyricEntry[]
		{
			// Intro Title Card (1.5s - 25.5s during guitar intro)
			new LyricEntry(1.5f, 25.5f,
				"Weight of the World / 壊レタ世界ノ歌",
				"Weight of the World / Kowareta Sekai no Uta",
				"— from NieR:Automata —",
				isTitleCard: true),

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

			// Instrumental Solo (227.5s - 256.5s / 3:48 - 4:16) - Guitar & orchestral solo, no lyrics

			// Bridge Chorus (Vocals enter at 4:16.8 / 256.8s)
			new LyricEntry(256.8f, 263.2f,
				"意味などないとしても、大声で叫び続ける",
				"Imi nado nai to shite mo, oogoe de sakebitsuzukeru",
				"'Cause we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(263.5f, 270.0f,
				"世界の重荷を背負っているかのように",
				"Sekai no omoni o seotte iru ka no you ni",
				"Like I'm carrying the weight of the world"),

			new LyricEntry(270.3f, 276.2f,
				"どうにかして、私たち全員を救えたらいいのに",
				"Dounika shite, watashitachi zen'in o sukuetara ii no ni",
				"I wish that someway, somehow that I could save every one of us"),

			new LyricEntry(276.5f, 282.2f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			// Climax (Choir & full orchestra)
			new LyricEntry(282.5f, 289.0f,
				"それでも、大声で叫び続ける",
				"Soredemo, oogoe de sakebitsuzukeru",
				"Still, we're gonna shout it loud, even if our words seem meaningless"),

			new LyricEntry(289.5f, 294.6f,
				"まるで世界の重荷を背負っているかのように",
				"Marude sekai no omoni o seotte iru ka no you ni",
				"It's like I'm carrying the weight of the world"),

			new LyricEntry(295.0f, 301.2f,
				"どうか私たち全員を救えますように",
				"Douka watashitachi zen'in o sukuemasu you ni",
				"I hope that someway, somehow that I could save every one of us"),

			new LyricEntry(301.8f, 306.8f,
				"でも本当は、私はただの一人の少女にすぎない",
				"Demo hontou wa, watashi wa tada no hitori no shoujo ni suginai",
				"But the truth is that I'm only one girl"),

			new LyricEntry(307.4f, 317.0f,
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
					float progress = elapsedSeconds - entry.Start;
					float remaining = entry.End - elapsedSeconds;

					// Smooth easing fade in / fade out (extended for title card, snappy for lyrics)
					float fadeInTime = entry.IsTitleCard ? 1.4f : 0.35f;
					float fadeOutTime = entry.IsTitleCard ? 1.8f : 0.35f;

					float inAlpha = MathHelper.Clamp(progress / fadeInTime, 0f, 1f);
					inAlpha = MathHelper.SmoothStep(0f, 1f, inAlpha);

					float outAlpha = MathHelper.Clamp(remaining / fadeOutTime, 0f, 1f);
					outAlpha = MathHelper.SmoothStep(0f, 1f, outAlpha);

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

			float topScale = lyric.IsTitleCard ? 0.90f : 0.88f;
			float bottomScale = lyric.IsTitleCard ? 0.72f : 0.78f;

			Vector2 topSize = font.MeasureString(topText) * topScale;
			Vector2 bottomSize = font.MeasureString(bottomText) * bottomScale;

			float maxWidth = Math.Max(topSize.X, bottomSize.X);
			float centerX = Main.screenWidth / 2f;

			// Position below the main buttons, near the bottom of the screen
			float posY = Main.screenHeight - 84f;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float bannerW = maxWidth + (lyric.IsTitleCard ? 92f : 78f);
			float bannerH = lyric.IsTitleCard ? 52f : 48f;
			Rectangle bannerRect = new Rectangle((int)(centerX - bannerW / 2f), (int)posY - 4, (int)bannerW, (int)bannerH);

			// 1. Soft subtle dark drop shadow
			Rectangle shadowRect = new Rectangle(bannerRect.X - 3, bannerRect.Y - 3, bannerRect.Width + 6, bannerRect.Height + 6);
			sb.Draw(pixel, shadowRect, Color.Black * (0.35f * lineAlpha));

			// 2. Solid translucent dark charcoal slate background
			sb.Draw(pixel, bannerRect, new Color(13, 13, 17) * (0.86f * lineAlpha));

			// 3. Fine NieR beige frame border
			Color borderColor = new Color(185, 175, 150) * (0.35f * lineAlpha);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, bannerRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - 1, bannerRect.Width, 1), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, 1, bannerRect.Height), borderColor);
			sb.Draw(pixel, new Rectangle(bannerRect.Right - 1, bannerRect.Y, 1, bannerRect.Height), borderColor);

			// 4. Authentic NieR HUD Corner Brackets (┌ ┐ └ ┘)
			Color bracketColor = new Color(235, 222, 190) * (0.85f * lineAlpha);
			int arm = 8;
			int thick = 2;

			// Top-Left ┌
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, arm, thick), bracketColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Y, thick, arm), bracketColor);

			// Top-Right ┐
			sb.Draw(pixel, new Rectangle(bannerRect.Right - arm, bannerRect.Y, arm, thick), bracketColor);
			sb.Draw(pixel, new Rectangle(bannerRect.Right - thick, bannerRect.Y, thick, arm), bracketColor);

			// Bottom-Left └
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - thick, arm, thick), bracketColor);
			sb.Draw(pixel, new Rectangle(bannerRect.X, bannerRect.Bottom - arm, thick, arm), bracketColor);

			// Bottom-Right ┘
			sb.Draw(pixel, new Rectangle(bannerRect.Right - arm, bannerRect.Bottom - thick, arm, thick), bracketColor);
			sb.Draw(pixel, new Rectangle(bannerRect.Right - thick, bannerRect.Bottom - arm, thick, arm), bracketColor);

			// 5. Decorative center edge notches
			int midY = bannerRect.Y + bannerRect.Height / 2;
			sb.Draw(pixel, new Rectangle(bannerRect.X - 1, midY - 2, 3, 5), bracketColor * 0.65f);
			sb.Draw(pixel, new Rectangle(bannerRect.Right - 2, midY - 2, 3, 5), bracketColor * 0.65f);

			// 6. Typography rendering
			Color topColor = lyric.IsTitleCard ? new Color(248, 238, 212) * lineAlpha : new Color(238, 226, 198) * lineAlpha;
			Color bottomColor = lyric.IsTitleCard ? new Color(185, 180, 165) * (lineAlpha * 0.90f) : new Color(192, 192, 198) * (lineAlpha * 0.90f);

			// Line 1: Japanese / Romaji or Song Title
			Vector2 topPos = new Vector2(centerX - topSize.X / 2f, lyric.IsTitleCard ? posY - 1f : posY);
			Utils.DrawBorderString(sb, topText, topPos, topColor, topScale);

			// Line 2: English translation or Game Subtitle
			Vector2 bottomPos = new Vector2(centerX - bottomSize.X / 2f, lyric.IsTitleCard ? posY + 23f : posY + 22f);
			Utils.DrawBorderString(sb, bottomText, bottomPos, bottomColor, bottomScale);
		}
	}
}
