from math import pi, sin
from pathlib import Path
from PIL import Image, ImageEnhance

SOURCE = Path(r"C:\Users\user\AppData\Local\Temp\codex-clipboard-ad30971e-0ba4-41f5-a747-8f2d3175e8d0.png")
APPROACH = Path("Cutscenes/heat_collapse_approach.png")
PILLOW = Path("Cutscenes/heat_collapse_face_pillow.png")
OUTPUT = Path("Cutscenes/heat_collapse_pillow_fall.gif")
SIZE = (640, 360)
FPS = 12
FRAMES = FPS * 9


def ease(value):
    return max(0, min(1, value)) ** 2 * (3 - 2 * max(0, min(1, value)))


def camera(image, zoom, x, y, rotation=0):
    width, height = image.size
    aspect = SIZE[0] / SIZE[1]
    crop_width = min(width, height * aspect) / zoom
    crop_height = crop_width / aspect
    if crop_height > height:
        crop_height = height / zoom
        crop_width = crop_height * aspect
    left = max(0, min(width - crop_width, width * x - crop_width / 2))
    top = max(0, min(height - crop_height, height * y - crop_height / 2))
    return image.crop((left, top, left + crop_width, top + crop_height)).resize(SIZE, Image.Resampling.LANCZOS).rotate(rotation, Image.Resampling.BICUBIC)


def frame(original, approach, pillow, index):
    progress = index / (FRAMES - 1)
    wobble = sin(progress * pi * 9) * (1 - ease((progress - .55) / .35))
    start = camera(original, 1 + .18 * ease(progress / .42), .57, .62, wobble * 1.3)
    middle = camera(approach, 1 + .12 * ease((progress - .35) / .38), .5, .53, wobble * 1.8)
    end = camera(pillow, 1.02, .5, .56, 2 + wobble * .35)
    if progress < .42:
        image = start
    elif progress < .68:
        image = Image.blend(start, middle, ease((progress - .42) / .26))
    else:
        image = Image.blend(middle, end, ease((progress - .68) / .22))
    image = Image.blend(image, Image.new("RGB", SIZE, (132, 45, 20)), .13 * (1 - ease((progress - .62) / .2)))
    if progress > .82:
        image = ImageEnhance.Brightness(image).enhance(1 - .85 * ease((progress - .82) / .18))
    return image


images = [Image.open(path).convert("RGB") for path in (SOURCE, APPROACH, PILLOW)]
frames = [frame(*images, index) for index in range(FRAMES)]
frames[0].save(OUTPUT, save_all=True, append_images=frames[1:], duration=1000 // FPS, loop=0, optimize=True)
print(OUTPUT.resolve())
