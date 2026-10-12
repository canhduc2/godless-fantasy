import os
from PIL import Image, ImageDraw

RESOURCES_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources"
os.makedirs(RESOURCES_DIR, exist_ok=True)

def new_img(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))

# -------------------------------------------------------------
# 1. TILES: BRIGHT VIBRANT GRASSLAND SYSTEM (32x32)
# -------------------------------------------------------------

def create_tile_grass_bright():
    # Đồng cỏ sáng tươi tắn kiểu Stardew / Zelda Minish Cap
    img = new_img(32, 32)
    pix = img.load()
    c_base = (112, 204, 72, 255)      # Xanh cỏ tươi sáng
    c_light = (136, 224, 88, 255)     # Ngọn cỏ đón nắng
    c_dark = (90, 178, 54, 255)       # Bóng cỏ mềm
    c_shadow = (72, 150, 42, 255)
    
    for y in range(32):
        for x in range(32):
            # Tạo chất liệu cỏ mịn tự nhiên
            v = (x * 11 + y * 17 + ((x ^ y) * 5)) % 31
            if v < 4:
                pix[x, y] = c_dark
            elif v > 24:
                pix[x, y] = c_light
            else:
                pix[x, y] = c_base

    # Vẽ các ngọn cỏ nhỏ li ti (tufts) ngẫu nhiên
    tufts = [(5, 6), (14, 18), (24, 8), (19, 26), (8, 22), (27, 20)]
    for tx, ty in tufts:
        pix[tx, ty] = c_light
        if ty > 0: pix[tx, ty - 1] = c_light
        if tx + 1 < 32 and ty > 0: pix[tx + 1, ty - 1] = (156, 238, 102, 255)
        if ty + 1 < 32: pix[tx, ty + 1] = c_shadow
    return img

def create_tile_grass_flowers(flower_type="yellow"):
    # Cỏ hoa dã quỳ vàng / hoa lưu ly xanh / hoa poppy đỏ
    img = create_tile_grass_bright()
    pix = img.load()
    
    flower_colors = {
        "yellow": ((255, 230, 60, 255), (255, 170, 20, 255)),
        "blue": ((100, 190, 255, 255), (40, 110, 240, 255)),
        "red": ((255, 80, 90, 255), (180, 30, 50, 255))
    }
    f_petal, f_center = flower_colors.get(flower_type, flower_colors["yellow"])
    
    spots = [(6, 10), (22, 7), (12, 24), (25, 22)]
    for sx, sy in spots:
        # Nhụy hoa
        pix[sx, sy] = f_center
        # Cánh hoa xung quanh
        if sx > 0: pix[sx - 1, sy] = f_petal
        if sx + 1 < 32: pix[sx + 1, sy] = f_petal
        if sy > 0: pix[sx, sy - 1] = f_petal
        if sy + 1 < 32: pix[sx, sy + 1] = f_petal
    return img

def create_tile_grass_dense():
    # Cỏ đậm rậm rạp có cỏ 3 lá (clover)
    img = new_img(32, 32)
    pix = img.load()
    c_base = (85, 175, 55, 255)
    c_light = (110, 200, 75, 255)
    c_dark = (65, 145, 38, 255)
    c_clover = (145, 225, 95, 255)
    
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 13) % 19
            pix[x, y] = c_light if v > 15 else (c_dark if v < 4 else c_base)
            
    # Cỏ 3 lá
    clovers = [(8, 12), (20, 22), (16, 6), (26, 14)]
    for cx, cy in clovers:
        pix[cx, cy] = c_clover
        if cx > 0: pix[cx - 1, cy] = c_clover
        if cy > 0: pix[cx, cy - 1] = c_clover
        if cy + 1 < 32: pix[cx, cy + 1] = (50, 110, 30, 255)
    return img

def create_tile_dirt_bright():
    # Đường đất nện ấm áp màu cát sỏi tự nhiên
    img = new_img(32, 32)
    pix = img.load()
    c_base = (212, 178, 128, 255)    # Cát đất ấm
    c_light = (235, 205, 155, 255)   # Ánh nắng
    c_dark = (182, 145, 98, 255)     # Rãnh đất
    c_pebble = (140, 130, 120, 255)  # Sỏi cuội
    
    for y in range(32):
        for x in range(32):
            v = (x * 13 + y * 7 + (x ^ y)) % 23
            if v < 5:
                pix[x, y] = c_dark
            elif v > 18:
                pix[x, y] = c_light
            else:
                pix[x, y] = c_base
                
    # Sỏi đá nhỏ
    pebbles = [(7, 8), (21, 15), (12, 23), (26, 25)]
    for px, py in pebbles:
        pix[px, py] = c_pebble
        if px + 1 < 32: pix[px + 1, py] = (175, 170, 160, 255)
        if py + 1 < 32: pix[px, py + 1] = (110, 95, 75, 255)
    return img

def create_tile_water_cyan():
    # Nước hồ / sông ngọc bích trong veo với sóng phản chiếu
    img = new_img(32, 32)
    pix = img.load()
    c_deep = (28, 130, 185, 255)      # Xanh lam ngọc sâu
    c_mid = (45, 170, 225, 255)       # Ngọc bích tươi sáng
    c_light = (85, 205, 245, 255)     # Mặt nước bắt sáng
    c_foam = (215, 248, 255, 240)     # Bọt sóng trắng
    
    for y in range(32):
        for x in range(32):
            v = (x * 5 + y * 9 + (x * y) % 7) % 17
            pix[x, y] = c_light if v > 13 else (c_deep if v < 4 else c_mid)
            
    # Gợn sóng bọt trắng phản quang
    waves = [(4, 6), (18, 12), (8, 22), (22, 26)]
    for wx, wy in waves:
        for offset in range(4):
            if wx + offset < 32:
                pix[wx + offset, wy] = c_foam
                if wy + 1 < 32: pix[wx + offset, wy + 1] = c_light
    return img

def create_tile_sand_shore():
    # Bãi cát vàng ven hồ / biển
    img = new_img(32, 32)
    pix = img.load()
    c_sand = (240, 218, 150, 255)
    c_sand_hi = (255, 238, 180, 255)
    c_sand_dark = (215, 190, 120, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 19 + y * 11) % 13
            pix[x, y] = c_sand_hi if v > 10 else (c_sand_dark if v < 3 else c_sand)
    return img

# -------------------------------------------------------------
# 2. PROPS: STEALTH BUSH, ROCKS, LOGS, TREES & WAYSTONES
# -------------------------------------------------------------

def create_stealth_bush():
    # Bụi rậm ẩn nấp (36x36) - tán lá rậm rạp tròn xoe cực đẹp
    w, h = 36, 36
    img = new_img(w, h)
    pix = img.load()
    
    c_dark = (24, 85, 36, 255)
    c_mid = (48, 155, 58, 255)
    c_light = (95, 215, 85, 255)
    c_hi = (165, 245, 120, 255)
    c_flower = (255, 240, 140, 255)
    
    # 3 cụm lá tròn kết hợp thành bụi cây phồng dày
    clusters = [(18, 20, 15, 13), (12, 17, 10, 10), (24, 17, 10, 10), (18, 12, 11, 9)]
    for cx, cy, rx, ry in clusters:
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < w and 0 <= y < h:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    dist = dx * dx + dy * dy
                    if dist <= 1.0:
                        if dist > 0.88:
                            pix[x, y] = c_dark
                        elif dy < -0.3 and dx < 0.2:
                            pix[x, y] = c_hi
                        elif dy < 0.15:
                            pix[x, y] = c_light
                        elif dy > 0.5:
                            pix[x, y] = c_dark
                        else:
                            pix[x, y] = c_mid
                            
    # Thêm vài chấm hoa vàng li ti trên bụi rậm
    berries = [(14, 15), (22, 18), (17, 10), (25, 13)]
    for bx, by in berries:
        pix[bx, by] = c_flower
        if by > 0: pix[bx, by - 1] = (255, 255, 220, 255)
    return img

def create_grass_tuft():
    # Khóm cỏ nổi đung đưa (16x16)
    w, h = 16, 16
    img = new_img(w, h)
    pix = img.load()
    c_blade = (110, 220, 75, 255)
    c_tip = (165, 250, 115, 255)
    c_base = (45, 125, 35, 255)
    
    blades = [
        [(7, 14), (6, 11), (5, 8), (4, 5)],     # Lá nghiêng trái
        [(8, 14), (8, 10), (8, 6), (8, 3)],     # Lá thẳng giữa
        [(9, 14), (10, 11), (11, 8), (12, 6)],  # Lá nghiêng phải
        [(6, 14), (7, 12), (6, 9)],
        [(10, 14), (10, 12), (11, 10)]
    ]
    for b in blades:
        for idx, (bx, by) in enumerate(b):
            col = c_tip if idx == len(b) - 1 else (c_base if idx == 0 else c_blade)
            pix[bx, by] = col
    return img

def create_mossy_boulder():
    # Tảng đá cuội lớn có rêu xanh (32x32)
    w, h = 32, 32
    img = new_img(w, h)
    pix = img.load()
    
    c_moss_hi = (130, 215, 85, 255)
    c_moss = (75, 160, 50, 255)
    c_stone_hi = (185, 195, 210, 255)
    c_stone = (135, 145, 160, 255)
    c_stone_dark = (85, 95, 110, 255)
    c_shadow = (45, 50, 60, 255)
    
    cx, cy, rx, ry = 16, 17, 13, 10
    for y in range(cy - ry, cy + ry + 1):
        for x in range(cx - rx, cx + rx + 1):
            if 0 <= x < w and 0 <= y < h:
                dx = (x - cx) / float(rx)
                dy = (y - cy) / float(ry)
                dist = dx * dx + dy * dy
                if dist <= 1.0:
                    if dist > 0.85 or dy > 0.6:
                        pix[x, y] = c_shadow
                    elif dy < -0.3:
                        # Rêu xanh trên đỉnh tảng đá
                        pix[x, y] = c_moss_hi if (dx < 0.1 and dy < -0.5) else c_moss
                    elif dx < -0.1 and dy < 0.2:
                        pix[x, y] = c_stone_hi
                    elif dy > 0.3:
                        pix[x, y] = c_stone_dark
                    else:
                        pix[x, y] = c_stone
    return img

def create_fallen_log():
    # Khúc gỗ mục nằm ngang có rêu và nấm (48x24)
    w, h = 48, 24
    img = new_img(w, h)
    pix = img.load()
    
    c_bark_dark = (55, 35, 20, 255)
    c_bark_mid = (95, 60, 35, 255)
    c_bark_light = (140, 95, 55, 255)
    c_rings = (185, 145, 95, 255)
    c_moss = (85, 175, 60, 255)
    c_shroom = (235, 65, 55, 255)
    
    # Thân khúc gỗ (x=6..42, y=9..18)
    for y in range(9, 19):
        for x in range(6, 42):
            if y == 9 or y == 18:
                pix[x, y] = c_bark_dark
            elif y < 13:
                pix[x, y] = c_moss if ((x + y) % 7 < 3 and x < 28) else c_bark_light
            elif y > 15:
                pix[x, y] = c_bark_dark
            else:
                pix[x, y] = c_bark_mid
                
    # Vòng tròn vân gỗ hai đầu cành
    for y in range(10, 18):
        pix[6, y] = c_rings
        pix[5, y] = c_bark_dark
        pix[42, y] = c_bark_dark
        
    # Nấm nhỏ đỏ mọc trên thân gỗ
    pix[22, 7] = c_shroom; pix[23, 7] = c_shroom; pix[24, 7] = (255, 255, 255, 255)
    pix[23, 8] = (240, 240, 220, 255)
    return img

def create_lush_oak_tree():
    # Cây sồi xanh tươi tráng lệ (52x68)
    w, h = 52, 68
    img = new_img(w, h)
    pix = img.load()
    
    c_wood_dark = (55, 32, 18, 255)
    c_wood_mid = (100, 60, 35, 255)
    c_wood_light = (150, 95, 55, 255)
    
    c_leaf_dark = (28, 85, 42, 255)
    c_leaf_mid = (55, 160, 68, 255)
    c_leaf_light = (105, 215, 95, 255)
    c_leaf_hi = (168, 248, 130, 255)
    
    # Thân cây lớn (x=22..29, y=40..66)
    for y in range(40, 67):
        w_trunk = 4 + int((y - 40) * 0.22)
        for x in range(26 - w_trunk, 26 + w_trunk):
            if x == 26 - w_trunk or x == 25 + w_trunk:
                pix[x, y] = c_wood_dark
            elif x < 25:
                pix[x, y] = c_wood_light
            else:
                pix[x, y] = c_wood_mid
                
    # Rễ bám đất
    for rx in range(18, 34):
        pix[rx, 66] = c_wood_dark
        
    # Tán lá vĩ đại (Oval xum xuê 4 tầng)
    foliage = [
        (26, 24, 22, 19), (16, 30, 15, 14), (36, 30, 15, 14),
        (26, 14, 16, 12), (20, 20, 13, 11), (32, 20, 13, 11)
    ]
    for cx, cy, rx, ry in foliage:
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < w and 0 <= y < h:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    dist = dx * dx + dy * dy
                    if dist <= 1.0:
                        if dist > 0.88:
                            pix[x, y] = c_leaf_dark
                        elif dy < -0.3 and dx < 0.1:
                            pix[x, y] = c_leaf_hi
                        elif dy < 0.18:
                            pix[x, y] = c_leaf_light
                        elif dy > 0.5:
                            pix[x, y] = c_leaf_dark
                        else:
                            pix[x, y] = c_leaf_mid
    return img

def create_waystone_shrine():
    # Trụ hồi sinh đá cổ khắc rune ma thuật phát sáng (28x56)
    w, h = 28, 56
    img = new_img(w, h)
    pix = img.load()
    
    c_stone_hi = (195, 205, 225, 255)
    c_stone_mid = (130, 140, 160, 255)
    c_stone_dark = (75, 82, 100, 255)
    c_stone_shadow = (45, 50, 65, 255)
    
    c_crystal_core = (230, 255, 255, 255)
    c_crystal_cyan = (60, 230, 255, 255)
    c_crystal_glow = (20, 160, 240, 220)
    c_gold = (255, 215, 60, 255)
    
    # 1. Bệ đá bậc thang đáy (y=44..54)
    for y in range(44, 55):
        bw = 12 if y >= 50 else 10
        for x in range(14 - bw, 14 + bw):
            if y == 54 or x == 14 - bw or x == 13 + bw:
                pix[x, y] = c_stone_shadow
            elif y == 44 or y == 50:
                pix[x, y] = c_stone_hi
            elif x < 14:
                pix[x, y] = c_stone_mid
            else:
                pix[x, y] = c_stone_dark
                
    # 2. Cột trụ đá khắc hoa văn (y=18..43)
    for y in range(18, 44):
        pw = 6
        for x in range(14 - pw, 14 + pw):
            if x == 14 - pw or x == 13 + pw:
                pix[x, y] = c_stone_shadow
            elif x == 14 - pw + 1:
                pix[x, y] = c_stone_hi
            elif x >= 14:
                pix[x, y] = c_stone_dark
            else:
                pix[x, y] = c_stone_mid
                
    # 3. Ký tự Rune ma thuật phát sáng rực rỡ trên thân trụ (y=22..38)
    runes = [(13, 23), (14, 23), (14, 24), (13, 27), (14, 27), (14, 28), (14, 32), (13, 33), (14, 34), (13, 37)]
    for rx, ry in runes:
        pix[rx, ry] = c_crystal_cyan
        if rx > 0: pix[rx - 1, ry] = c_crystal_glow
        if rx + 1 < w: pix[rx + 1, ry] = c_crystal_glow
        
    # 4. Viên Pha Lê Linh Hồn Lơ Lửng trên đỉnh (Floating Soul Crystal, y=4..15)
    crystal_coords = [
        (13, 4, c_crystal_core), (14, 4, c_crystal_core),
        (12, 5, c_crystal_cyan), (13, 5, c_crystal_core), (14, 5, c_crystal_core), (15, 5, c_crystal_cyan),
        (11, 7, c_crystal_glow), (12, 7, c_crystal_cyan), (13, 7, c_crystal_core), (14, 7, c_crystal_core), (15, 7, c_crystal_cyan), (16, 7, c_crystal_glow),
        (11, 9, c_crystal_glow), (12, 9, c_crystal_cyan), (13, 9, c_crystal_core), (14, 9, c_crystal_core), (15, 9, c_crystal_cyan), (16, 9, c_crystal_glow),
        (12, 12, c_crystal_cyan), (13, 12, c_crystal_core), (14, 12, c_crystal_core), (15, 12, c_crystal_cyan),
        (13, 14, c_crystal_cyan), (14, 14, c_crystal_cyan)
    ]
    for cx, cy, col in crystal_coords:
        pix[cx, cy] = col
        
    # 5. Viền kim loại bảo vệ bệ đỡ pha lê (y=16..18)
    for x in range(9, 19):
        pix[x, 17] = c_gold
    return img

def main():
    assets = {
        "tile_grass.png": create_tile_grass_bright(),
        "tile_grass_flower_yellow.png": create_tile_grass_flowers("yellow"),
        "tile_grass_flower_blue.png": create_tile_grass_flowers("blue"),
        "tile_grass_flower_red.png": create_tile_grass_flowers("red"),
        "tile_grass_dense.png": create_tile_grass_dense(),
        "tile_dirt_path.png": create_tile_dirt_bright(),
        "tile_water.png": create_tile_water_cyan(),
        "tile_sand_shore.png": create_tile_sand_shore(),
        "prop_bush_stealth.png": create_stealth_bush(),
        "prop_grass_tuft.png": create_grass_tuft(),
        "prop_rock_mossy.png": create_mossy_boulder(),
        "prop_fallen_log.png": create_fallen_log(),
        "prop_tree_village.png": create_lush_oak_tree(),
        "prop_waystone_shrine.png": create_waystone_shrine(),
    }
    
    for filename, img in assets.items():
        out_path = os.path.join(RESOURCES_DIR, filename)
        img.save(out_path)
        print(f"Generated {filename} ({img.size})")

if __name__ == "__main__":
    main()
