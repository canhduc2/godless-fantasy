import os, json
from PIL import Image

PARTS_DIR = 'scratch/rig_workshop/parts'
os.makedirs('scratch/rig_workshop', exist_ok=True)

# Helper to get part dimensions
def get_size(fname):
    p = os.path.join(PARTS_DIR, f"{fname}.png")
    if os.path.exists(p):
        return Image.open(p).size
    return (16, 16)

def build_default_config():
    config = {
        "female": {},
        "male": {}
    }

    # -------------------------------------------------------------
    # 1. FEMALE DEFAULTS
    # -------------------------------------------------------------
    # South (Front)
    # Head 16x22, Torso 13x16, ArmL 6x15, ArmR 6x15, LegL 6x18, LegR 7x18
    # Center X = 32
    f_south = {
        "idle_head":      {"file": "female_head_front",  "x": 24, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "female_torso_front", "x": 25, "y": 25, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "female_arml_front",  "x": 19, "y": 25, "z": 13, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "female_armr_front",  "x": 38, "y": 25, "z": 13, "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "female_legl_front",  "x": 25, "y": 38, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "female_legr_front",  "x": 32, "y": 38, "z": 10, "visible": True, "flip_x": False},
    }

    # East (Right Side)
    # Head 16x22, Torso 9x16, ArmNear 6x15, ArmFar 6x15, LegNear 6x18, LegFar 7x18
    f_east = {
        "idle_head":      {"file": "female_head_side",  "x": 24, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "female_torso_side", "x": 26, "y": 25, "z": 11, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "female_arm_near",   "x": 25, "y": 25, "z": 13, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "female_arm_far",    "x": 31, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "female_leg_near",   "x": 27, "y": 38, "z": 12, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "female_leg_far",    "x": 31, "y": 38, "z": 8,  "visible": True, "flip_x": False},
    }

    # North (Back)
    f_north = {
        "idle_head":      {"file": "female_head_back",   "x": 24, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "female_torso_back",  "x": 25, "y": 25, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "female_arml_back",   "x": 19, "y": 25, "z": 9,  "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "female_armr_back",   "x": 38, "y": 25, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "female_legl_back",   "x": 25, "y": 38, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "female_legr_back",   "x": 32, "y": 38, "z": 10, "visible": True, "flip_x": False},
    }

    # South-East (Diagonal Front-Right)
    f_se = {
        "idle_head":      {"file": "female_head_34",     "x": 24, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "female_torso_front", "x": 25, "y": 25, "z": 11, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "female_arml_front",  "x": 21, "y": 25, "z": 13, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "female_arm_far",     "x": 36, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "female_legl_front",  "x": 26, "y": 38, "z": 12, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "female_leg_far",     "x": 32, "y": 38, "z": 8,  "visible": True, "flip_x": False},
    }

    # North-East (Diagonal Back-Right)
    f_ne = {
        "idle_head":      {"file": "female_head_back",   "x": 24, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "female_torso_back",  "x": 25, "y": 25, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "female_arml_back",   "x": 21, "y": 25, "z": 9,  "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "female_arm_far",     "x": 36, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "female_legl_back",   "x": 26, "y": 38, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "female_leg_far",     "x": 32, "y": 38, "z": 8,  "visible": True, "flip_x": False},
    }

    # Mirroring helper for West, North-West, South-West
    def mirror_dir(src):
        m = {}
        for k, v in src.items():
            # flip arm and leg keys
            out_key = k
            if k == "idle_arm_left": out_key = "idle_arm_right"
            elif k == "idle_arm_right": out_key = "idle_arm_left"
            elif k == "idle_leg_left": out_key = "idle_leg_right"
            elif k == "idle_leg_right": out_key = "idle_leg_left"

            # calculate mirrored X: 64 - x - width
            w, _ = get_size(v["file"])
            mirrored_x = 64 - v["x"] - w
            m[out_key] = {
                "file": v["file"],
                "x": mirrored_x,
                "y": v["y"],
                "z": v["z"],
                "visible": v["visible"],
                "flip_x": True
            }
        return m

    config["female"]["south"]      = f_south
    config["female"]["south_east"] = f_se
    config["female"]["east"]       = f_east
    config["female"]["north_east"] = f_ne
    config["female"]["north"]      = f_north
    config["female"]["north_west"] = mirror_dir(f_ne)
    config["female"]["west"]       = mirror_dir(f_east)
    config["female"]["south_west"] = mirror_dir(f_se)

    # -------------------------------------------------------------
    # 2. MALE DEFAULTS (FULL UNCUT TORSO!)
    # -------------------------------------------------------------
    # South (Front)
    # Head 21x19, Torso 16x19, ArmL 8x16, ArmR 8x16, LegL 9x17, LegR 9x17
    m_south = {
        "idle_head":      {"file": "male_head_front",  "x": 22, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "male_torso_front", "x": 24, "y": 24, "z": 11, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "male_arml_front",  "x": 19, "y": 24, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "male_armr_front",  "x": 37, "y": 24, "z": 12, "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "male_legl_front",  "x": 23, "y": 39, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "male_legr_front",  "x": 32, "y": 39, "z": 10, "visible": True, "flip_x": False},
    }

    # East (Side)
    # Head 20x20, Torso 12x20, ArmNear 15x14, ArmFar 16x12, LegNear 13x18, LegFar 11x17
    m_east = {
        "idle_head":      {"file": "male_head_side",  "x": 22, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "male_torso_side", "x": 26, "y": 24, "z": 11, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "male_arm_near",   "x": 24, "y": 25, "z": 13, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "male_arm_far",    "x": 29, "y": 25, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "male_leg_near",   "x": 25, "y": 39, "z": 12, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "male_leg_far",    "x": 28, "y": 39, "z": 8,  "visible": True, "flip_x": False},
    }

    # North (Back)
    m_north = {
        "idle_head":      {"file": "male_head_back",   "x": 22, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "male_torso_back",  "x": 24, "y": 24, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "male_arml_back",   "x": 19, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "male_armr_back",   "x": 37, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "male_legl_back",   "x": 23, "y": 39, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "male_legr_back",   "x": 32, "y": 39, "z": 10, "visible": True, "flip_x": False},
    }

    # South-East (Diagonal Front-Right)
    m_se = {
        "idle_head":      {"file": "male_head_front",  "x": 22, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "male_torso_front", "x": 24, "y": 24, "z": 11, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "male_arml_front",  "x": 20, "y": 24, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "male_arm_far",     "x": 35, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "male_legl_front",  "x": 24, "y": 39, "z": 12, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "male_leg_far",     "x": 30, "y": 39, "z": 8,  "visible": True, "flip_x": False},
    }

    # North-East (Diagonal Back-Right)
    m_ne = {
        "idle_head":      {"file": "male_head_back",   "x": 22, "y": 8,  "z": 14, "visible": True, "flip_x": False},
        "idle_body":      {"file": "male_torso_back",  "x": 24, "y": 24, "z": 12, "visible": True, "flip_x": False},
        "idle_arm_left":  {"file": "male_arml_back",   "x": 20, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_arm_right": {"file": "male_arm_far",     "x": 35, "y": 24, "z": 9,  "visible": True, "flip_x": False},
        "idle_leg_left":  {"file": "male_legl_back",   "x": 24, "y": 39, "z": 10, "visible": True, "flip_x": False},
        "idle_leg_right": {"file": "male_leg_far",     "x": 30, "y": 39, "z": 8,  "visible": True, "flip_x": False},
    }

    config["male"]["south"]      = m_south
    config["male"]["south_east"] = m_se
    config["male"]["east"]       = m_east
    config["male"]["north_east"] = m_ne
    config["male"]["north"]      = m_north
    config["male"]["north_west"] = mirror_dir(m_ne)
    config["male"]["west"]       = mirror_dir(m_east)
    config["male"]["south_west"] = mirror_dir(m_se)

    out_file = 'scratch/rig_workshop/default_config.json'
    with open(out_file, 'w', encoding='utf-8') as f:
        json.dump(config, f, indent=2, ensure_ascii=False)
    print(f"Generated default rig config at {out_file}")

if __name__ == '__main__':
    build_default_config()
