import http.server
import socketserver
import os
import json
import urllib.parse
import base64
import shutil
from PIL import Image

import export_rig

PORT = 8080
WORKSHOP_DIR = os.path.dirname(os.path.abspath(__file__))
STATIC_DIR = os.path.join(WORKSHOP_DIR, 'static')
PARTS_DIR = os.path.join(WORKSHOP_DIR, 'parts')
PARTS_BACKUP_DIR = os.path.join(WORKSHOP_DIR, 'parts_backup')
CONFIG_PATH = os.path.join(WORKSHOP_DIR, 'config.json')
DEFAULT_CONFIG_PATH = os.path.join(WORKSHOP_DIR, 'default_config.json')
ANIM_CONFIG_PATH = os.path.join(WORKSHOP_DIR, 'animation_config.json')
DEFAULT_ANIM_CONFIG_PATH = os.path.join(WORKSHOP_DIR, 'default_anim_config.json')

# Ensure backup of parts exists
os.makedirs(PARTS_BACKUP_DIR, exist_ok=True)
if os.path.exists(PARTS_DIR):
    for f in os.listdir(PARTS_DIR):
        if f.endswith('.png'):
            dst = os.path.join(PARTS_BACKUP_DIR, f)
            if not os.path.exists(dst):
                shutil.copy2(os.path.join(PARTS_DIR, f), dst)

class RigWorkshopHandler(http.server.SimpleHTTPRequestHandler):
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
            elif fpath.endswith('.html'): ctype = 'text/html'
            self.serve_file(fpath, ctype)
            return

        if path.startswith('/parts/'):
            fname = path[len('/parts/'):]
            fpath = os.path.join(PARTS_DIR, fname)
            if not os.path.exists(fpath) and not fname.endswith('.png'):
                fpath = os.path.join(PARTS_DIR, f"{fname}.png")
            self.serve_file(fpath, 'image/png')
            return

        if path == '/api/config':
            cfg_file = CONFIG_PATH if os.path.exists(CONFIG_PATH) else DEFAULT_CONFIG_PATH
            with open(cfg_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            self.send_json(data)
            return

        if path == '/api/default_config':
            with open(DEFAULT_CONFIG_PATH, 'r', encoding='utf-8') as f:
                data = json.load(f)
            self.send_json(data)
            return

        if path == '/api/anim_config':
            cfg_file = ANIM_CONFIG_PATH if os.path.exists(ANIM_CONFIG_PATH) else DEFAULT_ANIM_CONFIG_PATH
            if not os.path.exists(cfg_file):
                cfg_file = DEFAULT_ANIM_CONFIG_PATH
            with open(cfg_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            self.send_json(data)
            return

        if path == '/api/default_anim_config':
            with open(DEFAULT_ANIM_CONFIG_PATH, 'r', encoding='utf-8') as f:
                data = json.load(f)
            self.send_json(data)
            return

        if path == '/api/parts_list':
            parts = []
            if os.path.exists(PARTS_DIR):
                for f in sorted(os.listdir(PARTS_DIR)):
                    if f.endswith('.png'):
                        name = f[:-4]
                        fp = os.path.join(PARTS_DIR, f)
                        im = Image.open(fp)
                        parts.append({
                            "name": name,
                            "file": name,
                            "width": im.width,
                            "height": im.height
                        })
            self.send_json(parts)
            return

        self.send_error(404, "File not found")

    def do_POST(self):
        parsed = urllib.parse.urlparse(self.path)
        path = parsed.path

        content_length = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(content_length)

        if path == '/api/save':
            try:
                config_data = json.loads(body.decode('utf-8'))
                with open(CONFIG_PATH, 'w', encoding='utf-8') as f:
                    json.dump(config_data, f, indent=2, ensure_ascii=False)

                # Trigger Unity export
                export_rig.export_all_rigs(config_data)

                self.send_json({
                    "success": True,
                    "message": "Đã lưu cấu hình và xuất toàn bộ 16 bộ rig vào Unity Assets thành công!"
                })
            except Exception as e:
                self.send_json({"success": False, "error": str(e)}, status=500)
            return

        if path == '/api/save_part':
            try:
                data = json.loads(body.decode('utf-8'))
                part_name = data.get('part_name')
                b64_data = data.get('image_data', '')

                if not part_name or not b64_data:
                    self.send_json({"success": False, "error": "Thiếu dữ liệu ảnh hoặc tên mảnh"}, status=400)
                    return

                if ',' in b64_data:
                    b64_data = b64_data.split(',', 1)[1]

                img_bytes = base64.b64decode(b64_data)
                out_path = os.path.join(PARTS_DIR, f"{part_name}.png")
                with open(out_path, 'wb') as f:
                    f.write(img_bytes)

                # Re-export Unity rigs
                export_rig.export_all_rigs()

                self.send_json({
                    "success": True,
                    "message": f"Đã lưu mảnh {part_name} và cập nhật toàn bộ Rig!"
                })
            except Exception as e:
                self.send_json({"success": False, "error": str(e)}, status=500)
            return

        if path == '/api/revert_part':
            try:
                data = json.loads(body.decode('utf-8'))
                part_name = data.get('part_name')
                src = os.path.join(PARTS_BACKUP_DIR, f"{part_name}.png")
                dst = os.path.join(PARTS_DIR, f"{part_name}.png")
                if os.path.exists(src):
                    shutil.copy2(src, dst)
                    export_rig.export_all_rigs()
                    self.send_json({"success": True, "message": f"Đã khôi phục mảnh {part_name} về ảnh gốc!"})
                else:
                    self.send_json({"success": False, "error": "Không tìm thấy bản lưu gốc"}, status=404)
            except Exception as e:
                self.send_json({"success": False, "error": str(e)}, status=500)
            return

        if path == '/api/save_anim_config':
            try:
                anim_data = json.loads(body.decode('utf-8'))
                with open(ANIM_CONFIG_PATH, 'w', encoding='utf-8') as f:
                    json.dump(anim_data, f, indent=2, ensure_ascii=False)
                
                # Copy directly into Unity Resources
                unity_anim_path = os.path.join(export_rig.CLIENT_RIG_ROOT, "animation_config.json")
                if os.path.exists(export_rig.CLIENT_RIG_ROOT):
                    with open(unity_anim_path, 'w', encoding='utf-8') as f:
                        json.dump(anim_data, f, indent=2, ensure_ascii=False)

                self.send_json({
                    "success": True,
                    "message": "Đã lưu cấu hình hoạt ảnh và xuất thẳng ra Unity Resources! Nhấn F5 trong Unity để nạp ngay."
                })
            except Exception as e:
                self.send_json({"success": False, "error": str(e)}, status=500)
            return

        if path == '/api/reset':
            try:
                with open(DEFAULT_CONFIG_PATH, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                with open(CONFIG_PATH, 'w', encoding='utf-8') as f:
                    json.dump(data, f, indent=2, ensure_ascii=False)
                export_rig.export_all_rigs(data)
                self.send_json({"success": True, "config": data, "message": "Đã khôi phục cài đặt mặc định!"})
            except Exception as e:
                self.send_json({"success": False, "error": str(e)}, status=500)
            return

        self.send_error(404, "Endpoint not found")

    def serve_file(self, filepath, content_type):
        if not os.path.exists(filepath):
            self.send_error(404, "File not found")
            return
        try:
            with open(filepath, 'rb') as f:
                content = f.read()
            self.send_response(200)
            self.send_header('Content-Type', content_type)
            self.send_header('Content-Length', str(len(content)))
            self.end_headers()
            self.wfile.write(content)
        except Exception as e:
            self.send_error(500, f"Error reading file: {e}")

    def send_json(self, data, status=200):
        content = json.dumps(data, ensure_ascii=False).encode('utf-8')
        self.send_response(status)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.send_header('Content-Length', str(len(content)))
        self.end_headers()
        self.wfile.write(content)

def run_server():
    os.chdir(WORKSHOP_DIR)
    # Allow port reuse immediately
    socketserver.TCPServer.allow_reuse_address = True
    with socketserver.TCPServer(("", PORT), RigWorkshopHandler) as httpd:
        print(f"============================================================")
        print(f"CHIBI RIG WORKSHOP SERVER RUNNING AT: http://localhost:{PORT}")
        print(f"============================================================")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nStopping server...")
            httpd.server_close()

if __name__ == '__main__':
    run_server()
