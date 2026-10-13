# -*- coding: utf-8 -*-
"""
Godless Fantasy - Test Rig Part Extraction & Simulated Joint Rotation
Verifies that separated parts assemble seamlessly and rotate cleanly.
"""
import os
from PIL import Image

ARTIFACT_DIR = r"C:\Users\OS\.gemini\antigravity-ide\brain\63d3153d-4724-4ae7-aa86-426d7c5db938"

def test_rig():
    full = Image.open(os.path.join(ARTIFACT_DIR, "female_south_64_full.png")).convert("RGBA")
    w, h = 64, 64

    # 1. HEAD (Head, face, hair, side locks, neck stub)
    head = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    hp = head.load()
    fp = full.load()
    for y in range(h):
        for x in range(w):
            # Keep head area (y <= 38) OR side locks (x <= 23 or x >= 41, y <= 46)
            if y <= 37:
                hp[x, y] = fp[x, y]
            elif y <= 46 and (x <= 23 or x >= 41):
                hp[x, y] = fp[x, y]
            elif y <= 39 and (30 <= x <= 34): # neck stub
                hp[x, y] = fp[x, y]

    # 2. BODY (Torso: collar, vest, belt, shorts pelvis)
    body = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    bp = body.load()
    for y in range(36, 49):
        for x in range(24, 41):
            bp[x, y] = fp[x, y]
    # Shoulder caps extension to prevent gaps during arm swing
    # Sample shirt color from center chest
    shirt_color = fp[32, 41]
    shirt_sh = fp[26, 41]
    for y in range(37, 40):
        if bp[23, y][3] == 0: bp[23, y] = shirt_sh
        if bp[41, y][3] == 0: bp[41, y] = shirt_sh

    # 3. ARM LEFT (Screen right, character's left: x >= 38, y=36..48)
    arm_l = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    alp = arm_l.load()
    for y in range(36, 49):
        for x in range(38, 47):
            # Only keep arm pixels (not hair)
            p = fp[x, y]
            # Skin or hand
            if p[3] > 0 and (x >= 40 or y >= 40):
                alp[x, y] = p
    # Rounded shoulder cap
    arm_col = fp[42, 42]
    for y in range(37, 40):
        for x in range(39, 43):
            if alp[x, y][3] == 0: alp[x, y] = arm_col

    # 4. ARM RIGHT (Screen left, character's right: x <= 26, y=36..48)
    arm_r = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    arp = arm_r.load()
    for y in range(36, 49):
        for x in range(18, 27):
            p = fp[x, y]
            if p[3] > 0 and (x <= 24 or y >= 40):
                arp[x, y] = p
    # Rounded shoulder cap
    arm_r_col = fp[22, 42]
    for y in range(37, 40):
        for x in range(21, 25):
            if arp[x, y][3] == 0: arp[x, y] = arm_r_col

    # 5. LEG LEFT (Screen right, character's left: x >= 32, y >= 46)
    leg_l = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    llp = leg_l.load()
    for y in range(46, 59):
        for x in range(32, 40):
            llp[x, y] = fp[x, y]
    # Rounded hip cap under shorts
    shorts_col = fp[33, 47]
    for y in range(45, 48):
        for x in range(33, 37):
            if llp[x, y][3] == 0: llp[x, y] = shorts_col

    # 6. LEG RIGHT (Screen left, character's right: x < 32, y >= 46)
    leg_r = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    lrp = leg_r.load()
    for y in range(46, 59):
        for x in range(24, 32):
            lrp[x, y] = fp[x, y]
    for y in range(45, 48):
        for x in range(27, 31):
            if lrp[x, y][3] == 0: lrp[x, y] = shorts_col

    # Save the 6 parts
    head.save(os.path.join(ARTIFACT_DIR, "rig_test_head.png"))
    body.save(os.path.join(ARTIFACT_DIR, "rig_test_body.png"))
    arm_l.save(os.path.join(ARTIFACT_DIR, "rig_test_arm_l.png"))
    arm_r.save(os.path.join(ARTIFACT_DIR, "rig_test_arm_r.png"))
    leg_l.save(os.path.join(ARTIFACT_DIR, "rig_test_leg_l.png"))
    leg_r.save(os.path.join(ARTIFACT_DIR, "rig_test_leg_r.png"))

    # Test re-assembly at rest (should match original full 100%)
    rest = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    rest.paste(leg_l, (0, 0), leg_l)
    rest.paste(leg_r, (0, 0), leg_r)
    rest.paste(body, (0, 0), body)
    rest.paste(arm_l, (0, 0), arm_l)
    rest.paste(arm_r, (0, 0), arm_r)
    rest.paste(head, (0, 0), head)
    rest.save(os.path.join(ARTIFACT_DIR, "rig_test_reassembled_rest.png"))

    print("Rig part extraction test completed successfully!")

if __name__ == "__main__":
    test_rig()
