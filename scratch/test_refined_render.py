import os
import math
from PIL import Image, ImageDraw

def draw_refined_character(gender="male", direction="south", frame=0):
    img = Image.new("RGBA", (160, 160), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # Palettes
    if gender == "male":
        c_white = (255, 255, 255, 255)
        c_light = (240, 245, 252, 255)
        c_mid = (212, 225, 240, 255)
        c_shadow = (165, 185, 210, 255)
        c_dark = (120, 145, 175, 255)
        
        t_glow = (180, 255, 255, 255)
        t_light = (0, 235, 255, 255)
        t_main = (0, 150, 255, 255)
        t_shadow = (0, 75, 195, 255)
        t_dark = (0, 35, 120, 255)

        skin_base = (255, 224, 202, 255)
        skin_light = (255, 242, 232, 255)
        skin_shadow = (225, 176, 150, 255)

        hair_main = (30, 42, 68, 255)
        hair_light = (55, 78, 120, 255)
        hair_shadow = (18, 26, 45, 255)

        eye_main = (0, 215, 255, 255)
        eye_dark = (0, 55, 125, 255)
        
        brooch_base = (255, 215, 0, 255)
        brooch_gem = (0, 240, 255, 255)
        
        tunic_main = (22, 28, 44, 255)
        belt_gold = (235, 195, 45, 255)
        boot_main = (28, 38, 55, 255)
        boot_highlight = (45, 60, 85, 255)
        outline = (12, 16, 28, 255)
    else:
        c_white = (255, 255, 255, 255)
        c_light = (255, 246, 250, 255)
        c_mid = (250, 226, 238, 255)
        c_shadow = (232, 182, 205, 255)
        c_dark = (188, 130, 160, 255)
        
        t_glow = (255, 210, 230, 255)
        t_light = (255, 130, 185, 255)
        t_main = (255, 45, 130, 255)
        t_shadow = (195, 18, 88, 255)
        t_dark = (125, 10, 55, 255)

        skin_base = (255, 226, 215, 255)
        skin_light = (255, 248, 244, 255)
        skin_shadow = (232, 185, 172, 255)
        blush = (255, 135, 160, 200)

        hair_main = (255, 215, 125, 255)
        hair_light = (255, 242, 175, 255)
        hair_shadow = (218, 162, 75, 255)

        eye_main = (255, 30, 125, 255)
        eye_dark = (105, 10, 52, 255)
        
        brooch_base = (255, 192, 203, 255)
        brooch_gem = (255, 20, 80, 255)
        
        tunic_main = (42, 25, 42, 255)
        belt_gold = (255, 215, 110, 255)
        boot_main = (52, 32, 50, 255)
        boot_highlight = (80, 50, 75, 255)
        outline = (25, 12, 22, 255)

    # Walk cycle offsets
    bob = 0
    stride_l = 0
    stride_r = 0
    flare = 0
    sway = 0

    if frame in [1, 5]:
        bob = 2
    elif frame in [2, 6]:
        bob = -1
    
    if frame == 1:
        stride_l = 4; stride_r = -2; flare = 2; sway = -2
    elif frame == 2:
        stride_l = 8; stride_r = -5; flare = 6; sway = -4
    elif frame == 3:
        stride_l = 4; stride_r = -2; flare = 3; sway = -2
    elif frame == 5:
        stride_r = 4; stride_l = -2; flare = 2; sway = 2
    elif frame == 6:
        stride_r = 8; stride_l = -5; flare = 6; sway = 4
    elif frame == 7:
        stride_r = 4; stride_l = -2; flare = 3; sway = 2

    cx = 80
    cy = 80 + bob

    # ================= 1. SOUTH (FRONT VIEW) =================
    if direction == "south":
        # Boots
        boot_y = 144 + bob
        # Left boot
        d.rounded_rectangle([cx - 18, boot_y + stride_l, cx - 6, boot_y + 10 + stride_l], radius=3, fill=boot_main, outline=outline)
        d.line([(cx - 17, boot_y + 1 + stride_l), (cx - 7, boot_y + 1 + stride_l)], fill=boot_highlight, width=1)
        # Right boot
        d.rounded_rectangle([cx + 6, boot_y + stride_r, cx + 18, boot_y + 10 + stride_r], radius=3, fill=boot_main, outline=outline)
        d.line([(cx + 7, boot_y + 1 + stride_r), (cx + 17, boot_y + 1 + stride_r)], fill=boot_highlight, width=1)

        # Back cloak body (widens down to hem)
        top_y = 66 + bob
        hem_y = 140 + bob
        hl = cx - 44 - flare + sway
        hr = cx + 44 + flare + sway
        
        # Cloak silhouette
        cloak_back = [
            (cx, top_y - 4),
            (cx + 26, top_y + 2),
            (hr, hem_y),
            (cx + 22 + sway, hem_y - 5),
            (cx + sway, hem_y - 2),
            (cx - 22 + sway, hem_y - 5),
            (hl, hem_y),
            (cx - 26, top_y + 2)
        ]
        d.polygon(cloak_back, fill=c_shadow, outline=outline)
        d.polygon([
            (cx, top_y - 2),
            (cx + 24, top_y + 4),
            (hr - 3, hem_y - 2),
            (cx + 20 + sway, hem_y - 6),
            (cx + sway, hem_y - 4),
            (cx - 20 + sway, hem_y - 6),
            (hl + 3, hem_y - 2),
            (cx - 24, top_y + 4)
        ], fill=c_mid)

        # Inner robe / tunic
        tunic_w = 14
        d.rectangle([cx - tunic_w, top_y + 4, cx + tunic_w, hem_y - 6], fill=tunic_main, outline=outline)
        # Tunic gold belt
        belt_y = top_y + 32
        d.rectangle([cx - tunic_w, belt_y, cx + tunic_w, belt_y + 6], fill=belt_gold, outline=outline)
        d.rectangle([cx - 4, belt_y - 1, cx + 4, belt_y + 7], fill=brooch_gem)

        # Left & Right main front drapes
        d.polygon([(cx - 26, top_y + 2), (cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2), (hl, hem_y)], fill=c_light, outline=outline)
        d.polygon([(cx + 26, top_y + 2), (cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2), (hr, hem_y)], fill=c_white, outline=outline)

        # Cloak shading folds
        d.line([(cx - 20, top_y + 12), (cx - 28 + sway, hem_y - 4)], fill=c_shadow, width=2)
        d.line([(cx + 20, top_y + 12), (cx + 28 + sway, hem_y - 4)], fill=c_mid, width=2)

        # Colored Trim along front opening
        d.line([(cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2)], fill=t_main, width=3)
        d.line([(cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2)], fill=t_light, width=1)
        d.line([(cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2)], fill=t_main, width=3)
        d.line([(cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2)], fill=t_light, width=1)

        # Bottom Hem Colored Trim (curving majestically)
        hem_pts = [(hl, hem_y), (cx - 22 + sway, hem_y - 5), (cx + sway, hem_y - 2), (cx + 22 + sway, hem_y - 5), (hr, hem_y)]
        d.line(hem_pts, fill=t_shadow, width=4)
        d.line(hem_pts, fill=t_main, width=2)
        d.line(hem_pts, fill=t_glow, width=1)

        # Shoulder Capelet (Mantle layer on shoulders)
        capelet_pts = [
            (cx, top_y - 2),
            (cx + 28, top_y + 4),
            (cx + 30, top_y + 22),
            (cx + 14, top_y + 24),
            (cx, top_y + 14),
            (cx - 14, top_y + 24),
            (cx - 30, top_y + 22),
            (cx - 28, top_y + 4)
        ]
        d.polygon(capelet_pts, fill=c_white, outline=outline)
        # Capelet colored trim
        c_trim_pts = [(cx - 30, top_y + 22), (cx - 14, top_y + 24), (cx, top_y + 14), (cx + 14, top_y + 24), (cx + 30, top_y + 22)]
        d.line(c_trim_pts, fill=t_main, width=3)
        d.line(c_trim_pts, fill=t_light, width=1)

        # Brooch at collar
        brooch_y = top_y + 4
        d.ellipse([cx - 7, brooch_y - 5, cx + 7, brooch_y + 5], fill=brooch_base, outline=outline, width=1)
        d.ellipse([cx - 4, brooch_y - 3, cx + 4, brooch_y + 3], fill=brooch_gem)
        d.point((cx - 1, brooch_y - 2), fill=(255, 255, 255, 255))

        # Hood
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_w = 32

        hood_outer = [
            (cx, hood_top - 5), # top fantasy peak
            (cx + 18, hood_top - 1),
            (cx + hood_w + 1, hood_top + 16),
            (cx + hood_w + 3, hood_top + 34),
            (cx + hood_w - 4, hood_bot - 4),
            (cx + 16, hood_bot + 2),
            (cx, hood_bot + 5),
            (cx - 16, hood_bot + 2),
            (cx - hood_w + 4, hood_bot - 4),
            (cx - hood_w - 3, hood_top + 34),
            (cx - hood_w - 1, hood_top + 16),
            (cx - 18, hood_top - 1)
        ]
        d.polygon(hood_outer, fill=c_white, outline=outline)
        d.line([(cx - 18, hood_top - 1), (cx, hood_top - 5), (cx + 18, hood_top - 1)], fill=c_white, width=2)
        d.line([(cx - hood_w - 2, hood_top + 30), (cx - 16, hood_bot)], fill=c_mid, width=2)

        # Hood face opening with colored trim
        open_w = 21
        open_top = hood_top + 12
        open_bot = hood_bot - 6
        # Outer trim
        d.rounded_rectangle([cx - open_w - 2, open_top - 2, cx + open_w + 2, open_bot + 2], radius=10, outline=t_shadow, width=4)
        d.rounded_rectangle([cx - open_w - 1, open_top - 1, cx + open_w + 1, open_bot + 1], radius=10, outline=t_main, width=2)
        d.rounded_rectangle([cx - open_w, open_top, cx + open_w, open_bot], radius=10, outline=t_glow, width=1)
        # Inner dark cavity
        d.rounded_rectangle([cx - open_w + 1, open_top + 1, cx + open_w - 1, open_bot - 1], radius=9, fill=hair_shadow)

        # Face skin
        face_x = cx - 18
        face_w = 36
        face_y = open_top + 8
        face_h = 32
        d.rounded_rectangle([face_x, face_y, face_x + face_w, face_y + face_h], radius=7, fill=skin_base)
        d.chord([face_x + 4, face_y + 18, face_x + face_w - 4, face_y + face_h], 0, 180, fill=skin_shadow)

        # Hair & Bangs
        if gender == "male":
            # Cool styled bangs
            d.polygon([(face_x + 2, face_y), (face_x + 12, face_y + 14), (face_x + 8, face_y)], fill=hair_main)
            d.polygon([(face_x + 8, face_y), (face_x + 18, face_y + 17), (face_x + 15, face_y)], fill=hair_light)
            d.polygon([(face_x + 16, face_y), (face_x + 26, face_y + 15), (face_x + 23, face_y)], fill=hair_main)
            d.polygon([(face_x + 23, face_y), (face_x + 33, face_y + 11), (face_x + 34, face_y)], fill=hair_shadow)
        else:
            # Cute blonde bangs & side locks
            d.polygon([(face_x + 1, face_y), (face_x + 9, face_y + 15), (face_x + 7, face_y)], fill=hair_main)
            d.polygon([(face_x + 6, face_y), (face_x + 18, face_y + 18), (face_x + 15, face_y)], fill=hair_light)
            d.polygon([(face_x + 14, face_y), (face_x + 26, face_y + 18), (face_x + 23, face_y)], fill=hair_light)
            d.polygon([(face_x + 22, face_y), (face_x + 33, face_y + 15), (face_x + 35, face_y)], fill=hair_main)
            # Side locks
            d.line([(face_x + 2, face_y + 6), (face_x + 2, face_y + 24)], fill=hair_main, width=2)
            d.line([(face_x + face_w - 3, face_y + 6), (face_x + face_w - 3, face_y + 24)], fill=hair_main, width=2)
            # Rosy blush
            d.ellipse([face_x + 4, face_y + 20, face_x + 10, face_y + 24], fill=blush)
            d.ellipse([face_x + face_w - 10, face_y + 20, face_x + face_w - 4, face_y + 24], fill=blush)

        # Eyes (expressive fantasy eyes with shine)
        eye_y = face_y + 13
        # Left eye
        d.rounded_rectangle([face_x + 6, eye_y, face_x + 13, eye_y + 7], radius=2, fill=eye_dark)
        d.rounded_rectangle([face_x + 7, eye_y + 1, face_x + 13, eye_y + 6], radius=2, fill=eye_main)
        d.rectangle([face_x + 7, eye_y + 1, face_x + 9, eye_y + 3], fill=(255, 255, 255, 255)) # catchlight
        d.line([(face_x + 5, eye_y - 1), (face_x + 14, eye_y - 1)], fill=outline, width=1) # lash

        # Right eye
        d.rounded_rectangle([face_x + face_w - 13, eye_y, face_x + face_w - 6, eye_y + 7], radius=2, fill=eye_dark)
        d.rounded_rectangle([face_x + face_w - 13, eye_y + 1, face_x + face_w - 7, eye_y + 6], radius=2, fill=eye_main)
        d.rectangle([face_x + face_w - 13, eye_y + 1, face_x + face_w - 11, eye_y + 3], fill=(255, 255, 255, 255))
        d.line([(face_x + face_w - 14, eye_y - 1), (face_x + face_w - 5, eye_y - 1)], fill=outline, width=1)

        # Nose & cute smile
        d.point((cx, eye_y + 9), fill=skin_shadow)
        d.line([(cx - 2, eye_y + 13), (cx + 2, eye_y + 13)], fill=skin_shadow, width=1)

    return img

os.makedirs("scratch/preview2", exist_ok=True)
m = draw_refined_character("male", "south", 0)
f = draw_refined_character("female", "south", 0)
m.save("scratch/preview2/male_south_0.png")
f.save("scratch/preview2/female_south_0.png")
print("Saved refined South frames successfully.")
