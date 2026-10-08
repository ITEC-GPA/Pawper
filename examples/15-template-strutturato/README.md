# 15 Content control e segnalibri

Riempire un DOTX con blocchi diversi e sostituire un segnalibro inline.

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

Content Control dimostra la creazione di un template. Il DOTX incluso contiene già Contenuti e il segnalibro Cliente.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Blocks di Compose usa esplicitamente Flatten per comporre un solo documento da tutte le sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** ← 02 Path / Full path

### 02 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `../assets/template.dotx`

### 03 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** (list): `opzionale / vuoto`
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 04 Fill — Fill Template

Sostituisce placeholder anche divisi in run e riempie controlli/segnalibri.

- **Document** ← 03 Compose / Document
- **Values** (item): `{}`
- **Targets** ← 05 Target / Target, 22 Target / Target
- **Strict** (item): `True`

### 05 Target — Template Target

Definisce un riempimento di content control o segnalibro inline.

- **Name** (item): `Contenuti`
- **Blocks** ← 06 Heading / Block, 20 Table / Block, 21 Image / Block
- **Bookmark** (item): `False`

### 06 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Dati del progetto`
- **Level** (item): `1`

### 07 Rows — Table Rows

Organizza una lista ordinata di celle in righe con lunghezze esplicite.

- **Cells** ← 08 Cell / Cell, 09 Cell / Cell, 10 Cell / Cell, 11 Cell / Cell, 12 Cell / Cell, 13 Cell / Cell, 14 Cell / Cell, 15 Cell / Cell, 16 Cell / Cell, 17 Cell / Cell, 18 Cell / Cell, 19 Cell / Cell
- **Counts** (list): `3, 3, 3, 3`

### 08 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Elemento`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 09 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Valore`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 10 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Unità`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 11 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Luce`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 12 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `6,50`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 13 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 14 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Carico`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 15 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `12,35`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `right`

### 16 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `kN/m`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 17 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Dato mancante`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 18 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 19 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 20 Table — Word Table

Ogni ramo del Data Tree è una riga; ogni elemento è una cella. Ordine dei path Grasshopper preservato.

- **Rows** ← 07 Rows / Rows
- **Widths mm** (list): `70, 45, 40`
- **Header** (item): `True`
- **Options** (item): `{   "HeaderFill": "24445C",   "StripeFill": "EEF3F6",   "BorderColor": "CBD5DD",   "PaddingMm": 2,   "StyleId": null }`

### 21 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `80`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Schema geometrico dimostrativo`
- **Format** (item): `png`
- **Alignment** (item): `center`

### 22 Target — Template Target

Definisce un riempimento di content control o segnalibro inline.

- **Name** (item): `Cliente`
- **Blocks** ← 23 Paragraph / Block
- **Bookmark** (item): `True`

### 23 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Cliente dimostrativo`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 24 Control — Content Control

Controllo contenuto Word a blocchi identificato da tag.

- **Tag** (item): `NuovoControllo`
- **Blocks** ← 25 Paragraph / Block

### 25 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Contenuto iniziale del controllo`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 26 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 04 Fill / Document

### 27 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 04 Fill / Document
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

