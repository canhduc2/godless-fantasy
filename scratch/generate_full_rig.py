import os
from PIL import Image

OUT_ROOT = 'client/GodlessFantasyClient/Assets/Resources/Characters/Rig'
SCRATCH_PREVIEW = 'scratch/rig_previews'
os.makedirs(SCRATCH_PREVIEW, exist_ok=True)

FEMALE_REF = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270681.png'
MALE_REF   = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270647.png'

DIRECTIONS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west']

def darken(color, factor=0.72):
    r, g, b, a = color
    if a < 30: return (0, 0, 0, 0)
    return (int(r * factor), int(g * factor), int(b * factor), a)

def get_color_at(im, x, y):
    if 0 <= x < im.size[0] and 0 <= y < im.size[1]:
        return im.getpixel((x, y))
    return (0, 0, 0, 0)

def extract_reference_bases(ref_path, is_female):
    im = Image.open(ref_path).convert('RGBA')
    w, h = im.size
    cell_w, cell_h = w // 4, h // 2
    raw = {}
    for r in range(2):
        for c in range(4):
            crop = im.crop((c * cell_w, r * cell_h, (c + 1) * cell_w, (r + 1) * cell_h))
            bbox = crop.getbbox()
            if bbox:
                char = crop.crop(bbox)
                cw, ch = char.size
                target_h = 48
                scale = target_h / ch
                target_w = max(1, int(cw * scale))
                char_scaled = char.resize((target_w, target_h), Image.Resampling.NEAREST)
                target = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
                px = (64 - target_w) // 2
                py = 58 - target_h
                target.paste(char_scaled, (px, py), char_scaled)
                raw[(r, c)] = target

    bases = {}
    if is_female:
        bases['south']      = raw[(0, 0)]
        bases['south_east'] = raw[(0, 1)]
        bases['east']       = raw[(0, 2)]
        bases['north_east'] = raw[(0, 3)]
        bases['north']      = raw[(1, 0)]
        # Fix female west mistakes: mirror east and diagonals so she faces left
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

def build_rig_for_character(gender):
    is_fem = (gender == 'female')
    ref_file = FEMALE_REF if is_fem else MALE_REF
    bases = extract_reference_bases(ref_file, is_fem)

    # Color tokens
    c_skin_hi  = (255, 230, 218, 255) if is_fem else (255, 228, 205, 255)
    c_skin_mid = (251, 215, 195, 255) if is_fem else (253, 216, 188, 255)
    c_skin_sh  = (225, 175, 155, 255) if is_fem else (230, 185, 155, 255)
    c_skin_out = (80, 45, 40, 255)    if is_fem else (60, 35, 25, 255)

    c_shirt_hi  = (255, 175, 140, 255) if is_fem else (255, 160, 130, 255)
    c_shirt_mid = (253, 144, 113, 255) if is_fem else (250, 135, 105, 255)
    c_shirt_sh  = (215, 105, 80, 255)  if is_fem else (210, 100, 75, 255)
    c_shirt_out = (90, 45, 40, 255)    if is_fem else (70, 30, 25, 255)

    c_shorts_hi  = (85, 75, 85, 255) if is_fem else (95, 95, 100, 255)
    c_shorts_mid = (55, 45, 55, 255) if is_fem else (60, 60, 65, 255)
    c_shorts_sh  = (35, 28, 35, 255) if is_fem else (40, 40, 45, 255)
    c_shorts_out = (25, 20, 25, 255) if is_fem else (25, 25, 25, 255)

    c_boot_hi  = (75, 65, 75, 255) if is_fem else c_skin_hi
    c_boot_mid = (45, 38, 45, 255) if is_fem else c_skin_mid
    c_boot_out = (25, 20, 25, 255) if is_fem else c_skin_out

    for d in DIRECTIONS:
        base = bases[d]
        out_dir = f'{OUT_ROOT}/{gender}/{d}'
        os.makedirs(out_dir, exist_ok=True)

        w, h = 64, 64

        # ─── 1. HEAD (idle_head.png) ──────────────────────────────────
        head = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        # Keep hair and face (y <= 36)
        for y in range(0, 37):
            for x in range(w):
                c = base.getpixel((x, y))
                if c[3] > 30:
                    head.putpixel((x, y), c)
        # Inpaint/seal chin & neck curve at y=35..37
        for y in (35, 36, 37):
            for x in range(29, 36):
                if head.getpixel((x, y))[3] == 0:
                    head.putpixel((x, y), c_skin_mid if y <= 36 else c_skin_sh)

        # ─── 2. SOLID TORSO (idle_body.png) ───────────────────────────
        body = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        # Detect torso bounds from base
        torso_pts = [(x, y) for y in range(36, 50) for x in range(w) if base.getpixel((x, y))[3] > 30]
        if torso_pts:
            min_y = min(p[1] for p in torso_pts)
            max_y = max(p[1] for p in torso_pts)
        else:
            min_y, max_y = 36, 49

        # Draw solid, seamless shirt (y=36..45) with rounded shoulders extending to edges
        for y in range(36, 46):
            # width based on direction
            if d in ('east', 'west'):
                # profile
                xs, xe = (27, 38) if d == 'east' else (26, 37)
            elif d in ('south_east', 'north_east'):
                xs, xe = (25, 41)
            elif d in ('south_west', 'north_west'):
                xs, xe = (23, 39)
            else: # south, north
                xs, xe = (22, 42)

            # Neck taper at top
            if y == 36: xs += 4; xe -= 4
            elif y == 37: xs += 2; xe -= 2

            for x in range(xs, xe + 1):
                if x in (xs, xe) or y == 36:
                    body.putpixel((x, y), c_shirt_out)
                elif x in (xs + 1, xe - 1):
                    body.putpixel((x, y), c_shirt_sh)
                elif y in (37, 38) and xs + 3 <= x <= xe - 3:
                    body.putpixel((x, y), c_shirt_hi)
                else:
                    body.putpixel((x, y), c_shirt_mid)

        # Draw solid shorts (y=46..49)
        for y in range(46, 50):
            if d in ('east', 'west'):
                xs, xe = (27, 38) if d == 'east' else (26, 37)
            elif d in ('south_east', 'north_east'):
                xs, xe = (24, 40)
            elif d in ('south_west', 'north_west'):
                xs, xe = (24, 40)
            else:
                xs, xe = (23, 41)

            for x in range(xs, xe + 1):
                if x in (xs, xe) or y == 49:
                    body.putpixel((x, y), c_shorts_out)
                elif y == 46:
                    body.putpixel((x, y), c_shorts_sh) # belt line
                else:
                    body.putpixel((x, y), c_shorts_mid)

        # ─── 3. ARMS (idle_arm_left.png & idle_arm_right.png) ──────────
        arm_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        arm_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        def draw_arm_limb(target, cx, cy_start, cy_end, is_dark):
            col_hi  = darken(c_skin_hi)  if is_dark else c_skin_hi
            col_mid = darken(c_skin_mid) if is_dark else c_skin_mid
            col_sh  = darken(c_skin_sh)  if is_dark else c_skin_sh
            col_out = darken(c_skin_out) if is_dark else c_skin_out

            for y in range(cy_start, cy_end + 1):
                # Rounded shoulder at top (y=cy_start), hand at bottom
                w_arm = 4 if is_fem else 5
                xs = cx - (w_arm // 2)
                xe = xs + w_arm - 1
                for x in range(xs, xe + 1):
                    if x in (xs, xe) or y in (cy_start, cy_end):
                        target.putpixel((x, y), col_out)
                    elif x == xs + 1:
                        target.putpixel((x, y), col_hi)
                    elif x == xe - 1:
                        target.putpixel((x, y), col_sh)
                    else:
                        target.putpixel((x, y), col_mid)

        if d in ('south', 'north'):
            # Left arm at x=22, Right arm at x=42
            draw_arm_limb(arm_l, 21, 37, 46, False)
            draw_arm_limb(arm_r, 43, 37, 46, False)
        elif d == 'east':
            # Facing right: Right arm is near (foreground), Left arm is far (background, darker)
            draw_arm_limb(arm_r, 36, 37, 46, False)
            draw_arm_limb(arm_l, 29, 37, 46, True)
        elif d == 'west':
            # Facing left: Left arm is near, Right arm is far (darker)
            draw_arm_limb(arm_l, 28, 37, 46, False)
            draw_arm_limb(arm_r, 35, 37, 46, True)
        elif d in ('south_east', 'north_east'):
            draw_arm_limb(arm_r, 39, 37, 46, False)
            draw_arm_limb(arm_l, 24, 37, 46, True)
        elif d in ('south_west', 'north_west'):
            draw_arm_limb(arm_l, 25, 37, 46, False)
            draw_arm_limb(arm_r, 40, 37, 46, True)

        # ─── 4. LEGS (idle_leg_left.png & idle_leg_right.png) ──────────
        # CRUCIAL: USER STATED BOTH LEGS MUST BE DRAWN IN EAST & WEST!
        leg_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        leg_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        def draw_leg_limb(target, cx, cy_start, cy_end, is_dark, foot_dir):
            col_hi   = darken(c_skin_hi)   if is_dark else c_skin_hi
            col_mid  = darken(c_skin_mid)  if is_dark else c_skin_mid
            col_sh   = darken(c_skin_sh)   if is_dark else c_skin_sh
            col_out  = darken(c_skin_out)  if is_dark else c_skin_out

            b_hi  = darken(c_boot_hi)  if is_dark else c_boot_hi
            b_mid = darken(c_boot_mid) if is_dark else c_boot_mid
            b_out = darken(c_boot_out) if is_dark else c_boot_out

            w_leg = 5 if is_fem else 6
            xs = cx - (w_leg // 2)
            xe = xs + w_leg - 1

            # Thigh & Knee (cy_start to 53)
            for y in range(cy_start, 54):
                for x in range(xs, xe + 1):
                    if x in (xs, xe) or y == cy_start:
                        target.putpixel((x, y), col_out)
                    elif x == xs + 1:
                        target.putpixel((x, y), col_hi)
                    else:
                        target.putpixel((x, y), col_mid)

            # Boot / Foot (54 to cy_end)
            for y in range(54, cy_end + 1):
                # extend foot in foot_dir (-1 = left, +1 = right, 0 = neutral)
                fxs = xs + (foot_dir if foot_dir < 0 and y >= 55 else 0)
                fxe = xe + (foot_dir if foot_dir > 0 and y >= 55 else 0)
                for x in range(fxs, fxe + 1):
                    if x in (fxs, fxe) or y == cy_end:
                        target.putpixel((x, y), b_out)
                    elif y == 54:
                        target.putpixel((x, y), b_hi)
                    else:
                        target.putpixel((x, y), b_mid)

        if d in ('south', 'north'):
            draw_leg_limb(leg_l, 27, 48, 57, False, 0)
            draw_leg_limb(leg_r, 37, 48, 57, False, 0)
        elif d == 'east':
            # Facing Right:
            # Right leg is near (foreground), toe pointing right (+1)
            # Left leg is far (background, darker), toe pointing right (+1)
            draw_leg_limb(leg_r, 35, 48, 57, False, +1)
            draw_leg_limb(leg_l, 28, 48, 57, True,  +1)
        elif d == 'west':
            # Facing Left:
            # Left leg is near (foreground), toe pointing left (-1)
            # Right leg is far (background, darker), toe pointing left (-1)
            draw_leg_limb(leg_l, 29, 48, 57, False, -1)
            draw_leg_limb(leg_r, 36, 48, 57, True,  -1)
        elif d in ('south_east', 'north_east'):
            draw_leg_limb(leg_r, 35, 48, 57, False, +1)
            draw_leg_limb(leg_l, 27, 48, 57, True,  +1)
        elif d in ('south_west', 'north_west'):
            draw_leg_limb(leg_l, 29, 48, 57, False, -1)
            draw_leg_limb(leg_r, 37, 48, 57, True,  -1)

        # ─── 5. FULL BODY COMPOSITE (body.png) ────────────────────────
        # Clean composite of all parts for icons, fallback, and equipment preview
        comp = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        if d in ('east', 'south_east', 'north_east'):
            # Far limbs -> Body -> Near Leg -> Head -> Near Arm
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

        # Save files
        head.save(f'{out_dir}/idle_head.png')
        body.save(f'{out_dir}/idle_body.png')
        arm_l.save(f'{out_dir}/idle_arm_left.png')
        arm_r.save(f'{out_dir}/idle_arm_right.png')
        leg_l.save(f'{out_dir}/idle_leg_left.png')
        leg_r.save(f'{out_dir}/idle_leg_right.png')
        comp.save(f'{out_dir}/body.png')

        # Also save preview for inspection
        comp.save(f'{SCRATCH_PREVIEW}/{gender}_{d}.png')

    print(f'Finished generating rig parts for {gender}')

build_rig_for_character('female')
build_rig_for_character('male')
print('ALL RIG PARTS GENERATED SUCCESSFULLY!')
