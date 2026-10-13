import os
from PIL import Image

os.makedirs('scratch/test_parts', exist_ok=True)

def create_female_south():
    base = Image.open('client/GodlessFantasyClient/Assets/Resources/Characters/Rig/female/south/body.png').convert('RGBA')
    w, h = 64, 64

    # 1. HEAD: Hair and face down to chin/neck
    head = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    for y in range(0, 36):
        for x in range(w):
            c = base.getpixel((x, y))
            if c[3] > 30:
                head.putpixel((x, y), c)
    neck_skin = (251, 215, 195, 255)
    neck_shadow = (215, 175, 155, 255)
    for x in range(30, 34):
        head.putpixel((x, 35), neck_skin)
        head.putpixel((x, 36), neck_shadow)
    head.save('scratch/test_parts/female_south_head.png')

    # 2. BODY: Solid torso (shirt + shorts) with FULL rounded shoulders and full pelvis
    body = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    c_shirt_hi = (255, 175, 140, 255)
    c_shirt_mid = (253, 144, 113, 255)
    c_shirt_sh = (215, 105, 80, 255)
    c_shirt_out = (90, 45, 40, 255)
    c_shorts_mid = (55, 45, 55, 255)
    c_shorts_sh = (35, 28, 35, 255)
    c_shorts_out = (25, 20, 25, 255)

    for y in range(36, 46):
        if y == 36: xs, xe = 27, 37
        elif y == 37: xs, xe = 25, 39
        elif y in (38, 39): xs, xe = 23, 41
        elif y in (40, 41, 42): xs, xe = 22, 42
        elif y in (43, 44): xs, xe = 23, 41
        else: xs, xe = 24, 40
        for x in range(xs, xe + 1):
            if x in (xs, xe):
                body.putpixel((x, y), c_shirt_out)
            elif x in (xs + 1, xe - 1):
                body.putpixel((x, y), c_shirt_sh)
            elif y in (37, 38) and 28 <= x <= 36:
                body.putpixel((x, y), c_shirt_hi)
            else:
                body.putpixel((x, y), c_shirt_mid)

    for y in range(46, 50):
        if y == 46: xs, xe = 24, 40
        elif y in (47, 48): xs, xe = 23, 41
        else: xs, xe = 24, 40
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 49:
                body.putpixel((x, y), c_shorts_out)
            elif y == 46:
                body.putpixel((x, y), c_shorts_sh)
            else:
                body.putpixel((x, y), c_shorts_mid)
    body.save('scratch/test_parts/female_south_body.png')

    # 3. ARMS
    c_skin_hi = (255, 230, 218, 255)
    c_skin_mid = (251, 215, 195, 255)
    c_skin_sh = (225, 175, 155, 255)
    c_skin_out = (80, 45, 40, 255)

    def draw_arm(is_left):
        arm = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        cx = 22 if is_left else 42
        for y in range(37, 47):
            w_arm = 4
            x_start = cx - 2 if is_left else cx - 1
            x_end = x_start + w_arm
            for x in range(x_start, x_end):
                if x in (x_start, x_end - 1) or y in (37, 46):
                    arm.putpixel((x, y), c_skin_out)
                elif (is_left and x == x_start + 1) or (not is_left and x == x_end - 2):
                    arm.putpixel((x, y), c_skin_hi)
                else:
                    arm.putpixel((x, y), c_skin_mid)
        return arm

    draw_arm(True).save('scratch/test_parts/female_south_arm_left.png')
    draw_arm(False).save('scratch/test_parts/female_south_arm_right.png')

    # 4. LEGS
    c_boot_hi = (75, 65, 75, 255)
    c_boot_mid = (45, 38, 45, 255)
    c_boot_out = (25, 20, 25, 255)

    def draw_leg(is_left):
        leg = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        cx = 28 if is_left else 36
        for y in range(48, 54):
            for x in range(cx - 3, cx + 2):
                if x in (cx - 3, cx + 1) or y == 48:
                    leg.putpixel((x, y), c_skin_out)
                elif x == cx - 2:
                    leg.putpixel((x, y), c_skin_hi)
                else:
                    leg.putpixel((x, y), c_skin_mid)
        for y in range(54, 58):
            for x in range(cx - 3, cx + 2):
                if x in (cx - 3, cx + 1) or y == 57:
                    leg.putpixel((x, y), c_boot_out)
                elif y == 54:
                    leg.putpixel((x, y), c_boot_hi)
                else:
                    leg.putpixel((x, y), c_boot_mid)
        return leg

    draw_leg(True).save('scratch/test_parts/female_south_leg_left.png')
    draw_leg(False).save('scratch/test_parts/female_south_leg_right.png')
    print('Generated Female South test parts')

create_female_south()
