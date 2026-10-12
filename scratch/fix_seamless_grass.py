import os
import math
from PIL import Image

RESOURCES_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources"
BLOCKS_DIR = os.path.join(RESOURCES_DIR, "Blocks")

def new_img(w, h):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))

def create_seamless_grass(base_rgb, var_rgb, seed=1):
    w, h = 32, 32
    img = new_img(w, h)
    pix = img.load()
    
    # Sử dụng hàm lượng giác tuần hoàn (toroidal sin/cos) để biên trái trùng biên phải, biên trên trùng biên dưới 100%
    for y in range(h):
        for x in range(w):
            # Tọa độ góc tuần hoàn
            ang_x = 2 * math.pi * x / w
            ang_y = 2 * math.pi * y / h
            
            # Sóng tuần hoàn bậc 1, 2, 4
            s1 = math.sin(ang_x * 1 + seed) * math.cos(ang_y * 1 + seed * 1.5)
            s2 = math.sin(ang_x * 2 - seed * 0.7) * math.sin(ang_y * 2 + seed) * 0.5
            s3 = math.cos(ang_x * 4 + ang_y * 2) * 0.25
            
            val = (s1 + s2 + s3) / 1.75 # [-1..1]
            val = (val + 1.0) / 2.0     # [0..1]
            
            # Interpolate giữa base_rgb và var_rgb rất êm dịu, không chênh lệch quá 12-15 đơn vị RGB
            r = int(base_rgb[0] + (var_rgb[0] - base_rgb[0]) * val)
            g = int(base_rgb[1] + (var_rgb[1] - base_rgb[1]) * val)
            b = int(base_rgb[2] + (var_rgb[2] - base_rgb[2]) * val)
            
            pix[x, y] = (r, g, b, 255)
            
    return img

def main():
    # Màu cỏ xanh tươi sáng đồng nhất, chuyển màu cực kỳ êm dịu không bị đốm tách biệt
    c_base = (118, 206, 76)
    c_light = (130, 218, 86)
    
    # 0: Cỏ tươi sáng chuẩn (seamless 100%)
    grass_bright = create_seamless_grass(c_base, c_light, seed=42)
    
    # Lưu đè cả tile_grass.png và block_grass_bright.png
    targets = [
        os.path.join(RESOURCES_DIR, "tile_grass.png"),
        os.path.join(RESOURCES_DIR, "block_grass_bright.png"),
        os.path.join(BLOCKS_DIR, "block_grass_bright.png")
    ]
    for p in targets:
        grass_bright.save(p)
        print(f"Saved seamless grass to {p}")

if __name__ == "__main__":
    main()
