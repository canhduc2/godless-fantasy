# -*- coding: utf-8 -*-
"""
Godless Fantasy - Handcrafted Pixel Matrix Art Engine
Every pixel is intentionally placed for pristine, professional chibi pixel art.
Zero cropping. Zero automated slicing. Pure crafted art.
"""
import os
from PIL import Image

ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

# ════════════════════════════════════════════════════════════════
# MASTER PALETTE (Sampled from the reference art style)
# ════════════════════════════════════════════════════════════════
PALETTE = {
    '.': (0, 0, 0, 0),         # Transparent
    '#': (62, 34, 28, 255),    # Dark outline (warm chocolate brown)
    '1': (198, 142, 114, 255), # Hair Highlight (glossy sheen)
    '2': (154, 108, 86, 255),  # Hair Base (chestnut auburn)
    '3': (116, 72, 54, 255),   # Hair Shadow (mid shadow)
    '4': (84, 48, 36, 255),    # Hair Deep Shadow
    'R': (228, 64, 60, 255),   # Red hairclip / bow
    'G': (248, 196, 68, 255),  # Gold clip accent / belt buckle
    'B': (40, 36, 42, 255),    # Black bow ribbon

    's': (255, 238, 228, 255), # Skin Highlight
    'S': (248, 210, 190, 255), # Skin Base (peachy anime)
    'd': (224, 172, 150, 255), # Skin Shadow
    'D': (192, 138, 116, 255), # Skin Deep Shadow
    'p': (246, 174, 170, 255), # Pink Blush

    'e': (40, 22, 28, 255),    # Eye dark lash
    'i': (128, 64, 48, 255),   # Eye iris upper
    'I': (188, 102, 74, 255),  # Eye iris lower reflex
    'w': (255, 255, 255, 255), # Eye white / Catchlight sparkle

    't': (250, 144, 118, 255), # Top Highlight (salmon/orange)
    'T': (224, 102, 78, 255),  # Top Base (adventurer orange)
    'o': (178, 68, 50, 255),   # Top Shadow
    'O': (128, 44, 32, 255),   # Top Deep Shadow / Crease
    'c': (246, 242, 238, 255), # Collar White trim

    'L': (116, 62, 38, 255),   # Leather Belt
    'q': (78, 78, 92, 255),    # Shorts Highlight (charcoal)
    'Q': (52, 52, 62, 255),    # Shorts Base
    'u': (34, 34, 42, 255),    # Shorts Shadow

    'h': (96, 80, 70, 255),    # Boot Highlight
    'H': (66, 52, 44, 255),    # Boot Base (leather)
    'k': (44, 34, 28, 255),    # Boot Shadow
    'K': (24, 18, 16, 255)     # Boot Sole
}

# ════════════════════════════════════════════════════════════════
# FEMALE SOUTH (Front View) - 28 pixels wide x 46 pixels tall
# ════════════════════════════════════════════════════════════════
# HEAD (y=0..26)
FEMALE_SOUTH_HEAD = [
    "..........########..........", # 0
    ".......###22222222###.......", # 1
    ".....##22221111112222##.....", # 2
    "....#222111111111111222#....", # 3
    "...#222111########111222#...", # 4
    "..#22111#BBB##..#B#11122#..",  # 5 Bow on bangs
    ".#22111#BBBB######B#11122#.",  # 6
    ".#22222#BB####....###2222#.",  # 7
    "#22233##R#...######..##222#",  # 8 Red clip
    "#2233#..#...#SSSSSS#...#32#",  # 9 Forehead
    "#233#.......#SSSSSS#....#3#",  # 10
    "#33#...#####SSSSSSSS#####.#",  # 11 Bangs framing
    "#3#...#22222#SSSSSS#22222#.", # 12
    "#3#..#22###22#SSSS#22###22#", # 13 Eyes row 1
    "#3#..#2#ewwe#2#SS#2#ewwe#2#", # 14 Eyelashes & whites
    "#3#..#2#eiiw#2#SS#2#eiiw#2#", # 15 Irises & sparkle
    "#3#..#2#eIIw#2SSSS2#eIIw#2#", # 16 Lower iris reflex
    ".##..#3#e##e#2pSSp2#e##e#3#", # 17 Lower lid & blush
    ".....#33####SpSSSSpS####33#", # 18 Cheeks
    ".....#3333333SS##SS3333333#", # 19 Mouth
    "......#333333SSSSSS333333#.", # 20 Chin
    ".......#333333SSSS333333#..", # 21 Jawline
    "........##3333dSSd3333##...", # 22 Chin tip
    "..........##33dSSd33##.....", # 23 Neck top
    "............#3dSSd3#.......", # 24 Neck stub
    "............#3dSSd3#.......", # 25 Neck stub (rotates inside collar)
    ".............######........"  # 26 Neck joint cap
]

# TORSO / BODY (y=24..35)
FEMALE_SOUTH_BODY = [
    "............######..........", # 24 Collar socket
    "..........##cccccc##........", # 25 White collar trim
    "........##TTccccccTToo......", # 26 Shoulder sockets
    ".......#ttTTTccccTTTToo#....", # 27 Upper chest
    ".......#ttTTTTTTTTTTToo#....", # 28 Center chest
    ".......#ttTTTTTTTTTTToo#....", # 29 Waist
    ".......#ooTTTTTTTTTTToo#....", # 30 Lower waist
    ".......#ooOOOOOOOOOOOoo#....", # 31 Shirt bottom hem
    ".......#LLLLLLGGLLLLLLL#....", # 32 Leather belt & gold buckle
    ".......#QQQQQQGGQQQQQQQ#....", # 33 Shorts waistband
    "......#uuQQQQQQQQQQQQQuu#...", # 34 Shorts pelvis
    "......#uuuQQQQ##QQQQQuuu#...", # 35 Shorts leg sockets
    ".......###uuuu##uuuu###....."  # 36 Shorts cuffs
]

# ARM RIGHT (Screen Left - y=26..36)
FEMALE_SOUTH_ARM_R = [
    "......####..................", # 26 Shoulder ball joint
    ".....#SSd#..................", # 27 Shoulder cap
    "....#SSSd#..................", # 28 Upper bicep
    "....#SSSd#..................", # 29 Mid arm
    "....#SSSd#..................", # 30 Forearm
    "....#SSSd#..................", # 31 Lower arm
    "....#SSdd#..................", # 32 Wrist
    ".....#SSS#..................", # 33 Hand / fist
    ".....#SSd#..................", # 34 Knuckles
    "......###..................."  # 35 Hand tip
]

# ARM LEFT (Screen Right - y=26..36)
FEMALE_SOUTH_ARM_L = [
    "..................####......", # 26 Shoulder ball joint
    "..................#dSS#.....", # 27 Shoulder cap
    "..................#dSSS#....", # 28 Upper bicep
    "..................#dSSS#....", # 29 Mid arm
    "..................#dSSS#....", # 30 Forearm
    "..................#dSSS#....", # 31 Lower arm
    "..................#ddSS#....", # 32 Wrist
    "..................#SSS#.....", # 33 Hand / fist
    "..................#dSS#.....", # 34 Knuckles
    "...................###......"  # 35 Hand tip
]

# LEG RIGHT (Screen Left - y=34..45)
FEMALE_SOUTH_LEG_R = [
    "......####..................", # 34 Hip ball joint
    ".....#QQQu#.................", # 35 Upper thigh inside shorts
    ".....#SSSd#.................", # 36 Bare thigh
    ".....#SSSd#.................", # 37 Mid thigh
    ".....#SSdd#.................", # 38 Knee
    ".....#SSSS#.................", # 39 Calf
    ".....#kHHk#.................", # 40 Boot cuff
    "....#kHHHHk#................", # 41 Boot upper
    "....#kHHhHk#................", # 42 Boot ankle
    "....#kHHHHk#................", # 43 Boot foot
    "....#KKKKKK#................"  # 44 Boot sole (stands on ground)
]

# LEG LEFT (Screen Right - y=34..45)
FEMALE_SOUTH_LEG_L = [
    "..................####......", # 34 Hip ball joint
    ".................#uQQQ#.....", # 35 Upper thigh inside shorts
    ".................#dSSS#.....", # 36 Bare thigh
    ".................#dSSS#.....", # 37 Mid thigh
    ".................#ddSS#.....", # 38 Knee
    ".................#SSSS#.....", # 39 Calf
    ".................#kHHk#.....", # 40 Boot cuff
    "................#kHHHHk#....", # 41 Boot upper
    "................#kHhHHk#....", # 42 Boot ankle
    "................#kHHHHk#....", # 43 Boot foot
    "................#KKKKKK#...."  # 44 Boot sole (stands on ground)
]

def render_grid(grid, start_y=0, start_x=18):
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    p = im.load()
    for row_idx, row in enumerate(grid):
        y = start_y + row_idx
        for col_idx, char in enumerate(row):
            x = start_x + col_idx
            if 0 <= x < 64 and 0 <= y < 64:
                col = PALETTE.get(char, (0, 0, 0, 0))
                if col[3] > 0:
                    p[x, y] = col
    return im

def build_test():
    # Render parts onto 64x64 canvases
    # Head starts at y=12
    head = render_grid(FEMALE_SOUTH_HEAD, start_y=12, start_x=18)
    # Body starts at y=35
    body = render_grid(FEMALE_SOUTH_BODY, start_y=35, start_x=18)
    # Arms start at y=37
    arm_r = render_grid(FEMALE_SOUTH_ARM_R, start_y=37, start_x=18)
    arm_l = render_grid(FEMALE_SOUTH_ARM_L, start_y=37, start_x=18)
    # Legs start at y=45
    leg_r = render_grid(FEMALE_SOUTH_LEG_R, start_y=45, start_x=18)
    leg_l = render_grid(FEMALE_SOUTH_LEG_L, start_y=45, start_x=18)

    # Assembled character
    comp = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    comp.paste(leg_l, (0, 0), leg_l)
    comp.paste(leg_r, (0, 0), leg_r)
    comp.paste(body, (0, 0), body)
    comp.paste(arm_l, (0, 0), arm_l)
    comp.paste(arm_r, (0, 0), arm_r)
    comp.paste(head, (0, 0), head)

    out_comp = os.path.join(ARTIFACT_DIR, "pixel_matrix_assembled.png")
    big = comp.resize((256, 256), Image.NEAREST)
    big.save(out_comp)
    print("Saved matrix test to:", out_comp)

if __name__ == "__main__":
    build_test()
