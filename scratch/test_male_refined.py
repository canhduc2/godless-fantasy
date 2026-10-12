# -*- coding: utf-8 -*-
import os
from PIL import Image

chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
m_base = Image.open(os.path.join(chars_dir, "player_base_male_east_0.png")).convert("RGBA")
m_upper = m_base.crop((0, 0, 64, 44))

M_SKIN_LIGHT   = (250, 222, 202, 255)
M_SKIN_MID     = (240, 190, 162, 255)
M_SKIN_SHADOW  = (212, 146, 116, 255)
M_SKIN_OUTLINE = (55, 30, 26, 255)

M_FAR_SKIN_MID = (200, 136, 110, 255)
M_FAR_SKIN_SH  = (170, 110, 88, 255)
M_FAR_SKIN_OUT = (48, 24, 22, 255)

# Male Shorts (Anime Chibi Shorts matching south_0)
M_SH_LIGHT   = (96, 96, 102, 255)
M_SH_MID     = (72, 72, 76, 255)
M_SH_SHADOW  = (50, 50, 54, 255)
M_SH_OUTLINE = (24, 24, 26, 255)

# Male Shoes / Footwear
M_SHOE_LIGHT   = (80, 80, 86, 255)
M_SHOE_MID     = (52, 52, 56, 255)
M_SHOE_SHADOW  = (36, 36, 40, 255)
M_SHOE_OUTLINE = (22, 22, 24, 255)
M_SHOE_SOLE    = (18, 18, 20, 255)
M_FAR_SHOE_MID = (38, 38, 42, 255)

canvas = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
canvas.paste(m_upper, (0, 0), m_upper)
pix = canvas.load()

# 1. FAR LEG (Background layer, left leg standing slightly back at x=25..30)
for y in range(49, 54):
    pix[25, y] = M_FAR_SKIN_OUT
    pix[26, y] = M_FAR_SKIN_SH
    pix[27, y] = M_FAR_SKIN_MID
    pix[28, y] = M_FAR_SKIN_MID
    pix[29, y] = M_FAR_SKIN_OUT

for y in range(54, 57):
    pix[24, y] = M_SHOE_OUTLINE
    pix[25, y] = M_FAR_SHOE_MID
    pix[26, y] = M_FAR_SHOE_MID
    pix[27, y] = M_FAR_SHOE_MID
    pix[28, y] = M_FAR_SHOE_MID
    pix[29, y] = M_SHOE_OUTLINE
for x in range(24, 30):
    pix[x, 57] = M_SHOE_SOLE

# 2. MALE SHORTS (y=44..48, x=25..37)
# y=44: Waistband
pix[26, 44] = M_SH_OUTLINE
pix[27, 44] = M_SH_SHADOW
pix[28, 44] = M_SH_MID
pix[29, 44] = M_SH_MID
pix[30, 44] = M_SH_LIGHT
pix[31, 44] = M_SH_LIGHT
pix[32, 44] = M_SH_MID
pix[33, 44] = M_SH_MID
pix[34, 44] = M_SH_LIGHT
pix[35, 44] = M_SH_MID
pix[36, 44] = M_SH_SHADOW
pix[37, 44] = M_SH_OUTLINE

# y=45: Upper hip & glute curve (x=25..37)
pix[25, 45] = M_SH_OUTLINE
pix[26, 45] = M_SH_SHADOW
pix[27, 45] = M_SH_MID
pix[28, 45] = M_SH_MID
pix[29, 45] = M_SH_LIGHT
pix[30, 45] = M_SH_LIGHT
pix[31, 45] = M_SH_LIGHT
pix[32, 45] = M_SH_MID
pix[33, 45] = M_SH_MID
pix[34, 45] = M_SH_LIGHT
pix[35, 45] = M_SH_MID
pix[36, 45] = M_SH_SHADOW
pix[37, 45] = M_SH_OUTLINE

# y=46: Mid hip (x=25..37)
pix[25, 46] = M_SH_OUTLINE
pix[26, 46] = M_SH_SHADOW
pix[27, 46] = M_SH_MID
pix[28, 46] = M_SH_MID
pix[29, 46] = M_SH_LIGHT
pix[30, 46] = M_SH_LIGHT
pix[31, 46] = M_SH_MID
pix[32, 46] = M_SH_MID
pix[33, 46] = M_SH_MID
pix[34, 46] = M_SH_SHADOW
pix[35, 46] = M_SH_SHADOW
pix[36, 46] = M_SH_OUTLINE

# y=47: Lower shorts
pix[25, 47] = M_SH_OUTLINE
pix[26, 47] = M_SH_SHADOW
pix[27, 47] = M_SH_MID
pix[28, 47] = M_SH_MID
pix[29, 47] = M_SH_MID
pix[30, 47] = M_SH_MID
pix[31, 47] = M_SH_MID
pix[32, 47] = M_SH_SHADOW
pix[33, 47] = M_SH_SHADOW
pix[34, 47] = M_SH_OUTLINE
pix[35, 47] = M_SH_OUTLINE

# y=48: Hem line
pix[25, 48] = M_SH_OUTLINE
pix[26, 48] = M_SH_SHADOW
pix[27, 48] = M_SH_MID
pix[28, 48] = M_SH_MID
pix[29, 48] = M_SH_MID
pix[30, 48] = M_SH_SHADOW
pix[31, 48] = M_SH_OUTLINE
pix[32, 48] = M_SH_OUTLINE

# 3. FOREGROUND NEAR LEG (Right Leg, y=49..57, x=28..36)
for y in range(49, 52):
    pix[28, y] = M_SKIN_OUTLINE
    pix[29, y] = M_SKIN_SHADOW
    pix[30, y] = M_SKIN_MID
    pix[31, y] = M_SKIN_LIGHT
    pix[32, y] = M_SKIN_LIGHT
    pix[33, y] = M_SKIN_MID
    pix[34, y] = M_SKIN_LIGHT
    pix[35, y] = M_SKIN_MID
    pix[36, y] = M_SKIN_OUTLINE

# Knee (y=52..53)
pix[28, 52] = M_SKIN_OUTLINE
pix[29, 52] = M_SKIN_SHADOW
pix[30, 52] = M_SKIN_MID
pix[31, 52] = M_SKIN_LIGHT
pix[32, 52] = M_SKIN_LIGHT
pix[33, 52] = M_SKIN_MID
pix[34, 52] = M_SKIN_LIGHT
pix[35, 52] = M_SKIN_LIGHT
pix[36, 52] = M_SKIN_OUTLINE

pix[29, 53] = M_SKIN_OUTLINE
pix[30, 53] = M_SKIN_SHADOW
pix[31, 53] = M_SKIN_MID
pix[32, 53] = M_SKIN_LIGHT
pix[33, 53] = M_SKIN_LIGHT
pix[34, 53] = M_SKIN_MID
pix[35, 53] = M_SKIN_LIGHT
pix[36, 53] = M_SKIN_OUTLINE

# Calf & Ankle (y=54..55)
pix[29, 54] = M_SKIN_OUTLINE
pix[30, 54] = M_SKIN_SHADOW
pix[31, 54] = M_SKIN_MID
pix[32, 54] = M_SKIN_LIGHT
pix[33, 54] = M_SKIN_LIGHT
pix[34, 54] = M_SKIN_MID
pix[35, 54] = M_SKIN_OUTLINE

pix[29, 55] = M_SHOE_OUTLINE
pix[30, 55] = M_SHOE_SHADOW
pix[31, 55] = M_SHOE_MID
pix[32, 55] = M_SHOE_LIGHT
pix[33, 55] = M_SHOE_LIGHT
pix[34, 55] = M_SHOE_MID
pix[35, 55] = M_SHOE_LIGHT
pix[36, 55] = M_SHOE_OUTLINE

# Shoe Body (y=56, heel at x=28, toe at x=37)
pix[27, 56] = M_SHOE_OUTLINE
pix[28, 56] = M_SHOE_SHADOW
pix[29, 56] = M_SHOE_MID
pix[30, 56] = M_SHOE_LIGHT
pix[31, 56] = M_SHOE_LIGHT
pix[32, 56] = M_SHOE_MID
pix[33, 56] = M_SHOE_MID
pix[34, 56] = M_SHOE_LIGHT
pix[35, 56] = M_SHOE_LIGHT
pix[36, 56] = M_SHOE_MID
pix[37, 56] = M_SHOE_OUTLINE

# Sole (y=57)
for x in range(27, 38):
    pix[x, 57] = M_SHOE_SOLE

# 4. FOREGROUND ARM & HAND (y=44..51, connects to y=43, x=30..34)
# Forearm at y=44:
pix[30, 44] = M_SKIN_OUTLINE
pix[31, 44] = M_SKIN_SHADOW
pix[32, 44] = M_SKIN_MID
pix[33, 44] = M_SKIN_LIGHT
pix[34, 44] = M_SKIN_OUTLINE

# Forearm at y=45:
pix[30, 45] = M_SKIN_OUTLINE
pix[31, 45] = M_SKIN_SHADOW
pix[32, 45] = M_SKIN_MID
pix[33, 45] = M_SKIN_LIGHT
pix[34, 45] = M_SKIN_OUTLINE

# Forearm at y=46:
pix[30, 46] = M_SKIN_OUTLINE
pix[31, 46] = M_SKIN_SHADOW
pix[32, 46] = M_SKIN_MID
pix[33, 46] = M_SKIN_LIGHT
pix[34, 46] = M_SKIN_OUTLINE

# Wrist at y=47:
pix[31, 47] = M_SKIN_OUTLINE
pix[32, 47] = M_SKIN_MID
pix[33, 47] = M_SKIN_LIGHT
pix[34, 47] = M_SKIN_OUTLINE

# Hand & Palm at y=48:
pix[31, 48] = M_SKIN_OUTLINE
pix[32, 48] = M_SKIN_MID
pix[33, 48] = M_SKIN_LIGHT
pix[34, 48] = M_SKIN_LIGHT
pix[35, 48] = M_SKIN_OUTLINE

# Fingers & Knuckles at y=49:
pix[31, 49] = M_SKIN_OUTLINE
pix[32, 49] = M_SKIN_MID
pix[33, 49] = M_SKIN_LIGHT
pix[34, 49] = M_SKIN_LIGHT
pix[35, 49] = M_SKIN_OUTLINE

# Fingertips at y=50:
pix[32, 50] = M_SKIN_OUTLINE
pix[33, 50] = M_SKIN_LIGHT
pix[34, 50] = M_SKIN_MID
pix[35, 50] = M_SKIN_OUTLINE

# Bottom round of hand at y=51:
pix[33, 51] = M_SKIN_OUTLINE
pix[34, 51] = M_SKIN_OUTLINE

canvas.save("scratch/refined_male_east_0.png")
print("Saved refined_male_east_0.png")
