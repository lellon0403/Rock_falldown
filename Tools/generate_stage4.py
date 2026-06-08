"""
Stage 4 (우주 테마) 아트 생성.
  - space.png : 우주/은하 바닥 타일 텍스처 (어두운 바탕 + 보라/파랑 성운 + 별)
  - star.png  : 투명 바위(보일 때)용 별이 빛나는 구체 텍스처

모델은 반드시 gpt-image-2 사용. (gpt-image-1 금지)

사용법:
  1) pip install openai
  2) .env 에 OPENAI_API_KEY (이미 있음)
  3) python Tools/generate_stage4.py
결과: Assets/Art/Ground/space.png, Assets/Art/Rocks/star.png
그 다음 Unity 메뉴:
  - Tools > Rock Falldown > Import & Assign Ground Textures (바닥)
  - Tools > Rock Falldown > Setup Stage 4 Content (Phasing Rocks) (바위 머티리얼)
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


# (출력경로, 프롬프트)
IMAGES = [
    (ROOT / "Assets" / "Art" / "Ground" / "space.png",
     "seamless tileable top-down space galaxy texture, deep black cosmos filled with tiny "
     "twinkling white stars, soft purple and blue nebula clouds, faint milky way band, "
     "dark and atmospheric, flat even lighting, no harsh shadows, repeating pattern, "
     "game ground texture, edges match for tiling"),
    (ROOT / "Assets" / "Art" / "Rocks" / "star.png",
     "seamless tileable starfield texture for a sphere, very dark navy-black background "
     "densely covered with bright glowing white and pale blue stars of varied sizes, a few "
     "soft purple nebula wisps, sparkling cosmic look, flat even lighting, no shadows, "
     "uniform repeating pattern, game texture, edges match for tiling"),
    # Stage 5 바닥 — 더 화려한 성운
    (ROOT / "Assets" / "Art" / "Ground" / "space5.png",
     "seamless tileable top-down deep space nebula texture, swirling vivid purple, magenta "
     "and teal cosmic clouds with bright scattered stars and distant galaxies, rich and "
     "colorful nebula, dark background, flat even lighting, no harsh shadows, repeating "
     "pattern, game ground texture, edges match for tiling"),
    # 결승선 바닥 — 우주풍 체커보드
    (ROOT / "Assets" / "Art" / "Ground" / "finish.png",
     "seamless tileable top-down checkered finish-line floor texture, alternating glossy "
     "black and white squares with a subtle purple cosmic glow along the edges, racing "
     "checkerboard pattern, clean and even, flat lighting, no harsh shadows, repeating "
     "pattern, game ground texture, edges match for tiling"),
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
        print(f"ERROR: 모델은 gpt-image-2 여야 합니다 (현재: {MODEL})"); sys.exit(1)

    client = OpenAI(api_key=key)

    for path, prompt in IMAGES:
        path.parent.mkdir(parents=True, exist_ok=True)
        if path.exists():
            print(f"[{path.name}] 이미 있음 — 건너뜀")
            continue
        print(f"[{path.name}] 생성 중... (model={MODEL})")
        res = client.images.generate(model=MODEL, prompt=prompt, size="1024x1024", n=1)
        path.write_bytes(base64.b64decode(res.data[0].b64_json))
        print(f"  완료: {path}")

    print("\nStage 4 아트 생성 완료.")


if __name__ == "__main__":
    main()
