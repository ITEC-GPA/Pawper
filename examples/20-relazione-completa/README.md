# 20 Relazione tecnica completa

Combinare copertina, sommario, dati, immagini, equazioni, note e sezioni.

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
- **Blocks** ← 03 Heading / Block, 04 Heading / Block, 05 Paragraph / Block, 06 Break / Block, 07 Heading / Block, 08 Field / Block, 09 Heading / Block, 10 QuickTable / Block, 11 Heading / Block, 12 Image / Block, 13 Equation / Block, 14 Paragraph / Block, 15 Note / Block, 16 Section / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** ← 20 Layout / Layout

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Relazione tecnica dimostrativa`
- **Level** (item): `0`

### 04 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Progetto di una trave appoggiata`
- **Level** (item): `-1`

### 05 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `Esempio completo di documentazione parametrica generata da Grasshopper.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 06 Break — Page Break

Interruzione di pagina.


### 07 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Sommario`
- **Level** (item): `1`

### 08 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `TOC \o "1-3" \h \z \u`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 09 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Dati del modello`
- **Level** (item): `1`

### 10 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Luce, 6.5, m]; {2;13} = [Carico, 12.3456, kN/m]; {2;16} = [Dato mancante, null]`
- **Headers** (list): `Elemento | Valore | Unità`
- **Columns** (item): `False`
- **Decimals** (list): `2`
- **Units** (list): ``
- **Culture** (item): `it-IT`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 11 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Schema e risultati`
- **Level** (item): `1`

### 12 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `120`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Schema geometrico dimostrativo`
- **Format** (item): `png`
- **Alignment** (item): `center`

### 13 Equation — Word Equation

Formula LaTeX nel sottoinsieme documentato, convertita in OMML modificabile.

- **LaTeX** (item): `M_{max}=\frac{qL^2}{8}`
- **Inline** (item): `False`
- **Numbered** (item): `True`
- **Bookmark** (item): `Momento`

### 14 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `La formula e i dati hanno finalità dimostrative e non costituiscono una verifica strutturale.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 15 Note — Word Note

Nota a piè di pagina o nota finale nativa.

- **Anchor** (item): `Ipotesi di modello`
- **Text** (item): `Il modello rappresenta una trave semplicemente appoggiata.`
- **Endnote** (item): `False`

### 16 Section — Section

Inizia una nuova sezione con layout dedicato.

- **Layout** ← 17 Layout / Layout
- **Blocks** ← 18 Heading / Block, 19 QuickTable / Block

### 17 Layout — Page Layout

Dimensioni e margini in millimetri; intestazioni e piè di pagina.

- **Width mm** (item): `215.9`
- **Height mm** (item): `279.4`
- **Margin mm** (item): `20`
- **Landscape** (item): `False`
- **Header** (item): ``
- **Footer** (item): ``
- **Advanced** (item): `{   "WidthMm": 210,   "HeightMm": 297,   "MarginMm": 20,   "Landscape": true,   "Header": "Appendice",   "Footer": "Pagina {{PAGE}}",   "FirstHeader": null,   "FirstFooter": null,   "EvenHeader": null,   "EvenFooter": null,   "PageStart": null }`

### 18 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Appendice tabellare`
- **Level** (item): `1`

### 19 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Luce, 6.5, m]; {2;13} = [Carico, 12.3456, kN/m]; {2;16} = [Dato mancante, null]`
- **Headers** (list): `Elemento | Valore | Unità`
- **Columns** (item): `False`
- **Decimals** (list): `2`
- **Units** (list): ``
- **Culture** (item): `it-IT`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 20 Layout — Page Layout

Dimensioni e margini in millimetri; intestazioni e piè di pagina.

- **Width mm** (item): `215.9`
- **Height mm** (item): `279.4`
- **Margin mm** (item): `20`
- **Landscape** (item): `False`
- **Header** (item): ``
- **Footer** (item): ``
- **Advanced** (item): `{   "WidthMm": 210,   "HeightMm": 297,   "MarginMm": 20,   "Landscape": false,   "Header": "Pawper \u2014 Relazione tecnica",   "Footer": "Pagina {{PAGE}} di {{NUMPAGES}}",   "FirstHeader": null,   "FirstFooter": null,   "EvenHeader": null,   "EvenFooter": null,   "PageStart": null }`

### 21 Metadata — Document Metadata

Proprietà standard e personalizzate del documento.

- **Document** ← 02 Compose / Document
- **Title** (item): `Relazione tecnica dimostrativa`
- **Author** (item): `Pawper`
- **Subject** (item): `Esempio completo`
- **Keywords** (item): `relazione;Grasshopper`
- **Custom** (item): `{}`

### 22 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 21 Metadata / Document

### 23 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 21 Metadata / Document
- **Path** ← 24 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 24 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 25 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 26 Path / Full path
- **Run** (item): `False`

### 26 Path — Project Path

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

