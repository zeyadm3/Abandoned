# Original ABANDONED assets

`environment.py` builds the modular architecture, depot props, shop fixtures, decay set and
procedural textures. `threats.py` builds eight original skinned silhouettes and their
Idle, Walk, Chase, Attack and Special animation takes.

Generated FBX/PNG assets are checked into `Game/Assets/_Project/Art/Custom/` so Unity does not
need Blender installed to import or build the game. Blender 4.3.2 was used for this pass and
is installed at `/Applications/Blender.app` on the development Mac.

Regenerate from the repository root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/environment.py
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/threats.py
Tools/unity.sh rebuild-horror
```

Close the Unity editor before batch regeneration. The horror regeneration command rebuilds
Mall and HQ, keeps TestMap untouched, and never runs tests, validation, icon captures or screenshots.
All source meshes and textures in this folder are original work for ABANDONED.
