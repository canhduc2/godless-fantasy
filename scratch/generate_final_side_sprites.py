# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Generator for East and West 8-Dir Character Sprites
Studio: SquiRRels Studio (c) 2026

Features:
1. Bright, vibrant, 100% opaque upper body (no faded/pale colors).
2. Natural hair flow on female profile (no chopped flat horizontal hair lines).
3. Organic, seamless connection between torso, waist, and shorts for both male and female.
4. Slender, petite, dainty anime chibi legs ("nhỏ nhắn xinh xắn, không bị béo").
5. Female: Slender anime legs with petite dark sneakers and soles.
6. Male: Slender anime legs, 100% BAREFOOT (no shoes, cute toes & heels).
7. Full 4-frame walk cycle (Idle, Step 1, Passing, Step 2) for East and West (flipped).
"""
import os
from PIL import Image

def generate_all_master_side_sprites():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
    
    # =========================================================================
    # 1. FEMALE SPRITES
    # =========================================================================
    f_raw = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
    f_upper = f_raw.crop((0, 0, 64, 44))
    f_upix = f_upper.load()
    
    # 100% solid, bright unmultiplied upper body
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

    def draw_female_east(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(f_upper, (0, bob), f_upper)
        pix = img.load()
        
        # A. FLOWING HAIR CURLS (Eliminates flat horizontal cutoff!)
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
        pix[25, 45 + bob] = HAIR_DARK_OUT
        
        pix[20, 46 + bob] = HAIR_DARK_OUT
        pix[21, 46 + bob] = HAIR_SHADOW
        pix[22, 46 + bob] = HAIR_MID
        pix[23, 46 + bob] = HAIR_SHADOW
        pix[24, 46 + bob] = HAIR_DARK_OUT
        
        pix[21, 47 + bob] = HAIR_DARK_OUT
        pix[22, 47 + bob] = HAIR_SHADOW
        pix[23, 47 + bob] = HAIR_DARK_OUT
        
        pix[22, 48 + bob] = HAIR_DARK_OUT

        # B. FAR LEG (Background Layer - Slender, Dainty Left Leg)
        if frame == 0:
            for y in range(48 + bob, 54 + bob):
                pix[28, y] = F_FAR_SKIN_OUT
                pix[29, y] = F_FAR_SKIN_SH
                pix[30, y] = F_FAR_SKIN_MID
                pix[31, y] = F_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[27, y] = F_SHOE_OUTLINE
                pix[28, y] = F_FAR_SHOE_MID
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_FAR_SHOE_MID
                pix[31, y] = F_SHOE_OUTLINE
            for x in range(27, 32):
                pix[x, 57 + bob] = F_SHOE_SOLE
                
        elif frame == 1:
            # Step 1: Reaches backward
            for y in range(48 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 26 - dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[23, y] = F_SHOE_OUTLINE
                pix[24, y] = F_FAR_SHOE_MID
                pix[25, y] = F_FAR_SHOE_MID
                pix[26, y] = F_SHOE_OUTLINE
            for x in range(23, 27):
                pix[x, 56 + bob] = F_SHOE_SOLE

        elif frame == 2:
            # Passing
            for y in range(48 + bob, 53 + bob):
                pix[28, y] = F_FAR_SKIN_OUT
                pix[29, y] = F_FAR_SKIN_SH
                pix[30, y] = F_FAR_SKIN_MID
                pix[31, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[28, y] = F_SHOE_OUTLINE
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_FAR_SHOE_MID
                pix[31, y] = F_SHOE_OUTLINE
            for x in range(28, 32):
                pix[x, 56 + bob] = F_SHOE_SOLE

        elif frame == 3:
            # Step 2: Steps forward
            for y in range(48 + bob, 53 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 31 + dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[33, y] = F_SHOE_OUTLINE
                pix[34, y] = F_FAR_SHOE_MID
                pix[35, y] = F_FAR_SHOE_MID
                pix[36, y] = F_FAR_SHOE_MID
                pix[37, y] = F_SHOE_OUTLINE
            for x in range(33, 38):
                pix[x, 57 + bob] = F_SHOE_SOLE

        # C. SHORTS (Slender, Dainty Chibi Shorts at x=28..36)
        pix[28, 44 + bob] = F_SH_OUTLINE
        pix[29, 44 + bob] = F_SH_SHADOW
        pix[30, 44 + bob] = F_SH_MID
        pix[31, 44 + bob] = F_SH_LIGHT
        pix[32, 44 + bob] = F_SH_LIGHT
        pix[33, 44 + bob] = F_SH_MID
        pix[34, 44 + bob] = F_SH_MID
        pix[35, 44 + bob] = F_SH_SHADOW
        pix[36, 44 + bob] = F_SH_OUTLINE

        pix[27, 45 + bob] = F_SH_OUTLINE
        pix[28, 45 + bob] = F_SH_SHADOW
        pix[29, 45 + bob] = F_SH_MID
        pix[30, 45 + bob] = F_SH_LIGHT
        pix[31, 45 + bob] = F_SH_LIGHT
        pix[32, 45 + bob] = F_SH_MID
        pix[33, 45 + bob] = F_SH_MID
        pix[34, 45 + bob] = F_SH_MID
        pix[35, 45 + bob] = F_SH_SHADOW
        pix[36, 45 + bob] = F_SH_OUTLINE

        pix[27, 46 + bob] = F_SH_OUTLINE
        pix[28, 46 + bob] = F_SH_SHADOW
        pix[29, 46 + bob] = F_SH_MID
        pix[30, 46 + bob] = F_SH_LIGHT
        pix[31, 46 + bob] = F_SH_MID
        pix[32, 46 + bob] = F_SH_MID
        pix[33, 46 + bob] = F_SH_SHADOW
        pix[34, 46 + bob] = F_SH_SHADOW
        pix[35, 46 + bob] = F_SH_OUTLINE

        pix[27, 47 + bob] = F_SH_OUTLINE
        pix[28, 47 + bob] = F_SH_SHADOW
        pix[29, 47 + bob] = F_SH_MID
        pix[30, 47 + bob] = F_SH_MID
        pix[31, 47 + bob] = F_SH_SHADOW
        pix[32, 47 + bob] = F_SH_OUTLINE
        pix[33, 47 + bob] = F_SH_OUTLINE
        pix[34, 47 + bob] = F_SH_OUTLINE

        # D. FOREGROUND NEAR LEG (Slender, Dainty Anime Leg, 4-5px wide!)
        if frame == 0:
            for y in range(48 + bob, 52 + bob):
                pix[31, y] = F_SKIN_OUTLINE
                pix[32, y] = F_SKIN_SHADOW
                pix[33, y] = F_SKIN_MID
                pix[34, y] = F_SKIN_LIGHT
                pix[35, y] = F_SKIN_OUTLINE

            pix[31, 52 + bob] = F_SKIN_OUTLINE
            pix[32, 52 + bob] = F_SKIN_SHADOW
            pix[33, 52 + bob] = F_SKIN_MID
            pix[34, 52 + bob] = F_SKIN_LIGHT
            pix[35, 52 + bob] = F_SKIN_OUTLINE

            pix[31, 53 + bob] = F_SKIN_OUTLINE
            pix[32, 53 + bob] = F_SKIN_SHADOW
            pix[33, 53 + bob] = F_SKIN_MID
            pix[34, 53 + bob] = F_SKIN_LIGHT
            pix[35, 53 + bob] = F_SKIN_OUTLINE

            pix[31, 54 + bob] = F_SKIN_OUTLINE
            pix[32, 54 + bob] = F_SKIN_SHADOW
            pix[33, 54 + bob] = F_SKIN_MID
            pix[34, 54 + bob] = F_SKIN_LIGHT
            pix[35, 54 + bob] = F_SKIN_OUTLINE

            # Petite sneaker (7px long)
            pix[31, 55 + bob] = F_SHOE_OUTLINE
            pix[32, 55 + bob] = F_SHOE_SHADOW
            pix[33, 55 + bob] = F_SHOE_MID
            pix[34, 55 + bob] = F_SHOE_LIGHT
            pix[35, 55 + bob] = F_SHOE_OUTLINE

            pix[30, 56 + bob] = F_SHOE_OUTLINE
            pix[31, 56 + bob] = F_SHOE_SHADOW
            pix[32, 56 + bob] = F_SHOE_MID
            pix[33, 56 + bob] = F_SHOE_LIGHT
            pix[34, 56 + bob] = F_SHOE_LIGHT
            pix[35, 56 + bob] = F_SHOE_MID
            pix[36, 56 + bob] = F_SHOE_OUTLINE

            for x in range(30, 37):
                pix[x, 57 + bob] = F_SHOE_SOLE

        elif frame == 1:
            for y in range(48 + bob, 54 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 31 + dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_OUTLINE
            for x in range(32, 38):
                pix[x, 54 + bob] = F_SHOE_MID if x in (34, 35) else F_SHOE_SHADOW
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (35, 36) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[31, 56 + bob] = F_SHOE_OUTLINE
            pix[38, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 2:
            for y in range(48 + bob, 54 + bob):
                pix[31, y] = F_SKIN_OUTLINE
                pix[32, y] = F_SKIN_SHADOW
                pix[33, y] = F_SKIN_MID
                pix[34, y] = F_SKIN_LIGHT
                pix[35, y] = F_SKIN_OUTLINE
            for x in range(30, 36):
                pix[x, 54 + bob] = F_SHOE_MID
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (32, 33) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[29, 56 + bob] = F_SHOE_OUTLINE
            pix[35, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 3:
            for y in range(48 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 27 - dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_OUTLINE
            for x in range(25, 31):
                pix[x, 54 + bob] = F_SHOE_MID
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (27, 28) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_SOLE
            pix[24, 55 + bob] = F_SHOE_OUTLINE
            pix[30, 55 + bob] = F_SHOE_OUTLINE

        # E. FOREGROUND ARM & HAND (Slender anime arm, seamless at x=31..35)
        arm_off = 0
        if frame == 1: arm_off = -1
        elif frame == 3: arm_off = 1
        ax = 31 + arm_off
        
        pix[ax,     44 + bob] = F_SKIN_OUTLINE
        pix[ax + 1, 44 + bob] = F_SKIN_SHADOW
        pix[ax + 2, 44 + bob] = F_SKIN_MID
        pix[ax + 3, 44 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 44 + bob] = F_SKIN_OUTLINE
        
        pix[ax,     45 + bob] = F_SKIN_OUTLINE
        pix[ax + 1, 45 + bob] = F_SKIN_SHADOW
        pix[ax + 2, 45 + bob] = F_SKIN_MID
        pix[ax + 3, 45 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 45 + bob] = F_SKIN_OUTLINE
        
        pix[ax + 1, 46 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 46 + bob] = F_SKIN_MID
        pix[ax + 3, 46 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 46 + bob] = F_SKIN_OUTLINE
        
        pix[ax + 1, 47 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 47 + bob] = F_SKIN_MID
        pix[ax + 3, 47 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 47 + bob] = F_SKIN_OUTLINE
        
        # Petite hand
        pix[ax + 1, 48 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 48 + bob] = F_SKIN_MID
        pix[ax + 3, 48 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 48 + bob] = F_SKIN_OUTLINE
        
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

    # =========================================================================
    # 2. MALE SPRITES (BAREFOOT & SLENDER)
    # =========================================================================
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

    def draw_male_east(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(m_upper, (0, bob), m_upper)
        pix = img.load()
        
        # A. SHIRT HEM BLEND (y=43..44, seamless connection!)
        pix[27, 43 + bob] = (215, 105, 80, 255)
        pix[28, 43 + bob] = (245, 135, 105, 255)
        pix[35, 43 + bob] = (215, 105, 80, 255)
        
        # B. FAR LEG (Background Layer - Slender, Barefoot Left Leg)
        if frame == 0:
            for y in range(49 + bob, 54 + bob):
                pix[27, y] = M_FAR_SKIN_OUT
                pix[28, y] = M_FAR_SKIN_SH
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[26, y] = M_FAR_SKIN_OUT
                pix[27, y] = M_FAR_SKIN_SH
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for x in range(26, 31):
                pix[x, 57 + bob] = M_GROUND_OUT
                
        elif frame == 1:
            for y in range(49 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 25 - dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[22, y] = M_FAR_SKIN_OUT
                pix[23, y] = M_FAR_SKIN_MID
                pix[24, y] = M_FAR_SKIN_MID
                pix[25, y] = M_FAR_SKIN_OUT
            for x in range(22, 26):
                pix[x, 56 + bob] = M_GROUND_OUT

        elif frame == 2:
            for y in range(49 + bob, 53 + bob):
                pix[27, y] = M_FAR_SKIN_OUT
                pix[28, y] = M_FAR_SKIN_SH
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[27, y] = M_FAR_SKIN_OUT
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for x in range(27, 31):
                pix[x, 56 + bob] = M_GROUND_OUT

        elif frame == 3:
            for y in range(49 + bob, 53 + bob):
                dx = (y - 49 - bob) // 2
                x_b = 31 + dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[33, y] = M_FAR_SKIN_OUT
                pix[34, y] = M_FAR_SKIN_MID
                pix[35, y] = M_FAR_SKIN_MID
                pix[36, y] = M_FAR_SKIN_OUT
            for x in range(33, 37):
                pix[x, 57 + bob] = M_GROUND_OUT

        # C. SHORTS (Slender, x=27..35)
        pix[27, 44 + bob] = M_SH_OUTLINE
        pix[28, 44 + bob] = M_SH_SHADOW
        pix[29, 44 + bob] = M_SH_MID
        pix[30, 44 + bob] = M_SH_LIGHT
        pix[31, 44 + bob] = M_SH_LIGHT
        pix[32, 44 + bob] = M_SH_MID
        pix[33, 44 + bob] = M_SH_MID
        pix[34, 44 + bob] = M_SH_SHADOW
        pix[35, 44 + bob] = M_SH_OUTLINE

        pix[26, 45 + bob] = M_SH_OUTLINE
        pix[27, 45 + bob] = M_SH_SHADOW
        pix[28, 45 + bob] = M_SH_MID
        pix[29, 45 + bob] = M_SH_LIGHT
        pix[30, 45 + bob] = M_SH_LIGHT
        pix[31, 45 + bob] = M_SH_MID
        pix[32, 45 + bob] = M_SH_MID
        pix[33, 45 + bob] = M_SH_MID
        pix[34, 45 + bob] = M_SH_SHADOW
        pix[35, 45 + bob] = M_SH_OUTLINE

        pix[26, 46 + bob] = M_SH_OUTLINE
        pix[27, 46 + bob] = M_SH_SHADOW
        pix[28, 46 + bob] = M_SH_MID
        pix[29, 46 + bob] = M_SH_LIGHT
        pix[30, 46 + bob] = M_SH_MID
        pix[31, 46 + bob] = M_SH_MID
        pix[32, 46 + bob] = M_SH_MID
        pix[33, 46 + bob] = M_SH_SHADOW
        pix[34, 46 + bob] = M_SH_OUTLINE

        pix[26, 47 + bob] = M_SH_OUTLINE
        pix[27, 47 + bob] = M_SH_SHADOW
        pix[28, 47 + bob] = M_SH_MID
        pix[29, 47 + bob] = M_SH_MID
        pix[30, 47 + bob] = M_SH_MID
        pix[31, 47 + bob] = M_SH_SHADOW
        pix[32, 47 + bob] = M_SH_OUTLINE
        pix[33, 47 + bob] = M_SH_OUTLINE

        # D. FOREGROUND NEAR LEG (Right Barefoot Leg, Slender 4-5px wide!)
        if frame == 0:
            for y in range(48 + bob, 52 + bob):
                pix[30, y] = M_SKIN_OUTLINE
                pix[31, y] = M_SKIN_SHADOW
                pix[32, y] = M_SKIN_MID
                pix[33, y] = M_SKIN_LIGHT
                pix[34, y] = M_SKIN_OUTLINE

            pix[30, 52 + bob] = M_SKIN_OUTLINE
            pix[31, 52 + bob] = M_SKIN_SHADOW
            pix[32, 52 + bob] = M_SKIN_MID
            pix[33, 52 + bob] = M_SKIN_LIGHT
            pix[34, 52 + bob] = M_SKIN_OUTLINE

            pix[30, 53 + bob] = M_SKIN_OUTLINE
            pix[31, 53 + bob] = M_SKIN_SHADOW
            pix[32, 53 + bob] = M_SKIN_MID
            pix[33, 53 + bob] = M_SKIN_LIGHT
            pix[34, 53 + bob] = M_SKIN_OUTLINE

            pix[30, 54 + bob] = M_SKIN_OUTLINE
            pix[31, 54 + bob] = M_SKIN_SHADOW
            pix[32, 54 + bob] = M_SKIN_MID
            pix[33, 54 + bob] = M_SKIN_LIGHT
            pix[34, 54 + bob] = M_SKIN_OUTLINE

            # Petite Bare Foot (7px long, heel at x=29, toes at x=35)
            pix[30, 55 + bob] = M_SKIN_OUTLINE
            pix[31, 55 + bob] = M_SKIN_SHADOW
            pix[32, 55 + bob] = M_SKIN_MID
            pix[33, 55 + bob] = M_SKIN_LIGHT
            pix[34, 55 + bob] = M_SKIN_OUTLINE

            pix[29, 56 + bob] = M_SKIN_OUTLINE
            pix[30, 56 + bob] = M_SKIN_SHADOW
            pix[31, 56 + bob] = M_SKIN_MID
            pix[32, 56 + bob] = M_SKIN_LIGHT
            pix[33, 56 + bob] = M_SKIN_LIGHT
            pix[34, 56 + bob] = M_SKIN_MID
            pix[35, 56 + bob] = M_SKIN_OUTLINE

            for x in range(29, 36):
                pix[x, 57 + bob] = M_GROUND_OUT

        elif frame == 1:
            for y in range(48 + bob, 54 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 30 + dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_OUTLINE
            for x in range(31, 37):
                pix[x, 54 + bob] = M_SKIN_MID if x in (33, 34) else M_SKIN_SHADOW
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (34, 35) else M_SKIN_MID
                pix[x, 56 + bob] = M_SKIN_MID
                pix[x, 57 + bob] = M_GROUND_OUT
            pix[30, 56 + bob] = M_SKIN_OUTLINE
            pix[37, 56 + bob] = M_SKIN_OUTLINE

        elif frame == 2:
            for y in range(48 + bob, 54 + bob):
                pix[30, y]     = M_SKIN_OUTLINE
                pix[31, y]     = M_SKIN_SHADOW
                pix[32, y]     = M_SKIN_MID
                pix[33, y]     = M_SKIN_LIGHT
                pix[34, y]     = M_SKIN_OUTLINE
            for x in range(29, 35):
                pix[x, 54 + bob] = M_SKIN_MID
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (31, 32) else M_SKIN_MID
                pix[x, 56 + bob] = M_SKIN_MID
                pix[x, 57 + bob] = M_GROUND_OUT
            pix[28, 56 + bob] = M_SKIN_OUTLINE
            pix[35, 56 + bob] = M_SKIN_OUTLINE

        elif frame == 3:
            for y in range(48 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 27 - dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_OUTLINE
            for x in range(24, 30):
                pix[x, 54 + bob] = M_SKIN_MID
                pix[x, 55 + bob] = M_SKIN_LIGHT if x in (26, 27) else M_SKIN_MID
                pix[x, 56 + bob] = M_GROUND_OUT
            pix[23, 55 + bob] = M_SKIN_OUTLINE
            pix[30, 55 + bob] = M_SKIN_OUTLINE

        # E. FOREGROUND ARM & HAND (Slender, seamless at x=30..34)
        arm_off = 0
        if frame == 1: arm_off = -1
        elif frame == 3: arm_off = 1
        ax = 30 + arm_off
        
        pix[ax,     44 + bob] = M_SKIN_OUTLINE
        pix[ax + 1, 44 + bob] = M_SKIN_SHADOW
        pix[ax + 2, 44 + bob] = M_SKIN_MID
        pix[ax + 3, 44 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 44 + bob] = M_SKIN_OUTLINE
        
        pix[ax,     45 + bob] = M_SKIN_OUTLINE
        pix[ax + 1, 45 + bob] = M_SKIN_SHADOW
        pix[ax + 2, 45 + bob] = M_SKIN_MID
        pix[ax + 3, 45 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 45 + bob] = M_SKIN_OUTLINE
        
        pix[ax + 1, 46 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 46 + bob] = M_SKIN_MID
        pix[ax + 3, 46 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 46 + bob] = M_SKIN_OUTLINE
        
        pix[ax + 1, 47 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 47 + bob] = M_SKIN_MID
        pix[ax + 3, 47 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 47 + bob] = M_SKIN_OUTLINE
        
        # Petite hand
        pix[ax + 1, 48 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 48 + bob] = M_SKIN_MID
        pix[ax + 3, 48 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 48 + bob] = M_SKIN_OUTLINE
        
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

    # =========================================================================
    # 3. EXPORT ALL 4 FRAMES TO ASSETS/RESOURCES/CHARACTERS
    # =========================================================================
    for f in range(4):
        # Female East & West
        fe = draw_female_east(f)
        fe.save(os.path.join(chars_dir, f"player_base_female_east_{f}.png"))
        fw = fe.transpose(Image.FLIP_LEFT_RIGHT)
        fw.save(os.path.join(chars_dir, f"player_base_female_west_{f}.png"))
        
        # Male East & West
        me = draw_male_east(f)
        me.save(os.path.join(chars_dir, f"player_base_male_east_{f}.png"))
        mw = me.transpose(Image.FLIP_LEFT_RIGHT)
        mw.save(os.path.join(chars_dir, f"player_base_male_west_{f}.png"))
        
        print(f"Master side sprites exported: frame {f} (female & male, east & west)")

if __name__ == "__main__":
    generate_all_master_side_sprites()
