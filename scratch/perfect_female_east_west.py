from PIL import Image

def build_refined_female_east_west():
    w, h = 64, 64

    # Palettes
    c_out      = (55, 30, 32, 255) # dark outline
    c_hair_hi  = (210, 165, 145, 255)
    c_hair_mid = (158, 112, 110, 255)
    c_hair_sh  = (115, 78, 80, 255)
    c_hair_out = (60, 35, 38, 255)
    c_headband = (245, 140, 80, 255)
    c_clip     = (45, 45, 50, 255)

    c_skin_hi  = (255, 235, 225, 255)
    c_skin_mid = (251, 215, 195, 255)
    c_skin_sh  = (225, 175, 155, 255)
    c_blush    = (255, 185, 175, 255)

    c_eye_iris = (255, 130, 85, 255)
    c_eye_pup  = (180, 50, 40, 255)
    c_eye_wht  = (255, 255, 255, 255)

    c_shirt_hi  = (255, 175, 140, 255)
    c_shirt_mid = (253, 144, 113, 255)
    c_shirt_sh  = (215, 105, 80, 255)
    c_shirt_out = (90, 45, 40, 255)

    c_shorts_hi  = (85, 75, 85, 255)
    c_shorts_mid = (55, 45, 55, 255)
    c_shorts_sh  = (35, 28, 35, 255)

    c_boot_hi  = (75, 65, 75, 255)
    c_boot_mid = (45, 38, 45, 255)
    c_boot_sh  = (25, 20, 25, 255)

    def darken_c(c, f=0.74):
        return (int(c[0]*f), int(c[1]*f), int(c[2]*f), c[3])

    # ══════════════════════════════════════════════════════════════════
    #  FEMALE WEST (Facing Left)
    # ══════════════════════════════════════════════════════════════════

    # 1. HEAD (idle_head)
    head_w = Image.new('RGBA', (w, h), (0, 0, 0, 0))

    # Base hair volume (back & top): x=25..44, y=10..36
    # Hair back cascade: x=34..45, y=26..46
    for y in range(10, 47):
        for x in range(20, 47):
            # Outer hair boundary
            if y < 14 and (x < 28 or x > 41): continue
            if y in (10, 11) and (x < 30 or x > 39): continue
            # Back cascade
            if y > 35 and (x < 33 or x > 44): continue
            if y > 42 and (x < 34 or x > 42): continue

            # Shading
            if x >= 42 or y >= 44 or (y > 36 and x == 33):
                head_w.putpixel((x, y), c_hair_out)
            elif x in (38, 39, 40) and y in (14, 15, 16):
                head_w.putpixel((x, y), c_hair_hi)
            elif x >= 36 or y >= 38:
                head_w.putpixel((x, y), c_hair_sh)
            else:
                head_w.putpixel((x, y), c_hair_mid)

    # Cute Headband & Clip on hair
    for x in range(32, 38):
        head_w.putpixel((x, 14), c_headband)
    for (x, y) in [(30, 18), (31, 18), (30, 19), (32, 19), (32, 20), (33, 20)]:
        head_w.putpixel((x, y), c_clip)

    # Face & Skin Profile:
    # Forehead curves down: y=20..24, x=24..33
    # Cute little chibi nose jutting to x=22 at y=25..26!
    # Mouth indent at x=24, y=28
    # Rounded chin at x=24..26, y=30..31
    # Jawline back to ear at x=31, y=31..33
    for y in range(20, 35):
        # determine front profile x for this row
        if y in (20, 21):   xf = 26 # forehead slope
        elif y in (22, 23): xf = 25
        elif y == 24:       xf = 24
        elif y in (25, 26): xf = 22 # CUTE NOSE PROTRUSION!
        elif y == 27:       xf = 23
        elif y == 28:       xf = 24 # mouth indent
        elif y == 29:       xf = 23 # lower lip
        elif y == 30:       xf = 24 # chin
        elif y == 31:       xf = 25 # jaw
        elif y in (32, 33): xf = 27 # neck base
        else:               xf = 29

        xb = 34 if y <= 31 else 32 # back of cheek / neck
        for x in range(xf, xb + 1):
            if x == xf:
                head_w.putpixel((x, y), c_out) # profile outline
            elif x == xf + 1:
                head_w.putpixel((x, y), c_skin_hi) # profile highlight
            elif y >= 32:
                head_w.putpixel((x, y), c_skin_sh) # neck shadow
            else:
                head_w.putpixel((x, y), c_skin_mid)

    # Big Expressive Anime Eye (Facing Left):
    # Eye is at x=25..28, y=24..27
    head_w.putpixel((25, 24), c_out)
    head_w.putpixel((26, 24), c_out)
    head_w.putpixel((27, 24), c_out) # top lash
    head_w.putpixel((25, 25), c_eye_pup)
    head_w.putpixel((26, 25), c_eye_iris)
    head_w.putpixel((27, 25), c_eye_wht)
    head_w.putpixel((25, 26), c_eye_iris)
    head_w.putpixel((26, 26), c_eye_iris)
    head_w.putpixel((25, 27), c_out) # lower lash
    head_w.putpixel((26, 27), c_out)
    # Cute blush under eye
    head_w.putpixel((26, 28), c_blush)
    head_w.putpixel((27, 28), c_blush)

    # Bangs over forehead:
    for (x, y) in [(25, 20), (25, 21), (26, 21), (26, 22), (27, 22), (28, 23), (29, 23)]:
        head_w.putpixel((x, y), c_hair_mid)
    head_w.putpixel((25, 22), c_hair_out)
    head_w.putpixel((26, 23), c_hair_out)

    # Ear at x=31, y=28..29
    head_w.putpixel((31, 28), c_skin_sh)
    head_w.putpixel((31, 29), c_skin_mid)

    # 2. SOLID TORSO (idle_body)
    body_w = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    # Neck collar at y=35..36
    for x in range(28, 33):
        body_w.putpixel((x, 35), c_skin_sh)
        body_w.putpixel((x, 36), c_shirt_out)

    # Orange crop top: y=37..44, profile curve x=26..36
    for y in range(37, 45):
        if y in (37, 38): xs, xe = 27, 35
        elif y in (39, 40): xs, xe = 26, 36 # chest & back curve
        elif y in (41, 42): xs, xe = 26, 35 # slender waist
        else:               xs, xe = 27, 35

        for x in range(xs, xe + 1):
            if x in (xs, xe) or y in (37, 44):
                body_w.putpixel((x, y), c_out)
            elif x in (xs + 1, xs + 2) and y in (38, 39, 40):
                body_w.putpixel((x, y), c_shirt_hi) # chest highlight
            elif x >= xe - 1:
                body_w.putpixel((x, y), c_shirt_sh) # back shadow
            else:
                body_w.putpixel((x, y), c_shirt_mid)

    # Midriff hint at y=45
    for x in range(28, 34):
        body_w.putpixel((x, 45), c_skin_mid)

    # Dark shorts: y=46..49, x=26..36
    for y in range(46, 50):
        xs, xe = 26, 36
        for x in range(xs, xe + 1):
            if x in (xs, xe) or y == 49:
                body_w.putpixel((x, y), c_shorts_sh)
            elif y == 46:
                body_w.putpixel((x, y), c_shorts_hi) # belt line
            else:
                body_w.putpixel((x, y), c_shorts_mid)

    # 3. LEGS (BOTH LEGS DRAWN!)
    leg_l_w = Image.new('RGBA', (w, h), (0, 0, 0, 0)) # Near leg (Left)
    leg_r_w = Image.new('RGBA', (w, h), (0, 0, 0, 0)) # Far leg (Right, darker)

    def draw_leg_w(target, cx, is_dark):
        sk_hi  = darken_c(c_skin_hi)  if is_dark else c_skin_hi
        sk_mid = darken_c(c_skin_mid) if is_dark else c_skin_mid
        sk_sh  = darken_c(c_skin_sh)  if is_dark else c_skin_sh
        bt_hi  = darken_c(c_boot_hi)  if is_dark else c_boot_hi
        bt_mid = darken_c(c_boot_mid) if is_dark else c_boot_mid
        bt_sh  = darken_c(c_boot_sh)  if is_dark else c_boot_sh

        # Thigh & Knee: y=48..53, width 5
        for y in range(48, 54):
            xs = cx - 2
            xe = cx + 2
            if y == 48: xs += 1; xe -= 1 # rounded hip joint
            for x in range(xs, xe + 1):
                if x in (xs, xe) or y == 48:
                    target.putpixel((x, y), bt_sh)
                elif x == xs + 1:
                    target.putpixel((x, y), sk_hi)
                else:
                    target.putpixel((x, y), sk_mid)

        # Boot pointing LEFT: y=54..57
        for y in range(54, 58):
            xs = cx - 2 - (1 if y >= 55 else 0) # toe points left
            xe = cx + 2
            for x in range(xs, xe + 1):
                if x in (xs, xe) or y == 57:
                    target.putpixel((x, y), bt_sh)
                elif y == 54:
                    target.putpixel((x, y), bt_hi)
                else:
                    target.putpixel((x, y), bt_mid)

    draw_leg_w(leg_l_w, 29, False) # Near leg
    draw_leg_w(leg_r_w, 35, True)  # Far leg (darker)

    # 4. ARMS (BOTH ARMS DRAWN!)
    arm_l_w = Image.new('RGBA', (w, h), (0, 0, 0, 0)) # Near arm (Left)
    arm_r_w = Image.new('RGBA', (w, h), (0, 0, 0, 0)) # Far arm (Right, darker)

    def draw_arm_w(target, cx, is_dark):
        sk_hi  = darken_c(c_skin_hi)  if is_dark else c_skin_hi
        sk_mid = darken_c(c_skin_mid) if is_dark else c_skin_mid
        sk_sh  = darken_c(c_skin_sh)  if is_dark else c_skin_sh
        sh_mid = darken_c(c_shirt_mid)if is_dark else c_shirt_mid

        for y in range(37, 47):
            xs = cx - 1
            xe = cx + 2
            for x in range(xs, xe + 1):
                if x in (xs, xe) or y in (37, 46):
                    target.putpixel((x, y), c_out)
                elif y in (37, 38):
                    target.putpixel((x, y), sh_mid) # shoulder strap
                elif x == xs + 1:
                    target.putpixel((x, y), sk_hi)
                else:
                    target.putpixel((x, y), sk_mid)

    draw_arm_w(arm_l_w, 28, False) # Near arm
    draw_arm_w(arm_r_w, 35, True)  # Far arm (darker)

    # Save Female West
    dir_w = 'client/GodlessFantasyClient/Assets/Resources/Characters/Rig/female/west'
    head_w.save(f'{dir_w}/idle_head.png')
    body_w.save(f'{dir_w}/idle_body.png')
    arm_l_w.save(f'{dir_w}/idle_arm_left.png')
    arm_r_w.save(f'{dir_w}/idle_arm_right.png')
    leg_l_w.save(f'{dir_w}/idle_leg_left.png')
    leg_r_w.save(f'{dir_w}/idle_leg_right.png')

    # Composite for West body.png
    comp_w = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    comp_w.alpha_composite(arm_r_w)
    comp_w.alpha_composite(leg_r_w)
    comp_w.alpha_composite(body_w)
    comp_w.alpha_composite(leg_l_w)
    comp_w.alpha_composite(head_w)
    comp_w.alpha_composite(arm_l_w)
    comp_w.save(f'{dir_w}/body.png')
    comp_w.save('scratch/rig_previews_v2/female_west_perfect.png')

    # ══════════════════════════════════════════════════════════════════
    #  FEMALE EAST (Facing Right) — Exact Mirror
    # ══════════════════════════════════════════════════════════════════
    dir_e = 'client/GodlessFantasyClient/Assets/Resources/Characters/Rig/female/east'
    head_e = head_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    body_e = body_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    # In East: Right limbs are Near, Left limbs are Far
    arm_r_e = arm_l_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT) # Near arm
    arm_l_e = arm_r_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT) # Far arm
    leg_r_e = leg_l_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT) # Near leg
    leg_l_e = leg_r_w.transpose(Image.Transpose.FLIP_LEFT_RIGHT) # Far leg

    head_e.save(f'{dir_e}/idle_head.png')
    body_e.save(f'{dir_e}/idle_body.png')
    arm_l_e.save(f'{dir_e}/idle_arm_left.png')
    arm_r_e.save(f'{dir_e}/idle_arm_right.png')
    leg_l_e.save(f'{dir_e}/idle_leg_left.png')
    leg_r_e.save(f'{dir_e}/idle_leg_right.png')

    comp_e = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    comp_e.alpha_composite(arm_l_e)
    comp_e.alpha_composite(leg_l_e)
    comp_e.alpha_composite(body_e)
    comp_e.alpha_composite(leg_r_e)
    comp_e.alpha_composite(head_e)
    comp_e.alpha_composite(arm_r_e)
    comp_e.save(f'{dir_e}/body.png')
    comp_e.save('scratch/rig_previews_v2/female_east_perfect.png')

    print('Built perfect Female East & West!')

build_refined_female_east_west()
