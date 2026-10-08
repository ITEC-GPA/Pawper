import json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
catalog=json.loads((root/'examples/catalog.json').read_text(encoding='utf-8-sig'))
text=['# Venti esempi Pawper','', 'Gli esempi procedono dalle funzioni di base alla relazione completa. Ogni cartella contiene la definizione binaria Grasshopper, la guida con input e collegamenti, i file Word e la specifica dei dati.','', '| N. | Esempio | Obiettivo |','|---|---|---|']
for e in catalog:
    folder=f"{e['Id']:02}-{e['Slug']}"
    text.append(f"| {e['Id']:02} | [{e['Title']}]({folder}/README.md) | {e['Purpose']} |")
text+=['','[Matrice dei componenti e delle funzionalità](COVERAGE.md).','', 'Gli input Run sono salvati su False. La riapertura disarma i trigger: inviare False→True. Gli esempi sono portabili se si conserva la cartella assets. Capture Rhino View richiede una sessione con viste disponibili; il componente è incluso disabilitato nel caso 07.','', 'Verifiche automatiche: verification.json (motore e casi di errore), grasshopper-verification.json (salvataggio, riapertura e contenuti dei file GH). Vedere anche il rapporto generale in ../docs/VERIFICHE.md.']
(root/'examples/README.md').write_text('\n'.join(text)+'\n',encoding='utf-8')
