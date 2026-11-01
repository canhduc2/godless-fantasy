import os
from PIL import Image, ImageDraw

vfx_dir = "client/GodlessFantasyClient/Assets/Resources/VFX"
os.makedirs(vfx_dir, exist_ok=True)

# ----------------- 1. ANIMATED 8-BIT FLAME SPRITES (32x48) -----------------
# Palette
c_white = (255, 255, 230, 255)
c_yellow = (255, 225, 45, 255)
c_orange = (255, 135, 15, 255)
c_red = (225, 35, 20, 255)
c_dark_red = (145, 15, 20, 255)
c_glow = (255, 160, 40, 120)

flame_configs = [
    # Frame 0: Balanced centered flame with rising left tongue
    {
        "outer": [(16, 8), (22, 18), (24, 30), (22, 42), (10, 42), (8, 30), (10, 18)],
        "mid":   [(16, 14), (20, 22), (21, 32), (19, 40), (13, 40), (11, 32), (12, 22)],
        "core":  [(16, 22), (18, 28), (17, 38), (15, 38), (14, 28)],
        "white": [(16, 30), (17, 36), (15, 36)],
        "embers": [(12, 6, 2), (20, 10, 1)]
    },
    # Frame 1: Tall intense burst, leaping up
    {
        "outer": [(15, 4), (20, 14), (25, 28), (23, 42), (9, 42), (7, 28), (11, 14)],
        "mid":   [(15, 10), (19, 18), (22, 30), (20, 40), (12, 40), (10, 30), (13, 18)],
        "core":  [(15, 18), (18, 26), (18, 38), (14, 38), (13, 26)],
        "white": [(15, 26), (17, 34), (15, 37), (14, 34)],
        "embers": [(17, 3, 2), (9, 8, 1), (23, 16, 1)]
    },
    # Frame 2: Leaning right, dynamic curl
    {
        "outer": [(19, 6), (24, 16), (25, 30), (21, 42), (11, 42), (9, 32), (13, 20)],
        "mid":   [(18, 12), (22, 20), (22, 32), (19, 40), (13, 40), (11, 32), (14, 22)],
        "core":  [(17, 20), (20, 28), (18, 38), (14, 38), (14, 28)],
        "white": [(17, 28), (18, 35), (15, 35)],
        "embers": [(22, 5, 2), (14, 10, 1)]
    },
    # Frame 3: Twin split tongues
    {
        "outer": [(12, 8), (16, 18), (20, 7), (24, 20), (24, 32), (21, 42), (11, 42), (8, 32), (9, 18)],
        "mid":   [(13, 14), (16, 22), (19, 13), (21, 24), (21, 34), (19, 40), (13, 40), (11, 34), (11, 22)],
        "core":  [(16, 22), (18, 28), (17, 38), (15, 38), (14, 28)],
        "white": [(16, 28), (17, 36), (15, 36)],
        "embers": [(10, 4, 1), (21, 4, 2), (16, 12, 1)]
    },
    # Frame 4: Leaning left, lively flutter
    {
        "outer": [(13, 6), (19, 16), (23, 28), (21, 42), (11, 42), (7, 30), (8, 18)],
        "mid":   [(14, 12), (18, 20), (20, 30), (19, 40), (13, 40), (10, 32), (11, 22)],
        "core":  [(15, 20), (18, 28), (17, 38), (14, 38), (13, 28)],
        "white": [(15, 28), (17, 35), (14, 35)],
        "embers": [(11, 4, 2), (24, 14, 1)]
    },
    # Frame 5: Broad warm pulse
    {
        "outer": [(16, 7), (22, 17), (24, 29), (22, 42), (10, 42), (8, 29), (11, 17)],
        "mid":   [(16, 13), (20, 21), (21, 31), (19, 40), (13, 40), (11, 31), (12, 21)],
        "core":  [(16, 21), (19, 27), (18, 38), (14, 38), (13, 27)],
        "white": [(16, 29), (18, 36), (15, 36)],
        "embers": [(15, 3, 2), (8, 11, 1)]
    }
]

for idx, cfg in enumerate(flame_configs):
    img = Image.new("RGBA", (32, 48), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # Glow aura
    d.polygon(cfg["outer"], fill=c_glow)
    # Dark red edge
    d.polygon(cfg["outer"], fill=c_red, outline=c_dark_red)
    # Bright orange mid
    d.polygon(cfg["mid"], fill=c_orange)
    # Core yellow
    d.polygon(cfg["core"], fill=c_yellow)
    # White-hot center
    d.polygon(cfg["white"], fill=c_white)

    # Floating ember sparks
    for (ex, ey, er) in cfg["embers"]:
        d.ellipse([ex - er, ey - er, ex + er, ey + er], fill=c_yellow, outline=c_orange)
        d.point((ex, ey), fill=c_white)

    out_file = os.path.join(vfx_dir, f"vfx_flame_8bit_{idx}.png")
    img.save(out_file)
    print(f"Saved {out_file}")

# ----------------- 2. ANIMATED 8-BIT DUST PUFF SPRITES (32x32) -----------------
# 4 frames of cute chibi 8-bit dust cloud puffing at feet
d_highlight = (255, 252, 240, 240)
d_mid = (235, 220, 195, 220)
d_shadow = (185, 165, 140, 190)
d_outline = (120, 100, 80, 160)

dust_configs = [
    # Frame 0: Small ground pop
    [
        (16, 26, 4), (10, 27, 3), (22, 27, 3)
    ],
    # Frame 1: Full cute puffy cloud
    [
        (16, 22, 7), (9, 24, 5), (23, 24, 5), (13, 20, 5), (19, 20, 5)
    ],
    # Frame 2: Expanding and rising
    [
        (16, 18, 8), (7, 21, 6), (25, 21, 6), (12, 16, 6), (20, 16, 6)
    ],
    # Frame 3: Dissipating into fine particles
    [
        (5, 18, 3), (11, 14, 4), (16, 12, 4), (22, 14, 4), (27, 18, 3)
    ]
]

for idx, puffs in enumerate(dust_configs):
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    alpha_fade = 1.0 - (idx * 0.18)

    for (px, py, pr) in puffs:
        col_out = (d_outline[0], d_outline[1], d_outline[2], int(d_outline[3] * alpha_fade))
        col_shd = (d_shadow[0], d_shadow[1], d_shadow[2], int(d_shadow[3] * alpha_fade))
        col_mid = (d_mid[0], d_mid[1], d_mid[2], int(d_mid[3] * alpha_fade))
        col_hi  = (d_highlight[0], d_highlight[1], d_highlight[2], int(d_highlight[3] * alpha_fade))

        d.ellipse([px - pr, py - pr, px + pr, py + pr], fill=col_shd, outline=col_out)
        d.ellipse([px - pr + 1, py - pr + 1, px + pr - 1, py + pr - 2], fill=col_mid)
        # Highlight top-left
        d.ellipse([px - pr + 2, py - pr + 1, px, py], fill=col_hi)

    out_file = os.path.join(vfx_dir, f"vfx_dust_puff_{idx}.png")
    img.save(out_file)
    print(f"Saved {out_file}")

print("All VFX sprites generated successfully.")
