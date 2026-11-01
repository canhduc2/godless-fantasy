import os
import math
from PIL import Image, ImageDraw, ImageFilter

def get_palettes():
    male = {
        "gender": "male",
        "cloak_light": (255, 255, 255, 255),
        "cloak_main": (235, 242, 250, 255),
        "cloak_mid": (205, 218, 232, 255),
        "cloak_shadow": (155, 175, 200, 255),
        "cloak_dark": (105, 128, 158, 255),
        "trim_light": (0, 245, 255, 255),
        "trim_main": (0, 160, 255, 255),
        "trim_shadow": (0, 80, 190, 255),
        "skin_light": (255, 240, 230, 255),
        "skin_main": (255, 218, 195, 255),
        "skin_shadow": (220, 170, 142, 255),
        "hair_main": (35, 50, 80, 255),
        "hair_light": (60, 85, 130, 255),
        "hair_shadow": (20, 30, 52, 255),
        "eye_color": (0, 210, 255, 255),
        "eye_dark": (0, 50, 110, 255),
        "eye_spark": (255, 255, 255, 255),
        "brooch_gold": (255, 215, 0, 255),
        "brooch_gem": (0, 240, 255, 255),
        "tunic_main": (25, 32, 50, 255),
        "tunic_trim": (0, 190, 255, 255),
        "belt_gold": (220, 180, 40, 255),
        "boot_main": (32, 42, 60, 255),
        "boot_shadow": (18, 24, 36, 255),
        "boot_trim": (0, 140, 220, 255),
        "outline": (15, 20, 32, 255)
    }

    female = {
        "gender": "female",
        "cloak_light": (255, 255, 255, 255),
        "cloak_main": (255, 245, 250, 255),
        "cloak_mid": (250, 225, 238, 255),
        "cloak_shadow": (230, 175, 200, 255),
        "cloak_dark": (185, 125, 155, 255),
        "trim_light": (255, 140, 185, 255),
        "trim_main": (255, 50, 130, 255),
        "trim_shadow": (190, 20, 85, 255),
        "skin_light": (255, 246, 242, 255),
        "skin_main": (255, 224, 212, 255),
        "skin_shadow": (230, 180, 165, 255),
        "blush": (255, 120, 150, 180),
        "hair_main": (255, 218, 125, 255),
        "hair_light": (255, 240, 170, 255),
        "hair_shadow": (215, 160, 75, 255),
        "eye_color": (255, 35, 125, 255),
        "eye_dark": (100, 10, 50, 255),
        "eye_spark": (255, 255, 255, 255),
        "brooch_gold": (255, 192, 203, 255),
        "brooch_gem": (255, 20, 75, 255),
        "tunic_main": (48, 28, 48, 255),
        "tunic_trim": (255, 100, 160, 255),
        "belt_gold": (255, 215, 100, 255),
        "boot_main": (55, 35, 52, 255),
        "boot_shadow": (32, 18, 30, 255),
        "boot_trim": (255, 70, 140, 255),
        "outline": (28, 15, 25, 255)
    }
    return male, female

print("Palettes initialized successfully.")
