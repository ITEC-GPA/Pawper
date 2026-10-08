# 10 Pagine e sezioni

Gestire pagina iniziale, pari/dispari, numeri pagina e orientamenti diversi.

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
- **Blocks** ← 03 Heading / Block, 04 Paragraph / Block, 05 Break / Block, 06 Heading / Block, 07 Paragraph / Block, 08 Section / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** ← 25 Layout / Layout

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Relazione con sezioni`
- **Level** (item): `0`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Prima pagina con intestazione dedicata.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 05 Break — Page Break

Interruzione di pagina.


### 06 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Seconda pagina`
- **Level** (item): `1`

### 07 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Verifica intestazioni pari e numerazione.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 08 Section — Section

Inizia una nuova sezione con layout dedicato.

- **Layout** ← 09 Layout / Layout
- **Blocks** ← 10 Heading / Block, 24 Table / Block

### 09 Layout — Page Layout

Dimensioni e margini in millimetri; intestazioni e piè di pagina.

- **Width mm** (item): `215.9`
- **Height mm** (item): `279.4`
- **Margin mm** (item): `20`
- **Landscape** (item): `False`
- **Header** (item): ``
- **Footer** (item): ``
- **Advanced** (item): `{   "WidthMm": 210,   "HeightMm": 297,   "MarginMm": 20,   "Landscape": true,   "Header": "Sezione orizzontale",   "Footer": "Pagina {{PAGE}}",   "FirstHeader": null,   "FirstFooter": null,   "EvenHeader": null,   "EvenFooter": null,   "PageStart": 1 }`

### 10 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Tabella orizzontale`
- **Level** (item): `1`

### 11 Rows — Table Rows

Organizza una lista ordinata di celle in righe con lunghezze esplicite.

- **Cells** ← 12 Cell / Cell, 13 Cell / Cell, 14 Cell / Cell, 15 Cell / Cell, 16 Cell / Cell, 17 Cell / Cell, 18 Cell / Cell, 19 Cell / Cell, 20 Cell / Cell, 21 Cell / Cell, 22 Cell / Cell, 23 Cell / Cell
- **Counts** (list): `3, 3, 3, 3`

### 12 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Elemento`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 13 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Valore`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 14 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Unità`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 15 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Luce`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 16 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `6,50`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 17 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 18 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Carico`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 19 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `12,35`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 20 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `kN/m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 21 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Dato mancante`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 22 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 23 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 24 Table — Word Table

Ogni ramo del Data Tree è una riga; ogni elemento è una cella. Ordine dei path Grasshopper preservato.

- **Rows** ← 11 Rows / Rows
- **Widths mm** (list): `70, 45, 40`
- **Header** (item): `True`
- **Options** (item): `{   "HeaderFill": "24445C",   "StripeFill": "EEF3F6",   "BorderColor": "CBD5DD",   "PaddingMm": 2,   "StyleId": null }`

### 25 Layout — Page Layout

Dimensioni e margini in millimetri; intestazioni e piè di pagina.

- **Width mm** (item): `215.9`
- **Height mm** (item): `279.4`
- **Margin mm** (item): `20`
- **Landscape** (item): `False`
- **Header** (item): ``
- **Footer** (item): ``
- **Advanced** (item): `{   "WidthMm": 210,   "HeightMm": 297,   "MarginMm": 20,   "Landscape": false,   "Header": "Intestazione dispari",   "Footer": "Pagina {{PAGE}} di {{NUMPAGES}}",   "FirstHeader": "Copertina",   "FirstFooter": "",   "EvenHeader": "Intestazione pari",   "EvenFooter": "Pagina {{PAGE}}",   "PageStart": null }`

### 26 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 27 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 28 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 28 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 29 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 30 Path / Full path
- **Run** (item): `False`

### 30 Path — Project Path

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

