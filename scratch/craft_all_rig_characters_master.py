# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Chibi Rig Generator
Crafts brand-new, clean, solid pixel-art parts for Female & Male in all 8 directions.
100% vector-precise pixel art. Zero JPEG artifacts. Rounded joint caps.
"""
import os
from PIL import Image

ROOT_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources\Characters"
RIG_DIR  = os.path.join(ROOT_DIR, "Rig")
ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

# ════════════════════════════════════════════════════════════════
# COLOR PALETTES
# ════════════════════════════════════════════════════════════════
SK_HL       = (255, 242, 234, 255)
SK_BASE     = (248, 208, 186, 255)
SK_SH       = (222, 168, 144, 255)
SK_DEEP_SH  = (188, 134, 110, 255)
SK_OUT      = (112, 58, 46, 255)
BLUSH       = (248, 168, 164, 255)

EYE_LASH    = (40, 22, 28, 255)
EYE_PUPIL   = (62, 30, 26, 255)
EYE_IRIS_T  = (118, 56, 42, 255)
EYE_IRIS_B  = (182, 98, 68, 255)
EYE_WHITE   = (255, 252, 252, 255)
EYE_HL      = (255, 255, 255, 255)

M_EYE_IRIS_T = (88, 74, 46, 255)
M_EYE_IRIS_B = (142, 128, 72, 255)

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
TOP_DARK_SH = (124, 40, 28, 255)
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

def canvas():
    return Image.new("RGBA", (64, 64), (0, 0, 0, 0))

META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 9
  spritePivot: {{x: 0.5, y: 0.1}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationMethod: 0
  spriteTessellationDetail: -1
  spriteGeometrySubdivision: -1
  textureType: 8
  textureShape: 1
"""

import hashlib
def get_guid(filepath):
    h = hashlib.md5(filepath.encode("utf-8")).hexdigest()
    return f"{h[:8]}-{h[8:12]}-{h[12:16]}-{h[16:20]}-{h[20:32]}"

def save_sprite(img, file_path):
    os.makedirs(os.path.dirname(file_path), exist_ok=True)
    img.save(file_path)
    meta_path = file_path + ".meta"
    if not os.path.exists(meta_path):
        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(META_TEMPLATE.format(guid=get_guid(file_path)))

# ════════════════════════════════════════════════════════════════
# FEMALE PARTS BUILDER
# ════════════════════════════════════════════════════════════════
def build_female_south():
    # HEAD
    head = canvas()
    hp = head.load()
    hair_back = canvas()
    hbp = hair_back.load()

    # Hair back volume
    for y in range(20, 44):
        if y < 26:   x_out, x_in = 17, 21
        elif y < 34: x_out, x_in = 15, 21
        elif y < 40: x_out, x_in = 16, 22
        else:        x_out, x_in = 18, 21
        for x in range(x_out, x_in + 1):
            if x == x_out or y == 43: hbp[x, y] = F_HAIR_OUT
            elif x == x_out + 1 or y >= 41: hbp[x, y] = F_HAIR_SH
            elif x == x_in: hbp[x, y] = F_HAIR_SH
            else: hbp[x, y] = F_HAIR_BASE

        rx_out = 64 - x_out - 1
        rx_in  = 64 - x_in - 1
        for x in range(rx_in, rx_out + 1):
            if x == rx_out or y == 43: hbp[x, y] = F_HAIR_OUT
            elif x == rx_out - 1 or y >= 41: hbp[x, y] = F_HAIR_SH
            elif x == rx_in: hbp[x, y] = F_HAIR_SH
            else: hbp[x, y] = F_HAIR_BASE

    # Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            hp[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

    # Face base
    face_rows = {
        22: (24, 40), 23: (23, 41), 24: (22, 42), 25: (21, 43),
        26: (20, 44), 27: (19, 45), 28: (19, 45), 29: (19, 45),
        30: (19, 45), 31: (20, 44), 32: (21, 43), 33: (23, 41),
        34: (25, 39), 35: (27, 37), 36: (30, 34)
    }
    for y, (x0, x1) in face_rows.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: hp[x, y] = SK_SH
            elif y == 23 and 28 <= x <= 36: hp[x, y] = SK_HL
            else: hp[x, y] = SK_BASE

    # Blush
    for bx, by in [(21, 31), (22, 31), (22, 30), (23, 31)]: hp[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31), (42, 30), (43, 31)]: hp[bx, by] = BLUSH
    hp[31, 33] = SK_OUT; hp[32, 34] = (176, 80, 80, 255); hp[33, 33] = SK_OUT

    # Eyes
    hp[24, 26] = EYE_LASH; hp[25, 25] = EYE_LASH; hp[26, 25] = EYE_LASH
    hp[27, 25] = EYE_LASH; hp[28, 26] = EYE_LASH; hp[23, 26] = EYE_LASH
    for y in range(27, 30):
        hp[24, y] = EYE_WHITE; hp[28, y] = EYE_WHITE
        hp[25, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        hp[26, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        hp[27, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    hp[24, 30] = (130, 60, 48, 255); hp[25, 30] = EYE_LASH; hp[26, 30] = EYE_LASH
    hp[27, 30] = EYE_LASH; hp[28, 30] = (130, 60, 48, 255)
    hp[25, 26] = EYE_HL; hp[26, 26] = EYE_HL

    hp[36, 26] = EYE_LASH; hp[37, 25] = EYE_LASH; hp[38, 25] = EYE_LASH
    hp[39, 25] = EYE_LASH; hp[40, 26] = EYE_LASH; hp[41, 26] = EYE_LASH
    for y in range(27, 30):
        hp[36, y] = EYE_WHITE; hp[40, y] = EYE_WHITE
        hp[37, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        hp[38, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        hp[39, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
    hp[36, 30] = (130, 60, 48, 255); hp[37, 30] = EYE_LASH; hp[38, 30] = EYE_LASH
    hp[39, 30] = EYE_LASH; hp[40, 30] = (130, 60, 48, 255)
    hp[37, 26] = EYE_HL; hp[38, 26] = EYE_HL

    # Eyebrows
    hp[24, 23] = F_HAIR_SH; hp[25, 23] = F_HAIR_OUT; hp[26, 23] = F_HAIR_OUT; hp[27, 24] = F_HAIR_SH
    hp[37, 24] = F_HAIR_SH; hp[38, 23] = F_HAIR_OUT; hp[39, 23] = F_HAIR_OUT; hp[40, 23] = F_HAIR_SH

    # Hair crown
    crown = {
        9: (29, 35), 10: (26, 38), 11: (24, 40), 12: (22, 42),
        13: (20, 44), 14: (19, 45), 15: (18, 46), 16: (17, 47),
        17: (17, 47), 18: (17, 47), 19: (17, 47), 20: (17, 47),
        21: (17, 47), 22: (17, 47)
    }
    for y, (x0, x1) in crown.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 9: hp[x, y] = F_HAIR_OUT
            elif y in (12, 13, 14) and (24 <= x <= 40):
                if (y == 12 and 27 <= x <= 37) or (y == 13 and 25 <= x <= 39) or (y == 14 and 26 <= x <= 38):
                    hp[x, y] = F_HAIR_HL
                else: hp[x, y] = F_HAIR_BASE
            elif x in (x0 + 1, x1 - 1) or y <= 11: hp[x, y] = F_HAIR_SH
            else: hp[x, y] = F_HAIR_BASE

    # Bangs
    for y in range(21, 28):
        for x in range(18, 23):
            if x == 18 or y == 27: hp[x, y] = F_HAIR_OUT
            elif x == 19: hp[x, y] = F_HAIR_SH
            else: hp[x, y] = F_HAIR_BASE
    for bx, by, c in [(23, 22, F_HAIR_BASE), (24, 22, F_HAIR_BASE), (24, 23, F_HAIR_BASE), (24, 24, F_HAIR_OUT),
                      (25, 23, F_HAIR_BASE), (25, 24, F_HAIR_OUT)]: hp[bx, by] = c
    for bx, by, c in [(28, 22, F_HAIR_BASE), (29, 22, F_HAIR_BASE), (29, 23, F_HAIR_BASE),
                      (30, 23, F_HAIR_BASE), (30, 24, F_HAIR_BASE), (30, 25, F_HAIR_OUT), (31, 24, F_HAIR_OUT)]: hp[bx, by] = c
    for bx, by, c in [(33, 22, F_HAIR_BASE), (34, 22, F_HAIR_BASE), (34, 23, F_HAIR_BASE),
                      (35, 23, F_HAIR_BASE), (35, 24, F_HAIR_OUT)]: hp[bx, by] = c
    for y in range(21, 28):
        for x in range(41, 46):
            if x == 45 or y == 27: hp[x, y] = F_HAIR_OUT
            elif x == 44: hp[x, y] = F_HAIR_SH
            else: hp[x, y] = F_HAIR_BASE

    # Clips
    hp[18, 20] = F_CLIP_RED; hp[19, 20] = F_CLIP_GOLD; hp[18, 21] = F_CLIP_RED; hp[19, 21] = F_CLIP_RED
    hp[44, 20] = F_CLIP_GOLD; hp[45, 20] = F_CLIP_RED; hp[44, 21] = F_CLIP_RED; hp[45, 21] = F_CLIP_RED

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
    composite.paste(hair_back, (0, 0), hair_back)
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "idle_hair": hair_back, "body": composite
    }

def build_female_east():
    head = canvas()
    hp = head.load()

    # Hair Back / Ponytail (flowing left/behind, x=15..28, y=16..43)
    for y in range(16, 44):
        x0 = 15 - (2 if 24 <= y <= 34 else 0)
        x1 = 27
        for x in range(x0, x1):
            if x == x0 or y == 43: hp[x, y] = F_HAIR_OUT
            elif x < x0 + 3: hp[x, y] = F_HAIR_SH
            elif 20 <= x <= 25 and 18 <= y <= 22: hp[x, y] = F_HAIR_HL
            else: hp[x, y] = F_HAIR_BASE

    # Face Profile (x=26..42, y=22..36, nose/mouth facing RIGHT at x=42..43)
    for y in range(22, 37):
        if y < 24:    x0, x1 = 26, 40
        elif y < 28:  x0, x1 = 26, 42
        elif y < 31:  x0, x1 = 26, 43
        elif y < 34:  x0, x1 = 26, 42
        elif y == 34: x0, x1 = 27, 40
        elif y == 35: x0, x1 = 28, 38
        else:         x0, x1 = 29, 36

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x == x0 + 1 or y >= 34: hp[x, y] = SK_SH
            else: hp[x, y] = SK_BASE

    hp[36, 31] = BLUSH; hp[37, 31] = BLUSH; hp[37, 32] = BLUSH

    # Side Anime Eye (facing right)
    for x in range(35, 40): hp[x, 26] = EYE_LASH
    hp[34, 26] = EYE_LASH; hp[40, 26] = EYE_LASH
    for y in range(27, 30):
        hp[35, y] = EYE_LASH
        hp[36, y] = EYE_IRIS_T if y == 27 else EYE_IRIS_B
        hp[37, y] = EYE_PUPIL if y == 28 else EYE_IRIS_T
        hp[38, y] = EYE_IRIS_B
        hp[39, y] = EYE_LASH
    for x in range(36, 39): hp[x, 30] = (120, 56, 44, 255)
    hp[36, 27] = EYE_HL; hp[37, 27] = EYE_HL
    hp[35, 24] = F_HAIR_OUT; hp[36, 24] = F_HAIR_OUT; hp[37, 24] = F_HAIR_SH

    # Hair crown
    for y in range(11, 25):
        if y < 14:    x0, x1 = 22, 36
        elif y < 18:  x0, x1 = 18, 39
        else:         x0, x1 = 16, 40

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 11: hp[x, y] = F_HAIR_OUT
            elif 13 <= y <= 16 and 24 <= x <= 35: hp[x, y] = F_HAIR_HL
            elif x < 20: hp[x, y] = F_HAIR_SH
            elif y >= 22 and 33 <= x <= 39: pass
            else: hp[x, y] = F_HAIR_BASE

    hp[38, 23] = F_HAIR_BASE; hp[39, 23] = F_HAIR_BASE; hp[39, 24] = F_HAIR_OUT
    hp[32, 24] = F_HAIR_BASE; hp[33, 24] = F_HAIR_OUT; hp[32, 25] = F_HAIR_OUT
    hp[30, 20] = F_CLIP_RED; hp[31, 20] = F_CLIP_GOLD; hp[30, 21] = F_CLIP_RED

    for y in range(36, 40):
        for x in range(29, 35):
            hp[x, y] = SK_SH if (x in (29, 34) or y == 39) else SK_BASE

    # BODY (Side view)
    body = canvas()
    bp = body.load()
    for y in range(37, 49):
        if y == 37:    x0, x1 = 28, 35
        elif y == 38:  x0, x1 = 27, 37
        elif y < 44:   x0, x1 = 26, 38
        elif y < 46:   x0, x1 = 26, 37
        else:          x0, x1 = 26, 38

        for x in range(x0, x1 + 1):
            if y == 38 and 33 <= x <= 36: bp[x, y] = COLLAR_W
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x == x0 + 1: bp[x, y] = TOP_DARK_SH
                elif x >= x1 - 2: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                if x >= x1 - 2: bp[x, y] = BELT_BUCKLE
                elif x in (x0, x1): bp[x, y] = SH_OUT
                else: bp[x, y] = BELT_BRN
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x == x0 + 1: bp[x, y] = SH_SH
                elif x >= x1 - 2: bp[x, y] = SH_HL
                else: bp[x, y] = SH_BASE

    # ARMS (East)
    arm_l = canvas() # Far arm (behind)
    alp = arm_l.load()
    for y in range(39, 48):
        for x in range(24, 28):
            alp[x, y] = SK_OUT if (x in (24, 27) or y == 47) else SK_DEEP_SH

    arm_r = canvas() # Near arm (front)
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 33, 37
        if y == 38: x1 = 36
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 47: arp[x, y] = SK_OUT
            elif x == x0 + 1: arp[x, y] = SK_SH
            else: arp[x, y] = SK_BASE
    arp[35, 47] = SK_BASE; arp[36, 47] = SK_SH

    # LEGS (East)
    leg_l = canvas() # Far leg
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_SH
    for x in range(26, 31): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31): llp[x, y] = SK_OUT if x in (27, 30) else SK_DEEP_SH
    for y in range(54, 59):
        for x in range(26, 32):
            if y == 58 or x in (26, 31): llp[x, y] = BOOT_SOLE
            else: llp[x, y] = BOOT_SH

    leg_r = canvas() # Near leg
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(31, 35): lrp[x, y] = SH_BASE
    for x in range(30, 36): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(31, 35):
            if x in (31, 34): lrp[x, y] = SK_OUT
            elif x == 32: lrp[x, y] = SK_BASE
            else: lrp[x, y] = SK_SH
    for y in range(54, 59):
        for x in range(30, 37):
            if y == 58 or x in (30, 36): lrp[x, y] = BOOT_SOLE
            elif x >= 33: lrp[x, y] = BOOT_HL
            else: lrp[x, y] = BOOT_BASE

    composite = canvas()
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(body, (0, 0), body)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(head, (0, 0), head)
    composite.paste(arm_r, (0, 0), arm_r)

    return {
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "body": composite
    }

def build_female_north():
    head = canvas()
    hp = head.load()
    dome = {
        9: (29, 35), 10: (26, 38), 11: (24, 40), 12: (22, 42),
        13: (20, 44), 14: (19, 45), 15: (18, 46), 16: (17, 47),
        17: (17, 47), 18: (17, 47), 19: (17, 47), 20: (16, 48),
        21: (16, 48), 22: (16, 48), 23: (16, 48), 24: (16, 48),
        25: (16, 48), 26: (17, 47), 27: (17, 47), 28: (17, 47),
        29: (18, 46), 30: (18, 46), 31: (19, 45), 32: (20, 44),
        33: (21, 43), 34: (22, 42), 35: (24, 40), 36: (26, 38), 37: (28, 36)
    }
    for y, (x0, x1) in dome.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 9 or y == 37: hp[x, y] = F_HAIR_OUT
            elif 12 <= y <= 15 and 25 <= x <= 39: hp[x, y] = F_HAIR_HL
            elif x in (x0 + 1, x1 - 1) or y >= 32: hp[x, y] = F_HAIR_SH
            else: hp[x, y] = F_HAIR_BASE

    for y in range(18, 35):
        if y % 3 == 0: hp[32, y] = F_HAIR_SH
        if y % 4 == 0: hp[25, y] = F_HAIR_OUT; hp[39, y] = F_HAIR_OUT
    hp[18, 20] = F_CLIP_RED; hp[19, 20] = F_CLIP_RED
    hp[44, 20] = F_CLIP_RED; hp[45, 20] = F_CLIP_RED

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
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SH
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                bp[x, y] = BELT_BRN if x not in (x0, x1) else SH_OUT
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SH_SH
                else: bp[x, y] = SH_BASE

    arm_l = canvas()
    alp = arm_l.load()
    for y in range(38, 48):
        for x in range(21, 25):
            alp[x, y] = SK_OUT if (x == 21 or y == 47) else SK_BASE

    arm_r = canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        for x in range(40, 44):
            arp[x, y] = SK_OUT if (x == 43 or y == 47) else SK_BASE

    leg_l = canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_BASE
    for x in range(26, 32): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31): llp[x, y] = SK_OUT if x in (27, 30) else SK_BASE
    for y in range(54, 59):
        for x in range(27, 32):
            llp[x, y] = BOOT_SOLE if (y == 58 or x in (27, 31)) else BOOT_BASE

    leg_r = canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38): lrp[x, y] = SH_BASE
    for x in range(33, 39): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(34, 38): lrp[x, y] = SK_OUT if x in (34, 37) else SK_BASE
    for y in range(54, 59):
        for x in range(34, 39):
            lrp[x, y] = BOOT_SOLE if (y == 58 or x in (34, 38)) else BOOT_BASE

    composite = canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "body": composite
    }

# ════════════════════════════════════════════════════════════════
# MALE PARTS BUILDER
# ════════════════════════════════════════════════════════════════
def build_male_south():
    # HEAD
    head = canvas()
    hp = head.load()

    # Neck stub (y=36..39)
    for y in range(36, 40):
        for x in range(30, 35):
            hp[x, y] = SK_SH if (x in (30, 34) or y == 39) else SK_BASE

    # Face base
    face_rows = {
        22: (24, 40), 23: (23, 41), 24: (22, 42), 25: (21, 43),
        26: (20, 44), 27: (19, 45), 28: (19, 45), 29: (19, 45),
        30: (19, 45), 31: (20, 44), 32: (21, 43), 33: (23, 41),
        34: (25, 39), 35: (27, 37), 36: (30, 34)
    }
    for y, (x0, x1) in face_rows.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x in (x0 + 1, x1 - 1) or y >= 34: hp[x, y] = SK_SH
            elif y == 23 and 28 <= x <= 36: hp[x, y] = SK_HL
            else: hp[x, y] = SK_BASE

    for bx, by in [(22, 31), (23, 31)]: hp[bx, by] = BLUSH
    for bx, by in [(41, 31), (42, 31)]: hp[bx, by] = BLUSH
    hp[31, 33] = SK_OUT; hp[32, 33] = (168, 72, 72, 255); hp[33, 33] = SK_OUT; hp[34, 32] = SK_OUT

    # Male Anime Eyes
    hp[24, 26] = EYE_LASH; hp[25, 25] = EYE_LASH; hp[26, 25] = EYE_LASH; hp[27, 25] = EYE_LASH; hp[28, 26] = EYE_LASH
    for y in range(27, 30):
        hp[24, y] = EYE_WHITE; hp[28, y] = EYE_WHITE
        hp[25, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        hp[26, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        hp[27, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    hp[25, 30] = EYE_LASH; hp[26, 30] = EYE_LASH; hp[27, 30] = EYE_LASH
    hp[25, 26] = EYE_HL

    hp[36, 26] = EYE_LASH; hp[37, 25] = EYE_LASH; hp[38, 25] = EYE_LASH; hp[39, 25] = EYE_LASH; hp[40, 26] = EYE_LASH
    for y in range(27, 30):
        hp[36, y] = EYE_WHITE; hp[40, y] = EYE_WHITE
        hp[37, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        hp[38, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        hp[39, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
    hp[37, 30] = EYE_LASH; hp[38, 30] = EYE_LASH; hp[39, 30] = EYE_LASH
    hp[37, 26] = EYE_HL

    for x in range(23, 29): hp[x, 24] = M_HAIR_OUT
    for x in range(36, 42): hp[x, 24] = M_HAIR_OUT

    # Hair Dome
    crown_m = {
        10: (28, 36), 11: (25, 39), 12: (22, 42), 13: (20, 44),
        14: (19, 45), 15: (18, 46), 16: (18, 46), 17: (18, 46),
        18: (18, 46), 19: (19, 45), 20: (20, 44), 21: (21, 43), 22: (22, 42)
    }
    for y, (x0, x1) in crown_m.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 10: hp[x, y] = M_HAIR_OUT
            elif 12 <= y <= 14 and 25 <= x <= 39:
                hp[x, y] = M_HAIR_HL if (27 <= x <= 37) else M_HAIR_BASE
            elif x in (x0 + 1, x1 - 1) or y <= 11: hp[x, y] = M_HAIR_SH
            else: hp[x, y] = M_HAIR_BASE

    # Spiky Tufts
    for sx, sy in [(31, 7), (32, 6), (33, 7), (32, 8)]: hp[sx, sy] = M_HAIR_OUT
    hp[32, 7] = M_HAIR_HL
    for sx, sy in [(28, 8), (27, 9), (28, 9)]: hp[sx, sy] = M_HAIR_OUT
    for sx, sy in [(36, 8), (37, 9), (36, 9)]: hp[sx, sy] = M_HAIR_OUT

    for sx, sy in [(21, 10), (20, 11), (19, 12), (20, 12)]: hp[sx, sy] = M_HAIR_OUT
    for sx, sy in [(18, 14), (17, 15), (18, 16)]: hp[sx, sy] = M_HAIR_OUT
    for sx, sy in [(43, 10), (44, 11), (45, 12), (44, 12)]: hp[sx, sy] = M_HAIR_OUT
    for sx, sy in [(46, 14), (47, 15), (46, 16)]: hp[sx, sy] = M_HAIR_OUT

    # Side hair
    for y in range(23, 28):
        hp[20, y] = M_HAIR_OUT; hp[21, y] = M_HAIR_SH
        hp[43, y] = M_HAIR_SH;  hp[44, y] = M_HAIR_OUT

    # Bangs
    hp[22, 23] = M_HAIR_BASE; hp[23, 23] = M_HAIR_BASE; hp[22, 24] = M_HAIR_OUT; hp[23, 24] = M_HAIR_OUT
    hp[27, 23] = M_HAIR_BASE; hp[28, 23] = M_HAIR_BASE; hp[28, 24] = M_HAIR_OUT
    hp[31, 23] = M_HAIR_BASE; hp[32, 23] = M_HAIR_BASE; hp[32, 24] = M_HAIR_BASE; hp[32, 25] = M_HAIR_OUT
    hp[36, 23] = M_HAIR_BASE; hp[37, 23] = M_HAIR_BASE; hp[37, 24] = M_HAIR_OUT
    hp[41, 23] = M_HAIR_BASE; hp[42, 23] = M_HAIR_BASE; hp[41, 24] = M_HAIR_OUT; hp[42, 24] = M_HAIR_OUT

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
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "body": composite
    }

def build_male_east():
    head = canvas()
    hp = head.load()

    # Hair profile back
    for y in range(15, 30):
        for x in range(17, 30):
            if x == 17 or y == 15: hp[x, y] = M_HAIR_OUT
            elif x < 21: hp[x, y] = M_HAIR_SH
            elif 23 <= x <= 28 and 16 <= y <= 20: hp[x, y] = M_HAIR_HL
            else: hp[x, y] = M_HAIR_BASE

    for sx, sy in [(15, 18), (14, 20), (15, 22), (28, 8), (29, 7), (30, 8)]:
        hp[sx, sy] = M_HAIR_OUT

    # Face profile facing RIGHT
    for y in range(22, 37):
        if y < 24:    x0, x1 = 26, 40
        elif y < 28:  x0, x1 = 26, 42
        elif y < 31:  x0, x1 = 26, 43 # nose
        elif y < 34:  x0, x1 = 26, 42
        elif y == 34: x0, x1 = 27, 40
        elif y == 35: x0, x1 = 28, 38
        else:         x0, x1 = 29, 36

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 36: hp[x, y] = SK_OUT
            elif x == x0 + 1 or y >= 34: hp[x, y] = SK_SH
            else: hp[x, y] = SK_BASE

    hp[36, 31] = BLUSH; hp[37, 31] = BLUSH

    # Side eye
    for x in range(35, 40): hp[x, 26] = EYE_LASH
    hp[34, 26] = EYE_LASH; hp[40, 26] = EYE_LASH
    for y in range(27, 30):
        hp[35, y] = EYE_LASH
        hp[36, y] = M_EYE_IRIS_T if y == 27 else M_EYE_IRIS_B
        hp[37, y] = EYE_PUPIL if y == 28 else M_EYE_IRIS_T
        hp[38, y] = M_EYE_IRIS_B
        hp[39, y] = EYE_LASH
    for x in range(36, 39): hp[x, 30] = (100, 70, 40, 255)
    hp[36, 27] = EYE_HL
    for x in range(34, 40): hp[x, 24] = M_HAIR_OUT

    # Hair crown
    for y in range(11, 25):
        if y < 14:    x0, x1 = 22, 36
        elif y < 18:  x0, x1 = 18, 39
        else:         x0, x1 = 17, 40

        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 11: hp[x, y] = M_HAIR_OUT
            elif 13 <= y <= 16 and 24 <= x <= 35: hp[x, y] = M_HAIR_HL
            elif x < 20: hp[x, y] = M_HAIR_SH
            elif y >= 22 and 33 <= x <= 39: pass
            else: hp[x, y] = M_HAIR_BASE

    hp[37, 23] = M_HAIR_BASE; hp[38, 23] = M_HAIR_BASE; hp[38, 24] = M_HAIR_OUT
    hp[32, 24] = M_HAIR_BASE; hp[33, 24] = M_HAIR_OUT

    for y in range(36, 40):
        for x in range(29, 35):
            hp[x, y] = SK_SH if (x in (29, 34) or y == 39) else SK_BASE

    # BODY
    body = canvas()
    bp = body.load()
    for y in range(37, 49):
        if y == 37:    x0, x1 = 28, 35
        elif y == 38:  x0, x1 = 27, 37
        elif y < 44:   x0, x1 = 26, 38
        elif y < 46:   x0, x1 = 26, 37
        else:          x0, x1 = 26, 38

        for x in range(x0, x1 + 1):
            if y == 38 and 33 <= x <= 36: bp[x, y] = COLLAR_W
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x == x0 + 1: bp[x, y] = TOP_DARK_SH
                elif x >= x1 - 2: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                if x >= x1 - 2: bp[x, y] = BELT_BUCKLE
                elif x in (x0, x1): bp[x, y] = SH_OUT
                else: bp[x, y] = BELT_BRN
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x == x0 + 1: bp[x, y] = SH_SH
                elif x >= x1 - 2: bp[x, y] = SH_HL
                else: bp[x, y] = SH_BASE

    arm_l = canvas()
    alp = arm_l.load()
    for y in range(39, 48):
        for x in range(24, 28):
            alp[x, y] = SK_OUT if (x in (24, 27) or y == 47) else SK_DEEP_SH

    arm_r = canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        x0, x1 = 33, 37
        if y == 38: x1 = 36
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 47: arp[x, y] = SK_OUT
            elif x == x0 + 1: arp[x, y] = SK_SH
            else: arp[x, y] = SK_BASE
    arp[35, 47] = SK_BASE; arp[36, 47] = SK_SH

    leg_l = canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_SH
    for x in range(26, 31): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31): llp[x, y] = SK_OUT if x in (27, 30) else SK_DEEP_SH
    for y in range(54, 59):
        for x in range(26, 32):
            if y == 58 or x in (26, 31): llp[x, y] = BOOT_SOLE
            else: llp[x, y] = BOOT_SH

    leg_r = canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(31, 35): lrp[x, y] = SH_BASE
    for x in range(30, 36): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(31, 35):
            if x in (31, 34): lrp[x, y] = SK_OUT
            elif x == 32: lrp[x, y] = SK_BASE
            else: lrp[x, y] = SK_SH
    for y in range(54, 59):
        for x in range(30, 37):
            if y == 58 or x in (30, 36): lrp[x, y] = BOOT_SOLE
            elif x >= 33: lrp[x, y] = BOOT_HL
            else: lrp[x, y] = BOOT_BASE

    composite = canvas()
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(body, (0, 0), body)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(head, (0, 0), head)
    composite.paste(arm_r, (0, 0), arm_r)

    return {
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "body": composite
    }

def build_male_north():
    head = canvas()
    hp = head.load()
    dome = {
        10: (28, 36), 11: (25, 39), 12: (22, 42), 13: (20, 44),
        14: (19, 45), 15: (18, 46), 16: (18, 46), 17: (18, 46),
        18: (18, 46), 19: (19, 45), 20: (20, 44), 21: (21, 43),
        22: (22, 42), 23: (24, 40), 24: (26, 38)
    }
    for y, (x0, x1) in dome.items():
        for x in range(x0, x1 + 1):
            if x in (x0, x1) or y == 10 or y == 24: hp[x, y] = M_HAIR_OUT
            elif 12 <= y <= 15 and 25 <= x <= 39: hp[x, y] = M_HAIR_HL
            elif x in (x0 + 1, x1 - 1) or y >= 22: hp[x, y] = M_HAIR_SH
            else: hp[x, y] = M_HAIR_BASE

    for sx, sy in [(31, 7), (32, 6), (33, 7), (28, 8), (36, 8), (20, 11), (44, 11), (17, 15), (47, 15)]:
        hp[sx, sy] = M_HAIR_OUT

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
            elif y <= 43:
                if x in (x0, x1): bp[x, y] = TOP_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = TOP_SH
                elif 30 <= x <= 34: bp[x, y] = TOP_HL
                else: bp[x, y] = TOP_BASE
            elif y in (44, 45):
                bp[x, y] = BELT_BRN if x not in (x0, x1) else SH_OUT
            else:
                if x in (x0, x1) or y == 48: bp[x, y] = SH_OUT
                elif x in (x0 + 1, x1 - 1): bp[x, y] = SH_SH
                else: bp[x, y] = SH_BASE

    arm_l = canvas()
    alp = arm_l.load()
    for y in range(38, 48):
        for x in range(21, 25):
            alp[x, y] = SK_OUT if (x == 21 or y == 47) else SK_BASE

    arm_r = canvas()
    arp = arm_r.load()
    for y in range(38, 48):
        for x in range(40, 44):
            arp[x, y] = SK_OUT if (x == 43 or y == 47) else SK_BASE

    leg_l = canvas()
    llp = leg_l.load()
    for y in range(47, 50):
        for x in range(27, 31): llp[x, y] = SH_BASE
    for x in range(26, 32): llp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(27, 31): llp[x, y] = SK_OUT if x in (27, 30) else SK_BASE
    for y in range(54, 59):
        for x in range(27, 32):
            llp[x, y] = BOOT_SOLE if (y == 58 or x in (27, 31)) else BOOT_BASE

    leg_r = canvas()
    lrp = leg_r.load()
    for y in range(47, 50):
        for x in range(34, 38): lrp[x, y] = SH_BASE
    for x in range(33, 39): lrp[x, 49] = SH_OUT
    for y in range(50, 54):
        for x in range(34, 38): lrp[x, y] = SK_OUT if x in (34, 37) else SK_BASE
    for y in range(54, 59):
        for x in range(34, 39):
            lrp[x, y] = BOOT_SOLE if (y == 58 or x in (34, 38)) else BOOT_BASE

    composite = canvas()
    composite.paste(leg_l, (0, 0), leg_l)
    composite.paste(leg_r, (0, 0), leg_r)
    composite.paste(body, (0, 0), body)
    composite.paste(arm_l, (0, 0), arm_l)
    composite.paste(arm_r, (0, 0), arm_r)
    composite.paste(head, (0, 0), head)

    return {
        "idle_head": head, "idle_body": body,
        "idle_arm_left": arm_l, "idle_arm_right": arm_r,
        "idle_leg_left": leg_l, "idle_leg_right": leg_r,
        "body": composite
    }

def flip_dict(d):
    """Lật đối xứng trục ngang X cho các hướng Tây (West)"""
    flipped = {}
    for k, v in d.items():
        if v is not None:
            flipped[k] = v.transpose(Image.FLIP_LEFT_RIGHT)
        else:
            flipped[k] = None
    # Lưu ý: Khi lật ngang, Arm Left và Arm Right đổi chỗ để duy trì near/far logic
    if "idle_arm_left" in flipped and "idle_arm_right" in flipped:
        flipped["idle_arm_left"], flipped["idle_arm_right"] = flipped["idle_arm_right"], flipped["idle_arm_left"]
    if "idle_leg_left" in flipped and "idle_leg_right" in flipped:
        flipped["idle_leg_left"], flipped["idle_leg_right"] = flipped["idle_leg_right"], flipped["idle_leg_left"]
    return flipped

# ════════════════════════════════════════════════════════════════
# MASTER RUNNER
# ════════════════════════════════════════════════════════════════
def generate_all():
    print("[1/4] Generating Female Base Sets...")
    f_south = build_female_south()
    f_east  = build_female_east()
    f_north = build_female_north()
    # Diagonal directions:
    # south_east and north_east use east base with slight adjustments or east directly
    f_se    = f_east
    f_ne    = f_east

    f_dirs = {
        "south": f_south,
        "east": f_east,
        "south_east": f_se,
        "north_east": f_ne,
        "north": f_north,
        "west": flip_dict(f_east),
        "south_west": flip_dict(f_se),
        "north_west": flip_dict(f_ne)
    }

    print("[2/4] Saving Female Rig Sprites...")
    for dname, parts in f_dirs.items():
        dir_path = os.path.join(RIG_DIR, "female", dname)
        for pname, pimg in parts.items():
            if pimg is not None:
                save_sprite(pimg, os.path.join(dir_path, f"{pname}.png"))

    # Avatar & FullBody for Female
    save_sprite(f_south["body"], os.path.join(ROOT_DIR, "fullbody_female.png"))
    # Avatar is cropped 36x36 around head
    avatar_f = f_south["idle_head"].crop((14, 8, 50, 44)).resize((64, 64), Image.NEAREST)
    save_sprite(avatar_f, os.path.join(ROOT_DIR, "avatar_female.png"))

    print("[3/4] Generating Male Base Sets...")
    m_south = build_male_south()
    m_east  = build_male_east()
    m_north = build_male_north()
    m_se    = m_east
    m_ne    = m_east

    m_dirs = {
        "south": m_south,
        "east": m_east,
        "south_east": m_se,
        "north_east": m_ne,
        "north": m_north,
        "west": flip_dict(m_east),
        "south_west": flip_dict(m_se),
        "north_west": flip_dict(m_ne)
    }

    print("[4/4] Saving Male Rig Sprites...")
    for dname, parts in m_dirs.items():
        dir_path = os.path.join(RIG_DIR, "male", dname)
        for pname, pimg in parts.items():
            if pimg is not None:
                save_sprite(pimg, os.path.join(dir_path, f"{pname}.png"))

    save_sprite(m_south["body"], os.path.join(ROOT_DIR, "fullbody_male.png"))
    avatar_m = m_south["idle_head"].crop((14, 6, 50, 42)).resize((64, 64), Image.NEAREST)
    save_sprite(avatar_m, os.path.join(ROOT_DIR, "avatar_male.png"))

    # Save a master 8-direction overview sheet to artifact directory
    sheet = Image.new("RGBA", (64 * 8, 64 * 2), (0, 0, 0, 0))
    d_order = ["south", "south_east", "east", "north_east", "north", "north_west", "west", "south_west"]
    for i, d in enumerate(d_order):
        sheet.paste(f_dirs[d]["body"], (i * 64, 0))
        sheet.paste(m_dirs[d]["body"], (i * 64, 64))

    big_sheet = sheet.resize((64 * 8 * 3, 64 * 2 * 3), Image.NEAREST)
    big_sheet.save(os.path.join(ARTIFACT_DIR, "master_8dir_rig_overview.png"))
    print("FINISHED ALL! Overview saved to master_8dir_rig_overview.png")

if __name__ == "__main__":
    generate_all()
