"""
스테이지 바닥용 반복(타일) 텍스처를 생성한다. (Stage1 잔디 / Stage2 얼음)
바닥에 무늬가 생겨서 캐릭터가 올라갈 때 '움직이는' 느낌을 준다.

사용법:
  1) pip install openai
  2) .env 에 OPENAI_API_KEY (이미 있음)
  3) python Tools/generate_ground.py
결과: Assets/Art/Ground/grass.png, ice.png
그 다음 Unity 메뉴 Tools > Rock Falldown > Import & Assign Ground Textures
"""

import os
import sys
import base64
import pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / "Assets" / "Art" / "Ground"


def load_env():
    p = ROOT / ".env"
    if p.exists():
        for line in p.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, v = line.split("=", 1)
                os.environ.setdefault(k.strip(), v.strip())


TEXTURES = [
    ("grass",
     "seamless tileable top-down ground texture of a flagstone path: irregular flat grey "
     "slate stones with green grass growing in the gaps between them, mixed stone and grass, "
     "natural and varied, flat even lighting, no harsh shadows, repeating pattern, "
     "game ground texture, edges match for tiling"),
    ("ice",
     "seamless tileable top-down frozen ground texture, pale blue ice mixed with patches of "
     "white frost and snow, subtle cracks and slightly darker frozen water areas, varied "
     "organic pattern, not uniform, flat even lighting, no harsh shadows, repeating pattern, "
     "game ground texture, edges match for tiling"),
]


def main():
    load_env()
    key = os.environ.get("OPENAI_API_KEY")
    if not key:
        print("ERROR: .env 에 OPENAI_API_KEY 없음"); sys.exit(1)
    try:
        from openai import OpenAI
    except ImportError:
        print("ERROR: pip install openai"); sys.exit(1)

    model = os.environ.get("IMAGE_MODEL", "gpt-image-1")
    client = OpenAI(api_key=key)
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    for name, prompt in TEXTURES:
        print(f"[{name}] 생성 중...")
        res = client.images.generate(model=model, prompt=prompt, size="1024x1024", n=1)
        path = OUT_DIR / f"{name}.png"
        path.write_bytes(base64.b64decode(res.data[0].b64_json))
        print(f"  완료: {path.name}")

    print("\n바닥 텍스처 생성 완료 →", OUT_DIR)


if __name__ == "__main__":
    main()
