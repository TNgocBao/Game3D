# Naruto-inspired modular 3D starter kit – Unity

Bộ mô hình **blockout 3D tự tạo bằng hình học procedural**, dựa trên ảnh tham chiếu. ĐÂY LÀ MESH 3D THẬT dạng OBJ, không phải ảnh cắt ghép, nhưng **chưa phải** mô hình sản xuất chất lượng như ảnh concept. Dành cho prototype / dùng Codex tiếp tục cải tiến.

## Các asset
- `Models/`: `Head`, `FaceDetails`, `Hair`, `Headband`, `Jacket`, `Hands`, `Pants`, `Sandals`, `Accessories`, `Kunai`, `Shuriken` và `FullCharacter_Static`.
- `Materials/NarutoPalette.mtl`: màu phân chia vật liệu; `Textures/ColorPalette_REFERENCE.png` là palette THAM KHẢO, **không phải texture UV**.
- `Editor/NarutoPrefabBuilder.cs`: tạo prefab gắn các OBJ tĩnh từ menu Tools.
- `Scripts/NarutoPreviewOrbit.cs`: camera orbit xem nhân vật (old Input Manager).

## Cài Unity
1. Tạo Unity 2022.3 LTS / Unity 6 project 3D. Sao chép cả thư mục `Assets/NarutoModular/` vào thư mục `Assets/` của dự án.
2. Trong Unity kiểm tra OBJ + material; có thể tự tạo material URP/Lit theo palette nếu dự án URP.
3. Menu `Tools > Naruto Modular > Build Static Preview Prefab`, sau đó kéo `Naruto_Modular_Static.prefab` vào Scene.
4. Nếu hướng mặt hoặc trục bị khác do import settings, chỉnh tại root của prefab thay vì từng OBJ. OBJ được sinh theo tọa độ Y-up, chiều cao khoảng 2.4 world units (tóc cao hơn cơ thể).
5. `FullCharacter_Static.obj` là bản liền để preview nhanh. KHÔNG chèn FullCharacter và các module cùng lúc (sẽ chồng mesh).

## Giới hạn minh bạch
- Không có humanoid rig, bones, skin weights, blendshapes, animations; không thể áp Animator Humanoid ngay.
- Chưa UV unwrap, PBR normal/roughness/AO, LOD, collision, trang phục thay thế. Một số chi tiết được giản lược.
- Các chi tiết facial/whisker, tóc, quần áo là mesh kiểu prototype, không phải bản sao chính xác concept.
- `Kunai.obj` và `Shuriken.obj` được tạo quanh gốc tọa độ để gắn vào transform tay sau này.

## Lộ trình để Codex hoàn thiện
Dùng Blender để retopology, UV unwrap, PBR textures; rig Armature và skin weights; export FBX (apply transforms, bone axis), kiểm tra Unity Rig Humanoid mapping; tạo idle/walk/run/jump/attack và Animator Controller; chuyển prefab static thành rigged. Không tuyên bố đã rig nếu chưa test.

## Bản quyền
Fan-made prototype mô phỏng nhân vật Naruto theo yêu cầu người dùng. Naruto là IP của chủ sở hữu tương ứng; kiểm tra quyền sử dụng trước khi phát hành/thương mại hóa.
