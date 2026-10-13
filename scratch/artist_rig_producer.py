import os
from PIL import Image

OUT_ROOT = 'client/GodlessFantasyClient/Assets/Resources/Characters/Rig'
SCRATCH_PREVIEW = 'scratch/final_previews'
os.makedirs(SCRATCH_PREVIEW, exist_ok=True)

FEMALE_REF = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270681.png'
MALE_REF   = 'C:/Users/OS/.gemini/antigravity-ide/brain/63d3153d-4724-4ae7-aa86-426d7c5db938/.user_uploaded/media_1791863270647.png'

DIRECTIONS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west']

def darken(color, factor=0.72):
    r, g, b, a = color
    if a < 30: return (0, 0, 0, 0)
    return (int(r * factor), int(g * factor), int(b * factor), a)

def get_base_sprites(sheet_path, is_female):
    im = Image.open(sheet_path).convert('RGBA')
    w, h = im.size
    cell_w, cell_h = w // 4, h // 2
    raw = {}
    for r in range(2):
        for c in range(4):
            crop = im.crop((c * cell_w, r * cell_h, (c + 1) * cell_w, (r + 1) * cell_h))
            clean = Image.new('RGBA', crop.size, (0, 0, 0, 0))
            for y in range(crop.size[1]):
                for x in range(crop.size[0]):
                    rgba = crop.getpixel((x, y))
                    if rgba[3] > 30 and not (rgba[0] > 235 and rgba[1] > 235 and rgba[2] > 235):
                        clean.putpixel((x, y), rgba)

            bbox = clean.getbbox()
            if bbox:
                char = clean.crop(bbox)
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
        # Flip right-facing poses for left-facing directions (West, NW, SW)
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

def process_character(gender):
    is_fem = (gender == 'female')
    sheet = FEMALE_REF if is_fem else MALE_REF
    bases = get_base_sprites(sheet, is_fem)

    w, h = 64, 64

    # Theme colors for inpainting
    c_shirt_mid = (253, 144, 113, 255) if is_fem else (250, 135, 105, 255)
    c_shorts_mid= (55, 45, 55, 255)    if is_fem else (60, 60, 65, 255)
    c_skin_mid  = (251, 215, 195, 255) if is_fem else (253, 216, 188, 255)
    c_skin_sh   = (225, 175, 155, 255) if is_fem else (230, 185, 155, 255)

    for d in DIRECTIONS:
        base = bases[d]
        out_dir = f'{OUT_ROOT}/{gender}/{d}'
        os.makedirs(out_dir, exist_ok=True)

        # ─── 1. HEAD (idle_head.png) ──────────────────────────────────
        head = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        for y in range(h):
            for x in range(w):
                c = base.getpixel((x, y))
                if c[3] < 30: continue
                is_head = (y <= 35)
                if is_fem and y <= 46:
                    if d in ('south', 'north') and (x <= 24 or x >= 40): is_head = True
                    elif d == 'east' and x <= 26: is_head = True
                    elif d == 'west' and x >= 38: is_head = True
                    elif d in ('south_east', 'north_east') and x <= 25: is_head = True
                    elif d in ('south_west', 'north_west') and x >= 39: is_head = True
                if is_head:
                    head.putpixel((x, y), c)

        # Inpaint neck base so head connection is solid
        for y in (34, 35, 36):
            for x in range(29, 36):
                if head.getpixel((x, y))[3] == 0:
                    head.putpixel((x, y), c_skin_mid if y <= 35 else c_skin_sh)

        # ─── 2. TORSO (idle_body.png) ─────────────────────────────────
        body = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        for y in range(36, 50):
            for x in range(w):
                c = base.getpixel((x, y))
                if c[3] < 30: continue
                if d in ('south', 'north') and (x < 25 or x > 39): continue
                body.putpixel((x, y), c)

        # Solidify shoulders and pelvis
        if d in ('south', 'north'):
            for y in range(36, 45):
                for x in range(25, 40):
                    if body.getpixel((x, y))[3] == 0: body.putpixel((x, y), c_shirt_mid)
            for y in range(45, 50):
                for x in range(25, 40):
                    if body.getpixel((x, y))[3] == 0: body.putpixel((x, y), c_shorts_mid)
        elif d in ('east', 'west'):
            xs, xe = (27, 37) if d == 'east' else (26, 36)
            for y in range(36, 45):
                for x in range(xs, xe + 1):
                    if body.getpixel((x, y))[3] == 0: body.putpixel((x, y), c_shirt_mid)
            for y in range(45, 50):
                for x in range(xs, xe + 1):
                    if body.getpixel((x, y))[3] == 0: body.putpixel((x, y), c_shorts_mid)

        # ─── 3. ARMS (idle_arm_left.png & idle_arm_right.png) ──────────
        arm_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        arm_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        if d in ('south', 'north'):
            for y in range(36, 48):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] < 30: continue
                    if x <= 25: arm_l.putpixel((x, y), c)
                    elif x >= 39: arm_r.putpixel((x, y), c)
            # Add rounded shoulder caps for female, skin for male
            sh_col = c_shirt_mid if is_fem else c_skin_mid
            for x in (23, 24):
                if arm_l.getpixel((x, 36))[3] == 0: arm_l.putpixel((x, 36), sh_col)
            for x in (39, 40):
                if arm_r.getpixel((x, 36))[3] == 0: arm_r.putpixel((x, 36), sh_col)

        elif d == 'east':
            for y in range(36, 48):
                for x in range(30, 42):
                    c = base.getpixel((x, y))
                    if c[3] > 30 and (c[0] > 190 and c[1] > 150): arm_r.putpixel((x, y), c)
            for y in range(37, 47):
                for x in range(27, 31):
                    c = arm_r.getpixel((x + 5, y))
                    arm_l.putpixel((x, y), darken(c if c[3] > 30 else c_skin_mid, 0.72))

        elif d == 'west':
            for y in range(36, 48):
                for x in range(22, 34):
                    c = base.getpixel((x, y))
                    if c[3] > 30 and (c[0] > 190 and c[1] > 150): arm_l.putpixel((x, y), c)
            for y in range(37, 47):
                for x in range(33, 37):
                    c = arm_l.getpixel((x - 5, y))
                    arm_r.putpixel((x, y), darken(c if c[3] > 30 else c_skin_mid, 0.72))

        elif d in ('south_east', 'north_east'):
            for y in range(36, 48):
                for x in range(32, 44):
                    c = base.getpixel((x, y))
                    if c[3] > 30: arm_r.putpixel((x, y), c)
            for y in range(37, 47):
                for x in range(24, 28): arm_l.putpixel((x, y), darken(c_skin_mid, 0.72))

        elif d in ('south_west', 'north_west'):
            for y in range(36, 48):
                for x in range(20, 32):
                    c = base.getpixel((x, y))
                    if c[3] > 30: arm_l.putpixel((x, y), c)
            for y in range(37, 47):
                for x in range(36, 40): arm_r.putpixel((x, y), darken(c_skin_mid, 0.72))

        # ─── 4. LEGS (idle_leg_left.png & idle_leg_right.png) ──────────
        leg_l = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        leg_r = Image.new('RGBA', (w, h), (0, 0, 0, 0))

        if d in ('south', 'north'):
            for y in range(48, h):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] < 30: continue
                    if x <= 31: leg_l.putpixel((x, y), c)
                    else: leg_r.putpixel((x, y), c)
            for x in (27, 28, 29):
                if leg_l.getpixel((x, 48))[3] == 0: leg_l.putpixel((x, 48), c_skin_mid)
            for x in (34, 35, 36):
                if leg_r.getpixel((x, 48))[3] == 0: leg_r.putpixel((x, 48), c_skin_mid)

        elif d == 'east':
            # Near leg (Right leg): extract leg from base
            for y in range(48, h):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] > 30: leg_r.putpixel((x, y), c)
            # Far leg (Left leg): Companion leg shifted 3px left, only below y=50, no stray pixels behind back!
            for y in range(50, h):
                for x in range(w):
                    c = leg_r.getpixel((x, y))
                    if c[3] > 30:
                        dest_x = x - 3
                        # Only draw if within character silhouette
                        if dest_x >= 28:
                            leg_l.putpixel((dest_x, y), darken(c, 0.72))

        elif d == 'west':
            # Near leg (Left leg): extract leg from base
            for y in range(48, h):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] > 30: leg_l.putpixel((x, y), c)
            # Far leg (Right leg): Companion leg shifted 3px right, only below y=50
            for y in range(50, h):
                for x in range(w):
                    c = leg_l.getpixel((x, y))
                    if c[3] > 30:
                        dest_x = x + 3
                        if dest_x <= 36:
                            leg_r.putpixel((dest_x, y), darken(c, 0.72))

        elif d in ('south_east', 'north_east'):
            for y in range(48, h):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] > 30: leg_r.putpixel((x, y), c)
            for y in range(50, h):
                for x in range(w):
                    c = leg_r.getpixel((x, y))
                    if c[3] > 30 and x - 3 >= 27:
                        leg_l.putpixel((x - 3, y), darken(c, 0.72))

        elif d in ('south_west', 'north_west'):
            for y in range(48, h):
                for x in range(w):
                    c = base.getpixel((x, y))
                    if c[3] > 30: leg_l.putpixel((x, y), c)
            for y in range(50, h):
                for x in range(w):
                    c = leg_l.getpixel((x, y))
                    if c[3] > 30 and x + 3 <= 37:
                        leg_r.putpixel((x + 3, y), darken(c, 0.72))

        # ─── 5. ASSEMBLE COMPOSITE (body.png) ─────────────────────────
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

    print(f'Master final processing complete for {gender}')

process_character('female')
process_character('male')
print('PERFECTION ACHIEVED!')
