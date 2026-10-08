# Verifiche della versione 0.1

Collaudo eseguito l'8 ottobre 2026 su Windows, Rhino 8.35.26251.13001 e .NET 8. Build della soluzione senza errori e senza avvisi. Il plugin distribuito non richiede Word per generare i documenti.

Il progetto è stato rinominato Pawper: soluzione, progetti, namespace, assembly, cartelle, documentazione e contenuti dimostrativi. Gli identificatori persistenti della prima versione sono conservati. Il rapporto `examples/branding-verification.json` registra la verifica delle icone e la riapertura delle definizioni precedenti.

- **42 icone 24 × 24 verificate** sui componenti e parametri reali, più il marchio della libreria; esportate in PNG e ispezionate visivamente. Incluso il logo SVG.
- **20 definizioni della versione precedente riaperte con Pawper**, senza componenti mancanti. Conservati tutti i 41 identificatori persistenti di componenti, parametri e libreria.

## Tabelle intere

Quick Table crea una tabella da righe o colonne del Data Tree; Text Table crea una tabella da testo Excel/CSV. Entrambi sono inclusi nell'esempio 05, mentre la relazione 20 usa un Quick Table per ciascuna delle due tabelle. Il confronto con la definizione precedente mostra **50 → 26 componenti** nell'intera relazione, senza rimuoverne i contenuti. Gli identificatori dei componenti precedenti non sono cambiati; sono stati aggiunti due nuovi identificatori e due icone.

Aggiunti 29 controlli del motore: orientamento righe/colonne, rami vuoti/irregolari, celle null, precisione, unità per colonna, cultura indipendente dal computer, input non mutati, codici testuali, TSV/CSV con virgolette e ritorni a capo, separatori espliciti/automatici, errori di formato e tabelle Open XML native. In Grasshopper verificati anche i messaggi per Strict e CSV malformato, il ripristino degli input, l'ordine dei blocchi con path sparsi e la conservazione dei valori dopo riapertura.

Il generatore isolato registra ora anche i tipi standard del SDK Grasshopper, necessari per deserializzare GH_Number/GH_String negli ingressi generici persistenti. Questo riguarda solo l'host di test; il plugin usa il normale ambiente Grasshopper. Gli esempi restano esattamente 20 e la matrice è rigenerata automaticamente.

## Risultati automatici

- **68 controlli del motore superati, zero fallimenti**: 20 esempi, serializzazione dei dati, isolamento dei rami, ricalcolo, trigger e disarmo dopo riapertura, placeholder spezzati e ripetuti, conservazione della formattazione e delle intestazioni, OMML, immagini, note/commenti, rifiuto di input non validi, collisioni batch, sovrascrittura e file bloccati.
- **20 definizioni Grasshopper salvate, riaperte e ricalcolate** con zero errori Open XML. Il contenuto generato prima e dopo la riapertura è stato confrontato e conservato.
- **Salvataggio dai componenti reali**: Save Word crea il file al fronte False→True e non lo riscrive lasciando True; Save Word Batch produce correttamente tre documenti da rami corrispondenti.
- **35 componenti pubblici coperti** dagli esempi. La matrice include le modalità principali, come stili carattere/tabella, formule inline/display, matrici, template DOCX/DOTX e sezioni.
- **49 DOCX aperti ed esportati con Microsoft Word in sola lettura**, includendo i risultati del motore, quelli delle definizioni, i batch, i template DOCX e un output preesistente della relazione 20. Nessun errore nella sessione finale di verifica. Word riconosce le formule tramite OMaths come equazioni native modificabili.
- **63 pagine controllate visivamente** dopo l'esportazione Word: nessun contenuto tagliato, sovrapposto o fuori pagina. Controllati testo, tabelle, immagini, formule, sezioni, intestazioni, piè di pagina e campi aggiornati. I DOCX distribuiti conservano i campi originali; l'aggiornamento effettuato per questa verifica non è stato salvato nei file sorgenti.

I rapporti riproducibili sono `examples/verification.json`, `examples/grasshopper-verification.json`, `examples/word-verification.json` e `examples/capture-verification.json`. I PDF e PNG di revisione sono intermedi QA e non fanno parte della distribuzione. Il renderer LibreOffice non è disponibile (soffice.exe assente); per questa verifica sono stati usati Microsoft Word e Poppler.

## Cattura delle viste

Il componente è compilato contro RhinoCommon e include acquisizione della vista attiva/nominata, risoluzione, trasparenza, conservazione del risultato e ripristino della proiezione. Il test del renderer in un host Rhino nascosto ha rilevato un accesso non valido nella libreria grafica nativa di Rhino. Il plugin ora rifiuta esplicitamente la cattura in modalità headless prima di chiamare quelle API: questa protezione è stata verificata.

**L'acquisizione in una finestra Rhino visibile resta da collaudare manualmente.** Nell'esempio 07 Capture è disabilitato; è incluso `model.3dm` per la prova. Aprirlo in Rhino, sbloccare il componente e inviare False→True a Capture. Per la modalità nominata salvare prima una vista e inserirne il nome.

## Limiti espliciti della versione

- LaTeX limitato al sottoinsieme documentato; nessuna conversione silenziosa a immagine.
- Segnalibri di template sostituibili inline nello stesso paragrafo; per sequenze di blocchi usare content control.
- Importazione selettiva di paragrafi, tabelle, immagini, link, numerazioni ed equazioni. Sezioni, note/commenti da altri documenti, OLE, altChunk e grafici Office incorporati vengono rifiutati con un errore.
- Nessun esportatore PDF o renderer di pagine incluso nel plugin. I campi di sommario, numerazione e riferimento richiedono l'aggiornamento del motore Word.
- Build binaria verificata su Rhino 8.35. Per altre release Rhino 8 compatibili con .NET 8 ricompilare e collaudare nell'installazione di destinazione.

Questi limiti sono esposti nel manuale e nei messaggi dei componenti; non sono funzionalità dichiarate complete tramite stub.
