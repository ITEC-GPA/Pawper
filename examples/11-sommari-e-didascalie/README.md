# 11 Sommari e didascalie

Creare campi Word per sommario ed elenchi figure/tabelle, da aggiornare in Word.

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
- **Blocks** ← 03 Heading / Block, 04 Field / Block, 05 Heading / Block, 06 Field / Block, 07 Heading / Block, 08 Field / Block, 09 Break / Block, 10 Heading / Block, 11 Image / Block, 12 Paragraph / Block, 29 Table / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Indici del documento`
- **Level** (item): `0`

### 04 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `TOC \o "1-3" \h \z \u`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 05 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Figure`
- **Level** (item): `1`

### 06 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `TOC \h \z \c "Figure"`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 07 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Tabelle`
- **Level** (item): `1`

### 08 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `TOC \h \z \c "Table"`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 09 Break — Page Break

Interruzione di pagina.


### 10 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Risultati`
- **Level** (item): `1`

### 11 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `90`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Schema geometrico dimostrativo`
- **Format** (item): `png`
- **Alignment** (item): `center`

### 12 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 13 Run / Run, 14 Run / Run, 15 Run / Run
- **Style ID** (item): `Caption`
- **Format** (item): ``

### 13 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Tabella `
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

### 14 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `1`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): ``
- **Field** (item): `SEQ Table \* ARABIC`
- **Style ID** (item): ``

### 15 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): ` — Dati di ingresso`
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

### 16 Rows — Table Rows

Organizza una lista ordinata di celle in righe con lunghezze esplicite.

- **Cells** ← 17 Cell / Cell, 18 Cell / Cell, 19 Cell / Cell, 20 Cell / Cell, 21 Cell / Cell, 22 Cell / Cell, 23 Cell / Cell, 24 Cell / Cell, 25 Cell / Cell, 26 Cell / Cell, 27 Cell / Cell, 28 Cell / Cell
- **Counts** (list): `3, 3, 3, 3`

### 17 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Elemento`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 18 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Valore`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 19 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Unità`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 20 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Luce`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 21 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `6,50`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 22 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 23 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Carico`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 24 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `12,35`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 25 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `kN/m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 26 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Dato mancante`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 27 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 28 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 29 Table — Word Table

Ogni ramo del Data Tree è una riga; ogni elemento è una cella. Ordine dei path Grasshopper preservato.

- **Rows** ← 16 Rows / Rows
- **Widths mm** (list): `70, 45, 40`
- **Header** (item): `True`
- **Options** (item): `{   "HeaderFill": "24445C",   "StripeFill": "EEF3F6",   "BorderColor": "CBD5DD",   "PaddingMm": 2,   "StyleId": null }`

### 30 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 31 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 32 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 32 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 33 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 34 Path / Full path
- **Run** (item): `False`

### 34 Path — Project Path

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

