# 05 Tabelle intere da dati e testo

Creare tabelle con un solo componente da righe, colonne, testo Excel e CSV.

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

Percorso rapido: collegare il Block di Quick Table o Text Table a Blocks di Compose. Non servono Table Cell o Table Rows per queste tabelle.

Quick Table per righe: Data contiene {2;10} = [Luce, 6.5, m], {2;13} = [Carico, 12.3456, kN/m], {2;16} = [Dato mancante, null]. Headers = Elemento, Valore, Unità; Decimals = 2. I numeri sono allineati a destra. Le celle mancanti restano vuote.

Quick Table per colonne: collegare tre liste ai tre ingressi di Entwine, poi Entwine a Data e Columns=True. Le liste sono [Trave A, Trave B, Mancante], [1234.567, 28.9], [4.1234, 3.8]. Headers = Elemento, Forza, Lunghezza; Decimals = [-1,2,3], Units = [vuoto,kN,m], Culture=en-US. Il file incluso memorizza già questi rami, senza richiedere altri componenti. Verificare 1,234.57 kN e 4.123 m nella prima riga.

Il terzo Quick Table, scollegato da Compose, dimostra Headers vuoto, Widths vuoto, unità e decimali comuni a tutte le colonne, Strict=True. Mostra una matrice 2×2 con valori in mm. Collegarlo a Compose per includerlo; accorciare una riga per verificare il messaggio di errore.

Text Table: copiare il contenuto di dati.tsv in un Panel e collegarlo a Text; Separator=auto e Header=True. È supportato sia un unico testo multilinea sia un Panel con una lista di righe. Rows permette di controllare che 001 conservi gli zeri e che Prima riga/seconda riga sia una sola cella. Per leggere il file usare Read File di Grasshopper con il percorso relativo risolto da Project Path.

Secondo Text Table: dati.csv usa Separator=; e Header=False. La riga A è una riga di dati; il punto e virgola tra virgolette resta nella seconda cella. Si possono scegliere anche comma, tab e pipe. In auto un pareggio tra separatori produce un errore: scegliere quello effettivo del file.

Controlli ed errori: collegare Rows a un Panel per vedere la griglia; non applicare Flatten a Data. Un numero di Headers inferiore alle colonne è un errore. Strict=False completa righe o colonne corte; Strict=True le rifiuta. I testi, compresi 001 e 12,50 incollati, non vengono convertiti in numeri. Per controllare i decimali fornire numeri reali a Quick Table. In Text Table chiudere le virgolette e raddoppiare quelle interne.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Blocks di Compose usa esplicitamente Flatten per comporre un solo documento da tutte le sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 04 Paragraph / Block, 05 Heading / Block, 06 QuickTable / Block, 07 Heading / Block, 08 QuickTable / Block, 09 Break / Block, 10 Heading / Block, 11 TextTable / Block, 12 Heading / Block, 13 TextTable / Block, 14 Paragraph / Block, 15 Paragraph / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Tabelle intere da dati e testo`
- **Level** (item): `0`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Quattro modi per trasformare dati tabellari in tabelle Word modificabili. I valori mancanti restano vuoti e i codici mantengono gli zeri iniziali.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 05 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Dati organizzati per righe`
- **Level** (item): `1`

### 06 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Luce, 6.5, m]; {2;13} = [Carico, 12.3456, kN/m]; {2;16} = [Dato mancante, null]`
- **Headers** (list): `Elemento | Valore | Unità`
- **Columns** (item): `False`
- **Decimals** (list): `2`
- **Units** (list): ``
- **Culture** (item): `it-IT`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 07 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Dati organizzati per colonne`
- **Level** (item): `1`

### 08 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Trave A, Trave B, Mancante]; {2;13} = [1234.567, 28.9]; {2;16} = [4.1234, 3.8]`
- **Headers** (list): `Elemento | Forza | Lunghezza`
- **Columns** (item): `True`
- **Decimals** (list): `-1, 2, 3`
- **Units** (list): ` | kN | m`
- **Culture** (item): `en-US`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 09 Break — Page Break

Interruzione di pagina.


### 10 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Testo copiato da Excel`
- **Level** (item): `1`

### 11 TextTable — Text Table

Tabella intera da testo incollato da Excel (TAB), CSV o righe di un Panel. Gestisce virgolette e celle multilinea.

- **Text** (list): `Codice	Descrizione	Valore | 001	"Prima riga | seconda riga"	12,50 | 002	"Testo con ""virgolette"""	 | 003	Dato mancante`
- **Separator** (item): `auto`
- **Header** (item): `True`
- **Widths mm** (list): `25, 90, 40`

### 12 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `CSV senza intestazione`
- **Level** (item): `1`

### 13 TextTable — Text Table

Tabella intera da testo incollato da Excel (TAB), CSV o righe di un Panel. Gestisce virgolette e celle multilinea.

- **Text** (list): `A;"Testo; con separatore";12,50 | B;"Virgolette ""OK"""; | C;Fine;0`
- **Separator** (item): `;`
- **Header** (item): `False`
- **Widths mm** (list): `25, 90, 40`

### 14 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Italiano: 1.234,57 kN`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 15 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `English: 1,234.567 kN`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 16 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [1, 2]; {2;13} = [3, 4]`
- **Headers** (list): ``
- **Columns** (item): `False`
- **Decimals** (list): `1`
- **Units** (list): `mm`
- **Culture** (item): `it-IT`
- **Widths mm** (list): ``
- **Strict** (item): `True`

### 17 Number — Format Number

Formatta un risultato con cultura, decimali e unità.

- **Value** (item): `1234.567`
- **Decimals** (item): `2`
- **Unit** (item): `kN`
- **Culture** (item): `it-IT`

### 18 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** ← 17 Number / Text
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 19 Number — Format Number

Formatta un risultato con cultura, decimali e unità.

- **Value** (item): `1234.567`
- **Decimals** (item): `3`
- **Unit** (item): `kN`
- **Culture** (item): `en-US`

### 20 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** ← 19 Number / Text
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 21 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 02 Compose / Document

### 22 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 02 Compose / Document
- **Path** ← 23 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 23 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 24 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 25 Path / Full path
- **Run** (item): `False`

### 25 Path — Project Path

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

