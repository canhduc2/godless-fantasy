import os
import subprocess
import glob

netstandard = r"D:\UnityHub\Editor\6000.6.5f1\Editor\Data\NetStandard\ref\2.1.0\netstandard.dll"
managed_dir = r"D:\UnityHub\Editor\6000.6.5f1\Editor\Data\Managed"
managed_engine_dir = r"D:\UnityHub\Editor\6000.6.5f1\Editor\Data\Managed\UnityEngine"

ref_candidates = [
    netstandard,
    os.path.join(managed_dir, "UnityEngine.dll")
]

if os.path.exists(managed_engine_dir):
    for dll in glob.glob(os.path.join(managed_engine_dir, "*.dll")):
        ref_candidates.append(dll)

refs = [f"/r:{dll}" for dll in ref_candidates if os.path.exists(dll)]

src_files = glob.glob(r"client\GodlessFantasyClient\Assets\Scripts\**\*.cs", recursive=True)

csc_dll = r"C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll"
out_dll = r"scratch\test_client_build.dll"

cmd = ["dotnet", csc_dll, "/target:library", f"/out:{out_dll}", "/langversion:latest", "/noconfig", "/nostdlib"] + refs + src_files

print(f"Compiling {len(src_files)} C# files with {len(refs)} references...")
res = subprocess.run(cmd, capture_output=True, text=True)
if res.returncode == 0:
    print("Compilation SUCCESSFUL! Zero errors.")
else:
    print(f"Compilation finished with return code {res.returncode}:")
    lines = res.stdout.strip().split("\n")
    for l in lines[:30]:
        print(l)
    if len(lines) > 30:
        print(f"... and {len(lines) - 30} more lines.")
