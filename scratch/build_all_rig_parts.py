import os
from PIL import Image

BRAIN_DIR = r'C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938'
CLIENT_RIG_ROOT = r'client\GodlessFantasyClient\Assets\Resources\Characters\Rig'
PREVIEWS_DIR = 'scratch/final_rig_previews'
os.makedirs(PREVIEWS_DIR, exist_ok=True)

# -------------------------------------------------------------
# 1. Extraction, Cleaning & Joint Ball Removal
# -------------------------------------------------------------
def extract_clean_part(im, bbox):
    crop = im.crop(bbox)
    rgba = Image.new('RGBA', crop.size, (0, 0, 0, 0))
    for y in range(crop.size[1]):
        for x in range(crop.size[0]):
            r, g, b = crop.getpixel((x, y))
            if r > 240 and g > 240 and b > 240:
                continue
            if r > 215 and g > 215 and b > 215:
                whiteness = (r + g + b) / (3.0 * 255.0)
                a = int((1.0 - whiteness) * 255 * 3.5)
                a = min(255, max(0, a))
                if a < 25: continue
                un_r = int(max(0, min(255, (r - 255 * (1 - a/255.0)) / (a/255.0))))
                un_g = int(max(0, min(255, (g - 255 * (1 - a/255.0)) / (a/255.0))))
                un_b = int(max(0, min(255, (b - 255 * (1 - a/255.0)) / (a/255.0))))
                rgba.putpixel((x, y), (un_r, un_g, un_b, a))
            else:
                rgba.putpixel((x, y), (r, g, b, 255))
    b = rgba.getbbox()
    return rgba.crop(b) if b else rgba

def clean_female_torso(raw_torso):
    w, h = raw_torso.size
    clean = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            c = raw_torso.getpixel((x, y))
            if c[3] == 0: continue
            # Remove artificial side hip knobs (y >= 165 outside [20, 183])
            if y >= 165 and (x < 20 or x > 183): continue
            # Remove shoulder knobs
            if y < 75 and (x < 20 or x > 183): continue
            clean.putpixel((x, y), c)
    b = clean.getbbox()
    return clean.crop(b) if b else clean

def clean_male_torso(raw_torso):
    w, h = raw_torso.size
    clean = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            c = raw_torso.getpixel((x, y))
            if c[3] == 0: continue
            # Remove artificial side hip knobs (y >= 210 outside [38, 217])
            if y >= 210 and (x < 38 or x > 217): continue
            # Remove shoulder knobs
            if y < 110 and (x < 38 or x > 217): continue
            clean.putpixel((x, y), c)
    b = clean.getbbox()
    return clean.crop(b) if b else clean

def clean_leg_top(raw_leg):
    # Removes the top artificial circular joint dome (y < 25)
    w, h = raw_leg.size
    clean = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    for y in range(25, h):
        for x in range(w):
            clean.putpixel((x, y - 25), raw_leg.getpixel((x, y)))
    b = clean.getbbox()
    return clean.crop(b) if b else clean

def scale_part(p, s):
    w = max(1, int(round(p.size[0] * s)))
    h = max(1, int(round(p.size[1] * s)))
    return p.resize((w, h), Image.Resampling.LANCZOS)

def darken(img, factor=0.76):
    res = Image.new('RGBA', img.size, (0, 0, 0, 0))
    for y in range(img.size[1]):
        for x in range(img.size[0]):
            r, g, b, a = img.getpixel((x, y))
            if a > 0:
                res.putpixel((x, y), (int(r * factor), int(g * factor), int(b * factor), a))
    return res

def place_on_64(img, x, y):
    canvas = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    canvas.paste(img, (x, y), img)
    return canvas

# -------------------------------------------------------------
# 2. Main Builder
# -------------------------------------------------------------
def build_character_rigs():
    im_ff = Image.open(os.path.join(BRAIN_DIR, 'female_rig_sheet_1791894935011.jpg'))
    im_fs = Image.open(os.path.join(BRAIN_DIR, 'female_rig_side_sheet_1791894991220.jpg'))
    im_fb = Image.open(os.path.join(BRAIN_DIR, 'female_rig_back_sheet_1791895016733.jpg'))

    im_mf = Image.open(os.path.join(BRAIN_DIR, 'male_rig_front_sheet_1791895051217.jpg'))
    im_ms = Image.open(os.path.join(BRAIN_DIR, 'male_rig_side_sheet_1791895079112.jpg'))
    im_mb = Image.open(os.path.join(BRAIN_DIR, 'male_rig_back_sheet_1791895106340.jpg'))

    f_scale = 50.0 / 772.0
    m_scale = 50.0 / 814.0

    DIRECTIONS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west']

    for gender in ['female', 'male']:
        is_fem = (gender == 'female')
        sc = f_scale if is_fem else m_scale
        sh_front = im_ff if is_fem else im_mf
        sh_side  = im_fs if is_fem else im_ms
        sh_back  = im_fb if is_fem else im_mb

        # --- EXTRACT & CLEAN PARTS ---
        if is_fem:
            # Female Front
            s_head   = scale_part(extract_clean_part(sh_front, (72, 56, 326, 398)), sc)
            s_head34 = scale_part(extract_clean_part(sh_front, (696, 58, 952, 398)), sc)
            s_torso  = scale_part(clean_female_torso(extract_clean_part(sh_front, (410, 144, 614, 388))), sc)
            s_arml   = scale_part(extract_clean_part(sh_front, (144, 472, 234, 706)), sc)
            s_armr   = scale_part(extract_clean_part(sh_front, (798, 472, 890, 706)), sc)
            s_legl   = scale_part(clean_leg_top(extract_clean_part(sh_front, (328, 686, 418, 992))), sc)
            s_legr   = scale_part(clean_leg_top(extract_clean_part(sh_front, (604, 686, 716, 992))), sc)

            # Female Side
            e_head     = scale_part(extract_clean_part(sh_side, (72, 56, 316, 398)), sc)
            e_torso    = scale_part(clean_female_torso(extract_clean_part(sh_side, (440, 144, 582, 388))), sc)
            e_near_arm = scale_part(extract_clean_part(sh_side, (144, 472, 234, 706)), sc)
            e_far_arm  = darken(scale_part(extract_clean_part(sh_side, (798, 472, 890, 706)), sc), 0.76)
            e_near_leg = scale_part(clean_leg_top(extract_clean_part(sh_side, (328, 686, 418, 992))), sc)
            e_far_leg  = darken(scale_part(clean_leg_top(extract_clean_part(sh_side, (604, 686, 716, 992))), sc), 0.76)

            # Female Back
            n_head   = scale_part(extract_clean_part(sh_back, (400, 56, 638, 368)), sc)
            n_torso  = scale_part(clean_female_torso(extract_clean_part(sh_back, (410, 410, 614, 634))), sc)
            n_arml   = scale_part(extract_clean_part(sh_back, (144, 472, 234, 706)), sc)
            n_armr   = scale_part(extract_clean_part(sh_back, (798, 472, 890, 706)), sc)
            n_legl   = scale_part(clean_leg_top(extract_clean_part(sh_back, (308, 686, 420, 992))), sc)
            n_legr   = scale_part(clean_leg_top(extract_clean_part(sh_back, (604, 686, 716, 992))), sc)
        else:
            # Male Front
            s_head   = scale_part(extract_clean_part(sh_front, (332, 40, 672, 354)), sc)
            s_head34 = s_head
            s_torso  = scale_part(clean_male_torso(extract_clean_part(sh_front, (384, 384, 640, 690))), sc)
            s_arml   = scale_part(extract_clean_part(sh_front, (128, 384, 256, 640)), sc)
            s_armr   = scale_part(extract_clean_part(sh_front, (768, 384, 896, 640)), sc)
            s_legl   = scale_part(clean_leg_top(extract_clean_part(sh_front, (192, 666, 332, 960))), sc)
            s_legr   = scale_part(clean_leg_top(extract_clean_part(sh_front, (692, 666, 832, 960))), sc)

            # Male Side
            m_s_leg_near = clean_leg_top(extract_clean_part(sh_side, (192, 666, 408, 984)))
            m_s_leg_far  = clean_leg_top(extract_clean_part(sh_side, (628, 678, 806, 984)))
            e_head     = scale_part(extract_clean_part(sh_side, (116, 38, 434, 358)), sc)
            e_torso    = scale_part(clean_male_torso(extract_clean_part(sh_side, (652, 76, 844, 396))), sc)
            e_near_arm = scale_part(extract_clean_part(sh_side, (180, 396, 422, 626)), sc)
            e_far_arm  = darken(scale_part(extract_clean_part(sh_side, (614, 436, 870, 626)), sc), 0.76)
            e_near_leg = scale_part(m_s_leg_near, sc)
            e_far_leg  = darken(scale_part(m_s_leg_far, sc), 0.76)

            # Male Back
            n_head   = scale_part(extract_clean_part(sh_back, (346, 38, 678, 358)), sc)
            n_torso  = scale_part(clean_male_torso(extract_clean_part(sh_back, (384, 384, 640, 690))), sc)
            n_arml   = scale_part(extract_clean_part(sh_back, (128, 384, 256, 640)), sc)
            n_armr   = scale_part(extract_clean_part(sh_back, (768, 384, 896, 640)), sc)
            n_legl   = scale_part(clean_leg_top(extract_clean_part(sh_back, (192, 666, 332, 960))), sc)
            n_legr   = scale_part(clean_leg_top(extract_clean_part(sh_back, (692, 666, 832, 960))), sc)

        dir_parts = {}

        # ─── A. SOUTH (Front) ──────────────────────────────────────────
        # Feet baseline at y = 57
        f_leg_dy = 3 if is_fem else 4
        f_arm_dy = 3 if is_fem else 4
        f_head_dy = 4 if is_fem else 2

        torso_x = 32 - s_torso.size[0] // 2
        torso_y = 57 - s_legl.size[1] - s_torso.size[1] + f_leg_dy
        # Place legs snugly under pelvis (center at 32):
        leg_l_x = 32 - s_legl.size[0]
        leg_r_x = 32
        leg_y   = torso_y + s_torso.size[1] - f_leg_dy
        arm_l_x = torso_x - s_arml.size[0] + 2
        arm_r_x = torso_x + s_torso.size[0] - 2
        arm_y   = torso_y + f_arm_dy
        head_x  = 32 - s_head.size[0] // 2
        head_y  = torso_y - s_head.size[1] + f_head_dy

        dir_parts['south'] = {
            'head':  place_on_64(s_head, head_x, head_y),
            'body':  place_on_64(s_torso, torso_x, torso_y),
            'arm_l': place_on_64(s_arml, arm_l_x, arm_y),
            'arm_r': place_on_64(s_armr, arm_r_x, arm_y),
            'leg_l': place_on_64(s_legl, leg_l_x, leg_y),
            'leg_r': place_on_64(s_legr, leg_r_x, leg_y),
            'layering': 'front'
        }

        # ─── B. NORTH (Back) ───────────────────────────────────────────
        b_leg_dy = 3 if is_fem else 4
        b_arm_dy = 3 if is_fem else 4
        b_head_dy = 4 if is_fem else 2

        n_tx = 32 - n_torso.size[0] // 2
        n_ty = 57 - n_legl.size[1] - n_torso.size[1] + b_leg_dy
        n_lx = 32 - n_legl.size[0]
        n_rx = 32
        n_ly = n_ty + n_torso.size[1] - b_leg_dy
        n_alx = n_tx - n_arml.size[0] + 2
        n_arx = n_tx + n_torso.size[0] - 2
        n_ay  = n_ty + b_arm_dy
        n_hx  = 32 - n_head.size[0] // 2
        n_hy  = n_ty - n_head.size[1] + b_head_dy

        dir_parts['north'] = {
            'head':  place_on_64(n_head, n_hx, n_hy),
            'body':  place_on_64(n_torso, n_tx, n_ty),
            'arm_l': place_on_64(n_arml, n_alx, n_ay),
            'arm_r': place_on_64(n_armr, n_arx, n_ay),
            'leg_l': place_on_64(n_legl, n_lx, n_ly),
            'leg_r': place_on_64(n_legr, n_rx, n_ly),
            'layering': 'back'
        }

        # ─── C. EAST (Side Facing Right) ───────────────────────────────
        s_leg_dy = 3 if is_fem else 4
        e_tx = 30 if is_fem else 30
        e_ty = 57 - e_near_leg.size[1] - e_torso.size[1] + s_leg_dy

        e_fl_x = e_tx - 2
        e_fl_y = e_ty + e_torso.size[1] - s_leg_dy
        e_fa_x = e_tx - 2
        e_fa_y = e_ty + 3

        e_nl_x = e_tx + 1 if is_fem else e_tx + 2
        e_nl_y = e_ty + e_torso.size[1] - s_leg_dy
        e_na_x = e_tx + 2 if is_fem else e_tx + 3
        e_na_y = e_ty + 3

        e_hx = 32 - e_head.size[0] // 2 - 1 if is_fem else 32 - e_head.size[0] // 2
        e_hy = e_ty - e_head.size[1] + (6 if is_fem else 4)

        dir_parts['east'] = {
            'head':  place_on_64(e_head, e_hx, e_hy),
            'body':  place_on_64(e_torso, e_tx, e_ty),
            'arm_l': place_on_64(e_far_arm, e_fa_x, e_fa_y),    # far arm
            'arm_r': place_on_64(e_near_arm, e_na_x, e_na_y),  # near arm
            'leg_l': place_on_64(e_far_leg, e_fl_x, e_fl_y),    # far leg
            'leg_r': place_on_64(e_near_leg, e_nl_x, e_nl_y),  # near leg
            'layering': 'east'
        }

        # ─── D. WEST (Side Facing Left) ────────────────────────────────
        dir_parts['west'] = {
            'head':  dir_parts['east']['head'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'body':  dir_parts['east']['body'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_l': dir_parts['east']['arm_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_r': dir_parts['east']['arm_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_l': dir_parts['east']['leg_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_r': dir_parts['east']['leg_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'layering': 'west'
        }

        # ─── E. SOUTH_EAST (3/4 Front-Right) ───────────────────────────
        se_hx = 32 - s_head34.size[0] // 2 + 1
        se_hy = head_y
        dir_parts['south_east'] = {
            'head':  place_on_64(s_head34, se_hx, se_hy),
            'body':  place_on_64(s_torso, torso_x + 1, torso_y),
            'arm_l': place_on_64(darken(s_arml, 0.85), arm_l_x + 1, arm_y),
            'arm_r': place_on_64(s_armr, arm_r_x + 1, arm_y),
            'leg_l': place_on_64(darken(s_legl, 0.85), leg_l_x + 1, leg_y),
            'leg_r': place_on_64(s_legr, leg_r_x + 1, leg_y),
            'layering': 'front'
        }

        # ─── F. SOUTH_WEST (3/4 Front-Left) ────────────────────────────
        dir_parts['south_west'] = {
            'head':  dir_parts['south_east']['head'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'body':  dir_parts['south_east']['body'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_l': dir_parts['south_east']['arm_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_r': dir_parts['south_east']['arm_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_l': dir_parts['south_east']['leg_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_r': dir_parts['south_east']['leg_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'layering': 'front'
        }

        # ─── G. NORTH_EAST (3/4 Back-Right) ────────────────────────────
        dir_parts['north_east'] = {
            'head':  place_on_64(n_head, n_hx + 1, n_hy),
            'body':  place_on_64(n_torso, n_tx + 1, n_ty),
            'arm_l': place_on_64(darken(n_arml, 0.85), n_alx + 1, n_ay),
            'arm_r': place_on_64(n_armr, n_arx + 1, n_ay),
            'leg_l': place_on_64(darken(n_legl, 0.85), n_lx + 1, n_ly),
            'leg_r': place_on_64(n_legr, n_rx + 1, n_ly),
            'layering': 'back'
        }

        # ─── H. NORTH_WEST (3/4 Back-Left) ─────────────────────────────
        dir_parts['north_west'] = {
            'head':  dir_parts['north_east']['head'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'body':  dir_parts['north_east']['body'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_l': dir_parts['north_east']['arm_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'arm_r': dir_parts['north_east']['arm_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_l': dir_parts['north_east']['leg_r'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'leg_r': dir_parts['north_east']['leg_l'].transpose(Image.Transpose.FLIP_LEFT_RIGHT),
            'layering': 'back'
        }

        # ─── SAVE TO CLIENT RIG RESOURCES ──────────────────────────────
        for d in DIRECTIONS:
            out_dir = os.path.join(CLIENT_RIG_ROOT, gender, d)
            os.makedirs(out_dir, exist_ok=True)
            p = dir_parts[d]

            p['head'].save(os.path.join(out_dir, 'idle_head.png'))
            p['body'].save(os.path.join(out_dir, 'idle_body.png'))
            p['arm_l'].save(os.path.join(out_dir, 'idle_arm_left.png'))
            p['arm_r'].save(os.path.join(out_dir, 'idle_arm_right.png'))
            p['leg_l'].save(os.path.join(out_dir, 'idle_leg_left.png'))
            p['leg_r'].save(os.path.join(out_dir, 'idle_leg_right.png'))

            # Assemble clean composite body.png
            comp = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
            lay = p['layering']

            if lay == 'front':
                comp.paste(p['body'], (0, 0), p['body'])
                comp.paste(p['leg_l'], (0, 0), p['leg_l'])
                comp.paste(p['leg_r'], (0, 0), p['leg_r'])
                comp.paste(p['arm_l'], (0, 0), p['arm_l'])
                comp.paste(p['arm_r'], (0, 0), p['arm_r'])
                comp.paste(p['head'], (0, 0), p['head'])
            elif lay == 'back':
                comp.paste(p['arm_l'], (0, 0), p['arm_l'])
                comp.paste(p['arm_r'], (0, 0), p['arm_r'])
                comp.paste(p['leg_l'], (0, 0), p['leg_l'])
                comp.paste(p['leg_r'], (0, 0), p['leg_r'])
                comp.paste(p['body'], (0, 0), p['body'])
                comp.paste(p['head'], (0, 0), p['head'])
            elif lay == 'east':
                comp.paste(p['arm_l'], (0, 0), p['arm_l'])
                comp.paste(p['leg_l'], (0, 0), p['leg_l'])
                comp.paste(p['body'], (0, 0), p['body'])
                comp.paste(p['leg_r'], (0, 0), p['leg_r'])
                comp.paste(p['arm_r'], (0, 0), p['arm_r'])
                comp.paste(p['head'], (0, 0), p['head'])
            elif lay == 'west':
                comp.paste(p['arm_r'], (0, 0), p['arm_r'])
                comp.paste(p['leg_r'], (0, 0), p['leg_r'])
                comp.paste(p['body'], (0, 0), p['body'])
                comp.paste(p['leg_l'], (0, 0), p['leg_l'])
                comp.paste(p['arm_l'], (0, 0), p['arm_l'])
                comp.paste(p['head'], (0, 0), p['head'])

            comp.save(os.path.join(out_dir, 'body.png'))

    print('Successfully generated all 8 directions with clean joint placements!')

    # Build review montage of all 8 directions (Female top, Male bottom)
    montage = Image.new('RGBA', (64 * 8, 64 * 2), (240, 240, 240, 255))
    for g_idx, g in enumerate(['female', 'male']):
        for d_idx, d in enumerate(DIRECTIONS):
            comp_path = os.path.join(CLIENT_RIG_ROOT, g, d, 'body.png')
            im = Image.open(comp_path)
            montage.paste(im, (d_idx * 64, g_idx * 64), im)

    montage_3x = montage.resize((montage.size[0] * 3, montage.size[1] * 3), Image.Resampling.NEAREST)
    montage_3x.save(os.path.join(PREVIEWS_DIR, 'all_8_directions_review_3x.png'))
    print(f'Saved preview montage to {PREVIEWS_DIR}/all_8_directions_review_3x.png')

if __name__ == '__main__':
    build_character_rigs()
