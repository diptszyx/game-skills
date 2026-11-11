"""Build one labeled contact sheet per resolution/style, with both language cases.
Usage: python3 tools/montage.py [shots-directory]
"""
from pathlib import Path
import argparse
from PIL import Image, ImageDraw, ImageFont, ImageStat
NAMES = ('main', 'shop', 'checkin', 'quests', 'leaderboard', 'loading', 'win', 'lose', 'revive', 'pause', 'leave')
def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', nargs='?', type=Path, default=Path(__file__).resolve().parents[1] / 'unity-smoke/Logs/shots')
    args = parser.parse_args()
    font = None
    for candidate in ('DejaVuSans.ttf', '/System/Library/Fonts/Supplemental/Arial.ttf'):
        try:
            font = ImageFont.truetype(candidate, 16)
            break
        except OSError:
            pass
    if font is None:
        font = ImageFont.load_default(size=16)
    for resolution in ('720x1600', '1080x1920', '1080x1920_notch'):
        for style in ('dark-flat', 'bright-rounded'):
            cellw, cellh, cols = 240, 578, 6
            sheet = Image.new('RGB', (cellw * cols, cellh * 4), '#333333')
            draw = ImageDraw.Draw(sheet)
            for index, (lang, name) in enumerate((lang, name) for lang in ('en', 'vi') for name in NAMES):
                path = args.directory / f'{resolution}_{style}_{lang}_{name}.png'
                with Image.open(path) as capture:
                    expected = tuple(map(int, resolution.split('_')[0].split('x')))
                    assert capture.size == expected, (path, capture.size)
                    assert any(v > 0 for v in ImageStat.Stat(capture).var), f'Uniform image: {path}'
                    capture.thumbnail((cellw - 8, cellh - 44), Image.Resampling.LANCZOS)
                    x, y = (index % cols) * cellw, (index // cols) * cellh
                    sheet.paste(capture, (x + (cellw - capture.width)//2, y + 36))
                    draw.text((x + 6, y + 9), f'{lang} / {name}', fill='white', font=font)
            draw.text((6, cellh * 4 - 23), f'{resolution} / {style} — en + vi', fill='white', font=font)
            output = args.directory / f'montage_{resolution}_{style}.png'
            sheet.save(output)
            print(output)
if __name__ == '__main__':
    main()
