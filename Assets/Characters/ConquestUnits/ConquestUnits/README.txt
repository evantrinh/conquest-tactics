CONQUEST UNITS  -  low poly medieval units (blue + red)
=======================================================
Units (each in _Blue and _Red):
  Footman     sword + round shield          ~680 tris   1.8 m
  Spearman    spear + kite shield           ~610 tris   1.9 m
  Archer      hood, bow, quiver             ~650 tris   1.8 m
  Mage        robe, hat, staff, orb         ~710 tris   2.1 m
  Juggernaut  spiked plate + warhammer      ~940 tris   2.5 m
  Cavalry     knight with lance on horse   ~1480 tris   horse+rider, ~3.5 m

Style matches Army_Knight_Blue.fbx: faceted low poly, T-pose, rigged, no animations,
ONE shared palette texture per team (Palette_Blue.png / Palette_Red.png).

UNITY SETUP
1. Copy the Models and Textures folders into Assets/ (keep them side by side).
2. Select a model > Inspector > Materials tab: either "Extract Materials" or
   use the embedded one. Set the shader to URP/Lit (or Standard) and assign
   Palette_Blue / Palette_Red as the Base Map. Set Smoothness to 0.
   (Palette texture import: Filter Mode = Point, Compression = None, to keep colors crisp.)
3. One material per team is enough: every unit of a team shares it.
4. Rig tab: the 5 humanoid units use standard humanoid bone names
   (Hips, Spine, Chest, Neck, Head, UpperArm.L, LowerArm.L, Hand.L, UpperLeg.L ...),
   so "Humanoid" works and you can use Mixamo / any humanoid animations.
   Cavalry has a custom skeleton (Horse_* and Rider_* bones): use "Generic".
5. Facing: units face -Y in Blender (same as the Knight); exported with Blender's
   default FBX axes just like your Army_Knight_Blue.fbx, so they behave the same way.

TEAM COLORS
Only 3 palette swatches differ between teams (team, team_dark, team_light), so any new
unit stays blue/red compatible if you keep using those swatches for tabards, shields etc.
