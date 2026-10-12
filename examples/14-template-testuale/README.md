# 14 Template con placeholder

Sostituire token divisi tra run nel corpo, nelle intestazioni e nei piè di pagina.

## Prerequisiti e file

Rhino 8 con .NET 8 e Pawper installato. Conservare la cartella dentro examples, accanto ad assets.

- `expected.docx`: risultato del motore indipendente.
- `grasshopper-verified.docx`: risultato dei componenti della definizione.
- `document.json`: specifica riproducibile.
- `example.gh`: definizione binaria salvata, riaperta e ricalcolata in Rhino/Grasshopper.

## Procedura

1. Aprire example.gh e ricalcolare.
2. Leggere Document Inspector: Validation deve essere vuoto.
3. Modificare gli input dal menu contestuale; Text Blocks e Text Table offrono anche un editor con doppio clic. Dove presenti, i formati avanzati sono già impostati.
4. In Save Word portare Run su False e poi True. output.docx sarà creato accanto alla definizione. Per sostituire un file esistente abilitare Overwrite.
5. Ricalcolare con Run ancora True: non deve esserci una nuova scrittura.
6. In Open Result portare Run su False e poi True.
7. In Word aggiornare i campi con F9.

Quick Document apre la copia di template.docx e aggiunge il paragrafo ProjectBody; Title vuoto evita titoli duplicati. Fill Fields.Names e Values sono due liste parallele: Progetto → Edificio dimostrativo, Cliente → Cliente esempio. Names accetta anche {{Progetto}}. Lunghezze diverse e nomi duplicati sono errori. Strict=True segnala token mancanti/nome inutilizzato alla validazione. Per provare Strict=False rimuovere una coppia: il relativo token resta letterale. Rimettere la coppia e Strict=True per ottenere il risultato atteso. I placeholder spezzati fra run, ripetuti e nelle intestazioni sono sostituiti insieme.

Alternativa avanzata da ricostruire: aggiungere Fill Template; collegare Quick Document.Document a Fill Template.Document; impostare Values JSON a {"Progetto":"Edificio dimostrativo","Cliente":"Cliente esempio"}, Targets vuoto e Strict=True. Collegare Fill Template.Document a Inspector e Save Word al posto di Fill Fields.Document. Il risultato atteso è identico. Questa variante manuale mostra il precedente input JSON; la definizione inclusa usa le liste semplificate.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Dove presente, Blocks di Compose o Quick Document usa esplicitamente Flatten per comporre un solo documento da più sorgenti; rimuoverlo per elaborare rami separati.

### 01 QuickDoc — Quick Document

Crea un documento con titolo, blocchi ordinati e impostazioni opzionali. Il salvataggio resta esplicito in Save Word.

- **Title** (item): ``
- **Blocks** ← 03 Paragraph / Block
- **Layout** (item): `opzionale / vuoto`
- **Template path** ← 02 Path / Full path
- **Styles** (list): `opzionale / vuoto`

### 02 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `../assets/template.docx`

### 03 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Aggiunta con stile del template`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `ProjectBody`
- **Format** (item): ``

### 04 FillFields — Fill Fields

Compila placeholder con due liste parallele di nomi e valori, senza JSON. Mantiene la formattazione del primo run del token.

- **Document** ← 01 QuickDoc / Document
- **Names** (list): `{{Progetto}} | {{Cliente}}`
- **Values** (list): `Edificio dimostrativo | Cliente esempio`
- **Strict** (item): `True`

### 05 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 04 FillFields / Document

### 06 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 04 FillFields / Document
- **Path** ← 07 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 07 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 08 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 09 Path / Full path
- **Run** (item): `False`

### 09 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

## Risultato e verifiche

Confrontare il risultato con expected.docx e grasshopper-verified.docx. I documenti devono aprirsi senza riparazione. Testi, tabelle e formule sono modificabili; le immagini sono incorporate. I grafici sono immagini.

## Errori comuni

- Risorsa mancante: ripristinare examples/assets o correggere Project Path.
- Stile mancante: usare l'ID di Inspector, non il nome tradotto.
- File già esistente/bloccato: cambiare percorso o chiudere Word e abilitare Overwrite.
- Nessun salvataggio: riportare Run a False prima di True.
- Tabella errata: Quick Table usa righe o colonne secondo Columns; non appiattire Data. Word Table e Table Rows restano disponibili per celle complesse.
- Formula rifiutata: consultare il sottoinsieme LaTeX nel manuale.

