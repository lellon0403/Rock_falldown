"""
Stage 5 천국(천상계) 테마 바닥 텍스처 생성.
  - heaven.png : 푸른 하늘 + 뭉게구름 + 빛줄기(god rays) 느낌의 타일 텍스처

모델은 반드시 gpt-image-2 사용. (gpt-image-1 금지)

사용법:
  1) pip install openai
  2) .env 에 OPENAI_API_KEY (이미 있음)
  3) python Tools/generate_heaven.py
결과: Assets/Art/Ground/heaven.png
그 다음 Unity 메뉴:
  - Tools > Rock Falldown > Import & Assign Ground Textures
"""

import os
import sys
import base64
import pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent


def load_env():
    p = ROOT / ".env"
    if p.exists():
        for line in p.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, v = line.split("=", 1)
                os.environ.setdefault(k.strip(), v.strip())


IMAGES = [
    (ROOT / "Assets" / "Art" / "Ground" / "heaven.png",
     "seamless tileable top-down heavenly sky texture, bright blue sky with soft fluffy "
     "white cumulus clouds, radiant golden sun rays and god rays beaming through the clouds, "
     "divine ethereal paradise atmosphere, soft glowing light, dreamy and serene, flat even "
     "lighting, no harsh shadows, repeating pattern, game ground texture, edges match for tiling"),
]

MODEL = os.environ.get("IMAGE_MODEL", "gpt-image-2")   # 반드시 gpt-image-2


def main():
    load_env()
    key = os.environ.get("OPENAI_API_KEY")
    if not key:
        print("ERROR: .env 에 OPENAI_API_KEY 없음"); sys.exit(1)
    try:
        from openai import OpenAI
    except ImportError:
        print("ERROR: pip install openai"); sys.exit(1)

    if MODEL != "gpt-image-2":
        print(f"ERROR: model must be gpt-image-2 (current: {MODEL})"); sys.exit(1)

    client = OpenAI(api_key=key)

    for path, prompt in IMAGES:
        path.parent.mkdir(parents=True, exist_ok=True)
        if path.exists():
            print(f"[{path.name}] already exists - skip")
            continue
        print(f"[{path.name}] generating... (model={MODEL})")
        res = client.images.generate(model=MODEL, prompt=prompt, size="1024x1024", n=1)
        path.write_bytes(base64.b64decode(res.data[0].b64_json))
        print(f"  done: {path}")

    print("\nHeaven texture done.")


if __name__ == "__main__":
    main()
