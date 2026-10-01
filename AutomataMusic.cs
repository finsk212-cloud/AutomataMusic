using AutomataMusic.Common;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataMusic : Mod
	{
		public static AutomataMusic Instance => ModContent.GetInstance<AutomataMusic>();

		public override void PostSetupContent()
		{
			MusicHelper.PreWarmAll(this);
		}

		public override void Unload()
		{
			MusicHelper.Unload();
		}
	}
}
