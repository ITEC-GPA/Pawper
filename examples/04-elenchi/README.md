# 04 Elenchi multilivello

Numerare voci, annidare livelli, usare punti e riavviare un elenco.

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
- **Blocks** ← 03 Heading / Block, 04 List / Block, 05 List / Block, 06 List / Block, 07 List / Block, 08 List / Block, 09 List / Block, 10 List / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Elenchi`
- **Level** (item): `0`

### 04 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Prima voce`
- **Level** (item): `0`
- **Bullet** (item): `False`
- **Group** (item): `default`

### 05 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Sottovoce`
- **Level** (item): `1`
- **Bullet** (item): `False`
- **Group** (item): `default`

### 06 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Dettaglio`
- **Level** (item): `2`
- **Bullet** (item): `False`
- **Group** (item): `default`

### 07 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Seconda voce`
- **Level** (item): `0`
- **Bullet** (item): `False`
- **Group** (item): `default`

### 08 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Punto principale`
- **Level** (item): `0`
- **Bullet** (item): `True`
- **Group** (item): `default`

### 09 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Punto secondario`
- **Level** (item): `1`
- **Bullet** (item): `True`
- **Group** (item): `default`

### 10 List — List Item

Voce di elenco numerato o puntato multilivello.

- **Text** (item): `Nuovo elenco`
- **Level** (item): `0`
- **Bullet** (item): `False`
- **Group** (item): `second`

### 11 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 12 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 13 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 13 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 14 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 15 Path / Full path
- **Run** (item): `False`

### 15 Path — Project Path

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

