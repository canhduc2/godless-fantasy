# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Chibi Generator v2
Crafts pristine, solid pixel-art character parts for Female & Male 2D Rig.
"""
import os
from PIL import Image

ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

# ════════════════════════════════════════════════════════════════
# COLOR PALETTES
# ════════════════════════════════════════════════════════════════
# Peachy Anime Skin
SK_HL       = (255, 240, 230, 255) # Light shine
SK_BASE     = (248, 208, 184, 255) # Base peachy
SK_SH       = (222, 168, 142, 255) # Soft shadow
SK_DEEP_SH  = (188, 134, 110, 255) # Deep shadow
SK_OUT      = (112, 60, 48, 255)   # Outline
BLUSH       = (246, 170, 164, 255) # Cheek blush

# Eyes
EYE_DARK    = (44, 24, 30, 255)    # Eyelashes
EYE_PUPIL   = (64, 32, 28, 255)    # Deep pupil
EYE_IRIS_T  = (118, 58, 44, 255)   # Upper iris
EYE_IRIS_B  = (178, 98, 70, 255)   # Lower amber reflex
EYE_HL      = (255, 255, 255, 255) # Sparkle catchlight

# Male Eyes (Determined Hazel/Greenish or warm brown)
M_EYE_IRIS_T = (88, 74, 46, 255)
M_EYE_IRIS_B = (142, 128, 72, 255)

# Female Hair (Chestnut Auburn Wavy)
F_HAIR_HL   = (194, 138, 106, 255)
F_HAIR_BASE = (148, 92, 64, 255)
F_HAIR_SH   = (114, 64, 42, 255)
F_HAIR_OUT  = (76, 38, 24, 255)
F_CLIP_RED  = (234, 68, 64, 255)
F_CLIP_GOLD = (248, 196, 72, 255)

# Male Hair (Spiky Fluffy Brown)
M_HAIR_HL   = (178, 126, 90, 255)
M_HAIR_BASE = (138, 86, 56, 255)
M_HAIR_SH   = (102, 56, 34, 255)
M_HAIR_OUT  = (66, 32, 18, 255)

# Clothes - Top (Adventurer Salmon / Orange Top)
TOP_HL      = (246, 138, 112, 255)
TOP_BASE    = (220, 96, 74, 255)
TOP_SH      = (174, 64, 48, 255)
TOP_OUT     = (82, 26, 20, 255)
COLLAR_W    = (246, 242, 238, 255)
COLLAR_SH   = (196, 188, 182, 255)

# Clothes - Belt & Shorts
BELT_BRN    = (112, 58, 36, 255)
BELT_GOLD   = (240, 196, 66, 255)
SH_HL       = (78, 78, 90, 255)
SH_BASE     = (52, 52, 62, 255)
SH_SH       = (34, 34, 42, 255)
SH_OUT      = (20, 20, 26, 255)

# Boots
BOOT_HL     = (94, 78, 68, 255)
BOOT_BASE   = (66, 52, 44, 255)
BOOT_SH     = (44, 34, 28, 255)
BOOT_SOLE   = (24, 18, 16, 255)

def canvas():
    return Image.new("RGBA", (64, 64), (0, 0, 0, 0))

# ════════════════════════════════════════════════════════════════
# 1. FEMALE SOUTH (Front Chibi)
# ════════════════════════════════════════════════════════════════
def craft_female_south():
    head = canvas()
    hp = head.load()

    # Hair back volume (locks curving around shoulders)
    for y in range(22, 44):
        # Left lock
        lx0 = 15 - (1 if 26 <= y <= 36 else 0)
        for x in range(lx0, 22):
            if x == lx0 or y == 43: hp[x, y] = F_HAIR_OUT
            elif x <= lx0 + 1:     hp[x, y] = F_HAIR_SH
            else:                  hp[x, y] = F_HAIR_BASE
        # Right lock
        rx1 = 49 + (1 if 26 <= y <= 36 else 0)
        for x in range(43, rx1 + 1):
            if x == rx1 or y == 43: hp[x, y] = F_HAIR_OUT
            elif x >= rx1 - 1:      hp[x, y] = F_HAIR_SH
            else:                   hp[x, y] = F_HAIR_BASE

    # Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            hp[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

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
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: hp[x, y] = SK_SH
            elif y == 23 and 27 <= x <= 37: hp[x, y] = SK_HL
            else: hp[x, y] = SK_BASE

    # Blush
    for bx, by in [(21, 31), (22, 31), (22, 30), (23, 31)]: hp[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31), (42, 30), (43, 31)]: hp[bx, by] = BLUSH

    # Mouth (cute small smile)
    hp[31, 33] = SK_OUT; hp[32, 34] = (172, 82, 82, 255); hp[33, 33] = SK_OUT

    # Eyes (Left: 24..28, Right: 36..40)
    for x in range(24, 29): hp[x, 26] = EYE_DARK
    hp[23, 26] = EYE_DARK; hp[29, 26] = EYE_DARK
    for y in range(27, 30):
        hp[24, y] = EYE_DARK; hp[28, y] = EYE_DARK
        hp[25, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        hp[26, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        hp[27, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    for x in range(25, 28): hp[x, 30] = (120, 56, 44, 255)
    hp[25, 27] = EYE_HL; hp[26, 27] = EYE_HL

    for x in range(36, 41): hp[x, 26] = EYE_DARK
    hp[35, 26] = EYE_DARK; hp[41, 26] = EYE_DARK
    for y in range(27, 30):
        hp[36, y] = EYE_DARK; hp[40, y] = EYE_DARK
        hp[37, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        hp[38, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        hp[39, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    for x in range(37, 40): hp[x, 30] = (120, 56, 44, 255)
    hp[37, 27] = EYE_HL; hp[38, 27] = EYE_HL

    # Eyebrows
    hp[24, 24] = F_HAIR_SH; hp[25, 24] = F_HAIR_OUT; hp[26, 24] = F_HAIR_OUT
    hp[38, 24] = F_HAIR_OUT; hp[39, 24] = F_HAIR_OUT; hp[40, 24] = F_HAIR_SH

    # Hair Dome
    dome = {
        10: (29, 35), 11: (26, 38), 12: (24, 40), 13: (22, 42),
        14: (20, 44), 15: (19, 45), 16: (18, 46), 17: (17, 47),
        18: (17, 47), 19: (17, 47), 20: (17, 47), 21: (17, 47), 22: (17, 47)
    }
    for y, (x0, x1) in dome.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 10: hp[x, y] = F_HAIR_OUT
            elif y in (11, 12) and x in (x0 + 1, x1 - 1): hp[x, y] = F_HAIR_SH
            elif 13 <= y <= 15 and 25 <= x <= 39: hp[x, y] = F_HAIR_HL
            elif y >= 16 and x in (x0 + 1, x1 - 1): hp[x, y] = F_HAIR_SH
            else: hp[x, y] = F_HAIR_BASE

    # Bangs
    for y in range(23, 28):
        for x in range(20, 24): hp[x, y] = F_HAIR_OUT if (x == 20 or y == 27) else F_HAIR_BASE
    hp[25, 23] = F_HAIR_BASE; hp[26, 23] = F_HAIR_BASE; hp[27, 23] = F_HAIR_BASE
    hp[26, 24] = F_HAIR_BASE; hp[27, 24] = F_HAIR_OUT; hp[26, 25] = F_HAIR_OUT

    hp[30, 23] = F_HAIR_BASE; hp[31, 23] = F_HAIR_BASE; hp[32, 23] = F_HAIR_BASE
    hp[31, 24] = F_HAIR_BASE; hp[32, 24] = F_HAIR_BASE; hp[33, 24] = F_HAIR_OUT; hp[32, 25] = F_HAIR_OUT

    hp[34, 23] = F_HAIR_BASE; hp[35, 23] = F_HAIR_BASE; hp[35, 24] = F_HAIR_OUT

    for y in range(23, 28):
        for x in range(41, 45): hp[x, y] = F_HAIR_OUT if (x == 44 or y == 27) else F_HAIR_BASE

    # Red/Gold Hair Clips
    hp[19, 21] = F_CLIP_RED; hp[20, 21] = F_CLIP_GOLD; hp[19, 22] = F_CLIP_RED; hp[20, 22] = F_CLIP_RED
    hp[44, 21] = F_CLIP_GOLD; hp[45, 21] = F_CLIP_RED; hp[44, 22] = F_CLIP_RED; hp[45, 22] = F_CLIP_RED

    # BODY
    body = canvas()
    bp = body.load()
    for y in range(37, 49):
        if y == 37:    x0, x1 = 29, 35
        elif y == 38:  x0, x1 = 26, 38
        elif y == 39:  x0, x1 = 24, 40
        elif y < 44:   x0, x1 = 25, 39
        elif y < 46:   x0, x1 = 25, 39
        else:          x0, x1 = 25, 39

        for x in range(x0, x1 + 1):
            if y == 38 and 30 <= x <= 34: bp[x, y] = COLLAR_W
            elif y == 39 and 31 <= x <= 33: bp[x, y] = COLLAR_W
            elif y == 40 and x == 32: bp[x, y] = COLLAR_SH
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SH
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                if 31 <= x <= 33: bp[x, y] = BELT_GOLD if y == 44 else (186, 142, 44, 255)
                elif x in (x0, x1): bp[x, y] = SH_OUT
                else: bp[x, y] = BELT_BRN if y == 44 else (80, 40, 24, 255)
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SH_SH
                elif 31 <= x <= 33: bp[x, y] = SH_HL
                else: bp[x, y] = SH_BASE

    # ARMS
    arm_l = canvas()
    alp = arm_l.load()
    for y in range(38, 48):
        x0, x1 = 21, 24
        if y == 38: x0 = 22
        for x in range(x0, x1 + 1):
            if x == x0 or y == 47: alp[x, y] = SK_OUT
            elif x == x0 + 1: alp[x, y] = SK_SH
            elif y == 38: alp[x, y] = SK_SH
            else: alp[x, y] = SK_BASE
    alp[22, 47] = SK_SH; alp[23, 47] = SK_BASE

    arm_r = canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 40, 43
        if y == 38: x1 = 42
        for x in range(x0, x1 + 1):
            if x == x1 or y == 47: arp[x, y] = SK_OUT
            elif x == x1 - 1: arp[x, y] = SK_SH
            elif y == 38: arp[x, y] = SK_SH
            else: arp[x, y] = SK_BASE
    arp[41, 47] = SK_BASE; arp[42, 47] = SK_SH

    # LEGS
    leg_l = canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_BASE if y < 49 else SH_HL
    for x in range(26, 32): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31):
            if x in (27, 30): llp[x, y] = SK_OUT
            elif x == 28: llp[x, y] = SK_BASE
            else: llp[x, y] = SK_SH
    for y in range(54, 59):
        x0 = 26 if y >= 56 else 27
        x1 = 31
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): llp[x, y] = BOOT_SOLE
            elif y == 57: llp[x, y] = BOOT_SH
            elif x == x0 + 1: llp[x, y] = BOOT_HL
            else: llp[x, y] = BOOT_BASE

    leg_r = canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38): lrp[x, y] = SH_BASE if y < 49 else SH_HL
    for x in range(33, 39): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(34, 38):
            if x in (34, 37): lrp[x, y] = SK_OUT
            elif x == 35: lrp[x, y] = SK_BASE
            else: lrp[x, y] = SK_SH
    for y in range(54, 59):
        x0 = 33
        x1 = 38 if y >= 56 else 37
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): lrp[x, y] = BOOT_SOLE
            elif y == 57: lrp[x, y] = BOOT_SH
            elif x == x0 + 1: lrp[x, y] = BOOT_HL
            else: lrp[x, y] = BOOT_BASE

    composite = canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "head": head, "body": body,
        "arm_left": arm_l, "arm_right": arm_r,
        "leg_left": leg_l, "leg_right": leg_r,
        "composite": composite
    }

# ════════════════════════════════════════════════════════════════
# 2. MALE SOUTH (Front Chibi)
# ════════════════════════════════════════════════════════════════
def craft_male_south():
    head = canvas()
    hp = head.load()

    # Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            hp[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

    # Face base
    for y in range(22, 37):
        if y < 24:    x0, x1 = 23, 41
        elif y < 28:  x0, x1 = 20, 44
        elif y < 33:  x0, x1 = 19, 45
        elif y == 33: x0, x1 = 21, 43
        elif y == 34: x0, x1 = 24, 40
        elif y == 35: x0, x1 = 27, 37
        else:         x0, x1 = 30, 34

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: hp[x, y] = SK_SH
            elif y == 23 and 27 <= x <= 37: hp[x, y] = SK_HL
            else: hp[x, y] = SK_BASE

    # Blush
    for bx, by in [(22, 31), (23, 31)]: hp[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31)]: hp[bx, by] = BLUSH

    # Determined grin / smile
    hp[31, 33] = SK_OUT; hp[32, 33] = (160, 68, 68, 255); hp[33, 33] = SK_OUT; hp[34, 32] = SK_OUT

    # Male Anime Eyes (Crisp & determined)
    for x in range(24, 29): hp[x, 26] = EYE_DARK
    hp[23, 26] = EYE_DARK
    for y in range(27, 30):
        hp[24, y] = EYE_DARK; hp[28, y] = EYE_DARK
        hp[25, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        hp[26, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        hp[27, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    for x in range(25, 28): hp[x, 30] = (100, 70, 40, 255)
    hp[25, 27] = EYE_HL

    for x in range(36, 41): hp[x, 26] = EYE_DARK
    hp[41, 26] = EYE_DARK
    for y in range(27, 30):
        hp[36, y] = EYE_DARK; hp[40, y] = EYE_DARK
        hp[37, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        hp[38, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        hp[39, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    for x in range(37, 40): hp[x, 30] = (100, 70, 40, 255)
    hp[37, 27] = EYE_HL

    # Bold eyebrows
    for x in range(23, 28): hp[x, 24] = M_HAIR_OUT
    for x in range(37, 42): hp[x, 24] = M_HAIR_OUT

    # Male Spiky Fluffy Hair Dome
    dome = {
        8:  (30, 34),
        9:  (28, 36),
        10: (26, 38), 11: (24, 40), 12: (22, 42), 13: (20, 44),
        14: (18, 46), 15: (17, 47), 16: (16, 48), 17: (16, 48),
        18: (16, 48), 19: (16, 48), 20: (16, 48), 21: (17, 47), 22: (18, 46)
    }
    for y, (x0, x1) in dome.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 8: hp[x, y] = M_HAIR_OUT
            elif 11 <= y <= 14 and 25 <= x <= 39: hp[x, y] = M_HAIR_HL
            elif x in (x0 + 1, x1 - 1) or y <= 10: hp[x, y] = M_HAIR_SH
            else: hp[x, y] = M_HAIR_BASE

    # Outer hair spikes / tufts (Spiky adventurer silhouette)
    spikes = [
        (23, 11), (22, 12), (21, 13), (17, 15), (15, 17), (15, 19), (16, 21), # left spikes
        (41, 11), (42, 12), (43, 13), (47, 15), (49, 17), (49, 19), (48, 21), # right spikes
        (29, 7), (32, 6), (35, 7) # top spikes
    ]
    for sx, sy in spikes:
        hp[sx, sy] = M_HAIR_OUT
        if 0 <= sx + 1 < 64: hp[sx + 1, sy] = M_HAIR_HL

    # Bangs
    for y in range(22, 27):
        hp[22, y] = M_HAIR_BASE; hp[23, y] = M_HAIR_BASE
    hp[22, 27] = M_HAIR_OUT; hp[23, 27] = M_HAIR_OUT

    # Center spikes over forehead
    hp[27, 23] = M_HAIR_BASE; hp[28, 23] = M_HAIR_BASE; hp[28, 24] = M_HAIR_OUT
    hp[32, 23] = M_HAIR_BASE; hp[33, 23] = M_HAIR_BASE; hp[33, 24] = M_HAIR_BASE; hp[33, 25] = M_HAIR_OUT
    hp[37, 23] = M_HAIR_BASE; hp[38, 23] = M_HAIR_BASE; hp[38, 24] = M_HAIR_OUT

    for y in range(22, 27):
        hp[41, y] = M_HAIR_BASE; hp[42, y] = M_HAIR_BASE
    hp[41, 27] = M_HAIR_OUT; hp[42, 27] = M_HAIR_OUT

    # BODY (Male Tank Top / Vest)
    body = canvas()
    bp = body.load()
    for y in range(37, 49):
        if y == 37:    x0, x1 = 29, 35
        elif y == 38:  x0, x1 = 26, 38
        elif y == 39:  x0, x1 = 24, 40
        elif y < 44:   x0, x1 = 25, 39
        elif y < 46:   x0, x1 = 25, 39
        else:          x0, x1 = 25, 39

        for x in range(x0, x1 + 1):
            if y == 38 and 30 <= x <= 34: bp[x, y] = COLLAR_W
            elif y == 39 and 31 <= x <= 33: bp[x, y] = COLLAR_W
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SH
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                if 31 <= x <= 33: bp[x, y] = BELT_GOLD if y == 44 else (186, 142, 44, 255)
                elif x in (x0, x1): bp[x, y] = SH_OUT
                else: bp[x, y] = BELT_BRN if y == 44 else (80, 40, 24, 255)
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SH_SH
                elif 31 <= x <= 33: bp[x, y] = SH_HL
                else: bp[x, y] = SH_BASE

    # ARMS (Male)
    arm_l = canvas()
    alp = arm_l.load()
    for y in range(38, 48):
        x0, x1 = 21, 24
        if y == 38: x0 = 22
        for x in range(x0, x1 + 1):
            if x == x0 or y == 47: alp[x, y] = SK_OUT
            elif x == x0 + 1: alp[x, y] = SK_SH
            elif y == 38: alp[x, y] = SK_SH
            else: alp[x, y] = SK_BASE
    alp[22, 47] = SK_SH; alp[23, 47] = SK_BASE

    arm_r = canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 40, 43
        if y == 38: x1 = 42
        for x in range(x0, x1 + 1):
            if x == x1 or y == 47: arp[x, y] = SK_OUT
            elif x == x1 - 1: arp[x, y] = SK_SH
            elif y == 38: arp[x, y] = SK_SH
            else: arp[x, y] = SK_BASE
    arp[41, 47] = SK_BASE; arp[42, 47] = SK_SH

    # LEGS (Male)
    leg_l = canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_BASE if y < 49 else SH_HL
    for x in range(26, 32): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31):
            if x in (27, 30): llp[x, y] = SK_OUT
            elif x == 28: llp[x, y] = SK_BASE
            else: llp[x, y] = SK_SH
    for y in range(54, 59):
        x0 = 26 if y >= 56 else 27
        x1 = 31
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): llp[x, y] = BOOT_SOLE
            elif y == 57: llp[x, y] = BOOT_SH
            elif x == x0 + 1: llp[x, y] = BOOT_HL
            else: llp[x, y] = BOOT_BASE

    leg_r = canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38): lrp[x, y] = SH_BASE if y < 49 else SH_HL
    for x in range(33, 39): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(34, 38):
            if x in (34, 37): lrp[x, y] = SK_OUT
            elif x == 35: lrp[x, y] = SK_BASE
            else: lrp[x, y] = SK_SH
    for y in range(54, 59):
        x0 = 33
        x1 = 38 if y >= 56 else 37
        for x in range(x0, x1 + 1):
            if y == 58 or x in (x0, x1): lrp[x, y] = BOOT_SOLE
            elif y == 57: lrp[x, y] = BOOT_SH
            elif x == x0 + 1: lrp[x, y] = BOOT_HL
            else: lrp[x, y] = BOOT_BASE

    composite = canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "head": head, "body": body,
        "arm_left": arm_l, "arm_right": arm_r,
        "leg_left": leg_l, "leg_right": leg_r,
        "composite": composite
    }

if __name__ == "__main__":
    f = craft_female_south()
    m = craft_male_south()

    test = Image.new("RGBA", (128, 64), (0, 0, 0, 0))
    test.paste(f["composite"], (0, 0))
    test.paste(m["composite"], (64, 0))

    big = test.resize((128 * 4, 64 * 4), Image.NEAREST)
    out_path = os.path.join(ARTIFACT_DIR, "preview_south_female_and_male.png")
    big.save(out_path)
    print("Saved preview to:", out_path)
