from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import sys
folder=Path(sys.argv[1]); folder.mkdir(parents=True,exist_ok=True)
im=Image.new('RGB',(1200,360),'white'); d=ImageDraw.Draw(im)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',24)
d.line((100,200,1100,200),fill='#24445C',width=10)
for x in [100,1100]:
    d.polygon([(x,205),(x-32,252),(x+32,252)],outline='#24445C',width=4)
    d.line((x-45,265,x+45,265),fill='#24445C',width=3)
for x in range(120,1100,70):
    d.line((x,85,x,180),fill='#507E9E',width=3)
    d.polygon([(x,183),(x-8,166),(x+8,166)],fill='#507E9E')
d.line((120,85,1090,85),fill='#507E9E',width=3)
d.text((490,25),'q = 12.35 kN/m',font=font,fill='#24445C')
d.line((100,307,1100,307),fill='#667788',width=2)
d.text((530,318),'L = 6.50 m',font=font,fill='#24445C')
im.save(folder/'diagram.png'); im.save(folder/'diagram.jpg',quality=95)
