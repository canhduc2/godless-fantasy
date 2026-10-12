# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Generator for East and West 8-Dir Character Sprites
Studio: SquiRRels Studio (c) 2026
Full Anatomical Pixel Art: Seamless Upper Arm to Forearm/Hand, Organic Curvature,
Charcoal Shorts with Shading, Natural Anime Chibi Legs, and Stylish Sneaker Footwear.
"""
import os
from PIL import Image

def generate_all_side_sprites():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
    
    # =========================================================================
    # 1. FEMALE PALETTE & ASSETS
    # =========================================================================
    f_base = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
    f_upper = f_base.crop((0, 0, 64, 44))
    
    F_SKIN_LIGHT   = (252, 224, 204, 255) # Peach highlight
    F_SKIN_MID     = (244, 196, 170, 255) # Base anime skin
    F_SKIN_SHADOW  = (218, 156, 130, 255) # Warm Shadow
    F_SKIN_OUTLINE = (96, 48, 44, 255)   # Soft anime outline
    
    F_FAR_SKIN_MID = (205, 142, 118, 255)
    F_FAR_SKIN_SH  = (175, 115, 95, 255)
    F_FAR_SKIN_OUT = (75, 36, 34, 255)
    
    F_SH_LIGHT   = (86, 86, 92, 255)
    F_SH_MID     = (64, 64, 68, 255)
    F_SH_SHADOW  = (46, 46, 50, 255)
    F_SH_OUTLINE = (28, 26, 30, 255)
    F_SH_FAR     = (38, 38, 42, 255)
    
    F_SHOE_LIGHT   = (82, 78, 84, 255)
    F_SHOE_MID     = (56, 52, 58, 255)
    F_SHOE_SHADOW  = (40, 36, 42, 255)
    F_SHOE_OUTLINE = (26, 24, 28, 255)
    F_SHOE_SOLE    = (20, 18, 22, 255)
    F_FAR_SHOE_MID = (40, 36, 42, 255)
    F_FAR_SHOE_OUT = (22, 20, 24, 255)

    def draw_female_east(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(f_upper, (0, bob), f_upper)
        pix = img.load()
        
        # ---------------------------------------------------------------------
        # LAYER 1: FAR LEG (Left Leg in Background)
        # ---------------------------------------------------------------------
        if frame == 0:
            # Idle: Far leg standing slightly back (x=26..31)
            for y in range(48 + bob, 54 + bob):
                pix[26, y] = F_FAR_SKIN_OUT
                pix[27, y] = F_FAR_SKIN_SH
                pix[28, y] = F_FAR_SKIN_MID
                pix[29, y] = F_FAR_SKIN_MID
                pix[30, y] = F_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[25, y] = F_FAR_SHOE_OUT
                pix[26, y] = F_FAR_SHOE_MID
                pix[27, y] = F_FAR_SHOE_MID
                pix[28, y] = F_FAR_SHOE_MID
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_FAR_SHOE_OUT
            for x in range(25, 31):
                pix[x, 57 + bob] = F_SHOE_SOLE
                
        elif frame == 1:
            # Step 1: Far leg stretches backward (x=22..28)
            for y in range(48 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 24 - dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_MID
                pix[x_b + 4, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[21, y] = F_FAR_SHOE_OUT
                pix[22, y] = F_FAR_SHOE_MID
                pix[23, y] = F_FAR_SHOE_MID
                pix[24, y] = F_FAR_SHOE_MID
                pix[25, y] = F_FAR_SHOE_OUT
            for x in range(21, 26):
                pix[x, 56 + bob] = F_SHOE_SOLE

        elif frame == 2:
            # Passing: Far leg crossing mid-body (x=27..31)
            for y in range(48 + bob, 53 + bob):
                pix[27, y] = F_FAR_SKIN_OUT
                pix[28, y] = F_FAR_SKIN_SH
                pix[29, y] = F_FAR_SKIN_MID
                pix[30, y] = F_FAR_SKIN_MID
                pix[31, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[27, y] = F_FAR_SHOE_OUT
                pix[28, y] = F_FAR_SHOE_MID
                pix[29, y] = F_FAR_SHOE_MID
                pix[30, y] = F_FAR_SHOE_MID
                pix[31, y] = F_FAR_SHOE_OUT
            for x in range(27, 32):
                pix[x, 56 + bob] = F_SHOE_SOLE

        elif frame == 3:
            # Step 2: Far leg steps forward to the right (x=33..39)
            for y in range(48 + bob, 53 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 32 + dx
                pix[x_b, y]     = F_FAR_SKIN_OUT
                pix[x_b + 1, y] = F_FAR_SKIN_SH
                pix[x_b + 2, y] = F_FAR_SKIN_MID
                pix[x_b + 3, y] = F_FAR_SKIN_MID
                pix[x_b + 4, y] = F_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[34, y] = F_FAR_SHOE_OUT
                pix[35, y] = F_FAR_SHOE_MID
                pix[36, y] = F_FAR_SHOE_MID
                pix[37, y] = F_FAR_SHOE_MID
                pix[38, y] = F_FAR_SHOE_MID
                pix[39, y] = F_FAR_SHOE_OUT
            for x in range(34, 40):
                pix[x, 57 + bob] = F_SHOE_SOLE

        # ---------------------------------------------------------------------
        # LAYER 2: SHORTS (y=44..47, plump rounded hips, x=26..38)
        # ---------------------------------------------------------------------
        pix[27, 44 + bob] = F_SH_OUTLINE
        pix[28, 44 + bob] = F_SH_SHADOW
        pix[29, 44 + bob] = F_SH_MID
        pix[30, 44 + bob] = F_SH_MID
        pix[31, 44 + bob] = F_SH_LIGHT
        pix[32, 44 + bob] = F_SH_LIGHT
        pix[33, 44 + bob] = F_SH_MID
        pix[34, 44 + bob] = F_SH_MID
        pix[35, 44 + bob] = F_SH_LIGHT
        pix[36, 44 + bob] = F_SH_MID
        pix[37, 44 + bob] = F_SH_SHADOW
        pix[38, 44 + bob] = F_SH_OUTLINE

        pix[26, 45 + bob] = F_SH_OUTLINE
        pix[27, 45 + bob] = F_SH_SHADOW
        pix[28, 45 + bob] = F_SH_MID
        pix[29, 45 + bob] = F_SH_MID
        pix[30, 45 + bob] = F_SH_LIGHT
        pix[31, 45 + bob] = F_SH_LIGHT
        pix[32, 45 + bob] = F_SH_LIGHT
        pix[33, 45 + bob] = F_SH_MID
        pix[34, 45 + bob] = F_SH_MID
        pix[35, 45 + bob] = F_SH_LIGHT
        pix[36, 45 + bob] = F_SH_MID
        pix[37, 45 + bob] = F_SH_SHADOW
        pix[38, 45 + bob] = F_SH_OUTLINE

        pix[26, 46 + bob] = F_SH_OUTLINE
        pix[27, 46 + bob] = F_SH_SHADOW
        pix[28, 46 + bob] = F_SH_MID
        pix[29, 46 + bob] = F_SH_MID
        pix[30, 46 + bob] = F_SH_LIGHT
        pix[31, 46 + bob] = F_SH_LIGHT
        pix[32, 46 + bob] = F_SH_MID
        pix[33, 46 + bob] = F_SH_MID
        pix[34, 46 + bob] = F_SH_MID
        pix[35, 46 + bob] = F_SH_SHADOW
        pix[36, 46 + bob] = F_SH_SHADOW
        pix[37, 46 + bob] = F_SH_OUTLINE

        pix[26, 47 + bob] = F_SH_OUTLINE
        pix[27, 47 + bob] = F_SH_SHADOW
        pix[28, 47 + bob] = F_SH_MID
        pix[29, 47 + bob] = F_SH_MID
        pix[30, 47 + bob] = F_SH_MID
        pix[31, 47 + bob] = F_SH_MID
        pix[32, 47 + bob] = F_SH_SHADOW
        pix[33, 47 + bob] = F_SH_SHADOW
        pix[34, 47 + bob] = F_SH_OUTLINE
        pix[35, 47 + bob] = F_SH_OUTLINE
        pix[36, 47 + bob] = F_SH_OUTLINE

        # ---------------------------------------------------------------------
        # LAYER 3: FOREGROUND NEAR LEG (Right Leg, y=48..57)
        # ---------------------------------------------------------------------
        if frame == 0:
            # Idle: Slender standing anime leg
            for y in range(48 + bob, 52 + bob):
                pix[29, y] = F_SKIN_OUTLINE
                pix[30, y] = F_SKIN_SHADOW
                pix[31, y] = F_SKIN_MID
                pix[32, y] = F_SKIN_LIGHT
                pix[33, y] = F_SKIN_LIGHT
                pix[34, y] = F_SKIN_MID
                pix[35, y] = F_SKIN_LIGHT
                pix[36, y] = F_SKIN_MID
                pix[37, y] = F_SKIN_OUTLINE

            pix[29, 52 + bob] = F_SKIN_OUTLINE
            pix[30, 52 + bob] = F_SKIN_SHADOW
            pix[31, 52 + bob] = F_SKIN_MID
            pix[32, 52 + bob] = F_SKIN_LIGHT
            pix[33, 52 + bob] = F_SKIN_LIGHT
            pix[34, 52 + bob] = F_SKIN_MID
            pix[35, 52 + bob] = F_SKIN_LIGHT
            pix[36, 52 + bob] = F_SKIN_LIGHT
            pix[37, 52 + bob] = F_SKIN_OUTLINE

            pix[30, 53 + bob] = F_SKIN_OUTLINE
            pix[31, 53 + bob] = F_SKIN_SHADOW
            pix[32, 53 + bob] = F_SKIN_MID
            pix[33, 53 + bob] = F_SKIN_LIGHT
            pix[34, 53 + bob] = F_SKIN_LIGHT
            pix[35, 53 + bob] = F_SKIN_MID
            pix[36, 53 + bob] = F_SKIN_LIGHT
            pix[37, 53 + bob] = F_SKIN_OUTLINE

            pix[30, 54 + bob] = F_SKIN_OUTLINE
            pix[31, 54 + bob] = F_SKIN_SHADOW
            pix[32, 54 + bob] = F_SKIN_MID
            pix[33, 54 + bob] = F_SKIN_LIGHT
            pix[34, 54 + bob] = F_SKIN_LIGHT
            pix[35, 54 + bob] = F_SKIN_MID
            pix[36, 54 + bob] = F_SKIN_OUTLINE

            pix[29, 55 + bob] = F_SHOE_OUTLINE
            pix[30, 55 + bob] = F_SHOE_SHADOW
            pix[31, 55 + bob] = F_SHOE_MID
            pix[32, 55 + bob] = F_SHOE_LIGHT
            pix[33, 55 + bob] = F_SHOE_LIGHT
            pix[34, 55 + bob] = F_SHOE_MID
            pix[35, 55 + bob] = F_SHOE_LIGHT
            pix[36, 55 + bob] = F_SHOE_LIGHT
            pix[37, 55 + bob] = F_SHOE_OUTLINE

            pix[28, 56 + bob] = F_SHOE_OUTLINE
            pix[29, 56 + bob] = F_SHOE_SHADOW
            pix[30, 56 + bob] = F_SHOE_MID
            pix[31, 56 + bob] = F_SHOE_LIGHT
            pix[32, 56 + bob] = F_SHOE_LIGHT
            pix[33, 56 + bob] = F_SHOE_MID
            pix[34, 56 + bob] = F_SHOE_MID
            pix[35, 56 + bob] = F_SHOE_LIGHT
            pix[36, 56 + bob] = F_SHOE_LIGHT
            pix[37, 56 + bob] = F_SHOE_MID
            pix[38, 56 + bob] = F_SHOE_OUTLINE

            for x in range(28, 39):
                pix[x, 57 + bob] = F_SHOE_SOLE

        elif frame == 1:
            # Step 1: Forward stride (x=31..38)
            for y in range(48 + bob, 54 + bob):
                dx = (y - 48 - bob) // 2
                x_b = 30 + dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_LIGHT
                pix[x_b + 5, y] = F_SKIN_MID
                pix[x_b + 6, y] = F_SKIN_OUTLINE
            for x in range(32, 39):
                pix[x, 54 + bob] = F_SHOE_MID if x in (34, 35) else F_SHOE_SHADOW
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (35, 36) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[31, 56 + bob] = F_SHOE_OUTLINE
            pix[39, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 2:
            # Passing: Near leg vertical
            for y in range(48 + bob, 54 + bob):
                pix[29, y] = F_SKIN_OUTLINE
                pix[30, y] = F_SKIN_SHADOW
                pix[31, y] = F_SKIN_MID
                pix[32, y] = F_SKIN_LIGHT
                pix[33, y] = F_SKIN_LIGHT
                pix[34, y] = F_SKIN_MID
                pix[35, y] = F_SKIN_LIGHT
                pix[36, y] = F_SKIN_OUTLINE
            for x in range(29, 36):
                pix[x, 54 + bob] = F_SHOE_MID if x in (31, 32) else F_SHOE_SHADOW
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (32, 33) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_MID
                pix[x, 57 + bob] = F_SHOE_SOLE
            pix[28, 56 + bob] = F_SHOE_OUTLINE
            pix[36, 56 + bob] = F_SHOE_OUTLINE

        elif frame == 3:
            # Step 2: Backward reach (x=24..31)
            for y in range(48 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 26 - dx
                pix[x_b, y]     = F_SKIN_OUTLINE
                pix[x_b + 1, y] = F_SKIN_SHADOW
                pix[x_b + 2, y] = F_SKIN_MID
                pix[x_b + 3, y] = F_SKIN_LIGHT
                pix[x_b + 4, y] = F_SKIN_LIGHT
                pix[x_b + 5, y] = F_SKIN_MID
                pix[x_b + 6, y] = F_SKIN_OUTLINE
            for x in range(24, 31):
                pix[x, 54 + bob] = F_SHOE_MID
                pix[x, 55 + bob] = F_SHOE_LIGHT if x in (26, 27) else F_SHOE_MID
                pix[x, 56 + bob] = F_SHOE_SOLE
            pix[23, 55 + bob] = F_SHOE_OUTLINE
            pix[30, 55 + bob] = F_SHOE_OUTLINE

        # ---------------------------------------------------------------------
        # LAYER 4: FOREGROUND ARM & HAND (Seamlessly connects to y=43, x=31..35)
        # ---------------------------------------------------------------------
        arm_off = 0
        if frame == 1: arm_off = -1 # swings slightly back
        elif frame == 3: arm_off = 1 # swings slightly forward
        
        ax = 31 + arm_off
        
        # Forearm at y=44:
        pix[ax,     44 + bob] = F_SKIN_OUTLINE
        pix[ax + 1, 44 + bob] = F_SKIN_SHADOW
        pix[ax + 2, 44 + bob] = F_SKIN_MID
        pix[ax + 3, 44 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 44 + bob] = F_SKIN_OUTLINE
        
        # Forearm at y=45:
        pix[ax,     45 + bob] = F_SKIN_OUTLINE
        pix[ax + 1, 45 + bob] = F_SKIN_SHADOW
        pix[ax + 2, 45 + bob] = F_SKIN_MID
        pix[ax + 3, 45 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 45 + bob] = F_SKIN_OUTLINE
        
        # Forearm at y=46:
        pix[ax,     46 + bob] = F_SKIN_OUTLINE
        pix[ax + 1, 46 + bob] = F_SKIN_SHADOW
        pix[ax + 2, 46 + bob] = F_SKIN_MID
        pix[ax + 3, 46 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 46 + bob] = F_SKIN_OUTLINE
        
        # Wrist at y=47:
        pix[ax + 1, 47 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 47 + bob] = F_SKIN_MID
        pix[ax + 3, 47 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 47 + bob] = F_SKIN_OUTLINE
        
        # Hand & Palm at y=48:
        pix[ax + 1, 48 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 48 + bob] = F_SKIN_MID
        pix[ax + 3, 48 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 48 + bob] = F_SKIN_LIGHT
        pix[ax + 5, 48 + bob] = F_SKIN_OUTLINE
        
        # Fingers & Knuckles at y=49:
        pix[ax + 1, 49 + bob] = F_SKIN_OUTLINE
        pix[ax + 2, 49 + bob] = F_SKIN_MID
        pix[ax + 3, 49 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 49 + bob] = F_SKIN_LIGHT
        pix[ax + 5, 49 + bob] = F_SKIN_OUTLINE
        
        # Fingertips at y=50:
        pix[ax + 2, 50 + bob] = F_SKIN_OUTLINE
        pix[ax + 3, 50 + bob] = F_SKIN_LIGHT
        pix[ax + 4, 50 + bob] = F_SKIN_MID
        pix[ax + 5, 50 + bob] = F_SKIN_OUTLINE
        
        # Bottom round of hand at y=51:
        pix[ax + 3, 51 + bob] = F_SKIN_OUTLINE
        pix[ax + 4, 51 + bob] = F_SKIN_OUTLINE
        
        return img

    # =========================================================================
    # 2. MALE PALETTE & ASSETS
    # =========================================================================
    m_base = Image.open(os.path.join(chars_dir, "player_base_male_east_0.png")).convert("RGBA")
    m_upper = m_base.crop((0, 0, 64, 44))
    
    M_SKIN_LIGHT   = (250, 222, 202, 255)
    M_SKIN_MID     = (240, 190, 162, 255)
    M_SKIN_SHADOW  = (212, 146, 116, 255)
    M_SKIN_OUTLINE = (55, 30, 26, 255)
    
    M_FAR_SKIN_MID = (200, 136, 110, 255)
    M_FAR_SKIN_SH  = (170, 110, 88, 255)
    M_FAR_SKIN_OUT = (48, 24, 22, 255)
    
    M_SH_LIGHT   = (96, 96, 102, 255)
    M_SH_MID     = (72, 72, 76, 255)
    M_SH_SHADOW  = (50, 50, 54, 255)
    M_SH_OUTLINE = (24, 24, 26, 255)
    M_SH_FAR     = (42, 42, 46, 255)
    
    M_SHOE_LIGHT   = (80, 80, 86, 255)
    M_SHOE_MID     = (52, 52, 56, 255)
    M_SHOE_SHADOW  = (36, 36, 40, 255)
    M_SHOE_OUTLINE = (22, 22, 24, 255)
    M_SHOE_SOLE    = (18, 18, 20, 255)
    M_FAR_SHOE_MID = (38, 38, 42, 255)

    def draw_male_east(frame):
        bob = -1 if frame in (1, 3) else 0
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(m_upper, (0, bob), m_upper)
        pix = img.load()
        
        # ---------------------------------------------------------------------
        # LAYER 1: FAR LEG (Left Leg in Background)
        # ---------------------------------------------------------------------
        if frame == 0:
            for y in range(49 + bob, 54 + bob):
                pix[25, y] = M_FAR_SKIN_OUT
                pix[26, y] = M_FAR_SKIN_SH
                pix[27, y] = M_FAR_SKIN_MID
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_OUT
            for y in range(54 + bob, 57 + bob):
                pix[24, y] = M_SHOE_OUTLINE
                pix[25, y] = M_FAR_SHOE_MID
                pix[26, y] = M_FAR_SHOE_MID
                pix[27, y] = M_FAR_SHOE_MID
                pix[28, y] = M_FAR_SHOE_MID
                pix[29, y] = M_SHOE_OUTLINE
            for x in range(24, 30):
                pix[x, 57 + bob] = M_SHOE_SOLE
                
        elif frame == 1:
            for y in range(49 + bob, 53 + bob):
                dx = (52 + bob - y) // 2
                x_b = 23 - dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_MID
                pix[x_b + 4, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[20, y] = M_SHOE_OUTLINE
                pix[21, y] = M_FAR_SHOE_MID
                pix[22, y] = M_FAR_SHOE_MID
                pix[23, y] = M_FAR_SHOE_MID
                pix[24, y] = M_SHOE_OUTLINE
            for x in range(20, 25):
                pix[x, 56 + bob] = M_SHOE_SOLE

        elif frame == 2:
            for y in range(49 + bob, 53 + bob):
                pix[26, y] = M_FAR_SKIN_OUT
                pix[27, y] = M_FAR_SKIN_SH
                pix[28, y] = M_FAR_SKIN_MID
                pix[29, y] = M_FAR_SKIN_MID
                pix[30, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 56 + bob):
                pix[26, y] = M_SHOE_OUTLINE
                pix[27, y] = M_FAR_SHOE_MID
                pix[28, y] = M_FAR_SHOE_MID
                pix[29, y] = M_FAR_SHOE_MID
                pix[30, y] = M_SHOE_OUTLINE
            for x in range(26, 31):
                pix[x, 56 + bob] = M_SHOE_SOLE

        elif frame == 3:
            for y in range(49 + bob, 53 + bob):
                dx = (y - 49 - bob) // 2
                x_b = 31 + dx
                pix[x_b, y]     = M_FAR_SKIN_OUT
                pix[x_b + 1, y] = M_FAR_SKIN_SH
                pix[x_b + 2, y] = M_FAR_SKIN_MID
                pix[x_b + 3, y] = M_FAR_SKIN_MID
                pix[x_b + 4, y] = M_FAR_SKIN_OUT
            for y in range(53 + bob, 57 + bob):
                pix[33, y] = M_SHOE_OUTLINE
                pix[34, y] = M_FAR_SHOE_MID
                pix[35, y] = M_FAR_SHOE_MID
                pix[36, y] = M_FAR_SHOE_MID
                pix[37, y] = M_FAR_SHOE_MID
                pix[38, y] = M_SHOE_OUTLINE
            for x in range(33, 39):
                pix[x, 57 + bob] = M_SHOE_SOLE

        # ---------------------------------------------------------------------
        # LAYER 2: MALE SHORTS (y=44..48, x=25..37)
        # ---------------------------------------------------------------------
        pix[26, 44 + bob] = M_SH_OUTLINE
        pix[27, 44 + bob] = M_SH_SHADOW
        pix[28, 44 + bob] = M_SH_MID
        pix[29, 44 + bob] = M_SH_MID
        pix[30, 44 + bob] = M_SH_LIGHT
        pix[31, 44 + bob] = M_SH_LIGHT
        pix[32, 44 + bob] = M_SH_MID
        pix[33, 44 + bob] = M_SH_MID
        pix[34, 44 + bob] = M_SH_LIGHT
        pix[35, 44 + bob] = M_SH_MID
        pix[36, 44 + bob] = M_SH_SHADOW
        pix[37, 44 + bob] = M_SH_OUTLINE

        pix[25, 45 + bob] = M_SH_OUTLINE
        pix[26, 45 + bob] = M_SH_SHADOW
        pix[27, 45 + bob] = M_SH_MID
        pix[28, 45 + bob] = M_SH_MID
        pix[29, 45 + bob] = M_SH_LIGHT
        pix[30, 45 + bob] = M_SH_LIGHT
        pix[31, 45 + bob] = M_SH_LIGHT
        pix[32, 45 + bob] = M_SH_MID
        pix[33, 45 + bob] = M_SH_MID
        pix[34, 45 + bob] = M_SH_LIGHT
        pix[35, 45 + bob] = M_SH_MID
        pix[36, 45 + bob] = M_SH_SHADOW
        pix[37, 45 + bob] = M_SH_OUTLINE

        pix[25, 46 + bob] = M_SH_OUTLINE
        pix[26, 46 + bob] = M_SH_SHADOW
        pix[27, 46 + bob] = M_SH_MID
        pix[28, 46 + bob] = M_SH_MID
        pix[29, 46 + bob] = M_SH_LIGHT
        pix[30, 46 + bob] = M_SH_LIGHT
        pix[31, 46 + bob] = M_SH_MID
        pix[32, 46 + bob] = M_SH_MID
        pix[33, 46 + bob] = M_SH_MID
        pix[34, 46 + bob] = M_SH_SHADOW
        pix[35, 46 + bob] = M_SH_SHADOW
        pix[36, 46 + bob] = M_SH_OUTLINE

        pix[25, 47 + bob] = M_SH_OUTLINE
        pix[26, 47 + bob] = M_SH_SHADOW
        pix[27, 47 + bob] = M_SH_MID
        pix[28, 47 + bob] = M_SH_MID
        pix[29, 47 + bob] = M_SH_MID
        pix[30, 47 + bob] = M_SH_MID
        pix[31, 47 + bob] = M_SH_MID
        pix[32, 47 + bob] = M_SH_SHADOW
        pix[33, 47 + bob] = M_SH_SHADOW
        pix[34, 47 + bob] = M_SH_OUTLINE
        pix[35, 47 + bob] = M_SH_OUTLINE

        pix[25, 48 + bob] = M_SH_OUTLINE
        pix[26, 48 + bob] = M_SH_SHADOW
        pix[27, 48 + bob] = M_SH_MID
        pix[28, 48 + bob] = M_SH_MID
        pix[29, 48 + bob] = M_SH_MID
        pix[30, 48 + bob] = M_SH_SHADOW
        pix[31, 48 + bob] = M_SH_OUTLINE
        pix[32, 48 + bob] = M_SH_OUTLINE

        # ---------------------------------------------------------------------
        # LAYER 3: FOREGROUND NEAR LEG (Right Leg, y=49..57, x=28..36)
        # ---------------------------------------------------------------------
        if frame == 0:
            # IDLE: Muscular chibi anime leg
            for y in range(49 + bob, 52 + bob):
                pix[28, y] = M_SKIN_OUTLINE
                pix[29, y] = M_SKIN_SHADOW
                pix[30, y] = M_SKIN_MID
                pix[31, y] = M_SKIN_LIGHT
                pix[32, y] = M_SKIN_LIGHT
                pix[33, y] = M_SKIN_MID
                pix[34, y] = M_SKIN_LIGHT
                pix[35, y] = M_SKIN_MID
                pix[36, y] = M_SKIN_OUTLINE

            pix[28, 52 + bob] = M_SKIN_OUTLINE
            pix[29, 52 + bob] = M_SKIN_SHADOW
            pix[30, 52 + bob] = M_SKIN_MID
            pix[31, 52 + bob] = M_SKIN_LIGHT
            pix[32, 52 + bob] = M_SKIN_LIGHT
            pix[33, 52 + bob] = M_SKIN_MID
            pix[34, 52 + bob] = M_SKIN_LIGHT
            pix[35, 52 + bob] = M_SKIN_LIGHT
            pix[36, 52 + bob] = M_SKIN_OUTLINE

            pix[29, 53 + bob] = M_SKIN_OUTLINE
            pix[30, 53 + bob] = M_SKIN_SHADOW
            pix[31, 53 + bob] = M_SKIN_MID
            pix[32, 53 + bob] = M_SKIN_LIGHT
            pix[33, 53 + bob] = M_SKIN_LIGHT
            pix[34, 53 + bob] = M_SKIN_MID
            pix[35, 53 + bob] = M_SKIN_LIGHT
            pix[36, 53 + bob] = M_SKIN_OUTLINE

            pix[29, 54 + bob] = M_SKIN_OUTLINE
            pix[30, 54 + bob] = M_SKIN_SHADOW
            pix[31, 54 + bob] = M_SKIN_MID
            pix[32, 54 + bob] = M_SKIN_LIGHT
            pix[33, 54 + bob] = M_SKIN_LIGHT
            pix[34, 54 + bob] = M_SKIN_MID
            pix[35, 54 + bob] = M_SKIN_OUTLINE

            pix[29, 55 + bob] = M_SHOE_OUTLINE
            pix[30, 55 + bob] = M_SHOE_SHADOW
            pix[31, 55 + bob] = M_SHOE_MID
            pix[32, 55 + bob] = M_SHOE_LIGHT
            pix[33, 55 + bob] = M_SHOE_LIGHT
            pix[34, 55 + bob] = M_SHOE_MID
            pix[35, 55 + bob] = M_SHOE_LIGHT
            pix[36, 55 + bob] = M_SHOE_OUTLINE

            pix[27, 56 + bob] = M_SHOE_OUTLINE
            pix[28, 56 + bob] = M_SHOE_SHADOW
            pix[29, 56 + bob] = M_SHOE_MID
            pix[30, 56 + bob] = M_SHOE_LIGHT
            pix[31, 56 + bob] = M_SHOE_LIGHT
            pix[32, 56 + bob] = M_SHOE_MID
            pix[33, 56 + bob] = M_SHOE_MID
            pix[34, 56 + bob] = M_SHOE_LIGHT
            pix[35, 56 + bob] = M_SHOE_LIGHT
            pix[36, 56 + bob] = M_SHOE_MID
            pix[37, 56 + bob] = M_SHOE_OUTLINE

            for x in range(27, 38):
                pix[x, 57 + bob] = M_SHOE_SOLE

        elif frame == 1:
            for y in range(49 + bob, 54 + bob):
                dx = (y - 49 - bob) // 2
                x_b = 30 + dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_LIGHT
                pix[x_b + 5, y] = M_SKIN_MID
                pix[x_b + 6, y] = M_SKIN_OUTLINE
            for x in range(31, 38):
                pix[x, 54 + bob] = M_SHOE_MID if x in (33, 34) else M_SHOE_SHADOW
                pix[x, 55 + bob] = M_SHOE_LIGHT if x in (34, 35) else M_SHOE_MID
                pix[x, 56 + bob] = M_SHOE_MID
                pix[x, 57 + bob] = M_SHOE_SOLE
            pix[30, 56 + bob] = M_SHOE_OUTLINE
            pix[37, 56 + bob] = M_SHOE_OUTLINE

        elif frame == 2:
            for y in range(49 + bob, 54 + bob):
                pix[28, y]     = M_SKIN_OUTLINE
                pix[29, y]     = M_SKIN_SHADOW
                pix[30, y]     = M_SKIN_MID
                pix[31, y]     = M_SKIN_LIGHT
                pix[32, y]     = M_SKIN_LIGHT
                pix[33, y]     = M_SKIN_MID
                pix[34, y]     = M_SKIN_LIGHT
                pix[35, y]     = M_SKIN_OUTLINE
            for x in range(28, 35):
                pix[x, 54 + bob] = M_SHOE_MID
                pix[x, 55 + bob] = M_SHOE_LIGHT if x in (30, 31) else M_SHOE_MID
                pix[x, 56 + bob] = M_SHOE_MID
                pix[x, 57 + bob] = M_SHOE_SOLE
            pix[27, 56 + bob] = M_SHOE_OUTLINE
            pix[35, 56 + bob] = M_SHOE_OUTLINE

        elif frame == 3:
            for y in range(49 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_b = 26 - dx
                pix[x_b, y]     = M_SKIN_OUTLINE
                pix[x_b + 1, y] = M_SKIN_SHADOW
                pix[x_b + 2, y] = M_SKIN_MID
                pix[x_b + 3, y] = M_SKIN_LIGHT
                pix[x_b + 4, y] = M_SKIN_LIGHT
                pix[x_b + 5, y] = M_SKIN_MID
                pix[x_b + 6, y] = M_SKIN_OUTLINE
            for x in range(23, 30):
                pix[x, 54 + bob] = M_SHOE_MID
                pix[x, 55 + bob] = M_SHOE_LIGHT if x in (25, 26) else M_SHOE_MID
                pix[x, 56 + bob] = M_SHOE_SOLE
            pix[22, 55 + bob] = M_SHOE_OUTLINE
            pix[29, 55 + bob] = M_SHOE_OUTLINE

        # ---------------------------------------------------------------------
        # LAYER 4: MALE FOREGROUND ARM & HAND (Connects to y=43, x=30..34)
        # ---------------------------------------------------------------------
        arm_off = 0
        if frame == 1: arm_off = -1
        elif frame == 3: arm_off = 1
        
        ax = 30 + arm_off
        
        # Forearm at y=44:
        pix[ax,     44 + bob] = M_SKIN_OUTLINE
        pix[ax + 1, 44 + bob] = M_SKIN_SHADOW
        pix[ax + 2, 44 + bob] = M_SKIN_MID
        pix[ax + 3, 44 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 44 + bob] = M_SKIN_OUTLINE
        
        # Forearm at y=45:
        pix[ax,     45 + bob] = M_SKIN_OUTLINE
        pix[ax + 1, 45 + bob] = M_SKIN_SHADOW
        pix[ax + 2, 45 + bob] = M_SKIN_MID
        pix[ax + 3, 45 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 45 + bob] = M_SKIN_OUTLINE
        
        # Forearm at y=46:
        pix[ax,     46 + bob] = M_SKIN_OUTLINE
        pix[ax + 1, 46 + bob] = M_SKIN_SHADOW
        pix[ax + 2, 46 + bob] = M_SKIN_MID
        pix[ax + 3, 46 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 46 + bob] = M_SKIN_OUTLINE
        
        # Wrist at y=47:
        pix[ax + 1, 47 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 47 + bob] = M_SKIN_MID
        pix[ax + 3, 47 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 47 + bob] = M_SKIN_OUTLINE
        
        # Hand & Palm at y=48:
        pix[ax + 1, 48 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 48 + bob] = M_SKIN_MID
        pix[ax + 3, 48 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 48 + bob] = M_SKIN_LIGHT
        pix[ax + 5, 48 + bob] = M_SKIN_OUTLINE
        
        # Fingers & Knuckles at y=49:
        pix[ax + 1, 49 + bob] = M_SKIN_OUTLINE
        pix[ax + 2, 49 + bob] = M_SKIN_MID
        pix[ax + 3, 49 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 49 + bob] = M_SKIN_LIGHT
        pix[ax + 5, 49 + bob] = M_SKIN_OUTLINE
        
        # Fingertips at y=50:
        pix[ax + 2, 50 + bob] = M_SKIN_OUTLINE
        pix[ax + 3, 50 + bob] = M_SKIN_LIGHT
        pix[ax + 4, 50 + bob] = M_SKIN_MID
        pix[ax + 5, 50 + bob] = M_SKIN_OUTLINE
        
        # Bottom round of hand at y=51:
        pix[ax + 3, 51 + bob] = M_SKIN_OUTLINE
        pix[ax + 4, 51 + bob] = M_SKIN_OUTLINE
        
        return img

    # =========================================================================
    # 3. WRITE TO ASSETS/RESOURCES/CHARACTERS
    # =========================================================================
    for f in range(4):
        # Female East & West
        f_east = draw_female_east(f)
        f_east.save(os.path.join(chars_dir, f"player_base_female_east_{f}.png"))
        f_west = f_east.transpose(Image.FLIP_LEFT_RIGHT)
        f_west.save(os.path.join(chars_dir, f"player_base_female_west_{f}.png"))
        
        # Male East & West
        m_east = draw_male_east(f)
        m_east.save(os.path.join(chars_dir, f"player_base_male_east_{f}.png"))
        m_west = m_east.transpose(Image.FLIP_LEFT_RIGHT)
        m_west.save(os.path.join(chars_dir, f"player_base_male_west_{f}.png"))
        
        print(f"Rendered and saved 4-directional side frames (frame {f})")

if __name__ == "__main__":
    generate_all_side_sprites()
