using System;
using AutomataMusic.Common;
using AutomataMusic.UI;
using Terraria;
using Terraria.ModLoader;

namespace AutomataMusic
{
	public class AutomataMusic : Mod
	{
		public static AutomataMusic Instance => ModContent.GetInstance<AutomataMusic>();

		public override void PostSetupContent()
		{
			MusicHelper.PreWarmAll(this);

			if (AutomataMusicConfig.Instance == null || AutomataMusicConfig.Instance.SetAsDefaultMenuTheme)
			{
				ActivateMenuTheme();
			}
		}

		public static void ActivateMenuTheme()
		{
			try
			{
				var menu = ModContent.GetInstance<AutomataModMenu>();
				if (menu == null)
					return;

				var menuLoaderType = typeof(MenuLoader);
				var switchToMenuField = menuLoaderType.GetField("switchToMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				var lastSelectedField = menuLoaderType.GetField("LastSelectedModMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

				switchToMenuField?.SetValue(null, menu);
				lastSelectedField?.SetValue(null, menu.FullName);
			}
			catch
			{
			}
		}

		public override void Load()
		{
			NierMenuButtons.Load();
		}

		public override void Unload()
		{
			NierMenuButtons.Unload();
			MusicHelper.Unload();
		}
	}
}
