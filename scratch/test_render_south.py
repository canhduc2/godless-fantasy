import os
import math
from PIL import Image, ImageDraw
from build_palettes import get_palettes

def draw_character(palette, direction="south", frame=0):
    img = Image.new("RGBA", (160, 160), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    p = palette
    gender = p["gender"]

    # Bobbing and swaying according to walk frame
    bob = 0
    stride_l = 0
    stride_r = 0
    cloak_flare = 0
    cloak_sway = 0

    if frame in [1, 5]:
        bob = 2
    elif frame in [2, 6]:
        bob = -1
    elif frame in [4, 0]:
        bob = 0

    # Foot stride
    if frame == 1:
        stride_l = 4; stride_r = -2; cloak_flare = 2; cloak_sway = -2
    elif frame == 2:
        stride_l = 8; stride_r = -5; cloak_flare = 5; cloak_sway = -4
    elif frame == 3:
        stride_l = 4; stride_r = -2; cloak_flare = 3; cloak_sway = -2
    elif frame == 5:
        stride_r = 4; stride_l = -2; cloak_flare = 2; cloak_sway = 2
    elif frame == 6:
        stride_r = 8; stride_l = -5; cloak_flare = 5; cloak_sway = 4
    elif frame == 7:
        stride_r = 4; stride_l = -2; cloak_flare = 3; cloak_sway = 2

    cx = 80
    cy_base = 148 + bob

    # 1. DRAW BOOTS / FEET
    boot_w = 12
    boot_h = 10
    l_foot_x = cx - 18 + (cloak_sway * 0.5)
    r_foot_x = cx + 6 + (cloak_sway * 0.5)

    l_foot_y = cy_base - boot_h + stride_l
    r_foot_y = cy_base - boot_h + stride_r

    # Left boot
    d.rounded_rectangle([l_foot_x, l_foot_y, l_foot_x + boot_w, l_foot_y + boot_h], radius=3, fill=p["boot_main"], outline=p["outline"], width=1)
    d.rectangle([l_foot_x + 1, l_foot_y + 1, l_foot_x + boot_w - 2, l_foot_y + 3], fill=p["boot_trim"])
    
    # Right boot
    d.rounded_rectangle([r_foot_x, r_foot_y, r_foot_x + boot_w, r_foot_y + boot_h], radius=3, fill=p["boot_main"], outline=p["outline"], width=1)
    d.rectangle([r_foot_x + 1, r_foot_y + 1, r_foot_x + boot_w - 2, r_foot_y + 3], fill=p["boot_trim"])

    # 2. DRAW MAIN CLOAK (BACK/BODY)
    # The cloak drapes from shoulders (y ~ 65) down to hem (y ~ 140)
    top_y = 62 + bob
    hem_y = 140 + bob
    
    hem_left = cx - 38 - cloak_flare + cloak_sway
    hem_right = cx + 38 + cloak_flare + cloak_sway

    shoulder_l = cx - 24
    shoulder_r = cx + 24

    # Back drape of cloak
    cloak_poly = [
        (cx, top_y - 6),
        (shoulder_r, top_y),
        (hem_right, hem_y),
        (cx + 20 + cloak_sway, hem_y - 4),
        (cx + cloak_sway, hem_y - 2),
        (cx - 20 + cloak_sway, hem_y - 4),
        (hem_left, hem_y),
        (shoulder_l, top_y)
    ]
    d.polygon(cloak_poly, fill=p["cloak_shadow"], outline=p["outline"])
    d.polygon([
        (cx, top_y - 4),
        (shoulder_r - 2, top_y + 2),
        (hem_right - 4, hem_y - 2),
        (cx + 18 + cloak_sway, hem_y - 5),
        (cx + cloak_sway, hem_y - 4),
        (cx - 18 + cloak_sway, hem_y - 5),
        (hem_left + 4, hem_y - 2),
        (shoulder_l + 2, top_y + 2)
    ], fill=p["cloak_main"])

    # 3. INNER TUNIC / ROBE (Visible in front opening)
    tunic_top = top_y + 6
    tunic_bot = hem_y - 8
    d.rectangle([cx - 14, tunic_top, cx + 14, tunic_bot], fill=p["tunic_main"], outline=p["outline"], width=1)
    # Tunic vertical trim
    d.line([cx, tunic_top, cx, tunic_bot], fill=p["tunic_trim"], width=2)
    # Belt
    belt_y = tunic_top + 28
    d.rectangle([cx - 14, belt_y, cx + 14, belt_y + 6], fill=p["belt_gold"], outline=p["outline"], width=1)
    d.rectangle([cx - 4, belt_y - 1, cx + 4, belt_y + 7], fill=p["brooch_gem"])

    # 4. FRONT CLOAK FLAPS (LEFT & RIGHT) WITH COLORED TRIM
    # Left front lapel
    flap_l = [
        (cx - 2, tunic_top),
        (shoulder_l, top_y + 4),
        (hem_left + 6, hem_y - 1),
        (cx - 10 + cloak_sway, hem_y - 3),
        (cx - 8, tunic_top + 18),
        (cx - 2, tunic_top)
    ]
    d.polygon(flap_l, fill=p["cloak_main"], outline=p["outline"])
    # Left lapel trim (gorgeous cyan or pink stripe)
    d.line([(cx - 2, tunic_top), (cx - 8, tunic_top + 18), (cx - 10 + cloak_sway, hem_y - 3), (hem_left + 6, hem_y - 1)], fill=p["trim_main"], width=3)
    d.line([(cx - 2, tunic_top), (cx - 8, tunic_top + 18), (cx - 10 + cloak_sway, hem_y - 3)], fill=p["trim_light"], width=1)

    # Right front lapel
    flap_r = [
        (cx + 2, tunic_top),
        (shoulder_r, top_y + 4),
        (hem_right - 6, hem_y - 1),
        (cx + 10 + cloak_sway, hem_y - 3),
        (cx + 8, tunic_top + 18),
        (cx + 2, tunic_top)
    ]
    d.polygon(flap_r, fill=p["cloak_main"], outline=p["outline"])
    # Right lapel trim
    d.line([(cx + 2, tunic_top), (cx + 8, tunic_top + 18), (cx + 10 + cloak_sway, hem_y - 3), (hem_right - 6, hem_y - 1)], fill=p["trim_main"], width=3)
    d.line([(cx + 2, tunic_top), (cx + 8, tunic_top + 18), (cx + 10 + cloak_sway, hem_y - 3)], fill=p["trim_light"], width=1)

    # Hem bottom colored trim (glowing border around lower hem)
    hem_trim_pts = [
        (hem_left, hem_y),
        (cx - 20 + cloak_sway, hem_y - 4),
        (cx + cloak_sway, hem_y - 2),
        (cx + 20 + cloak_sway, hem_y - 4),
        (hem_right, hem_y)
    ]
    d.line(hem_trim_pts, fill=p["trim_main"], width=3)
    d.line(hem_trim_pts, fill=p["trim_light"], width=1)

    # 5. HOOD (OUTER)
    hood_cx = cx
    hood_top = 14 + bob
    hood_bottom = 66 + bob
    hood_r = 27

    # Outer Hood silhouette
    hood_pts = [
        (hood_cx, hood_top - 4), # pointy hood tip
        (hood_cx + 18, hood_top),
        (hood_cx + hood_r + 2, hood_top + 20),
        (hood_cx + hood_r, hood_top + 40),
        (hood_cx + 18, hood_bottom),
        (hood_cx, hood_bottom + 4),
        (hood_cx - 18, hood_bottom),
        (hood_cx - hood_r, hood_top + 40),
        (hood_cx - hood_r - 2, hood_top + 20),
        (hood_cx - 18, hood_top)
    ]
    d.polygon(hood_pts, fill=p["cloak_main"], outline=p["outline"])

    # Hood rim colored border (Cyan / Pink border running along face opening)
    face_left = hood_cx - 18
    face_right = hood_cx + 18
    face_top = hood_top + 12
    face_bot = hood_bottom - 6

    d.rounded_rectangle([face_left - 3, face_top - 3, face_right + 3, face_bot + 3], radius=8, outline=p["trim_main"], width=3)
    d.rounded_rectangle([face_left - 2, face_top - 2, face_right + 2, face_bot + 2], radius=8, outline=p["trim_light"], width=1)

    # Inside hood cavity (shadow around face)
    d.rounded_rectangle([face_left, face_top, face_right, face_bot], radius=7, fill=p["hair_shadow"])

    # 6. FACE (SKIN)
    skin_x = hood_cx - 15
    skin_w = 30
    skin_y = face_top + 6
    skin_h = 32
    d.rounded_rectangle([skin_x, skin_y, skin_x + skin_w, skin_y + skin_h], radius=6, fill=p["skin_main"])
    # Chin shadow
    d.chord([skin_x + 3, skin_y + 18, skin_x + skin_w - 3, skin_y + skin_h], 0, 180, fill=p["skin_shadow"])

    # 7. HAIR BANGS
    if gender == "male":
        # Cool stylish dark locks peeking from inside hood
        d.polygon([(skin_x + 2, skin_y), (skin_x + 10, skin_y + 10), (skin_x + 6, skin_y)], fill=p["hair_main"])
        d.polygon([(skin_x + 8, skin_y), (skin_x + 16, skin_y + 14), (skin_x + 14, skin_y)], fill=p["hair_light"])
        d.polygon([(skin_x + 14, skin_y), (skin_x + 22, skin_y + 11), (skin_x + 20, skin_y)], fill=p["hair_main"])
        d.polygon([(skin_x + 20, skin_y), (skin_x + 28, skin_y + 8), (skin_x + 28, skin_y)], fill=p["hair_shadow"])
    else:
        # Cute soft golden blonde anime bangs
        d.polygon([(skin_x, skin_y), (skin_x + 8, skin_y + 14), (skin_x + 6, skin_y)], fill=p["hair_main"])
        d.polygon([(skin_x + 5, skin_y), (skin_x + 15, skin_y + 16), (skin_x + 13, skin_y)], fill=p["hair_light"])
        d.polygon([(skin_x + 12, skin_y), (skin_x + 22, skin_y + 16), (skin_x + 20, skin_y)], fill=p["hair_light"])
        d.polygon([(skin_x + 18, skin_y), (skin_x + 28, skin_y + 14), (skin_x + 28, skin_y)], fill=p["hair_main"])
        # Cute side-locks framing face
        d.line([(skin_x + 1, skin_y + 4), (skin_x + 1, skin_y + 22)], fill=p["hair_main"], width=2)
        d.line([(skin_x + skin_w - 2, skin_y + 4), (skin_x + skin_w - 2, skin_y + 22)], fill=p["hair_main"], width=2)
        # Rosy blush on cheeks
        d.ellipse([skin_x + 3, skin_y + 18, skin_x + 8, skin_y + 21], fill=p["blush"])
        d.ellipse([skin_x + skin_w - 8, skin_y + 18, skin_x + skin_w - 3, skin_y + 21], fill=p["blush"])

    # 8. EXPRESSIVE EYES (BEAUTIFUL ANIME / FANTASY EYES)
    eye_y = skin_y + 12
    # Left eye
    d.rectangle([skin_x + 5, eye_y, skin_x + 10, eye_y + 6], fill=p["eye_dark"])
    d.rectangle([skin_x + 6, eye_y + 1, skin_x + 10, eye_y + 5], fill=p["eye_color"])
    d.rectangle([skin_x + 6, eye_y + 1, skin_x + 7, eye_y + 2], fill=p["eye_spark"]) # sparkle
    d.line([(skin_x + 4, eye_y - 1), (skin_x + 11, eye_y - 1)], fill=p["outline"], width=1) # eyelash

    # Right eye
    d.rectangle([skin_x + skin_w - 10, eye_y, skin_x + skin_w - 5, eye_y + 6], fill=p["eye_dark"])
    d.rectangle([skin_x + skin_w - 10, eye_y + 1, skin_x + skin_w - 6, eye_y + 5], fill=p["eye_color"])
    d.rectangle([skin_x + skin_w - 10, eye_y + 1, skin_x + skin_w - 9, eye_y + 2], fill=p["eye_spark"]) # sparkle
    d.line([(skin_x + skin_w - 11, eye_y - 1), (skin_x + skin_w - 4, eye_y - 1)], fill=p["outline"], width=1) # eyelash

    # Cute nose and smile
    d.point((hood_cx, eye_y + 9), fill=p["skin_shadow"])
    d.line([(hood_cx - 2, eye_y + 13), (hood_cx + 2, eye_y + 13)], fill=p["skin_shadow"], width=1)

    # 9. COLLAR BROOCH / CLASP
    brooch_y = hood_bottom + 1
    d.ellipse([hood_cx - 6, brooch_y - 4, hood_cx + 6, brooch_y + 5], fill=p["brooch_gold"], outline=p["outline"], width=1)
    d.ellipse([hood_cx - 3, brooch_y - 2, hood_cx + 3, brooch_y + 3], fill=p["brooch_gem"])
    d.point((hood_cx - 1, brooch_y - 1), fill=(255, 255, 255, 255)) # sparkle on gem

    return img

male_p, female_p = get_palettes()
m_img = draw_character(male_p, "south", 0)
f_img = draw_character(female_p, "south", 0)

os.makedirs("scratch/preview", exist_ok=True)
m_img.save("scratch/preview/male_south_0.png")
f_img.save("scratch/preview/female_south_0.png")
print("Saved preview/male_south_0.png and female_south_0.png successfully!")
