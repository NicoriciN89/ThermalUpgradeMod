"""
Обрабатывает рендеры из Tripo3D:
- Убирает белый фон
- Обрезает до содержимого
- Масштабирует до 512x512
- Добавляет кожаные заплатки на колени и бедра
- Сохраняет финальные иконки

Запуск: py -3 process_renders.py
"""
from PIL import Image, ImageDraw
import math, os

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
OUT_DIR   = os.path.join(TOOLS_DIR, "output_icons")
os.makedirs(OUT_DIR, exist_ok=True)

SRC_THERMAL = os.path.join(TOOLS_DIR, "source_thermal.png")
SRC_WOOL    = os.path.join(TOOLS_DIR, "source_wool.png")

SIZE = 512

PATCH = {
    "fill":   (139,  90,  43, 210),
    "edge":   ( 90,  55,  20, 255),
    "stitch": (210, 170, 110, 190),
    "shadow": (  0,   0,   0,  60),
}

# ------------------------------------------------------------------
def remove_white_bg(img: Image.Image, threshold=240) -> Image.Image:
    """Убирает белый/светлый фон, делает прозрачным."""
    img = img.convert("RGBA")
    data = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = data[x, y]
            if r >= threshold and g >= threshold and b >= threshold:
                data[x, y] = (r, g, b, 0)
    return img

def autocrop(img: Image.Image, padding=8) -> Image.Image:
    """Обрезает по bbox непрозрачных пикселей."""
    bb = img.getbbox()
    if bb is None:
        return img
    l, t, r, b = bb
    l = max(0, l - padding)
    t = max(0, t - padding)
    r = min(img.width,  r + padding)
    b = min(img.height, b + padding)
    return img.crop((l, t, r, b))

def fit_to_square(img: Image.Image, size=512) -> Image.Image:
    """Помещает изображение в квадрат size x size с прозрачными полями."""
    img.thumbnail((size, size), Image.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    x = (size - img.width)  // 2
    y = (size - img.height) // 2
    canvas.paste(img, (x, y), img)
    return canvas

# ------------------------------------------------------------------
def _rr(cx, cy, w, h, angle_deg):
    """Повернутый прямоугольник — список из 4 вершин."""
    a = math.radians(angle_deg)
    pts = []
    for dx, dy in [(-w/2,-h/2),(w/2,-h/2),(w/2,h/2),(-w/2,h/2)]:
        pts.append((dx*math.cos(a) - dy*math.sin(a) + cx,
                    dx*math.sin(a) + dy*math.cos(a) + cy))
    return pts

def _dashed(d, pts, color, dash=5, gap=4):
    for i in range(len(pts)):
        x0,y0 = pts[i]; x1,y1 = pts[(i+1) % len(pts)]
        ln = math.hypot(x1-x0, y1-y0)
        if ln == 0: continue
        ux,uy = (x1-x0)/ln, (y1-y0)/ln
        pos, on = 0, True
        while pos < ln:
            seg = min((dash if on else gap), ln-pos)
            if on:
                d.line([(x0+ux*pos, y0+uy*pos),(x0+ux*(pos+seg), y0+uy*(pos+seg))],
                       fill=color, width=1)
            pos += seg; on = not on

def _patch(d, cx, cy, w, h, angle):
    d.polygon(_rr(cx+3, cy+3, w+4, h+4, angle), fill=PATCH["shadow"])
    d.polygon(_rr(cx,   cy,   w,   h,   angle), fill=PATCH["fill"])
    d.polygon(_rr(cx,   cy,   w-4, h-4, angle), outline=PATCH["edge"], width=2)
    _dashed(d, _rr(cx, cy, w-10, h-10, angle), PATCH["stitch"])

def add_patches(img: Image.Image) -> Image.Image:
    """Добавляет 4 кожаных заплатки под силуэт 512x512."""
    overlay = Image.new("RGBA", (SIZE, SIZE), (0,0,0,0))
    d = ImageDraw.Draw(overlay)
    cx = SIZE // 2
    # Позиции подобраны под фронтальный рендер штанов в 512x512
    _patch(d, cx-68, 330, 72, 46,  4)   # левое колено
    _patch(d, cx+68, 330, 72, 46, -4)   # правое колено
    _patch(d, cx-72, 195, 58, 38,  3)   # левое бедро
    _patch(d, cx+72, 195, 58, 38, -3)   # правое бедро
    return Image.alpha_composite(img.convert("RGBA"), overlay)

# ------------------------------------------------------------------
def process(src_path: str, names: list):
    if not os.path.exists(src_path):
        print(f"  ERROR: not found: {src_path}")
        return

    img = Image.open(src_path)
    img = remove_white_bg(img)
    img = autocrop(img)
    img = fit_to_square(img, SIZE)

    for name in names:
        img.save(os.path.join(OUT_DIR, name))
        print(f"  saved -> {name}")

process(SRC_THERMAL, [
    "ico_GearItem__ThermalUnderwearUpgraded.png",
    "ico_CraftItem__ThermalUnderwearUpgraded.png",
])

process(SRC_WOOL, [
    "ico_GearItem__WoolLongjohnsUpgraded.png",
    "ico_CraftItem__WoolLongjohnsUpgraded.png",
])

print(f"\nDone! {OUT_DIR}")
