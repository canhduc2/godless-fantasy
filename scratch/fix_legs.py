import os
from PIL import Image

def generate_clean_side_views():
    chars_dir = r"client\GodlessFantasyClient\Assets\Resources\Characters"
    
    # -------------------------------------------------------------
    # 1. MALE SIDE VIEW (EAST & WEST)
    # Sampling colors directly from player_base_male_south_0.png
    # -------------------------------------------------------------
    m_south = Image.open(os.path.join(chars_dir, "player_base_male_south_0.png")).convert("RGBA")
    
    # Palette from south_0
    m_shorts_fill = m_south.getpixel((28, 46)) # smooth grey (83, 83, 83, 255)
    m_shorts_dark = m_south.getpixel((27, 45)) # (63, 63, 63, 255)
    m_shorts_outline = (15, 12, 12, 255)
    
    m_skin_light = m_south.getpixel((27, 51)) # (250, 251, 248, 255) or warm skin
    m_skin_mid = (248, 208, 180, 255)
    m_skin_shadow = (213, 145, 115, 255)
    m_skin_outline = (25, 15, 15, 255)
    
    m_shoe_fill = m_south.getpixel((27, 55)) # dark shoe (7, 7, 7, 255)
    m_shoe_sole = (2, 2, 2, 255)
    
    # Base east upper body (y < 44)
    m_base_east = Image.open(os.path.join(chars_dir, "player_base_male_east_0.png")).convert("RGBA")
    m_upper = m_base_east.crop((0, 0, 64, 44))
    
    # Waist in male east is x=26 to 37.
    def draw_male_east(stride, bob_y=0):
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(m_upper, (0, bob_y), m_upper)
        pix = img.load()
        
        # Shorts (y=44..49, x=26..37)
        # Outline
        for x in range(26, 38):
            pix[x, 44 + bob_y] = m_shorts_dark
            pix[x, 49 + bob_y] = m_shorts_outline
        for y in range(44 + bob_y, 50 + bob_y):
            pix[26, y] = m_shorts_outline
            pix[37, y] = m_shorts_outline
        # Fill
        for y in range(45 + bob_y, 49 + bob_y):
            for x in range(27, 37):
                pix[x, y] = m_shorts_fill if x > 29 else m_shorts_dark
                
        if stride == 0:
            # IDLE: Both legs grounded naturally
            # Back leg (x=26..30)
            for y in range(50 + bob_y, 55 + bob_y):
                pix[26, y] = m_skin_outline
                for x in range(27, 30):
                    pix[x, y] = m_skin_shadow
                pix[30, y] = m_skin_outline
            # Back shoe (y=55..57, x=26..31)
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(26, 31):
                    pix[x, y] = m_shoe_fill if y < 57 + bob_y else m_shoe_sole
            
            # Front leg (x=30..36)
            for y in range(50 + bob_y, 55 + bob_y):
                pix[30, y] = m_skin_outline
                for x in range(31, 36):
                    pix[x, y] = m_skin_mid if x < 34 else m_skin_light
                pix[36, y] = m_skin_outline
            # Front shoe (y=55..58, x=29..37)
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(29, 37):
                    pix[x, y] = m_shoe_fill if y < 57 + bob_y else m_shoe_sole
                    
        elif stride == 1:
            # STEP 1: Front leg steps forward, Back leg reaches backward
            # Back leg reaching back (x=24..29, y=50..54)
            for y in range(50 + bob_y, 54 + bob_y):
                dx = (53 + bob_y - y) // 2
                pix[24 + dx, y] = m_skin_outline
                for x in range(25 + dx, 28 + dx):
                    pix[x, y] = m_skin_shadow
                pix[28 + dx, y] = m_skin_outline
            # Back shoe lifted (y=54..56, x=23..28)
            for y in range(54 + bob_y, 57 + bob_y):
                for x in range(23, 28):
                    pix[x, y] = m_shoe_fill if y < 56 + bob_y else m_shoe_sole
                    
            # Front leg forward (x=32..38, y=50..55)
            for y in range(50 + bob_y, 55 + bob_y):
                dx = (y - 50 - bob_y) // 2
                pix[31 + dx, y] = m_skin_outline
                for x in range(32 + dx, 36 + dx):
                    pix[x, y] = m_skin_mid if x < 35 + dx else m_skin_light
                pix[36 + dx, y] = m_skin_outline
            # Front shoe planted (y=55..58, x=31..38)
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(31, 38):
                    pix[x, y] = m_shoe_fill if y < 57 + bob_y else m_shoe_sole
                    
        elif stride == 2:
            # PASSING: Legs cross mid-step
            # Back leg passing through
            for y in range(50 + bob_y, 54 + bob_y):
                pix[27, y] = m_skin_outline
                for x in range(28, 31):
                    pix[x, y] = m_skin_shadow
                pix[31, y] = m_skin_outline
            for y in range(54 + bob_y, 56 + bob_y):
                for x in range(27, 31):
                    pix[x, y] = m_shoe_fill
                    
            # Front leg under center
            for y in range(50 + bob_y, 55 + bob_y):
                pix[31, y] = m_skin_outline
                for x in range(32, 36):
                    pix[x, y] = m_skin_mid if x < 34 else m_skin_light
                pix[36, y] = m_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(30, 37):
                    pix[x, y] = m_shoe_fill if y < 57 + bob_y else m_shoe_sole
                    
        elif stride == 3:
            # STEP 2: Front leg pushes back, Back leg steps forward
            # Front leg back (x=25..30)
            for y in range(50 + bob_y, 54 + bob_y):
                dx = (53 + bob_y - y) // 2
                pix[25 + dx, y] = m_skin_outline
                for x in range(26 + dx, 29 + dx):
                    pix[x, y] = m_skin_mid
                pix[29 + dx, y] = m_skin_outline
            for y in range(54 + bob_y, 57 + bob_y):
                for x in range(24, 29):
                    pix[x, y] = m_shoe_fill if y < 56 + bob_y else m_shoe_sole
                    
            # Back leg forward (x=31..37)
            for y in range(50 + bob_y, 55 + bob_y):
                dx = (y - 50 - bob_y) // 2
                pix[31 + dx, y] = m_skin_outline
                for x in range(32 + dx, 36 + dx):
                    pix[x, y] = m_skin_shadow
                pix[36 + dx, y] = m_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(31, 38):
                    pix[x, y] = m_shoe_fill if y < 57 + bob_y else m_shoe_sole
                    
        return img

    for f in range(4):
        bob = -1 if f in (1, 3) else 0
        img_e = draw_male_east(f, bob)
        img_e.save(os.path.join(chars_dir, f"player_base_male_east_{f}.png"))
        img_w = img_e.transpose(Image.FLIP_LEFT_RIGHT)
        img_w.save(os.path.join(chars_dir, f"player_base_male_west_{f}.png"))
        print(f"Clean male east_{f} and west_{f} saved")

    # -------------------------------------------------------------
    # 2. FEMALE SIDE VIEW (EAST & WEST)
    # Sampling colors directly from player_base_female_south_0.png
    # -------------------------------------------------------------
    f_south = Image.open(os.path.join(chars_dir, "player_base_female_south_0.png")).convert("RGBA")
    
    f_shorts_fill = (68, 68, 70, 255)
    f_shorts_dark = (50, 50, 52, 255)
    f_shorts_outline = (25, 20, 22, 255)
    
    f_skin_light = (252, 224, 204, 255)
    f_skin_mid = (248, 198, 172, 255)
    f_skin_shadow = (220, 160, 135, 255)
    f_skin_outline = (40, 25, 25, 255)
    
    f_shoe_fill = (55, 52, 55, 255) # Clean charcoal sneaker matching Image 2
    f_shoe_sole = (28, 26, 28, 255)
    
    f_base_east = Image.open(os.path.join(chars_dir, "player_base_female_east_0.png")).convert("RGBA")
    f_upper = f_base_east.crop((0, 0, 64, 44))
    
    # Waist in female east is x=29 to 38. Hair is behind at x=20..28.
    def draw_female_east(stride, bob_y=0):
        img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
        img.paste(f_upper, (0, bob_y), f_upper)
        pix = img.load()
        
        # Shorts (y=44..48, x=29..38)
        for x in range(29, 39):
            pix[x, 44 + bob_y] = f_shorts_dark
            pix[x, 48 + bob_y] = f_shorts_outline
        for y in range(44 + bob_y, 49 + bob_y):
            pix[29, y] = f_shorts_outline
            pix[38, y] = f_shorts_outline
        for y in range(45 + bob_y, 48 + bob_y):
            for x in range(30, 38):
                pix[x, y] = f_shorts_fill if x > 32 else f_shorts_dark
                
        if stride == 0:
            # IDLE: Both legs grounded, slender anime legs
            # Back leg (x=29..33)
            for y in range(49 + bob_y, 55 + bob_y):
                pix[29, y] = f_skin_outline
                for x in range(30, 33):
                    pix[x, y] = f_skin_shadow
                pix[33, y] = f_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(29, 33):
                    pix[x, y] = f_shoe_fill if y < 57 + bob_y else f_shoe_sole
                    
            # Front leg (x=33..38)
            for y in range(49 + bob_y, 55 + bob_y):
                pix[33, y] = f_skin_outline
                for x in range(34, 38):
                    pix[x, y] = f_skin_mid if x < 36 else f_skin_light
                pix[38, y] = f_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(32, 38):
                    pix[x, y] = f_shoe_fill if y < 57 + bob_y else f_shoe_sole
                    
        elif stride == 1:
            # STEP 1
            # Back leg back (x=27..32)
            for y in range(49 + bob_y, 54 + bob_y):
                dx = (53 + bob_y - y) // 2
                pix[27 + dx, y] = f_skin_outline
                for x in range(28 + dx, 31 + dx):
                    pix[x, y] = f_skin_shadow
                pix[31 + dx, y] = f_skin_outline
            for y in range(54 + bob_y, 57 + bob_y):
                for x in range(26, 31):
                    pix[x, y] = f_shoe_fill if y < 56 + bob_y else f_shoe_sole
                    
            # Front leg forward (x=34..39)
            for y in range(49 + bob_y, 55 + bob_y):
                dx = (y - 49 - bob_y) // 2
                pix[33 + dx, y] = f_skin_outline
                for x in range(34 + dx, 38 + dx):
                    pix[x, y] = f_skin_mid if x < 36 + dx else f_skin_light
                pix[38 + dx, y] = f_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(34, 40):
                    pix[x, y] = f_shoe_fill if y < 57 + bob_y else f_shoe_sole
                    
        elif stride == 2:
            # PASSING
            for y in range(49 + bob_y, 54 + bob_y):
                pix[30, y] = f_skin_outline
                for x in range(31, 34):
                    pix[x, y] = f_skin_shadow
                pix[34, y] = f_skin_outline
            for y in range(54 + bob_y, 56 + bob_y):
                for x in range(30, 34):
                    pix[x, y] = f_shoe_fill
                    
            for y in range(49 + bob_y, 55 + bob_y):
                pix[34, y] = f_skin_outline
                for x in range(35, 38):
                    pix[x, y] = f_skin_mid
                pix[38, y] = f_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(33, 38):
                    pix[x, y] = f_shoe_fill if y < 57 + bob_y else f_shoe_sole
                    
        elif stride == 3:
            # STEP 2
            for y in range(49 + bob_y, 54 + bob_y):
                dx = (53 + bob_y - y) // 2
                pix[28 + dx, y] = f_skin_outline
                for x in range(29 + dx, 32 + dx):
                    pix[x, y] = f_skin_mid
                pix[32 + dx, y] = f_skin_outline
            for y in range(54 + bob_y, 57 + bob_y):
                for x in range(27, 32):
                    pix[x, y] = f_shoe_fill if y < 56 + bob_y else f_shoe_sole
                    
            for y in range(49 + bob_y, 55 + bob_y):
                dx = (y - 49 - bob_y) // 2
                pix[33 + dx, y] = f_skin_outline
                for x in range(34 + dx, 38 + dx):
                    pix[x, y] = f_skin_shadow
                pix[38 + dx, y] = f_skin_outline
            for y in range(55 + bob_y, 58 + bob_y):
                for x in range(34, 40):
                    pix[x, y] = f_shoe_fill if y < 57 + bob_y else f_shoe_sole
                    
        return img

    for f in range(4):
        bob = -1 if f in (1, 3) else 0
        img_e = draw_female_east(f, bob)
        img_e.save(os.path.join(chars_dir, f"player_base_female_east_{f}.png"))
        img_w = img_e.transpose(Image.FLIP_LEFT_RIGHT)
        img_w.save(os.path.join(chars_dir, f"player_base_female_west_{f}.png"))
        print(f"Clean female east_{f} and west_{f} saved")

if __name__ == "__main__":
    generate_clean_side_views()
