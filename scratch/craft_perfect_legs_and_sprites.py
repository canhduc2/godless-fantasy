# -*- coding: utf-8 -*-
"""
Generate moderate, well-proportioned anime chibi legs (6-7px wide)
for Male (barefoot) and Female (sneakers) East/West 0..3 with zero pixel gaps.
"""
import os
from PIL import Image

def generate_proportioned_sprites():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"

    # Female raw upper
    f_raw = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
    f_upper = f_raw.crop((0, 0, 64, 44))
    f_upix = f_upper.load()
    for y in range(44):
        for x in range(64):
            r, g, b, a = f_upix[x, y]
            if a > 20:
                ru = min(255, int(r * 255.0 / a))
                gu = min(255, int(g * 255.0 / a))
                bu = min(255, int(b * 255.0 / a))
                f_upix[x, y] = (ru, gu, bu, 255)
            else:
                f_upix[x, y] = (0, 0, 0, 0)

    # Male raw upper
    m_raw = Image.open(os.path.join(chars_dir, "player_base_male_east_0.png")).convert("RGBA")
    m_upper = m_raw.crop((0, 0, 64, 44))
    m_upix = m_upper.load()
    for y in range(44):
        for x in range(64):
            r, g, b, a = m_upix[x, y]
            if a > 20:
                ru = min(255, int(r * 255.0 / a))
                gu = min(255, int(g * 255.0 / a))
                bu = min(255, int(b * 255.0 / a))
                m_upix[x, y] = (ru, gu, bu, 255)
            else:
                m_upix[x, y] = (0, 0, 0, 0)

    # Palettes
    HAIR_DARK_OUT = (52, 28, 28, 255)
    HAIR_SHADOW   = (98, 62, 60, 255)
    HAIR_MID      = (148, 102, 100, 255)
    HAIR_LIGHT    = (185, 132, 128, 255)
    
    F_SKIN_LIGHT   = (252, 224, 204, 255)
    F_SKIN_MID     = (244, 196, 170, 255)
    F_SKIN_SHADOW  = (218, 156, 130, 255)
    F_SKIN_OUTLINE = (96, 48, 44, 255)
    
    F_FAR_SKIN_MID = (205, 142, 118, 255)
    F_FAR_SKIN_SH  = (175, 115, 95, 255)
    F_FAR_SKIN_OUT = (75, 36, 34, 255)
    
    F_SH_LIGHT   = (86, 86, 92, 255)
    F_SH_MID     = (64, 64, 68, 255)
    F_SH_SHADOW  = (46, 46, 50, 255)
    F_SH_OUTLINE = (28, 26, 30, 255)
    
    F_SHOE_LIGHT   = (82, 78, 84, 255)
    F_SHOE_MID     = (56, 52, 58, 255)
    F_SHOE_SHADOW  = (40, 36, 42, 255)
    F_SHOE_OUTLINE = (26, 24, 28, 255)
    F_SHOE_SOLE    = (20, 18, 22, 255)
    F_FAR_SHOE_MID = (40, 36, 42, 255)

    M_SKIN_LIGHT   = (250, 222, 202, 255)
    M_SKIN_MID     = (240, 190, 162, 255)
    M_SKIN_SHADOW  = (212, 146, 116, 255)
    M_SKIN_OUTLINE = (45, 25, 22, 255)
    M_GROUND_OUT   = (25, 15, 15, 255)
    
    M_FAR_SKIN_MID = (200, 136, 110, 255)
    M_FAR_SKIN_SH  = (170, 110, 88, 255)
    M_FAR_SKIN_OUT = (48, 24, 22, 255)
    
    M_SH_LIGHT   = (96, 96, 102, 255)
    M_SH_MID     = (72, 72, 76, 255)
    M_SH_SHADOW  = (50, 50, 54, 255)
    M_SH_OUTLINE = (24, 24, 26, 255)

    # -------------------------------------------------------------
    # FEMALE EAST (Moderate 6-7px legs, sneakers, natural hair curls)
    # -------------------------------------------------------------
    def draw_female(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(f_upper, (0, bob), f_upper)
        pix = img.load()

        # Hair curls (natural wavy flowing locks down to y=48)
        pix[19, 44 + bob] = HAIR_DARK_OUT
        pix[20, 44 + bob] = HAIR_DARK_OUT
        pix[21, 44 + bob] = HAIR_SHADOW
        pix[22, 44 + bob] = HAIR_MID
        pix[23, 44 + bob] = HAIR_MID
        pix[24, 44 + bob] = HAIR_SHADOW
        pix[25, 44 + bob] = HAIR_MID
        pix[26, 44 + bob] = HAIR_SHADOW
        pix[27, 44 + bob] = HAIR_DARK_OUT
        
        pix[20, 45 + bob] = HAIR_DARK_OUT
        pix[21, 45 + bob] = HAIR_SHADOW
        pix[22, 45 + bob] = HAIR_MID
        pix[23, 45 + bob] = HAIR_MID
        pix[24, 45 + bob] = HAIR_SHADOW
        pix[25, 45 + bob] = HAIR_MID
        pix[26, 45 + bob] = HAIR_DARK_OUT
        
        pix[20, 46 + bob] = HAIR_DARK_OUT
        pix[21, 46 + bob] = HAIR_SHADOW
        pix[22, 46 + bob] = HAIR_MID
        pix[23, 46 + bob] = HAIR_MID
        pix[24, 46 + bob] = HAIR_SHADOW
        pix[25, 46 + bob] = HAIR_DARK_OUT
        
        pix[21, 47 + bob] = HAIR_DARK_OUT
        pix[22, 47 + bob] = HAIR_SHADOW
        pix[23, 47 + bob] = HAIR_MID
        pix[24, 47 + bob] = HAIR_SHADOW
        pix[25, 47 + bob] = HAIR_DARK_OUT
        
        pix[22, 48 + bob] = HAIR_DARK_OUT
        pix[23, 48 + bob] = HAIR_SHADOW
        pix[24, 48 + bob] = HAIR_DARK_OUT

        # FAR LEG (Moderate 5-6px wide, shaded depth)
        if frame == 0:
            for y in range(48 + bob, 54 + bob):
                pix[25, y] = F_FAR_SKIN_OUT
                pix[26, y] = F_FAR_SKIN_SH
                pix[27, y] = F_FAR_SKIN_MID
                pix[28, y] = F_FAR_SKIN_MID
                pix[29, y] = F_FAR_SKIN_MID
                pix[30, y] = F_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[25, y] = F_SHOE_OUTLINE
                pix[26, y] = F_FAR_SHOE_MID
                pix[27, y] = F_FAR_SHOE_MID
                pix[28, y] = F_FAR_SHOE_MID
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_SHOE_OUTLINE
            for x in range(25, 31):
                pix[x, 57 + bob] = F_SHOE_SOLE
        elif frame == 1:
            for y in range(48 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 25 - dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_MID
                pix[x_b + 4, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[21, y] = F_SHOE_OUTLINE
                pix[22, y] = F_FAR_SHOE_MID
                pix[23, y] = F_FAR_SHOE_MID
                pix[24, y] = F_FAR_SHOE_MID
                pix[25, y] = F_SHOE_OUTLINE
            for x in range(21, 26):
                pix[x, 56 + bob] = F_SHOE_SOLE
        elif frame == 2:
            for y in range(48 + bob, 53 + bob):
                pix[26, y] = F_FAR_SKIN_OUT
                pix[27, y] = F_FAR_SKIN_SH
                pix[28, y] = F_FAR_SKIN_MID
                pix[29, y] = F_FAR_SKIN_MID
                pix[30, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[26, y] = F_SHOE_OUTLINE
                pix[27, y] = F_FAR_SHOE_MID
                pix[28, y] = F_FAR_SHOE_MID
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_SHOE_OUTLINE
            for x in range(26, 31):
                pix[x, 56 + bob] = F_SHOE_SOLE
        elif frame == 3:
            for y in range(48 + bob, 53 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 30 + dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_MID
                pix[x_b + 4, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[32, y] = F_SHOE_OUTLINE
                pix[33, y] = F_FAR_SHOE_MID
                pix[34, y] = F_FAR_SHOE_MID
                pix[35, y] = F_FAR_SHOE_MID
                pix[36, y] = F_SHOE_OUTLINE
            for x in range(32, 37):
                pix[x, 57 + bob] = F_SHOE_SOLE

        # SHORTS (Hips & Hem x=26..36, covers perfectly)
        # y=44: x=27..36
        for x in range(27, 37):
            c = F_SH_MID
            if x == 27 or x == 36: c = F_SH_OUTLINE
            elif x in (28, 35): c = F_SH_SHADOW
            elif x in (30, 31): c = F_SH_LIGHT
            pix[x, 44 + bob] = c

        # y=45: x=26..36
        for x in range(26, 37):
            c = F_SH_MID
            if x == 26 or x == 36: c = F_SH_OUTLINE
            elif x in (27, 34, 35): c = F_SH_SHADOW
            elif x in (29, 30): c = F_SH_LIGHT
            pix[x, 45 + bob] = c

        # y=46: x=26..35
        for x in range(26, 36):
            c = F_SH_MID
            if x == 26 or x == 35: c = F_SH_OUTLINE
            elif x in (27, 33, 34): c = F_SH_SHADOW
            elif x in (29, 30): c = F_SH_LIGHT
            pix[x, 46 + bob] = c

        # y=47: x=26..35
        for x in range(26, 36):
            c = F_SH_MID
            if x == 26 or x == 35: c = F_SH_OUTLINE
            elif x in (27, 32, 33, 34): c = F_SH_SHADOW
            pix[x, 47 + bob] = c

        # y=48: shorts hem back portion at x=25..31 (seals back thigh completely!)
        pix[25, 48 + bob] = F_SH_OUTLINE
        pix[26, 48 + bob] = F_SH_SHADOW
        pix[27, 48 + bob] = F_SH_MID
        pix[28, 48 + bob] = F_SH_MID
        pix[29, 48 + bob] = F_SH_SHADOW
        pix[30, 48 + bob] = F_SH_OUTLINE

        # NEAR LEG (Moderate 6-7px wide, healthy anime proportions)
        if frame == 0:
            # Thigh y=48..51: 7px wide (x=29..35)
            for y in range(48 + bob, 52 + bob):
                pix[29, y] = F_SKIN_OUTLINE
                pix[30, y] = F_SKIN_SHADOW
                pix[31, y] = F_SKIN_MID
                pix[32, y] = F_SKIN_LIGHT
                pix[33, y] = F_SKIN_LIGHT
                pix[34, y] = F_SKIN_MID
                pix[35, y] = F_SKIN_OUTLINE

            # Knee/Calf y=52..54: 6px wide (x=29..34)
            for y in range(52 + bob, 55 + bob):
                pix[29, y] = F_SKIN_OUTLINE
                pix[30, y] = F_SKIN_SHADOW
                pix[31, y] = F_SKIN_MID
                pix[32, y] = F_SKIN_LIGHT
                pix[33, y] = F_SKIN_MID
                pix[34, y] = F_SKIN_OUTLINE

            # Ankle y=55: 6px wide (x=29..34)
            pix[29, 55 + bob] = F_SKIN_OUTLINE
            pix[30, 55 + bob] = F_SKIN_SHADOW
            pix[31, 55 + bob] = F_SKIN_MID
            pix[32, 55 + bob] = F_SKIN_LIGHT
            pix[33, 55 + bob] = F_SKIN_MID
            pix[34, 55 + bob] = F_SKIN_OUTLINE

            # Shoe y=56: 8px length (x=29..36)
            pix[29, 56 + bob] = F_SHOE_OUTLINE
            pix[30, 56 + bob] = F_SHOE_SHADOW
            pix[31, 56 + bob] = F_SHOE_MID
            pix[32, 56 + bob] = F_SHOE_LIGHT
            pix[33, 56 + bob] = F_SHOE_LIGHT
            pix[34, 56 + bob] = F_SHOE_MID
            pix[35, 56 + bob] = F_SHOE_SHADOW
            pix[36, 56 + bob] = F_SHOE_OUTLINE

            # Sole y=57:
            for x in range(29, 37):
                pix[x, 57 + bob] = F_SHOE_SOLE

        elif frame == 1:
            for y in range(48 + bob, 54 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 29 + dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_MID
                pix[x_b + 5, y] = F_SKIN_OUTLINE
            for x in range(31, 38):
                pix[x, 54 + bob] = F_SHOE_MID if x in (33, 34, 35) else F_SHOE_SHADOW
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (34, 35, 36) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[30, 56 + bob] = F_SHOE_OUTLINE
            pix[38, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 2:
            for y in range(48 + bob, 54 + bob):
                pix[29, y] = F_SKIN_OUTLINE
                pix[30, y] = F_SKIN_SHADOW
                pix[31, y] = F_SKIN_MID
                pix[32, y] = F_SKIN_LIGHT
                pix[33, y] = F_SKIN_MID
                pix[34, y] = F_SKIN_OUTLINE
            for x in range(29, 36):
                pix[x, 54 + bob] = F_SHOE_MID
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (31, 32, 33) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[28, 56 + bob] = F_SHOE_OUTLINE
            pix[36, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 3:
            for y in range(48 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 26 - dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_MID
                pix[x_b + 5, y] = F_SKIN_OUTLINE
            for x in range(24, 31):
                pix[x, 54 + bob] = F_SHOE_MID
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (26, 27, 28) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_SOLE
            pix[23, 55 + bob] = F_SHOE_OUTLINE
            pix[31, 55 + bob] = F_SHOE_OUTLINE

        # Arm & Hand:
        arm_off = 0
        if frame == 1: arm_off = -1
        elif frame == 3: arm_off = 1
        ax = 31 + arm_off

        for y in (44, 45):
            pix[ax,     y + bob] = F_SKIN_OUTLINE
            pix[ax + 1, y + bob] = F_SKIN_SHADOW
            pix[ax + 2, y + bob] = F_SKIN_MID
            pix[ax + 3, y + bob] = F_SKIN_LIGHT
            pix[ax + 4, y + bob] = F_SKIN_OUTLINE

        for y in (46, 47, 48):
            pix[ax + 1, y + bob] = F_SKIN_OUTLINE
            pix[ax + 2, y + bob] = F_SKIN_MID
            pix[ax + 3, y + bob] = F_SKIN_LIGHT
            pix[ax + 4, y + bob] = F_SKIN_OUTLINE

        pix[ax + 1, 49 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 49 + bob] = F_SKIN_LIGHT
        pix[ax + 3, 49 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 49 + bob] = F_SKIN_OUTLINE

        pix[ax + 2, 50 + bob] = F_SKIN_OUTLINE
        pix[ax + 3, 50 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 50 + bob] = F_SKIN_OUTLINE

        pix[ax + 2, 51 + bob] = F_SKIN_OUTLINE
        pix[ax + 3, 51 + bob] = F_SKIN_OUTLINE

        return img

    # -------------------------------------------------------------
    # MALE EAST (Moderate 6-7px legs, barefoot, zero gaps)
    # -------------------------------------------------------------
    def draw_male(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(m_upper, (0, bob), m_upper)
        pix = img.load()

        # Seamless shirt blend at y=43:
        pix[27, 43 + bob] = (215, 105, 80, 255)
        pix[28, 43 + bob] = (245, 135, 105, 255)
        pix[35, 43 + bob] = (215, 105, 80, 255)

        # FAR LEG (Moderate 5-6px wide, barefoot)
        if frame == 0:
            for y in range(48 + bob, 54 + bob):
                pix[25, y] = M_FAR_SKIN_OUT
                pix[26, y] = M_FAR_SKIN_SH
                pix[27, y] = M_FAR_SKIN_MID
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[25, y] = M_FAR_SKIN_OUT
                pix[26, y] = M_FAR_SKIN_SH
                pix[27, y] = M_FAR_SKIN_MID
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for x in range(25, 31):
                pix[x, 57 + bob] = M_GROUND_OUT
        elif frame == 1:
            for y in range(48 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 25 - dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_MID
                pix[x_b + 4, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[21, y] = M_FAR_SKIN_OUT
                pix[22, y] = M_FAR_SKIN_MID
                pix[23, y] = M_FAR_SKIN_MID
                pix[24, y] = M_FAR_SKIN_MID
                pix[25, y] = M_FAR_SKIN_OUT
            for x in range(21, 26):
                pix[x, 56 + bob] = M_GROUND_OUT
        elif frame == 2:
            for y in range(48 + bob, 53 + bob):
                pix[26, y] = M_FAR_SKIN_OUT
                pix[27, y] = M_FAR_SKIN_SH
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[26, y] = M_FAR_SKIN_OUT
                pix[27, y] = M_FAR_SKIN_MID
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for x in range(26, 31):
                pix[x, 56 + bob] = M_GROUND_OUT
        elif frame == 3:
            for y in range(48 + bob, 53 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 30 + dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_MID
                pix[x_b + 4, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[32, y] = M_FAR_SKIN_OUT
                pix[33, y] = M_FAR_SKIN_MID
                pix[34, y] = M_FAR_SKIN_MID
                pix[35, y] = M_FAR_SKIN_MID
                pix[36, y] = M_FAR_SKIN_OUT
            for x in range(32, 37):
                pix[x, 57 + bob] = M_GROUND_OUT

        # SHORTS (Hips & Hem x=26..35)
        # y=44: x=27..35
        for x in range(27, 36):
            c = M_SH_MID
            if x == 27 or x == 35: c = M_SH_OUTLINE
            elif x in (28, 34): c = M_SH_SHADOW
            elif x in (30, 31): c = M_SH_LIGHT
            pix[x, 44 + bob] = c

        # y=45: x=26..35
        for x in range(26, 36):
            c = M_SH_MID
            if x == 26 or x == 35: c = M_SH_OUTLINE
            elif x in (27, 34): c = M_SH_SHADOW
            elif x in (29, 30): c = M_SH_LIGHT
            pix[x, 45 + bob] = c

        # y=46: x=26..35
        for x in range(26, 36):
            c = M_SH_MID
            if x == 26 or x == 35: c = M_SH_OUTLINE
            elif x in (27, 33, 34): c = M_SH_SHADOW
            elif x in (29, 30): c = M_SH_LIGHT
            pix[x, 46 + bob] = c

        # y=47: x=26..34
        for x in range(26, 35):
            c = M_SH_MID
            if x == 26 or x == 34: c = M_SH_OUTLINE
            elif x in (27, 31, 32, 33): c = M_SH_SHADOW
            pix[x, 47 + bob] = c

        # y=48: shorts hem back portion at x=25..31 (COMPLETELY CLOSES ANY GAP!)
        pix[25, 48 + bob] = M_SH_OUTLINE
        pix[26, 48 + bob] = M_SH_SHADOW
        pix[27, 48 + bob] = M_SH_MID
        pix[28, 48 + bob] = M_SH_MID
        pix[29, 48 + bob] = M_SH_SHADOW
        pix[30, 48 + bob] = M_SH_OUTLINE

        # NEAR LEG (Moderate 6-7px wide, barefoot)
        if frame == 0:
            # Thigh y=48..51: 7px wide (x=29..35)
            for y in range(48 + bob, 52 + bob):
                pix[29, y] = M_SKIN_OUTLINE
                pix[30, y] = M_SKIN_SHADOW
                pix[31, y] = M_SKIN_MID
                pix[32, y] = M_SKIN_LIGHT
                pix[33, y] = M_SKIN_LIGHT
                pix[34, y] = M_SKIN_MID
                pix[35, y] = M_SKIN_OUTLINE

            # Knee/Calf y=52..54: 6px wide (x=29..34)
            for y in range(52 + bob, 55 + bob):
                pix[29, y] = M_SKIN_OUTLINE
                pix[30, y] = M_SKIN_SHADOW
                pix[31, y] = M_SKIN_MID
                pix[32, y] = M_SKIN_LIGHT
                pix[33, y] = M_SKIN_MID
                pix[34, y] = M_SKIN_OUTLINE

            # Ankle y=55: 6px wide (x=29..34)
            pix[29, 55 + bob] = M_SKIN_OUTLINE
            pix[30, 55 + bob] = M_SKIN_SHADOW
            pix[31, 55 + bob] = M_SKIN_MID
            pix[32, 55 + bob] = M_SKIN_LIGHT
            pix[33, 55 + bob] = M_SKIN_MID
            pix[34, 55 + bob] = M_SKIN_OUTLINE

            # Barefoot foot y=56: (x=28..35)
            pix[28, 56 + bob] = M_SKIN_OUTLINE
            pix[29, 56 + bob] = M_SKIN_SHADOW
            pix[30, 56 + bob] = M_SKIN_MID
            pix[31, 56 + bob] = M_SKIN_LIGHT
            pix[32, 56 + bob] = M_SKIN_LIGHT
            pix[33, 56 + bob] = M_SKIN_LIGHT
            pix[34, 56 + bob] = M_SKIN_MID
            pix[35, 56 + bob] = M_SKIN_OUTLINE

            # Sole & Ground line y=57:
            for x in range(28, 36):
                pix[x, 57 + bob] = M_GROUND_OUT

        elif frame == 1:
            for y in range(48 + bob, 54 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 29 + dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_MID
                pix[x_b + 5, y] = M_SKIN_OUTLINE
            for x in range(31, 38):
                pix[x, 54 + bob] = M_SKIN_MID if x in (33, 34, 35) else M_SKIN_SHADOW
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (34, 35, 36) else M_SKIN_MID
                pix[x, 56 + bob] = M_SKIN_MID
                pix[x, 57 + bob] = M_GROUND_OUT
            pix[30, 56 + bob] = M_SKIN_OUTLINE
            pix[38, 56 + bob] = M_SKIN_OUTLINE

        elif frame == 2:
            for y in range(48 + bob, 54 + bob):
                pix[29, y] = M_SKIN_OUTLINE
                pix[30, y] = M_SKIN_SHADOW
                pix[31, y] = M_SKIN_MID
                pix[32, y] = M_SKIN_LIGHT
                pix[33, y] = M_SKIN_MID
                pix[34, y] = M_SKIN_OUTLINE
            for x in range(28, 35):
                pix[x, 54 + bob] = M_SKIN_MID
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (30, 31, 32) else M_SKIN_MID
                pix[x, 56 + bob] = M_SKIN_MID
                pix[x, 57 + bob] = M_GROUND_OUT
            pix[27, 56 + bob] = M_SKIN_OUTLINE
            pix[35, 56 + bob] = M_SKIN_OUTLINE

        elif frame == 3:
            for y in range(48 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 26 - dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_MID
                pix[x_b + 5, y] = M_SKIN_OUTLINE
            for x in range(23, 30):
                pix[x, 54 + bob] = M_SKIN_MID
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (25, 26, 27) else M_SKIN_MID
                pix[x, 56 + bob] = M_GROUND_OUT
            pix[22, 55 + bob] = M_SKIN_OUTLINE
            pix[30, 55 + bob] = M_SKIN_OUTLINE

        # Arm & Hand:
        arm_off = 0
        if frame == 1: arm_off = -1
        elif frame == 3: arm_off = 1
        ax = 30 + arm_off

        for y in (44, 45):
            pix[ax,     y + bob] = M_SKIN_OUTLINE
            pix[ax + 1, y + bob] = M_SKIN_SHADOW
            pix[ax + 2, y + bob] = M_SKIN_MID
            pix[ax + 3, y + bob] = M_SKIN_LIGHT
            pix[ax + 4, y + bob] = M_SKIN_OUTLINE

        for y in (46, 47, 48):
            pix[ax + 1, y + bob] = M_SKIN_OUTLINE
            pix[ax + 2, y + bob] = M_SKIN_MID
            pix[ax + 3, y + bob] = M_SKIN_LIGHT
            pix[ax + 4, y + bob] = M_SKIN_OUTLINE

        pix[ax + 1, 49 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 49 + bob] = M_SKIN_LIGHT
        pix[ax + 3, 49 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 49 + bob] = M_SKIN_OUTLINE

        pix[ax + 2, 50 + bob] = M_SKIN_OUTLINE
        pix[ax + 3, 50 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 50 + bob] = M_SKIN_OUTLINE

        pix[ax + 2, 51 + bob] = M_SKIN_OUTLINE
        pix[ax + 3, 51 + bob] = M_SKIN_OUTLINE

        return img

    # Export all 16 sprites and verify
    for f in range(4):
        fe = draw_female(f)
        fe.save(os.path.join(chars_dir, f"player_base_female_east_{f}.png"))
        fw = fe.transpose(Image.FLIP_LEFT_RIGHT)
        fw.save(os.path.join(chars_dir, f"player_base_female_west_{f}.png"))

        me = draw_male(f)
        me.save(os.path.join(chars_dir, f"player_base_male_east_{f}.png"))
        mw = me.transpose(Image.FLIP_LEFT_RIGHT)
        mw.save(os.path.join(chars_dir, f"player_base_male_west_{f}.png"))

    print("All 16 sprites generated successfully!")

if __name__ == "__main__":
    generate_proportioned_sprites()
