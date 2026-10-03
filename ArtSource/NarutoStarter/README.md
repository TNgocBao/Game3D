# Naruto Starter — bản tích hợp game

Nguồn: bộ OBJ người dùng cung cấp tại Downloads/Naruto_Unity_Starter/Naruto_Unity_Starter.

- FullCharacter_Static.obj và NarutoPalette.mtl: bản nguồn được giữ nguyên.
- build_starter.py: làm mượt mesh, thêm rig 11 xương và skin weights, xuất FBX cho Unity.
- NarutoStarter-Rigged.blend: bản Blender có thể chỉnh sửa.
- Assets/Art/NarutoStarter/NarutoStarter.fbx: FBX đang dùng để tạo prefab.
- Assets/Editor/LumiStarterNarutoBuild.cs: tạo mesh, vật liệu, prefab và điểm đặt camera.

Dựng lại: ArtSource/Tools/blender-env/Scripts/python.exe ArtSource/NarutoStarter/build_starter.py
Sau đó gọi LumiStarterNarutoBuild.Build trong Unity Editor.

Animation dùng hệ thống LumiInfantryMotion và LumiTechniquePoses của game. Bộ gốc không có animation clip hoặc humanoid rig; bản tích hợp sử dụng rig tùy chỉnh cho hệ thống hiện có.
