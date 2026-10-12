# -*- coding: utf-8 -*-
"""
Godless Fantasy - Master Pixel Art Crafting for East/West 8-Dir Character Sprites
Studio: SquiRRels Studio (c) 2026
"""
import os
from PIL import Image

def craft_female_east():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
    base_img = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
    
    # Crop head and chest strictly above waist (y < 44)
    # y=0..43 contains the pristine, high quality face, hair, and upper chest
    upper = base_img.crop((0, 0, 64, 44))
    
    # Color palette sampled from female_south_0 & female_south_east_0:
    C_TRANSPARENT = (0, 0, 0, 0)
    
    # Skin
    SKIN_LIGHT   = (252, 224, 204, 255) # Peach highlight
    SKIN_MID     = (244, 196, 170, 255) # Base anime skin
    SKIN_SHADOW  = (218, 156, 130, 255) # Shadow under clothes / rear
    SKIN_DARK_SH = (185, 125, 102, 255) # Deep shadow for far leg
    SKIN_OUTLINE = (96, 48, 44, 255)   # Soft warm outline
    SKIN_OUT_FAR = (68, 34, 32, 255)   # Outline for far leg
    
    # Shorts (Dark Charcoal Anime Shorts)
    SH_LIGHT   = (86, 86, 92, 255)
    SH_MID     = (64, 64, 68, 255)
    SH_SHADOW  = (46, 46, 50, 255)
    SH_OUTLINE = (28, 26, 30, 255)
    SH_FAR_MID = (42, 42, 46, 255)
    
    # Shoes (Sneakers / Boots)
    SHOE_LIGHT   = (82, 78, 84, 255)
    SHOE_MID     = (56, 52, 58, 255)
    SHOE_SHADOW  = (38, 35, 40, 255)
    SHOE_OUTLINE = (28, 26, 30, 255)
    SHOE_SOLE    = (24, 22, 26, 255)
    SHOE_FAR_MID = (38, 35, 40, 255)
    
    # Let's define the 4 animation frames for Female East:
    # Frame 0: Idle / Contact
    # Frame 1: Walk Step 1 (Right/front leg forward, Left/back leg back)
    # Frame 2: Walk Passing (Legs crossing)
    # Frame 3: Walk Step 2 (Left/back leg forward, Right/front leg back)
    
    frames = []
    
    for f in range(4):
        canvas = Image.new("RGBA", (64, 64), C_TRANSPARENT)
        
        # Bobbing: -1 on step frames (1, 3)
        bob = -1 if f in (1, 3) else 0
        canvas.paste(upper, (0, bob), upper)
        pix = canvas.load()
        
        # ------------------------------------------------------------------
        # 1. FAR LEG (Background layer - Left Leg)
        # ------------------------------------------------------------------
        if f == 0:
            # Idle: Far leg slightly visible behind near leg (x=27..30, y=48..56)
            # Thigh/Calf:
            for y in range(48 + bob, 54 + bob):
                pix[27, y] = SKIN_OUT_FAR
                pix[28, y] = SKIN_DARK_SH
                pix[29, y] = SKIN_DARK_SH
                pix[30, y] = SKIN_DARK_SH
            # Shoe (planted slightly behind):
            for y in range(54 + bob, 57 + bob):
                pix[27, y] = SHOE_SOLE
                pix[28, y] = SHOE_FAR_MID
                pix[29, y] = SHOE_FAR_MID
                pix[30, y] = SHOE_FAR_MID
                pix[31, y] = SHOE_SOLE
            pix[27, 57 + bob] = SHOE_SOLE
            pix[28, 57 + bob] = SHOE_SOLE
            pix[29, 57 + bob] = SHOE_SOLE
            pix[30, 57 + bob] = SHOE_SOLE
            
        elif f == 1:
            # Step 1: Far leg extends backward to the left (x=23..28)
            for y in range(48 + bob, 53 + bob):
                dx = (52 + bob - y)
                x_base = 25 - dx // 2
                pix[x_base, y] = SKIN_OUT_FAR
                pix[x_base + 1, y] = SKIN_DARK_SH
                pix[x_base + 2, y] = SKIN_DARK_SH
                pix[x_base + 3, y] = SKIN_OUT_FAR
            # Lifted foot angled back:
            for y in range(53 + bob, 56 + bob):
                pix[21, y] = SHOE_SOLE
                pix[22, y] = SHOE_FAR_MID
                pix[23, y] = SHOE_FAR_MID
                pix[24, y] = SHOE_SOLE
            pix[21, 56 + bob] = SHOE_SOLE
            pix[22, 56 + bob] = SHOE_SOLE
            pix[23, 56 + bob] = SHOE_SOLE
            
        elif f == 2:
            # Passing: Far leg crossing mid-stride (x=28..31)
            for y in range(48 + bob, 53 + bob):
                pix[28, y] = SKIN_OUT_FAR
                pix[29, y] = SKIN_DARK_SH
                pix[30, y] = SKIN_DARK_SH
                pix[31, y] = SKIN_OUT_FAR
            for y in range(53 + bob, 56 + bob):
                pix[28, y] = SHOE_SOLE
                pix[29, y] = SHOE_FAR_MID
                pix[30, y] = SHOE_FAR_MID
                pix[31, y] = SHOE_SOLE
            pix[28, 56 + bob] = SHOE_SOLE
            pix[29, 56 + bob] = SHOE_SOLE
            pix[30, 56 + bob] = SHOE_SOLE
            
        elif f == 3:
            # Step 2: Far leg steps forward to the right (x=33..38)
            for y in range(48 + bob, 53 + bob):
                dx = (y - 48 - bob) // 2
                x_base = 32 + dx
                pix[x_base, y] = SKIN_OUT_FAR
                pix[x_base + 1, y] = SKIN_DARK_SH
                pix[x_base + 2, y] = SKIN_DARK_SH
                pix[x_base + 3, y] = SKIN_OUT_FAR
            # Planted forward foot:
            for y in range(53 + bob, 56 + bob):
                pix[34, y] = SHOE_SOLE
                pix[35, y] = SHOE_FAR_MID
                pix[36, y] = SHOE_FAR_MID
                pix[37, y] = SHOE_FAR_MID
                pix[38, y] = SHOE_SOLE
            pix[34, 56 + bob] = SHOE_SOLE
            pix[35, 56 + bob] = SHOE_SOLE
            pix[36, 56 + bob] = SHOE_SOLE
            pix[37, 56 + bob] = SHOE_SOLE
            pix[38, 56 + bob] = SHOE_SOLE

        # ------------------------------------------------------------------
        # 2. SHORTS (Waist & Hips, y=44..48)
        # Glute curve at x=27..28, pelvic slope at x=36..37
        # ------------------------------------------------------------------
        # Waistband line at y=44:
        pix[28, 44 + bob] = SH_OUTLINE
        pix[29, 44 + bob] = SH_SHADOW
        pix[30, 44 + bob] = SH_MID
        pix[31, 44 + bob] = SH_MID
        pix[32, 44 + bob] = SH_LIGHT
        pix[33, 44 + bob] = SH_LIGHT
        pix[34, 44 + bob] = SH_MID
        pix[35, 44 + bob] = SH_MID
        pix[36, 44 + bob] = SH_SHADOW
        pix[37, 44 + bob] = SH_OUTLINE
        
        # Upper hip at y=45:
        pix[27, 45 + bob] = SH_OUTLINE # glute curvature starts
        pix[28, 45 + bob] = SH_SHADOW
        pix[29, 45 + bob] = SH_MID
        pix[30, 45 + bob] = SH_MID
        pix[31, 45 + bob] = SH_LIGHT
        pix[32, 45 + bob] = SH_LIGHT
        pix[33, 45 + bob] = SH_MID
        pix[34, 45 + bob] = SH_MID
        pix[35, 45 + bob] = SH_SHADOW
        pix[36, 45 + bob] = SH_OUTLINE
        
        # Mid hip at y=46:
        pix[27, 46 + bob] = SH_OUTLINE
        pix[28, 46 + bob] = SH_SHADOW
        pix[29, 46 + bob] = SH_MID
        pix[30, 46 + bob] = SH_MID
        pix[31, 46 + bob] = SH_LIGHT
        pix[32, 46 + bob] = SH_MID
        pix[33, 46 + bob] = SH_MID
        pix[34, 46 + bob] = SH_SHADOW
        pix[35, 46 + bob] = SH_SHADOW
        pix[36, 46 + bob] = SH_OUTLINE
        
        # Lower hip at y=47:
        pix[27, 47 + bob] = SH_OUTLINE
        pix[28, 47 + bob] = SH_SHADOW
        pix[29, 47 + bob] = SH_MID
        pix[30, 47 + bob] = SH_MID
        pix[31, 47 + bob] = SH_MID
        pix[32, 47 + bob] = SH_MID
        pix[33, 47 + bob] = SH_SHADOW
        pix[34, 47 + bob] = SH_SHADOW
        pix[35, 47 + bob] = SH_OUTLINE
        
        # Hem curve at y=48:
        pix[28, 48 + bob] = SH_OUTLINE
        pix[29, 48 + bob] = SH_OUTLINE
        pix[30, 48 + bob] = SH_SHADOW
        pix[31, 48 + bob] = SH_SHADOW
        pix[32, 48 + bob] = SH_MID
        pix[33, 48 + bob] = SH_OUTLINE
        pix[34, 48 + bob] = SH_OUTLINE
        
        # ------------------------------------------------------------------
        # 3. FOREGROUND NEAR LEG (Right Leg)
        # ------------------------------------------------------------------
        if f == 0:
            # Idle: Near leg standing straight with natural anime curvature
            # Thigh (y=49..51, x=31..36)
            for y in range(49 + bob, 52 + bob):
                pix[31, y] = SKIN_OUTLINE
                pix[32, y] = SKIN_SHADOW
                pix[33, y] = SKIN_MID
                pix[34, y] = SKIN_LIGHT
                pix[35, y] = SKIN_MID
                pix[36, y] = SKIN_OUTLINE
            
            # Knee (y=52, x=31..36, slight knee rounding)
            pix[31, 52 + bob] = SKIN_OUTLINE
            pix[32, 52 + bob] = SKIN_SHADOW
            pix[33, 52 + bob] = SKIN_MID
            pix[34, 52 + bob] = SKIN_LIGHT
            pix[35, 52 + bob] = SKIN_LIGHT
            pix[36, 52 + bob] = SKIN_OUTLINE
            
            # Calf & Shin (y=53..54, x=31..36, calf curves back at x=31)
            for y in range(53 + bob, 55 + bob):
                pix[31, y] = SKIN_OUTLINE
                pix[32, y] = SKIN_SHADOW
                pix[33, y] = SKIN_MID
                pix[34, y] = SKIN_LIGHT
                pix[35, y] = SKIN_MID
                pix[36, y] = SKIN_OUTLINE
            
            # Ankle (y=55, x=32..35, slim taper)
            pix[32, 55 + bob] = SKIN_OUTLINE
            pix[33, 55 + bob] = SKIN_MID
            pix[34, 55 + bob] = SKIN_LIGHT
            pix[35, 55 + bob] = SKIN_OUTLINE
            
            # Sneaker (y=55..57, x=30..38, stylish anime shoe pointing right)
            # Ankle collar / upper (y=55)
            pix[31, 55 + bob] = SHOE_OUTLINE
            pix[32, 55 + bob] = SHOE_MID
            pix[35, 55 + bob] = SHOE_LIGHT
            pix[36, 55 + bob] = SHOE_OUTLINE
            
            # Shoe body (y=56, heel at x=30, toe at x=37)
            pix[30, 56 + bob] = SHOE_OUTLINE # heel counter
            pix[31, 56 + bob] = SHOE_SHADOW
            pix[32, 56 + bob] = SHOE_MID
            pix[33, 56 + bob] = SHOE_LIGHT
            pix[34, 56 + bob] = SHOE_LIGHT
            pix[35, 56 + bob] = SHOE_MID
            pix[36, 56 + bob] = SHOE_LIGHT # toe cap highlight
            pix[37, 56 + bob] = SHOE_OUTLINE # rounded toe
            
            # Shoe sole (y=57, x=30..37)
            for x in range(30, 38):
                pix[x, 57 + bob] = SHOE_SOLE
                
        elif f == 1:
            # Step 1: Near leg stepping forward to the right (x=33..38)
            for y in range(49 + bob, 54 + bob):
                dx = (y - 49 - bob) // 2
                x_base = 32 + dx
                pix[x_base, y] = SKIN_OUTLINE
                pix[x_base + 1, y] = SKIN_SHADOW
                pix[x_base + 2, y] = SKIN_MID
                pix[x_base + 3, y] = SKIN_LIGHT
                pix[x_base + 4, y] = SKIN_OUTLINE
            # Planted forward shoe:
            pix[32, 55 + bob] = SHOE_OUTLINE
            pix[33, 55 + bob] = SHOE_SHADOW
            pix[34, 55 + bob] = SHOE_MID
            pix[35, 55 + bob] = SHOE_LIGHT
            pix[36, 55 + bob] = SHOE_LIGHT
            pix[37, 55 + bob] = SHOE_OUTLINE
            for x in range(32, 39):
                pix[x, 56 + bob] = SHOE_MID if x in (34, 35, 36) else SHOE_SHADOW
                pix[x, 57 + bob] = SHOE_SOLE
            pix[38, 56 + bob] = SHOE_OUTLINE
            
        elif f == 2:
            # Passing: Near leg vertical / slightly flexing
            for y in range(49 + bob, 54 + bob):
                pix[31, y] = SKIN_OUTLINE
                pix[32, y] = SKIN_SHADOW
                pix[33, y] = SKIN_MID
                pix[34, y] = SKIN_LIGHT
                pix[35, y] = SKIN_OUTLINE
            pix[31, 55 + bob] = SHOE_OUTLINE
            pix[32, 55 + bob] = SHOE_MID
            pix[33, 55 + bob] = SHOE_LIGHT
            pix[34, 55 + bob] = SHOE_MID
            pix[35, 55 + bob] = SHOE_OUTLINE
            for x in range(30, 36):
                pix[x, 56 + bob] = SHOE_MID
                pix[x, 57 + bob] = SHOE_SOLE
            pix[30, 56 + bob] = SHOE_OUTLINE
            pix[35, 56 + bob] = SHOE_OUTLINE

        elif f == 3:
            # Step 2: Near leg pushing backward to the left (x=25..31)
            for y in range(49 + bob, 54 + bob):
                dx = (53 + bob - y) // 2
                x_base = 27 - dx
                pix[x_base, y] = SKIN_OUTLINE
                pix[x_base + 1, y] = SKIN_SHADOW
                pix[x_base + 2, y] = SKIN_MID
                pix[x_base + 3, y] = SKIN_LIGHT
                pix[x_base + 4, y] = SKIN_OUTLINE
            for x in range(24, 30):
                pix[x, 55 + bob] = SHOE_MID
                pix[x, 56 + bob] = SHOE_SOLE
            pix[23, 55 + bob] = SHOE_OUTLINE
            pix[29, 55 + bob] = SHOE_OUTLINE
            pix[24, 56 + bob] = SHOE_SOLE
            pix[29, 56 + bob] = SHOE_SOLE

        # ------------------------------------------------------------------
        # 4. FOREGROUND ARM & HAND (Layered on top of shorts & hip!)
        # Right arm hangs down at x=33..37 from elbow (y=44) to hand (y=50)
        # ------------------------------------------------------------------
        # Elbow / Forearm top at y=44:
        # Seamlessly connects to upper arm at y=43 (x=31..35)
        arm_x_off = 0
        if f == 1: arm_x_off = -1 # swings slightly back when stepping forward
        elif f == 3: arm_x_off = 1 # swings forward when stepping back
        
        ax = 33 + arm_x_off
        
        # Forearm (y=44..47)
        pix[ax - 1, 44 + bob] = SKIN_OUTLINE
        pix[ax,     44 + bob] = SKIN_SHADOW
        pix[ax + 1, 44 + bob] = SKIN_MID
        pix[ax + 2, 44 + bob] = SKIN_LIGHT
        pix[ax + 3, 44 + bob] = SKIN_OUTLINE
        
        pix[ax - 1, 45 + bob] = SKIN_OUTLINE
        pix[ax,     45 + bob] = SKIN_SHADOW
        pix[ax + 1, 45 + bob] = SKIN_MID
        pix[ax + 2, 45 + bob] = SKIN_LIGHT
        pix[ax + 3, 45 + bob] = SKIN_OUTLINE
        
        pix[ax,     46 + bob] = SKIN_OUTLINE
        pix[ax + 1, 46 + bob] = SKIN_SHADOW
        pix[ax + 2, 46 + bob] = SKIN_MID
        pix[ax + 3, 46 + bob] = SKIN_LIGHT
        pix[ax + 4, 46 + bob] = SKIN_OUTLINE
        
        # Wrist (y=47)
        pix[ax,     47 + bob] = SKIN_OUTLINE
        pix[ax + 1, 47 + bob] = SKIN_MID
        pix[ax + 2, 47 + bob] = SKIN_LIGHT
        pix[ax + 3, 47 + bob] = SKIN_OUTLINE
        
        # Hand / Knuckles / Fingers (y=48..50)
        # Cute rounded chibi anime hand resting naturally beside the thigh
        pix[ax,     48 + bob] = SKIN_OUTLINE
        pix[ax + 1, 48 + bob] = SKIN_SHADOW
        pix[ax + 2, 48 + bob] = SKIN_MID
        pix[ax + 3, 48 + bob] = SKIN_LIGHT # thumb highlight
        pix[ax + 4, 48 + bob] = SKIN_OUTLINE
        
        pix[ax,     49 + bob] = SKIN_OUTLINE
        pix[ax + 1, 49 + bob] = SKIN_MID
        pix[ax + 2, 49 + bob] = SKIN_LIGHT
        pix[ax + 3, 49 + bob] = SKIN_LIGHT
        pix[ax + 4, 49 + bob] = SKIN_OUTLINE
        
        # Fingertips (y=50)
        pix[ax + 1, 50 + bob] = SKIN_OUTLINE
        pix[ax + 2, 50 + bob] = SKIN_LIGHT
        pix[ax + 3, 50 + bob] = SKIN_MID
        pix[ax + 4, 50 + bob] = SKIN_OUTLINE
        
        # Bottom curve of hand (y=51)
        pix[ax + 2, 51 + bob] = SKIN_OUTLINE
        pix[ax + 3, 51 + bob] = SKIN_OUTLINE
        
        frames.append(canvas)
        
    return frames

if __name__ == "__main__":
    frames = craft_female_east()
    for i, frame in enumerate(frames):
        frame.save(f"scratch/test_female_east_{i}.png")
    print("Test female east frames generated in scratch/")
