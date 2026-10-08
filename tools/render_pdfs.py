from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from pdf2image import convert_from_path
import sys, json
root=Path(sys.argv[1]); out=root/'pages'; out.mkdir(exist_ok=True)
poppler=Path(sys.argv[2]); files=[]
for pdf in sorted(root.glob('*.pdf')):
    if not pdf.stat().st_size: continue
    try:
        pages=convert_from_path(str(pdf),dpi=72,poppler_path=str(poppler))
        for i,page in enumerate(pages,1):
            p=out/f'{pdf.stem}-{i}.png'; page.save(p); files.append(p)
    except Exception as e: print(f'FAIL {pdf.name}: {e}')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
for batch in range(0,len(files),6):
    sheet=Image.new('RGB',(1800,2700),'#dddddd'); d=ImageDraw.Draw(sheet)
    for index,p in enumerate(files[batch:batch+6]):
        im=Image.open(p); x=(index%2)*900; y=(index//2)*900
        sheet.paste(im,(x+(900-im.width)//2,y+35)); d.text((x+10,y+5),p.stem[:70],font=font,fill='black')
    sheet.save(root/f'contact-{batch//6+1:02}.png')
print(json.dumps({'pages':len(files),'sheets':(len(files)+5)//6}))
