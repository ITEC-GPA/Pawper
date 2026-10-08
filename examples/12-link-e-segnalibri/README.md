# 12 Link e segnalibri

Collegamenti esterni e interni e riferimenti al testo o alla pagina.

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
- **Blocks** ← 03 Heading / Block, 04 Paragraph / Block, 06 Paragraph / Block, 08 Bookmark / Block, 10 Field / Block, 11 Field / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Navigazione`
- **Level** (item): `0`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 05 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 05 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Documentazione Rhino`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): `https://developer.rhino3d.com/`
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 06 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 07 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 07 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `Vai ai risultati`
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): `#Risultati`
- **Math** (item): ``
- **Field** (item): ``
- **Style ID** (item): ``

### 08 Bookmark — Bookmark

Segnalibro univoco intorno a una sequenza di blocchi.

- **Name** (item): `Risultati`
- **Blocks** ← 09 Heading / Block

### 09 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Risultati verificati`
- **Level** (item): `1`

### 10 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `REF Risultati \h`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 11 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `PAGEREF Risultati \h`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 12 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 13 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 14 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 14 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 15 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 16 Path / Full path
- **Run** (item): `False`

### 16 Path — Project Path

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

