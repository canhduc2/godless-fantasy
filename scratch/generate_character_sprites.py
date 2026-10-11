# -*- coding: utf-8 -*-
"""
Godless Fantasy - Soul Knight Style 8-Direction Chibi Character & Layered Paperdoll Generator
Studio: SquiRRels Studio (c) 2026

Generates:
1. Base Bodies (Male & Female) in Soul Knight Chibi proportions
   - 8 DIRECTIONS (south, south_east, east, north_east, north, north_west, west, south_west)
   - 4 FRAMES per direction (Walk cycle: Contact -> Step L -> Pass -> Step R)
   - Clean, solid sneakers (NO naked bare skin gaps, NO floating black pixels!)
2. Paperdoll Layers: Pants, Armor, Helmet, Scarf (8 directions x 4 frames)
3. Full Legendary Skins:
   - Skin Soul Knight Paladin (Rounded dome helmet, gold visor, flowing orange scarf, black boots)
   - Skin Void Reaper (Hooded cowl, crown, glowing red cross eye, detached hands, red tentacles)
4. Portait/Avatar Textures:
   - avatar_male.png, avatar_female.png, avatar_soul_knight.png, avatar_void_reaper.png
5. Elemental Slash VFX (7 elements x 3 frames)
6. Unity .meta files with Point filtering (filterMode: 0) and PPU: 32
"""

import os
import sys
import math
from PIL import Image, ImageDraw

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

ROOT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
OUTPUT_DIR = os.path.join(ROOT_DIR, "client", "GodlessFantasyClient", "Assets", "Resources", "Characters")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# FULL 8 DIRECTIONS
DIRECTIONS_8 = [
    "south",        # 0: Chính diện nhìn xuống
    "south_east",   # 1: Chéo dưới phải
    "east",         # 2: Nhìn sang phải
    "north_east",   # 3: Chéo trên phải
    "north",        # 4: Sau lưng nhìn lên
    "north_west",   # 5: Chéo trên trái
    "west",         # 6: Nhìn sang trái
    "south_west"    # 7: Chéo dưới trái
]

CANVAS_SIZE = 32

def make_meta_file(file_path):
    import uuid
    guid = uuid.uuid5(uuid.NAMESPACE_DNS, os.path.basename(file_path)).hex
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
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
  spritePivot: {{x: 0.5, y: 0.2}}
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
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
"""
    with open(file_path + ".meta", "w", encoding="utf-8") as f:
        f.write(meta_content)

def save_sprite(img, name):
    path = os.path.join(OUTPUT_DIR, f"{name}.png")
    img.save(path)
    make_meta_file(path)
    return path

# ==================== PALETTE ====================
OUTLINE = (22, 24, 32, 255)
SKIN_LIGHT = (254, 218, 178, 255)
SKIN_MID = (242, 188, 140, 255)
SKIN_SHADOW = (210, 148, 105, 255)

EYE_BLUE = (35, 125, 225, 255)
EYE_DARK = (15, 35, 70, 255)
EYE_SHINE = (255, 255, 255, 255)

HAIR_MALE = (60, 40, 26, 255)
HAIR_MALE_LIGHT = (95, 65, 45, 255)
HAIR_FEMALE = (165, 82, 42, 255)
HAIR_FEMALE_LIGHT = (215, 115, 60, 255)

SHIRT_WHITE = (248, 250, 252, 255)
SHIRT_SHADOW = (200, 210, 225, 255)

SHORTS_MALE = (50, 60, 75, 255)
SHORTS_MALE_LIGHT = (80, 95, 115, 255)
SHORTS_FEMALE = (219, 39, 119, 255) # Pink sports shorts
SHORTS_FEMALE_LIGHT = (244, 114, 182, 255)

# Cute sneakers / shoes (Solid, clean, NO bare legs gap!)
SHOE_DARK = (30, 36, 48, 255)
SHOE_LIGHT = (65, 78, 98, 255)
SHOE_SOLE = (240, 245, 255, 255)

SCARF_ORANGE = (249, 115, 22, 255)
SCARF_LIGHT = (251, 146, 60, 255)
SCARF_SHADOW = (194, 65, 12, 255)

KNIGHT_GREY = (148, 163, 184, 255)
KNIGHT_LIGHT = (203, 213, 225, 255)
KNIGHT_SHADOW = (71, 85, 105, 255)
KNIGHT_GOLD = (245, 158, 11, 255)
KNIGHT_GOLD_LIGHT = (252, 211, 77, 255)

# ==================== 1. BASE BODY BUILDER ====================
def draw_base_body_core(gender="male", direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # Walk cycle bobbing:
    # Frame 0: Contact (bob=0)
    # Frame 1: Step L (dip=1)
    # Frame 2: Passing (rise=-1)
    # Frame 3: Step R (dip=1)
    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise

    cx = 16
    leg_y = 25 + bob

    # Foot offsets for natural walking
    leg_l_off = -1 if frame == 1 else (1 if frame == 3 else 0)
    leg_r_off = 1 if frame == 1 else (-1 if frame == 3 else 0)

    # 1. DRAW SNEAKERS / SHOES (SOLID, NO BARE SKIN HOLES!)
    sole_col = (245, 248, 255, 255) if gender == "male" else (255, 230, 240, 255)

    if direction in ("south", "north"):
        # Left sneaker
        d.rectangle([cx - 5, leg_y + leg_l_off, cx - 1, leg_y + 3 + leg_l_off], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx - 4, leg_y + leg_l_off, cx - 2, leg_y + leg_l_off], fill=SHOE_LIGHT)
        d.line([cx - 5, leg_y + 3 + leg_l_off, cx - 1, leg_y + 3 + leg_l_off], fill=sole_col)
        # Right sneaker
        d.rectangle([cx + 1, leg_y + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx + 2, leg_y + leg_r_off, cx + 4, leg_y + leg_r_off], fill=SHOE_LIGHT)
        d.line([cx + 1, leg_y + 3 + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=sole_col)
    elif direction in ("south_east", "north_east"):
        # 3/4 view: angled sneakers
        d.rectangle([cx - 4, leg_y + leg_l_off, cx, leg_y + 3 + leg_l_off], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx - 4, leg_y + 3 + leg_l_off, cx, leg_y + 3 + leg_l_off], fill=sole_col)
        d.rectangle([cx + 1, leg_y + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx + 1, leg_y + 3 + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=sole_col)
    elif direction == "east":
        # Side view: overlapping natural walking strides
        d.rectangle([cx - 3 + leg_l_off, leg_y, cx + 2 + leg_l_off, leg_y + 3], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx - 3 + leg_l_off, leg_y + 3, cx + 2 + leg_l_off, leg_y + 3], fill=sole_col)
        d.rectangle([cx - 1 + leg_r_off, leg_y, cx + 4 + leg_r_off, leg_y + 3], fill=SHOE_DARK, outline=OUTLINE)
        d.line([cx - 1 + leg_r_off, leg_y + 3, cx + 4 + leg_r_off, leg_y + 3], fill=sole_col)

    # 2. DRAW TORSO (TANK TOP / CROPTOP & SHORTS)
    torso_y = 18 + bob
    shorts_col = SHORTS_FEMALE if gender == "female" else SHORTS_MALE
    shorts_light = SHORTS_FEMALE_LIGHT if gender == "female" else SHORTS_MALE_LIGHT

    if direction == "south":
        # White Tank Top / Croptop
        d.rectangle([cx - 4, torso_y, cx + 4, torso_y + 4], fill=SHIRT_WHITE, outline=OUTLINE)
        d.line([cx - 3, torso_y + 1, cx + 3, torso_y + 1], fill=(255, 255, 255, 255))
        d.line([cx - 4, torso_y + 4, cx + 4, torso_y + 4], fill=SHIRT_SHADOW)
        if gender == "female":
            # Croptop shows cute midriff
            d.line([cx - 3, torso_y + 4, cx + 3, torso_y + 4], fill=SKIN_LIGHT)
        # Shorts (Seamlessly meets shoes!)
        d.rectangle([cx - 5, torso_y + 4, cx + 5, torso_y + 6], fill=shorts_col, outline=OUTLINE)
        d.line([cx - 4, torso_y + 4, cx + 4, torso_y + 4], fill=shorts_light)
        # Arms
        d.rectangle([cx - 6, torso_y, cx - 5, torso_y + 4], fill=SKIN_MID, outline=OUTLINE)
        d.rectangle([cx + 5, torso_y, cx + 6, torso_y + 4], fill=SKIN_MID, outline=OUTLINE)
    elif direction in ("south_east", "north_east"):
        d.rectangle([cx - 4, torso_y, cx + 4, torso_y + 4], fill=SHIRT_WHITE, outline=OUTLINE)
        d.line([cx - 3, torso_y + 4, cx + 3, torso_y + 4], fill=SHIRT_SHADOW)
        d.rectangle([cx - 4, torso_y + 4, cx + 4, torso_y + 6], fill=shorts_col, outline=OUTLINE)
        d.rectangle([cx + 3, torso_y + 1, cx + 5, torso_y + 4], fill=SKIN_MID, outline=OUTLINE)
    elif direction == "east":
        d.rectangle([cx - 3, torso_y, cx + 3, torso_y + 4], fill=SHIRT_WHITE, outline=OUTLINE)
        d.rectangle([cx - 3, torso_y + 4, cx + 3, torso_y + 6], fill=shorts_col, outline=OUTLINE)
        d.rectangle([cx - 1, torso_y + 1, cx + 1, torso_y + 5], fill=SKIN_MID, outline=OUTLINE)
    elif direction == "north":
        d.rectangle([cx - 4, torso_y, cx + 4, torso_y + 4], fill=SHIRT_WHITE, outline=OUTLINE)
        d.rectangle([cx - 5, torso_y + 4, cx + 5, torso_y + 6], fill=shorts_col, outline=OUTLINE)
        d.rectangle([cx - 6, torso_y, cx - 5, torso_y + 4], fill=SKIN_SHADOW, outline=OUTLINE)
        d.rectangle([cx + 5, torso_y, cx + 6, torso_y + 4], fill=SKIN_SHADOW, outline=OUTLINE)

    # 3. DRAW HEAD & FACE (CUTE ANIME CHIBI)
    head_y = 6 + bob
    # Rounded head
    d.rectangle([cx - 6, head_y, cx + 6, head_y + 11], fill=SKIN_LIGHT, outline=OUTLINE)
    d.line([cx - 5, head_y + 10, cx + 5, head_y + 10], fill=SKIN_MID)

    hair_col = HAIR_FEMALE if gender == "female" else HAIR_MALE
    hair_light = HAIR_FEMALE_LIGHT if gender == "female" else HAIR_MALE_LIGHT

    if direction == "south":
        # Anime Eyes
        d.rectangle([cx - 4, head_y + 5, cx - 2, head_y + 8], fill=EYE_BLUE, outline=OUTLINE)
        d.point((cx - 4, head_y + 5), fill=EYE_SHINE)
        d.point((cx - 3, head_y + 7), fill=EYE_DARK)
        d.rectangle([cx + 2, head_y + 5, cx + 4, head_y + 8], fill=EYE_BLUE, outline=OUTLINE)
        d.point((cx + 2, head_y + 5), fill=EYE_SHINE)
        d.point((cx + 3, head_y + 7), fill=EYE_DARK)

        # Mouth / smile
        d.line([cx - 1, head_y + 9, cx + 1, head_y + 9], fill=(220, 100, 90, 255))

        if gender == "female":
            # Soft blush
            d.line([cx - 5, head_y + 8, cx - 4, head_y + 8], fill=(255, 160, 175, 255))
            d.line([cx + 4, head_y + 8, cx + 5, head_y + 8], fill=(255, 160, 175, 255))

        # Hair bangs
        d.rectangle([cx - 7, head_y - 1, cx + 7, head_y + 4], fill=hair_col, outline=OUTLINE)
        d.line([cx - 6, head_y, cx + 6, head_y], fill=hair_light)
        d.polygon([(cx - 3, head_y + 4), (cx - 1, head_y + 4), (cx - 2, head_y + 6)], fill=hair_col)
        d.polygon([(cx + 1, head_y + 4), (cx + 3, head_y + 4), (cx + 2, head_y + 6)], fill=hair_col)
        if gender == "female":
            d.rectangle([cx - 8, head_y + 2, cx - 7, head_y + 10], fill=hair_col, outline=OUTLINE)
            d.rectangle([cx + 7, head_y + 2, cx + 8, head_y + 10], fill=hair_col, outline=OUTLINE)
            d.rectangle([cx + 6, head_y - 2, cx + 9, head_y + 3], fill=hair_col, outline=OUTLINE) # ponytail tip
    elif direction == "south_east":
        d.rectangle([cx - 1, head_y + 5, cx + 1, head_y + 8], fill=EYE_BLUE, outline=OUTLINE)
        d.point((cx - 1, head_y + 5), fill=EYE_SHINE)
        d.rectangle([cx + 3, head_y + 5, cx + 5, head_y + 8], fill=EYE_BLUE, outline=OUTLINE)
        d.point((cx + 3, head_y + 5), fill=EYE_SHINE)
        d.rectangle([cx - 7, head_y - 1, cx + 7, head_y + 4], fill=hair_col, outline=OUTLINE)
        d.line([cx - 6, head_y, cx + 5, head_y], fill=hair_light)
        if gender == "female":
            d.rectangle([cx - 8, head_y + 1, cx - 6, head_y + 7], fill=hair_col, outline=OUTLINE)
    elif direction == "east":
        d.rectangle([cx + 2, head_y + 5, cx + 4, head_y + 8], fill=EYE_BLUE, outline=OUTLINE)
        d.point((cx + 2, head_y + 5), fill=EYE_SHINE)
        d.rectangle([cx - 7, head_y - 1, cx + 4, head_y + 4], fill=hair_col, outline=OUTLINE)
        d.rectangle([cx - 7, head_y + 3, cx - 4, head_y + 9], fill=hair_col, outline=OUTLINE)
        if gender == "female":
            pony_flutter = -1 if frame in (1, 3) else 0
            d.rectangle([cx - 10, head_y + 2 + pony_flutter, cx - 7, head_y + 7 + pony_flutter], fill=hair_col, outline=OUTLINE)
    elif direction == "north_east":
        d.rectangle([cx - 7, head_y - 1, cx + 7, head_y + 9], fill=hair_col, outline=OUTLINE)
        d.line([cx - 5, head_y + 1, cx + 5, head_y + 1], fill=hair_light)
        if gender == "female":
            d.rectangle([cx + 4, head_y + 3, cx + 8, head_y + 9], fill=hair_col, outline=OUTLINE)
    elif direction == "north":
        d.rectangle([cx - 7, head_y - 1, cx + 7, head_y + 10], fill=hair_col, outline=OUTLINE)
        d.line([cx - 5, head_y + 1, cx + 5, head_y + 1], fill=hair_light)
        if gender == "female":
            d.rectangle([cx - 2, head_y - 3, cx + 2, head_y], fill=(230, 80, 80, 255))
            d.rectangle([cx - 3, head_y - 6, cx + 3, head_y - 3], fill=hair_col, outline=OUTLINE)

    return img

def get_8dir_image(draw_fn, dir_name, frame):
    """
    Returns image for any of the 8 directions.
    If dir_name is a West direction, flip the corresponding East direction horizontally!
    """
    if dir_name == "west":
        base = draw_fn("east", frame)
        return base.transpose(Image.FLIP_LEFT_RIGHT)
    elif dir_name == "north_west":
        base = draw_fn("north_east", frame)
        return base.transpose(Image.FLIP_LEFT_RIGHT)
    elif dir_name == "south_west":
        base = draw_fn("south_east", frame)
        return base.transpose(Image.FLIP_LEFT_RIGHT)
    else:
        return draw_fn(dir_name, frame)

# ==================== 2. SOUL KNIGHT PALADIN SKIN (ROUNDED DOME HELMET) ====================
def draw_soul_knight_paladin_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise

    cx = 16
    leg_y = 25 + bob

    leg_l_off = -1 if frame == 1 else (1 if frame == 3 else 0)
    leg_r_off = 1 if frame == 1 else (-1 if frame == 3 else 0)

    # 1. Solid Dark Boots (Chunky Soul Knight stubby feet, clean outline, NO naked flesh!)
    boot_col = (28, 34, 46, 255)
    boot_hl = (55, 68, 88, 255)
    if direction in ("south", "north"):
        d.rectangle([cx - 5, leg_y + leg_l_off, cx - 1, leg_y + 3 + leg_l_off], fill=boot_col, outline=OUTLINE)
        d.line([cx - 4, leg_y + leg_l_off, cx - 2, leg_y + leg_l_off], fill=boot_hl)
        d.rectangle([cx + 1, leg_y + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=boot_col, outline=OUTLINE)
        d.line([cx + 2, leg_y + leg_r_off, cx + 4, leg_y + leg_r_off], fill=boot_hl)
    elif direction in ("south_east", "north_east"):
        d.rectangle([cx - 4, leg_y + leg_l_off, cx, leg_y + 3 + leg_l_off], fill=boot_col, outline=OUTLINE)
        d.rectangle([cx + 1, leg_y + leg_r_off, cx + 5, leg_y + 3 + leg_r_off], fill=boot_col, outline=OUTLINE)
    elif direction == "east":
        d.rectangle([cx - 3 + leg_l_off, leg_y, cx + 2 + leg_l_off, leg_y + 3], fill=boot_col, outline=OUTLINE)
        d.rectangle([cx - 1 + leg_r_off, leg_y, cx + 4 + leg_r_off, leg_y + 3], fill=boot_col, outline=OUTLINE)

    # 2. Armored Torso
    torso_y = 18 + bob
    d.rectangle([cx - 5, torso_y, cx + 5, torso_y + 6], fill=KNIGHT_GREY, outline=OUTLINE)
    d.line([cx - 4, torso_y + 1, cx + 4, torso_y + 1], fill=KNIGHT_LIGHT)
    d.line([cx - 4, torso_y + 6, cx + 4, torso_y + 6], fill=KNIGHT_SHADOW)
    # Gold chest plate emblem
    d.rectangle([cx - 2, torso_y + 2, cx + 2, torso_y + 4], fill=KNIGHT_GOLD, outline=OUTLINE)

    # 3. Flowing Orange Scarf
    scarf_y = 16 + bob
    scarf_wave = -1 if frame in (1, 3) else 0

    if direction == "south":
        d.rectangle([cx - 6, scarf_y, cx + 6, scarf_y + 2], fill=SCARF_ORANGE, outline=OUTLINE)
        d.line([cx - 5, scarf_y, cx + 5, scarf_y], fill=SCARF_LIGHT)
        d.polygon([(cx - 6, scarf_y + 2), (cx - 9, scarf_y + 5 + scarf_wave), (cx - 6, scarf_y + 7 + scarf_wave)], fill=SCARF_ORANGE, outline=OUTLINE)
    elif direction in ("south_east", "east"):
        d.rectangle([cx - 5, scarf_y, cx + 5, scarf_y + 2], fill=SCARF_ORANGE, outline=OUTLINE)
        d.rectangle([cx - 11, scarf_y + 1 + scarf_wave, cx - 5, scarf_y + 5 + scarf_wave], fill=SCARF_ORANGE, outline=OUTLINE)
        d.line([cx - 10, scarf_y + 1 + scarf_wave, cx - 6, scarf_y + 1 + scarf_wave], fill=SCARF_LIGHT)
    elif direction in ("north_east", "north"):
        d.rectangle([cx - 6, scarf_y, cx + 6, scarf_y + 3], fill=SCARF_ORANGE, outline=OUTLINE)
        d.rectangle([cx - 3, scarf_y + 3, cx + 3, scarf_y + 7 + scarf_wave], fill=SCARF_SHADOW, outline=OUTLINE)

    # 4. Rounded Dome Helmet (Not boxy! Chamfered top corners!)
    head_y = 5 + bob
    # Main dome body
    d.rectangle([cx - 7, head_y + 1, cx + 7, head_y + 11], fill=KNIGHT_GREY, outline=OUTLINE)
    d.rectangle([cx - 6, head_y, cx + 6, head_y + 1], fill=KNIGHT_GREY, outline=OUTLINE) # Chamfer top corners
    d.line([cx - 5, head_y + 1, cx + 5, head_y + 1], fill=KNIGHT_LIGHT)
    d.point((cx - 3, head_y + 2), fill=(255, 255, 255, 255)) # Shine

    if direction == "south":
        # Golden Visor Box
        d.rectangle([cx - 5, head_y + 4, cx + 5, head_y + 9], fill=KNIGHT_GOLD, outline=OUTLINE)
        d.line([cx - 4, head_y + 5, cx + 4, head_y + 5], fill=KNIGHT_GOLD_LIGHT)
        # Visor eye slit
        d.rectangle([cx - 4, head_y + 6, cx + 4, head_y + 8], fill=(30, 25, 20, 255))
        d.rectangle([cx - 3, head_y + 7, cx - 1, head_y + 7], fill=SKIN_LIGHT)
        d.rectangle([cx + 1, head_y + 7, cx + 3, head_y + 7], fill=SKIN_LIGHT)
        d.line([cx - 1, head_y + 10, cx + 1, head_y + 10], fill=(225, 110, 50, 255))
    elif direction in ("south_east", "east"):
        d.rectangle([cx - 1, head_y + 4, cx + 6, head_y + 9], fill=KNIGHT_GOLD, outline=OUTLINE)
        d.line([cx, head_y + 5, cx + 5, head_y + 5], fill=KNIGHT_GOLD_LIGHT)
        d.rectangle([cx + 1, head_y + 6, cx + 5, head_y + 8], fill=(30, 25, 20, 255))
        d.rectangle([cx + 2, head_y + 7, cx + 4, head_y + 7], fill=SKIN_LIGHT)
    elif direction in ("north_east", "north"):
        d.line([cx, head_y + 2, cx, head_y + 10], fill=KNIGHT_SHADOW)
        d.line([cx - 3, head_y + 3, cx - 3, head_y + 9], fill=KNIGHT_SHADOW)
        d.line([cx + 3, head_y + 3, cx + 3, head_y + 9], fill=KNIGHT_SHADOW)

    return img

# ==================== 3. VOID REAPER SKIN ====================
def draw_void_reaper_skin_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    float_y = int(math.sin(frame * math.pi / 2.0) * 1.5)
    cx = 16

    ROBE_GREY = (100, 110, 125, 255)
    ROBE_LIGHT = (160, 175, 195, 255)
    ROBE_DARK = (50, 55, 65, 255)
    GOLD_TRIM = (245, 170, 20, 255)
    RED_TENTACLE = (220, 38, 38, 255)
    RED_GLOW = (255, 80, 80, 255)

    # 1. Swirling Red Tentacles
    anim = frame % 4
    t1_y = 15 + float_y + anim
    t2_y = 15 + float_y - anim
    d.line([cx - 5, t1_y, cx - 11, t1_y + 4, cx - 13, t1_y + 10], fill=RED_TENTACLE, width=2)
    d.line([cx + 5, t2_y, cx + 11, t2_y + 4, cx + 13, t2_y + 10], fill=RED_TENTACLE, width=2)
    d.line([cx - 2, 22 + float_y, cx - 5, 28 + float_y, cx - 4, 31], fill=RED_TENTACLE)
    d.line([cx + 2, 22 + float_y, cx + 5, 28 + float_y, cx + 4, 31], fill=RED_TENTACLE)

    # 2. Conical Body (Hovering)
    cone_top = 17 + float_y
    cone_tip = 28 + float_y
    d.polygon([(cx - 5, cone_top), (cx + 5, cone_top), (cx, cone_tip)], fill=ROBE_GREY, outline=OUTLINE)
    d.line([cx, cone_top, cx, cone_tip], fill=ROBE_LIGHT)
    d.line([cx - 4, cone_top + 1, cx + 4, cone_top + 1], fill=GOLD_TRIM)

    # 3. Detached Floating Hands
    hand_l_y = 20 + float_y + (1 if frame in (1, 2) else 0)
    hand_r_y = 20 + float_y + (1 if frame in (0, 3) else 0)

    d.rectangle([cx - 12, hand_l_y, cx - 9, hand_l_y + 3], fill=ROBE_GREY, outline=OUTLINE)
    d.line([cx - 12, hand_l_y, cx - 9, hand_l_y], fill=GOLD_TRIM)
    d.line([cx - 13, hand_l_y + 1, cx - 8, hand_l_y + 1], fill=RED_GLOW)

    d.rectangle([cx + 9, hand_r_y, cx + 12, hand_r_y + 3], fill=ROBE_GREY, outline=OUTLINE)
    d.line([cx + 9, hand_r_y, cx + 12, hand_r_y], fill=GOLD_TRIM)
    d.line([cx + 8, hand_r_y + 1, cx + 13, hand_r_y + 1], fill=RED_GLOW)

    # 4. Hooded Cowl & Crown
    hood_y = 6 + float_y
    d.rectangle([cx - 7, hood_y, cx + 7, hood_y + 11], fill=ROBE_GREY, outline=OUTLINE)
    d.line([cx - 5, hood_y + 1, cx + 5, hood_y + 1], fill=ROBE_LIGHT)
    d.polygon([(cx - 7, hood_y + 8), (cx - 10, hood_y + 12), (cx - 6, hood_y + 12)], fill=ROBE_GREY, outline=OUTLINE)
    d.polygon([(cx + 7, hood_y + 8), (cx + 10, hood_y + 12), (cx + 6, hood_y + 12)], fill=ROBE_GREY, outline=OUTLINE)

    d.rectangle([cx - 5, hood_y - 2, cx + 5, hood_y], fill=GOLD_TRIM, outline=OUTLINE)
    d.point((cx - 3, hood_y - 2), fill=(255, 255, 200, 255))
    d.point((cx + 3, hood_y - 2), fill=(255, 255, 200, 255))

    if direction in ("south", "south_east", "east"):
        d.rectangle([cx - 4, hood_y + 4, cx + 4, hood_y + 9], fill=(20, 25, 35, 255))
        eye_x = cx if direction == "south" else (cx + 2)
        eye_y = hood_y + 6
        d.line([eye_x - 3, eye_y, eye_x + 3, eye_y], fill=RED_GLOW, width=2)
        d.line([eye_x, eye_y - 3, eye_x, eye_y + 3], fill=RED_GLOW, width=2)
        d.point((eye_x, eye_y), fill=(255, 240, 240, 255))
    else:
        d.line([cx, hood_y + 2, cx, hood_y + 10], fill=ROBE_DARK)

    return img

# ==================== 4. PAPERDOLL LAYERS (SEPARATED) ====================
def draw_paperdoll_armor_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise
    cx = 16
    torso_y = 18 + bob

    d.rectangle([cx - 5, torso_y, cx + 5, torso_y + 5], fill=KNIGHT_GREY, outline=OUTLINE)
    d.line([cx - 4, torso_y + 1, cx + 4, torso_y + 1], fill=KNIGHT_LIGHT)
    d.line([cx - 4, torso_y + 5, cx + 4, torso_y + 5], fill=KNIGHT_SHADOW)
    d.rectangle([cx - 1, torso_y + 2, cx + 1, torso_y + 4], fill=KNIGHT_GOLD, outline=OUTLINE)
    if direction in ("south", "north"):
        d.rectangle([cx - 7, torso_y - 1, cx - 5, torso_y + 3], fill=KNIGHT_GREY, outline=OUTLINE)
        d.rectangle([cx + 5, torso_y - 1, cx + 7, torso_y + 3], fill=KNIGHT_GREY, outline=OUTLINE)
    return img

def draw_paperdoll_pants_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise
    cx = 16
    leg_y = 23 + bob

    leg_l_off = -1 if frame == 1 else (1 if frame == 3 else 0)
    leg_r_off = 1 if frame == 1 else (-1 if frame == 3 else 0)

    # Armored greaves covering full leg down to ankle!
    PANTS_COL = (70, 80, 100, 255)
    d.rectangle([cx - 5, leg_y, cx + 5, leg_y + 2], fill=PANTS_COL, outline=OUTLINE)
    if direction in ("south", "north"):
        d.rectangle([cx - 4, leg_y + 2 + leg_l_off, cx - 2, leg_y + 5 + leg_l_off], fill=KNIGHT_GREY, outline=OUTLINE)
        d.rectangle([cx + 2, leg_y + 2 + leg_r_off, cx + 4, leg_y + 5 + leg_r_off], fill=KNIGHT_GREY, outline=OUTLINE)
    return img

def draw_paperdoll_scarf_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise
    cx = 16
    scarf_y = 16 + bob
    wave = -1 if frame in (1, 3) else 0

    d.rectangle([cx - 6, scarf_y, cx + 6, scarf_y + 2], fill=SCARF_ORANGE, outline=OUTLINE)
    d.line([cx - 5, scarf_y, cx + 5, scarf_y], fill=SCARF_LIGHT)
    if direction in ("south_east", "east"):
        d.rectangle([cx - 11, scarf_y + 1 + wave, cx - 5, scarf_y + 5 + wave], fill=SCARF_ORANGE, outline=OUTLINE)
        d.line([cx - 10, scarf_y + 1 + wave, cx - 6, scarf_y + 1 + wave], fill=SCARF_LIGHT)
    else:
        d.polygon([(cx - 6, scarf_y + 2), (cx - 9, scarf_y + 5 + wave), (cx - 6, scarf_y + 7 + wave)], fill=SCARF_ORANGE, outline=OUTLINE)
    return img

def draw_paperdoll_helmet_core(direction="south", frame=0):
    img = Image.new("RGBA", (CANVAS_SIZE, CANVAS_SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    dip = 1 if frame in (1, 3) else 0
    rise = -1 if frame == 2 else 0
    bob = dip + rise
    cx = 16
    head_y = 5 + bob

    # Rounded dome
    d.rectangle([cx - 7, head_y + 1, cx + 7, head_y + 11], fill=KNIGHT_GREY, outline=OUTLINE)
    d.rectangle([cx - 6, head_y, cx + 6, head_y + 1], fill=KNIGHT_GREY, outline=OUTLINE)
    d.line([cx - 5, head_y + 1, cx + 5, head_y + 1], fill=KNIGHT_LIGHT)

    if direction == "south":
        d.rectangle([cx - 5, head_y + 4, cx + 5, head_y + 9], fill=KNIGHT_GOLD, outline=OUTLINE)
        d.rectangle([cx - 4, head_y + 6, cx + 4, head_y + 8], fill=(30, 25, 20, 255))
        d.rectangle([cx - 3, head_y + 7, cx - 1, head_y + 7], fill=SKIN_LIGHT)
        d.rectangle([cx + 1, head_y + 7, cx + 3, head_y + 7], fill=SKIN_LIGHT)
    elif direction in ("south_east", "east"):
        d.rectangle([cx - 1, head_y + 4, cx + 6, head_y + 9], fill=KNIGHT_GOLD, outline=OUTLINE)
        d.rectangle([cx + 1, head_y + 6, cx + 5, head_y + 8], fill=(30, 25, 20, 255))
        d.rectangle([cx + 2, head_y + 7, cx + 4, head_y + 7], fill=SKIN_LIGHT)
    return img

# ==================== 5. ELEMENTAL SLASH VFX ====================
SLASH_ELEMENTS = {
    "fire": { "core": (255, 245, 180, 255), "mid": (249, 115, 22, 255), "dark": (194, 65, 12, 255), "trail": (239, 68, 68, 200) },
    "ice": { "core": (240, 255, 255, 255), "mid": (56, 189, 248, 255), "dark": (2, 132, 199, 255), "trail": (14, 165, 233, 200) },
    "thunder": { "core": (255, 255, 255, 255), "mid": (250, 204, 21, 255), "dark": (168, 85, 247, 255), "trail": (234, 179, 8, 200) },
    "wind": { "core": (240, 253, 250, 255), "mid": (45, 212, 191, 255), "dark": (13, 148, 136, 255), "trail": (20, 184, 166, 200) },
    "holy": { "core": (255, 255, 255, 255), "mid": (253, 224, 71, 255), "dark": (234, 179, 8, 255), "trail": (254, 240, 138, 220) },
    "dark": { "core": (216, 180, 254, 255), "mid": (147, 51, 234, 255), "dark": (88, 28, 135, 255), "trail": (30, 10, 60, 220) },
    "nature": { "core": (236, 253, 245, 255), "mid": (34, 197, 94, 255), "dark": (21, 128, 61, 255), "trail": (74, 222, 128, 200) }
}

def draw_elemental_slash(element="fire", frame=0):
    size = 48
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    p = SLASH_ELEMENTS.get(element, SLASH_ELEMENTS["fire"])
    core, mid, dark, trail = p["core"], p["mid"], p["dark"], p["trail"]
    cx, cy = size // 2, size // 2

    if frame == 0:
        d.arc([cx - 10, cy - 10, cx + 10, cy + 10], start=-60, end=30, fill=mid, width=3)
        d.arc([cx - 8, cy - 8, cx + 8, cy + 8], start=-45, end=15, fill=core, width=2)
    elif frame == 1:
        d.arc([cx - 18, cy - 18, cx + 18, cy + 18], start=-80, end=70, fill=dark, width=5)
        d.arc([cx - 17, cy - 17, cx + 17, cy + 17], start=-70, end=60, fill=mid, width=4)
        d.arc([cx - 16, cy - 16, cx + 16, cy + 16], start=-60, end=50, fill=core, width=2)
        d.line([cx + 14, cy - 10, cx + 18, cy - 3], fill=core, width=2)
    elif frame == 2:
        d.arc([cx - 18, cy - 18, cx + 18, cy + 18], start=-40, end=80, fill=trail, width=2)
        d.point((cx + 14, cy + 8), fill=mid)
        d.point((cx + 16, cy + 12), fill=core)
    return img

# ==================== MAIN GENERATOR ====================
def main():
    print("=" * 60)
    print("🎨 GENERATING 8-DIRECTION CHIBI ASSETS FOR GODLESS FANTASY")
    print("=" * 60)

    total = 0

    # 1. Base Bodies (Male & Female) - 8 Directions x 4 Frames = 64 Sprites
    for gender in ["male", "female"]:
        for d_name in DIRECTIONS_8:
            for f in range(4):
                sprite = get_8dir_image(lambda d, fr: draw_base_body_core(gender, d, fr), d_name, f)
                save_sprite(sprite, f"player_base_{gender}_{d_name}_{f}")
                total += 1
    print(f"✅ Generated 64 Base Body Sprites (Male & Female in 8 Directions)")

    # 2. Paperdoll Layers (Armor, Pants, Scarf, Helmet) - 8 Directions x 4 Frames = 128 Sprites
    for d_name in DIRECTIONS_8:
        for f in range(4):
            # Armor
            s_armor = get_8dir_image(draw_paperdoll_armor_core, d_name, f)
            save_sprite(s_armor, f"layer_armor_iron_{d_name}_{f}")
            # Pants
            s_pants = get_8dir_image(draw_paperdoll_pants_core, d_name, f)
            save_sprite(s_pants, f"layer_pants_iron_{d_name}_{f}")
            # Scarf
            s_scarf = get_8dir_image(draw_paperdoll_scarf_core, d_name, f)
            save_sprite(s_scarf, f"layer_scarf_orange_{d_name}_{f}")
            # Helmet
            s_helm = get_8dir_image(draw_paperdoll_helmet_core, d_name, f)
            save_sprite(s_helm, f"layer_helmet_knight_{d_name}_{f}")
            total += 4
    print(f"✅ Generated 128 Paperdoll Layer Sprites in 8 Directions")

    # 3. Full Legendary Skins (Soul Knight & Void Reaper) - 8 Directions x 4 Frames = 64 Sprites
    for d_name in DIRECTIONS_8:
        for f in range(4):
            s_sk = get_8dir_image(draw_soul_knight_paladin_core, d_name, f)
            save_sprite(s_sk, f"skin_soul_knight_{d_name}_{f}")
            s_vr = get_8dir_image(draw_void_reaper_skin_core, d_name, f)
            save_sprite(s_vr, f"skin_void_reaper_{d_name}_{f}")
            total += 2
    print(f"✅ Generated 64 Full Legendary Skin Sprites in 8 Directions")

    # 4. Elemental Slash VFX (7 Elements x 3 Frames = 21 Sprites)
    for elem in SLASH_ELEMENTS.keys():
        for f in range(3):
            s_slash = draw_elemental_slash(elem, f)
            save_sprite(s_slash, f"vfx_slash_{elem}_{f}")
            total += 1
    print(f"✅ Generated 21 Elemental Slash VFX Sprites")

    # 5. UI Avatar / Portrait Sprites (Front South view, Frame 0)
    save_sprite(draw_base_body_core("male", "south", 0), "avatar_male")
    save_sprite(draw_base_body_core("female", "south", 0), "avatar_female")
    save_sprite(draw_soul_knight_paladin_core("south", 0), "avatar_soul_knight")
    save_sprite(draw_void_reaper_skin_core("south", 0), "avatar_void_reaper")
    total += 4
    print(f"✅ Generated 4 High-Resolution UI Portrait & Avatar Sprites")

    print(f"\n🎉 HOÀN TẤT TỔNG CỘNG: {total} SPRITES 8 HƯỚNG ĐÃ XUẤT THÀNH CÔNG VÀO UNITY!")

if __name__ == "__main__":
    main()
