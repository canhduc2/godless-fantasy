import os
import urllib.request
import json
from PIL import Image

OUT_DIR = r"d:\godless-fantasy\client\GodlessFantasyClient\Assets\Resources\Blocks"
os.makedirs(OUT_DIR, exist_ok=True)

IMG_URL = "https://api.pixellab.ai/mcp/tilesets/31501962-8635-4e0e-8020-1bc88e460fa2/image"
SHEET_PATH = os.path.join(OUT_DIR, "tileset_water_grass_sheet.png")

print(f"Downloading {IMG_URL}...")
req = urllib.request.Request(IMG_URL, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req) as resp, open(SHEET_PATH, 'wb') as f:
    f.write(resp.read())

print(f"Saved sheet to {SHEET_PATH}")
sheet = Image.open(SHEET_PATH)
w, h = sheet.size
print(f"Sheet dimensions: {w}x{h}")

# The spritesheet is a 4x8 grid of 32x32 tiles (25 tiles total)
tile_w = 32
tile_h = 32
cols = w // tile_w
rows = h // tile_h

tile_index = 0
for r in range(rows):
    for c in range(cols):
        left = c * tile_w
        top = r * tile_h
        right = left + tile_w
        bottom = top + tile_h
        tile_crop = sheet.crop((left, top, right, bottom))
        
        # Check if crop has content
        if tile_crop.getbbox():
            tile_filename = f"mcp_water_grass_{tile_index:02d}.png"
            tile_crop.save(os.path.join(OUT_DIR, tile_filename))
            print(f"Saved tile {tile_filename}")
            tile_index += 1

print(f"Total tiles extracted: {tile_index}")
