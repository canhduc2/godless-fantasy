import os
from PIL import Image, ImageDraw

RESOURCES_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources"
BLOCKS_DIR = os.path.join(RESOURCES_DIR, "Blocks")
os.makedirs(BLOCKS_DIR, exist_ok=True)

def new_img(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))

# -------------------------------------------------------------
# NHÓM 1: HỆ ĐỒNG CỎ & ĐẤT (IDs 0..11)
# -------------------------------------------------------------

def gen_grass_bright():
    # 0: grass_bright
    img = new_img(32, 32); pix = img.load()
    c_base = (112, 204, 72, 255); c_hi = (138, 226, 92, 255); c_dark = (90, 178, 54, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 11 + y * 17 + ((x ^ y) * 5)) % 31
            pix[x, y] = c_hi if v > 24 else (c_dark if v < 4 else c_base)
    tufts = [(5, 6), (14, 18), (24, 8), (19, 26), (8, 22), (27, 20)]
    for tx, ty in tufts:
        pix[tx, ty] = c_hi
        if ty > 0: pix[tx, ty - 1] = c_hi
        if ty + 1 < 32: pix[tx, ty + 1] = (70, 148, 40, 255)
    return img

def gen_grass_flowers(col_petal, col_center):
    img = gen_grass_bright(); pix = img.load()
    spots = [(6, 10), (22, 7), (12, 24), (25, 22)]
    for sx, sy in spots:
        pix[sx, sy] = col_center
        if sx > 0: pix[sx - 1, sy] = col_petal
        if sx + 1 < 32: pix[sx + 1, sy] = col_petal
        if sy > 0: pix[sx, sy - 1] = col_petal
        if sy + 1 < 32: pix[sx, sy + 1] = col_petal
    return img

def gen_grass_dense():
    # 5: grass_dense_clover
    img = new_img(32, 32); pix = img.load()
    c_base = (80, 170, 50, 255); c_hi = (105, 195, 70, 255); c_dark = (60, 138, 35, 255); c_clover = (145, 225, 95, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 13) % 19
            pix[x, y] = c_hi if v > 15 else (c_dark if v < 4 else c_base)
    clovers = [(8, 12), (20, 22), (16, 6), (26, 14)]
    for cx, cy in clovers:
        pix[cx, cy] = c_clover
        if cx > 0: pix[cx - 1, cy] = c_clover
        if cy > 0: pix[cx, cy - 1] = c_clover
        if cy + 1 < 32: pix[cx, cy + 1] = (50, 110, 30, 255)
    return img

def gen_grass_autumn():
    # 6: grass_autumn
    img = new_img(32, 32); pix = img.load()
    c_base = (195, 160, 65, 255); c_hi = (225, 190, 85, 255); c_dark = (165, 130, 48, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 9 + y * 11) % 17
            pix[x, y] = c_hi if v > 13 else (c_dark if v < 4 else c_base)
    return img

def gen_grass_dark():
    # 7: grass_dark_forest
    img = new_img(32, 32); pix = img.load()
    c_base = (35, 95, 55, 255); c_hi = (55, 130, 75, 255); c_dark = (22, 68, 38, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 5 + y * 9) % 13
            pix[x, y] = c_hi if v > 9 else (c_dark if v < 3 else c_base)
    return img

def gen_dirt_path():
    # 8: dirt_warm_path
    img = new_img(32, 32); pix = img.load()
    c_base = (212, 178, 128, 255); c_hi = (235, 205, 155, 255); c_dark = (182, 145, 98, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 13 + y * 7 + (x ^ y)) % 23
            pix[x, y] = c_dark if v < 5 else (c_hi if v > 18 else c_base)
    pebbles = [(7, 8), (21, 15), (12, 23), (26, 25)]
    for px, py in pebbles:
        pix[px, py] = (140, 130, 120, 255)
        if px + 1 < 32: pix[px + 1, py] = (175, 170, 160, 255)
    return img

def gen_dirt_packed_road():
    # 9: dirt_packed_road
    img = new_img(32, 32); pix = img.load()
    c_base = (175, 145, 110, 255); c_dark = (140, 110, 80, 255); c_stone = (120, 125, 135, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 8 + y * 12) % 15
            pix[x, y] = c_dark if v < 4 else c_base
            if (x * y) % 29 == 0: pix[x, y] = c_stone
    return img

def gen_farmland():
    # 10: farmland_tilled
    img = new_img(32, 32); pix = img.load()
    c_ridge = (120, 85, 55, 255); c_furrow = (80, 55, 35, 255); c_sprout = (110, 210, 70, 255)
    for y in range(32):
        is_furrow = (y % 6 < 3)
        for x in range(32):
            pix[x, y] = c_furrow if is_furrow else c_ridge
            if is_furrow and (x % 7 == 3): pix[x, y] = c_sprout
    return img

def gen_mud_swamp():
    # 11: mud_swamp
    img = new_img(32, 32); pix = img.load()
    c_base = (90, 75, 55, 255); c_dark = (60, 50, 35, 255); c_puddle = (55, 80, 70, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 9) % 11
            pix[x, y] = c_puddle if v < 3 else (c_dark if v > 8 else c_base)
    return img

# -------------------------------------------------------------
# NHÓM 2: HỆ NƯỚC, SÔNG, BIỂN & BỜ CÁT (IDs 12..19)
# -------------------------------------------------------------

def gen_water_shallow():
    # 12: water_shallow
    img = new_img(32, 32); pix = img.load()
    c_deep = (35, 150, 210, 255); c_mid = (55, 185, 235, 255); c_light = (95, 215, 250, 255); c_foam = (220, 250, 255, 240)
    for y in range(32):
        for x in range(32):
            v = (x * 5 + y * 9) % 17
            pix[x, y] = c_light if v > 13 else (c_deep if v < 4 else c_mid)
    waves = [(4, 6), (18, 12), (8, 22), (22, 26)]
    for wx, wy in waves:
        for offset in range(4):
            if wx + offset < 32: pix[wx + offset, wy] = c_foam
    return img

def gen_water_deep():
    # 13: water_deep
    img = new_img(32, 32); pix = img.load()
    c_deep = (18, 70, 150, 255); c_mid = (25, 105, 185, 255); c_light = (45, 135, 215, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 5) % 13
            pix[x, y] = c_deep if v < 4 else (c_light if v > 10 else c_mid)
    return img

def gen_water_stream():
    # 14: water_river_stream
    img = gen_water_shallow(); pix = img.load()
    for y in range(32):
        offset = (y * 2) % 32
        pix[offset, y] = (255, 255, 255, 255)
        if offset + 1 < 32: pix[offset + 1, y] = (180, 240, 255, 255)
    return img

def gen_waterfall():
    # 15: water_waterfall
    img = new_img(32, 32); pix = img.load()
    c_water = (70, 190, 240, 255); c_foam = (255, 255, 255, 255)
    for y in range(32):
        for x in range(32):
            pix[x, y] = c_foam if (x % 3 == 0 or y % 4 == 0) else c_water
    return img

def gen_sand_shore():
    # 16: sand_shore
    img = new_img(32, 32); pix = img.load()
    c_sand = (240, 218, 150, 255); c_hi = (255, 238, 180, 255); c_dark = (215, 190, 120, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 19 + y * 11) % 13
            pix[x, y] = c_hi if v > 10 else (c_dark if v < 3 else c_sand)
    return img

def gen_sand_wet():
    # 17: sand_wet
    img = new_img(32, 32); pix = img.load()
    c_sand = (195, 170, 115, 255); c_dark = (165, 140, 90, 255); c_glint = (235, 215, 160, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 13 + y * 7) % 11
            pix[x, y] = c_glint if v > 9 else (c_dark if v < 3 else c_sand)
    return img

def gen_ice_frozen():
    # 18: ice_frozen
    img = new_img(32, 32); pix = img.load()
    c_ice = (175, 225, 245, 255); c_glint = (240, 255, 255, 255); c_crack = (120, 185, 215, 255)
    for y in range(32):
        for x in range(32):
            pix[x, y] = c_ice
            if (x + y * 2) % 17 == 0: pix[x, y] = c_crack
            elif (x ^ y) % 23 == 0: pix[x, y] = c_glint
    return img

def gen_poison_swamp():
    # 19: poison_swamp_water
    img = new_img(32, 32); pix = img.load()
    c_base = (45, 110, 45, 255); c_bubble = (110, 225, 80, 255); c_dark = (25, 70, 30, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 13) % 17
            pix[x, y] = c_bubble if v > 14 else (c_dark if v < 4 else c_base)
    return img

# -------------------------------------------------------------
# NHÓM 3: HỆ ĐÁ, TƯỜNG, HẦM NGỤC & ĐƯỜNG PHỐ (IDs 20..29)
# -------------------------------------------------------------

def gen_cobble_plaza():
    # 20: cobble_plaza_clean
    img = new_img(32, 32); pix = img.load()
    c_stone = (165, 175, 190, 255); c_hi = (205, 215, 230, 255); c_grout = (95, 105, 120, 255)
    for y in range(32):
        for x in range(32):
            is_grout = (x % 8 == 0 or y % 8 == 0)
            is_hi = (x % 8 == 1 or y % 8 == 1)
            pix[x, y] = c_grout if is_grout else (c_hi if is_hi else c_stone)
    return img

def gen_cobble_mossy():
    # 21: cobble_mossy
    img = gen_cobble_plaza(); pix = img.load()
    c_moss = (95, 175, 65, 255)
    for y in range(32):
        for x in range(32):
            if (x * 3 + y * 5) % 11 < 3: pix[x, y] = c_moss
    return img

def gen_dungeon_tile():
    # 22: stone_dungeon_tile
    img = new_img(32, 32); pix = img.load()
    c_stone = (85, 75, 95, 255); c_dark = (50, 42, 60, 255); c_crack = (30, 25, 38, 255)
    for y in range(32):
        for x in range(32):
            is_grout = (x % 16 == 0 or y % 16 == 0)
            pix[x, y] = c_crack if is_grout else ((x + y) % 7 < 2 and c_dark or c_stone)
    return img

def gen_stone_wall():
    # 23: stone_brick_wall
    img = new_img(32, 32); pix = img.load()
    c_brick = (115, 110, 120, 255); c_top = (165, 160, 175, 255); c_mortar = (55, 50, 60, 255)
    for y in range(32):
        for x in range(32):
            if y < 4: pix[x, y] = c_top
            else:
                row = y // 7
                x_offset = (row % 2) * 8
                is_mortar = ((x + x_offset) % 16 == 0 or y % 7 == 0)
                pix[x, y] = c_mortar if is_mortar else c_brick
    return img

def gen_cliff_wall():
    # 24: cliff_rock_wall
    img = new_img(32, 32); pix = img.load()
    c_cliff = (110, 95, 80, 255); c_hi = (155, 135, 115, 255); c_shadow = (65, 55, 45, 255)
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 13) % 17
            pix[x, y] = c_hi if v > 12 else (c_shadow if v < 4 else c_cliff)
    return img

def gen_wood_plank():
    # 25: wooden_floor_plank
    img = new_img(32, 32); pix = img.load()
    c_plank = (165, 115, 70, 255); c_dark = (120, 80, 45, 255); c_nail = (70, 60, 50, 255)
    for y in range(32):
        is_seam = (y % 8 == 0)
        for x in range(32):
            pix[x, y] = c_dark if is_seam else c_plank
            if (x == 4 or x == 28) and (y % 8 == 4): pix[x, y] = c_nail
    return img

def gen_wood_bridge():
    # 26: wooden_bridge
    img = gen_wood_plank(); pix = img.load()
    for y in range(32):
        pix[0, y] = (80, 50, 30, 255); pix[1, y] = (110, 75, 45, 255)
        pix[30, y] = (110, 75, 45, 255); pix[31, y] = (80, 50, 30, 255)
    return img

def gen_ruins_pillar():
    # 27: ruins_pillar_base
    img = new_img(32, 32); pix = img.load()
    c_base = (145, 150, 165, 255); c_grout = (80, 85, 95, 255); c_gold = (215, 180, 60, 255)
    for y in range(32):
        for x in range(32):
            dist = ((x - 16)**2 + (y - 16)**2)**0.5
            pix[x, y] = c_gold if (10 < dist < 12) else (c_base if dist <= 10 else c_grout)
    return img

def gen_magic_rune_circle():
    # 28: magic_rune_circle
    img = gen_dungeon_tile(); pix = img.load()
    c_glow = (180, 95, 255, 255)
    for y in range(32):
        for x in range(32):
            dist = ((x - 15.5)**2 + (y - 15.5)**2)**0.5
            if 8 <= dist <= 10 or (abs(x - y) < 1 and dist < 10) or (abs(x + y - 31) < 1 and dist < 10):
                pix[x, y] = c_glow
    return img

def gen_safezone_gold():
    # 29: safezone_gold_tile
    img = gen_cobble_plaza(); pix = img.load()
    c_gold = (255, 215, 65, 255)
    for x in range(32):
        pix[x, 0] = c_gold; pix[x, 31] = c_gold
    for y in range(32):
        pix[0, y] = c_gold; pix[31, y] = c_gold
    return img

# -------------------------------------------------------------
# NHÓM 4: THỰC VẬT & BỤI RẬM & ANIMATED FOLIAGE (IDs 30..39)
# -------------------------------------------------------------

def gen_stealth_bush():
    # 30: foliage_stealth_bush (32x32)
    img = new_img(32, 32); pix = img.load()
    c_dark = (24, 85, 36, 255); c_mid = (48, 155, 58, 255); c_hi = (115, 225, 95, 255); c_glow = (175, 250, 135, 255)
    cx, cy, rx, ry = 16, 16, 14, 13
    for y in range(cy - ry, cy + ry + 1):
        for x in range(cx - rx, cx + rx + 1):
            if 0 <= x < 32 and 0 <= y < 32:
                dist = ((x - cx) / float(rx))**2 + ((y - cy) / float(ry))**2
                if dist <= 1.0:
                    pix[x, y] = c_dark if dist > 0.85 else (c_glow if (y < cy - 4 and x < cx + 2) else (c_hi if y < cy + 2 else c_mid))
    # Chấm hoa
    pix[12, 12] = (255, 240, 120, 255); pix[20, 14] = (255, 240, 120, 255)
    return img

def gen_bush_berries():
    # 31: foliage_bush_berries
    img = gen_stealth_bush(); pix = img.load()
    berries = [(10, 14), (22, 12), (15, 20), (18, 10)]
    for bx, by in berries:
        pix[bx, by] = (245, 50, 75, 255)
        if by > 0: pix[bx, by - 1] = (255, 160, 180, 255)
    return img

def gen_grass_tuft():
    # 32: foliage_grass_tuft
    img = new_img(32, 32); pix = img.load()
    c_blade = (110, 220, 75, 255); c_tip = (165, 250, 115, 255); c_base = (45, 125, 35, 255)
    blades = [
        [(14, 28), (12, 22), (10, 16), (8, 10)],
        [(16, 28), (16, 20), (16, 12), (16, 6)],
        [(18, 28), (20, 22), (22, 16), (24, 10)],
        [(13, 28), (14, 23), (12, 18)],
        [(19, 28), (19, 23), (21, 18)]
    ]
    for b in blades:
        for idx, (bx, by) in enumerate(b):
            col = c_tip if idx == len(b) - 1 else (c_base if idx == 0 else c_blade)
            pix[bx, by] = col
            if bx + 1 < 32: pix[bx + 1, by] = col
    return img

def gen_tall_reed():
    # 33: foliage_tall_reed
    img = new_img(32, 32); pix = img.load()
    c_stem = (135, 175, 75, 255); c_head = (140, 90, 50, 255)
    for x in [10, 16, 22]:
        for y in range(8, 30): pix[x, y] = c_stem
        for y in range(4, 10): pix[x, y] = c_head; pix[x + 1, y] = c_head
    return img

def gen_tree_oak():
    # 34: tree_oak_lush (32x32 icon block)
    img = new_img(32, 32); pix = img.load()
    c_wood = (95, 60, 35, 255); c_leaf = (55, 160, 68, 255); c_hi = (120, 225, 105, 255)
    for y in range(18, 32):
        for x in range(13, 19): pix[x, y] = c_wood
    for y in range(2, 22):
        for x in range(4, 28):
            if ((x - 16)**2 / 12**2 + (y - 12)**2 / 10**2) <= 1.0:
                pix[x, y] = c_hi if y < 10 else c_leaf
    return img

def gen_tree_apple():
    # 35: tree_apple_village
    img = gen_tree_oak(); pix = img.load()
    apples = [(10, 8), (18, 7), (14, 14), (22, 12)]
    for ax, ay in apples:
        pix[ax, ay] = (245, 45, 55, 255)
    return img

def gen_tree_pine():
    # 36: tree_pine_forest
    img = new_img(32, 32); pix = img.load()
    c_wood = (75, 45, 25, 255); c_needle = (30, 95, 65, 255); c_hi = (65, 155, 105, 255)
    for y in range(24, 32):
        for x in range(14, 18): pix[x, y] = c_wood
    # 3 tầng lá kim tam giác
    for layer in range(3):
        top_y = 4 + layer * 7
        bot_y = 14 + layer * 7
        hw = 3 + layer * 4
        for y in range(top_y, bot_y):
            prog = (y - top_y) / float(bot_y - top_y)
            w = int(hw * prog)
            for x in range(16 - w, 16 + w + 1):
                pix[x, y] = c_hi if x == 16 - w else c_needle
    return img

def gen_tree_dark():
    # 37: tree_dark_corrupted
    img = new_img(32, 32); pix = img.load()
    c_wood = (40, 25, 50, 255); c_leaf = (75, 35, 105, 255); c_glow = (185, 95, 245, 255)
    for y in range(18, 32):
        for x in range(13, 19): pix[x, y] = c_wood
    for y in range(2, 22):
        for x in range(4, 28):
            if ((x - 16)**2 / 12**2 + (y - 12)**2 / 10**2) <= 1.0:
                pix[x, y] = c_glow if (x + y) % 5 == 0 else c_leaf
    return img

def gen_flower_patch():
    # 38: foliage_flower_patch
    img = gen_grass_bright(); pix = img.load()
    for fx in range(4, 28, 5):
        for fy in range(4, 28, 5):
            pix[fx, fy] = (255, 225, 45, 255)
            if fx + 1 < 32: pix[fx + 1, fy] = (255, 180, 30, 255)
    return img

def gen_mushroom_giant():
    # 39: foliage_mushroom_red
    img = new_img(32, 32); pix = img.load()
    c_stem = (235, 235, 220, 255); c_cap = (225, 45, 55, 255); c_dot = (255, 255, 255, 255)
    for y in range(16, 30):
        for x in range(13, 19): pix[x, y] = c_stem
    for y in range(6, 17):
        for x in range(6, 26):
            if ((x - 16)**2 / 10**2 + (y - 15)**2 / 9**2) <= 1.0 and y <= 16:
                pix[x, y] = c_cap
    dots = [(12, 10), (16, 8), (20, 11), (15, 13)]
    for dx, dy in dots: pix[dx, dy] = c_dot
    return img

# -------------------------------------------------------------
# NHÓM 5: ĐẠO CỤ & CHƯỚNG NGẠI VẬT & TƯƠNG TÁC (IDs 40..47)
# -------------------------------------------------------------

def gen_mossy_boulder():
    # 40: prop_mossy_boulder
    img = new_img(32, 32); pix = img.load()
    c_stone = (135, 145, 160, 255); c_moss = (95, 185, 65, 255); c_dark = (65, 75, 85, 255)
    for y in range(6, 28):
        for x in range(4, 28):
            dist = ((x - 16)**2 / 11**2 + (y - 17)**2 / 10**2)
            if dist <= 1.0:
                pix[x, y] = c_dark if dist > 0.8 else (c_moss if y < 14 else c_stone)
    return img

def gen_pebble_cluster():
    # 41: prop_pebble_cluster
    img = new_img(32, 32); pix = img.load()
    c_p = (150, 155, 165, 255); c_d = (90, 95, 105, 255)
    spots = [(8, 12, 3), (20, 16, 4), (12, 22, 2), (24, 24, 3)]
    for sx, sy, r in spots:
        for y in range(sy - r, sy + r + 1):
            for x in range(sx - r, sx + r + 1):
                if ((x - sx)**2 + (y - sy)**2) <= r**2:
                    pix[x, y] = c_d if y > sy else c_p
    return img

def gen_fallen_log():
    # 42: prop_fallen_log
    img = new_img(32, 32); pix = img.load()
    c_bark = (105, 65, 40, 255); c_rings = (185, 145, 95, 255); c_dark = (60, 35, 20, 255)
    for y in range(12, 22):
        for x in range(4, 28):
            pix[x, y] = c_dark if (y == 12 or y == 21) else c_bark
            if x == 4 or x == 27: pix[x, y] = c_rings
    return img

def gen_tree_stump():
    # 43: prop_tree_stump
    img = new_img(32, 32); pix = img.load()
    c_bark = (90, 55, 30, 255); c_top = (175, 135, 85, 255); c_ring = (135, 95, 55, 255)
    for y in range(16, 28):
        for x in range(8, 24): pix[x, y] = c_bark
    for y in range(12, 17):
        for x in range(9, 23):
            pix[x, y] = c_ring if (x == 16 or y == 14) else c_top
    return img

def gen_wooden_fence():
    # 44: prop_wooden_fence
    img = new_img(32, 32); pix = img.load()
    c_wood = (145, 95, 55, 255); c_dark = (95, 60, 35, 255)
    for x in range(32):
        for y in [12, 13, 20, 21]: pix[x, y] = c_wood
    for x in [6, 7, 24, 25]:
        for y in range(6, 28): pix[x, y] = c_wood; pix[x - 1, y] = c_dark
    return img

def gen_torch_post():
    # 45: prop_torch_post
    img = new_img(32, 32); pix = img.load()
    c_wood = (95, 60, 35, 255); c_fire = (255, 185, 40, 255); c_core = (255, 255, 220, 255)
    for y in range(14, 30):
        pix[15, y] = c_wood; pix[16, y] = c_wood
    for y in range(4, 14):
        for x in range(13, 19):
            if ((x - 15.5)**2 + (y - 9)**2) <= 16:
                pix[x, y] = c_core if y > 7 else c_fire
    return img

def gen_waystone_shrine():
    # 46: prop_waystone_shrine
    img = new_img(32, 32); pix = img.load()
    c_stone = (140, 150, 170, 255); c_crystal = (70, 230, 255, 255); c_core = (240, 255, 255, 255)
    for y in range(16, 30):
        for x in range(11, 21): pix[x, y] = c_stone
    for y in range(4, 16):
        for x in range(13, 19):
            if abs(x - 15.5) + abs(y - 10) <= 5:
                pix[x, y] = c_core if abs(x - 15.5) < 1 else c_crystal
    return img

def gen_dungeon_gate():
    # 47: prop_dungeon_gate
    img = new_img(32, 32); pix = img.load()
    c_arch = (95, 85, 105, 255); c_void = (180, 85, 245, 255); c_dark = (25, 15, 35, 255)
    for y in range(4, 30):
        for x in range(4, 28):
            if x < 8 or x > 23 or y < 9: pix[x, y] = c_arch
            else: pix[x, y] = c_void if (x + y) % 4 == 0 else c_dark
    return img

def main():
    registry = {
        # Nhóm 1 (0..11)
        0: ("block_grass_bright.png", gen_grass_bright()),
        1: ("block_grass_flower_yellow.png", gen_grass_flowers((255, 230, 60, 255), (255, 170, 20, 255))),
        2: ("block_grass_flower_blue.png", gen_grass_flowers((100, 190, 255, 255), (40, 110, 240, 255))),
        3: ("block_grass_flower_red.png", gen_grass_flowers((255, 80, 90, 255), (180, 30, 50, 255))),
        4: ("block_grass_flower_white.png", gen_grass_flowers((255, 255, 255, 255), (255, 220, 80, 255))),
        5: ("block_grass_dense_clover.png", gen_grass_dense()),
        6: ("block_grass_autumn.png", gen_grass_autumn()),
        7: ("block_grass_dark_forest.png", gen_grass_dark()),
        8: ("block_dirt_warm_path.png", gen_dirt_path()),
        9: ("block_dirt_packed_road.png", gen_dirt_packed_road()),
        10: ("block_farmland_tilled.png", gen_farmland()),
        11: ("block_mud_swamp.png", gen_mud_swamp()),
        
        # Nhóm 2 (12..19)
        12: ("block_water_shallow.png", gen_water_shallow()),
        13: ("block_water_deep.png", gen_water_deep()),
        14: ("block_water_stream.png", gen_water_stream()),
        15: ("block_waterfall.png", gen_waterfall()),
        16: ("block_sand_shore.png", gen_sand_shore()),
        17: ("block_sand_wet.png", gen_sand_wet()),
        18: ("block_ice_frozen.png", gen_ice_frozen()),
        19: ("block_poison_swamp.png", gen_poison_swamp()),
        
        # Nhóm 3 (20..29)
        20: ("block_cobble_plaza.png", gen_cobble_plaza()),
        21: ("block_cobble_mossy.png", gen_cobble_mossy()),
        22: ("block_stone_dungeon.png", gen_dungeon_tile()),
        23: ("block_stone_wall.png", gen_stone_wall()),
        24: ("block_cliff_wall.png", gen_cliff_wall()),
        25: ("block_wooden_plank.png", gen_wood_plank()),
        26: ("block_wooden_bridge.png", gen_wood_bridge()),
        27: ("block_ruins_pillar.png", gen_ruins_pillar()),
        28: ("block_magic_rune_circle.png", gen_magic_rune_circle()),
        29: ("block_safezone_gold.png", gen_safezone_gold()),
        
        # Nhóm 4 (30..39)
        30: ("block_foliage_stealth_bush.png", gen_stealth_bush()),
        31: ("block_foliage_bush_berries.png", gen_bush_berries()),
        32: ("block_foliage_grass_tuft.png", gen_grass_tuft()),
        33: ("block_foliage_tall_reed.png", gen_tall_reed()),
        34: ("block_tree_oak.png", gen_tree_oak()),
        35: ("block_tree_apple.png", gen_tree_apple()),
        36: ("block_tree_pine.png", gen_tree_pine()),
        37: ("block_tree_dark.png", gen_tree_dark()),
        38: ("block_foliage_flower_patch.png", gen_flower_patch()),
        39: ("block_mushroom_giant.png", gen_mushroom_giant()),
        
        # Nhóm 5 (40..47)
        40: ("block_prop_mossy_boulder.png", gen_mossy_boulder()),
        41: ("block_prop_pebble_cluster.png", gen_pebble_cluster()),
        42: ("block_prop_fallen_log.png", gen_fallen_log()),
        43: ("block_prop_tree_stump.png", gen_tree_stump()),
        44: ("block_prop_wooden_fence.png", gen_wooden_fence()),
        45: ("block_prop_torch_post.png", gen_torch_post()),
        46: ("block_prop_waystone_shrine.png", gen_waystone_shrine()),
        47: ("block_prop_dungeon_gate.png", gen_dungeon_gate())
    }
    
    print(f"Total blocks defined: {len(registry)}")
    for block_id, (filename, img) in registry.items():
        # Lưu vào Resources/Blocks/
        out_path = os.path.join(BLOCKS_DIR, filename)
        img.save(out_path)
        # Đồng thời lưu vào Resources/ để load trực tiếp theo tên rút gọn
        root_out = os.path.join(RESOURCES_DIR, filename.replace(".png", "") + ".png")
        img.save(root_out)
        print(f"Block [{block_id:02d}]: {filename} saved successfully.")

if __name__ == "__main__":
    main()
