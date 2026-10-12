# 06 Celle e unioni

Unire righe e colonne e inserire contenuti formattati nelle celle.

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

Table Format: collegare una tabella a Table e tre Colour Swatch opachi a Header, Stripe e Border. Padding è in mm. Style ID vuoto mantiene lo stile precedente. L'output è una copia: un secondo ramo resta invariato. Nell'esempio 06 l'intestazione chiara usa testo nero; gli altri esempi mostrano intestazioni scure con testo bianco. Celle unite e contenuti complessi sono conservati.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Dove presente, Blocks di Compose o Quick Document usa esplicitamente Flatten per comporre un solo documento da più sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 18 TableFormat / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Tabella composta`
- **Level** (item): `0`

### 04 Rows — Table Rows

Organizza una lista ordinata di celle in righe con lunghezze esplicite.

- **Cells** ← 05 Cell / Cell, 06 Cell / Cell, 07 Cell / Cell, 08 Cell / Cell, 09 Cell / Cell, 10 Cell / Cell, 11 Cell / Cell, 14 Cell / Cell, 15 Cell / Cell
- **Counts** (list): `2, 3, 2, 2`

### 05 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Verifiche`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `2`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 06 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Esito`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 07 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Gruppo A`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `2`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 08 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Resistenza`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 09 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `OK`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): `DDEEDD`
- **Alignment** (item): `center`

### 10 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Deformazione`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 11 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** ← 12 Paragraph / Block
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): `FFF0CC`
- **Alignment** (item): `left`

### 12 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 13 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 13 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `ATTENZIONE`
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

### 14 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): `Schema`
- **Blocks** (list): `opzionale / vuoto`
- **Column span** (item): `2`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 15 Cell — Table Cell

Cella con blocchi, unioni e colore condizionale collegabile.

- **Text** (item): ``
- **Blocks** ← 16 Image / Block
- **Column span** (item): `1`
- **Row span** (item): `1`
- **Fill** (item): ``
- **Alignment** (item): `left`

### 16 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `35`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): ``
- **Format** (item): `png`
- **Alignment** (item): `center`

### 17 Table — Word Table

Ogni ramo del Data Tree è una riga; ogni elemento è una cella. Ordine dei path Grasshopper preservato.

- **Rows** ← 04 Rows / Rows
- **Widths mm** (list): `40, 65, 50`
- **Header** (item): `True`
- **Options** (item): `{   "HeaderFill": "DCEAF2",   "StripeFill": "F3F6F8",   "BorderColor": "D9D9D9",   "PaddingMm": 2.5,   "StyleId": null }`

### 18 TableFormat — Table Format

Colori e margini di una tabella completa. Preserva contenuti, celle unite e larghezze; restituisce una copia.

- **Table** ← 17 Table / Block
- **Header** (item): `#DCEAF2`
- **Stripe** (item): `#F3F6F8`
- **Border** (item): `#D9D9D9`
- **Padding mm** (item): `2.5`
- **Style ID** (item): ``

### 19 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 20 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 21 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 21 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 22 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 23 Path / Full path
- **Run** (item): `False`

### 23 Path — Project Path

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

