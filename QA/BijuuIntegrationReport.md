# Matatabi và Gyuki trong gameplay

Hai prefab đang dùng: Assets/Resources/LumiEnemies/Matatabi.prefab và Gyuki.prefab, lấy từ bản dựng mới theo ảnh ở Assets/Art/BijuuReference. Source Blender nằm tại ArtSource/BijuuReference. Không dùng GLB PROXY_ONLY trong Production Kit.

- Toàn bộ tier Sprout sử dụng Matatabi cận chiến; tier Ranger sử dụng Gyuki tầm xa. Toàn bộ tier Golem sử dụng Kurama; đã bỏ bộ sinh các vĩ thú dạng primitive khác.
- Idle/Run/Attack được điều khiển bởi LumiImportedBijuuMotion trong gameplay; AI LumiEnemy gọi động tác rồi sát thương ở thời điểm ra đòn. Gyuki tụ bom tại miệng và bắn theo hướng mục tiêu ba chiều.
- Matatabi tính tầm vồ từ bounds thân tới mục tiêu để tiếp xúc thực tế không bị giới hạn bởi khoảng cách gốc model. Tránh đánh tức thời rồi chạy animation riêng.
- Hitbox thân/đuôi cập nhật theo skin/bone; LOS và tránh vật cản bỏ qua collider của chính quái. AimPoint trỏ vào tâm hitbox thân.
- Né đòn, lui về tế đàn, hồi máu, lựa chọn phân thân, tăng chỉ số theo map dùng hệ hiện có. Khi AI chuyển sang né/lui, động tác đang tụ được hủy và orb được dọn.
- Kích thước body height 1:1 Kurama được giữ nguyên. Không dùng tỷ lệ 12m/18m trong hình kit.

Kiểm tra thực tế trong Unity Play Mode: quái sinh bằng factory live, AI di chuyển với Run, tự tấn công người chơi, Matatabi gây 1 sát thương và Gyuki gây 3 ở map 1; nhận 4 sát thương đúng, projectile trace giữa thân/hai mép 3/3. Runtime và Editor compile thành công. Chi tiết Logs/BijuuLiveQA/Validation.txt.

Dọn 9 mục không còn dùng khỏi project: model/material/source GLB cũ Assets/Art/Bijuu và meta, source Blender cũ ArtSource/Bijuu, Logs/BijuuQA, hai script rig/grounding cũ, cache Python và frame PNG đã encode của preview mới. Kiểm tra GUID không có tham chiếu từ asset còn sử dụng trước khi di chuyển. Giữ source, prefab, tài nguyên bản mới, video preview và bằng chứng QA.

Lệnh xóa hàng loạt bị công cụ chặn với lý do “blocked by policy”. Các mục này được lưu trữ ngoài project tại D:/Game3D/SuperGame_AssetArchive/Bijuu-Retired-2026-10-05, chưa xóa vĩnh viễn. Danh sách cụ thể Logs/BijuuLiveQA/Cleanup.json. Không sửa folder Downloads của người dùng.

