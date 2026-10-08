# Build APK — Naruto Adventure

Project dùng Unity **2021.3.13f1**. Scene build chính là Assets/3D.unity.

## 1. Cài Android Build Support

Máy hiện chưa có Android module cho Unity 2021.3.13f1.

1. Mở **Unity Hub** → **Installs**.
2. Ở Unity **2021.3.13f1**, bấm biểu tượng bánh răng → **Add modules**.
3. Chọn đủ Android Build Support, Android SDK & NDK Tools và OpenJDK.
4. Bấm Install. Không cần cài Android Studio để build bằng bộ công cụ đi kèm Unity.

Nếu Hub không cho thêm module cho bản cài hiện tại, cài thêm đúng Unity 2021.3.13f1 và chọn ba module trên trong lúc cài.

## 2. Chuẩn bị project

Mở project D:\Game3D\SuperGame, đợi Unity import và compile xong. Trên thanh menu chọn:

**Naruto → Android → 1 - Prepare APK settings**

Lệnh này đặt tên game Naruto Adventure, package com.ngocbao.narutoadventure, landscape trái/phải, Minimum API 22, Target API tự động, IL2CPP, ARMv7 + ARM64, dạng APK và scene Assets/3D.unity.

## 3. Xuất APK

Chọn **Naruto → Android → 2 - Build APK**.

Khi thành công, Unity mở thư mục chứa file:

D:\Game3D\SuperGame\Builds\Android\NarutoAdventure.apk

Lần build IL2CPP đầu có thể lâu vì Unity biên dịch native. Nếu Unity báo thiếu SDK/NDK/JDK, vào **Edit → Preferences → External Tools** và bật các đường dẫn installed with Unity.

## 4. Cài lên điện thoại

Cách đơn giản: chép APK vào điện thoại, mở file và cho phép cài ứng dụng không rõ nguồn gốc đối với ứng dụng quản lý file đang dùng.

Qua ADB:

    adb install -r "D:\Game3D\SuperGame\Builds\Android\NarutoAdventure.apk"

Tham số -r giữ dữ liệu khi cài đè. Nếu đổi chữ ký và Android từ chối cập nhật, gỡ bản cũ rồi cài lại; thao tác đó sẽ xóa dữ liệu lưu cục bộ của game.

## 5. Bản phát hành

APK debug mặc định phù hợp để thử trực tiếp. Trước khi phát hành:

1. Tạo keystore trong **Project Settings → Player → Android → Publishing Settings**.
2. Tăng Version và Bundle Version Code cho mỗi bản cập nhật.
3. Nếu đăng Google Play, bật Build App Bundle và xuất .aab; Google Play yêu cầu ARM64 và keystore phát hành.

## Điều khiển góc nhìn

- PC: bấm V để chuyển Thứ nhất → Thứ ba tự do → Theo hướng di chuyển.
- Mobile: bấm nút tròn **CAM** ở góc trên trái.
- Ở chế độ Theo hướng di chuyển, WASD hoặc joystick quyết định hướng chạy và hướng quay của nhân vật. Chuột hoặc vùng vuốt bên phải chỉ chọn hướng tấn công; camera tự bám sau lưng.
- Home đưa về góc nhìn thứ nhất.
