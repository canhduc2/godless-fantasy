import os
import math
import random
from PIL import Image, ImageDraw

RESOURCES_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources"
os.makedirs(RESOURCES_DIR, exist_ok=True)

def new_img(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))

# ==============================================================================
# 1. TILES: HỆ ĐỒNG CỎ TỰ NHIÊN (SEAMLESS RPG GRASS TILES - 32x32)
# ==============================================================================

# Palette cỏ tự nhiên mượt mà (Secret of Mana / Zelda style)
C_GRASS_BASE = (92, 184, 52, 255)      # Xanh cỏ tươi
C_GRASS_DARK = (72, 156, 40, 255)      # Đổ bóng cỏ
C_GRASS_DEEP = (56, 128, 32, 255)      # Bóng sâu kẽ lá
C_GRASS_LIGHT = (118, 206, 68, 255)    # Ánh sáng mặt trời
C_GRASS_HIGH = (148, 226, 88, 255)     # Điểm nhấn ngọn cỏ

def create_base_grass(seed=42):
    """Cỏ nền liền mạch, không bao giờ lộ viền khối vuông"""
    random.seed(seed)
    img = new_img(32, 32)
    pix = img.load()
    
    for y in range(32):
        for x in range(32):
            # Tạo độ gồ ghề mềm mại tự nhiên
            noise = (math.sin(x * 0.4) + math.cos(y * 0.4) + math.sin((x+y)*0.3)) * 0.5
            if noise > 0.4:
                pix[x, y] = C_GRASS_LIGHT
            elif noise < -0.4:
                pix[x, y] = C_GRASS_DARK
            else:
                pix[x, y] = C_GRASS_BASE
                
    # Rải các ngọn cỏ nhỏ (grass blades) tự nhiên
    for _ in range(36):
        gx = random.randint(1, 30)
        gy = random.randint(2, 30)
        pix[gx, gy] = C_GRASS_DARK
        pix[gx, gy - 1] = C_GRASS_HIGH
        if random.random() > 0.5 and gx + 1 < 32:
            pix[gx + 1, gy - 1] = C_GRASS_LIGHT
            
    return img

def create_flower_grass(flower_type="yellow"):
    """Cỏ mọc hoa dại thành khóm đẹp mắt chuẩn mực game fantasy RPG"""
    img = create_base_grass(seed=hash(flower_type) % 1000)
    draw = ImageDraw.Draw(img)
    pix = img.load()
    
    if flower_type == "yellow":
        c_petals = (255, 230, 60, 255)
        c_petals_hi = (255, 250, 160, 255)
        c_center = (240, 140, 20, 255)
        centers = [(8, 8), (22, 10), (14, 20), (24, 24)]
    elif flower_type == "red":
        c_petals = (235, 55, 55, 255)
        c_petals_hi = (255, 110, 100, 255)
        c_center = (255, 220, 50, 255)
        centers = [(10, 12), (20, 8), (12, 22), (22, 22)]
    elif flower_type == "blue":
        c_petals = (60, 140, 245, 255)
        c_petals_hi = (130, 200, 255, 255)
        c_center = (255, 240, 120, 255)
        centers = [(8, 14), (22, 12), (15, 8), (18, 24)]
    elif flower_type == "white":
        c_petals = (245, 245, 248, 255)
        c_petals_hi = (255, 255, 255, 255)
        c_center = (255, 210, 40, 255)
        centers = [(11, 9), (21, 14), (8, 21), (22, 23)]
    else: # Clover / rau má
        c_petals = (40, 120, 30, 255)
        c_petals_hi = (90, 195, 50, 255)
        c_center = (60, 150, 35, 255)
        centers = [(7, 8), (16, 12), (24, 9), (10, 22), (22, 21)]

    # Vẽ từng bông hoa 5 cánh hoặc cụm cỏ
    for cx, cy in centers:
        # Cuống & lá nhỏ
        pix[cx, cy + 2] = C_GRASS_DEEP
        pix[cx - 1, cy + 2] = (50, 130, 30, 255)
        pix[cx + 1, cy + 2] = (50, 130, 30, 255)
        
        # Cánh hoa
        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (1, -1)]:
            if 0 <= cx + dx < 32 and 0 <= cy + dy < 32:
                pix[cx + dx, cy + dy] = c_petals
        # Điểm sáng cánh
        if 0 <= cx < 32 and 0 <= cy - 1 < 32:
            pix[cx, cy - 1] = c_petals_hi
        # Nhụy hoa
        pix[cx, cy] = c_center

    return img

def create_dense_clover_grass():
    """Khóm cỏ ba lá dày đặc (Denser Clover)"""
    img = create_base_grass(seed=99)
    pix = img.load()
    c_leaf = (45, 135, 35, 255)
    c_leaf_hi = (85, 190, 55, 255)
    c_stem = (30, 95, 25, 255)
    
    clovers = [(6, 10), (14, 6), (23, 11), (9, 20), (18, 18), (25, 24)]
    for cx, cy in clovers:
        pix[cx, cy] = c_stem
        pix[cx, cy + 1] = c_stem
        # 3 hoặc 4 cánh lá hình trái tim
        for dx, dy in [(-1, 0), (1, 0), (0, -1), (-1, -1), (1, -1)]:
            pix[cx + dx, cy + dy] = c_leaf
        pix[cx, cy - 1] = c_leaf_hi
        pix[cx - 1, cy] = c_leaf_hi
    return img

# ==============================================================================
# 2. TILES: ĐÁ HOA CƯƠNG LÁT QUẢNG TRƯỜNG & ĐỀN THỜ (32x32)
# ==============================================================================

def create_plaza_cobble():
    """Đá lát quảng trường phong cách hoàng gia sáng ấm, vân ghép đá tự nhiên"""
    img = new_img(32, 32)
    pix = img.load()
    c_base = (205, 202, 195, 255)      # Xám sáng thanh lịch
    c_light = (225, 223, 218, 255)    # Ánh sáng
    c_dark = (165, 160, 155, 255)     # Khối đá
    c_mortar = (115, 110, 105, 255)   # Mạch vữa giữa các viên đá
    
    for y in range(32):
        for x in range(32):
            # Mạch vữa gạch zíc zắc 8x8
            is_h = (y % 8 == 0)
            row = y // 8
            is_v = ((x + (row % 2) * 4) % 8 == 0)
            
            if is_h or is_v:
                pix[x, y] = c_mortar
            else:
                lx = (x + (row % 2) * 4) % 8
                ly = y % 8
                if ly == 1 or lx == 1:
                    pix[x, y] = c_light
                elif ly == 7 or lx == 7:
                    pix[x, y] = c_dark
                else:
                    noise = (x * 3 + y * 7) % 5
                    pix[x, y] = c_light if noise == 0 else (c_dark if noise == 1 else c_base)
    return img

def create_safezone_gold():
    """Viền đá vàng phong ấn bảo hộ thần thánh của làng"""
    img = create_plaza_cobble()
    pix = img.load()
    c_gold = (245, 200, 45, 255)
    c_gold_hi = (255, 235, 120, 255)
    c_gold_dark = (185, 140, 25, 255)
    
    # Kẻ viền hoa văn vàng trang trọng
    for i in range(32):
        for w in [0, 1, 30, 31]:
            pix[i, w] = c_gold_hi if w in [0, 30] else c_gold_dark
            pix[w, i] = c_gold_hi if w in [0, 30] else c_gold_dark
            
    # Hoa văn góc thánh tích
    for cx, cy in [(4, 4), (27, 4), (4, 27), (27, 27), (16, 16)]:
        for dx in range(-2, 3):
            for dy in range(-2, 3):
                if abs(dx) + abs(dy) <= 2:
                    pix[cx + dx, cy + dy] = c_gold
        pix[cx, cy] = c_gold_hi
    return img

# ==============================================================================
# 3. PROPS: CÔNG TRÌNH HOÀNH TRÁNG (ĐÀI PHUN NƯỚC, BỤI RẬM, CÂY SỒI)
# ==============================================================================

def create_grand_fountain():
    """Đài Phun Nước Thần Thoại Làng Tân Thủ (64x64) cực kỳ tráng lệ"""
    img = new_img(64, 64)
    draw = ImageDraw.Draw(img)
    pix = img.load()
    
    c_stone_base = (195, 195, 205, 255)
    c_stone_hi = (235, 235, 245, 255)
    c_stone_dark = (120, 120, 135, 255)
    c_stone_rim = (85, 85, 100, 255)
    
    c_water_deep = (30, 110, 200, 230)
    c_water_mid = (60, 165, 240, 240)
    c_water_hi = (150, 220, 255, 255)
    c_foam = (230, 250, 255, 255)
    
    # 1. Bể đá tròn đáy lớn (Tâm: 32, 40; bán kính X=26, Y=18)
    for y in range(64):
        for x in range(64):
            dx = (x - 32) / 26.0
            dy = (y - 40) / 18.0
            d2 = dx * dx + dy * dy
            if d2 <= 1.0:
                if d2 > 0.82:
                    # Thành bể đá
                    pix[x, y] = c_stone_hi if (dy < 0 and dx < 0) else c_stone_dark
                elif d2 > 0.72:
                    # Gờ trong thành bể
                    pix[x, y] = c_stone_rim
                else:
                    # Lòng hồ nước trong vắt
                    wave = (math.sin(x * 0.6) + math.cos(y * 0.6)) * 0.5
                    pix[x, y] = c_water_hi if wave > 0.4 else (c_water_deep if wave < -0.3 else c_water_mid)
                    
    # 2. Trụ đá tầng 2 & Bát nước nhỏ (Tâm: 32, 28; bán kính X=13, Y=9)
    for y in range(64):
        for x in range(64):
            dx = (x - 32) / 13.0
            dy = (y - 28) / 9.0
            d2 = dx * dx + dy * dy
            if d2 <= 1.0:
                if d2 > 0.75:
                    pix[x, y] = c_stone_hi if (dy < 0 and dx < 0) else c_stone_dark
                else:
                    pix[x, y] = c_water_hi if (x + y) % 3 == 0 else c_water_mid

    # 3. Trụ điêu khắc thiên thần / hoa sen ở trung tâm phun nước (x=28..36, y=14..24)
    for y in range(14, 25):
        for x in range(29, 36):
            pix[x, y] = c_stone_hi if x <= 31 else c_stone_dark
            
    # 4. Tia nước và bọt nước tung trắng xóa từ vòi phun trên đỉnh
    water_jets = [
        (32, 6, c_foam), (32, 7, c_foam), (31, 8, c_water_hi), (33, 8, c_water_hi),
        (30, 10, c_water_hi), (34, 10, c_water_hi), (28, 13, c_foam), (36, 13, c_foam),
        (26, 17, c_water_hi), (38, 17, c_water_hi), (24, 22, c_foam), (40, 22, c_foam),
        # Giọt nước bắn rơi xuống hồ đáy
        (20, 36, c_foam), (44, 36, c_foam), (32, 44, c_foam), (22, 42, c_water_hi), (42, 42, c_water_hi)
    ]
    for wx, wy, col in water_jets:
        if 0 <= wx < 64 and 0 <= wy < 64:
            pix[wx, wy] = col
            if wx + 1 < 64: pix[wx + 1, wy] = c_water_hi
            
    return img

def create_stealth_bush_big():
    """Bụi Rậm Ẩn Nấp Hoành Tráng (64x64) - Tán lá rậm rạp xum xuê chuẩn MMO"""
    img = new_img(64, 64)
    pix = img.load()
    
    c_leaf_dark = (16, 68, 28, 255)
    c_leaf_mid = (32, 122, 48, 255)
    c_leaf_light = (58, 178, 68, 255)
    c_leaf_high = (110, 222, 95, 255)
    c_berry = (235, 45, 65, 255)
    c_berry_hi = (255, 140, 150, 255)
    
    # 5 Khóm lá lồng vào nhau tạo bụi xum xuê 3D
    clusters = [
        (32, 38, 24, 18),  # Khóm trung tâm
        (18, 42, 14, 14),  # Khóm trái
        (46, 42, 14, 14),  # Khóm phải
        (26, 26, 16, 14),  # Khóm trên trái
        (38, 26, 16, 14),  # Khóm trên phải
    ]
    
    for cx, cy, rx, ry in clusters:
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 64 and 0 <= y < 64:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    d2 = dx * dx + dy * dy
                    if d2 <= 1.0:
                        # Răng cưa viền tán lá
                        edge_noise = (math.sin(x * 1.5) + math.cos(y * 1.5)) * 0.08
                        if d2 + edge_noise > 0.92:
                            pix[x, y] = c_leaf_dark
                        elif dy < -0.2 and dx < 0.1:
                            pix[x, y] = c_leaf_high if (x + y) % 3 == 0 else c_leaf_light
                        elif dy > 0.4:
                            pix[x, y] = c_leaf_dark
                        elif dx > 0.3 and dy > 0.1:
                            pix[x, y] = c_leaf_dark
                        elif dy < 0.1:
                            pix[x, y] = c_leaf_light
                        else:
                            pix[x, y] = c_leaf_mid
                            
    # Quả mọng dại lấp lánh trong bụi
    berries = [(22, 32), (40, 30), (31, 24), (16, 44), (48, 42), (28, 42), (36, 40)]
    for bx, by in berries:
        pix[bx, by] = c_berry
        pix[bx, by - 1] = c_berry_hi
        if bx + 1 < 64: pix[bx + 1, by] = c_berry
        
    return img

def create_magic_water_pillar():
    """Trụ Nước Thần Năng / Đền Nước Thần (48x64)"""
    img = new_img(48, 64)
    pix = img.load()
    
    c_stone = (140, 155, 175, 255)
    c_stone_hi = (200, 215, 235, 255)
    c_stone_dark = (75, 85, 105, 255)
    c_rune_blue = (40, 210, 255, 255)
    c_rune_glow = (180, 245, 255, 255)
    c_water_core = (30, 140, 255, 240)
    
    # 1. Bệ đá cổ ở chân (y=48..63, x=8..40)
    for y in range(48, 64):
        for x in range(8, 40):
            if y == 48 or x == 8: pix[x, y] = c_stone_hi
            elif y == 63 or x == 39: pix[x, y] = c_stone_dark
            else: pix[x, y] = c_stone
            
    # 2. Thân cột đá cổ khắc cổ tự rune (y=24..48, x=14..34)
    for y in range(24, 48):
        for x in range(14, 34):
            if x <= 16: pix[x, y] = c_stone_hi
            elif x >= 32: pix[x, y] = c_stone_dark
            else: pix[x, y] = c_stone
            
            # Cổ tự Rune phát sáng xanh lam
            if (y % 6 == 0 and 18 <= x <= 29) or (x == 23 and 26 <= y <= 45):
                pix[x, y] = c_rune_glow if (x + y) % 2 == 0 else c_rune_blue

    # 3. Viên Thạch Thần Nước lơ lửng trên đỉnh (y=8..22, x=17..31)
    for y in range(8, 23):
        for x in range(17, 31):
            dx = (x - 24) / 6.0
            dy = (y - 15) / 6.0
            if dx*dx + dy*dy <= 1.0:
                if dx*dx + dy*dy <= 0.35:
                    pix[x, y] = c_rune_glow
                else:
                    pix[x, y] = c_water_core
    return img

# ==============================================================================
# 4. MONSTERS: BỘ SPRITE QUÁI VẬT FANTASY SẮC NÉT (48x48)
# ==============================================================================

def create_slime_monster(element="grass"):
    """Quái Slime ngộ nghĩnh, biểu cảm mắt long lanh (48x48)"""
    img = new_img(48, 48)
    pix = img.load()
    
    if element == "grass":
        c_base = (65, 195, 60, 240)
        c_hi = (145, 240, 110, 255)
        c_dark = (35, 125, 30, 255)
    elif element == "water":
        c_base = (45, 155, 235, 240)
        c_hi = (130, 220, 255, 255)
        c_dark = (25, 85, 165, 255)
    else: # Fire / Lava
        c_base = (235, 85, 45, 240)
        c_hi = (255, 180, 80, 255)
        c_dark = (155, 40, 20, 255)
        
    c_eye_white = (255, 255, 255, 255)
    c_eye_pupil = (20, 20, 30, 255)
    
    # Thân Slime hình giọt nước tròn bè đáy (Tâm: 24, 30)
    for y in range(48):
        for x in range(48):
            dx = (x - 24) / 16.0
            dy = (y - 30) / 12.0
            if dy < 0: # Vuốt nhọn phía trên
                dx = (x - 24) / (16.0 - abs(dy) * 4.0)
            d2 = dx * dx + dy * dy
            if d2 <= 1.0:
                if d2 > 0.85:
                    pix[x, y] = c_dark
                elif dy < -0.2 and dx < 0.1:
                    pix[x, y] = c_hi
                else:
                    pix[x, y] = c_base

    # Đôi mắt sáng long lanh
    eyes = [(19, 27), (29, 27)]
    for ex, ey in eyes:
        for dx in range(-2, 3):
            for dy in range(-2, 3):
                if dx*dx + dy*dy <= 4:
                    pix[ex + dx, ey + dy] = c_eye_white
        pix[ex, ey] = c_eye_pupil
        pix[ex + 1, ey] = c_eye_pupil
        pix[ex - 1, ey - 1] = (255, 255, 255, 255) # Tia sáng mắt
        
    # Miệng cười
    pix[23, 33] = c_dark; pix[24, 33] = c_dark; pix[25, 33] = c_dark
    return img

def create_forest_wolf():
    """Dã Lang Rừng Sâu (Forest Wolf - 48x48)"""
    img = new_img(48, 48)
    pix = img.load()
    c_fur = (120, 115, 125, 255)
    c_fur_dark = (70, 65, 75, 255)
    c_fur_hi = (180, 175, 185, 255)
    c_eye_glow = (255, 220, 40, 255)
    
    # Đầu sói uy nghiêm
    for y in range(12, 38):
        for x in range(14, 34):
            dx = (x - 24) / 9.0
            dy = (y - 26) / 10.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_fur_hi if dx < 0 else c_fur
                if dy > 0.6: pix[x, y] = c_fur_dark
                
    # Tai sói nhọn
    for y in range(6, 16):
        w = (16 - y) // 2
        for x in range(16 - w, 17 + w): pix[x, y] = c_fur_dark
        for x in range(31 - w, 32 + w): pix[x, y] = c_fur_dark
        
    # Mõm & Mũi sói
    for y in range(28, 36):
        for x in range(21, 28): pix[x, y] = (50, 45, 55, 255)
    pix[24, 30] = (20, 20, 25, 255) # Mũi
    
    # Đôi mắt vàng hung dữ rực lửa
    pix[19, 23] = c_eye_glow; pix[20, 23] = c_eye_glow
    pix[27, 23] = c_eye_glow; pix[28, 23] = c_eye_glow
    return img

def create_ancient_lich_boss():
    """Đại Vu Yêu Hắc Ám Lich (Boss Hầm Ngục - 64x64) cực kỳ quyền năng"""
    img = new_img(64, 64)
    pix = img.load()
    c_robe = (45, 20, 75, 255)
    c_robe_gold = (225, 180, 40, 255)
    c_skull = (215, 210, 200, 255)
    c_eye_flame = (40, 230, 255, 255)
    
    # Áo choàng ma thuật bay lượn (y=24..60, x=16..48)
    for y in range(24, 61):
        w = 10 + int((y - 24) * 0.35)
        for x in range(32 - w, 33 + w):
            pix[x, y] = c_robe
            if x == 32 - w or x == 32 + w:
                pix[x, y] = c_robe_gold
                
    # Hộp sọ đầu lâu Lich (y=12..25, x=26..38)
    for y in range(12, 26):
        for x in range(26, 39):
            dx = (x - 32) / 5.5
            dy = (y - 18) / 6.0
            if dx*dx + dy*dy <= 1.0:
                pix[x, y] = c_skull
                
    # Vương miện hắc ám có sừng
    for x in range(25, 40): pix[x, 11] = c_robe_gold
    for h in range(5, 12):
        pix[25, h] = c_robe_gold; pix[39, h] = c_robe_gold
        
    # Mắt linh hồn rực lửa xanh băng
    pix[29, 18] = c_eye_flame; pix[30, 18] = c_eye_flame
    pix[34, 18] = c_eye_flame; pix[35, 18] = c_eye_flame
    return img

def main():
    print("Generating Tile, Prop, and Monster Pixel Art...")
    
    assets = {
        # 1. He co tu nhien chuan muc
        "tile_grass.png": create_base_grass(),
        "tile_grass_dense.png": create_dense_clover_grass(),
        "tile_grass_flower_yellow.png": create_flower_grass("yellow"),
        "tile_grass_flower_red.png": create_flower_grass("red"),
        "tile_grass_flower_blue.png": create_flower_grass("blue"),
        "tile_grass_flower_white.png": create_flower_grass("white"),
        
        # 2. Da quang truong & Vanh dai bao ho
        "tile_stone.png": create_plaza_cobble(),
        "tile_safezone.png": create_safezone_gold(),
        
        # 3. Cong trinh hoanh trang
        "prop_fountain_grand.png": create_grand_fountain(),
        "prop_bush_stealth.png": create_stealth_bush_big(),
        "prop_water_pillar.png": create_magic_water_pillar(),
        
        # 4. Quai vat song dong da dang
        "monster_slime_grass.png": create_slime_monster("grass"),
        "monster_slime_water.png": create_slime_monster("water"),
        "monster_forest_wolf.png": create_forest_wolf(),
        "monster_lich_boss.png": create_ancient_lich_boss(),
    }
    
    for filename, img in assets.items():
        out_path = os.path.join(RESOURCES_DIR, filename)
        img.save(out_path)
        print(f"  + Generated: {filename} ({img.size[0]}x{img.size[1]})")

    print("DONE: Generated all pixel art assets successfully!")

if __name__ == "__main__":
    main()
