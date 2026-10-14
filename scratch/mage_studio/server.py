import http.server
import socketserver
import os
import json
import urllib.parse
import base64
import shutil
from PIL import Image
import io

PORT = 8080
STUDIO_DIR = os.path.dirname(os.path.abspath(__file__))
STATIC_DIR = os.path.join(STUDIO_DIR, 'static')
BACKUP_DIR = os.path.join(STUDIO_DIR, 'backup')

# Thư mục gốc Unity Mage Sprites
PROJECT_ROOT = os.path.abspath(os.path.join(STUDIO_DIR, '..', '..'))
UNITY_MAGE_DIR = os.path.join(PROJECT_ROOT, 'client', 'GodlessFantasyClient', 'Assets', 'Resources', 'Characters', 'Mage')

DIRECTIONS = ['south', 'south_west', 'west', 'north_west', 'north', 'north_east', 'east', 'south_east']
GENDERS = ['male', 'female']

class MageStudioHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        super().end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.end_headers()

    def do_GET(self):
        parsed = urllib.parse.urlparse(self.path)
        path = parsed.path
        query = urllib.parse.parse_qs(parsed.query)

        if path == '/' or path == '/index.html':
            self.serve_file(os.path.join(STATIC_DIR, 'index.html'), 'text/html; charset=utf-8')
            return

        if path.startswith('/static/'):
            rel = path[len('/static/'):]
            fpath = os.path.join(STATIC_DIR, rel)
            ctype = 'text/plain'
            if fpath.endswith('.css'): ctype = 'text/css'
            elif fpath.endswith('.js'): ctype = 'application/javascript'
            elif fpath.endswith('.png'): ctype = 'image/png'
            elif fpath.endswith('.svg'): ctype = 'image/svg+xml'
            elif fpath.endswith('.html'): ctype = 'text/html'
            self.serve_file(fpath, ctype)
            return

        if path == '/api/get_frame':
            gender = query.get('gender', ['male'])[0]
            direction = query.get('direction', ['south'])[0]
            frame = query.get('frame', ['0'])[0]

            frame_path = os.path.join(UNITY_MAGE_DIR, gender, f'mage_{direction}_{frame}.png')
            if not os.path.exists(frame_path):
                # Fallback to root Mage dir
                frame_path = os.path.join(UNITY_MAGE_DIR, f'mage_{direction}_{frame}.png')

            if os.path.exists(frame_path):
                self.serve_file(frame_path, 'image/png')
            else:
                self.send_error(404, f'Frame not found: {gender}/{direction}/{frame}')
            return

        if path == '/api/frames_info':
            gender = query.get('gender', ['male'])[0]
            direction = query.get('direction', ['south'])[0]

            frames_data = []
            for f in range(8):
                f_path = os.path.join(UNITY_MAGE_DIR, gender, f'mage_{direction}_{f}.png')
                exists = os.path.exists(f_path)
                mtime = os.path.getmtime(f_path) if exists else 0
                frames_data.append({
                    'index': f,
                    'name': f'mage_{direction}_{f}.png',
                    'exists': exists,
                    'mtime': int(mtime * 1000)
                })

            resp = {
                'gender': gender,
                'direction': direction,
                'directions': DIRECTIONS,
                'genders': GENDERS,
                'frames': frames_data
            }
            self.send_json(resp)
            return

        self.send_error(404, 'Endpoint Not Found')

    def do_POST(self):
        parsed = urllib.parse.urlparse(self.path)
        path = parsed.path

        length = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(length).decode('utf-8')
        try:
            data = json.loads(body) if body else {}
        except Exception as e:
            self.send_json({'error': f'Invalid JSON: {str(e)}'}, 400)
            return

        if path == '/api/save_frame':
            gender = data.get('gender', 'male')
            direction = data.get('direction', 'south')
            frame = int(data.get('frame', 0))
            img_b64 = data.get('image_data', '')

            if not img_b64:
                self.send_json({'error': 'No image data provided'}, 400)
                return

            if ',' in img_b64:
                img_b64 = img_b64.split(',', 1)[1]

            img_bytes = base64.b64decode(img_b64)
            img = Image.open(io.BytesIO(img_bytes)).convert('RGBA')

            # Đảm bảo kích thước 160x160 chuẩn
            if img.size != (160, 160):
                img = img.resize((160, 160), Image.NEAREST)

            gender_dir = os.path.join(UNITY_MAGE_DIR, gender)
            os.makedirs(gender_dir, exist_ok=True)
            dst_file = os.path.join(gender_dir, f'mage_{direction}_{frame}.png')

            img.save(dst_file, 'PNG')
            print(f'[MAGE STUDIO] Saved frame {gender}/{direction}_{frame} -> {dst_file}')

            self.send_json({
                'success': True,
                'message': f'Đã lưu thành công Frame {frame} hướng {direction} ({gender})!',
                'gender': gender,
                'direction': direction,
                'frame': frame,
                'mtime': int(os.path.getmtime(dst_file) * 1000)
            })
            return

        if path == '/api/mirror_direction':
            gender = data.get('gender', 'male')
            source_dir = data.get('source_direction')
            target_dir = data.get('target_direction')

            if not source_dir or not target_dir:
                self.send_json({'error': 'Cần chỉ định source_direction và target_direction'}, 400)
                return

            gender_dir = os.path.join(UNITY_MAGE_DIR, gender)
            mirrored_count = 0

            for f in range(8):
                src_path = os.path.join(gender_dir, f'mage_{source_dir}_{f}.png')
                if not os.path.exists(src_path):
                    src_path = os.path.join(UNITY_MAGE_DIR, f'mage_{source_dir}_{f}.png')

                if os.path.exists(src_path):
                    im = Image.open(src_path).convert('RGBA')
                    flipped = im.transpose(Image.FLIP_LEFT_RIGHT)
                    dst_path = os.path.join(gender_dir, f'mage_{target_dir}_{f}.png')
                    flipped.save(dst_path, 'PNG')
                    mirrored_count += 1

            self.send_json({
                'success': True,
                'message': f'Đã sao chép đối xứng thành công {mirrored_count} frame từ {source_dir} sang {target_dir} ({gender})!',
                'count': mirrored_count
            })
            return

        if path == '/api/copy_gender':
            src_gender = data.get('source_gender', 'male')
            dst_gender = data.get('target_gender', 'female')

            if src_gender == dst_gender:
                self.send_json({'error': 'Giới tính nguồn và đích phải khác nhau'}, 400)
                return

            src_dir = os.path.join(UNITY_MAGE_DIR, src_gender)
            dst_dir = os.path.join(UNITY_MAGE_DIR, dst_gender)
            os.makedirs(dst_dir, exist_ok=True)

            copied = 0
            for d in DIRECTIONS:
                for f in range(8):
                    f_name = f'mage_{d}_{f}.png'
                    src_f = os.path.join(src_dir, f_name)
                    if os.path.exists(src_f):
                        shutil.copy2(src_f, os.path.join(dst_dir, f_name))
                        copied += 1

            self.send_json({
                'success': True,
                'message': f'Đã sao chép thành công {copied} frame từ {src_gender} sang {dst_gender}!',
                'count': copied
            })
            return

        if path == '/api/restore_backup':
            gender = data.get('gender', 'male')
            direction = data.get('direction', 'all')
            frame = data.get('frame', 'all')

            b_dir = os.path.join(BACKUP_DIR, gender)
            dst_dir = os.path.join(UNITY_MAGE_DIR, gender)
            os.makedirs(dst_dir, exist_ok=True)

            restored = 0
            for d in (DIRECTIONS if direction == 'all' else [direction]):
                for f in (range(8) if frame == 'all' else [int(frame)]):
                    f_name = f'mage_{d}_{f}.png'
                    b_file = os.path.join(b_dir, f_name)
                    if os.path.exists(b_file):
                        shutil.copy2(b_file, os.path.join(dst_dir, f_name))
                        restored += 1

            self.send_json({
                'success': True,
                'message': f'Đã khôi phục {restored} frame từ bản gốc an toàn!',
                'count': restored
            })
            return

        if path == '/api/export_sheet':
            gender = data.get('gender', 'male')
            gender_dir = os.path.join(UNITY_MAGE_DIR, gender)

            sheet = Image.new('RGBA', (160 * 8, 160 * 8), (0, 0, 0, 0))
            for row, d in enumerate(DIRECTIONS):
                for col in range(8):
                    f_path = os.path.join(gender_dir, f'mage_{d}_{col}.png')
                    if os.path.exists(f_path):
                        frame_img = Image.open(f_path).convert('RGBA')
                        sheet.paste(frame_img, (col * 160, row * 160))

            dst_sheet = os.path.join(gender_dir, 'mage_sheet_8dir.png')
            sheet.save(dst_sheet, 'PNG')

            self.send_json({
                'success': True,
                'message': f'Đã xuất thành công Sprite Sheet 8 Hướng (1280x1280) cho {gender}!',
                'path': dst_sheet
            })
            return

        self.send_error(404, 'Endpoint Not Found')

    def serve_file(self, fpath, ctype):
        if not os.path.exists(fpath):
            self.send_error(404, f'File Not Found: {fpath}')
            return
        with open(fpath, 'rb') as f:
            content = f.read()
        self.send_response(200)
        self.send_header('Content-Type', ctype)
        self.send_header('Content-Length', str(len(content)))
        self.end_headers()
        self.wfile.write(content)

    def send_json(self, data, status=200):
        body = json.dumps(data, ensure_ascii=False).encode('utf-8')
        self.send_response(status)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Content-Length', str(len(body)))
        self.end_headers()
        self.wfile.write(body)

def run():
    socketserver.TCPServer.allow_reuse_address = True
    with socketserver.TCPServer(('', PORT), MageStudioHandler) as httpd:
        print(f'==============================================================')
        print(f'   MAGE 8-DIRECTION SPRITE STUDIO RUNNING ON PORT {PORT}      ')
        print(f'   Open in browser: http://localhost:{PORT}                   ')
        print(f'==============================================================')
        httpd.serve_forever()

if __name__ == '__main__':
    run()
