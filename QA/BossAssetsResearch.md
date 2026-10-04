# Tài nguyên cho 5 boss Naruto — khảo sát 04/10/2026

## Đã sửa gameplay và kiểm tra
- Boss HP ×2: 168, 192, 216, 240, 264.
- Quái thường: hệ số HP/sát thương theo map 1–5 = 1.00, 1.15, 1.30, 1.45, 1.60; tốc độ, tầm phát hiện và nhịp AI còn tăng theo Intelligence hiện có.
- Boss tăng sát thương, tốc độ di chuyển và tần suất chiêu theo map.
- StartLevel chặn màn chưa mở. Chỉ thắng màn đã mở mới mở màn kế tiếp. Dữ liệu tiến trình cũ được giữ.
- Dưới 50% HP: đánh dấu giai đoạn 2 một lần, hiện tăng nhịp chiêu. Hồi máu không xóa giai đoạn 2. Chưa có chiêu thứ hai/animation mới được import.
- PASS: compile runtime/editor, 1044 kiểm tra rules và Play Mode kiểm tra 5 map, phase 2, phân thân/AI/tế đàn. Logs/CombatAIQA/Validation.txt.

## Model nhân vật: nguồn cụ thể và giới hạn

| Boss | Nguồn | Tình trạng |
|---|---|---|
| Hashirama | [CGTrader rigged](https://www.cgtrader.com/3d-models/character/fantasy-character/hashirama-senju-naruto-shippuden-938f1f28-ce2b-424e-89db-2e6ea0005a02) | Listing có rig, FBX/Blend, không có animation sẵn; cần kiểm tra mật độ mesh/avatar sau khi có file. |
| Hashirama dự phòng | [Sketchfab miễn phí](https://sketchfab.com/3d-models/hashirama-d7333b0f20394170a03bc3b0c4f2f0ec) | 2.1k triangles, tải được, không có clip; chưa xác nhận rig. |
| Gaara | [CGTrader rigged](https://www.cgtrader.com/3d-models/character/fantasy-character/gaara-3d-model) | Có FBX/Blend và rig theo mô tả; không xác nhận clip chiêu. |
| Gaara dự phòng | [Sketchfab miễn phí](https://sketchfab.com/3d-models/gaara-ddf17859e3d143a787d4b5346cce3aaf) | 18.7k triangles, tải được, không có animation; CC Attribution. |
| Ōnoki | Steam Workshop có bản port được nhắc trong [collection](https://steamcommunity.com/sharedfiles/filedetails/?id=3258761354) | Chưa xác minh bản FBX rigged có quyền tái sử dụng cho Unity. Không chọn như asset sẵn dùng. |
| Raikage | [Sketchfab viewer](https://sketchfab.com/3d-models/ay-raikage-naruto-541b1cce9975498e8615f092c2405397) | Không cho tải; ~1.15 triệu triangles, không có animation. Không phù hợp để import trực tiếp. |
| Mei | [MMD candidate](https://www.patreon.com/posts/mmd-barefoot-mei-124270219) | Có RAR theo listing nhưng chưa xem file/rig/quyền dùng; nhiều kết quả khác là STL tượng, không phù hợp cho animation game. Chưa chọn được asset sẵn dùng. |

Metadata model và giấy phép ghi trong BossAssetCandidates.json; thông tin rig trên listing chưa được xác nhận bằng file tải xuống. Asset fan-made không đồng nghĩa là gói Naruto chính thức.

## Animation và VFX
- [GAARA SAND STREAM JUTSU](https://sketchfab.com/3d-models/gaara-sand-stream-jutsu-61dfef60317b46f8b68a352a3affe906): API Sketchfab xác nhận tải được, 1 animation, 8514 triangles. CC Attribution, tác giả StrykerDoesAnimation. Tác giả ghi có glitch ở bình cát. Cần mở clip và kiểm tra rig/mesh; animation mesh không tự động là ParticleSystem Unity. Chưa import.
- [Mixamo](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html): animation nền miễn phí với Adobe ID; dùng để chạy, né, đánh, phản ứng. Kết ấn Naruto, tụ Trần Độn và động tác phun chiêu đặc trưng cần chỉnh/làm thêm.
- [Anime Stylized VFX — Vefects](https://assetstore.unity.com/packages/vfx/anime-stylized-vfx-216008): Built-in, có bảng tương thích 2020.3/2021.3/2023.1; giá đang hiển thị $39.97 trước thuế. Ứng viên nền phù hợp Unity 2021.3 Built-in, chưa test trên phiên bản 2021.3.13f1 của project.
- [Sand VFX — Vefects](https://assetstore.unity.com/packages/vfx/sand-vfx-265901): Built-in, $34.99, phiên bản Unity gốc 2023.1. Chưa xác nhận Unity 2021; chưa nên mua cho project hiện tại nếu chưa có bản tương thích.
- [Stylized AoE VFX](https://assetstore.unity.com/packages/vfx/stylized-aoe-vfx-303484): có Nature/Earth/Electric/Magma/Poison..., 32 prefab, Built-in, Unity gốc 2023.1. Có thể làm nền cho vùng kỹ năng nhưng cần xác nhận phiên bản.
- [Ninja Warrior Mecanim Animation Pack](https://assetstore.unity.com/packages/3d/animations/ninja-warrior-mecanim-animation-pack-35307): $24.99, hiện yêu cầu 2022.3.61; không lựa chọn mặc định cho Unity 2021.3.

## Đề xuất hai chiêu cho mỗi boss
Đây là kế hoạch gameplay, chưa phải mô tả nội dung một gói asset đã có.

| Boss | Chiêu thường | Chiêu mở dưới 50% | Phần cần dựng/chỉnh |
|---|---|---|---|
| Hashirama | Mộc long/rễ trồi lên | Mộc nhân đập đất diện rộng | Mesh gỗ có khớp, kết ấn, cú đập, bụi/rễ; pack nguyên tố không chứa mộc nhân hoàn chỉnh. |
| Gaara | Dòng cát/vùi cát | Đại sóng cát | Có ứng viên clip Sand Stream; cần mesh sóng cát, động tác điều khiển tay và particle cát đồng bộ. |
| Ōnoki | Khối Trần Độn | Trần Độn quét vùng rộng | Khối phát sáng/tia và shader có thể dựng; cần rig bay/tụ giữa hai tay. |
| Raikage | Giáp sét lao đánh | Lariat/chuỗi lao áp sát | Có thể retarget đánh/lao, thêm lightning trails, pose lấy đà và hồi thế. |
| Mei | Phun dung nham | Sương ăn mòn | Jet/pool/mist VFX, động tác lấy hơi/phun và vùng cảnh báo; cần model rigged phù hợp. |

Mỗi chiêu cần ba đoạn báo trước–ra đòn–hồi thế, VFX tại bàn tay/miệng và sự kiện damage khớp thời điểm. Không thay procedural rig bằng Animator chạy đồng thời: phải chọn một bên điều khiển xương để tránh giật/lệch khớp.

Chưa mua, tải hoặc import gói bên ngoài. Chưa tìm được một bộ đầy đủ, đồng bộ cả 5 model rigged + 10 animation chiêu + VFX Naruto.
