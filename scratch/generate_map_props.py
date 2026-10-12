import os
from PIL import Image, ImageDraw

RESOURCES_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources"
os.makedirs(RESOURCES_DIR, exist_ok=True)

# Helper function to create image
def new_img(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))

# 1. TILE: Dirt Path (32x32)
def create_dirt_path():
    img = new_img(32, 32)
    pix = img.load()
    c_base = (142, 108, 72, 255)
    c_dark = (112, 82, 52, 255)
    c_light = (168, 132, 92, 255)
    c_pebble = (100, 105, 115, 255)
    
    for y in range(32):
        for x in range(32):
            v = (x * 7 + y * 13) % 17
            if v < 4:
                pix[x, y] = c_dark
            elif v > 13:
                pix[x, y] = c_light
            else:
                pix[x, y] = c_base
    # Add small pebbles
    pebbles = [(6, 8), (22, 14), (10, 24), (25, 26), (16, 5)]
    for px, py in pebbles:
        pix[px, py] = c_pebble
        if px + 1 < 32: pix[px + 1, py] = (140, 145, 155, 255)
        if py + 1 < 32: pix[px, py + 1] = (60, 65, 75, 255)
    return img

# 2. TILE: Water (32x32)
def create_water_tile():
    img = new_img(32, 32)
    pix = img.load()
    c_deep = (20, 50, 120, 255)
    c_mid = (30, 80, 175, 255)
    c_light = (55, 120, 220, 255)
    c_ripple = (140, 200, 255, 230)
    
    for y in range(32):
        for x in range(32):
            v = (x * 3 + y * 5) % 11
            pix[x, y] = c_light if v > 8 else (c_deep if v < 3 else c_mid)
            
    # Wave ripples
    for rx in range(4, 28, 8):
        ry = (rx * 3) % 24 + 4
        if ry < 30:
            pix[rx, ry] = c_ripple
            if rx + 1 < 32: pix[rx + 1, ry] = c_ripple
            if rx + 2 < 32: pix[rx + 2, ry] = c_light
    return img

# 3. PROP: Village Green Tree (48x64)
def create_village_tree():
    img = new_img(48, 64)
    pix = img.load()
    
    # Colors
    c_leaf_dark = (18, 55, 30, 255)
    c_leaf_mid = (35, 110, 55, 255)
    c_leaf_light = (70, 170, 85, 255)
    c_leaf_high = (120, 215, 115, 255)
    c_wood_dark = (45, 25, 15, 255)
    c_wood_mid = (85, 50, 30, 255)
    c_wood_light = (125, 80, 48, 255)
    
    # Trunk (x=20..27, y=36..63)
    for y in range(36, 64):
        w = 4 + int((y - 36) * 0.15)
        for x in range(24 - w, 24 + w):
            if x == 24 - w or x == 23 + w:
                pix[x, y] = c_wood_dark
            elif x < 23:
                pix[x, y] = c_wood_light
            else:
                pix[x, y] = c_wood_mid
    # Roots
    for rx in range(16, 32):
        if 60 <= 62 < 64: pix[rx, 63] = c_wood_dark

    # Foliage Canopy (Oval clusters around x=24, y=24)
    clusters = [(24, 22, 20, 18), (17, 28, 14, 13), (31, 28, 14, 13), (24, 12, 15, 11)]
    for cx, cy, rx, ry in clusters:
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 48 and 0 <= y < 64:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    dist = dx * dx + dy * dy
                    if dist <= 1.0:
                        # Shading based on position and top-left lighting
                        if dist > 0.85:
                            pix[x, y] = c_leaf_dark
                        elif dy < -0.2 and dx < 0.1:
                            pix[x, y] = c_leaf_high
                        elif dy < 0.2:
                            pix[x, y] = c_leaf_light
                        elif dy > 0.5:
                            pix[x, y] = c_leaf_dark
                        else:
                            pix[x, y] = c_leaf_mid
    return img

# 4. PROP: Dark Corrupted Tree (48x64)
def create_dark_tree():
    img = new_img(48, 64)
    pix = img.load()
    
    c_wood_dark = (25, 15, 30, 255)
    c_wood_mid = (45, 30, 55, 255)
    c_leaf_dark = (30, 15, 45, 255)
    c_leaf_mid = (65, 30, 95, 255)
    c_leaf_light = (115, 55, 160, 255)
    c_leaf_glow = (180, 95, 240, 255)
    
    # Twisted Trunk
    for y in range(28, 64):
        offset = int(2 * ((y % 12) - 6) / 6.0)
        cx = 24 + offset
        for x in range(cx - 3, cx + 4):
            if 0 <= x < 48:
                pix[x, y] = c_wood_dark if (x == cx - 3 or x == cx + 3) else c_wood_mid
                
    # Gnarled bare branches & purple spore foliage
    clusters = [(24, 18, 18, 14), (15, 24, 12, 10), (33, 22, 12, 10)]
    for cx, cy, rx, ry in clusters:
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 48 and 0 <= y < 64:
                    dx = (x - cx) / float(rx)
                    dy = (y - cy) / float(ry)
                    dist = dx * dx + dy * dy
                    if dist <= 0.95:
                        if dist > 0.8:
                            pix[x, y] = c_leaf_dark
                        elif dy < -0.2:
                            pix[x, y] = c_leaf_glow if (x + y) % 5 == 0 else c_leaf_light
                        else:
                            pix[x, y] = c_leaf_mid
    return img

# 5. PROP: Torch Post (16x32)
def create_torch_post():
    img = new_img(16, 32)
    pix = img.load()
    
    c_wood_dark = (50, 30, 15, 255)
    c_wood_mid = (90, 55, 30, 255)
    c_iron = (55, 60, 70, 255)
    c_fire_core = (255, 255, 220, 255)
    c_fire_mid = (255, 180, 30, 255)
    c_fire_outer = (240, 60, 20, 255)
    c_fire_smoke = (120, 50, 20, 180)
    
    # Post (x=7..8, y=12..31)
    for y in range(12, 32):
        pix[6, y] = c_wood_dark
        pix[7, y] = c_wood_mid
        pix[8, y] = c_wood_dark
        pix[9, y] = (30, 18, 10, 255)
        
    # Iron brazier bracket (x=4..11, y=10..13)
    for x in range(4, 12):
        pix[x, 11] = c_iron
    pix[4, 10] = c_iron; pix[11, 10] = c_iron
    pix[5, 12] = c_iron; pix[10, 12] = c_iron
    
    # Flame (x=5..10, y=2..10)
    flame_coords = [
        (7, 2, c_fire_mid), (8, 2, c_fire_smoke),
        (7, 3, c_fire_core), (8, 3, c_fire_mid), (6, 4, c_fire_outer),
        (7, 5, c_fire_core), (8, 5, c_fire_core), (9, 5, c_fire_outer),
        (6, 6, c_fire_mid), (7, 6, c_fire_core), (8, 6, c_fire_core), (9, 6, c_fire_mid),
        (5, 7, c_fire_outer), (6, 7, c_fire_mid), (7, 7, c_fire_core), (8, 7, c_fire_core), (9, 7, c_fire_mid), (10, 7, c_fire_outer),
        (6, 8, c_fire_mid), (7, 8, c_fire_core), (8, 8, c_fire_core), (9, 8, c_fire_mid),
        (6, 9, c_fire_outer), (7, 9, c_fire_mid), (8, 9, c_fire_mid), (9, 9, c_fire_outer)
    ]
    for fx, fy, col in flame_coords:
        pix[fx, fy] = col
    return img

# 6. PROP: Stone Well (32x32)
def create_stone_well():
    img = new_img(32, 32)
    pix = img.load()
    c_roof = (160, 60, 40, 255)
    c_roof_hi = (210, 95, 65, 255)
    c_roof_dark = (100, 35, 25, 255)
    c_wood = (90, 55, 30, 255)
    c_stone = (110, 115, 125, 255)
    c_stone_hi = (155, 160, 175, 255)
    c_stone_dark = (60, 65, 75, 255)
    c_water = (30, 90, 180, 255)
    
    # Roof (y=2..8, x=4..27)
    for y in range(2, 9):
        w = 12 - (y - 2)
        for x in range(16 - w, 16 + w):
            pix[x, y] = c_roof_hi if y == 2 or x < 15 else (c_roof_dark if y == 8 else c_roof)
            
    # Wooden posts (x=7, 24, y=9..19)
    for y in range(9, 20):
        pix[7, y] = c_wood; pix[24, y] = c_wood
        
    # Stone rim (y=19..31, x=5..26)
    for y in range(19, 31):
        for x in range(5, 27):
            dx = (x - 15.5) / 10.5
            dy = (y - 24.5) / 5.5
            dist = dx * dx + dy * dy
            if dist <= 1.0:
                if dist <= 0.45 and y < 26:
                    pix[x, y] = c_water
                elif dist > 0.8 or y == 30:
                    pix[x, y] = c_stone_dark
                elif y == 19 or dx < -0.4:
                    pix[x, y] = c_stone_hi
                else:
                    pix[x, y] = c_stone
    return img

# 7. PROP: Wooden Fence (32x16)
def create_wooden_fence():
    img = new_img(32, 16)
    pix = img.load()
    c_dark = (50, 30, 15, 255)
    c_mid = (95, 60, 32, 255)
    c_light = (135, 90, 50, 255)
    
    # Horizontal rails (y=5..6 and y=11..12)
    for x in range(32):
        pix[x, 5] = c_light; pix[x, 6] = c_dark
        pix[x, 11] = c_light; pix[x, 12] = c_dark
        
    # 4 Vertical posts with pointed tops
    posts = [2, 10, 18, 26]
    for px in posts:
        # Pointed top
        pix[px + 1, 1] = c_light
        pix[px, 2] = c_light; pix[px + 1, 2] = c_mid; pix[px + 2, 2] = c_dark
        for y in range(3, 16):
            pix[px, y] = c_light
            pix[px + 1, y] = c_mid
            pix[px + 2, y] = c_dark
    return img

# 8. PROP: Dungeon Arch Portal (48x64)
def create_dungeon_arch():
    img = new_img(48, 64)
    pix = img.load()
    
    c_stone = (70, 75, 90, 255)
    c_stone_hi = (110, 115, 135, 255)
    c_stone_dark = (35, 40, 50, 255)
    c_void_black = (10, 5, 20, 255)
    c_void_purple = (75, 20, 120, 255)
    c_rune_glow = (180, 80, 255, 255)
    
    # Arch outline
    for y in range(8, 64):
        for x in range(6, 42):
            dx = (x - 23.5) / 16.0
            dy = (y - 28.0) / 24.0
            
            # Arch top curve
            if y < 28:
                curve_dist = (x - 23.5)**2 / (15.5**2) + (y - 28.0)**2 / (20.0**2)
            else:
                curve_dist = (abs(x - 23.5) / 15.5)**2
                
            if curve_dist <= 1.0:
                # Inside void portal
                inner_dist = (abs(x - 23.5) / 10.5)**2 + ((y - 32.0) / 22.0)**2 if y < 32 else (abs(x - 23.5) / 10.5)**2
                if inner_dist <= 0.85:
                    # Swirling vortex
                    v = (x * 4 + y * 7) % 9
                    pix[x, y] = c_rune_glow if v == 0 else (c_void_purple if v < 4 else c_void_black)
                else:
                    # Stone inner bevel
                    pix[x, y] = c_stone_dark
            elif curve_dist <= 1.55:
                # Stone arch structure
                if x < 12 or y < 14:
                    pix[x, y] = c_stone_hi
                elif x > 36:
                    pix[x, y] = c_stone_dark
                else:
                    pix[x, y] = c_stone
                    
    # Carved Glowing Runes on pillars
    runes = [(9, 24), (9, 36), (9, 48), (38, 24), (38, 36), (38, 48), (24, 11)]
    for rx, ry in runes:
        pix[rx, ry] = c_rune_glow
        if rx + 1 < 48: pix[rx + 1, ry] = (220, 160, 255, 255)
    return img

def main():
    assets = {
        "tile_dirt_path.png": create_dirt_path(),
        "tile_water.png": create_water_tile(),
        "prop_tree_village.png": create_village_tree(),
        "prop_tree_dark.png": create_dark_tree(),
        "prop_torch_post.png": create_torch_post(),
        "prop_stone_well.png": create_stone_well(),
        "prop_wooden_fence.png": create_wooden_fence(),
        "prop_dungeon_gate.png": create_dungeon_arch()
    }
    
    for filename, img in assets.items():
        out_path = os.path.join(RESOURCES_DIR, filename)
        img.save(out_path)
        print(f"Generated {filename} ({img.size})")

if __name__ == "__main__":
    main()
