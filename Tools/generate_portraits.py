"""
IQ 진화 단계별 HUD 초상 이미지를 생성한다. (4단계 × 2프레임 = 8장)

- frame A : 새로 생성 (tier별 캐릭터)
- frame B : frame A를 'edit'해서 만든 변형 (눈 깜빡임/살짝 미소 등 '움직이는 부분만' 다름)
  → 같은 단계 안에서 일관성을 최대한 유지(나머지는 동일)해 2장 번갈아 보면 움직이는 느낌.

사전 준비:
  1) pip install openai
  2) 프로젝트 루트의 .env.example 을 .env 로 복사 후 OPENAI_API_KEY 채우기
  3) python Tools/generate_portraits.py

결과: Assets/Art/Portraits/Tier{n}_{name}_a.png, _b.png
그 다음 Unity 메뉴 Tools > Rock Falldown > Import & Assign Portraits 로 스프라이트 연결.
"""

import os
import sys
import base64
import pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent
OUT_DIR = ROOT / "Assets" / "Art" / "Portraits"


def load_env():
    env_path = ROOT / ".env"
    if env_path.exists():
        for line in env_path.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            k, v = line.split("=", 1)
            os.environ.setdefault(k.strip(), v.strip())


# 단계별 캐릭터 설명. 스타일 문구는 모든 단계에 공통으로 붙여 톤을 통일.
STYLE = (
    "flat 2D cartoon mascot portrait, head and shoulders, facing forward, "
    "centered, bold clean outlines, simple solid colors, friendly, "
    "plain solid white background, consistent art style, game UI icon"
)

TIERS = [
    ("Tier0_Caveman",   "a caveman with messy hair and a bone, dim confused expression"),
    ("Tier1_Student",   "a young student wearing round glasses, focused expression, holding a pencil"),
    ("Tier2_Professor", "a professor with a graduation cap and gown, confident smile"),
    ("Tier3_Genius",    "a genius with a glowing brain aura, sparkling eyes, big bright smile"),
]

# frame B: A와 동일하되 '움직이는 부분'만 변경 (일관성 핵심)
MOVE_EDIT = (
    "Keep the exact same character, identical pose, colors, framing and style. "
    "Only change: close the eyes (blinking) and a slightly bigger smile. "
    "Everything else must stay identical."
)


def main():
    load_env()
    api_key = os.environ.get("OPENAI_API_KEY")
    if not api_key:
        print("ERROR: .env 에 OPENAI_API_KEY 가 없습니다.")
        sys.exit(1)

    try:
        from openai import OpenAI
    except ImportError:
        print("ERROR: openai 패키지가 없습니다.  pip install openai")
        sys.exit(1)

    model = os.environ.get("IMAGE_MODEL", "gpt-image-1")
    client = OpenAI(api_key=api_key)
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    for asset_name, desc in TIERS:
        prompt_a = f"{desc}. {STYLE}."
        print(f"[{asset_name}] frame A 생성 중...")
        res_a = client.images.generate(
            model=model, prompt=prompt_a, size="1024x1024", n=1
        )
        path_a = OUT_DIR / f"{asset_name}_a.png"
        path_a.write_bytes(base64.b64decode(res_a.data[0].b64_json))

        print(f"[{asset_name}] frame B (edit) 생성 중...")
        with open(path_a, "rb") as img:
            res_b = client.images.edit(
                model=model, image=img, prompt=MOVE_EDIT, size="1024x1024"
            )
        path_b = OUT_DIR / f"{asset_name}_b.png"
        path_b.write_bytes(base64.b64decode(res_b.data[0].b64_json))

        print(f"  완료: {path_a.name}, {path_b.name}")

    print("\n모든 초상 생성 완료 →", OUT_DIR)
    print("Unity로 돌아가 새로고침 후, 텍스처를 Sprite로 임포트하고 PortraitDisplay에 연결하세요.")


if __name__ == "__main__":
    main()
