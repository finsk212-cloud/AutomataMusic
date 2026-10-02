======================================================================
                     AUTOMATA: MUSIC - TRACK GUIDE
======================================================================

Place your audio files (.ogg format recommended) directly into this folder:
C:\Users\Finsk\Documents\My Games\Terraria\tModLoader\ModSources\AutomataMusic\Assets\Music\

The mod supports BOTH NieR song titles AND simplified category names!
You only need to drop in whatever tracks you have. If a track isn't present,
Terraria will safely fall back to its vanilla music automatically without any errors.

----------------------------------------------------------------------
BOSS THEMES
----------------------------------------------------------------------
1. Early Bosses (Eye of Cthulhu, King Slime, Eater of Worlds, Brain of Cthulhu, Skeletron, Deerclops)
   - Accepted File Names:
     BirthOfAWish.ogg  OR  Boss1.ogg

2. Queen Bee
   - Accepted File Names:
     ABeautifulSong.ogg  OR  QueenBee.ogg

3. Wall of Flesh
   - Accepted File Names:
     GrandmaDestruction.ogg  OR  WallOfFlesh.ogg

4. Mechanical Bosses (The Destroyer, The Twins, Skeletron Prime)
   - Accepted File Names:
     DependentWeakling.ogg  OR  WarAndWar.ogg  OR  MechBoss.ogg

5. Plantera
   - Accepted File Names:
     AlienManifestation.ogg  OR  Plantera.ogg

6. Post-Plantera Bosses (Golem, Duke Fishron, Empress of Light, Lunatic Cultist)
   - Accepted File Names:
     PossessedByDisease.ogg  OR  LateBoss.ogg

7. Moon Lord
   - Accepted File Names:
     DarkColossusKaiju.ogg  OR  TheEndOfTheUnknown.ogg  OR  MoonLord.ogg

----------------------------------------------------------------------
BIOME / AMBIENT THEMES
----------------------------------------------------------------------
1. Forest Surface (Day) - Sunny Day
   - Recommended: City Ruins - Rays of Light
   - Accepted File Names:
     CityRuins.ogg  OR  RaysOfLight.ogg  OR  SurfaceDay.ogg

2. Forest Surface (Night) - Nighttime
   - Recommended: Voice of No Return
   - Accepted File Names:
     VoiceOfNoReturn.ogg  OR  SurfaceNight.ogg

3. Forest Surface (Town / Resistance Camp) - When near 2+ friendly NPCs
   - Recommended: Peaceful Sleep (Resistance Camp)
   - Accepted File Names:
     PeacefulSleep.ogg  OR  ResistanceCamp.ogg  OR  TownDay.ogg  OR  Town.ogg

4. Forest Surface (Rain) - Rain showers
   - Recommended: Vague Hope - Cold Rain
   - Accepted File Names:
     VagueHope.ogg  OR  VagueHopeColdRain.ogg  OR  ColdRain.ogg  OR  SurfaceRain.ogg  OR  Rain.ogg

5. Forest Surface (Windy Day) - High winds
   - Recommended: Forest Kingdom
   - Accepted File Names:
     ForestKingdom.ogg  OR  WindyDay.ogg

3. Underground / Caverns
   - Accepted File Names:
     AmusementPark.ogg  OR  CopiedCity.ogg  OR  Underground.ogg

4. Desert (Surface & Underground)
   - Accepted File Names:
     MemoriesOfDust.ogg  OR  Desert.ogg

5. Snow / Ice Biome
   - Accepted File Names:
     KaineSalvation.ogg  OR  PeacefulSleep.ogg  OR  Snow.ogg

6. Jungle (Surface & Underground)
   - Accepted File Names:
     ForestKingdom.ogg  OR  Pascal.ogg  OR  Jungle.ogg

7. Evil Biomes (Corruption, Crimson, Graveyard)
   - Accepted File Names:
     WretchedWeaponry.ogg  OR  EmilDespair.ogg  OR  EvilBiome.ogg

8. Underworld (Hell)
   - Accepted File Names:
     TheSoundOfTheEnd.ogg  OR  Underworld.ogg

9. Ocean / Beach
   - Accepted File Names:
     TreasuredTimes.ogg  OR  Ocean.ogg

======================================================================
HOW TO SET SEAMLESS LOOP POINTS IN AUDACITY (.ogg)
======================================================================
1. Open your audio file in Audacity (free audio editor).
2. At the very bottom of Audacity, change the time selection display
   from "hh:mm:ss" to "samples".
3. Find where you want the song to start looping after the intro (e.g. sample 661500).
   This is your LOOPSTART.
4. (Optional) Find the end of the loop (or the total length of the loop in samples).
   This is your LOOPLENGTH.
5. Go to Edit -> Metadata... (or File -> Export -> Export as OGG).
6. In the Metadata window, add two new rows / tags:
   - Tag Name: LOOPSTART       Value: <number of samples, e.g. 661500>
   - Tag Name: LOOPLENGTH      Value: <number of samples in the loop>
7. Export as Ogg Vorbis (.ogg) with Quality 7 or higher (44100 Hz or 48000 Hz).
8. Put the .ogg file in this folder and rebuild the mod in tModLoader!
======================================================================
