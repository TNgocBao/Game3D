# Naruto — Hành Trình Hokage

Unity 2021.3.13f1, Built-in Render Pipeline. Mở `Assets/3D.unity` và nhấn Play.

Điều khiển: WASD di chuyển, chuột nhìn/ngắm, Q/R/F dùng nhẫn thuật, V đổi góc nhìn, Tab thả hoặc khóa chuột.

- `Assets/Art/NarutoShippuden`: FBX rig gốc và bộ texture Naruto Shippuden.
- `Assets/Resources/NarutoChibi/Naruto.prefab`: nhân vật Naruto đang được game nạp.
- `Assets/Editor/LumiShippudenNarutoBuild.cs`: dựng lại prefab từ FBX qua menu Naruto trong Unity.
- `Assets/Scripts/Lumi`: gameplay, rig motion, camera, UI và kỹ năng.
- `Image`: hình mẫu Naruto và vĩ thú.
- `QA`: kiểm tra project và tạo tài nguyên.

Rig FBX có 342 xương (209 được mesh sử dụng). Nguồn không chứa animation clip; game điều khiển các xương chính bằng chuyển động và tư thế nhẫn thuật hiện có.

Cảnh quan dùng prefab tại `Assets/Resources/LumiEnvironment`: cây `tree_001`, nhà Styloo `house2/house3/house4/house6` và hai mẫu Medieval `House_01_full/House_04_full`. Model gốc được giữ trong `Assets/Art/Environment/*/Source`; OBJ là bản dẫn xuất để Unity import không cần plugin. Nhà/cây primitive cũ đã được gỡ. Shader `Lumi/Environment Tree Wind` tạo gió theo đợt, giữ collider cây cố định. Menu `Naruto/Prepare environment assets` dựng lại prefab; `Naruto/Validate environment in Play Mode` kiểm tra năm map và ghi ảnh/báo cáo vào `Logs/EnvironmentQA`.
