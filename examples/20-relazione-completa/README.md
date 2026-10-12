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
3. Modificare gli input dal menu contestuale; Text Blocks e Text Table offrono anche un editor con doppio clic. Dove presenti, i formati avanzati sono già impostati.
4. In Save Word portare Run su False e poi True. output.docx sarà creato accanto alla definizione. Per sostituire un file esistente abilitare Overwrite.
5. Ricalcolare con Run ancora True: non deve esserci una nuova scrittura.
6. In Open Result portare Run su False e poi True.
7. In Word aggiornare i campi con F9.

Paragraph Format: scegliere l'allineamento dal menu o dall'input; Before/After sono punti, Line è un multiplo. Collegare Format a Paragraph.Format oppure Word Style.Paragraph format. Keep next mantiene il paragrafo col successivo. Per rientri e opzioni aggiuntive resta disponibile il formato avanzato.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Dove presente, Blocks di Compose o Quick Document usa esplicitamente Flatten per comporre un solo documento da più sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 04 Heading / Block, 05 Paragraph / Block, 07 Break / Block, 08 Heading / Block, 09 Field / Block, 10 Heading / Block, 11 QuickTable / Block, 12 Heading / Block, 13 Image / Block, 14 Equation / Block, 15 Paragraph / Block, 17 Note / Block, 18 Section / Block
- **Styles** (list): `opzionale / vuoto`
- **Layout** ← 22 PagePreset / Layout

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
- **Format** ← 06 ParaFormat / Format

### 06 ParaFormat — Paragraph Format

Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.

- **Alignment** (item): `left`
- **Before pt** (item): `0`
- **After pt** (item): `6`
- **Line** (item): `1.15`
- **Keep next** (item): `False`

### 07 Break — Page Break

Interruzione di pagina.


### 08 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Sommario`
- **Level** (item): `1`

### 09 Field — Word Field

Campo Word: TOC, SEQ, REF, PAGEREF, PAGE, NUMPAGES. Richiede aggiornamento in Word.

- **Instruction** (item): `TOC \o "1-3" \h \z \u`
- **Placeholder** (item): `Aggiornare il campo in Word`
- **Style ID** (item): `Normal`

### 10 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Dati del modello`
- **Level** (item): `1`

### 11 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Luce, 6.5, m]; {2;13} = [Carico, 12.3456, kN/m]; {2;16} = [Dato mancante, null]`
- **Headers** (list): `Elemento | Valore | Unità`
- **Columns** (item): `False`
- **Decimals** (list): `2`
- **Units** (list): ``
- **Culture** (item): `it-IT`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 12 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Schema e risultati`
- **Level** (item): `1`

### 13 Image — Word Image

Incorpora PNG/JPEG da percorso o Base64 mantenendo le proporzioni.

- **Path or Base64** (item): `Immagine Base64 incorporata; sostituibile tramite Image con un percorso.`
- **Width mm** (item): `120`
- **Alt text** (item): `Schema di una trave appoggiata con carico distribuito`
- **Caption** (item): `Schema geometrico dimostrativo`
- **Format** (item): `png`
- **Alignment** (item): `center`

### 14 Equation — Word Equation

Formula LaTeX nel sottoinsieme documentato, convertita in OMML modificabile.

- **LaTeX** (item): `M_{max}=\frac{qL^2}{8}`
- **Inline** (item): `False`
- **Numbered** (item): `True`
- **Bookmark** (item): `Momento`

### 15 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): `La formula e i dati hanno finalità dimostrative e non costituiscono una verifica strutturale.`
- **Runs** (list): `opzionale / vuoto`
- **Style ID** (item): `Normal`
- **Format** ← 16 ParaFormat / Format

### 16 ParaFormat — Paragraph Format

Impostazioni del paragrafo con input diretti. Collegare Format a Paragraph oppure a Paragraph format di Word Style.

- **Alignment** (item): `left`
- **Before pt** (item): `0`
- **After pt** (item): `6`
- **Line** (item): `1.15`
- **Keep next** (item): `False`

### 17 Note — Word Note

Nota a piè di pagina o nota finale nativa.

- **Anchor** (item): `Ipotesi di modello`
- **Text** (item): `Il modello rappresenta una trave semplicemente appoggiata.`
- **Endnote** (item): `False`

### 18 Section — Section

Inizia una nuova sezione con layout dedicato.

- **Layout** ← 19 PagePreset / Layout
- **Blocks** ← 20 Heading / Block, 21 QuickTable / Block

### 19 PagePreset — Page Preset

Pagina A4, A3, Letter o Legal con margini e intestazioni. Per impostazioni di sezione avanzate usare Page Layout.

- **Format** (item): `A4`
- **Landscape** (item): `True`
- **Margin mm** (item): `20`
- **Header** (item): `Appendice`
- **Footer** (item): `Pagina {{PAGE}}`

### 20 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Appendice tabellare`
- **Level** (item): `1`

### 21 QuickTable — Quick Table

Crea una tabella intera da righe o colonne di dati, con intestazioni e formattazione numerica diretta.

- **Data** (tree): `{2;10} = [Luce, 6.5, m]; {2;13} = [Carico, 12.3456, kN/m]; {2;16} = [Dato mancante, null]`
- **Headers** (list): `Elemento | Valore | Unità`
- **Columns** (item): `False`
- **Decimals** (list): `2`
- **Units** (list): ``
- **Culture** (item): `it-IT`
- **Widths mm** (list): `70, 45, 40`
- **Strict** (item): `False`

### 22 PagePreset — Page Preset

Pagina A4, A3, Letter o Legal con margini e intestazioni. Per impostazioni di sezione avanzate usare Page Layout.

- **Format** (item): `A4`
- **Landscape** (item): `False`
- **Margin mm** (item): `20`
- **Header** (item): `Pawper — Relazione tecnica`
- **Footer** (item): `Pagina {{PAGE}} di {{NUMPAGES}}`

### 23 Metadata — Document Metadata

Proprietà standard e personalizzate del documento.

- **Document** ← 02 Compose / Document
- **Title** (item): `Relazione tecnica dimostrativa`
- **Author** (item): `Pawper`
- **Subject** (item): `Esempio completo`
- **Keywords** (item): `relazione;Grasshopper`
- **Custom** (item): `{}`

### 24 Inspect — Document Inspector

Struttura, stili, metadati e validazione; non è un'anteprima impaginata.

- **Document** ← 23 Metadata / Document

### 25 Save — Save Word

Scrive solo sul fronte False→True. Dopo riapertura portare Run a False prima di salvare.

- **Document** ← 23 Metadata / Document
- **Path** ← 26 Path / Full path
- **Run** (item): `False`
- **Overwrite** (item): `False`

### 26 Path — Project Path

Risolve un percorso rispetto al file .gh salvato, per esempi portabili.

- **Relative path** (item): `output.docx`

### 27 Open — Open Result

Apre il DOCX nell'applicazione associata solo su comando esplicito.

- **Path** ← 28 Path / Full path
- **Run** (item): `False`

### 28 Path — Project Path

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

