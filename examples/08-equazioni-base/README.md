# 08 Equazioni modificabili

Scrivere formule inline e separate con frazioni, radici, indici ed esponenti.

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

Text Blocks: doppio clic sul corpo o menu Modifica testo per l'editor; in alternativa collegare un Panel multilinea o una lista di righe. Un ramo produce una lista ordinata, rami distinti restano distinti. Righe vuote separano paragrafi; #..######### crea titoli, **grassetto**, *corsivo*, ***entrambi***. Gli elenchi -/+ e 1. usano due spazi per livello. $formula$ è inline, $$formula$$ separata; --- crea una pagina nuova. Usare testo.txt dove incluso. Editor disabilitato se Text è collegato o contiene più rami; Annulla non modifica l'input. Sintassi non chiusa produce un errore con numero di riga.

## Componenti e collegamenti

I numeri corrispondono ai nickname nel canvas. I valori sono già salvati. Dove presente, Blocks di Compose o Quick Document usa esplicitamente Flatten per comporre un solo documento da più sorgenti; rimuoverlo per elaborare rami separati.

### 01 New — New Document

Crea un documento vuoto oppure carica una copia di DOCX/DOTX.

- **Template path** (item): ``

### 02 Compose — Compose Document

Aggiunge blocchi ordinati a una nuova copia del documento.

- **Document** ← 01 New / Document
- **Blocks** ← 03 Heading / Block, 04 Paragraph / Block, 08 Equation / Block, 09 Equation / Block, 10 TextBlocks / Blocks
- **Styles** (list): `opzionale / vuoto`
- **Layout** (item): `opzionale / vuoto`

### 03 Heading — Heading

Titolo, sottotitolo o intestazione gerarchica.

- **Text** (item): `Equazioni native`
- **Level** (item): `0`

### 04 Paragraph — Paragraph

Paragrafo con testo, run misti e stile Word.

- **Text** (item): ``
- **Runs** ← 05 Run / Run, 06 Run / Run, 07 Run / Run
- **Style ID** (item): `Normal`
- **Format** (item): ``

### 05 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): `La relazione `
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

### 06 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): ``
- **Font** (item): `Aptos`
- **Size** (item): `11`
- **Bold** (item): `False`
- **Italic** (item): `False`
- **Underline** (item): `False`
- **Color** (item): `000000`
- **Script** (item): `baseline`
- **Link** (item): ``
- **Math** (item): `\sigma=\frac{N}{A}`
- **Field** (item): ``
- **Style ID** (item): ``

### 07 Run — Text Run

Testo formattato, collegamento, formula inline o campo.

- **Text** (item): ` è una formula inline modificabile.`
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

### 08 Equation — Word Equation

Formula LaTeX nel sottoinsieme documentato, convertita in OMML modificabile.

- **LaTeX** (item): `x_1^2+\sqrt[3]{a}=\frac{b}{c}`
- **Inline** (item): `False`
- **Numbered** (item): `False`
- **Bookmark** (item): ``

### 09 Equation — Word Equation

Formula LaTeX nel sottoinsieme documentato, convertita in OMML modificabile.

- **LaTeX** (item): `\alpha+\beta+\gamma=\pi`
- **Inline** (item): `True`
- **Numbered** (item): `False`
- **Bookmark** (item): ``

### 10 TextBlocks — Text Blocks

Titoli, paragrafi, elenchi e formule da un testo unico. Doppio clic per aprire l'editor; sottoinsieme documentato, non Markdown completo.

- **Text** (list): `# Formule da testo | La tensione vale $\sigma=\frac{N}{A}$ nel modello dimostrativo. |  | $$x_1^2+\sqrt{a}$$ |  | $$ | \sum_{i=1}^{n}{x_i} | $$`

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

