from PIL import Image

def draw_female_south_handcrafted():
    w, h = 64, 64

    # Palettes
    c_out      = (50, 30, 32, 255) # dark crisp outline
    c_hair_hi  = (215, 170, 150, 255)
    c_hair_mid = (158, 112, 110, 255)
    c_hair_sh  = (115, 78, 80, 255)
    c_hair_out = (60, 35, 38, 255)
    c_headband = (250, 140, 80, 255)
    c_clip     = (45, 45, 55, 255)

    c_skin_hi  = (255, 238, 228, 255)
    c_skin_mid = (251, 215, 195, 255)
    c_skin_sh  = (225, 175, 155, 255)
    c_blush    = (255, 185, 180, 255)

    c_eye_iris = (255, 125, 75, 255)
    c_eye_pup  = (175, 45, 35, 255)
    c_eye_wht  = (255, 255, 255, 255)

    c_shirt_hi  = (255, 180, 145, 255)
    c_shirt_mid = (253, 144, 113, 255)
    c_shirt_sh  = (215, 105, 80, 255)
    c_shirt_out = (85, 40, 35, 255)

    c_shorts_hi  = (85, 75, 85, 255)
    c_shorts_mid = (55, 45, 55, 255)
    c_shorts_sh  = (35, 28, 35, 255)

    c_boot_hi  = (75, 65, 75, 255)
    c_boot_mid = (45, 38, 45, 255)
    c_boot_sh  = (25, 20, 25, 255)

    # 1. HEAD (idle_head.png)
    head = Image.new('RGBA', (w, h), (0, 0, 0, 0))

    # Hair Dome (y=11..23)
    hair_map = {
        11: (28, 36),
        12: (26, 38),
        13: (24, 40),
        14: (23, 41),
        15: (22, 42),
        16: (21, 43),
        17: (20, 43),
        18: (20, 44),
        19: (19, 44),
        20: (19, 44),
        21: (19, 44),
        22: (19, 44),
        23: (19, 44),
    }
    for y, (xs, xe) in hair_map.items():
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 11:
                head.putpixel((x, y), c_hair_out)
            elif y in (15, 16) and 29 <= x <= 34:
                head.putpixel((x, y), c_hair_hi) # hair highlight
            elif y >= 20 and (x <= xs + 2 or x >= xe - 2):
                head.putpixel((x, y), c_hair_sh)
            else:
                head.putpixel((x, y), c_hair_mid)

    # Orange Headband at y=13..15
    for x in range(25, 40):
        if 26 <= x <= 38: head.putpixel((x, 14), c_headband)

    # Hair Clip at (26..27, 18..20)
    for (x, y) in [(26, 18), (27, 18), (26, 19), (28, 19), (28, 20)]:
        head.putpixel((x, y), c_clip)

    # Face Skin Area (y=24..34, x=24..39)
    face_map = {
        24: (24, 39),
        25: (24, 39),
        26: (24, 39),
        27: (24, 39),
        28: (24, 39),
        29: (24, 39),
        30: (25, 38),
        31: (26, 37),
        32: (27, 36),
        33: (28, 35),
        34: (30, 33), # Chin
    }
    for y, (xs, xe) in face_map.items():
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 34:
                head.putpixel((x, y), c_out)
            elif x in (xs + 1, xe - 1):
                head.putpixel((x, y), c_skin_sh)
            else:
                head.putpixel((x, y), c_skin_mid)

    # Bangs over forehead (y=21..25)
    for x in range(24, 40):
        # Front bangs styling
        if 28 <= x <= 35:
            head.putpixel((x, 22), c_hair_mid)
            head.putpixel((x, 23), c_hair_mid)
        if x in (25, 26, 27, 36, 37, 38):
            head.putpixel((x, 23), c_hair_mid)
            head.putpixel((x, 24), c_hair_mid)
        # Bangs tips outline
        if x in (28, 31, 35):
            head.putpixel((x, 24), c_hair_out)

    # Big Anime Eyes:
    # Left Eye (x=25..28, y=26..29)
    head.putpixel((25, 26), c_out); head.putpixel((26, 26), c_out); head.putpixel((27, 26), c_out); head.putpixel((28, 26), c_out)
    head.putpixel((25, 27), c_eye_pup); head.putpixel((26, 27), c_eye_pup); head.putpixel((27, 27), c_eye_wht); head.putpixel((28, 27), c_skin_mid)
    head.putpixel((25, 28), c_eye_iris); head.putpixel((26, 28), c_eye_iris); head.putpixel((27, 28), c_eye_iris); head.putpixel((28, 28), c_skin_mid)
    head.putpixel((26, 29), c_out); head.putpixel((27, 29), c_out)

    # Right Eye (x=35..38, y=26..29)
    head.putpixel((35, 26), c_out); head.putpixel((36, 26), c_out); head.putpixel((37, 26), c_out); head.putpixel((38, 26), c_out)
    head.putpixel((35, 27), c_skin_mid); head.putpixel((36, 27), c_eye_wht); head.putpixel((37, 27), c_eye_pup); head.putpixel((38, 27), c_eye_pup)
    head.putpixel((35, 28), c_skin_mid); head.putpixel((36, 28), c_eye_iris); head.putpixel((37, 28), c_eye_iris); head.putpixel((38, 28), c_eye_iris)
    head.putpixel((36, 29), c_out); head.putpixel((37, 29), c_out)

    # Cheeks Blush
    head.putpixel((25, 30), c_blush); head.putpixel((26, 30), c_blush)
    head.putpixel((37, 30), c_blush); head.putpixel((38, 30), c_blush)

    # Nose & Mouth
    head.putpixel((32, 30), c_skin_sh)
    head.putpixel((31, 32), c_out); head.putpixel((32, 32), c_out)

    # Neck (y=35..37, x=30..33)
    for y in (35, 36, 37):
        for x in range(30, 34):
            head.putpixel((x, y), c_skin_mid if y == 35 else c_skin_sh)

    # Long Hair Side Curls (flowing down past shoulders)
    for y in range(24, 45):
        # Left curl: x=19..23
        w_curl = 4 if y < 40 else 3
        for x in range(19, 19 + w_curl):
            if x == 19 or y == 44 or (x == 19 + w_curl - 1 and y > 35):
                head.putpixel((x, y), c_hair_out)
            elif x == 20 and y < 35:
                head.putpixel((x, y), c_hair_hi)
            else:
                head.putpixel((x, y), c_hair_mid)
        # Right curl: x=40..44
        for x in range(45 - w_curl, 45):
            if x == 44 or y == 44 or (x == 45 - w_curl and y > 35):
                head.putpixel((x, y), c_hair_out)
            elif x == 43 and y < 35:
                head.putpixel((x, y), c_hair_hi)
            else:
                head.putpixel((x, y), c_hair_mid)

    # 2. TORSO (idle_body.png)
    body = Image.new('RGBA', (w, h), (0, 0, 0, 0))

    # Scoop neckline showing neck skin at y=36..37 (x=30..33)
    for y in (36, 37):
        for x in range(30, 34):
            body.putpixel((x, y), c_skin_mid if y == 36 else c_skin_sh)

    # Orange Crop Top (y=37..44, width x=26..37)
    shirt_map = {
        37: (27, 36),
        38: (26, 37),
        39: (26, 37),
        40: (26, 37),
        41: (26, 37),
        42: (26, 37),
        43: (26, 37),
        44: (27, 36), # slender waist
    }
    for y, (xs, xe) in shirt_map.items():
        for x in range(xs, xe + 1):
            # preserve neckline opening
            if y == 37 and 30 <= x <= 33: continue
            if x in (xs, xe) or y in (37, 44):
                body.putpixel((x, y), c_shirt_out)
            elif x in (xs + 1, xe - 1):
                body.putpixel((x, y), c_shirt_sh)
            elif y in (39, 40) and 29 <= x <= 34:
                body.putpixel((x, y), c_shirt_hi)
            else:
                body.putpixel((x, y), c_shirt_mid)

    # Belt / Midriff at y=44..45
    for x in range(28, 36):
        body.putpixel((x, 44), c_shorts_hi)

    # Dark Shorts (y=45..49, width x=25..38)
    for y in range(45, 50):
        xs, xe = 26, 37
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 49 or (y == 48 and x == 31):
                body.putpixel((x, y), c_shorts_sh)
            elif y == 45:
                body.putpixel((x, y), c_shorts_hi)
            else:
                body.putpixel((x, y), c_shorts_mid)

    # 3. ARMS (idle_arm_left.png & idle_arm_right.png)
    arm_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    arm_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

    # Left Arm: x=23..26, y=37..46
    for y in range(37, 47):
        xs, xe = 23, 26
        if y == 37: xs += 1; xe -= 1 # rounded shoulder ball
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y in (37, 46):
                arm_l.putpixel((x, y), c_out)
            elif y in (37, 38):
                arm_l.putpixel((x, y), c_shirt_mid) # tank top strap
            elif x == xs + 1:
                arm_l.putpixel((x, y), c_skin_hi)
            else:
                arm_l.putpixel((x, y), c_skin_mid)

    # Right Arm: x=37..40, y=37..46
    for y in range(37, 47):
        xs, xe = 37, 40
        if y == 37: xs += 1; xe -= 1 # rounded shoulder ball
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y in (37, 46):
                arm_r.putpixel((x, y), c_out)
            elif y in (37, 38):
                arm_r.putpixel((x, y), c_shirt_mid)
            elif x == xe - 1:
                arm_r.putpixel((x, y), c_skin_hi)
            else:
                arm_r.putpixel((x, y), c_skin_mid)

    # 4. LEGS (idle_leg_left.png & idle_leg_right.png)
    leg_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    leg_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

    # Left Leg: x=27..31, y=48..57
    for y in range(48, 54): # Thigh & Knee
        xs, xe = 27, 31
        if y == 48: xs += 1; xe -= 1 # rounded hip joint
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 48: leg_l.putpixel((x, y), c_out)
            elif x == xs + 1: leg_l.putpixel((x, y), c_skin_hi)
            else: leg_l.putpixel((x, y), c_skin_mid)
    for y in range(54, 58): # Boot
        xs, xe = 27, 31
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 57: leg_l.putpixel((x, y), c_boot_sh)
            elif y == 54: leg_l.putpixel((x, y), c_boot_hi)
            else: leg_l.putpixel((x, y), c_boot_mid)

    # Right Leg: x=33..37, y=48..57
    for y in range(48, 54):
        xs, xe = 33, 37
        if y == 48: xs += 1; xe -= 1
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 48: leg_r.putpixel((x, y), c_out)
            elif x == xe - 1: leg_r.putpixel((x, y), c_skin_hi)
            else: leg_r.putpixel((x, y), c_skin_mid)
    for y in range(54, 58):
        xs, xe = 33, 37
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 57: leg_r.putpixel((x, y), c_boot_sh)
            elif y == 54: leg_r.putpixel((x, y), c_boot_hi)
            else: leg_r.putpixel((x, y), c_boot_mid)

    # Composite
    comp = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    comp.alpha_composite(leg_l)
    comp.alpha_composite(leg_r)
    comp.alpha_composite(body)
    comp.alpha_composite(arm_l)
    comp.alpha_composite(arm_r)
    comp.alpha_composite(head)

    head.save('scratch/hc_female_south_head.png')
    body.save('scratch/hc_female_south_body.png')
    arm_l.save('scratch/hc_female_south_arm_l.png')
    arm_r.save('scratch/hc_female_south_arm_r.png')
    leg_l.save('scratch/hc_female_south_leg_l.png')
    leg_r.save('scratch/hc_female_south_leg_r.png')
    comp.save('scratch/hc_female_south_comp.png')
    print('Generated handcrafted female south')

draw_female_south_handcrafted()
