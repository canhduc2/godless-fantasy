import os
from PIL import Image

BRAIN_DIR = r'C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938'
PARTS_DIR = 'scratch/rig_workshop/parts'
os.makedirs(PARTS_DIR, exist_ok=True)

def extract_clean_part(im, bbox):
    crop = im.crop(bbox)
    rgba = Image.new('RGBA', crop.size, (0, 0, 0, 0))
    for y in range(crop.size[1]):
        for x in range(crop.size[0]):
            r, g, b = crop.getpixel((x, y))
            # pure white background removal
            if r > 240 and g > 240 and b > 240:
                continue
            # antialiased edges
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

def clean_female_front_torso(raw):
    # Recolor the side knobs at bottom of shorts (y >= 160) to dark shorts color
    # Do NOT cut shoulders or neck!
    w, h = raw.size
    res = raw.copy()
    shorts_color = (68, 62, 65, 255)
    for y in range(h):
        for x in range(w):
            c = res.getpixel((x, y))
            if c[3] < 50: continue
            # If in lower half and has skin tone on outer hips, turn into shorts color
            if y >= 155 and (x < 35 or x > 170):
                r, g, b, a = c
                # skin tone detection
                if r > 160 and g > 110 and b > 90:
                    res.putpixel((x, y), shorts_color)
    b = res.getbbox()
    return res.crop(b) if b else res

def clean_leg(raw):
    # Trims the circular ball-joint dome at top (approx top 8-10% of height)
    w, h = raw.size
    cut_y = int(h * 0.08)
    crop = raw.crop((0, cut_y, w, h))
    b = crop.getbbox()
    return crop.crop(b) if b else crop

def scale_part(p, s):
    w = max(1, int(round(p.size[0] * s)))
    h = max(1, int(round(p.size[1] * s)))
    return p.resize((w, h), Image.Resampling.LANCZOS)

def darken(img, factor=0.78):
    res = Image.new('RGBA', img.size, (0, 0, 0, 0))
    for y in range(img.size[1]):
        for x in range(img.size[0]):
            r, g, b, a = img.getpixel((x, y))
            if a > 0:
                res.putpixel((x, y), (int(r * factor), int(g * factor), int(b * factor), a))
    return res

def flip_h(img):
    return img.transpose(Image.FLIP_LEFT_RIGHT)

def main():
    im_ff = Image.open(os.path.join(BRAIN_DIR, 'female_rig_sheet_1791894935011.jpg'))
    im_fs = Image.open(os.path.join(BRAIN_DIR, 'female_rig_side_sheet_1791894991220.jpg'))
    im_fb = Image.open(os.path.join(BRAIN_DIR, 'female_rig_back_sheet_1791895016733.jpg'))

    im_mf = Image.open(os.path.join(BRAIN_DIR, 'male_rig_front_sheet_1791895051217.jpg'))
    im_ms = Image.open(os.path.join(BRAIN_DIR, 'male_rig_side_sheet_1791895079112.jpg'))
    im_mb = Image.open(os.path.join(BRAIN_DIR, 'male_rig_back_sheet_1791895106340.jpg'))

    f_scale = 50.0 / 772.0
    m_scale = 50.0 / 814.0

    parts = {}

    # 1. FEMALE PARTS
    # Front
    f_head_front = scale_part(extract_clean_part(im_ff, (72, 56, 326, 398)), f_scale)
    f_head_34    = scale_part(extract_clean_part(im_ff, (696, 58, 952, 398)), f_scale)
    f_torso_raw  = extract_clean_part(im_ff, (410, 144, 614, 388))
    f_torso_front= scale_part(clean_female_front_torso(f_torso_raw), f_scale)
    f_arml_front = scale_part(extract_clean_part(im_ff, (144, 472, 234, 706)), f_scale)
    f_armr_front = scale_part(extract_clean_part(im_ff, (798, 472, 890, 706)), f_scale)
    f_legl_front = scale_part(clean_leg(extract_clean_part(im_ff, (328, 686, 418, 992))), f_scale)
    f_legr_front = scale_part(clean_leg(extract_clean_part(im_ff, (604, 686, 716, 992))), f_scale)

    # Side
    f_head_side  = scale_part(extract_clean_part(im_fs, (72, 56, 316, 398)), f_scale)
    f_torso_side = scale_part(extract_clean_part(im_fs, (440, 144, 582, 388)), f_scale)
    f_arm_near   = scale_part(extract_clean_part(im_fs, (144, 472, 234, 706)), f_scale)
    f_arm_far    = darken(scale_part(extract_clean_part(im_fs, (798, 472, 890, 706)), f_scale), 0.78)
    f_leg_near   = scale_part(clean_leg(extract_clean_part(im_fs, (328, 686, 418, 992))), f_scale)
    f_leg_far    = darken(scale_part(clean_leg(extract_clean_part(im_fs, (604, 686, 716, 992))), f_scale), 0.78)

    # Back
    f_head_back  = scale_part(extract_clean_part(im_fb, (400, 56, 638, 368)), f_scale)
    f_torso_back = scale_part(extract_clean_part(im_fb, (410, 410, 614, 634)), f_scale)
    f_arml_back  = scale_part(extract_clean_part(im_fb, (144, 472, 234, 706)), f_scale)
    f_armr_back  = scale_part(extract_clean_part(im_fb, (798, 472, 890, 706)), f_scale)
    f_legl_back  = scale_part(clean_leg(extract_clean_part(im_fb, (308, 686, 420, 992))), f_scale)
    f_legr_back  = scale_part(clean_leg(extract_clean_part(im_fb, (604, 686, 716, 992))), f_scale)

    parts['female_head_front']  = f_head_front
    parts['female_head_34']     = f_head_34
    parts['female_head_side']   = f_head_side
    parts['female_head_back']   = f_head_back
    parts['female_torso_front'] = f_torso_front
    parts['female_torso_side']  = f_torso_side
    parts['female_torso_back']  = f_torso_back
    parts['female_arml_front']  = f_arml_front
    parts['female_armr_front']  = f_armr_front
    parts['female_arm_near']    = f_arm_near
    parts['female_arm_far']     = f_arm_far
    parts['female_arml_back']   = f_arml_back
    parts['female_armr_back']   = f_armr_back
    parts['female_legl_front']  = f_legl_front
    parts['female_legr_front']  = f_legr_front
    parts['female_leg_near']    = f_leg_near
    parts['female_leg_far']     = f_leg_far
    parts['female_legl_back']   = f_legl_back
    parts['female_legr_back']   = f_legr_back

    # 2. MALE PARTS (FULL UNCUT TORSO!)
    # Front
    m_head_front = scale_part(extract_clean_part(im_mf, (332, 40, 672, 354)), m_scale)
    m_torso_front= scale_part(extract_clean_part(im_mf, (384, 384, 640, 690)), m_scale) # UNCUT!
    m_arml_front = scale_part(extract_clean_part(im_mf, (128, 384, 256, 640)), m_scale)
    m_armr_front = scale_part(extract_clean_part(im_mf, (768, 384, 896, 640)), m_scale)
    m_legl_front = scale_part(clean_leg(extract_clean_part(im_mf, (192, 666, 332, 960))), m_scale)
    m_legr_front = scale_part(clean_leg(extract_clean_part(im_mf, (692, 666, 832, 960))), m_scale)

    # Side
    m_head_side  = scale_part(extract_clean_part(im_ms, (112, 36, 436, 360)), m_scale)
    m_torso_side = scale_part(extract_clean_part(im_ms, (652, 76, 848, 400)), m_scale)
    m_arm_near   = scale_part(extract_clean_part(im_ms, (176, 396, 424, 628)), m_scale)
    m_arm_far    = darken(scale_part(extract_clean_part(im_ms, (612, 432, 872, 628)), m_scale), 0.78)
    m_leg_near   = scale_part(clean_leg(extract_clean_part(im_ms, (192, 664, 412, 988))), m_scale)
    m_leg_far    = darken(scale_part(clean_leg(extract_clean_part(im_ms, (624, 676, 808, 988))), m_scale), 0.78)

    # Back
    m_head_back  = scale_part(extract_clean_part(im_mb, (332, 40, 672, 354)), m_scale)
    m_torso_back = scale_part(extract_clean_part(im_mb, (384, 384, 640, 690)), m_scale) # UNCUT!
    m_arml_back  = scale_part(extract_clean_part(im_mb, (128, 384, 256, 640)), m_scale)
    m_armr_back  = scale_part(extract_clean_part(im_mb, (768, 384, 896, 640)), m_scale)
    m_legl_back  = scale_part(clean_leg(extract_clean_part(im_mb, (192, 666, 332, 960))), m_scale)
    m_legr_back  = scale_part(clean_leg(extract_clean_part(im_mb, (692, 666, 832, 960))), m_scale)

    parts['male_head_front']  = m_head_front
    parts['male_head_34']     = m_head_front
    parts['male_head_side']   = m_head_side
    parts['male_head_back']   = m_head_back
    parts['male_torso_front'] = m_torso_front
    parts['male_torso_side']  = m_torso_side
    parts['male_torso_back']  = m_torso_back
    parts['male_arml_front']  = m_arml_front
    parts['male_armr_front']  = m_armr_front
    parts['male_arm_near']    = m_arm_near
    parts['male_arm_far']     = m_arm_far
    parts['male_arml_back']   = m_arml_back
    parts['male_armr_back']   = m_armr_back
    parts['male_legl_front']  = m_legl_front
    parts['male_legr_front']  = m_legr_front
    parts['male_leg_near']    = m_leg_near
    parts['male_leg_far']     = m_leg_far
    parts['male_legl_back']   = m_legl_back
    parts['male_legr_back']   = m_legr_back

    # Save all individual parts
    for name, img in parts.items():
        out_path = os.path.join(PARTS_DIR, f"{name}.png")
        img.save(out_path)
        print(f"Saved {name}: {img.size}")

    print("Successfully extracted all parts into", PARTS_DIR)

if __name__ == '__main__':
    main()
