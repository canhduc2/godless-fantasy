import os
import math
from PIL import Image, ImageDraw, ImageOps

def get_palette(gender="male"):
    if gender == "male":
        return {
            "c_white": (255, 255, 255, 255),
            "c_light": (240, 245, 252, 255),
            "c_mid": (212, 225, 240, 255),
            "c_shadow": (165, 185, 210, 255),
            "c_dark": (120, 145, 175, 255),
            
            "t_glow": (180, 255, 255, 255),
            "t_light": (0, 235, 255, 255),
            "t_main": (0, 150, 255, 255),
            "t_shadow": (0, 75, 195, 255),
            "t_dark": (0, 35, 120, 255),

            "skin_base": (255, 224, 202, 255),
            "skin_light": (255, 242, 232, 255),
            "skin_shadow": (225, 176, 150, 255),
            "blush": (255, 200, 190, 0),

            "hair_main": (30, 42, 68, 255),
            "hair_light": (55, 78, 120, 255),
            "hair_shadow": (18, 26, 45, 255),

            "eye_main": (0, 215, 255, 255),
            "eye_dark": (0, 55, 125, 255),
            
            "brooch_base": (255, 215, 0, 255),
            "brooch_gem": (0, 240, 255, 255),
            
            "tunic_main": (22, 28, 44, 255),
            "belt_gold": (235, 195, 45, 255),
            "boot_main": (28, 38, 55, 255),
            "boot_highlight": (45, 60, 85, 255),
            "outline": (12, 16, 28, 255)
        }
    else:
        return {
            "c_white": (255, 255, 255, 255),
            "c_light": (255, 246, 250, 255),
            "c_mid": (250, 226, 238, 255),
            "c_shadow": (232, 182, 205, 255),
            "c_dark": (188, 130, 160, 255),
            
            "t_glow": (255, 210, 230, 255),
            "t_light": (255, 130, 185, 255),
            "t_main": (255, 45, 130, 255),
            "t_shadow": (195, 18, 88, 255),
            "t_dark": (125, 10, 55, 255),

            "skin_base": (255, 226, 215, 255),
            "skin_light": (255, 248, 244, 255),
            "skin_shadow": (232, 185, 172, 255),
            "blush": (255, 135, 160, 200),

            "hair_main": (255, 215, 125, 255),
            "hair_light": (255, 242, 175, 255),
            "hair_shadow": (218, 162, 75, 255),

            "eye_main": (255, 30, 125, 255),
            "eye_dark": (105, 10, 52, 255),
            
            "brooch_base": (255, 192, 203, 255),
            "brooch_gem": (255, 20, 80, 255),
            
            "tunic_main": (42, 25, 42, 255),
            "belt_gold": (255, 215, 110, 255),
            "boot_main": (52, 32, 50, 255),
            "boot_highlight": (80, 50, 75, 255),
            "outline": (25, 12, 22, 255)
        }

def draw_character_frame(gender="male", direction="south", frame=0):
    p = get_palette(gender)
    img = Image.new("RGBA", (160, 160), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # Frame motion dynamics
    bob = 0
    stride_l = 0
    stride_r = 0
    flare = 0
    sway = 0

    if frame in [1, 5]:
        bob = 2
    elif frame in [2, 6]:
        bob = -1
    elif frame == 4:
        bob = 1

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

    # Check mirror directions
    if direction == "east":
        sub = draw_character_frame(gender, "west", frame)
        return ImageOps.mirror(sub)
    elif direction == "south_east":
        sub = draw_character_frame(gender, "south_west", frame)
        return ImageOps.mirror(sub)
    elif direction == "north_east":
        sub = draw_character_frame(gender, "north_west", frame)
        return ImageOps.mirror(sub)

    # ---------------- 1. SOUTH (FRONT) ----------------
    if direction == "south":
        boot_y = 144 + bob
        # Left boot
        d.rounded_rectangle([cx - 18, boot_y + stride_l, cx - 6, boot_y + 10 + stride_l], radius=3, fill=p["boot_main"], outline=p["outline"])
        d.line([(cx - 17, boot_y + 1 + stride_l), (cx - 7, boot_y + 1 + stride_l)], fill=p["boot_highlight"], width=1)
        # Right boot
        d.rounded_rectangle([cx + 6, boot_y + stride_r, cx + 18, boot_y + 10 + stride_r], radius=3, fill=p["boot_main"], outline=p["outline"])
        d.line([(cx + 7, boot_y + 1 + stride_r), (cx + 17, boot_y + 1 + stride_r)], fill=p["boot_highlight"], width=1)

        # Back cloak body
        top_y = 66 + bob
        hem_y = 140 + bob
        hl = cx - 44 - flare + sway
        hr = cx + 44 + flare + sway
        
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
        d.polygon(cloak_back, fill=p["c_shadow"], outline=p["outline"])
        d.polygon([
            (cx, top_y - 2),
            (cx + 24, top_y + 4),
            (hr - 3, hem_y - 2),
            (cx + 20 + sway, hem_y - 6),
            (cx + sway, hem_y - 4),
            (cx - 20 + sway, hem_y - 6),
            (hl + 3, hem_y - 2),
            (cx - 24, top_y + 4)
        ], fill=p["c_mid"])

        # Inner robe / tunic
        tunic_w = 14
        d.rectangle([cx - tunic_w, top_y + 4, cx + tunic_w, hem_y - 6], fill=p["tunic_main"], outline=p["outline"])
        belt_y = top_y + 32
        d.rectangle([cx - tunic_w, belt_y, cx + tunic_w, belt_y + 6], fill=p["belt_gold"], outline=p["outline"])
        d.rectangle([cx - 4, belt_y - 1, cx + 4, belt_y + 7], fill=p["brooch_gem"])

        # Left & Right front drapes
        d.polygon([(cx - 26, top_y + 2), (cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2), (hl, hem_y)], fill=p["c_light"], outline=p["outline"])
        d.polygon([(cx + 26, top_y + 2), (cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2), (hr, hem_y)], fill=p["c_white"], outline=p["outline"])

        d.line([(cx - 20, top_y + 12), (cx - 28 + sway, hem_y - 4)], fill=p["c_shadow"], width=2)
        d.line([(cx + 20, top_y + 12), (cx + 28 + sway, hem_y - 4)], fill=p["c_mid"], width=2)

        # Colored Trim along front opening
        d.line([(cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2)], fill=p["t_main"], width=3)
        d.line([(cx - 4, top_y + 6), (cx - 12 + sway, hem_y - 2)], fill=p["t_light"], width=1)
        d.line([(cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2)], fill=p["t_main"], width=3)
        d.line([(cx + 4, top_y + 6), (cx + 12 + sway, hem_y - 2)], fill=p["t_light"], width=1)

        # Bottom Hem Colored Trim
        hem_pts = [(hl, hem_y), (cx - 22 + sway, hem_y - 5), (cx + sway, hem_y - 2), (cx + 22 + sway, hem_y - 5), (hr, hem_y)]
        d.line(hem_pts, fill=p["t_shadow"], width=4)
        d.line(hem_pts, fill=p["t_main"], width=2)
        d.line(hem_pts, fill=p["t_glow"], width=1)

        # Shoulder Capelet
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
        d.polygon(capelet_pts, fill=p["c_white"], outline=p["outline"])
        c_trim_pts = [(cx - 30, top_y + 22), (cx - 14, top_y + 24), (cx, top_y + 14), (cx + 14, top_y + 24), (cx + 30, top_y + 22)]
        d.line(c_trim_pts, fill=p["t_main"], width=3)
        d.line(c_trim_pts, fill=p["t_light"], width=1)

        # Brooch at collar
        brooch_y = top_y + 4
        d.ellipse([cx - 7, brooch_y - 5, cx + 7, brooch_y + 5], fill=p["brooch_base"], outline=p["outline"], width=1)
        d.ellipse([cx - 4, brooch_y - 3, cx + 4, brooch_y + 3], fill=p["brooch_gem"])
        d.point((cx - 1, brooch_y - 2), fill=(255, 255, 255, 255))

        # Hood
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_w = 32

        hood_outer = [
            (cx, hood_top - 5),
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
        d.polygon(hood_outer, fill=p["c_white"], outline=p["outline"])
        d.line([(cx - 18, hood_top - 1), (cx, hood_top - 5), (cx + 18, hood_top - 1)], fill=p["c_white"], width=2)
        d.line([(cx - hood_w - 2, hood_top + 30), (cx - 16, hood_bot)], fill=p["c_mid"], width=2)

        # Hood face opening with colored trim
        open_w = 21
        open_top = hood_top + 12
        open_bot = hood_bot - 6
        d.rounded_rectangle([cx - open_w - 2, open_top - 2, cx + open_w + 2, open_bot + 2], radius=10, outline=p["t_shadow"], width=4)
        d.rounded_rectangle([cx - open_w - 1, open_top - 1, cx + open_w + 1, open_bot + 1], radius=10, outline=p["t_main"], width=2)
        d.rounded_rectangle([cx - open_w, open_top, cx + open_w, open_bot], radius=10, outline=p["t_glow"], width=1)
        d.rounded_rectangle([cx - open_w + 1, open_top + 1, cx + open_w - 1, open_bot - 1], radius=9, fill=p["hair_shadow"])

        # Face skin
        face_x = cx - 18
        face_w = 36
        face_y = open_top + 8
        face_h = 32
        d.rounded_rectangle([face_x, face_y, face_x + face_w, face_y + face_h], radius=7, fill=p["skin_base"])
        d.chord([face_x + 4, face_y + 18, face_x + face_w - 4, face_y + face_h], 0, 180, fill=p["skin_shadow"])

        # Hair & Bangs
        if gender == "male":
            d.polygon([(face_x + 2, face_y), (face_x + 12, face_y + 14), (face_x + 8, face_y)], fill=p["hair_main"])
            d.polygon([(face_x + 8, face_y), (face_x + 18, face_y + 17), (face_x + 15, face_y)], fill=p["hair_light"])
            d.polygon([(face_x + 16, face_y), (face_x + 26, face_y + 15), (face_x + 23, face_y)], fill=p["hair_main"])
            d.polygon([(face_x + 23, face_y), (face_x + 33, face_y + 11), (face_x + 34, face_y)], fill=p["hair_shadow"])
        else:
            d.polygon([(face_x + 1, face_y), (face_x + 9, face_y + 15), (face_x + 7, face_y)], fill=p["hair_main"])
            d.polygon([(face_x + 6, face_y), (face_x + 18, face_y + 18), (face_x + 15, face_y)], fill=p["hair_light"])
            d.polygon([(face_x + 14, face_y), (face_x + 26, face_y + 18), (face_x + 23, face_y)], fill=p["hair_light"])
            d.polygon([(face_x + 22, face_y), (face_x + 33, face_y + 15), (face_x + 35, face_y)], fill=p["hair_main"])
            d.line([(face_x + 2, face_y + 6), (face_x + 2, face_y + 24)], fill=p["hair_main"], width=2)
            d.line([(face_x + face_w - 3, face_y + 6), (face_x + face_w - 3, face_y + 24)], fill=p["hair_main"], width=2)
            d.ellipse([face_x + 4, face_y + 20, face_x + 10, face_y + 24], fill=p["blush"])
            d.ellipse([face_x + face_w - 10, face_y + 20, face_x + face_w - 4, face_y + 24], fill=p["blush"])

        # Eyes
        eye_y = face_y + 13
        d.rounded_rectangle([face_x + 6, eye_y, face_x + 13, eye_y + 7], radius=2, fill=p["eye_dark"])
        d.rounded_rectangle([face_x + 7, eye_y + 1, face_x + 13, eye_y + 6], radius=2, fill=p["eye_main"])
        d.rectangle([face_x + 7, eye_y + 1, face_x + 9, eye_y + 3], fill=(255, 255, 255, 255))
        d.line([(face_x + 5, eye_y - 1), (face_x + 14, eye_y - 1)], fill=p["outline"], width=1)

        d.rounded_rectangle([face_x + face_w - 13, eye_y, face_x + face_w - 6, eye_y + 7], radius=2, fill=p["eye_dark"])
        d.rounded_rectangle([face_x + face_w - 13, eye_y + 1, face_x + face_w - 7, eye_y + 6], radius=2, fill=p["eye_main"])
        d.rectangle([face_x + face_w - 13, eye_y + 1, face_x + face_w - 11, eye_y + 3], fill=(255, 255, 255, 255))
        d.line([(face_x + face_w - 14, eye_y - 1), (face_x + face_w - 5, eye_y - 1)], fill=p["outline"], width=1)

        # Nose & smile
        d.point((cx, eye_y + 9), fill=p["skin_shadow"])
        d.line([(cx - 2, eye_y + 13), (cx + 2, eye_y + 13)], fill=p["skin_shadow"], width=1)

    # ---------------- 2. NORTH (BACK) ----------------
    elif direction == "north":
        boot_y = 144 + bob
        # Left boot
        d.rounded_rectangle([cx - 17, boot_y + stride_r, cx - 7, boot_y + 8 + stride_r], radius=2, fill=p["boot_main"], outline=p["outline"])
        # Right boot
        d.rounded_rectangle([cx + 7, boot_y + stride_l, cx + 17, boot_y + 8 + stride_l], radius=2, fill=p["boot_main"], outline=p["outline"])

        top_y = 66 + bob
        hem_y = 141 + bob
        hl = cx - 46 - flare - sway
        hr = cx + 46 + flare - sway

        # Full flowing back cloak
        cloak_pts = [
            (cx, top_y - 4),
            (cx + 28, top_y + 2),
            (hr, hem_y),
            (cx + 20 - sway, hem_y - 4),
            (cx - sway, hem_y - 2),
            (cx - 20 - sway, hem_y - 4),
            (hl, hem_y),
            (cx - 28, top_y + 2)
        ]
        d.polygon(cloak_pts, fill=p["c_white"], outline=p["outline"])

        # Shading folds running down the back
        d.polygon([
            (cx - 10, top_y + 10),
            (cx - sway, hem_y - 4),
            (cx + 10, top_y + 10)
        ], fill=p["c_light"])
        d.line([(cx - 22, top_y + 14), (cx - 26 - sway, hem_y - 3)], fill=p["c_mid"], width=3)
        d.line([(cx + 22, top_y + 14), (cx + 26 - sway, hem_y - 3)], fill=p["c_shadow"], width=3)
        d.line([(cx - 8, top_y + 12), (cx - 12 - sway, hem_y - 4)], fill=p["c_mid"], width=2)
        d.line([(cx + 8, top_y + 12), (cx + 12 - sway, hem_y - 4)], fill=p["c_mid"], width=2)

        # Bottom Hem Colored Trim
        hem_pts = [(hl, hem_y), (cx - 20 - sway, hem_y - 4), (cx - sway, hem_y - 2), (cx + 20 - sway, hem_y - 4), (hr, hem_y)]
        d.line(hem_pts, fill=p["t_shadow"], width=4)
        d.line(hem_pts, fill=p["t_main"], width=2)
        d.line(hem_pts, fill=p["t_glow"], width=1)

        # Back Shoulder Capelet
        capelet_back = [
            (cx, top_y - 2),
            (cx + 30, top_y + 4),
            (cx + 32, top_y + 24),
            (cx, top_y + 30),
            (cx - 32, top_y + 24),
            (cx - 30, top_y + 4)
        ]
        d.polygon(capelet_back, fill=p["c_light"], outline=p["outline"])
        d.line([(cx, top_y - 2), (cx, top_y + 30)], fill=p["c_shadow"], width=2)
        c_trim = [(cx - 32, top_y + 24), (cx, top_y + 30), (cx + 32, top_y + 24)]
        d.line(c_trim, fill=p["t_shadow"], width=3)
        d.line(c_trim, fill=p["t_main"], width=2)
        d.line(c_trim, fill=p["t_glow"], width=1)

        # Hood (Seen from behind)
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_w = 32

        hood_back = [
            (cx, hood_top - 6),
            (cx + 20, hood_top - 1),
            (cx + hood_w + 2, hood_top + 18),
            (cx + hood_w + 1, hood_top + 38),
            (cx + 18, hood_bot + 4),
            (cx, hood_bot + 8),
            (cx - 18, hood_bot + 4),
            (cx - hood_w - 1, hood_top + 38),
            (cx - hood_w - 2, hood_top + 18),
            (cx - 20, hood_top - 1)
        ]
        d.polygon(hood_back, fill=p["c_white"], outline=p["outline"])
        d.line([(cx, hood_top - 6), (cx, hood_bot + 8)], fill=p["c_mid"], width=2)
        d.line([(cx - 16, hood_top + 12), (cx - 10, hood_bot + 2)], fill=p["c_mid"], width=2)
        d.line([(cx + 16, hood_top + 12), (cx + 10, hood_bot + 2)], fill=p["c_shadow"], width=2)

    # ---------------- 3. WEST (SIDE PROFILE) ----------------
    elif direction == "west":
        boot_y = 144 + bob
        # Left boot (front stride, stepping left)
        d.polygon([(cx - 20 + stride_l, boot_y + 2), (cx - 6 + stride_l, boot_y + 2), (cx - 4 + stride_l, boot_y + 9), (cx - 22 + stride_l, boot_y + 9)], fill=p["boot_main"], outline=p["outline"])
        d.line([(cx - 20 + stride_l, boot_y + 3), (cx - 7 + stride_l, boot_y + 3)], fill=p["boot_highlight"], width=1)
        # Right boot (back stride, trailing right)
        d.polygon([(cx + 4 + stride_r, boot_y + 1), (cx + 16 + stride_r, boot_y + 1), (cx + 18 + stride_r, boot_y + 8), (cx + 2 + stride_r, boot_y + 8)], fill=p["boot_main"], outline=p["outline"])

        top_y = 66 + bob
        hem_y = 140 + bob
        front_x = cx - 22
        back_x = cx + 40 + flare * 2

        # Flowing cloak trailing behind
        cloak_side = [
            (cx - 12, top_y + 2),
            (cx + 8, top_y),
            (back_x, hem_y),
            (cx + 18, hem_y - 4),
            (front_x - 4, hem_y),
            (front_x, top_y + 8)
        ]
        d.polygon(cloak_side, fill=p["c_light"], outline=p["outline"])
        d.polygon([
            (cx - 6, top_y + 4),
            (cx + 6, top_y + 2),
            (back_x - 3, hem_y - 2),
            (cx + 18, hem_y - 4),
            (cx + 4, hem_y - 2)
        ], fill=p["c_shadow"])

        # Inner robe under front
        d.rectangle([front_x + 2, top_y + 12, front_x + 14, hem_y - 6], fill=p["tunic_main"], outline=p["outline"])
        d.rectangle([front_x + 2, top_y + 32, front_x + 14, top_y + 37], fill=p["belt_gold"], outline=p["outline"])

        # Front drape
        d.polygon([(cx - 12, top_y + 4), (front_x, top_y + 10), (front_x - 4, hem_y), (cx - 4, hem_y - 2)], fill=p["c_white"], outline=p["outline"])
        d.line([(front_x, top_y + 10), (front_x - 4, hem_y)], fill=p["t_main"], width=3)
        d.line([(front_x, top_y + 10), (front_x - 4, hem_y)], fill=p["t_light"], width=1)
        
        # Bottom Hem Trim
        hem_trim = [(front_x - 4, hem_y), (cx + 18, hem_y - 4), (back_x, hem_y)]
        d.line(hem_trim, fill=p["t_shadow"], width=4)
        d.line(hem_trim, fill=p["t_main"], width=2)
        d.line(hem_trim, fill=p["t_glow"], width=1)

        # Shoulder Capelet side
        capelet_side = [
            (cx - 14, top_y + 2),
            (cx + 14, top_y + 4),
            (cx + 18, top_y + 24),
            (cx - 8, top_y + 22),
            (cx - 18, top_y + 12)
        ]
        d.polygon(capelet_side, fill=p["c_white"], outline=p["outline"])
        c_trim = [(cx - 18, top_y + 12), (cx - 8, top_y + 22), (cx + 18, top_y + 24)]
        d.line(c_trim, fill=p["t_main"], width=3)
        d.line(c_trim, fill=p["t_light"], width=1)

        # Side Brooch
        d.ellipse([cx - 14, top_y + 6, cx - 6, top_y + 14], fill=p["brooch_base"], outline=p["outline"])
        d.ellipse([cx - 12, top_y + 8, cx - 8, top_y + 12], fill=p["brooch_gem"])

        # Hood side profile
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_back_x = cx + 28
        hood_front_x = cx - 22

        hood_side = [
            (cx - 6, hood_top - 5),
            (hood_back_x - 2, hood_top + 4),
            (hood_back_x + 4, hood_top + 24),
            (hood_back_x, hood_bot + 2),
            (cx, hood_bot + 4),
            (hood_front_x + 6, hood_bot),
            (hood_front_x, hood_top + 26),
            (hood_front_x + 6, hood_top + 8)
        ]
        d.polygon(hood_side, fill=p["c_white"], outline=p["outline"])
        d.line([(hood_back_x, hood_top + 16), (cx + 8, hood_bot)], fill=p["c_mid"], width=2)

        open_top_side = hood_top + 12
        open_bot_side = hood_bot - 8
        d.rounded_rectangle([hood_front_x + 2, open_top_side, hood_front_x + 20, open_bot_side], radius=7, outline=p["t_main"], width=2)
        d.rounded_rectangle([hood_front_x + 4, open_top_side + 2, hood_front_x + 19, open_bot_side - 2], radius=6, fill=p["hair_shadow"])

        # Side face
        s_face_x = hood_front_x + 4
        s_face_y = open_top_side + 6
        d.polygon([
            (s_face_x + 1, s_face_y + 4),
            (s_face_x + 14, s_face_y + 2),
            (s_face_x + 14, s_face_y + 22),
            (s_face_x + 3, s_face_y + 22),
            (s_face_x - 1, s_face_y + 14),
            (s_face_x, s_face_y + 8)
        ], fill=p["skin_base"])

        if gender == "male":
            d.polygon([(s_face_x + 2, s_face_y), (s_face_x + 7, s_face_y + 12), (s_face_x + 5, s_face_y)], fill=p["hair_main"])
            d.polygon([(s_face_x + 6, s_face_y), (s_face_x + 13, s_face_y + 14), (s_face_x + 11, s_face_y)], fill=p["hair_light"])
        else:
            d.polygon([(s_face_x + 2, s_face_y), (s_face_x + 8, s_face_y + 13), (s_face_x + 6, s_face_y)], fill=p["hair_light"])
            d.polygon([(s_face_x + 7, s_face_y), (s_face_x + 13, s_face_y + 15), (s_face_x + 12, s_face_y)], fill=p["hair_main"])
            d.line([(s_face_x + 13, s_face_y + 10), (s_face_x + 12, s_face_y + 24)], fill=p["hair_main"], width=2)
            d.ellipse([s_face_x + 3, s_face_y + 16, s_face_x + 8, s_face_y + 20], fill=p["blush"])

        # Side eye
        s_eye_y = s_face_y + 9
        d.polygon([(s_face_x + 3, s_eye_y + 1), (s_face_x + 8, s_eye_y), (s_face_x + 7, s_eye_y + 5), (s_face_x + 3, s_eye_y + 4)], fill=p["eye_dark"])
        d.polygon([(s_face_x + 4, s_eye_y + 1), (s_face_x + 8, s_eye_y + 1), (s_face_x + 7, s_eye_y + 4), (s_face_x + 4, s_eye_y + 3)], fill=p["eye_main"])
        d.point((s_face_x + 5, s_eye_y + 2), fill=(255, 255, 255, 255))
        d.line([(s_face_x + 2, s_eye_y), (s_face_x + 8, s_eye_y)], fill=p["outline"], width=1)

    # ---------------- 4. SOUTH-WEST (3/4 FRONT) ----------------
    elif direction == "south_west":
        boot_y = 144 + bob
        # Left boot
        d.rounded_rectangle([cx - 20 + stride_l, boot_y + 1, cx - 7 + stride_l, boot_y + 10 + stride_l], radius=3, fill=p["boot_main"], outline=p["outline"])
        d.line([(cx - 19 + stride_l, boot_y + 2), (cx - 8 + stride_l, boot_y + 2)], fill=p["boot_highlight"], width=1)
        # Right boot
        d.rounded_rectangle([cx + 3 + stride_r, boot_y + stride_r, cx + 15 + stride_r, boot_y + 9 + stride_r], radius=3, fill=p["boot_main"], outline=p["outline"])

        top_y = 66 + bob
        hem_y = 140 + bob
        hl = cx - 40 - flare + sway
        hr = cx + 46 + flare + sway

        # Cloak silhouette 3/4
        cloak_34 = [
            (cx - 8, top_y - 2),
            (cx + 22, top_y + 2),
            (hr, hem_y),
            (cx + 18 + sway, hem_y - 4),
            (cx - 8 + sway, hem_y - 3),
            (hl, hem_y),
            (cx - 28, top_y + 4)
        ]
        d.polygon(cloak_34, fill=p["c_mid"], outline=p["outline"])
        d.polygon([
            (cx + 2, top_y + 4),
            (cx + 20, top_y + 4),
            (hr - 2, hem_y - 2),
            (cx + 18 + sway, hem_y - 4)
        ], fill=p["c_shadow"])

        # Inner tunic
        d.rectangle([cx - 22, top_y + 6, cx - 2, hem_y - 6], fill=p["tunic_main"], outline=p["outline"])
        belt_y = top_y + 32
        d.rectangle([cx - 22, belt_y, cx - 2, belt_y + 6], fill=p["belt_gold"], outline=p["outline"])
        d.rectangle([cx - 15, belt_y - 1, cx - 7, belt_y + 7], fill=p["brooch_gem"])

        # Front drapes
        d.polygon([(cx - 28, top_y + 4), (cx - 12, top_y + 6), (cx - 18 + sway, hem_y - 2), (hl, hem_y)], fill=p["c_light"], outline=p["outline"])
        d.polygon([(cx + 18, top_y + 2), (cx - 2, top_y + 6), (cx + 6 + sway, hem_y - 2), (hr, hem_y)], fill=p["c_white"], outline=p["outline"])

        # Trim along 3/4 front opening
        d.line([(cx - 2, top_y + 6), (cx + 6 + sway, hem_y - 2)], fill=p["t_main"], width=3)
        d.line([(cx - 2, top_y + 6), (cx + 6 + sway, hem_y - 2)], fill=p["t_light"], width=1)

        # Bottom Hem Trim
        hem_pts_34 = [(hl, hem_y), (cx - 8 + sway, hem_y - 3), (cx + 18 + sway, hem_y - 4), (hr, hem_y)]
        d.line(hem_pts_34, fill=p["t_shadow"], width=4)
        d.line(hem_pts_34, fill=p["t_main"], width=2)
        d.line(hem_pts_34, fill=p["t_glow"], width=1)

        # Capelet 3/4
        capelet_34 = [
            (cx - 6, top_y - 2),
            (cx + 26, top_y + 4),
            (cx + 28, top_y + 22),
            (cx + 8, top_y + 24),
            (cx - 8, top_y + 16),
            (cx - 26, top_y + 20),
            (cx - 28, top_y + 4)
        ]
        d.polygon(capelet_34, fill=p["c_white"], outline=p["outline"])
        c_trim = [(cx - 26, top_y + 20), (cx - 8, top_y + 16), (cx + 8, top_y + 24), (cx + 28, top_y + 22)]
        d.line(c_trim, fill=p["t_main"], width=3)
        d.line(c_trim, fill=p["t_light"], width=1)

        # Brooch
        brooch_y = top_y + 5
        d.ellipse([cx - 14, brooch_y - 5, cx, brooch_y + 5], fill=p["brooch_base"], outline=p["outline"])
        d.ellipse([cx - 11, brooch_y - 3, cx - 3, brooch_y + 3], fill=p["brooch_gem"])

        # Hood 3/4
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_outer_34 = [
            (cx - 4, hood_top - 5),
            (cx + 18, hood_top - 2),
            (cx + 32, hood_top + 18),
            (cx + 33, hood_top + 36),
            (cx + 14, hood_bot + 4),
            (cx - 6, hood_bot + 4),
            (cx - 30, hood_bot - 4),
            (cx - 33, hood_top + 28),
            (cx - 24, hood_top)
        ]
        d.polygon(hood_outer_34, fill=p["c_white"], outline=p["outline"])
        d.line([(cx + 12, hood_top + 8), (cx + 24, hood_bot)], fill=p["c_mid"], width=2)

        # Face opening 3/4
        open_34_x = cx - 14
        d.rounded_rectangle([open_34_x - 14, hood_top + 12, open_34_x + 18, hood_bot - 6], radius=9, outline=p["t_main"], width=2)
        d.rounded_rectangle([open_34_x - 12, hood_top + 14, open_34_x + 16, hood_bot - 8], radius=8, fill=p["hair_shadow"])

        # Face skin 3/4
        f_x = open_34_x - 10
        f_y = hood_top + 20
        d.rounded_rectangle([f_x, f_y, f_x + 26, f_y + 30], radius=6, fill=p["skin_base"])
        d.chord([f_x + 2, f_y + 16, f_x + 24, f_y + 28], 0, 180, fill=p["skin_shadow"])

        if gender == "male":
            d.polygon([(f_x + 1, f_y), (f_x + 8, f_y + 14), (f_x + 6, f_y)], fill=p["hair_main"])
            d.polygon([(f_x + 5, f_y), (f_x + 15, f_y + 17), (f_x + 13, f_y)], fill=p["hair_light"])
            d.polygon([(f_x + 12, f_y), (f_x + 22, f_y + 14), (f_x + 20, f_y)], fill=p["hair_main"])
        else:
            d.polygon([(f_x + 1, f_y), (f_x + 7, f_y + 15), (f_x + 5, f_y)], fill=p["hair_light"])
            d.polygon([(f_x + 5, f_y), (f_x + 15, f_y + 18), (f_x + 13, f_y)], fill=p["hair_light"])
            d.polygon([(f_x + 12, f_y), (f_x + 24, f_y + 15), (f_x + 22, f_y)], fill=p["hair_main"])
            d.line([(f_x + 1, f_y + 6), (f_x + 1, f_y + 22)], fill=p["hair_main"], width=2)
            d.ellipse([f_x + 3, f_y + 18, f_x + 8, f_y + 22], fill=p["blush"])

        # Eyes 3/4
        e_y = f_y + 12
        d.rounded_rectangle([f_x + 3, e_y, f_x + 10, e_y + 7], radius=2, fill=p["eye_dark"])
        d.rounded_rectangle([f_x + 4, e_y + 1, f_x + 10, e_y + 6], radius=2, fill=p["eye_main"])
        d.rectangle([f_x + 4, e_y + 1, f_x + 6, e_y + 3], fill=(255, 255, 255, 255))
        d.line([(f_x + 2, e_y - 1), (f_x + 11, e_y - 1)], fill=p["outline"], width=1)

        d.rounded_rectangle([f_x + 14, e_y + 1, f_x + 19, e_y + 7], radius=2, fill=p["eye_dark"])
        d.rounded_rectangle([f_x + 14, e_y + 2, f_x + 18, e_y + 6], radius=2, fill=p["eye_main"])
        d.rectangle([f_x + 14, e_y + 2, f_x + 16, e_y + 4], fill=(255, 255, 255, 255))
        d.line([(f_x + 13, e_y), (f_x + 20, e_y)], fill=p["outline"], width=1)

    # ---------------- 5. NORTH-WEST (3/4 BACK) ----------------
    elif direction == "north_west":
        boot_y = 144 + bob
        # Left boot
        d.rounded_rectangle([cx - 18 + stride_l, boot_y + stride_l, cx - 8 + stride_l, boot_y + 8 + stride_l], radius=2, fill=p["boot_main"], outline=p["outline"])
        # Right boot
        d.rounded_rectangle([cx + 6 + stride_r, boot_y + stride_r, cx + 16 + stride_r, boot_y + 8 + stride_r], radius=2, fill=p["boot_main"], outline=p["outline"])

        top_y = 66 + bob
        hem_y = 141 + bob
        hl = cx - 44 - flare - sway
        hr = cx + 46 + flare - sway

        # Cloak back 3/4
        cloak_nw = [
            (cx - 4, top_y - 4),
            (cx + 26, top_y + 2),
            (hr, hem_y),
            (cx + 18 - sway, hem_y - 4),
            (cx - 8 - sway, hem_y - 2),
            (hl, hem_y),
            (cx - 26, top_y + 2)
        ]
        d.polygon(cloak_nw, fill=p["c_white"], outline=p["outline"])
        d.line([(cx + 14, top_y + 14), (cx + 20 - sway, hem_y - 3)], fill=p["c_shadow"], width=4)
        d.line([(cx - 10, top_y + 12), (cx - 16 - sway, hem_y - 4)], fill=p["c_mid"], width=3)

        # Bottom Hem Trim
        hem_trim_nw = [(hl, hem_y), (cx - 8 - sway, hem_y - 2), (cx + 18 - sway, hem_y - 4), (hr, hem_y)]
        d.line(hem_trim_nw, fill=p["t_shadow"], width=4)
        d.line(hem_trim_nw, fill=p["t_main"], width=2)
        d.line(hem_trim_nw, fill=p["t_glow"], width=1)

        # Capelet 3/4 back
        capelet_nw = [
            (cx - 4, top_y - 2),
            (cx + 28, top_y + 4),
            (cx + 30, top_y + 24),
            (cx + 4, top_y + 28),
            (cx - 26, top_y + 22),
            (cx - 28, top_y + 4)
        ]
        d.polygon(capelet_nw, fill=p["c_light"], outline=p["outline"])
        c_trim = [(cx - 26, top_y + 22), (cx + 4, top_y + 28), (cx + 30, top_y + 24)]
        d.line(c_trim, fill=p["t_main"], width=3)
        d.line(c_trim, fill=p["t_light"], width=1)

        # Hood 3/4 back
        hood_top = 12 + bob
        hood_bot = 68 + bob
        hood_nw = [
            (cx + 4, hood_top - 6),
            (cx + 26, hood_top),
            (cx + 34, hood_top + 22),
            (cx + 30, hood_top + 40),
            (cx + 12, hood_bot + 4),
            (cx - 14, hood_bot + 4),
            (cx - 30, hood_top + 34),
            (cx - 28, hood_top + 16),
            (cx - 14, hood_top - 2)
        ]
        d.polygon(hood_nw, fill=p["c_white"], outline=p["outline"])
        d.line([(cx + 4, hood_top - 6), (cx + 2, hood_bot + 4)], fill=p["c_mid"], width=2)
        d.line([(cx + 18, hood_top + 10), (cx + 18, hood_bot + 2)], fill=p["c_shadow"], width=2)

    return img

def build_all():
    dirs = ["south", "south_west", "west", "north_west", "north", "north_east", "east", "south_east"]
    
    # 1. Output paths
    male_dir = "client/GodlessFantasyClient/Assets/Resources/Characters/Mage/male"
    female_dir = "client/GodlessFantasyClient/Assets/Resources/Characters/Mage/female"
    fallback_dir = "client/GodlessFantasyClient/Assets/Resources/Characters/Mage"

    os.makedirs(male_dir, exist_ok=True)
    os.makedirs(female_dir, exist_ok=True)

    # 2. Build frames and sheets
    for gender, out_dir in [("male", male_dir), ("female", female_dir)]:
        sheet = Image.new("RGBA", (1280, 1280), (0, 0, 0, 0))
        for row_idx, d_name in enumerate(dirs):
            for col_idx in range(8):
                frame_img = draw_character_frame(gender, d_name, col_idx)
                
                # Save single frame file
                f_path = os.path.join(out_dir, f"mage_{d_name}_{col_idx}.png")
                frame_img.save(f_path)

                # If male, also save fallback in Characters/Mage/
                if gender == "male":
                    os.path.join(fallback_dir, f"mage_{d_name}_{col_idx}.png")
                    frame_img.save(os.path.join(fallback_dir, f"mage_{d_name}_{col_idx}.png"))

                # Paste into 8x8 sheet (160x160 per tile)
                sheet.paste(frame_img, (col_idx * 160, row_idx * 160), frame_img)

        # Save spritesheet
        sheet.save(os.path.join(out_dir, "mage_sheet_8dir.png"))
        if gender == "male":
            sheet.save(os.path.join(fallback_dir, "mage_sheet_8dir.png"))
        print(f"Generated 64 frames & sheet for {gender} successfully.")

    # 3. Build Avatars & Fullbody Portraits
    build_portraits()

def build_portraits():
    char_dir = "client/GodlessFantasyClient/Assets/Resources/Characters"
    os.makedirs(char_dir, exist_ok=True)

    # High quality 8-bit avatars (128x128)
    for gender in ["male", "female"]:
        p = get_palette(gender)
        av = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
        d = ImageDraw.Draw(av)

        # Background gradient medallion
        for r in range(58, 0, -2):
            val = int(25 + (58 - r) * 0.8)
            col = (val, val + 10, val + 25, 240) if gender == "male" else (val + 20, val, val + 15, 240)
            d.ellipse([64 - r, 64 - r, 64 + r, 64 + r], fill=col)
        # Gold/Rose trim ring
        ring_col = (255, 215, 50, 255) if gender == "male" else (255, 140, 185, 255)
        d.ellipse([6, 6, 122, 122], outline=ring_col, width=3)
        d.ellipse([9, 9, 119, 119], outline=p["outline"], width=1)

        # Headshot
        cx = 64
        top_y = 18
        hood_w = 36
        hood_bot = 95

        # Hood
        hood_outer = [
            (cx, top_y - 6),
            (cx + 22, top_y - 2),
            (cx + hood_w + 3, top_y + 20),
            (cx + hood_w + 2, top_y + 44),
            (cx + 22, hood_bot),
            (cx, hood_bot + 5),
            (cx - 22, hood_bot),
            (cx - hood_w - 2, top_y + 44),
            (cx - hood_w - 3, top_y + 20),
            (cx - 22, top_y - 2)
        ]
        d.polygon(hood_outer, fill=p["c_white"], outline=p["outline"])
        
        # Hood trim
        open_w = 23
        open_top = top_y + 14
        open_bot = hood_bot - 8
        d.rounded_rectangle([cx - open_w - 2, open_top - 2, cx + open_w + 2, open_bot + 2], radius=11, outline=p["t_shadow"], width=3)
        d.rounded_rectangle([cx - open_w - 1, open_top - 1, cx + open_w + 1, open_bot + 1], radius=11, outline=p["t_main"], width=2)
        d.rounded_rectangle([cx - open_w, open_top, cx + open_w, open_bot], radius=11, outline=p["t_glow"], width=1)
        d.rounded_rectangle([cx - open_w + 1, open_top + 1, cx + open_w - 1, open_bot - 1], radius=10, fill=p["hair_shadow"])

        # Face
        fx = cx - 20
        fy = open_top + 8
        fw = 40
        fh = 36
        d.rounded_rectangle([fx, fy, fx + fw, fy + fh], radius=8, fill=p["skin_base"])
        d.chord([fx + 4, fy + 22, fx + fw - 4, fy + fh], 0, 180, fill=p["skin_shadow"])

        # Bangs
        if gender == "male":
            d.polygon([(fx + 2, fy), (fx + 14, fy + 16), (fx + 9, fy)], fill=p["hair_main"])
            d.polygon([(fx + 9, fy), (fx + 21, fy + 20), (fx + 17, fy)], fill=p["hair_light"])
            d.polygon([(fx + 18, fy), (fx + 30, fy + 18), (fx + 26, fy)], fill=p["hair_main"])
            d.polygon([(fx + 26, fy), (fx + 38, fy + 13), (fx + 39, fy)], fill=p["hair_shadow"])
        else:
            d.polygon([(fx + 1, fy), (fx + 10, fy + 17), (fx + 8, fy)], fill=p["hair_light"])
            d.polygon([(fx + 7, fy), (fx + 21, fy + 21), (fx + 17, fy)], fill=p["hair_light"])
            d.polygon([(fx + 16, fy), (fx + 30, fy + 21), (fx + 26, fy)], fill=p["hair_main"])
            d.polygon([(fx + 25, fy), (fx + 38, fy + 17), (fx + 39, fy)], fill=p["hair_shadow"])
            d.line([(fx + 2, fy + 8), (fx + 2, fy + 26)], fill=p["hair_main"], width=2)
            d.line([(fx + fw - 3, fy + 8), (fx + fw - 3, fy + 26)], fill=p["hair_main"], width=2)
            d.ellipse([fx + 4, fy + 22, fx + 11, fy + 27], fill=p["blush"])
            d.ellipse([fx + fw - 11, fy + 22, fx + fw - 4, fy + 27], fill=p["blush"])

        # Big expressive anime eyes
        ey = fy + 14
        d.rounded_rectangle([fx + 7, ey, fx + 15, ey + 9], radius=3, fill=p["eye_dark"])
        d.rounded_rectangle([fx + 8, ey + 1, fx + 15, ey + 8], radius=2, fill=p["eye_main"])
        d.rectangle([fx + 8, ey + 1, fx + 11, ey + 4], fill=(255, 255, 255, 255))
        d.line([(fx + 6, ey - 1), (fx + 16, ey - 1)], fill=p["outline"], width=1)

        d.rounded_rectangle([fx + fw - 15, ey, fx + fw - 7, ey + 9], radius=3, fill=p["eye_dark"])
        d.rounded_rectangle([fx + fw - 15, ey + 1, fx + fw - 8, ey + 8], radius=2, fill=p["eye_main"])
        d.rectangle([fx + fw - 15, ey + 1, fx + fw - 12, ey + 4], fill=(255, 255, 255, 255))
        d.line([(fx + fw - 16, ey - 1), (fx + fw - 6, ey - 1)], fill=p["outline"], width=1)

        # Smile
        d.point((cx, ey + 11), fill=p["skin_shadow"])
        d.line([(cx - 3, ey + 15), (cx + 3, ey + 15)], fill=p["skin_shadow"], width=1)

        # Collar capelet & brooch
        d.polygon([(cx - 30, hood_bot - 4), (cx, hood_bot + 16), (cx + 30, hood_bot - 4)], fill=p["c_white"], outline=p["outline"])
        d.line([(cx - 30, hood_bot - 4), (cx, hood_bot + 16), (cx + 30, hood_bot - 4)], fill=p["t_main"], width=2)
        d.ellipse([cx - 8, hood_bot - 2, cx + 8, hood_bot + 12], fill=p["brooch_base"], outline=p["outline"])
        d.ellipse([cx - 5, hood_bot + 1, cx + 5, hood_bot + 9], fill=p["brooch_gem"])

        # Save avatar
        av.save(os.path.join(char_dir, f"avatar_{gender}.png"))
        print(f"Generated avatar_{gender}.png")

        # Fullbody preview portrait (160x240)
        fb = Image.new("RGBA", (160, 240), (0, 0, 0, 0))
        full_char = draw_character_frame(gender, "south", 0)
        fb.paste(full_char, (0, 40), full_char)
        fb.save(os.path.join(char_dir, f"fullbody_{gender}.png"))
        print(f"Generated fullbody_{gender}.png")

if __name__ == "__main__":
    build_all()
