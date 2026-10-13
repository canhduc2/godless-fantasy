# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Chibi Rig Generator
Crafts pristine, solid, high-detail chibi pixel art parts for 2D skeletal animation.
Zero cropping from images. Pure crafted pixel art based on reference design.
"""
import os
from PIL import Image

ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

# ════════════════════════════════════════════════════════════════
# COLOR PALETTE (Sampled & Harmonized with Anime ARPG Standard)
# ════════════════════════════════════════════════════════════════
# Skin
SKIN_HL       = (255, 238, 226, 255) # Soft light highlight
SKIN_BASE     = (246, 204, 180, 255) # Peachy chibi skin
SKIN_SHADOW   = (222, 166, 140, 255) # Shadow under hair / chin
SKIN_DEEP_SH  = (188, 132, 108, 255) # Deep shadow for far limbs
SKIN_OUTLINE  = (112, 60, 48, 255)   # Soft warm dark outline
BLUSH_COLOR   = (244, 168, 162, 255) # Cute cheeks blush

# Eyes
EYE_LASH      = (42, 22, 28, 255)    # Upper eyelash line
EYE_PUPIL     = (62, 32, 28, 255)    # Deep pupil
EYE_IRIS_TOP  = (118, 58, 44, 255)   # Upper iris
EYE_IRIS_BOT  = (176, 96, 68, 255)   # Lower iris reflex (amber glow)
EYE_WHITE     = (252, 250, 250, 255) # Sclera
EYE_SPARKLE   = (255, 255, 255, 255) # Bright white catchlight

# Female Hair (Warm Chestnut Auburn)
F_HAIR_HL     = (192, 136, 104, 255) # Glossy highlight sheen
F_HAIR_BASE   = (148, 92, 64, 255)   # Base chestnut
F_HAIR_MID_SH = (116, 66, 44, 255)   # Mid shadow
F_HAIR_DEEP   = (80, 40, 26, 255)    # Deep shadow & outlines
F_CLIP_RED    = (234, 68, 64, 255)   # Red hair clip
F_CLIP_GOLD   = (248, 196, 72, 255)  # Gold trim on clip

# Male Hair (Spiky Fluffy Brown)
M_HAIR_HL     = (176, 124, 88, 255)  # Spiky hair tips
M_HAIR_BASE   = (136, 84, 54, 255)   # Base warm brown
M_HAIR_MID_SH = (104, 58, 36, 255)   # Shadow between clumps
M_HAIR_DEEP   = (68, 34, 20, 255)    # Outer dark contour

# Clothes - Top (Adventurer Salmon / Orange Vest)
TOP_HL        = (246, 138, 112, 255) # Center chest highlight
TOP_BASE      = (220, 96, 74, 255)   # Base vibrant salmon/orange
TOP_SHADOW    = (174, 64, 48, 255)   # Side / crease shadow
TOP_DARK_SH   = (124, 40, 28, 255)   # Deep folds & seams
TOP_OUTLINE   = (78, 24, 18, 255)    # Dark contour
COLLAR_WHITE  = (246, 242, 238, 255) # White collar trim
COLLAR_SHADOW = (196, 188, 182, 255) # Collar crease

# Clothes - Belt & Shorts
BELT_LEATHER  = (112, 58, 36, 255)   # Brown belt
BELT_BUCKLE   = (240, 196, 66, 255)  # Gold buckle
SHORTS_HL     = (78, 78, 90, 255)    # Dark charcoal highlight
SHORTS_BASE   = (52, 52, 62, 255)    # Base charcoal
SHORTS_SHADOW = (34, 34, 42, 255)    # Shadow
SHORTS_OUT    = (20, 20, 26, 255)    # Dark contour

# Shoes / Boots
BOOT_HL       = (94, 78, 68, 255)    # Leather shine
BOOT_BASE     = (66, 52, 44, 255)    # Base dark leather
BOOT_SHADOW   = (44, 34, 28, 255)    # Shadow
BOOT_SOLE     = (24, 18, 16, 255)    # Thick sole

def new_canvas():
    return Image.new("RGBA", (64, 64), (0, 0, 0, 0))

# ════════════════════════════════════════════════════════════════
# 1. FEMALE SOUTH (Front Chibi)
# ════════════════════════════════════════════════════════════════
def build_female_south():
    # ── HEAD ──────────────────────────────────────────────────────
    head = new_canvas()
    hp = head.load()

    # Back hair volume (wavy curls flowing around shoulders)
    for y in range(22, 44):
        # Left lock
        lx_start = 16 - (1 if 26 <= y <= 36 else 0)
        lx_end   = 22
        for x in range(lx_start, lx_end):
            if x == lx_start or y == 43: hp[x, y] = F_HAIR_DEEP
            elif x == lx_start + 1:      hp[x, y] = F_HAIR_MID_SH
            elif x >= 20:                hp[x, y] = F_HAIR_MID_SH
            else:                        hp[x, y] = F_HAIR_BASE

        # Right lock
        rx_start = 42
        rx_end   = 48 + (1 if 26 <= y <= 36 else 0)
        for x in range(rx_start, rx_end + 1):
            if x == rx_end or y == 43:  hp[x, y] = F_HAIR_DEEP
            elif x == rx_end - 1:       hp[x, y] = F_HAIR_MID_SH
            elif x <= 43:               hp[x, y] = F_HAIR_MID_SH
            else:                       hp[x, y] = F_HAIR_BASE

    # Neck stub (y=36..39, rounded into collar)
    for y in range(36, 40):
        for x in range(30, 35):
            if x in (30, 34) or y == 39: hp[x, y] = SKIN_SHADOW
            else: hp[x, y] = SKIN_BASE

    # Face base (round chibi cheeks, cute chin)
    for y in range(22, 37):
        if y < 24:    x0, x1 = 23, 41
        elif y < 28:  x0, x1 = 20, 44
        elif y < 33:  x0, x1 = 19, 45 # max cheek width
        elif y == 33: x0, x1 = 21, 43
        elif y == 34: x0, x1 = 24, 40
        elif y == 35: x0, x1 = 27, 37
        else:         x0, x1 = 30, 34 # chin

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36:
                hp[x, y] = SKIN_OUTLINE
            elif x in (x0 + 1, x1 - 1) or y >= 34:
                hp[x, y] = SKIN_SHADOW
            elif y == 23 and 27 <= x <= 37:
                hp[x, y] = SKIN_HL
            else:
                hp[x, y] = SKIN_BASE

    # Blush
    for bx, by in [(21, 31), (22, 31), (22, 30), (23, 31)]: hp[bx, by] = BLUSH_COLOR
    for bx, by in [(41, 31), (42, 31), (42, 30), (43, 31)]: hp[bx, by] = BLUSH_COLOR

    # Mouth (cute happy smile)
    hp[31, 33] = SKIN_OUTLINE; hp[32, 34] = (168, 80, 80, 255); hp[33, 33] = SKIN_OUTLINE

    # Anime Eyes (Big expressive eyes with iris reflex & white sparkle)
    # Left Eye (x=24..28, y=26..30)
    for x in range(24, 29): hp[x, 26] = EYE_LASH
    hp[23, 26] = EYE_LASH; hp[29, 26] = EYE_LASH
    for y in range(27, 30):
        hp[24, y] = EYE_LASH
        hp[25, y] = EYE_IRIS_TOP if y == 27 else EYE_IRIS_BOT
        hp[26, y] = EYE_PUPIL if y == 28 else EYE_IRIS_TOP
        hp[27, y] = EYE_IRIS_TOP if y == 27 else EYE_IRIS_BOT
        hp[28, y] = EYE_LASH
    for x in range(25, 28): hp[x, 30] = (120, 56, 44, 255)
    # Catchlight (Sparkle)
    hp[25, 27] = EYE_SPARKLE; hp[26, 27] = EYE_SPARKLE

    # Right Eye (x=36..40, y=26..30)
    for x in range(36, 41): hp[x, 26] = EYE_LASH
    hp[35, 26] = EYE_LASH; hp[41, 26] = EYE_LASH
    for y in range(27, 30):
        hp[36, y] = EYE_LASH
        hp[37, y] = EYE_IRIS_TOP if y == 27 else EYE_IRIS_BOT
        hp[38, y] = EYE_PUPIL if y == 28 else EYE_IRIS_TOP
        hp[39, y] = EYE_IRIS_TOP if y == 27 else EYE_IRIS_BOT
        hp[40, y] = EYE_LASH
    for x in range(37, 40): hp[x, 30] = (120, 56, 44, 255)
    # Catchlight (Sparkle)
    hp[37, 27] = EYE_SPARKLE; hp[38, 27] = EYE_SPARKLE

    # Cute Eyebrows
    hp[24, 24] = F_HAIR_MID_SH; hp[25, 24] = F_HAIR_DEEP; hp[26, 24] = F_HAIR_DEEP
    hp[38, 24] = F_HAIR_DEEP; hp[39, 24] = F_HAIR_DEEP; hp[40, 24] = F_HAIR_MID_SH

    # Hair Dome / Volume (Natural rounded top with tufts)
    # y=10..22
    dome_bounds = {
        10: (29, 35),
        11: (26, 38),
        12: (24, 40),
        13: (22, 42),
        14: (20, 44),
        15: (19, 45),
        16: (18, 46),
        17: (17, 47),
        18: (17, 47),
        19: (17, 47),
        20: (17, 47),
        21: (17, 47),
        22: (17, 47)
    }
    for y, (x0, x1) in dome_bounds.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 10:
                hp[x, y] = F_HAIR_DEEP
            elif y in (11, 12) and (x in (x0 + 1, x1 - 1)):
                hp[x, y] = F_HAIR_MID_SH
            elif 13 <= y <= 15 and 25 <= x <= 39:
                hp[x, y] = F_HAIR_HL # Glossy sheen band across crown
            elif y >= 16 and (x in (x0 + 1, x1 - 1)):
                hp[x, y] = F_HAIR_MID_SH
            else:
                hp[x, y] = F_HAIR_BASE

    # Bangs Tufts (Natural curved anime bangs framing the forehead)
    # Strand 1 (Left): x=20..23, y=23..27
    for y in range(23, 28):
        for x in range(20, 24):
            hp[x, y] = F_HAIR_DEEP if x == 20 or y == 27 else F_HAIR_BASE

    # Strand 2 (Center Left): x=25..27, y=23..25
    hp[25, 23] = F_HAIR_BASE; hp[26, 23] = F_HAIR_BASE; hp[27, 23] = F_HAIR_BASE
    hp[26, 24] = F_HAIR_BASE; hp[27, 24] = F_HAIR_DEEP
    hp[26, 25] = F_HAIR_DEEP

    # Strand 3 (Center): x=30..33, y=23..25
    hp[30, 23] = F_HAIR_BASE; hp[31, 23] = F_HAIR_BASE; hp[32, 23] = F_HAIR_BASE
    hp[31, 24] = F_HAIR_BASE; hp[32, 24] = F_HAIR_BASE; hp[33, 24] = F_HAIR_DEEP
    hp[32, 25] = F_HAIR_DEEP

    # Strand 4 (Center Right): x=34..36, y=23..24
    hp[34, 23] = F_HAIR_BASE; hp[35, 23] = F_HAIR_BASE
    hp[35, 24] = F_HAIR_DEEP

    # Strand 5 (Right): x=41..44, y=23..27
    for y in range(23, 28):
        for x in range(41, 45):
            hp[x, y] = F_HAIR_DEEP if x == 44 or y == 27 else F_HAIR_BASE

    # Cute Hair Accessories (Red Clips with Gold Trim on Sides)
    hp[19, 21] = F_CLIP_RED; hp[20, 21] = F_CLIP_GOLD; hp[19, 22] = F_CLIP_RED; hp[20, 22] = F_CLIP_RED
    hp[44, 21] = F_CLIP_GOLD; hp[45, 21] = F_CLIP_RED; hp[44, 22] = F_CLIP_RED; hp[45, 22] = F_CLIP_RED

    # ── TORSO / BODY ──────────────────────────────────────────────
    body = new_canvas()
    bp = body.load()

    # Shoulders & Chest & Waist (y=37..48)
    for y in range(37, 49):
        if y == 37:    x0, x1 = 29, 35 # neck collar
        elif y == 38:  x0, x1 = 26, 38 # collar & shoulders
        elif y == 39:  x0, x1 = 24, 40 # full shoulders
        elif y < 44:   x0, x1 = 25, 39 # chest & waist
        elif y < 46:   x0, x1 = 25, 39 # belt
        else:          x0, x1 = 25, 39 # shorts pelvis

        for x in range(x0, x1 + 1):
            # Collar V-Neck
            if y == 38 and 30 <= x <= 34:
                bp[x, y] = COLLAR_WHITE
            elif y == 39 and 31 <= x <= 33:
                bp[x, y] = COLLAR_WHITE
            elif y == 40 and x == 32:
                bp[x, y] = COLLAR_SHADOW

            # Adventurer Shirt / Vest
            elif y <= 43:
                if x in (x0, x1):             bp[x, y] = TOP_OUTLINE
                elif x in (x0 + 1, x1 - 1):   bp[x, y] = TOP_SHADOW
                elif 30 <= x <= 34:           bp[x, y] = TOP_HL
                else:                         bp[x, y] = TOP_BASE

            # Leather Belt & Gold Buckle
            elif y in (44, 45):
                if 31 <= x <= 33:
                    bp[x, y] = BELT_BUCKLE if y == 44 else (186, 142, 44, 255)
                elif x in (x0, x1):
                    bp[x, y] = SHORTS_OUT
                else:
                    bp[x, y] = BELT_LEATHER if y == 44 else (80, 40, 24, 255)

            # Charcoal Shorts Pelvis (with rounded leg sockets)
            else:
                if x in (x0, x1) or y == 48:
                    bp[x, y] = SHORTS_OUT
                elif x in (x0 + 1, x1 - 1):
                    bp[x, y] = SHORTS_SHADOW
                elif 31 <= x <= 33:
                    bp[x, y] = SHORTS_HL
                else:
                    bp[x, y] = SHORTS_BASE

    # ── ARMS (LEFT & RIGHT) ───────────────────────────────────────
    # Left Arm (hangs beside left torso, x=21..24, y=38..48)
    arm_l = new_canvas()
    alp = arm_l.load()
    # Rounded shoulder cap (y=38..40, pivots seamlessly inside torso shoulder)
    for y in range(38, 48):
        x0, x1 = 21, 24
        if y == 38: x0 = 22 # rounded top
        for x in range(x0, x1 + 1):
            if x == x0 or y == 47: alp[x, y] = SKIN_OUTLINE
            elif x == x0 + 1:      alp[x, y] = SKIN_SHADOW
            elif y == 38:          alp[x, y] = SKIN_SHADOW
            else:                  alp[x, y] = SKIN_BASE
    # Chibi fist
    alp[22, 47] = SKIN_SHADOW; alp[23, 47] = SKIN_BASE

    # Right Arm (hangs beside right torso, x=40..43, y=38..48)
    arm_r = new_canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 40, 43
        if y == 38: x1 = 42 # rounded top
        for x in range(x0, x1 + 1):
            if x == x1 or y == 47: arp[x, y] = SKIN_OUTLINE
            elif x == x1 - 1:      arp[x, y] = SKIN_SHADOW
            elif y == 38:          arp[x, y] = SKIN_SHADOW
            else:                  arp[x, y] = SKIN_BASE
    arp[41, 47] = SKIN_BASE; arp[42, 47] = SKIN_SHADOW

    # ── LEGS (LEFT & RIGHT) ───────────────────────────────────────
    # Left Leg (x=26..31, y=47..58)
    leg_l = new_canvas()
    llp = leg_l.load()
    # Rounded hip cap (y=47..49, fits inside shorts socket)
    for y in range(47, 50):
        for x in range(27, 31):
            llp[x, y] = SHORTS_BASE if y < 49 else SHORTS_HL
    # Shorts cuff outline
    for x in range(26, 32): llp[x, 49] = SHORTS_OUT

    # Bare Thigh / Knee (y=50..53)
    for y in range(50, 54):
        for x in range(27, 31):
            if x in (27, 30): llp[x, y] = SKIN_OUTLINE
            elif x == 28:     llp[x, y] = SKIN_BASE
            else:             llp[x, y] = SKIN_SHADOW

    # Leather Boot (y=54..58)
    for y in range(54, 59):
        x0 = 26 if y >= 56 else 27
        x1 = 31
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): llp[x, y] = BOOT_SOLE
            elif y == 57:                llp[x, y] = BOOT_SHADOW
            elif x == x0 + 1:            llp[x, y] = BOOT_HL
            else:                        llp[x, y] = BOOT_BASE

    # Right Leg (x=33..38, y=47..58)
    leg_r = new_canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38):
            lrp[x, y] = SHORTS_BASE if y < 49 else SHORTS_HL
    for x in range(33, 39): lrp[x, 49] = SHORTS_OUT

    for y in range(50, 54):
        for x in range(34, 38):
            if x in (34, 37): lrp[x, y] = SKIN_OUTLINE
            elif x == 35:     lrp[x, y] = SKIN_BASE
            else:             lrp[x, y] = SKIN_SHADOW

    for y in range(54, 59):
        x0 = 33
        x1 = 38 if y >= 56 else 37
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): lrp[x, y] = BOOT_SOLE
            elif y == 57:                lrp[x, y] = BOOT_SHADOW
            elif x == x0 + 1:            lrp[x, y] = BOOT_HL
            else:                        lrp[x, y] = BOOT_BASE

    # Assembled Composite
    composite = new_canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "head": head,
        "body": body,
        "arm_left": arm_l,
        "arm_right": arm_r,
        "leg_left": leg_l,
        "leg_right": leg_r,
        "composite": composite
    }

# ════════════════════════════════════════════════════════════════
# 2. FEMALE EAST (Side / 3/4 Profile Facing RIGHT)
# ════════════════════════════════════════════════════════════════
def build_female_east():
    # HEAD (Profile facing Right)
    head = new_canvas()
    hp = head.load()

    # Hair Back / Ponytail (Flowing left/behind, x=15..28, y=18..44)
    for y in range(16, 44):
        x0 = 15 - (2 if 24 <= y <= 34 else 0)
        x1 = 27
        for x in range(x0, x1):
            if x == x0 or y == 43: hp[x, y] = F_HAIR_DEEP
            elif x < x0 + 3:       hp[x, y] = F_HAIR_MID_SH
            elif 20 <= x <= 25 and 18 <= y <= 22: hp[x, y] = F_HAIR_HL
            else:                  hp[x, y] = F_HAIR_BASE

    # Face Profile (x=26..42, y=22..36)
    # Nose & mouth profile facing RIGHT at x=41..42
    for y in range(22, 37):
        if y < 24:    x0, x1 = 26, 40
        elif y < 28:  x0, x1 = 26, 42
        elif y < 31:  x0, x1 = 26, 43 # nose tip at x=43, y=29..30
        elif y < 34:  x0, x1 = 26, 42 # lips / chin
        elif y == 34: x0, x1 = 27, 40
        elif y == 35: x0, x1 = 28, 38
        else:         x0, x1 = 29, 36 # jaw to neck

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36:
                hp[x, y] = SKIN_OUTLINE
            elif x == x0 + 1 or y >= 34:
                hp[x, y] = SKIN_SHADOW
            else:
                hp[x, y] = SKIN_BASE

    # Cheek Blush (x=36..38, y=31..32)
    hp[36, 31] = BLUSH_COLOR; hp[37, 31] = BLUSH_COLOR; hp[37, 32] = BLUSH_COLOR

    # Side Anime Eye (Facing Right, x=34..39, y=26..30)
    for x in range(35, 40): hp[x, 26] = EYE_LASH
    hp[34, 26] = EYE_LASH; hp[40, 26] = EYE_LASH
    for y in range(27, 30):
        hp[35, y] = EYE_LASH
        hp[36, y] = EYE_IRIS_TOP if y == 27 else EYE_IRIS_BOT
        hp[37, y] = EYE_PUPIL if y == 28 else EYE_IRIS_TOP
        hp[38, y] = EYE_IRIS_BOT
        hp[39, y] = EYE_LASH
    for x in range(36, 39): hp[x, 30] = (120, 56, 44, 255)
    # Sparkle
    hp[36, 27] = EYE_SPARKLE; hp[37, 27] = EYE_SPARKLE

    # Eyebrow
    hp[35, 24] = F_HAIR_DEEP; hp[36, 24] = F_HAIR_DEEP; hp[37, 24] = F_HAIR_MID_SH

    # Hair Crown & Bangs (Over face profile)
    for y in range(11, 25):
        if y < 14:    x0, x1 = 22, 36
        elif y < 18:  x0, x1 = 18, 39
        else:         x0, x1 = 16, 40

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 11:
                hp[x, y] = F_HAIR_DEEP
            elif 13 <= y <= 16 and 24 <= x <= 35:
                hp[x, y] = F_HAIR_HL
            elif x < 20:
                hp[x, y] = F_HAIR_MID_SH
            elif y >= 22 and 33 <= x <= 39:
                pass # face shows through
            else:
                hp[x, y] = F_HAIR_BASE

    # Side bang tip curving forward
    hp[38, 23] = F_HAIR_BASE; hp[39, 23] = F_HAIR_BASE; hp[39, 24] = F_HAIR_DEEP
    hp[32, 24] = F_HAIR_BASE; hp[33, 24] = F_HAIR_DEEP; hp[32, 25] = F_HAIR_DEEP

    # Hair clip
    hp[30, 20] = F_CLIP_RED; hp[31, 20] = F_CLIP_GOLD; hp[30, 21] = F_CLIP_RED

    # Neck stub (y=36..39, x=29..35)
    for y in range(36, 40):
        for x in range(29, 35):
            hp[x, y] = SKIN_SHADOW if (x in (29, 34) or y == 39) else SKIN_BASE

    # ── BODY (Side view facing Right) ─────────────────────────────
    body = new_canvas()
    bp = body.load()

    for y in range(37, 49):
        if y == 37:    x0, x1 = 28, 35 # collar
        elif y == 38:  x0, x1 = 27, 37
        elif y < 44:   x0, x1 = 26, 38 # chest & back
        elif y < 46:   x0, x1 = 26, 37 # belt
        else:          x0, x1 = 26, 38 # shorts

        for x in range(x0, x1 + 1):
            if y == 38 and 33 <= x <= 36: bp[x, y] = COLLAR_WHITE
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUTLINE
                elif x == x0 + 1: bp[x, y] = TOP_DARK_SH
                elif x >= x1 - 2: bp[x, y] = TOP_HL
                else:             bp[x, y] = TOP_BASE
            elif y in (44, 45):
                if x >= x1 - 2:   bp[x, y] = BELT_BUCKLE
                elif x in (x0, x1): bp[x, y] = SHORTS_OUT
                else:             bp[x, y] = BELT_LEATHER
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SHORTS_OUT
                elif x == x0 + 1:            bp[x, y] = SHORTS_SHADOW
                elif x >= x1 - 2:            bp[x, y] = SHORTS_HL
                else:                        bp[x, y] = SHORTS_BASE

    # ── ARMS (EAST VIEW) ──────────────────────────────────────────
    # Far Arm (Arm Left - behind body, x=24..27, y=39..48)
    arm_l = new_canvas()
    alp = arm_l.load()
    for y in range(39, 48):
        for x in range(24, 28):
            if x in (24, 27) or y == 47: alp[x, y] = SKIN_OUTLINE
            else:                        alp[x, y] = SKIN_DEEP_SH

    # Near Arm (Arm Right - in front of body, x=33..37, y=38..48)
    arm_r = new_canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 33, 37
        if y == 38: x1 = 36 # rounded shoulder
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 47: arp[x, y] = SKIN_OUTLINE
            elif x == x0 + 1:            arp[x, y] = SKIN_SHADOW
            else:                        arp[x, y] = SKIN_BASE
    arp[35, 47] = SKIN_BASE; arp[36, 47] = SKIN_SHADOW

    # ── LEGS (EAST VIEW) ──────────────────────────────────────────
    # Far Leg (Leg Left - behind, x=26..30, y=47..58)
    leg_l = new_canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SHORTS_SHADOW
    for x in range(26, 31): llp[x, 49] = SHORTS_OUT
    for y in range(50, 54):
        for x in range(27, 31):
            llp[x, y] = SKIN_OUTLINE if x in (27, 30) else SKIN_DEEP_SH
    for y in range(54, 59):
        x0, x1 = 26, 31
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): llp[x, y] = BOOT_SOLE
            else:                        llp[x, y] = BOOT_SHADOW

    # Near Leg (Leg Right - front, x=30..35, y=47..58)
    leg_r = new_canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(31, 35): lrp[x, y] = SHORTS_BASE
    for x in range(30, 36): lrp[x, 49] = SHORTS_OUT
    for y in range(50, 54):
        for x in range(31, 35):
            if x in (31, 34): lrp[x, y] = SKIN_OUTLINE
            elif x == 32:     lrp[x, y] = SKIN_BASE
            else:             lrp[x, y] = SKIN_SHADOW
    for y in range(54, 59):
        x0, x1 = 30, 36
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): lrp[x, y] = BOOT_SOLE
            elif x >= 33:                lrp[x, y] = BOOT_HL
            else:                        lrp[x, y] = BOOT_BASE

    # Assembled Composite (Sorting: ArmL -> LegL -> Body -> LegR -> Head -> ArmR)
    composite = new_canvas()
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(body, (0, 0), body)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(head, (0, 0), head)
    composite.paste(arm_r, (0, 0), arm_r)

    return {
        "head": head,
        "body": body,
        "arm_left": arm_l,
        "arm_right": arm_r,
        "leg_left": leg_l,
        "leg_right": leg_r,
        "composite": composite
    }

# ════════════════════════════════════════════════════════════════
# 3. FEMALE NORTH (Back Chibi)
# ════════════════════════════════════════════════════════════════
def build_female_north():
    # HEAD (Back of head & hair)
    head = new_canvas()
    hp = head.load()

    # Full flowing back hair
    dome_bounds = {
        10: (29, 35),
        11: (26, 38),
        12: (24, 40),
        13: (22, 42),
        14: (20, 44),
        15: (19, 45),
        16: (18, 46),
        17: (17, 47),
        18: (17, 47),
        19: (17, 47),
        20: (16, 48),
        21: (16, 48),
        22: (16, 48),
        23: (16, 48),
        24: (16, 48),
        25: (16, 48),
        26: (17, 47),
        27: (17, 47),
        28: (17, 47),
        29: (18, 46),
        30: (18, 46),
        31: (19, 45),
        32: (20, 44),
        33: (21, 43),
        34: (22, 42),
        35: (24, 40),
        36: (26, 38),
        37: (28, 36)
    }
    for y, (x0, x1) in dome_bounds.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 10 or y == 37:
                hp[x, y] = F_HAIR_DEEP
            elif 13 <= y <= 16 and 25 <= x <= 39:
                hp[x, y] = F_HAIR_HL
            elif x in (x0 + 1, x1 - 1) or y >= 32:
                hp[x, y] = F_HAIR_MID_SH
            else:
                hp[x, y] = F_HAIR_BASE

    # Hair flow parting lines in back
    for y in range(18, 35):
        if y % 3 == 0:
            hp[32, y] = F_HAIR_MID_SH
        if y % 4 == 0:
            hp[25, y] = F_HAIR_DEEP; hp[39, y] = F_HAIR_DEEP

    # Red clips visible from behind
    hp[19, 21] = F_CLIP_RED; hp[20, 21] = F_CLIP_RED
    hp[44, 21] = F_CLIP_RED; hp[45, 21] = F_CLIP_RED

    # BODY (Back view)
    body = new_canvas()
    bp = body.load()

    for y in range(37, 49):
        if y == 37:    x0, x1 = 29, 35
        elif y == 38:  x0, x1 = 26, 38
        elif y == 39:  x0, x1 = 24, 40
        elif y < 44:   x0, x1 = 25, 39
        elif y < 46:   x0, x1 = 25, 39
        else:          x0, x1 = 25, 39

        for x in range(x0, x1 + 1):
            if y == 38 and 30 <= x <= 34:
                bp[x, y] = COLLAR_WHITE
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUTLINE
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SHADOW
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                bp[x, y] = BELT_LEATHER if x not in (x0, x1) else SHORTS_OUT
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SHORTS_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SHORTS_SHADOW
                else: bp[x, y] = SHORTS_BASE

    # ARMS (North view - seen from behind)
    arm_l = new_canvas()
    alp = arm_l.load()
    for y in range(38, 48):
        for x in range(21, 25):
            if x == 21 or y == 47: alp[x, y] = SKIN_OUTLINE
            else:                  alp[x, y] = SKIN_BASE

    arm_r = new_canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        for x in range(40, 44):
            if x == 43 or y == 47: arp[x, y] = SKIN_OUTLINE
            else:                  arp[x, y] = SKIN_BASE

    # LEGS (North view - back of legs & boots)
    leg_l = new_canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SHORTS_BASE
    for x in range(26, 32): llp[x, 49] = SHORTS_OUT
    for y in range(50, 54):
        for x in range(27, 31):
            llp[x, y] = SKIN_OUTLINE if x in (27, 30) else SKIN_BASE
    for y in range(54, 59):
        x0, x1 = 27, 31
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): llp[x, y] = BOOT_SOLE
            else:                        llp[x, y] = BOOT_BASE

    leg_r = new_canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38): lrp[x, y] = SHORTS_BASE
    for x in range(33, 39): lrp[x, 49] = SHORTS_OUT
    for y in range(50, 54):
        for x in range(34, 38):
            lrp[x, y] = SKIN_OUTLINE if x in (34, 37) else SKIN_BASE
    for y in range(54, 59):
        x0, x1 = 34, 38
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): lrp[x, y] = BOOT_SOLE
            else:                        lrp[x, y] = BOOT_BASE

    composite = new_canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "head": head,
        "body": body,
        "arm_left": arm_l,
        "arm_right": arm_r,
        "leg_left": leg_l,
        "leg_right": leg_r,
        "composite": composite
    }

if __name__ == "__main__":
    f_south = build_female_south()
    f_east  = build_female_east()
    f_north = build_female_north()

    # Save overview of female 3 angles
    overview = Image.new("RGBA", (64 * 3, 64), (0, 0, 0, 0))
    overview.paste(f_south["composite"], (0, 0))
    overview.paste(f_east["composite"],  (64, 0))
    overview.paste(f_north["composite"], (128, 0))

    big_ov = overview.resize((64 * 3 * 4, 64 * 4), Image.NEAREST)
    out_path = os.path.join(ARTIFACT_DIR, "preview_female_master_angles.png")
    big_ov.save(out_path)
    print("Saved female master overview to:", out_path)
