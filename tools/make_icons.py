"""
Генерирует иконки улучшенного термобелья и шерстяных кальсонов
полностью программно — в стиле ванильных иконок TLD.
Не требует внешних файлов.

Запуск:  py -3 make_icons.py
"""
from PIL import Image, ImageDraw, ImageFilter
import math, os

OUT_DIR = r"e:\games\TheLongDark\ThermalUpgradeMod_Dev\tools\output_icons"
os.makedirs(OUT_DIR, exist_ok=True)

SIZE = 512

# ──────────────────────────────────────────────
# Палитры (взяты с ванильных скриншотов)
# ──────────────────────────────────────────────
THERMAL = {
    "body_fill":  (32,  88,  88),
    "body_dark":  (20,  60,  60),
    "seam":       (15,  40,  40),
    "band_fill":  (138, 105, 45),
    "band_dark":  ( 90,  65, 20),
    "cuff_fill":  ( 20,  25,  25),
    "buckle":     True,
}
WOOL = {
    "body_fill":  (110,  22,  18),
    "body_dark":  ( 75,  12,  10),
    "seam":       ( 55,   8,   6),
    "band_fill":  ( 20,  20,  20),
    "band_dark":  ( 10,  10,  10),
    "cuff_fill":  ( 30,  30,  35),
    "buckle":     False,
}
PATCH = {
    "fill":   (139,  90,  43, 200),
    "edge":   ( 90,  55,  20, 255),
    "stitch": (210, 170, 110, 180),
    "shadow": (  0,   0,   0,  55),
}

def pants_polys(s=512):
    cx = s // 2
    # Пояс
    waist = [(cx-100, 55), (cx+100, 55), (cx+105, 88), (cx-105, 88)]

    # Вся передняя часть штанов — единый U-образный силуэт
    # Обход: слева сверху → вниз левой ноги → через пах → вверх правой ноги → справа сверху
    body = [
        (cx-105, 88),   # верх-левый
        (cx+105, 88),   # верх-правый
        (cx+102, 110),  # правый бок сверху (слабый скос)
        (cx+90,  160),  # правое бедро внешнее
        (cx+80,  265),  # правое колено внешнее
        (cx+90,  385),  # правый голень внешний низ
        (cx+95,  415),  # правая щиколотка внешняя
        (cx+70,  420),  # правая ступня
        (cx+45,  400),  # правая ступня внутри
        (cx+25,  300),  # правый внутренний колено
        (cx+12,  160),  # правый пах внутри
        (cx,     140),  # пах центр
        (cx-12,  160),  # левый пах внутри
        (cx-25,  300),  # левый внутренний колено
        (cx-45,  400),  # левая ступня внутри
        (cx-70,  420),  # левая ступня
        (cx-95,  415),  # левая щиколотка внешняя
        (cx-90,  385),  # левый голень внешний низ
        (cx-80,  265),  # левое колено внешнее
        (cx-90,  160),  # левое бедро внешнее
        (cx-102, 110),  # левый бок сверху
    ]

    # Манжеты
    left_cuff  = [(cx-95, 415), (cx-70, 420), (cx-68, 455), (cx-98, 450)]
    right_cuff = [(cx+70, 420), (cx+95, 415), (cx+98, 450), (cx+68, 455)]
    return waist, body, None, left_cuff, right_cuff

def draw_pants(pal: dict, with_patches: bool) -> Image.Image:
    img = Image.new("RGBA", (SIZE, SIZE), (255, 255, 255, 0))
    d   = ImageDraw.Draw(img)
    cx  = SIZE // 2
    waist, body, _unused, lc, rc = pants_polys(SIZE)

    d.polygon(body, fill=pal["body_fill"])

    # Тени по краям ног (левый и правый контур темнее)
    shade = Image.new("RGBA", (SIZE, SIZE), (0,0,0,0))
    sd = ImageDraw.Draw(shade)
    sd.polygon([(cx-105,88),(cx-75,90),(cx-62,170),(cx-76,270),(cx-92,380),(cx-95,410),(cx-105,88)], fill=(0,0,0,50))
    sd.polygon([(cx+105,88),(cx+75,90),(cx+62,170),(cx+76,270),(cx+92,380),(cx+95,410),(cx+105,88)], fill=(0,0,0,50))
    img = Image.alpha_composite(img, shade)
    d   = ImageDraw.Draw(img)

    # Центральная линия (разделение ног)
    d.line([(cx, 88), (cx, 145), (cx-10, 295), (cx-20, 395)], fill=pal["seam"], width=2)
    d.line([(cx, 88), (cx, 145), (cx+10, 295), (cx+20, 395)], fill=pal["seam"], width=1)

    # Пояс
    d.polygon(waist, fill=pal["band_fill"])
    d.line([tuple(waist[0]), tuple(waist[1])], fill=pal["band_dark"], width=3)
    d.line([tuple(waist[2]), tuple(waist[3])], fill=pal["band_dark"], width=2)

    if pal["buckle"]:
        bx, by = cx, 80
        d.rectangle([bx-12, by-8, bx+12, by+8], fill=pal["band_dark"], outline=(180,140,60), width=2)
        d.rectangle([bx-4,  by-3, bx+4,  by+3], fill=(180,140,60))
    else:
        for bx in [cx-18, cx+18]:
            d.ellipse([bx-7, 73, bx+7, 87], fill=(40,40,40), outline=(80,80,80), width=1)
            d.ellipse([bx-3, 77, bx+3, 83], fill=(60,60,60))

    # Манжеты
    d.polygon(lc, fill=pal["cuff_fill"])
    d.polygon(rc, fill=pal["cuff_fill"])

    # Заплатки — позиции под новую форму
    if with_patches:
        overlay = Image.new("RGBA", (SIZE, SIZE), (0,0,0,0))
        pd = ImageDraw.Draw(overlay)
        _patch(pd, cx-52, 305, 62, 44,  5)   # левое колено
        _patch(pd, cx+52, 305, 62, 44, -5)   # правое колено
        _patch(pd, cx-55, 185, 54, 36,  3)   # левое бедро
        _patch(pd, cx+55, 185, 54, 36, -3)   # правое бедро
        img = Image.alpha_composite(img, overlay)

    return img.filter(ImageFilter.SMOOTH_MORE)

def _patch(d, cx, cy, w, h, angle_deg):
    def rr(cx, cy, w, h, a):
        a = math.radians(a)
        return [(x*math.cos(a)-y*math.sin(a)+cx, x*math.sin(a)+y*math.cos(a)+cy)
                for x,y in [(-w/2,-h/2),(w/2,-h/2),(w/2,h/2),(-w/2,h/2)]]
    d.polygon(rr(cx+3, cy+3, w+4, h+4, angle_deg), fill=PATCH["shadow"])
    d.polygon(rr(cx,   cy,   w,   h,   angle_deg), fill=PATCH["fill"])
    d.polygon(rr(cx,   cy,   w-4, h-4, angle_deg), outline=PATCH["edge"], width=2)
    _dashed(d, rr(cx, cy, w-10, h-10, angle_deg), PATCH["stitch"])

def _dashed(d, pts, color, dash=5, gap=4):
    for i in range(len(pts)):
        x0,y0 = pts[i]; x1,y1 = pts[(i+1)%len(pts)]
        ln = math.hypot(x1-x0, y1-y0)
        if ln == 0: continue
        ux,uy = (x1-x0)/ln, (y1-y0)/ln
        pos, on = 0, True
        while pos < ln:
            seg = min(dash if on else gap, ln-pos)
            if on:
                d.line([(x0+ux*pos, y0+uy*pos),(x0+ux*(pos+seg), y0+uy*(pos+seg))], fill=color, width=1)
            pos += seg; on = not on

items = [
    ("ico_GearItem__ThermalUnderwearUpgraded.png",  THERMAL, True),
    ("ico_CraftItem__ThermalUnderwearUpgraded.png", THERMAL, True),
    ("ico_GearItem__WoolLongjohnsUpgraded.png",     WOOL,    True),
    ("ico_CraftItem__WoolLongjohnsUpgraded.png",    WOOL,    True),
    ("_preview_vanilla_thermal.png",                THERMAL, False),
    ("_preview_vanilla_wool.png",                   WOOL,    False),
]

for fname, pal, patches in items:
    img = draw_pants(pal, patches)
    img.save(os.path.join(OUT_DIR, fname))
    print(f"  {'+ patches' if patches else 'vanilla  '} -> {fname}")

print(f"\nГотово! {OUT_DIR}")


os.makedirs(OUT_DIR, exist_ok=True)

# Цвета
LEATHER_FILL    = (139, 90,  43, 220)   # коричневая кожа
LEATHER_EDGE    = ( 90, 55,  20, 255)   # тёмный край заплатки
STITCH_COLOR    = (210, 170, 110, 200)  # цвет стежков
SHADOW          = (  0,   0,   0,  60)  # тень под заплаткой

def add_patches(img: Image.Image) -> Image.Image:
    """Добавляет кожаные заплатки на колени и бёдра."""
    img = img.convert("RGBA").resize((512, 512), Image.LANCZOS)
    overlay = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)

    # --- Заплатки: координаты подобраны под силуэт брюк 512x512 ---
    # Левое колено  (левая нога на экране = правая нога персонажа)
    patches = [
        # (x_center, y_center, w, h, угол наклона)
        (175, 330, 90, 60,  4),   # левое колено
        (325, 330, 90, 60, -4),   # правое колено
        (195, 195, 70, 45,  3),   # левое бедро
        (315, 195, 70, 45, -3),   # правое бедро
    ]

    for cx, cy, w, h, angle in patches:
        _draw_patch(draw, cx, cy, w, h, angle)

    # Смешиваем с оригиналом
    result = Image.alpha_composite(img, overlay)
    return result

def _draw_patch(draw: ImageDraw.ImageDraw, cx, cy, w, h, angle_deg):
    """Рисует одну кожаную заплатку с тенью и строчкой."""
    import math

    def rotated_rect(cx, cy, w, h, angle_deg):
        angle = math.radians(angle_deg)
        corners = [(-w/2, -h/2), (w/2, -h/2), (w/2, h/2), (-w/2, h/2)]
        rotated = []
        for x, y in corners:
            rx = x * math.cos(angle) - y * math.sin(angle) + cx
            ry = x * math.sin(angle) + y * math.cos(angle) + cy
            rotated.append((rx, ry))
        return rotated

    # Тень (смещение +3,+3)
    shadow_pts = rotated_rect(cx+3, cy+3, w+4, h+4, angle_deg)
    draw.polygon(shadow_pts, fill=SHADOW)

    # Основная заплатка
    pts = rotated_rect(cx, cy, w, h, angle_deg)
    draw.polygon(pts, fill=LEATHER_FILL)

    # Край заплатки (чуть меньше)
    edge_pts = rotated_rect(cx, cy, w-4, h-4, angle_deg)
    draw.polygon(edge_pts, outline=LEATHER_EDGE, width=2)

    # Строчка — пунктир по периметру внутреннего прямоугольника
    stitch_pts = rotated_rect(cx, cy, w-10, h-10, angle_deg)
    _draw_dashed_polygon(draw, stitch_pts, STITCH_COLOR, dash=6, gap=4, width=1)

def _draw_dashed_polygon(draw, pts, color, dash=6, gap=4, width=1):
    """Рисует пунктирный контур полигона."""
    import math
    n = len(pts)
    for i in range(n):
        x0, y0 = pts[i]
        x1, y1 = pts[(i + 1) % n]
        dx, dy = x1 - x0, y1 - y0
        length = math.hypot(dx, dy)
        if length == 0:
            continue
        ux, uy = dx / length, dy / length
        pos = 0
        drawing = True
        while pos < length:
            seg_len = dash if drawing else gap
            seg_len = min(seg_len, length - pos)
            if drawing:
                ax, ay = x0 + ux * pos, y0 + uy * pos
                bx, by = x0 + ux * (pos + seg_len), y0 + uy * (pos + seg_len)
                draw.line([(ax, ay), (bx, by)], fill=color, width=width)
            pos += seg_len
            drawing = not drawing

def process(src_path, out_name, label):
    if not os.path.exists(src_path):
        print(f"  ОШИБКА: файл не найден: {src_path}")
        return

    img = Image.open(src_path)
    result = add_patches(img)

    out_path = os.path.join(OUT_DIR, out_name)
    result.save(out_path)
    print(f"  [{label}] -> {out_path}  {result.size}")

print("Создаю иконки улучшенного термобелья...\n")

process(SRC_THERMAL,
        "ico_GearItem__ThermalUnderwearUpgraded.png",
        "Thermal GearItem icon")

process(SRC_THERMAL,
        "ico_CraftItem__ThermalUnderwearUpgraded.png",
        "Thermal CraftItem icon")

process(SRC_WOOL,
        "ico_GearItem__WoolLongjohnsUpgraded.png",
        "Wool GearItem icon")

process(SRC_WOOL,
        "ico_CraftItem__WoolLongjohnsUpgraded.png",
        "Wool CraftItem icon")

print("\nГотово! Проверь папку:", OUT_DIR)
