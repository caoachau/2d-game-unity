FALLEN SPIRES - SEPARATED ASSET PACK

Nguồn: asset atlas đã tạo từ màn hình gameplay tham chiếu.
Các ảnh đã được tách thành file PNG riêng và phần lớn có nền trong suốt.

Cấu trúc:
- Character: frame nhân vật theo action
- Environment: stone/cliff, moss/grass, platform, special
- Background: 4 lớp parallax
- Props: banner, lantern, statue, barrel, sign, vegetation...
- NPCEnemies: knight, mage, raven, bird
- Effects: slash, hit, dust, landing, fall, jump, dash, death
- UI: buttons, HUD, hearts, icons
- VFX: các frame hiệu ứng
- ReferencePanels: ảnh panel gốc để đối chiếu

Unity import gợi ý:
1. Texture Type = Sprite (2D and UI)
2. Sprite Mode = Single cho từng PNG riêng
3. Filter Mode = Point (no filter) nếu muốn giữ phong cách pixel
4. Compression = None
5. Mip Maps = Off
6. Character có thể dùng PPU 48; Environment có thể bắt đầu với PPU 32 rồi cân lại trong Scene.
7. Background để Sprite (2D and UI), không cần alpha, dùng Sorting Layer Background.

Lưu ý: atlas ban đầu là ảnh concept/generative, vì vậy đây là bộ asset tách từ atlas để prototype và authoring trong Unity; một số frame/tile có thể cần chỉnh pixel thủ công nếu dùng cho bản phát hành cuối.
