# -*- coding: utf-8 -*-
import os
from PIL import Image

chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
f_base = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
f_upper = f_base.crop((0, 0, 64, 44))

F_SKIN_LIGHT   = (252, 224, 204, 255) # Peach highlight
F_SKIN_MID     = (244, 196, 170, 255) # Base anime skin
F_SKIN_SHADOW  = (218, 156, 130, 255) # Warm Shadow
F_SKIN_OUTLINE = (96, 48, 44, 255)   # Soft anime outline

F_FAR_SKIN_MID = (205, 142, 118, 255)
F_FAR_SKIN_SH  = (175, 115, 95, 255)
F_FAR_SKIN_OUT = (75, 36, 34, 255)

# Shorts (Smooth dark charcoal)
F_SH_LIGHT   = (86, 86, 92, 255)
F_SH_MID     = (64, 64, 68, 255)
F_SH_SHADOW  = (46, 46, 50, 255)
F_SH_OUTLINE = (28, 26, 30, 255)
F_SH_FAR     = (38, 38, 42, 255)

# Shoes (Anime Sneakers / Boots)
F_SHOE_LIGHT   = (82, 78, 84, 255)
F_SHOE_MID     = (56, 52, 58, 255)
F_SHOE_SHADOW  = (40, 36, 42, 255)
F_SHOE_OUTLINE = (26, 24, 28, 255)
F_SHOE_SOLE    = (20, 18, 22, 255)
F_FAR_SHOE_MID = (40, 36, 42, 255)
F_FAR_SHOE_OUT = (22, 20, 24, 255)

canvas = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
canvas.paste(f_upper, (0, 0), f_upper)
pix = canvas.load()

# 1. FAR LEG (Background layer, left leg standing slightly back at x=26..31)
for y in range(48, 54):
    pix[26, y] = F_FAR_SKIN_OUT
    pix[27, y] = F_FAR_SKIN_SH
    pix[28, y] = F_FAR_SKIN_MID
    pix[29, y] = F_FAR_SKIN_MID
    pix[30, y] = F_FAR_SKIN_OUT

for y in range(54, 57):
    pix[25, y] = F_FAR_SHOE_OUT
    pix[26, y] = F_FAR_SHOE_MID
    pix[27, y] = F_FAR_SHOE_MID
    pix[28, y] = F_FAR_SHOE_MID
    pix[29, y] = F_FAR_SHOE_MID
    pix[30, y] = F_FAR_SHOE_OUT
for x in range(25, 31):
    pix[x, 57] = F_SHOE_SOLE

# 2. SHORTS (y=44..47, plump rounded hips, x=26..38)
# y=44: Waistline (aligns under chest x=38 and back x=27)
pix[27, 44] = F_SH_OUTLINE
pix[28, 44] = F_SH_SHADOW
pix[29, 44] = F_SH_MID
pix[30, 44] = F_SH_MID
pix[31, 44] = F_SH_LIGHT
pix[32, 44] = F_SH_LIGHT
pix[33, 44] = F_SH_MID
pix[34, 44] = F_SH_MID
pix[35, 44] = F_SH_LIGHT
pix[36, 44] = F_SH_MID
pix[37, 44] = F_SH_SHADOW
pix[38, 44] = F_SH_OUTLINE

# y=45: Upper hip & glute curve (x=26..38)
pix[26, 45] = F_SH_OUTLINE
pix[27, 45] = F_SH_SHADOW
pix[28, 45] = F_SH_MID
pix[29, 45] = F_SH_MID
pix[30, 45] = F_SH_LIGHT
pix[31, 45] = F_SH_LIGHT
pix[32, 45] = F_SH_LIGHT
pix[33, 45] = F_SH_MID
pix[34, 45] = F_SH_MID
pix[35, 45] = F_SH_LIGHT
pix[36, 45] = F_SH_MID
pix[37, 45] = F_SH_SHADOW
pix[38, 45] = F_SH_OUTLINE

# y=46: Mid hip (x=26..38)
pix[26, 46] = F_SH_OUTLINE
pix[27, 46] = F_SH_SHADOW
pix[28, 46] = F_SH_MID
pix[29, 46] = F_SH_MID
pix[30, 46] = F_SH_LIGHT
pix[31, 46] = F_SH_LIGHT
pix[32, 46] = F_SH_MID
pix[33, 46] = F_SH_MID
pix[34, 46] = F_SH_MID
pix[35, 46] = F_SH_SHADOW
pix[36, 46] = F_SH_SHADOW
pix[37, 46] = F_SH_OUTLINE

# y=47: Lower shorts & leg hem curve (x=26..36)
pix[26, 47] = F_SH_OUTLINE
pix[27, 47] = F_SH_SHADOW
pix[28, 47] = F_SH_MID
pix[29, 47] = F_SH_MID
pix[30, 47] = F_SH_MID
pix[31, 47] = F_SH_MID
pix[32, 47] = F_SH_SHADOW
pix[33, 47] = F_SH_SHADOW
pix[34, 47] = F_SH_OUTLINE
pix[35, 47] = F_SH_OUTLINE
pix[36, 47] = F_SH_OUTLINE

# 3. FOREGROUND NEAR LEG (Right Leg, y=48..57, x=29..37)
# Thigh (y=48..51, x=29..37)
for y in range(48, 52):
    pix[29, y] = F_SKIN_OUTLINE
    pix[30, y] = F_SKIN_SHADOW
    pix[31, y] = F_SKIN_MID
    pix[32, y] = F_SKIN_LIGHT
    pix[33, y] = F_SKIN_LIGHT
    pix[34, y] = F_SKIN_MID
    pix[35, y] = F_SKIN_LIGHT
    pix[36, y] = F_SKIN_MID
    pix[37, y] = F_SKIN_OUTLINE

# Knee (y=52..53, rounded kneecap at front x=36..37)
pix[29, 52] = F_SKIN_OUTLINE
pix[30, 52] = F_SKIN_SHADOW
pix[31, 52] = F_SKIN_MID
pix[32, 52] = F_SKIN_LIGHT
pix[33, 52] = F_SKIN_LIGHT
pix[34, 52] = F_SKIN_MID
pix[35, 52] = F_SKIN_LIGHT
pix[36, 52] = F_SKIN_LIGHT
pix[37, 52] = F_SKIN_OUTLINE

pix[30, 53] = F_SKIN_OUTLINE
pix[31, 53] = F_SKIN_SHADOW
pix[32, 53] = F_SKIN_MID
pix[33, 53] = F_SKIN_LIGHT
pix[34, 53] = F_SKIN_LIGHT
pix[35, 53] = F_SKIN_MID
pix[36, 53] = F_SKIN_LIGHT
pix[37, 53] = F_SKIN_OUTLINE

# Calf & Shin (y=54)
pix[30, 54] = F_SKIN_OUTLINE
pix[31, 54] = F_SKIN_SHADOW
pix[32, 54] = F_SKIN_MID
pix[33, 54] = F_SKIN_LIGHT
pix[34, 54] = F_SKIN_LIGHT
pix[35, 54] = F_SKIN_MID
pix[36, 54] = F_SKIN_OUTLINE

# Sneaker Collar / Ankle (y=55, x=29..37)
pix[29, 55] = F_SHOE_OUTLINE
pix[30, 55] = F_SHOE_SHADOW
pix[31, 55] = F_SHOE_MID
pix[32, 55] = F_SHOE_LIGHT
pix[33, 55] = F_SHOE_LIGHT
pix[34, 55] = F_SHOE_MID
pix[35, 55] = F_SHOE_LIGHT
pix[36, 55] = F_SHOE_LIGHT
pix[37, 55] = F_SHOE_OUTLINE

# Sneaker Body (y=56, heel at x=28, toe at x=38)
pix[28, 56] = F_SHOE_OUTLINE # heel
pix[29, 56] = F_SHOE_SHADOW
pix[30, 56] = F_SHOE_MID
pix[31, 56] = F_SHOE_LIGHT
pix[32, 56] = F_SHOE_LIGHT
pix[33, 56] = F_SHOE_MID
pix[34, 56] = F_SHOE_MID
pix[35, 56] = F_SHOE_LIGHT
pix[36, 56] = F_SHOE_LIGHT
pix[37, 56] = F_SHOE_MID
pix[38, 56] = F_SHOE_OUTLINE # toe cap

# Sole (y=57, x=28..38)
for x in range(28, 39):
    pix[x, 57] = F_SHOE_SOLE

# 4. FOREGROUND ARM & HAND (y=44..51, resting at side)
# Forearm at y=44 (connects with upper arm at y=43, x=31..35)
pix[31, 44] = F_SKIN_OUTLINE
pix[32, 44] = F_SKIN_SHADOW
pix[33, 44] = F_SKIN_MID
pix[34, 44] = F_SKIN_LIGHT
pix[35, 44] = F_SKIN_OUTLINE

# Forearm at y=45:
pix[31, 45] = F_SKIN_OUTLINE
pix[32, 45] = F_SKIN_SHADOW
pix[33, 45] = F_SKIN_MID
pix[34, 45] = F_SKIN_LIGHT
pix[35, 45] = F_SKIN_OUTLINE

# Forearm at y=46:
pix[31, 46] = F_SKIN_OUTLINE
pix[32, 46] = F_SKIN_SHADOW
pix[33, 46] = F_SKIN_MID
pix[34, 46] = F_SKIN_LIGHT
pix[35, 46] = F_SKIN_OUTLINE

# Wrist at y=47:
pix[32, 47] = F_SKIN_OUTLINE
pix[33, 47] = F_SKIN_MID
pix[34, 47] = F_SKIN_LIGHT
pix[35, 47] = F_SKIN_OUTLINE

# Hand & Palm at y=48:
pix[32, 48] = F_SKIN_OUTLINE
pix[33, 48] = F_SKIN_MID
pix[34, 48] = F_SKIN_LIGHT
pix[35, 48] = F_SKIN_LIGHT
pix[36, 48] = F_SKIN_OUTLINE

# Fingers & Knuckles at y=49:
pix[32, 49] = F_SKIN_OUTLINE
pix[33, 49] = F_SKIN_MID
pix[34, 49] = F_SKIN_LIGHT
pix[35, 49] = F_SKIN_LIGHT
pix[36, 49] = F_SKIN_OUTLINE

# Fingertips at y=50:
pix[33, 50] = F_SKIN_OUTLINE
pix[34, 50] = F_SKIN_LIGHT
pix[35, 50] = F_SKIN_MID
pix[36, 50] = F_SKIN_OUTLINE

# Bottom round of hand at y=51:
pix[34, 51] = F_SKIN_OUTLINE
pix[35, 51] = F_SKIN_OUTLINE

canvas.save("scratch/refined_female_east_0.png")
print("Saved refined_female_east_0.png")
