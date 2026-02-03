# Endless Runner (3D – 3 Lane) | Unity


A 3D lane-based endless runner game developed in Unity.  
The player moves between three lanes (left, center, right) while avoiding obstacles, collecting coins, and surviving as long as possible. The project focuses on clean spawning logic, object pooling for performance, and modular gameplay architecture.

### Features
- 3-lane movement system (left / center / right)
- Endless procedural track spawning
- Obstacles and hazards
- Coin/collectible system
- Object pooling for optimized performance
- Camera follow system
- Basic UI and score logic

### Controls
- Move Left/Right: A/D or Arrow Keys
- Jump/Action: (if implemented)
- Pause: ESC

### Tech Stack
- Unity **6000.3.4f1**
- C#
- Object Pooling
- Modular game architecture

### How to Run
1. Open with Unity **6000.3.4f1**
2. Load: `Assets/Scenes/SampleScene.unity`
3. Press Play

### Project Structure
- `Assets/_Project/Scripts/Core` → Game manager
- `Player` → Player controller
- `Spawning` → Lane/obstacle/coin spawn system
- `Track` → Track generation
- `UI` → UI logic
- `Utils` → Camera & pooling helpers

---


Unity ile geliştirilmiş 3D ve 3 şeritli (sol–orta–sağ) endless runner oyunudur.  
Oyuncu engellerden kaçarken coin toplar ve mümkün olduğunca uzun süre hayatta kalmaya çalışır. Proje performans için object pooling ve modüler oyun mimarisi kullanmaktadır.

### Özellikler
- 3 şeritli hareket sistemi (sol / orta / sağ)
- Sonsuz track üretimi
- Engeller ve tuzaklar
- Coin/collectible sistemi
- Performans için object pooling
- Kamera takip sistemi
- Basit UI ve skor mantığı

### Kontroller
- Sağ/Sol: A/D veya Yön Tuşları
- Zıplama/Aksiyon: (varsa)
- Duraklat: ESC

### Teknolojiler
- Unity **6000.3.4f1**
- C#
- Object Pooling
- Modüler oyun yapısı

### Çalıştırma
1. Unity **6000.3.4f1** ile açın
2. `Assets/Scenes/SampleScene.unity` sahnesini yükleyin
3. Play’e basın
