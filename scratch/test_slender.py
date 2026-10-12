# -*- coding: utf-8 -*-
"""
Godless Fantasy - Dainty, Slender, Petite Anime Chibi Legs & Hair Flow Fix
Studio: SquiRRels Studio (c) 2026
"""
import os
from PIL import Image

def test_slender_duo():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
    
    # =========================================================================
    # 1. FEMALE
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
                
    # Sample hair colors from f_upper at y=40..43
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
    
    f_canvas = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    f_canvas.paste(f_upper, (0, 0), f_upper)
    fpix = f_canvas.load()
    
    # -------------------------------------------------------------------------
    # A. NATURAL FLOWING HAIR CURLS (y=44..49, x=19..28)
    # Eliminates the chopped flat line at the bottom of the hair!
    # -------------------------------------------------------------------------
    # y=44: hair curl continues down
    fpix[19, 44] = HAIR_DARK_OUT
    fpix[20, 44] = HAIR_DARK_OUT
    fpix[21, 44] = HAIR_SHADOW
    fpix[22, 44] = HAIR_MID
    fpix[23, 44] = HAIR_MID
    fpix[24, 44] = HAIR_SHADOW
    fpix[25, 44] = HAIR_MID
    fpix[26, 44] = HAIR_SHADOW
    fpix[27, 44] = HAIR_DARK_OUT
    
    # y=45: tapering hair locks
    fpix[20, 45] = HAIR_DARK_OUT
    fpix[21, 45] = HAIR_SHADOW
    fpix[22, 45] = HAIR_MID
    fpix[23, 45] = HAIR_MID
    fpix[24, 45] = HAIR_SHADOW
    fpix[25, 45] = HAIR_DARK_OUT
    
    # y=46: cute anime curls
    fpix[20, 46] = HAIR_DARK_OUT
    fpix[21, 46] = HAIR_SHADOW
    fpix[22, 46] = HAIR_MID
    fpix[23, 46] = HAIR_SHADOW
    fpix[24, 46] = HAIR_DARK_OUT
    
    # y=47: curl tips
    fpix[21, 47] = HAIR_DARK_OUT
    fpix[22, 47] = HAIR_SHADOW
    fpix[23, 47] = HAIR_DARK_OUT
    
    # y=48: soft tip
    fpix[22, 48] = HAIR_DARK_OUT

    # -------------------------------------------------------------------------
    # B. FAR LEG (Background Layer - Left Leg, dainty & slender at x=28..31)
    # -------------------------------------------------------------------------
    for y in range(48, 54):
        fpix[28, y] = F_FAR_SKIN_OUT
        fpix[29, y] = F_FAR_SKIN_SH
        fpix[30, y] = F_FAR_SKIN_MID
        fpix[31, y] = F_FAR_SKIN_OUT
    # Far shoe (petite):
    for y in range(54, 57):
        fpix[27, y] = F_SHOE_OUTLINE
        fpix[28, y] = F_FAR_SHOE_MID
        fpix[29, y] = F_FAR_SHOE_MID
        fpix[30, y] = F_FAR_SHOE_MID
        fpix[31, y] = F_SHOE_OUTLINE
    for x in range(27, 32):
        fpix[x, 57] = F_SHOE_SOLE

    # -------------------------------------------------------------------------
    # C. SHORTS (Dainty, slender chibi hips at x=28..36)
    # Seamless transition from croptop & lower back
    # -------------------------------------------------------------------------
    # y=44: Waistline (connects to shirt/back)
    fpix[28, 44] = F_SH_OUTLINE
    fpix[29, 44] = F_SH_SHADOW
    fpix[30, 44] = F_SH_MID
    fpix[31, 44] = F_SH_LIGHT
    fpix[32, 44] = F_SH_LIGHT
    fpix[33, 44] = F_SH_MID
    fpix[34, 44] = F_SH_MID
    fpix[35, 44] = F_SH_SHADOW
    fpix[36, 44] = F_SH_OUTLINE

    # y=45: Upper hip curve
    fpix[27, 45] = F_SH_OUTLINE
    fpix[28, 45] = F_SH_SHADOW
    fpix[29, 45] = F_SH_MID
    fpix[30, 45] = F_SH_LIGHT
    fpix[31, 45] = F_SH_LIGHT
    fpix[32, 45] = F_SH_MID
    fpix[33, 45] = F_SH_MID
    fpix[34, 45] = F_SH_MID
    fpix[35, 45] = F_SH_SHADOW
    fpix[36, 45] = F_SH_OUTLINE

    # y=46: Mid hip & glute
    fpix[27, 46] = F_SH_OUTLINE
    fpix[28, 46] = F_SH_SHADOW
    fpix[29, 46] = F_SH_MID
    fpix[30, 46] = F_SH_LIGHT
    fpix[31, 46] = F_SH_MID
    fpix[32, 46] = F_SH_MID
    fpix[33, 46] = F_SH_SHADOW
    fpix[34, 46] = F_SH_SHADOW
    fpix[35, 46] = F_SH_OUTLINE

    # y=47: Shorts hem (slender opening)
    fpix[27, 47] = F_SH_OUTLINE
    fpix[28, 47] = F_SH_SHADOW
    fpix[29, 47] = F_SH_MID
    fpix[30, 47] = F_SH_MID
    fpix[31, 47] = F_SH_SHADOW
    fpix[32, 47] = F_SH_OUTLINE
    fpix[33, 47] = F_SH_OUTLINE
    fpix[34, 47] = F_SH_OUTLINE

    # -------------------------------------------------------------------------
    # D. NEAR LEG (Slender, Dainty Anime Leg, 4-5px wide, x=31..35)
    # -------------------------------------------------------------------------
    # Thigh (y=48..51, x=31..35, exactly 4-5px wide!)
    for y in range(48, 52):
        fpix[31, y] = F_SKIN_OUTLINE
        fpix[32, y] = F_SKIN_SHADOW
        fpix[33, y] = F_SKIN_MID
        fpix[34, y] = F_SKIN_LIGHT
        fpix[35, y] = F_SKIN_OUTLINE

    # Knee (y=52..53, rounded, slender)
    fpix[31, 52] = F_SKIN_OUTLINE
    fpix[32, 52] = F_SKIN_SHADOW
    fpix[33, 52] = F_SKIN_MID
    fpix[34, 52] = F_SKIN_LIGHT
    fpix[35, 52] = F_SKIN_OUTLINE

    fpix[31, 53] = F_SKIN_OUTLINE
    fpix[32, 53] = F_SKIN_SHADOW
    fpix[33, 53] = F_SKIN_MID
    fpix[34, 53] = F_SKIN_LIGHT
    fpix[35, 53] = F_SKIN_OUTLINE

    # Shin & Ankle (y=54..55, slim anime taper to 4px)
    fpix[31, 54] = F_SKIN_OUTLINE
    fpix[32, 54] = F_SKIN_SHADOW
    fpix[33, 54] = F_SKIN_MID
    fpix[34, 54] = F_SKIN_LIGHT
    fpix[35, 54] = F_SKIN_OUTLINE

    # Petite Sneaker (y=55..57, heel at x=30, toe at x=36 - only 7px long!)
    fpix[31, 55] = F_SHOE_OUTLINE
    fpix[32, 55] = F_SHOE_SHADOW
    fpix[33, 55] = F_SHOE_MID
    fpix[34, 55] = F_SHOE_LIGHT
    fpix[35, 55] = F_SHOE_OUTLINE

    fpix[30, 56] = F_SHOE_OUTLINE # heel
    fpix[31, 56] = F_SHOE_SHADOW
    fpix[32, 56] = F_SHOE_MID
    fpix[33, 56] = F_SHOE_LIGHT
    fpix[34, 56] = F_SHOE_LIGHT
    fpix[35, 56] = F_SHOE_MID
    fpix[36, 56] = F_SHOE_OUTLINE # rounded toe cap

    for x in range(30, 37):
        fpix[x, 57] = F_SHOE_SOLE

    # -------------------------------------------------------------------------
    # E. FOREGROUND ARM & HAND (Slender, seamless at x=32..35)
    # -------------------------------------------------------------------------
    fpix[31, 44] = F_SKIN_OUTLINE
    fpix[32, 44] = F_SKIN_SHADOW
    fpix[33, 44] = F_SKIN_MID
    fpix[34, 44] = F_SKIN_LIGHT
    fpix[35, 44] = F_SKIN_OUTLINE

    fpix[31, 45] = F_SKIN_OUTLINE
    fpix[32, 45] = F_SKIN_SHADOW
    fpix[33, 45] = F_SKIN_MID
    fpix[34, 45] = F_SKIN_LIGHT
    fpix[35, 45] = F_SKIN_OUTLINE

    fpix[32, 46] = F_SKIN_OUTLINE
    fpix[33, 46] = F_SKIN_MID
    fpix[34, 46] = F_SKIN_LIGHT
    fpix[35, 46] = F_SKIN_OUTLINE

    fpix[32, 47] = F_SKIN_OUTLINE
    fpix[33, 47] = F_SKIN_MID
    fpix[34, 47] = F_SKIN_LIGHT
    fpix[35, 47] = F_SKIN_OUTLINE

    # Hand & petite fingers (y=48..50)
    fpix[32, 48] = F_SKIN_OUTLINE
    fpix[33, 48] = F_SKIN_MID
    fpix[34, 48] = F_SKIN_LIGHT
    fpix[35, 48] = F_SKIN_OUTLINE

    fpix[32, 49] = F_SKIN_OUTLINE
    fpix[33, 49] = F_SKIN_LIGHT
    fpix[34, 49] = F_SKIN_LIGHT
    fpix[35, 49] = F_SKIN_OUTLINE

    fpix[33, 50] = F_SKIN_OUTLINE
    fpix[34, 50] = F_SKIN_LIGHT
    fpix[35, 50] = F_SKIN_OUTLINE

    fpix[33, 51] = F_SKIN_OUTLINE
    fpix[34, 51] = F_SKIN_OUTLINE

    f_canvas.save("scratch/test_slender_female.png")
    print("Saved test_slender_female.png")

    # =========================================================================
    # 2. MALE
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

    m_canvas = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    m_canvas.paste(m_upper, (0, 0), m_upper)
    mpix = m_canvas.load()
    
    # Smooth connection between shirt and shorts at y=43..44:
    # Blend shirt hem
    mpix[27, 43] = (215, 105, 80, 255) # shirt bottom shadow
    mpix[28, 43] = (245, 135, 105, 255)
    mpix[35, 43] = (215, 105, 80, 255)
    
    # Far Leg (slender, standing slightly behind at x=27..30)
    for y in range(49, 54):
        mpix[27, y] = M_FAR_SKIN_OUT
        mpix[28, y] = M_FAR_SKIN_SH
        mpix[29, y] = M_FAR_SKIN_MID
        mpix[30, y] = M_FAR_SKIN_OUT
    for y in range(54, 57):
        mpix[26, y] = M_FAR_SKIN_OUT
        mpix[27, y] = M_FAR_SKIN_SH
        mpix[28, y] = M_FAR_SKIN_MID
        mpix[29, y] = M_FAR_SKIN_MID
        mpix[30, y] = M_FAR_SKIN_OUT
    for x in range(26, 31):
        mpix[x, 57] = M_GROUND_OUT

    # Shorts (slender, x=27..35)
    mpix[27, 44] = M_SH_OUTLINE
    mpix[28, 44] = M_SH_SHADOW
    mpix[29, 44] = M_SH_MID
    mpix[30, 44] = M_SH_LIGHT
    mpix[31, 44] = M_SH_LIGHT
    mpix[32, 44] = M_SH_MID
    mpix[33, 44] = M_SH_MID
    mpix[34, 44] = M_SH_SHADOW
    mpix[35, 44] = M_SH_OUTLINE

    mpix[26, 45] = M_SH_OUTLINE
    mpix[27, 45] = M_SH_SHADOW
    mpix[28, 45] = M_SH_MID
    mpix[29, 45] = M_SH_LIGHT
    mpix[30, 45] = M_SH_LIGHT
    mpix[31, 45] = M_SH_MID
    mpix[32, 45] = M_SH_MID
    mpix[33, 45] = M_SH_MID
    mpix[34, 45] = M_SH_SHADOW
    mpix[35, 45] = M_SH_OUTLINE

    mpix[26, 46] = M_SH_OUTLINE
    mpix[27, 46] = M_SH_SHADOW
    mpix[28, 46] = M_SH_MID
    mpix[29, 46] = M_SH_LIGHT
    mpix[30, 46] = M_SH_MID
    mpix[31, 46] = M_SH_MID
    mpix[32, 46] = M_SH_MID
    mpix[33, 46] = M_SH_SHADOW
    mpix[34, 46] = M_SH_OUTLINE

    mpix[26, 47] = M_SH_OUTLINE
    mpix[27, 47] = M_SH_SHADOW
    mpix[28, 47] = M_SH_MID
    mpix[29, 47] = M_SH_MID
    mpix[30, 47] = M_SH_MID
    mpix[31, 47] = M_SH_SHADOW
    mpix[32, 47] = M_SH_OUTLINE
    mpix[33, 47] = M_SH_OUTLINE

    # Near Leg (Slender, Dainty Chibi Bare Leg, 4-5px wide, x=30..34)
    for y in range(48, 52):
        mpix[30, y] = M_SKIN_OUTLINE
        mpix[31, y] = M_SKIN_SHADOW
        mpix[32, y] = M_SKIN_MID
        mpix[33, y] = M_SKIN_LIGHT
        mpix[34, y] = M_SKIN_OUTLINE

    mpix[30, 52] = M_SKIN_OUTLINE
    mpix[31, 52] = M_SKIN_SHADOW
    mpix[32, 52] = M_SKIN_MID
    mpix[33, 52] = M_SKIN_LIGHT
    mpix[34, 52] = M_SKIN_OUTLINE

    mpix[30, 53] = M_SKIN_OUTLINE
    mpix[31, 53] = M_SKIN_SHADOW
    mpix[32, 53] = M_SKIN_MID
    mpix[33, 53] = M_SKIN_LIGHT
    mpix[34, 53] = M_SKIN_OUTLINE

    mpix[30, 54] = M_SKIN_OUTLINE
    mpix[31, 54] = M_SKIN_SHADOW
    mpix[32, 54] = M_SKIN_MID
    mpix[33, 54] = M_SKIN_LIGHT
    mpix[34, 54] = M_SKIN_OUTLINE

    # Petite Bare Foot (y=55..57, heel at x=29, toes at x=35 - 7px long!)
    mpix[30, 55] = M_SKIN_OUTLINE
    mpix[31, 55] = M_SKIN_SHADOW
    mpix[32, 55] = M_SKIN_MID
    mpix[33, 55] = M_SKIN_LIGHT
    mpix[34, 55] = M_SKIN_OUTLINE

    mpix[29, 56] = M_SKIN_OUTLINE # heel
    mpix[30, 56] = M_SKIN_SHADOW
    mpix[31, 56] = M_SKIN_MID
    mpix[32, 56] = M_SKIN_LIGHT
    mpix[33, 56] = M_SKIN_LIGHT
    mpix[34, 56] = M_SKIN_MID   # toes
    mpix[35, 56] = M_SKIN_OUTLINE

    for x in range(29, 36):
        mpix[x, 57] = M_GROUND_OUT

    # Forearm & Hand (Slender, x=31..34)
    mpix[30, 44] = M_SKIN_OUTLINE
    mpix[31, 44] = M_SKIN_SHADOW
    mpix[32, 44] = M_SKIN_MID
    mpix[33, 44] = M_SKIN_LIGHT
    mpix[34, 44] = M_SKIN_OUTLINE

    mpix[30, 45] = M_SKIN_OUTLINE
    mpix[31, 45] = M_SKIN_SHADOW
    mpix[32, 45] = M_SKIN_MID
    mpix[33, 45] = M_SKIN_LIGHT
    mpix[34, 45] = M_SKIN_OUTLINE

    mpix[31, 46] = M_SKIN_OUTLINE
    mpix[32, 46] = M_SKIN_MID
    mpix[33, 46] = M_SKIN_LIGHT
    mpix[34, 46] = M_SKIN_OUTLINE

    mpix[31, 47] = M_SKIN_OUTLINE
    mpix[32, 47] = M_SKIN_MID
    mpix[33, 47] = M_SKIN_LIGHT
    mpix[34, 47] = M_SKIN_OUTLINE

    mpix[31, 48] = M_SKIN_OUTLINE
    mpix[32, 48] = M_SKIN_MID
    mpix[33, 48] = M_SKIN_LIGHT
    mpix[34, 48] = M_SKIN_OUTLINE

    mpix[31, 49] = M_SKIN_OUTLINE
    mpix[32, 49] = M_SKIN_LIGHT
    mpix[33, 49] = M_SKIN_LIGHT
    mpix[34, 49] = M_SKIN_OUTLINE

    mpix[32, 50] = M_SKIN_OUTLINE
    mpix[33, 50] = M_SKIN_LIGHT
    mpix[34, 50] = M_SKIN_OUTLINE

    mpix[32, 51] = M_SKIN_OUTLINE
    mpix[33, 51] = M_SKIN_OUTLINE

    m_canvas.save("scratch/test_slender_male.png")
    print("Saved test_slender_male.png")

if __name__ == "__main__":
    test_slender_duo()
