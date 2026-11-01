import os
from PIL import Image, ImageDraw

def create_palette(gender="male"):
    if gender == "male":
        return {
            "cloak_highlight": (255, 255, 255, 255),
            "cloak_main": (238, 244, 250, 255),
            "cloak_mid": (210, 222, 235, 255),
            "cloak_shadow": (160, 180, 205, 255),
            "cloak_deep_shadow": (115, 138, 168, 255),
            "trim_highlight": (0, 245, 255, 255),
            "trim_main": (0, 180, 255, 255),
            "trim_shadow": (0, 95, 205, 255),
            "skin_highlight": (255, 240, 230, 255),
            "skin_main": (255, 220, 195, 255),
            "skin_shadow": (225, 175, 145, 255),
            "hair_main": (40, 55, 85, 255),
            "hair_shadow": (25, 35, 55, 255),
            "eye_color": (0, 220, 255, 255),
            "eye_pupil": (10, 30, 60, 255),
            "eye_spark": (255, 255, 255, 255),
            "brooch_gold": (255, 215, 0, 255),
            "brooch_gem": (0, 235, 255, 255),
            "tunic_dark": (30, 35, 55, 255),
            "boot_main": (35, 45, 65, 255),
            "boot_shadow": (20, 25, 38, 255),
            "outline": (15, 20, 35, 255)
        }
    else:
        return {
            "cloak_highlight": (255, 255, 255, 255),
            "cloak_main": (255, 244, 248, 255),
            "cloak_mid": (250, 225, 235, 255),
            "cloak_shadow": (235, 180, 200, 255),
            "cloak_deep_shadow": (195, 135, 160, 255),
            "trim_highlight": (255, 128, 175, 255),
            "trim_main": (255, 64, 128, 255),
            "trim_shadow": (185, 20, 80, 255),
            "skin_highlight": (255, 245, 240, 255),
            "skin_main": (255, 225, 210, 255),
            "skin_shadow": (230, 185, 165, 255),
            "blush": (255, 140, 160, 210),
            "hair_main": (255, 215, 130, 255),
            "hair_shadow": (220, 165, 80, 255),
            "eye_color": (255, 40, 130, 255),
            "eye_pupil": (90, 10, 45, 255),
            "eye_spark": (255, 255, 255, 255),
            "brooch_gold": (255, 182, 193, 255),
            "brooch_gem": (255, 23, 68, 255),
            "tunic_dark": (45, 30, 45, 255),
            "boot_main": (60, 40, 55, 255),
            "boot_shadow": (35, 20, 30, 255),
            "outline": (30, 15, 25, 255)
        }

print("Palette test OK")
