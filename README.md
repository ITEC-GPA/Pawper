# Pawper

Plugin Grasshopper per generare documenti Word modificabili da contenuti parametrici. Il motore crea DOCX senza Microsoft Word installato. La distribuzione comprende il plugin compilato, i sorgenti e 20 esempi completi con definizione Grasshopper e risultato Word.

## Installazione

1. Usare Rhino 8 su Windows a 64 bit, con runtime .NET 8. La build distribuita è compilata e collaudata con Rhino **8.35.26251.13001**. Per altre release Rhino 8 da 8.20 in avanti ricompilare contro le librerie locali; tali release non sono state eseguite in questo ambiente.
2. In Grasshopper aprire **File → Special Folders → Components Folder**.
3. Chiudere Rhino. Creare una cartella `Pawper` nella cartella Components e copiarvi tutti i file di `bin` della root del progetto. Se si usa lo ZIP, copiare l'intera cartella `plugin/Pawper` della distribuzione. Tenere insieme `.gha` e tutte le DLL incluse; non copiare RhinoCommon, Grasshopper o GH_IO.
4. Se Windows blocca i file scaricati, sbloccare l'archivio ZIP nelle sue proprietà prima di estrarlo.
5. Riavviare Rhino e Grasshopper. Comparirà la scheda **Pawper** con 35 componenti e 7 parametri tipizzati.
6. Aprire `examples/01-primo-documento/example.gh`. Impostare **Run** di Save Word su False, poi True. Verrà creato `output.docx` accanto alla definizione.

Non è stata modificata automaticamente l'installazione Grasshopper personale.

Per aggiornare dalla prima versione con il nome provvisorio, rimuovere la precedente cartella del plugin dalla cartella Components prima di copiare Pawper. Le due versioni usano gli stessi identificatori e non devono essere caricate insieme. I 33 componenti, i 7 parametri e l'identificatore della libreria restano compatibili con le definizioni esistenti.

## Icone

Il marchio è un foglio con una zampa. Le icone dei componenti e dei parametri sono disegnate per la griglia Grasshopper di 24 pixel, con simboli distinti per le funzioni. La cartella `assets/icons` contiene i PNG, il logo SVG e l'anteprima `pawper-icons.png`. Le icone sono generate dal codice del plugin, senza dipendenze grafiche aggiuntive.

## Creare una tabella intera

**Quick Table** trasforma un Data Tree in una tabella: collegare i dati, aggiungere Headers se servono e collegare Block a Compose. Con Columns=True ogni ramo diventa una colonna: basta Entwine per unire liste di nomi e risultati. Decimali, unità e cultura hanno input diretti.

**Text Table** crea la tabella da un Panel contenente testo copiato da Excel o CSV. Riconosce i separatori più comuni e conserva codici, celle vuote e testi multilinea tra virgolette. L'output Rows permette di controllare i dati prima del salvataggio.

Aprire l'[esempio 05](examples/05-tabella-numerica/README.md) per provare i quattro casi pronti; la [relazione completa](examples/20-relazione-completa/README.md) usa un Quick Table per ciascuna tabella. Word Table, Table Cell e Table Rows restano disponibili per unioni e contenuti complessi. Dettagli nel [manuale](docs/MANUALE.md#tabelle).

## Esempi

[Indice dei 20 esempi](examples/README.md) · [Matrice di copertura](examples/COVERAGE.md) · [Manuale](docs/MANUALE.md) · [Verifiche e limiti](docs/VERIFICHE.md).

Ogni cartella contiene `example.gh`, la guida passo passo con valori degli input e collegamenti, la specifica `document.json`, `expected.docx` e il risultato generato dai componenti `grasshopper-verified.docx`. Conservare la cartella condivisa `examples/assets`: i collegamenti sono relativi.

## Compilazione

Servono Windows, Rhino 8 installato e SDK .NET 8 (feature band 8.0.300 o successiva). Il percorso Rhino predefinito è `C:\Program Files\Rhino 8`; può essere modificato con la proprietà MSBuild `RhinoInstall`.

```powershell
./build.ps1
# Facoltativo: rigenera gli esempi ed esegue le verifiche del motore:
./build.ps1 -Verify
# Include generazione, riapertura e ricalcolo dei 20 file GH in Rhino.Inside:
./build.ps1 -Verify -Definitions
```

Gli strumenti di sviluppo non vengono installati nel plugin. `tools/Pawper.Verify` è un eseguibile di test senza Rhino né Word; `tools/Pawper.Definitions` usa Rhino.Inside e la licenza Rhino locale. Il test host usa un registro isolato dei componenti per evitare il caricamento degli altri plugin installati. Tale isolamento usa API interne esclusivamente nel generatore, non nel plugin distribuito.

`build.ps1` è il comando di compilazione e consegna del progetto: compila l'intera soluzione esclusivamente in **Release** in una cartella temporanea, poi pubblica direttamente nella cartella **`bin` della root** questi cinque file:

- `Pawper.Grasshopper.gha`
- `Pawper.Core.dll`
- `DocumentFormat.OpenXml.dll`
- `DocumentFormat.OpenXml.Framework.dll`
- `System.IO.Packaging.dll`

In `bin` non vengono lasciati eseguibili di test, PDB, sottocartelle Debug/Release o file intermedi. Dopo una compilazione riuscita vengono eliminate le cartelle `bin`/`obj` dei singoli progetti, gli output intermedi della root e `dist` con i vecchi pacchetti. La cartella temporanea viene rimossa anche in caso di errore; se compilazione o verifiche falliscono, la precedente `bin` viene conservata. Sorgenti, esempi, icone e documentazione restano disponibili.

Per ottenere questo comportamento usare sempre `./build.ps1`: una chiamata diretta a `dotnet build` o il comando Build dell'IDE non esegue la fase di raccolta e pulizia. I percorsi sono relativi al progetto e non dipendono dalla cartella in cui è salvato.

`./package.ps1` crea facoltativamente uno ZIP in `dist` leggendo la Release da `bin`, senza mantenere una seconda cartella con copie degli assembly. Il pacchetto viene eliminato dalla successiva compilazione, per evitare di distribuire una versione obsoleta.

## Architettura

- `Pawper.Core`: record immutabili, composizione, parser LaTeX, generatore Open XML, template, validazione e salvataggio atomico.
- `Pawper.Grasshopper`: wrapper persistenti, parametri, componenti e cattura delle viste Rhino.
- `Pawper.Verify`: generazione degli esempi e test dei comportamenti critici.
- `Pawper.Definitions`: costruzione dei grafi con componenti reali, salvataggio binario `.gh`, riapertura e ricalcolo.

Ogni documento è una specifica immutabile. Comporre, duplicare e ramificare la definizione non modifica gli altri rami. La lettura di un template acquisisce una copia in memoria. Solo Save Word e Save Word Batch scrivono DOCX; Open Result richiede un impulso separato.

## Dipendenze

Open XML SDK 3.3.0, Open XML Framework 3.3.0 e System.IO.Packaging 8.0.1, licenza MIT. RhinoCommon/Grasshopper/GH_IO provengono dall'installazione Rhino e non sono redistribuiti. Rhino.Inside 8.0.7-beta serve esclusivamente al generatore di esempi. Vedere [licenze](THIRD-PARTY-NOTICES.md).

Compatibilità runtime verificata sulla base della [documentazione McNeel](https://developer.rhino3d.com/guides/rhinocommon/moving-to-dotnet-core/). Struttura Word e validazione basate su [Open XML SDK](https://github.com/dotnet/Open-XML-SDK).
