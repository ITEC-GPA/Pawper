# 03 Stili e titoli

Usare identificatori indipendenti dalla lingua di Word e tutti i livelli di titolo.

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

Paragraph Format: scegliere l'allineamento dal menu o dall'input; Before/After sono punti, Line è un multiplo. Collegare Format a Paragraph.Format oppure Word Style.Paragraph format. Keep next mantiene il paragrafo col successivo. Per rientri e opzioni aggiuntive resta disponibile il formato avanzato.

Table Format: collegare una tabella a Table e tre Colour Swatch opachi a Header, Stripe e Border. Padding è in mm. Style ID vuoto mantiene lo stile precedente. L'output è una copia: un secondo ramo resta invariato. Nell'esempio 06 l'intestazione chiara usa testo nero; gli altri esempi mostrano intestazioni scure con testo bianco. Celle unite e contenuti complessi sono conservati.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Dove presente, Blocks di Compose o Quick Document usa esplicitamente Flatten per comporre un solo documento da più sorgenti; rimuoverlo per elaborare rami separati.

### 01 QuickDoc — Quick Document

Crea un documento con titolo, blocchi ordinati e impostazioni opzionali. Il salvataggio resta esplicito in Save Word.

- **Title** (item): ``
- **Blocks** ← 02 Heading / Block, 03 Heading / Block, 04 Paragraph / Block, 05 Heading / Block, 06 Heading / Block, 07 Heading / Block, 08 Heading / Block, 09 Heading / Block, 10 Heading / Block, 11 Heading / Block, 12 Heading / Block, 13 Heading / Block, 14 Paragraph / Block, 30 TableFormat / Block
- **Layout** (item): `opzionale / vuoto`
- **Template path** (item): ``
- **Styles** ← 31 Style / Style, 33 Style / Style, 35 Style / Style

### 02 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Stili del progetto`
- **Level** (item): `0`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Sottotitolo della relazione`
- **Level** (item): `-1`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Questo paragrafo applica ProjectBody.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `ProjectBody`
- **Format** (item): ``

### 05 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 1`
- **Level** (item): `1`

### 06 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 2`
- **Level** (item): `2`

### 07 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 3`
- **Level** (item): `3`

### 08 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 4`
- **Level** (item): `4`

### 09 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 5`
- **Level** (item): `5`

### 10 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 6`
- **Level** (item): `6`

### 11 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 7`
- **Level** (item): `7`

### 12 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 8`
- **Level** (item): `8`

### 13 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Intestazione livello 9`
- **Level** (item): `9`

### 14 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 15 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 15 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Stile carattere Evidenza`
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
- **Style ID** (item): `EmphasisProject`

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
- **Options** (item): `{   "HeaderFill": "24445C",   "StripeFill": "EEF3F6",   "BorderColor": "CBD5DD",   "PaddingMm": 2,   "StyleId": "ProjectTable" }`

### 30 TableFormat — Table Format

Colori e margini di una tabella completa. Preserva contenuti, celle unite e larghezze; restituisce una copia.

- **Table** ← 29 Table / Block
- **Header** (item): `#24445C`
- **Stripe** (item): `#EEF3F6`
- **Border** (item): `#CBD5DD`
- **Padding mm** (item): `2`
- **Style ID** (item): `ProjectTable`

### 31 Style — Word Style

Crea o aggiorna uno stile Word per ID, distinto dal nome visualizzato.

- **ID** (item): `ProjectBody`
- **Name** (item): `Testo progetto`
- **Based on** (item): `Normal`
- **Run format** (item): `{   "Font": "Aptos",   "Size": 11,   "Bold": false,   "Italic": false,   "Underline": false,   "Color": "24445C",   "Script": null }`
- **Paragraph format** ← 32 ParaFormat / Format
- **Type** (item): `paragraph`

### 32 ParaFormat — Paragraph Format

Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.

- **Alignment** (item): `left`
- **Before pt** (item): `0`
- **After pt** (item): `8`
- **Line** (item): `1.15`
- **Keep next** (item): `False`

### 33 Style — Word Style

Crea o aggiorna uno stile Word per ID, distinto dal nome visualizzato.

- **ID** (item): `EmphasisProject`
- **Name** (item): `Evidenza`
- **Based on** (item): `Normal`
- **Run format** (item): `{   "Font": null,   "Size": null,   "Bold": false,   "Italic": true,   "Underline": false,   "Color": null,   "Script": null }`
- **Paragraph format** ← 34 ParaFormat / Format
- **Type** (item): `character`

### 34 ParaFormat — Paragraph Format

Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.

- **Alignment** (item): `left`
- **Before pt** (item): `0`
- **After pt** (item): `6`
- **Line** (item): `1.15`
- **Keep next** (item): `False`

### 35 Style — Word Style

Crea o aggiorna uno stile Word per ID, distinto dal nome visualizzato.

- **ID** (item): `ProjectTable`
- **Name** (item): `Tabella progetto`
- **Based on** (item): `Normal`
- **Run format** (item): `{   "Font": "Aptos",   "Size": 11,   "Bold": false,   "Italic": false,   "Underline": false,   "Color": null,   "Script": null }`
- **Paragraph format** ← 36 ParaFormat / Format
- **Type** (item): `table`

### 36 ParaFormat — Paragraph Format

Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.

- **Alignment** (item): `left`
- **Before pt** (item): `0`
- **After pt** (item): `6`
- **Line** (item): `1.15`
- **Keep next** (item): `False`

### 37 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 01 QuickDoc / Document

### 38 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 01 QuickDoc / Document
- **Path** ← 39 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 39 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 40 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 41 Path / Full path
- **Run** (item): `False`

### 41 Path — Project Path

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

