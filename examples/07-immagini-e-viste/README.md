# 07 Immagini e viste Rhino

Incorporare PNG e JPEG, mantenere le proporzioni e acquisire viste attive o nominate.

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

Aprire il file incluso model.3dm in una finestra Rhino visibile. Sbloccare Capture Rhino View e portare Capture da False a True. Named view vuoto usa la vista attiva; per una vista nominata salvarla prima in Rhino e inserire il suo nome. Collegare l'output a Compose. L'acquisizione interattiva resta da collaudare manualmente; il rifiuto della cattura in modalità headless è stato verificato.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Blocks di Compose usa esplicitamente Flatten per comporre un solo documento da tutte le sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 04 Image / Block, 05 Image / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Immagini del progetto`
- **Level** (item): `0`

### 04 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `120`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Schema geometrico dimostrativo`
- **Format** (item): `png`
- **Alignment** (item): `center`

### 05 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `80`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Versione JPEG`
- **Format** (item): `jpg`
- **Alignment** (item): `left`

### 06 Capture — Capture Rhino View

Acquisisce la vista attiva o nominata su impulso; ripristina la vista originale.

- **Named view** (item): ``
- **Width pixels** (item): `1600`
- **Height pixels** (item): `1000`
- **Transparent** (item): `False`
- **Width mm** (item): `150`
- **Capture** (item): `False`

### 07 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** ← 08 Path / Full path
- **Width mm** (item): `100`
- **Alt text** (item): `Schema da file`
- **Caption** (item): ``
- **Format** (item): ``
- **Alignment** (item): `center`

### 08 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `../assets/diagram.png`

### 09 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 10 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 11 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 11 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 12 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 13 Path / Full path
- **Run** (item): `False`

### 13 Path — Project Path

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

