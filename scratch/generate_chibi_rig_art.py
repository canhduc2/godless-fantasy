# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Chibi Rig Generator
Crafts brand new pixel art parts for 2D Skeletal Animation
"""
import os
from PIL import Image, ImageDraw

ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

# ════════════════════════════════════════════════════════════════
# COLOR PALETTES
# ════════════════════════════════════════════════════════════════
# Skin
SKIN_HL     = (255, 238, 228, 255)
SKIN_BASE   = (248, 206, 182, 255)
SKIN_SHADOW = (224, 168, 142, 255)
SKIN_DEEP   = (192, 134, 110, 255)
SKIN_OUT    = (128, 72, 60, 255)
BLUSH       = (246, 172, 166, 255)

# Eyes
EYE_DARK    = (48, 26, 32, 255)
EYE_IRIS    = (106, 54, 42, 255)
EYE_WHITE   = (255, 255, 255, 255)
EYE_HL      = (255, 255, 255, 255)

# Female Hair (Chestnut Auburn)
F_HAIR_HL   = (182, 126, 96, 255)
F_HAIR_BASE = (142, 88, 62, 255)
F_HAIR_SH   = (102, 54, 34, 255)
F_HAIR_OUT  = (68, 32, 18, 255)
F_CLIP_RED  = (232, 68, 64, 255)
F_CLIP_HL   = (255, 138, 130, 255)

# Male Hair (Spiky Brown)
M_HAIR_HL   = (168, 116, 82, 255)
M_HAIR_BASE = (130, 80, 52, 255)
M_HAIR_SH   = (92, 48, 28, 255)
M_HAIR_OUT  = (58, 28, 16, 255)

# Clothes - Top (Salmon / Orange Adventurer Vest)
TOP_HL      = (242, 134, 108, 255)
TOP_BASE    = (218, 96, 74, 255)
TOP_SH      = (172, 62, 46, 255)
TOP_OUT     = (112, 36, 26, 255)
COLLAR_W    = (244, 240, 236, 255)
COLLAR_SH   = (198, 190, 184, 255)

# Clothes - Bottom (Charcoal Shorts & Belt)
BELT_BRN    = (115, 62, 38, 255)
BELT_GOLD   = (236, 192, 64, 255)
SH_HL       = (76, 76, 88, 255)
SH_BASE     = (52, 52, 62, 255)
SH_SH       = (34, 34, 42, 255)
SH_OUT      = (22, 22, 28, 255)

# Boots
BOOT_HL     = (92, 76, 66, 255)
BOOT_BASE   = (64, 50, 42, 255)
BOOT_SH     = (42, 32, 26, 255)
BOOT_SOLE   = (24, 18, 16, 255)

def create_canvas():
    return Image.new("RGBA", (64, 64), (0, 0, 0, 0))

# ════════════════════════════════════════════════════════════════
# 1. FEMALE SOUTH (Front View)
# ════════════════════════════════════════════════════════════════
def craft_female_south():
    # HEAD
    head = create_canvas()
    hp = head.load()

    # Hair Back (drawn under head or separate)
    hair_back = create_canvas()
    hbp = hair_back.load()
    # Flowing wavy twin locks in the back
    for y in range(24, 44):
        # Left back lock
        w = 3 + (y - 24) // 5
        for x in range(16 - w, 22):
            if x < 16 - w + 1 or x > 20: c = F_HAIR_OUT
            elif x < 18: c = F_HAIR_SH
            else: c = F_HAIR_BASE
            hbp[x, y] = c
        # Right back lock
        for x in range(42, 48 + w):
            if x == 42 or x == 47 + w: c = F_HAIR_OUT
            elif x > 44: c = F_HAIR_SH
            else: c = F_HAIR_BASE
            hbp[x, y] = c

    # Head & Face
    # 1. Neck stub (for seamless rotation over collar)
    for y in range(36, 40):
        for x in range(30, 35):
            hp[x, y] = SKIN_SHADOW if (x in (30, 34) or y == 39) else SKIN_BASE

    # 2. Chibi Face Base (rounded cheeks, cute chin)
    for y in range(22, 37):
        if y < 24:   x0, x1 = 23, 41
        elif y < 28: x0, x1 = 20, 44
        elif y < 33: x0, x1 = 19, 45 # max cheek width
        elif y == 33: x0, x1 = 21, 43
        elif y == 34: x0, x1 = 24, 40
        elif y == 35: x0, x1 = 27, 37
        else:        x0, x1 = 30, 34 # chin

        for x in range(x0, x1 + 1):
            if x == x0 or x == x1 or (y == 36):
                hp[x, y] = SKIN_OUT
            elif x == x0 + 1 or x == x1 - 1 or y >= 34:
                hp[x, y] = SKIN_SHADOW
            elif y == 23:
                hp[x, y] = SKIN_HL
            else:
                hp[x, y] = SKIN_BASE

    # Cheeks Blush
    hp[22, 31] = BLUSH; hp[23, 31] = BLUSH; hp[23, 30] = BLUSH
    hp[41, 31] = BLUSH; hp[42, 31] = BLUSH; hp[41, 30] = BLUSH

    # Mouth (cute small smile)
    hp[31, 33] = SKIN_OUT; hp[32, 33] = SKIN_OUT; hp[33, 33] = SKIN_OUT

    # Eyes (Large expressive chibi anime eyes)
    # Left eye: x=24..28, y=26..30
    for x in range(24, 29): hp[x, 26] = EYE_DARK # top eyelash
    hp[23, 26] = EYE_DARK # wing
    for y in range(27, 30):
        hp[24, y] = EYE_DARK
        hp[25, y] = EYE_IRIS
        hp[26, y] = EYE_IRIS
        hp[27, y] = EYE_IRIS
        hp[28, y] = EYE_DARK
    for x in range(25, 28): hp[x, 30] = EYE_DARK # bottom lid
    # Sparkle / highlight
    hp[25, 27] = EYE_HL
    hp[26, 27] = EYE_HL
    hp[27, 29] = (160, 90, 75, 255) # pupil lower reflex

    # Right eye: x=36..40, y=26..30
    for x in range(36, 41): hp[x, 26] = EYE_DARK # top eyelash
    hp[41, 26] = EYE_DARK # wing
    for y in range(27, 30):
        hp[36, y] = EYE_DARK
        hp[37, y] = EYE_IRIS
        hp[38, y] = EYE_IRIS
        hp[39, y] = EYE_IRIS
        hp[40, y] = EYE_DARK
    for x in range(37, 40): hp[x, 30] = EYE_DARK
    # Sparkle / highlight
    hp[37, 27] = EYE_HL
    hp[38, 27] = EYE_HL
    hp[39, 29] = (160, 90, 75, 255)

    # Hair Top & Bangs
    for y in range(11, 27):
        if y < 14:   x0, x1 = 26, 38
        elif y < 18: x0, x1 = 21, 43
        elif y < 22: x0, x1 = 18, 46
        else:        x0, x1 = 17, 47

        for x in range(x0, x1 + 1):
            # outline
            if y == 11 or (y < 18 and (x == x0 or x == x1)):
                hp[x, y] = F_HAIR_OUT
            elif y <= 16:
                # highlights
                hp[x, y] = F_HAIR_HL if (14 <= y <= 16 and 25 <= x <= 39) else F_HAIR_BASE
            elif y < 22:
                hp[x, y] = F_HAIR_BASE
            else:
                # Bangs tufts framing forehead
                # Middle forehead cut-out for face: x=24..40, y=23..25
                if 23 <= y <= 25 and 26 <= x <= 38:
                    pass # face shows through
                else:
                    hp[x, y] = F_HAIR_BASE

    # Bangs tips curving over forehead
    # Left bang strand
    hp[24, 22] = F_HAIR_BASE; hp[24, 23] = F_HAIR_BASE; hp[24, 24] = F_HAIR_BASE; hp[24, 25] = F_HAIR_OUT
    hp[25, 23] = F_HAIR_BASE; hp[25, 24] = F_HAIR_OUT
    # Center bang strands
    hp[30, 23] = F_HAIR_BASE; hp[30, 24] = F_HAIR_BASE; hp[30, 25] = F_HAIR_OUT
    hp[31, 24] = F_HAIR_BASE; hp[31, 25] = F_HAIR_OUT
    hp[34, 23] = F_HAIR_BASE; hp[34, 24] = F_HAIR_OUT
    # Right bang strand
    hp[39, 23] = F_HAIR_BASE; hp[39, 24] = F_HAIR_BASE; hp[39, 25] = F_HAIR_BASE; hp[40, 25] = F_HAIR_OUT

    # Cute red hairclips on sides
    hp[19, 22] = F_CLIP_HL;  hp[20, 22] = F_CLIP_RED; hp[19, 23] = F_CLIP_RED; hp[20, 23] = F_CLIP_RED
    hp[44, 22] = F_CLIP_HL;  hp[45, 22] = F_CLIP_RED; hp[44, 23] = F_CLIP_RED; hp[45, 23] = F_CLIP_RED

    # Outer hair outlines & depth
    for y in range(12, 38):
        if y < 36:
            hp[18, y] = F_HAIR_OUT if y > 20 else hp[18, y]
            hp[46, y] = F_HAIR_OUT if y > 20 else hp[46, y]

    # ════════════════════════════════════════════════════════════════
    # TORSO / BODY
    # ════════════════════════════════════════════════════════════════
    body = create_canvas()
    bp = body.load()

    # Shoulder caps & neck base (y=37..40)
    for y in range(37, 49):
        if y == 37:    x0, x1 = 29, 35 # neck collar
        elif y == 38:  x0, x1 = 26, 38 # upper shoulders
        elif y == 39:  x0, x1 = 24, 40 # full shoulders
        elif y < 44:   x0, x1 = 25, 39 # chest & waist
        elif y < 47:   x0, x1 = 25, 39 # belt & hips
        else:          x0, x1 = 25, 39 # shorts pelvis

        for x in range(x0, x1 + 1):
            # Collar V-trim
            if y == 38 and 30 <= x <= 34:
                bp[x, y] = COLLAR_W
            elif y == 39 and 31 <= x <= 33:
                bp[x, y] = COLLAR_W
            # Shirt
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SH
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            # Belt
            elif y in (44, 45):
                if 30 <= x <= 34:
                    bp[x, y] = BELT_GOLD if (31 <= x <= 33 and y == 44) else (180, 140, 40, 255)
                else:
                    bp[x, y] = BELT_BRN if x not in (x0, x1) else (60, 30, 18, 255)
            # Shorts Pelvis
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SH_SH
                elif 30 <= x <= 34: bp[x, y] = SH_HL
                else: bp[x, y] = SH_BASE

    # ════════════════════════════════════════════════════════════════
    # ARMS (LEFT & RIGHT)
    # ════════════════════════════════════════════════════════════════
    # Arm Left (hangs at left side, x=21..24, y=39..48)
    arm_l = create_canvas()
    alp = arm_l.load()
    # Rounded shoulder cap (y=38..40, pivots smoothly)
    for y in range(38, 48):
        x0, x1 = 21, 24
        for x in range(x0, x1 + 1):
            if x == x0 or y == 47: alp[x, y] = SKIN_OUT
            elif x == x0 + 1:      alp[x, y] = SKIN_SHADOW
            elif y == 38:          alp[x, y] = SKIN_SHADOW
            else:                  alp[x, y] = SKIN_BASE
    # Chibi hand/fist (y=45..47)
    alp[22, 47] = SKIN_SHADOW; alp[23, 47] = SKIN_BASE

    # Arm Right (hangs at right side, x=40..43, y=39..48)
    arm_r = create_canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 40, 43
        for x in range(x0, x1 + 1):
            if x == x1 or y == 47: arp[x, y] = SKIN_OUT
            elif x == x1 - 1:      arp[x, y] = SKIN_SHADOW
            elif y == 38:          arp[x, y] = SKIN_SHADOW
            else:                  arp[x, y] = SKIN_BASE
    arp[41, 47] = SKIN_BASE; arp[42, 47] = SKIN_SHADOW

    # ════════════════════════════════════════════════════════════════
    # LEGS (LEFT & RIGHT)
    # ════════════════════════════════════════════════════════════════
    # Leg Left (x=26..31, y=47..58)
    leg_l = create_canvas()
    llp = leg_l.load()
    # Hip joint cap (y=47..49, rounded to rotate cleanly under shorts)
    for y in range(47, 50):
        for x in range(27, 31):
            llp[x, y] = SH_BASE if y < 49 else SH_HL
    # Shorts cuff outline
    for x in range(26, 32): llp[x, 49] = SH_OUT
    # Bare Thigh / Knee (y=50..53)
    for y in range(50, 54):
        for x in range(27, 31):
            if x == 27 or x == 30: llp[x, y] = SKIN_OUT
            elif x == 28:          llp[x, y] = SKIN_BASE
            else:                  llp[x, y] = SKIN_SHADOW
    # Boot (y=54..58)
    for y in range(54, 59):
        x0 = 26 if y >= 56 else 27
        x1 = 31
        for x in range(x0, x1 + 1):
            if y == 58 or x == x0 or x == x1: llp[x, y] = BOOT_SOLE
            elif y == 57:                     llp[x, y] = BOOT_SH
            elif x == x0 + 1:                 llp[x, y] = BOOT_HL
            else:                             llp[x, y] = BOOT_BASE

    # Leg Right (x=33..38, y=47..58)
    leg_r = create_canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38):
            lrp[x, y] = SH_BASE if y < 49 else SH_HL
    for x in range(33, 39): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(34, 38):
            if x == 34 or x == 37: lrp[x, y] = SKIN_OUT
            elif x == 35:          lrp[x, y] = SKIN_BASE
            else:                  lrp[x, y] = SKIN_SHADOW
    for y in range(54, 59):
        x0 = 33
        x1 = 38 if y >= 56 else 37
        for x in range(x0, x1 + 1):
            if y == 58 or x == x0 or x == x1: lrp[x, y] = BOOT_SOLE
            elif y == 57:                     lrp[x, y] = BOOT_SH
            elif x == x0 + 1:                 lrp[x, y] = BOOT_HL
            else:                             lrp[x, y] = BOOT_BASE

    # Composite Full Body
    composite = create_canvas()
    composite.paste(hair_back, (0, 0), hair_back)
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "head": head,
        "hair_back": hair_back,
        "body": body,
        "arm_left": arm_l,
        "arm_right": arm_r,
        "leg_left": leg_l,
        "leg_right": leg_r,
        "composite": composite
    }

if __name__ == "__main__":
    parts = craft_female_south()
    out_path = os.path.join(ARTIFACT_DIR, "test_crafted_female_south.png")
    # Scale up 4x for easy viewing
    big = parts["composite"].resize((256, 256), Image.NEAREST)
    big.save(out_path)
    print("Saved preview to:", out_path)
