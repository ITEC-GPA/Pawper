# Manuale Pawper

## Composizione

Collegare New Document a Compose Document. Collegare titoli, paragrafi, tabelle, immagini e formule all'ingresso Blocks di Compose nell'ordine desiderato. Una lista mantiene l'ordine; Grasshopper elabora normalmente i rami indipendentemente. Non viene eseguito alcun appiattimento implicito dei Data Tree.

I tipi Document, ContentBlock, TextRun, Style, PageLayout, Cell e TemplateTarget hanno parametri persistenti dedicati. I valori sono record immutabili con raccolte immutabili; la duplicazione e il salvataggio nei file GH avvengono tramite JSON con discriminatori di tipo espliciti. Nessun riferimento a pacchetti Word aperti attraversa i collegamenti.

## Testo e stili

Heading usa Level 0 per Title, -1 per Subtitle e 1..9 per Heading1..Heading9. Paragraph usa l'ID dello stile; l'ID è distinto dal nome visualizzato in Word. Uno stile assente produce un errore. Word Style crea/aggiorna uno stile e Compose lo aggiunge al documento, lasciando gli altri stili del template intatti.

Text Run controlla font, punti, grassetto, corsivo, sottolineato, RGB senza #, apice/pedice, link, formule inline e campi. L'input Style ID applica uno stile carattere tramite il suo identificatore Word. In Paragraph il testo iniziale precede l'elenco dei run. I formati diretti prevalgono sullo stile. Per ereditare interamente lo stile usare Text di Paragraph senza run formattati.

Format di Paragraph accetta JSON opzionale. Un input vuoto eredita lo stile; `{}` applica i valori predefiniti del formato.

```json
{"Alignment":"both","Before":0,"After":8,"Line":1.15,"LeftMm":0,"RightMm":0,"FirstLineMm":5,"KeepNext":false}
```

Before/After sono punti, Line è un moltiplicatore, i rientri sono millimetri. FirstLineMm negativo crea un rientro sporgente. List Item usa livelli 0..8; Group identifica la sequenza numerata. Cambiare Group riavvia la numerazione. Bullet separa gli elenchi puntati da quelli numerati.

## Tabelle

Per una tabella intera partire da **Quick Table** o **Text Table**: basta collegare l'output Block a Blocks di Compose Document. Entrambi producono tabelle Word native e modificabili, con bordo, righe alternate e intestazione ripetuta. Non occorre creare una Table Cell per ogni valore.

| Dati disponibili | Componente | Collegamento minimo |
|---|---|---|
| Data Tree con una riga per ramo | Quick Table | Data + Headers opzionale → Block |
| Liste di nomi e risultati, una per colonna | Quick Table | Liste → Entwine → Data; Columns=True |
| Area copiata da Excel o testo CSV | Text Table | Panel → Text → Block |
| Celle unite, immagini o testo formattato nelle celle | Word Table | Data Tree di Table Cell → Rows |

### Quick Table

- **Data**: numeri, stringhe, booleani, date o null; un ramo per riga con Columns=False, oppure un ramo per colonna con Columns=True. I rami sono letti nell'ordine dei path Grasshopper, anche con path sparsi o profondi. Non applicare Flatten. Un singolo ramo è una riga, oppure una colonna nella seconda modalità.
- **Headers**: lista opzionale con un titolo per colonna. Se vuota, tutte le righe restano dati e non viene creata un'intestazione. La prima riga di Data non viene mai consumata. Per righe più corte delle intestazioni vengono aggiunte celle vuote; dati più larghi delle intestazioni sono un errore. In modalità colonne servono tante intestazioni quanti sono i rami.
- **Columns**: False per righe, True per colonne. Per tre liste di risultati collegarle ai tre ingressi di Entwine e poi usare Columns=True. Controllare che ciascuna lista occupi un solo ramo.
- **Decimals**: -1 conserva la precisione; 0..15 imposta decimali e separatore delle migliaia. Un valore vale per tutte le colonne; una lista deve avere esattamente un valore per colonna. Si applica solo ai numeri, non alle stringhe.
- **Units**: vuoto per nessuna unità; un valore comune oppure uno per colonna. Usare una stringa vuota nelle colonne senza unità. I null restano vuoti e i testi non ricevono suffissi.
- **Culture**: it-IT predefinita; en-US produce, per esempio, 1,234.57 anziché 1.234,57.
- **Widths mm**: opzionale, una larghezza positiva per colonna; vuoto usa larghezze uniformi nella pagina.
- **Strict**: False completa righe/colonne corte con celle vuote e segnala la differenza; True rifiuta i rami di lunghezza diversa (e le righe più corte dell'intestazione).

Senza Headers la larghezza viene ricavata da tutte le righe, quindi una prima riga corta non tronca le successive. Gli elementi null e i rami vuoti mantengono la propria posizione. I numeri sono allineati a destra, il testo a sinistra. Sono rifiutati NaN, infinito, oggetti geometrici e più di 63 colonne. Convertire esplicitamente la geometria in testi/valori prima dell'ingresso.

L'output **Rows**, collegato a un Panel, mostra la griglia formattata inclusa l'intestazione. È un'anteprima dei dati, non delle pagine Word. Per creare più documenti continuare a usare la composizione e il batch; un Data Tree intero in Data produce una sola tabella.

### Text Table

Copiare un rettangolo di celle da Excel, incollarlo in un Panel e collegare il Panel a **Text**. Sono accettati un testo multilinea oppure una lista di righe nello stesso ramo. Non è richiesta un'installazione di Excel; il plugin elabora il testo incollato. Non legge direttamente file XLSX né accede automaticamente agli appunti. Per CSV/TSV su disco collegare il normale Read File di Grasshopper, con il percorso risolto da Project Path.

- **Separator**: auto (predefinito), tab/tsv, comma, semicolon, pipe, oppure un singolo carattere. Auto conta TAB, punto e virgola, virgola e barra verticale nel primo record fuori dalle virgolette. Se due separatori hanno lo stesso conteggio massimo, scegliere esplicitamente quello del file. Senza separatori riconosciuti si ottiene una sola colonna.
- **Header**: True usa il primo record come intestazione; False lo mantiene come dato.
- **Widths mm**: opzionale, come Quick Table.
- **Rows**: griglia risultante, inclusa l'eventuale intestazione e il completamento delle celle mancanti. **Separator** in output indica il carattere scelto (TAB per la tabulazione).

Esempio da incollare in un Panel (Separator=semicolon, Header=True):

```text
Elemento;Lunghezza;Nota
Trave A;6,50;"Descrizione; con separatore"
Trave B;4,20;
```

Le celle tra virgolette possono contenere separatori e ritorni a capo. Una virgoletta interna si scrive doppia: `"Testo con ""virgolette"""`. Le righe completamente vuote vengono ignorate; una riga di delimitatori o `""` conserva celle vuote. I campi rimangono testo: codici come 001, date e virgole decimali non vengono reinterpretati. Per formattare risultati numerici usare Quick Table con numeri reali. Le righe corte sono completate, quelle più larghe dell'intestazione sono rifiutate; senza intestazione viene usata la riga più larga. Le virgolette non chiuse producono un errore con numero di riga.

Vedere l'[esempio 05](../examples/05-tabella-numerica/README.md), che contiene entrambe le modalità e i file dati.tsv/dati.csv, e l'[esempio 20](../examples/20-relazione-completa/README.md), con due tabelle complete create da due soli Quick Table.

La gestione dei rami segue la [documentazione McNeel sui Data Tree](https://developer.rhino3d.com/guides/rhinopython/grasshopper-datatrees-and-python/). Il testo delimitato usa [TextFieldParser di .NET](https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.fileio.textfieldparser), incluse [celle tra virgolette e multilinea](https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.fileio.textfieldparser.hasfieldsenclosedinquotes), senza nuove dipendenze NuGet.

### Tabelle con celle complesse

Word Table tratta ogni ramo come una riga e ogni elemento come una cella, in ordine di path Grasshopper. Accetta stringhe, numeri già formattati e valori Table Cell. Usare Format Number per controllare cultura, decimali e unità: la conversione generica di un numero nella tabella segue la rappresentazione Grasshopper.

Table Rows permette di collegare celle ordinate e specificare Counts, per esempio `[3,3,2]`: produce i rami `{0}`, `{1}`, `{2}`. I conteggi devono coincidere con il numero di celle.

La prima riga determina la griglia contando ColumnSpan. Righe più corte vengono completate con celle vuote; righe troppo lunghe sono rifiutate. Per RowSpan, omettere nelle righe successive le celle già coperte dall'unione. Le unioni fuori griglia o sovrapposte sono rifiutate. Non sono ammesse tabelle senza righe. Non vengono imposte altezze fisse: i testi lunghi vanno a capo. Header ripete la prima riga sulle pagine successive.

Widths mm contiene una larghezza positiva per ogni colonna fisica. Se vuoto, vengono calcolate larghezze uniformi dalla pagina corrente. Options:

```json
{"HeaderFill":"24445C","StripeFill":"EEF3F6","BorderColor":"CBD5DD","PaddingMm":2,"StyleId":null}
```

Table Cell supporta paragrafi, formule, tabelle annidate e immagini. Fill può essere collegato al risultato di una condizione Grasshopper; in assenza di Fill si usa lo sfondo della tabella. Sezioni e importazioni non sono ammesse nelle celle. Il testo delle intestazioni è bianco, quindi scegliere uno sfondo sufficientemente scuro.

## Immagini e viste

Word Image incorpora PNG/JPEG da un file quando Format è vuoto, oppure da Base64 quando Format è png/jpeg. La larghezza è in mm e l'altezza viene calcolata dalle dimensioni originali. Alignment ammette left, center, right. Caption crea una didascalia con SEQ Figure. Alt text viene memorizzato nel disegno.

Capture Rhino View acquisisce la vista attiva o nominata, con risoluzione 64..8192 pixel per lato. Le API Rhino vengono chiamate sul thread dell'interfaccia; la proiezione iniziale viene ripristinata anche in caso di errore. Il risultato viene conservato in memoria e nel file GH. Il trigger disarmato richiede False→True dopo la riapertura. La trasparenza dipende dal display Rhino. La modalità headless viene rifiutata prima di chiamare il renderer nativo. L'esempio 07 include il componente disabilitato e un model.3dm: sbloccarlo in Rhino visibile. Questa acquisizione interattiva resta da collaudare; vedere VERIFICHE.md.

Diagrammi e grafici vengono inseriti come immagini incorporate; non sono grafici Office con dati modificabili.

## Equazioni

Le formule diventano OMML nativo. Non esiste un fallback automatico a immagine. Non aggiungere delimitatori `$...$`. Gli errori riportano la posizione nella formula.

Sintassi implementata:

- caratteri e operatori ordinari, gruppi `{...}`, pedici `_` e apici `^`;
- `\frac{a}{b}`, `\sqrt{x}`, `\sqrt[3]{x}`;
- lettere greche comuni e simboli `\times`, `\cdot`, `\pm`, `\leq`, `\geq`, `\neq`, `\approx`, `\infty`, `\partial`, `\nabla`, `\to`;
- `\sin`, `\cos`, `\tan`, `\log`, `\ln`, `\exp`;
- `\text{testo}`, `\mathrm{testo}`, `\operatorname{testo}`;
- `\sum_{i=1}^{n}{x_i}`, `\prod_{i=1}^{n}{a_i}`, `\int_0^L{q(x)}\,dx`;
- `\begin{matrix}a&b\\c&d\end{matrix}` e le varianti pmatrix/bmatrix.

L'argomento di somme, prodotti e integrali deve essere racchiuso tra graffe se contiene più simboli. Le matrici devono essere rettangolari. Le parentesi normali sono caratteri matematici; pmatrix/bmatrix producono delimitatori strutturati. Macro, pacchetti LaTeX, ambienti diversi e `\left`/`\right` non sono implementati: usare la sintassi documentata, senza assumere compatibilità con LaTeX completo.

Numbered inserisce SEQ Equation. Se viene fornito Bookmark, il segnalibro racchiude il numero e un campo `REF Nome` restituisce la numerazione. Senza Numbered, Bookmark racchiude la formula. Le formule native sono state riconosciute dall'oggetto OMaths di Word nel collaudo.

## Layout e campi

Page Layout controlla dimensioni, margini uniformi, orientamento e intestazioni. Advanced accetta un oggetto PageLayout completo e prevale sugli altri ingressi. Header/Footer null nelle sezioni consentono l'ereditarietà Word; stringa vuota crea una parte vuota esplicita. FirstHeader/FirstFooter attivano la prima pagina diversa; EvenHeader/EvenFooter attivano pari/dispari a livello di documento. PageStart può riavviare la numerazione.

```json
{"WidthMm":210,"HeightMm":297,"MarginMm":20,"Landscape":false,"Header":"Progetto","Footer":"Pagina {{PAGE}} di {{NUMPAGES}}","FirstHeader":"Copertina","FirstFooter":"","EvenHeader":"Pagina pari","EvenFooter":"Pagina {{PAGE}}","PageStart":1}
```

Section chiude la sezione precedente e inizia una nuova pagina con il layout indicato. I blocchi collegati appartengono alla nuova sezione. Page Break inserisce un'interruzione ordinaria.

Word Field crea campi reali, per esempio `TOC \o "1-3" \h \z \u`, `TOC \c "Figure"`, `TOC \c "Table"`, `SEQ Table \* ARABIC`, `REF Nome \h`, `PAGEREF Nome \h`. La generazione inserisce il campo e marca il documento per l'aggiornamento: non calcola da sola pagine, sommari o riferimenti. Aprire Word e aggiornare i campi (F9; anche intestazioni e piè di pagina quando necessario). Non è incluso un renderer o un esportatore PDF nel plugin.

## Template

New Document legge DOCX e DOTX in memoria e produce sempre un nuovo DOCX. L'originale non viene sovrascritto automaticamente. Fill Template accetta un oggetto JSON di sostituzioni testuali e una lista di Template Target.

I placeholder `{{Nome}}` possono essere divisi fra run consecutivi nello stesso paragrafo. La sostituzione eredita la formattazione del primo run interessato, conservando il resto. Si applica a corpo, intestazioni, piè di pagina e note. Per evitare modifiche impreviste un token non attraversa campi, link, tabulazioni, interruzioni o paragrafi.

Strict=True genera un errore per placeholder senza valore o chiavi non trovate. Strict=False lascia invariati i token sconosciuti e ignora chiavi inutilizzate. Tutte le occorrenze dello stesso placeholder vengono sostituite.

Content Control crea controlli a blocchi identificati da Tag. Template Target può riempirli con testo, tabelle, immagini o sequenze. Controlli inline e segnalibri inline accettano un solo paragrafo. Controlli a livello di cella/riga e segnalibri estesi su più paragrafi non sono sostituiti: usare controlli a blocchi. I target strutturati sono cercati nel corpo principale; la sostituzione testuale funziona anche nelle parti secondarie.

Repeat Blocks sostituisce i placeholder nei blocchi per ogni record JSON, rispettando l'ordine record/blocco. Conditional Blocks restituisce una lista vuota quando la condizione è falsa. Non si tratta di un interprete di istruzioni inserite nel template: la logica è nella definizione Grasshopper.

## Importazione selettiva

Import Content seleziona indici zero-based dei blocchi diretti del corpo del DOCX sorgente, esclusa la proprietà finale di sezione. Usa Inspector per individuare gli indici. Il componente acquisisce una copia del file; durante la generazione rimappa gli ID degli stili, le dipendenze degli stili, abstractNum/num, le immagini, le relazioni hyperlink, i segnalibri e i riferimenti REF/PAGEREF supportati.

Sono supportati paragrafi, tabelle, immagini incorporate, link, elenchi ed equazioni. La versione 0.1 rifiuta esplicitamente blocchi con sezioni, note/commenti, altChunk, OLE/grafici incorporati o elenchi con immagini puntate. La selezione deve comprendere per intero gli intervalli dei segnalibri. La fusione arbitraria di documenti Word non è promessa.

## Salvataggio e batch

Save Word scrive sul fronte False→True, non quando il booleano rimane True durante altri ricalcoli. Dopo la riapertura il trigger è disarmato: riportarlo prima a False. Un errore consuma l'impulso; correggere e inviarne uno nuovo.

L'output viene prima costruito e validato in memoria, scritto in un file temporaneo nella stessa cartella e poi rinominato/sostituito. Overwrite=False protegge i file esistenti. Un documento aperto con lock genera un errore senza modificare il file originale.

Save Word Batch richiede gli stessi path nei due Data Tree, un documento e una destinazione per ramo. Controlla conteggi, estensioni DOCX, nomi duplicati ed esistenza dei file prima di scrivere. Ogni documento viene salvato atomicamente, ma il batch completo non è una transazione: gli esiti distinguono i documenti riusciti da quelli falliti.

Open Result è un comando separato e apre soltanto un DOCX esistente nell'applicazione associata. Inspector costruisce e valida in memoria, senza salvare: è un riepilogo strutturale, non un'anteprima delle pagine.

## Input semplificati

### Text Blocks ed editor integrato

Text Blocks riceve Text come lista: un singolo testo multilinea o le righe di un Panel nello stesso ramo. Restituisce una lista ordinata di blocchi nativi. Ogni ramo viene elaborato separatamente; non viene applicato Flatten. Per comporre un solo documento da più sorgenti, unire le liste nell'ordine desiderato e applicare Flatten solo quando è una scelta intenzionale. Elenchi generati da componenti o rami diversi hanno gruppi di numerazione distinti.

Doppio clic sul corpo di Text Blocks/Text Table, oppure menu **Modifica testo…**, apre un editor con ritorni a capo, tabulazioni e barre di scorrimento. **Applica** salva il testo come input persistente e ricalcola; **Annulla** mantiene quello precedente. La modifica partecipa all'Undo Grasshopper. Se l'input ha sorgenti collegate o più di un ramo, l'editor locale è disabilitato: modificare il Panel o il singolo ramo. Nessuna lettura automatica degli appunti e nessun salvataggio DOCX dall'editor.

```text
# Dati del progetto
Il risultato è **verificato**, il commento è *provvisorio*.
Questa seconda riga resta nello stesso paragrafo.

- Trave A
  - Luce 6 m
- Trave B

1. Geometria
2. Risultati

La tensione vale $\sigma=\frac{N}{A}$.

$$M=\frac{qL^2}{8}$$
```

Sintassi implementata:

| Input | Risultato Word |
|---|---|
| Riga vuota | Separa paragrafi; più righe vuote non creano paragrafi vuoti. |
| Righe consecutive | Un paragrafo con interruzioni di riga native. |
| Da `# Titolo` a `######### Titolo` | Heading1…Heading9; spazio dopo # obbligatorio. |
| `**testo**`, `*testo*`, `***testo***` | Grassetto, corsivo, entrambi. Delimitatori chiusi nello stesso paragrafo. |
| `- testo` o `+ testo` | Elenco puntato. |
| `1. testo`, `2. testo` | Elenco numerato. Un nuovo elenco inizia con 1; Word genera i numeri successivi. |
| Due spazi iniziali per livello | Elenchi multilivello, livelli 0…8. Non usare TAB per indentare elenchi. |
| Riga vuota o cambio tipo elenco | Avvia un nuovo gruppo di numerazione. |
| `$formula$` | Formula OMML inline modificabile. |
| `$$formula$$` su una riga | Formula OMML separata. |
| Due righe contenenti solo `$$` | Racchiudono una formula su più righe di input, resa come un'unica formula Word. |
| `---` su una riga | Interruzione di pagina. |
| `\*`, `\$`, `\\`, `\#`, `\[`, `\]` | Simboli letterali. |

Non è un interprete Markdown completo: link, immagini, tabelle, citazioni, blocchi di codice e numerazione delle equazioni usano i componenti dedicati. Le strutture riconoscibili non supportate (per esempio righe che iniziano con `|`, `>`, tre backtick, `![`) vengono rifiutate; altre sequenze non riconosciute restano testo. Asterischi o dollari non chiusi producono errore con numero di riga. Le formule usano lo stesso sottoinsieme LaTeX di Word Equation; nessun fallback a immagine. Le formule non acquisiscono grassetto/corsivo dagli asterischi esterni.

Esempi 01, 02, 04, 08 e 10. I file `testo.txt` inclusi mostrano gli input riproducibili.

### Quick Document

Input: **Title** (facoltativo), **Blocks** (lista), **Layout** (facoltativo), **Template path** (DOCX/DOTX facoltativo), **Styles** (lista facoltativa). Output Document collegabile a Inspector, Fill Fields e Save Word.

Senza template crea un documento vuoto, aggiunge Title con lo stile Word Title se non vuoto, poi Blocks nell'ordine della lista. Con template conserva il documento sorgente e aggiunge titolo/blocchi in fondo a una copia. Layout assente conserva l'impaginazione del template; Styles aggiunge/modifica stili tramite ID. Title non modifica i metadati: usare Document Metadata. Per sostituire punti interni del template usare Fill Fields oppure Fill Template e Template Target. Esempi 01, 03, 10 e 14.

### Fill Fields

Collegare Document; fornire due liste parallele **Names** e **Values** con uguale lunghezza. I nomi possono essere `Progetto` o `{{Progetto}}`, sono sensibili alle maiuscole e non possono essere duplicati dopo la normalizzazione. Nessuna ripetizione implicita di valori. Un valore vuoto sostituisce il token con testo vuoto. Per numeri formattati usare Format Number prima di Values.

Le coppie si aggiungono alle sostituzioni del documento; un nome già impostato viene aggiornato nella copia. Un placeholder ripetuto viene sostituito ovunque; i placeholder divisi fra run mantengono la formattazione del primo run. Strict=True segnala, alla validazione/scrittura, token senza valore e nomi non presenti; Strict=False lascia i token non risolti e ignora i nomi inutilizzati. Questo componente riempie testo; per tabelle/immagini/sequenze in content control usare Fill Template. Esempio 14.

### Paragraph Format

Input diretti: Alignment (`left`, `center`, `right`, `both`; anche alias `justified`), Before/After in punti (0…1000), Line come multiplo (>0 e ≤10), Keep next booleano. Il menu contestuale offre i quattro allineamenti quando Alignment è locale. L'output **Format** è un testo serializzato già compatibile con **Paragraph.Format** e **Word Style.Paragraph format**, senza doverlo scrivere a mano. Per rientri e altre proprietà avanzate resta disponibile il formato JSON completo. Esempi 03 e 20.

### Table Format

Collegare il blocco di Quick Table, Text Table o Word Table a **Table**. Collegare Colour Swatch agli input Header, Stripe e Border, oppure impostarli localmente. Usare colori opachi: Alpha diverso da 255 viene rifiutato. **Padding mm** imposta i margini delle celle (0…50). **Style ID** vuoto conserva lo stile corrente, altrimenti applica l'ID indicato; lo stile deve esistere nel documento. Colori diretti e riempimenti delle singole celle prevalgono sullo stile.

L'output è una nuova tabella: dati, larghezze, allineamenti, unioni e contenuti complessi restano invariati. Il riempimento specifico di una cella resta prioritario. Per le intestazioni il motore sceglie testo bianco o nero secondo il contrasto col riempimento effettivo. Esempi 03 e 06.

### Page Preset

**Format** sceglie A4 (210×297 mm), A3 (297×420), Letter (215,9×279,4), Legal (215,9×355,6); valore iniziale A4. Anche il menu contestuale offre i formati quando l'input è locale. **Landscape** attiva l'orientamento orizzontale; **Margin mm** imposta margini uniformi e deve lasciare area utile. **Header** e **Footer** sono testi; `{{PAGE}}` e `{{NUMPAGES}}` creano campi Word, non valori già calcolati.

Collegare Layout a Quick Document, Compose o Word Section. Usare Page Layout per prima pagina, pari/dispari, ripartenza della numerazione e dimensioni personalizzate. I quattro preset sono pronti come varianti nell'esempio 10; l'esempio 20 usa A4 verticale e orizzontale nella relazione.
