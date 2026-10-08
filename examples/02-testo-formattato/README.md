# 02 Testo formattato

Combinare frammenti con proprietà tipografiche diverse.

## Prerequisiti e file

Rhino 8 con .NET 8 e Pawper installato. Conservare la cartella dentro examples, accanto ad assets.

- `expected.docx`: risultato del motore indipendente.
- `grasshopper-verified.docx`: risultato dei componenti della definizione.
- `document.json`: specifica riproducibile.
- `example.gh`: definizione binaria salvata, riaperta e ricalcolata in Rhino/Grasshopper.

## Procedura

1. Aprire example.gh e ricalcolare.
2. Leggere Document Inspector: Validation deve essere vuoto.
3. Modificare i valori degli input con il menu contestuale. I valori avanzati JSON sono già impostati.
4. In Save Word portare Run su False e poi True. output.docx sarà creato accanto alla definizione. Per sostituire un file esistente abilitare Overwrite.
5. Ricalcolare con Run ancora True: non deve esserci una nuova scrittura.
6. In Open Result portare Run su False e poi True.
7. In Word aggiornare i campi con F9.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Blocks di Compose usa esplicitamente Flatten per comporre un solo documento da tutte le sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 04 Paragraph / Block, 15 Paragraph / Block, 16 Paragraph / Block, 17 Paragraph / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Testo e formattazione`
- **Level** (item): `0`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 05 Run / Run, 06 Run / Run, 07 Run / Run, 08 Run / Run, 09 Run / Run, 10 Run / Run, 11 Run / Run, 12 Run / Run, 13 Run / Run, 14 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 05 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Normale `
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 06 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `grassetto `
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `True`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 07 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `corsivo `
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `True`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 08 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `sottolineato `
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `True`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 09 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `colore `
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `24445C`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 10 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `x`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 11 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `2`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `superscript`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 12 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): ` H`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 13 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `2`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `subscript`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 14 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `O`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 15 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Paragrafo con rientro e spaziatura. Seconda riga con un'interruzione esplicita.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): `{   "Alignment": "both",   "Before": 6,   "After": 12,   "Line": 1.15,   "LeftMm": 5,   "RightMm": 0,   "FirstLineMm": 5,   "KeepNext": false }`

### 16 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Testo piccolo e grande`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 17 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 18 Run / Run, 19 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 18 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Aptos 10 `
- **Font** (item): `Aptos`
- **Size** (item): `10`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 19 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Arial 16`
- **Font** (item): `Arial`
- **Size** (item): `16`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 20 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 21 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 22 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 22 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 23 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 24 Path / Full path
- **Run** (item): `False`

### 24 Path — Project Path

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

