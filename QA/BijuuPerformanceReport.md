# Chỉ ba loại quái và tối ưu

LumiBeastArt chỉ còn prefab Matatabi (Sprout/cận chiến), Gyuki (Ranger/tầm xa), Kurama (Golem/quái mạnh). Đã bỏ toàn bộ bộ tạo quái primitive và LumiBeastMotion cũ. Các boss và quy tắc tăng độ khó/số quái theo map giữ nguyên. Tên đối tượng spawn trong Hierarchy dùng tên vĩ thú.

Tối ưu đã áp dụng:

- Matatabi 155 submesh → 1; Gyuki 232 submesh → 1. Mesh giữ nguyên vertices, normals, vertex colors, bone weights, bind poses và tất cả tam giác; chỉ gộp các phần dùng cùng vật liệu. Hai skin mới nằm ở Assets/Art/BijuuReference/Optimized và được prefab tham chiếu.
- Cache mảng xương thay vì đọc SkinnedMeshRenderer.bones trong từng vòng lặp; cache ma trận chuyển sang local cho mỗi lượt cập nhật.
- Hitbox chỉ cần các đỉnh convex hull của từng nhóm có cùng skin weights. Phép skinning của mỗi nhóm là affine, nên giữ đỉnh hull giữ các cực trị của mesh trong mọi pose. Matatabi: 14.289 → 3.447 mẫu; Gyuki: 37.502 → 22.055; Kurama: 3.620 → 1.659. Qhull không xử lý được nhóm suy biến thì giữ nguyên nhóm. Không giảm độ chi tiết mesh hiển thị. Dữ liệu trong Resources/LumiEnemies/*-HitHull.json; script QA/build_bijuu_hit_hulls.py.
- Hitbox nhận sát thương ở layer EnemyDamage (31), không tham gia tiếp xúc vật lý. CharacterController vẫn là thể tích di chuyển. Các truy vấn đạn, skill và chuột vẫn bao gồm hitbox thân/đuôi; truy vấn tránh vật cản và LOS AI bỏ layer này.
- Cập nhật hitbox giãn thời điểm giữa các quái (~10Hz gần người chơi, ~4Hz ngoài 40m), tránh mọi con cập nhật đồng loạt. Ngừng ép skin render khi ngoài camera.
- Ray/SphereCast cho AI dùng buffer tái sử dụng; nếu buffer đầy vẫn truy vấn đầy đủ để bảo toàn kết quả. Quái đứng yên và đã chạm đất không gọi lại CharacterController.Move chỉ để giữ trọng lực.
- Khử răng cưa 2x; bóng Medium, 2 cascades, khoảng cách 45m. Không đổi tạo hình, skeleton, clip hay chỉ số chiến đấu. HUD trạng thái không tạo list/buff array mỗi frame.

Kiểm tra Unity Play Mode:

- So sánh tất cả box hitbox với cách dùng toàn bộ vertices ở Idle/Run/Attack tại 0%, 33%, 67% thời lượng clip cho ba vĩ thú. Sai lệch giới hạn 0,025m; đạt.
- Sinh quái qua factory live ở map 1 và 5, không có ngoại hình khác.
- Matatabi/Gyuki tự di chuyển, phát Run, tự tấn công và gây sát thương; nhận sát thương đúng; đạn trace giữa thân và hai mép 3/3. Kurama load prefab và ba animation, hitbox được kiểm tra cùng bài so sánh.
- Runtime và Editor compile thành công. Validation.txt phải ghi PASS.

Số đo trong Editor, không phải benchmark bản build: lần đầu sau gộp render, map 5 với 37 quái khoảng 19,5 FPS / p95 175ms. Các lượt tối ưu sau đó với 38–45 quái khoảng 30–40 FPS, map 1 khoảng 57–59 FPS. Số quái và vị trí được random nên đây là các mẫu tham khảo, không phải A/B cố định. Map 5 đông quái vẫn có frame chậm; không tuyên bố đạt 60 FPS ổn định. Các số cuối nằm tại Logs/BijuuLiveQA/Performance.txt; tách chi phí cập nhật hitbox với FPS toàn scene. Không đo lúc chụp ảnh.

Bản thử BakeMesh gặp sai lệch transform/scale đã loại khỏi code. Công thức hitbox hiện dùng skin weights đã được đối chiếu; các hàm Reference chỉ để kiểm tra và đo, không được gọi trong gameplay.

Lượt cuối sau khi Unity compile toàn bộ: PASS; map 1 có 12 quái ~59,9 FPS, p95 21,47ms; map 5 có 35 quái ~40,6 FPS, p95 34,98ms. Số quái/scene random và đây là số đo Editor, chưa phải cam kết FPS bản build. Đã kiểm tra factory chỉ còn ba prefab và không còn BakeMesh thử nghiệm hay truy cập bones[b] qua getter của renderer trong vòng lặp.
