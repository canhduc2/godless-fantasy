import os
from PIL import Image

OUT_ROOT = 'client/GodlessFantasyClient/Assets/Resources/Characters/Rig'
SCRATCH_PREVIEW = 'scratch/master_previews'
os.makedirs(SCRATCH_PREVIEW, exist_ok=True)

FEMALE_REF = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270681.png'
MALE_REF   = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270647.png'

DIRECTIONS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west']

def darken_c(c, f=0.74):
    return (int(c[0]*f), int(c[1]*f), int(c[2]*f), c[3])

def extract_clean_reference(sheet_path, is_female):
    im = Image.open(sheet_path).convert('RGBA')
    w, h = im.size
    cell_w, cell_h = w // 4, h // 2
    raw = {}
    for r in range(2):
        for c in range(4):
            crop = im.crop((c * cell_w, r * cell_h, (c + 1) * cell_w, (r + 1) * cell_h))
            # Remove white background
            clean = Image.new('RGBA', crop.size, (0, 0, 0, 0))
            for y in range(crop.size[1]):
                for x in range(crop.size[0]):
                    rgba = crop.getpixel((x, y))
                    if rgba[3] > 30 and not (rgba[0] > 240 and rgba[1] > 240 and rgba[2] > 240):
                        clean.putpixel((x, y), rgba)

            bbox = clean.getbbox()
            if bbox:
                char = clean.crop(bbox)
                cw, ch = char.size
                target_h = 47
                scale = target_h / ch
                target_w = max(1, int(cw * scale))
                char_scaled = char.resize((target_w, target_h), Image.Resampling.NEAREST)
                target = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
                px = (64 - target_w) // 2
                py = 57 - target_h
                target.paste(char_scaled, (px, py), char_scaled)
                raw[(r, c)] = target

    bases = {}
    if is_female:
        bases['south']      = raw[(0, 0)]
        bases['south_east'] = raw[(0, 1)]
        bases['east']       = raw[(0, 2)]
        bases['north_east'] = raw[(0, 3)]
        bases['north']      = raw[(1, 0)]
        # Flipped poses for left-facing directions (West, NW, SW)
        bases['north_west'] = raw[(0, 3)].transpose(Image.Transpose.FLIP_LEFT_RIGHT)
        bases['west']       = raw[(0, 2)].transpose(Image.Transpose.FLIP_LEFT_RIGHT)
        bases['south_west'] = raw[(0, 1)].transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    else:
        bases['south']      = raw[(0, 0)]
        bases['south_east'] = raw[(0, 1)]
        bases['east']       = raw[(0, 2)]
        bases['north_east'] = raw[(0, 3)]
        bases['north']      = raw[(1, 0)]
        bases['north_west'] = raw[(1, 1)]
        bases['west']       = raw[(1, 2)]
        bases['south_west'] = raw[(1, 3)]
    return bases

def build_character(gender):
    is_fem = (gender == 'female')
    sheet = FEMALE_REF if is_fem else MALE_REF
    bases = extract_clean_reference(sheet, is_fem)

    w, h = 64, 64

    # Character Color Palettes
    c_out      = (55, 30, 32, 255) if is_fem else (50, 30, 22, 255)
    c_skin_hi  = (255, 235, 225, 255) if is_fem else (255, 230, 210, 255)
    c_skin_mid = (251, 215, 195, 255) if is_fem else (253, 216, 188, 255)
    c_skin_sh  = (225, 175, 155, 255) if is_fem else (230, 185, 155, 255)
    c_blush    = (255, 185, 175, 255)

    c_hair_hi  = (210, 165, 145, 255) if is_fem else (195, 145, 105, 255)
    c_hair_mid = (158, 112, 110, 255) if is_fem else (155, 103, 70, 255)
    c_hair_sh  = (115, 78, 80, 255)  if is_fem else (115, 70, 45, 255)
    c_hair_out = (60, 35, 38, 255)   if is_fem else (55, 35, 25, 255)

    c_shirt_hi  = (255, 175, 140, 255) if is_fem else (255, 160, 130, 255)
    c_shirt_mid = (253, 144, 113, 255) if is_fem else (250, 135, 105, 255)
    c_shirt_sh  = (215, 105, 80, 255)  if is_fem else (210, 100, 75, 255)
    c_shirt_out = (90, 45, 40, 255)    if is_fem else (75, 35, 28, 255)

    c_shorts_hi  = (85, 75, 85, 255) if is_fem else (95, 95, 100, 255)
    c_shorts_mid = (55, 45, 55, 255) if is_fem else (60, 60, 65, 255)
    c_shorts_sh  = (35, 28, 35, 255) if is_fem else (40, 40, 45, 255)

    c_boot_hi  = (75, 65, 75, 255) if is_fem else c_skin_hi
    c_boot_mid = (45, 38, 45, 255) if is_fem else c_skin_mid
    c_boot_sh  = (25, 20, 25, 255) if is_fem else c_skin_sh

    for d in DIRECTIONS:
        base = bases[d]
        out_dir = f'{OUT_ROOT}/{gender}/{d}'
        os.makedirs(out_dir, exist_ok=True)

        # ─── 1. HEAD (idle_head.png) ──────────────────────────────────
        head = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        # Copy original head & hair from base artwork (y <= 36 + side curls for female)
        for y in range(h):
            for x in range(w):
                c = base.getpixel((x, y))
                if c[3] < 30: continue
                is_head = False
                if y <= 35:
                    is_head = True
                elif is_fem and y <= 46:
                    if d in ('south', 'north') and (x <= 24 or x >= 40): is_head = True
                    elif d == 'east' and x <= 26: is_head = True
                    elif d == 'west' and x >= 38: is_head = True
                    elif d in ('south_east', 'north_east') and x <= 25: is_head = True
                    elif d in ('south_west', 'north_west') and x >= 39: is_head = True
                elif not is_fem and y <= 36:
                    is_head = True

                if is_head:
                    head.putpixel((x, y), c)

        # CUTE CHIBI NOSE & PROFILE ENHANCEMENT FOR EAST & WEST
        if d == 'west':
            # West faces left: cute nose at x=22, y=25..26, mouth at y=28, chin at y=30
            head.putpixel((22, 25), c_out)
            head.putpixel((23, 25), c_skin_hi)
            head.putpixel((22, 26), c_out)
            head.putpixel((23, 26), c_skin_mid)
            head.putpixel((23, 27), c_out)
            head.putpixel((24, 27), c_skin_mid)
            head.putpixel((24, 28), c_out) # mouth indent
            head.putpixel((25, 28), c_skin_sh)
            head.putpixel((23, 29), c_out) # lower lip
            head.putpixel((24, 29), c_skin_mid)
            head.putpixel((24, 30), c_out) # chin
            head.putpixel((25, 30), c_skin_mid)
            # blush under eye
            head.putpixel((26, 28), c_blush)
            head.putpixel((27, 28), c_blush)
            # Neck base
            for y in (34, 35, 36):
                for x in range(27, 34):
                    if head.getpixel((x, y))[3] == 0:
                        head.putpixel((x, y), c_skin_mid if y <= 35 else c_skin_sh)

        elif d == 'east':
            # East faces right: cute nose at x=41, y=25..26
            head.putpixel((41, 25), c_out)
            head.putpixel((40, 25), c_skin_hi)
            head.putpixel((41, 26), c_out)
            head.putpixel((40, 26), c_skin_mid)
            head.putpixel((40, 27), c_out)
            head.putpixel((39, 27), c_skin_mid)
            head.putpixel((39, 28), c_out) # mouth
            head.putpixel((38, 28), c_skin_sh)
            head.putpixel((40, 29), c_out)
            head.putpixel((39, 29), c_skin_mid)
            head.putpixel((39, 30), c_out) # chin
            head.putpixel((38, 30), c_skin_mid)
            # blush
            head.putpixel((36, 28), c_blush)
            head.putpixel((37, 28), c_blush)
            for y in (34, 35, 36):
                for x in range(30, 37):
                    if head.getpixel((x, y))[3] == 0:
                        head.putpixel((x, y), c_skin_mid if y <= 35 else c_skin_sh)
        else:
            # Seal neck for front/back/diagonals
            for y in (35, 36):
                for x in range(29, 35):
                    if head.getpixel((x, y))[3] == 0:
                        head.putpixel((x, y), c_skin_mid if y == 35 else c_skin_sh)

        # ─── 2. PETITE SOLID TORSO (idle_body.png) ────────────────────
        # NOT bloated! Torso width: 13px in south/north, 11px in east/west
        body = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        if d in ('east', 'west'):
            xs_t, xe_t = (27, 37) if d == 'east' else (26, 36) # 11px wide
        elif d in ('south_east', 'north_east'):
            xs_t, xe_t = (26, 38)
        elif d in ('south_west', 'north_west'):
            xs_t, xe_t = (25, 37)
        else: # south, north
            xs_t, xe_t = (25, 38) # 14px wide! (Petite and snug)

        # Shirt (y=36..44)
        for y in range(36, 45):
            cur_xs, cur_xe = xs_t, xe_t
            if y == 36: cur_xs += 2; cur_xe -= 2 # collar taper
            elif y in (43, 44): cur_xs += 1; cur_xe -= 1 # slender waist

            for x in range(cur_xs, cur_xe + 1):
                if x in (cur_xs, cur_xe) or y == 36:
                    body.putpixel((x, y), c_shirt_out)
                elif x in (cur_xs + 1, cur_xe - 1):
                    body.putpixel((x, y), c_shirt_sh)
                elif y in (37, 38) and cur_xs + 2 <= x <= cur_xe - 2:
                    body.putpixel((x, y), c_shirt_hi)
                else:
                    body.putpixel((x, y), c_shirt_mid)

        # Shorts (y=45..49)
        for y in range(45, 50):
            cur_xs, cur_xe = xs_t + 1, xe_t - 1
            for x in range(cur_xs, cur_xe + 1):
                if x in (cur_xs, cur_xe) or y == 49:
                    body.putpixel((x, y), c_shorts_sh)
                elif y == 45:
                    body.putpixel((x, y), c_shorts_hi) # belt line
                else:
                    body.putpixel((x, y), c_shorts_mid)

        # ─── 3. SNUG ARMS (idle_arm_left.png & idle_arm_right.png) ────
        # Sitting snug right against torso!
        arm_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        arm_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        def draw_snug_arm(target, cx, cy_start, cy_end, is_dark):
            hi  = darken_c(c_skin_hi)  if is_dark else c_skin_hi
            mid = darken_c(c_skin_mid) if is_dark else c_skin_mid
            sh  = darken_c(c_skin_sh)  if is_dark else c_skin_sh
            out = darken_c(c_out)      if is_dark else c_out
            sh_col = darken_c(c_shirt_mid) if is_dark else c_shirt_mid

            w_arm = 4 if is_fem else 4
            xs = cx - (w_arm // 2)
            xe = xs + w_arm - 1

            for y in range(cy_start, cy_end + 1):
                cur_xs, cur_xe = xs, xe
                if y == cy_start: cur_xs += 1; cur_xe -= 1 # rounded shoulder ball
                for x in range(cur_xs, cur_xe + 1):
                    if x in (cur_xs, cur_xe) or y in (cy_start, cy_end):
                        target.putpixel((x, y), out)
                    elif y in (cy_start, cy_start + 1):
                        target.putpixel((x, y), sh_col) # shirt strap at shoulder
                    elif x == cur_xs + 1:
                        target.putpixel((x, y), hi)
                    else:
                        target.putpixel((x, y), mid)

        if d in ('south', 'north'):
            # Torso is x=25..38. Left arm is at x=22..25, Right arm is at x=38..41! (SNUG!)
            draw_snug_arm(arm_l, 24, 37, 46, False)
            draw_snug_arm(arm_r, 39, 37, 46, False)
        elif d == 'east':
            # Facing Right: Right arm is near (foreground), Left arm is far (background, darker)
            draw_snug_arm(arm_r, 34, 37, 46, False)
            draw_snug_arm(arm_l, 28, 37, 46, True)
        elif d == 'west':
            # Facing Left: Left arm is near (foreground), Right arm is far (background, darker)
            draw_snug_arm(arm_l, 29, 37, 46, False)
            draw_snug_arm(arm_r, 35, 37, 46, True)
        elif d in ('south_east', 'north_east'):
            draw_snug_arm(arm_r, 37, 37, 46, False)
            draw_snug_arm(arm_l, 25, 37, 46, True)
        elif d in ('south_west', 'north_west'):
            draw_snug_arm(arm_l, 26, 37, 46, False)
            draw_snug_arm(arm_r, 38, 37, 46, True)

        # ─── 4. SNUG LEGS (idle_leg_left.png & idle_leg_right.png) ────
        # BOTH LEGS DRAWN in East and West! Snug under hips!
        leg_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        leg_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        def draw_snug_leg(target, cx, cy_start, cy_end, is_dark, foot_dir):
            hi  = darken_c(c_skin_hi)  if is_dark else c_skin_hi
            mid = darken_c(c_skin_mid) if is_dark else c_skin_mid
            out = darken_c(c_out)      if is_dark else c_out

            b_hi  = darken_c(c_boot_hi)  if is_dark else c_boot_hi
            b_mid = darken_c(c_boot_mid) if is_dark else c_boot_mid
            b_sh  = darken_c(c_boot_sh)  if is_dark else c_boot_sh

            w_leg = 4 if is_fem else 5
            xs = cx - (w_leg // 2)
            xe = xs + w_leg - 1

            # Thigh (y=cy_start..53)
            for y in range(cy_start, 54):
                cur_xs, cur_xe = xs, xe
                if y == cy_start: cur_xs += 1; cur_xe -= 1 # rounded hip joint
                for x in range(cur_xs, cur_xe + 1):
                    if x in (cur_xs, cur_xe) or y == cy_start:
                        target.putpixel((x, y), out)
                    elif x == cur_xs + 1:
                        target.putpixel((x, y), hi)
                    else:
                        target.putpixel((x, y), mid)

            # Boot / Foot (y=54..cy_end)
            for y in range(54, cy_end + 1):
                cur_xs = xs + (foot_dir if foot_dir < 0 and y >= 55 else 0)
                cur_xe = xe + (foot_dir if foot_dir > 0 and y >= 55 else 0)
                for x in range(cur_xs, cur_xe + 1):
                    if x in (cur_xs, cur_xe) or y == cy_end:
                        target.putpixel((x, y), b_sh)
                    elif y == 54:
                        target.putpixel((x, y), b_hi)
                    else:
                        target.putpixel((x, y), b_mid)

        if d in ('south', 'north'):
            # Torso width x=25..38. Left leg at x=28, Right leg at x=35. (Snug, 2px gap!)
            draw_snug_leg(leg_l, 28, 48, 57, False, 0)
            draw_snug_leg(leg_r, 35, 48, 57, False, 0)
        elif d == 'east':
            # BOTH LEGS: Near leg (right), Far leg (left, darker). Feet point right (+1)!
            draw_snug_leg(leg_r, 33, 48, 57, False, +1) # Near
            draw_snug_leg(leg_l, 29, 48, 57, True,  +1) # Far
        elif d == 'west':
            # BOTH LEGS: Near leg (left), Far leg (right, darker). Feet point left (-1)!
            draw_snug_leg(leg_l, 30, 48, 57, False, -1) # Near
            draw_snug_leg(leg_r, 34, 48, 57, True,  -1) # Far
        elif d in ('south_east', 'north_east'):
            draw_snug_leg(leg_r, 34, 48, 57, False, +1)
            draw_snug_leg(leg_l, 28, 48, 57, True,  +1)
        elif d in ('south_west', 'north_west'):
            draw_snug_leg(leg_l, 29, 48, 57, False, -1)
            draw_snug_leg(leg_r, 35, 48, 57, True,  -1)

        # ─── 5. FULL BODY COMPOSITE (body.png) ────────────────────────
        comp = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        if d in ('east', 'south_east', 'north_east'):
            comp.alpha_composite(arm_l)
            comp.alpha_composite(leg_l)
            comp.alpha_composite(body)
            comp.alpha_composite(leg_r)
            comp.alpha_composite(head)
            comp.alpha_composite(arm_r)
        elif d in ('west', 'south_west', 'north_west'):
            comp.alpha_composite(arm_r)
            comp.alpha_composite(leg_r)
            comp.alpha_composite(body)
            comp.alpha_composite(leg_l)
            comp.alpha_composite(head)
            comp.alpha_composite(arm_l)
        elif d == 'north':
            comp.alpha_composite(arm_l)
            comp.alpha_composite(arm_r)
            comp.alpha_composite(leg_l)
            comp.alpha_composite(leg_r)
            comp.alpha_composite(body)
            comp.alpha_composite(head)
        else: # south
            comp.alpha_composite(leg_l)
            comp.alpha_composite(leg_r)
            comp.alpha_composite(body)
            comp.alpha_composite(arm_l)
            comp.alpha_composite(arm_r)
            comp.alpha_composite(head)

        head.save(f'{out_dir}/idle_head.png')
        body.save(f'{out_dir}/idle_body.png')
        arm_l.save(f'{out_dir}/idle_arm_left.png')
        arm_r.save(f'{out_dir}/idle_arm_right.png')
        leg_l.save(f'{out_dir}/idle_leg_left.png')
        leg_r.save(f'{out_dir}/idle_leg_right.png')
        comp.save(f'{out_dir}/body.png')

        comp.save(f'{SCRATCH_PREVIEW}/{gender}_{d}.png')

    print(f'Finished master rig generation for {gender}')

build_character('female')
build_character('male')
print('MASTER RIG COMPLETED SUCCESSFULLY!')
