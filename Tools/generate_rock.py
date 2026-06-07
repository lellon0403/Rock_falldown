"""
굴러오는 돌(바위)용 텍스처를 생성한다. 구(Sphere) 돌에 입히면 회전이 보여 '굴러오는' 느낌.
사용법: python Tools/generate_rock.py  ->  Assets/Art/Rocks/rock.png
그 다음 Unity 메뉴 Tools > Rock Falldown > Apply Rock Texture
"""
import os, sys, base64, pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / "Assets" / "Art" / "Rocks"


def load_env():
    p = ROOT / ".env"
    if p.exists():
        for line in p.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, v = line.split("=", 1)
                os.environ.setdefault(k.strip(), v.strip())


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

    textures = [
        ("rock",
         "seamless tileable rock boulder surface texture, grey rough stone with cracks "
         "and bumps, natural granite, flat even lighting, no shadows, uniform repeating "
         "pattern, game texture, edges match for tiling"),
        ("snow",
         "seamless tileable snow surface texture, white packed fluffy snowball surface, "
         "soft subtle bumps, slight sparkle, flat even lighting, no shadows, uniform "
         "repeating pattern, game texture, edges match for tiling"),
    ]
    for name, prompt in textures:
        path = OUT_DIR / f"{name}.png"
        if path.exists():
            print(f"[{name}] 이미 있음 — 건너뜀")
            continue
        print(f"[{name}] 생성 중...")
        res = client.images.generate(model=model, prompt=prompt, size="1024x1024", n=1)
        path.write_bytes(base64.b64decode(res.data[0].b64_json))
        print("완료:", path)


if __name__ == "__main__":
    main()
