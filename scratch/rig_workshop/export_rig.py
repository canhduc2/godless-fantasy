import os, json
from PIL import Image

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__)) # d:\godless-fantasy\scratch\rig_workshop
ROOT_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, '..', '..')) # d:\godless-fantasy

PARTS_DIR = os.path.join(SCRIPT_DIR, 'parts')
CONFIG_PATH = os.path.join(SCRIPT_DIR, 'config.json')
DEFAULT_CONFIG_PATH = os.path.join(SCRIPT_DIR, 'default_config.json')
CLIENT_RIG_ROOT = os.path.join(ROOT_DIR, 'client', 'GodlessFantasyClient', 'Assets', 'Resources', 'Characters', 'Rig')
PREVIEWS_DIR = os.path.join(ROOT_DIR, 'scratch', 'final_rig_previews')
STATIC_DIR = os.path.join(SCRIPT_DIR, 'static')

os.makedirs(PREVIEWS_DIR, exist_ok=True)
os.makedirs(STATIC_DIR, exist_ok=True)

PART_KEYS = ['idle_head', 'idle_body', 'idle_arm_left', 'idle_arm_right', 'idle_leg_left', 'idle_leg_right']
DIRECTIONS = ['south', 'south_east', 'east', 'north_east', 'north', 'north_west', 'west', 'south_west']

def export_all_rigs(config=None, config_path=None):
    if config is None:
        if config_path is None:
            config_path = CONFIG_PATH if os.path.exists(CONFIG_PATH) else DEFAULT_CONFIG_PATH
        with open(config_path, 'r', encoding='utf-8') as f:
            config = json.load(f)

    # Fresh image loader (do not stale cache)
    def get_part_img(file_name, flip_x=False, flip_y=False):
        p = os.path.join(PARTS_DIR, f"{file_name}.png")
        if not os.path.exists(p):
            return Image.new('RGBA', (1, 1), (0, 0, 0, 0))
        im = Image.open(p).copy() # Load fresh
        if flip_x:
            im = im.transpose(Image.FLIP_LEFT_RIGHT)
        if flip_y:
            im = im.transpose(Image.FLIP_TOP_BOTTOM)
        return im

    composite_sprites = {} # (gender, dir) -> 64x64 Image

    for gender in ['female', 'male']:
        g_data = config.get(gender, {})
        for direction in DIRECTIONS:
            d_data = g_data.get(direction, {})
            target_dir = os.path.join(CLIENT_RIG_ROOT, gender, direction)
            os.makedirs(target_dir, exist_ok=True)

            # Sort parts by Z ascending for composite
            parts_sorted = []
            for part_key in PART_KEYS:
                part_conf = d_data.get(part_key, {})
                z = part_conf.get('z', 10)
                parts_sorted.append((z, part_key, part_conf))
            parts_sorted.sort(key=lambda item: item[0])

            # 1. Build composite 64x64 body
            composite = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
            for z, part_key, part_conf in parts_sorted:
                if not part_conf.get('visible', True):
                    continue
                file_name = part_conf.get('file', '')
                if not file_name: continue
                img = get_part_img(file_name, part_conf.get('flip_x', False), part_conf.get('flip_y', False))
                x = int(part_conf.get('x', 0))
                y = int(part_conf.get('y', 0))
                layer = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
                layer.paste(img, (x, y))
                composite = Image.alpha_composite(composite, layer)

            composite.save(os.path.join(target_dir, 'body.png'))
            composite_sprites[(gender, direction)] = composite

            # 2. Build individual 64x64 part files for Unity
            for part_key in PART_KEYS:
                part_conf = d_data.get(part_key, {})
                part_canvas = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
                if part_conf.get('visible', True):
                    file_name = part_conf.get('file', '')
                    if file_name:
                        img = get_part_img(file_name, part_conf.get('flip_x', False), part_conf.get('flip_y', False))
                        x = int(part_conf.get('x', 0))
                        y = int(part_conf.get('y', 0))
                        part_canvas.paste(img, (x, y))
                part_canvas.save(os.path.join(target_dir, f"{part_key}.png"))

    # Also save rig_config.json and animation_config.json into Unity Resources for runtime access
    unity_config_path = os.path.join(CLIENT_RIG_ROOT, "rig_config.json")
    with open(unity_config_path, "w", encoding="utf-8") as f:
        json.dump(config, f, indent=2)

    anim_config_src = os.path.join(SCRIPT_DIR, "animation_config.json")
    if os.path.exists(anim_config_src):
        unity_anim_path = os.path.join(CLIENT_RIG_ROOT, "animation_config.json")
        with open(anim_config_src, "r", encoding="utf-8") as f_src, open(unity_anim_path, "w", encoding="utf-8") as f_dst:
            f_dst.write(f_src.read())
    tile_size = 192
    montage = Image.new('RGBA', (tile_size * 8, tile_size * 2), (238, 238, 238, 255))
    for row_idx, gender in enumerate(['female', 'male']):
        for col_idx, direction in enumerate(DIRECTIONS):
            comp = composite_sprites.get((gender, direction))
            if comp:
                scaled = comp.resize((tile_size, tile_size), Image.Resampling.NEAREST)
                montage.paste(scaled, (col_idx * tile_size, row_idx * tile_size), scaled)

    review_path = os.path.join(PREVIEWS_DIR, 'all_8_directions_review_3x.png')
    montage.save(review_path)

    # Also save to static folder for browser preview
    static_prev = os.path.join(STATIC_DIR, 'preview_8dir.png')
    montage.save(static_prev)

    print(f"Exported all rig parts to {CLIENT_RIG_ROOT}")
    print(f"Saved review montage to {review_path}")
    return review_path

if __name__ == '__main__':
    export_all_rigs()
