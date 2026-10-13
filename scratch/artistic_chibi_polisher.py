# -*- coding: utf-8 -*-
"""
Artistic Chibi Pixel Art Polisher
Renders soft anime curves, radiant spiky hair, expressive anime eyes, and curved bangs.
"""
from PIL import Image

SK_HL       = (255, 242, 234, 255)
SK_BASE     = (248, 208, 186, 255)
SK_SH       = (222, 168, 144, 255)
SK_DEEP     = (192, 134, 110, 255)
SK_OUT      = (112, 58, 46, 255)
BLUSH       = (248, 168, 164, 255)

EYE_LASH    = (40, 22, 28, 255)
EYE_PUPIL   = (62, 30, 26, 255)
EYE_IRIS_T  = (118, 56, 42, 255)
EYE_IRIS_B  = (182, 98, 68, 255)
M_EYE_IRIS_T = (88, 74, 46, 255)
M_EYE_IRIS_B = (142, 128, 72, 255)
EYE_WHITE   = (255, 252, 252, 255)
EYE_HL      = (255, 255, 255, 255)

F_HAIR_HL   = (198, 142, 108, 255)
F_HAIR_BASE = (150, 94, 66, 255)
F_HAIR_SH   = (114, 64, 42, 255)
F_HAIR_OUT  = (76, 36, 22, 255)
F_CLIP_RED  = (236, 68, 64, 255)
F_CLIP_GOLD = (248, 198, 72, 255)

M_HAIR_HL   = (182, 130, 92, 255)
M_HAIR_BASE = (140, 88, 58, 255)
M_HAIR_SH   = (104, 58, 36, 255)
M_HAIR_OUT  = (68, 32, 18, 255)

TOP_HL      = (246, 138, 112, 255)
TOP_BASE    = (220, 96, 74, 255)
TOP_SH      = (174, 64, 48, 255)
TOP_OUT     = (82, 26, 20, 255)
COLLAR_W    = (246, 242, 238, 255)
COLLAR_SH   = (196, 188, 182, 255)

BELT_BRN    = (112, 58, 36, 255)
BELT_GOLD   = (240, 196, 66, 255)
SH_HL       = (78, 78, 90, 255)
SH_BASE     = (52, 52, 62, 255)
SH_SH       = (34, 34, 42, 255)
SH_OUT      = (20, 20, 26, 255)

BOOT_HL     = (94, 78, 68, 255)
BOOT_BASE   = (66, 52, 44, 255)
BOOT_SH     = (44, 34, 28, 255)
BOOT_SOLE   = (24, 18, 16, 255)

def craft_female_head_v3():
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    p = im.load()

    # 1. Back hair flowing locks (smooth natural curves)
    for y in range(20, 44):
        # Left lock: starts at ear (x=17), widens to x=15 at y=30, curves to x=18 at y=42
        if y < 26:   x_out, x_in = 17, 21
        elif y < 34: x_out, x_in = 15, 21
        elif y < 40: x_out, x_in = 16, 22
        else:        x_out, x_in = 18, 21 # tip

        for x in range(x_out, x_in + 1):
            if x == x_out or y == 43: p[x, y] = F_HAIR_OUT
            elif x == x_out + 1 or y >= 41: p[x, y] = F_HAIR_SH
            elif x == x_in: p[x, y] = F_HAIR_SH
            else: p[x, y] = F_HAIR_BASE

        # Right lock
        rx_out = 64 - x_out - 1
        rx_in  = 64 - x_in - 1
        for x in range(rx_in, rx_out + 1):
            if x == rx_out or y == 43: p[x, y] = F_HAIR_OUT
            elif x == rx_out - 1 or y >= 41: p[x, y] = F_HAIR_SH
            elif x == rx_in: p[x, y] = F_HAIR_SH
            else: p[x, y] = F_HAIR_BASE

    # 2. Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            p[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

    # 3. Soft Chibi Face (natural anime jaw curve)
    face_rows = {
        22: (24, 40), 23: (23, 41), 24: (22, 42), 25: (21, 43),
        26: (20, 44), 27: (19, 45), 28: (19, 45), 29: (19, 45),
        30: (19, 45), 31: (20, 44), 32: (21, 43), 33: (23, 41),
        34: (25, 39), 35: (27, 37), 36: (30, 34)
    }
    for y, (x0, x1) in face_rows.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: p[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: p[x, y] = SK_SH
            elif y == 23 and 28 <= x <= 36: p[x, y] = SK_HL
            else: p[x, y] = SK_BASE

    # Cheeks Blush
    for bx, by in [(21, 31), (22, 31), (22, 30), (23, 31)]: p[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31), (42, 30), (43, 31)]: p[bx, by] = BLUSH

    # Mouth (cute happy anime smile)
    p[31, 33] = SK_OUT; p[32, 34] = (176, 80, 80, 255); p[33, 33] = SK_OUT

    # 4. Expressive Anime Eyes (Curved upper lid + large sparkle)
    # Left eye: x=24..28, y=25..30
    p[24, 26] = EYE_LASH; p[25, 25] = EYE_LASH; p[26, 25] = EYE_LASH
    p[27, 25] = EYE_LASH; p[28, 26] = EYE_LASH; p[23, 26] = EYE_LASH
    for y in range(27, 30):
        p[24, y] = EYE_WHITE; p[28, y] = EYE_WHITE
        p[25, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        p[26, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        p[27, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    p[24, 30] = (130, 60, 48, 255); p[25, 30] = EYE_LASH; p[26, 30] = EYE_LASH
    p[27, 30] = EYE_LASH; p[28, 30] = (130, 60, 48, 255)
    # Sparkle
    p[25, 26] = EYE_HL; p[26, 26] = EYE_HL

    # Right eye: x=36..40, y=25..30
    p[36, 26] = EYE_LASH; p[37, 25] = EYE_LASH; p[38, 25] = EYE_LASH
    p[39, 25] = EYE_LASH; p[40, 26] = EYE_LASH; p[41, 26] = EYE_LASH
    for y in range(27, 30):
        p[36, y] = EYE_WHITE; p[40, y] = EYE_WHITE
        p[37, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        p[38, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        p[39, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    p[36, 30] = (130, 60, 48, 255); p[37, 30] = EYE_LASH; p[38, 30] = EYE_LASH
    p[39, 30] = EYE_LASH; p[40, 30] = (130, 60, 48, 255)
    # Sparkle
    p[37, 26] = EYE_HL; p[38, 26] = EYE_HL

    # Eyebrows
    p[24, 23] = F_HAIR_SH; p[25, 23] = F_HAIR_OUT; p[26, 23] = F_HAIR_OUT; p[27, 24] = F_HAIR_SH
    p[37, 24] = F_HAIR_SH; p[38, 23] = F_HAIR_OUT; p[39, 23] = F_HAIR_OUT; p[40, 23] = F_HAIR_SH

    # 5. Hair Crown Dome (Natural curved top, not a block)
    crown_rows = {
        9:  (29, 35),
        10: (26, 38),
        11: (24, 40),
        12: (22, 42),
        13: (20, 44),
        14: (19, 45),
        15: (18, 46),
        16: (17, 47),
        17: (17, 47),
        18: (17, 47),
        19: (17, 47),
        20: (17, 47),
        21: (17, 47),
        22: (17, 47)
    }
    for y, (x0, x1) in crown_rows.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 9: p[x, y] = F_HAIR_OUT
            # Curved highlight band following crown curvature
            elif y in (12, 13, 14) and (24 <= x <= 40):
                # Crescent highlight shape
                if (y == 12 and 27 <= x <= 37) or (y == 13 and 25 <= x <= 39) or (y == 14 and 26 <= x <= 38):
                    p[x, y] = F_HAIR_HL
                else: p[x, y] = F_HAIR_BASE
            elif x in (x0 + 1, x1 - 1) or y <= 11: p[x, y] = F_HAIR_SH
            else: p[x, y] = F_HAIR_BASE

    # 6. Curved Anime Bangs (Natural overlapping strands)
    # Left temple framing
    for y in range(21, 28):
        for x in range(18, 23):
            if x == 18 or y == 27: p[x, y] = F_HAIR_OUT
            elif x == 19: p[x, y] = F_HAIR_SH
            else: p[x, y] = F_HAIR_BASE

    # Bang strand 1 (Left sweep)
    for bx, by, c in [(23, 22, F_HAIR_BASE), (24, 22, F_HAIR_BASE), (24, 23, F_HAIR_BASE), (24, 24, F_HAIR_OUT),
                      (25, 23, F_HAIR_BASE), (25, 24, F_HAIR_OUT)]:
        p[bx, by] = c

    # Bang strand 2 (Center left)
    for bx, by, c in [(28, 22, F_HAIR_BASE), (29, 22, F_HAIR_BASE), (29, 23, F_HAIR_BASE),
                      (30, 23, F_HAIR_BASE), (30, 24, F_HAIR_BASE), (30, 25, F_HAIR_OUT), (31, 24, F_HAIR_OUT)]:
        p[bx, by] = c

    # Bang strand 3 (Center right)
    for bx, by, c in [(33, 22, F_HAIR_BASE), (34, 22, F_HAIR_BASE), (34, 23, F_HAIR_BASE),
                      (35, 23, F_HAIR_BASE), (35, 24, F_HAIR_OUT)]:
        p[bx, by] = c

    # Right temple framing
    for y in range(21, 28):
        for x in range(41, 46):
            if x == 45 or y == 27: p[x, y] = F_HAIR_OUT
            elif x == 44: p[x, y] = F_HAIR_SH
            else: p[x, y] = F_HAIR_BASE

    # Hair Clips (Red with Gold Bead)
    p[18, 20] = F_CLIP_RED; p[19, 20] = F_CLIP_GOLD; p[18, 21] = F_CLIP_RED; p[19, 21] = F_CLIP_RED
    p[44, 20] = F_CLIP_GOLD; p[45, 20] = F_CLIP_RED; p[44, 21] = F_CLIP_RED; p[45, 21] = F_CLIP_RED

    return im

def craft_male_head_v3():
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    p = im.load()

    # Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            p[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

    # Face base
    face_rows = {
        22: (24, 40), 23: (23, 41), 24: (22, 42), 25: (21, 43),
        26: (20, 44), 27: (19, 45), 28: (19, 45), 29: (19, 45),
        30: (19, 45), 31: (20, 44), 32: (21, 43), 33: (23, 41),
        34: (25, 39), 35: (27, 37), 36: (30, 34)
    }
    for y, (x0, x1) in face_rows.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: p[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: p[x, y] = SK_SH
            elif y == 23 and 28 <= x <= 36: p[x, y] = SK_HL
            else: p[x, y] = SK_BASE

    # Cheek blush
    for bx, by in [(22, 31), (23, 31)]: p[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31)]: p[bx, by] = BLUSH

    # Confident grin
    p[31, 33] = SK_OUT; p[32, 33] = (168, 72, 72, 255); p[33, 33] = SK_OUT; p[34, 32] = SK_OUT

    # Male Anime Eyes (Crisp & determined)
    p[24, 26] = EYE_LASH; p[25, 25] = EYE_LASH; p[26, 25] = EYE_LASH; p[27, 25] = EYE_LASH; p[28, 26] = EYE_LASH
    for y in range(27, 30):
        p[24, y] = EYE_WHITE; p[28, y] = EYE_WHITE
        p[25, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        p[26, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        p[27, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    p[25, 30] = EYE_LASH; p[26, 30] = EYE_LASH; p[27, 30] = EYE_LASH
    p[25, 26] = EYE_HL

    p[36, 26] = EYE_LASH; p[37, 25] = EYE_LASH; p[38, 25] = EYE_LASH; p[39, 25] = EYE_LASH; p[40, 26] = EYE_LASH
    for y in range(27, 30):
        p[36, y] = EYE_WHITE; p[40, y] = EYE_WHITE
        p[37, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        p[38, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        p[39, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    p[37, 30] = EYE_LASH; p[38, 30] = EYE_LASH; p[39, 30] = EYE_LASH
    p[37, 26] = EYE_HL

    # Strong eyebrows
    for x in range(23, 29): p[x, 24] = M_HAIR_OUT
    for x in range(36, 42): p[x, 24] = M_HAIR_OUT

    # Male Spiky Anime Hair Dome (Natural anime protagonist ruffled hair)
    crown_m = {
        8:  (29, 35),
        9:  (26, 38),
        10: (23, 41),
        11: (20, 44),
        12: (19, 45),
        13: (18, 46),
        14: (18, 46),
        15: (18, 46),
        16: (18, 46),
        17: (19, 45),
        18: (19, 45),
        19: (20, 44),
        20: (21, 43),
        21: (22, 42),
        22: (23, 41)
    }
    for y, (x0, x1) in crown_m.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 8: p[x, y] = M_HAIR_OUT
            elif 11 <= y <= 13 and 25 <= x <= 39:
                # Glossy spiky highlight band
                p[x, y] = M_HAIR_HL if (27 <= x <= 37) else M_HAIR_BASE
            elif x in (x0 + 1, x1 - 1) or y <= 10: p[x, y] = M_HAIR_SH
            else: p[x, y] = M_HAIR_BASE

    # Spiky Clumps & Tufts (Triangular clumps, not floating pixels)
    # Top spikes
    for sx, sy in [(31, 6), (32, 5), (33, 6), (32, 7)]: p[sx, sy] = M_HAIR_OUT
    p[32, 6] = M_HAIR_HL
    for sx, sy in [(28, 7), (27, 8), (28, 8)]: p[sx, sy] = M_HAIR_OUT
    for sx, sy in [(36, 7), (37, 8), (36, 8)]: p[sx, sy] = M_HAIR_OUT

    # Left side spikes
    for sx, sy in [(21, 9), (20, 10), (19, 11), (20, 11)]: p[sx, sy] = M_HAIR_OUT
    for sx, sy in [(17, 13), (16, 14), (17, 15)]: p[sx, sy] = M_HAIR_OUT
    for sx, sy in [(16, 17), (15, 18), (16, 19)]: p[sx, sy] = M_HAIR_OUT

    # Right side spikes
    for sx, sy in [(43, 9), (44, 10), (45, 11), (44, 11)]: p[sx, sy] = M_HAIR_OUT
    for sx, sy in [(47, 13), (48, 14), (47, 15)]: p[sx, sy] = M_HAIR_OUT
    for sx, sy in [(48, 17), (49, 18), (48, 19)]: p[sx, sy] = M_HAIR_OUT

    # Side hair framing ears/cheeks
    for y in range(23, 28):
        p[20, y] = M_HAIR_OUT; p[21, y] = M_HAIR_SH
        p[43, y] = M_HAIR_SH;  p[44, y] = M_HAIR_OUT

    # Male Spiky Bangs
    # Left bang
    p[22, 23] = M_HAIR_BASE; p[23, 23] = M_HAIR_BASE; p[22, 24] = M_HAIR_OUT; p[23, 24] = M_HAIR_OUT
    # Center clumps
    p[27, 23] = M_HAIR_BASE; p[28, 23] = M_HAIR_BASE; p[28, 24] = M_HAIR_OUT
    p[31, 23] = M_HAIR_BASE; p[32, 23] = M_HAIR_BASE; p[32, 24] = M_HAIR_BASE; p[32, 25] = M_HAIR_OUT
    p[36, 23] = M_HAIR_BASE; p[37, 23] = M_HAIR_BASE; p[37, 24] = M_HAIR_OUT
    # Right bang
    p[41, 23] = M_HAIR_BASE; p[42, 23] = M_HAIR_BASE; p[41, 24] = M_HAIR_OUT; p[42, 24] = M_HAIR_OUT

    return im

if __name__ == "__main__":
    import os
    ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"
    fh = craft_female_head_v3()
    mh = craft_male_head_v3()

    comp = Image.new("RGBA", (128, 64), (0, 0, 0, 0))
    comp.paste(fh, (0, 0))
    comp.paste(mh, (64, 0))
    big = comp.resize((128 * 4, 64 * 4), Image.NEAREST)
    out = os.path.join(ARTIFACT_DIR, "preview_v3_heads.png")
    big.save(out)
    print("Saved preview to:", out)
