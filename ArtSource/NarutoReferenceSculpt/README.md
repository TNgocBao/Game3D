# Naruto reference sculpt

Reference: Image/Naruto/Chibi-Turnaround-User.png supplied by the user.

Editable source: Naruto-Reference-Sculpt.blend.
Rebuild: ../Tools/blender-env/Scripts/python.exe build_reference_sculpt.py
Export: Naruto-Reference-Sculpt.fbx, with 11 deformation bones.
Studio previews: Naruto-Blender-{Front,Side,Back,ThreeQuarter}.png.

This is a locally modeled approximation and still needs visual refinement against the exact reference. Meshy generation is pending user login and authorization to upload the reference; no image has been uploaded and no paid credits have been used.

Unity importer: Assets/Editor/LumiBlenderNarutoBuild.cs. It rebuilds identity rotation pivots from the imported skeleton and preserves material submeshes for LumiInfantryMotion. The importer creates Eye camera socket from the head bone.

First-person camera uses the eye socket and hides all renderers under the local player, including dynamically added attachments. Third-person restores their shadow modes. Mouse yaw covers 360 degrees, pitch spans -89.5 to +89.5 degrees, and movement uses yaw independently of pitch.
