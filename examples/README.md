# Venti esempi Pawper

Gli esempi procedono dalle funzioni di base alla relazione completa. Ogni cartella contiene la definizione binaria Grasshopper, la guida con input e collegamenti, i file Word e la specifica dei dati.

| N. | Esempio | Obiettivo |
|---|---|---|
| 01 | [Primo documento](01-primo-documento/README.md) | Creare, comporre e salvare un documento senza Word installato. |
| 02 | [Testo formattato](02-testo-formattato/README.md) | Combinare frammenti con proprietà tipografiche diverse. |
| 03 | [Stili e titoli](03-stili-e-titoli/README.md) | Usare identificatori indipendenti dalla lingua di Word e tutti i livelli di titolo. |
| 04 | [Elenchi multilivello](04-elenchi/README.md) | Numerare voci, annidare livelli, usare punti e riavviare un elenco. |
| 05 | [Tabelle intere da dati e testo](05-tabella-numerica/README.md) | Creare tabelle con un solo componente da righe, colonne, testo Excel e CSV. |
| 06 | [Celle e unioni](06-celle-e-unioni/README.md) | Unire righe e colonne e inserire contenuti formattati nelle celle. |
| 07 | [Immagini e viste Rhino](07-immagini-e-viste/README.md) | Incorporare PNG e JPEG, mantenere le proporzioni e acquisire viste attive o nominate. |
| 08 | [Equazioni modificabili](08-equazioni-base/README.md) | Scrivere formule inline e separate con frazioni, radici, indici ed esponenti. |
| 09 | [Equazioni avanzate](09-equazioni-avanzate/README.md) | Sommatorie, integrali, prodotti, matrici e riferimenti alle equazioni. |
| 10 | [Pagine e sezioni](10-pagine-e-sezioni/README.md) | Gestire pagina iniziale, pari/dispari, numeri pagina e orientamenti diversi. |
| 11 | [Sommari e didascalie](11-sommari-e-didascalie/README.md) | Creare campi Word per sommario ed elenchi figure/tabelle, da aggiornare in Word. |
| 12 | [Link e segnalibri](12-link-e-segnalibri/README.md) | Collegamenti esterni e interni e riferimenti al testo o alla pagina. |
| 13 | [Metadati](13-metadati/README.md) | Impostare proprietà standard e personalizzate e ispezionare il documento. |
| 14 | [Template con placeholder](14-template-testuale/README.md) | Sostituire token divisi tra run nel corpo, nelle intestazioni e nei piè di pagina. |
| 15 | [Content control e segnalibri](15-template-strutturato/README.md) | Riempire un DOTX con blocchi diversi e sostituire un segnalibro inline. |
| 16 | [Ripetizioni e condizioni](16-ripetizioni-e-condizioni/README.md) | Ripetere contenuti per record e includere sezioni solo quando richiesto. |
| 17 | [Note e commenti](17-note-e-commenti/README.md) | Note a piè di pagina, note finali e commenti modificabili. |
| 18 | [Importazione selettiva](18-importazione/README.md) | Importare due volte gli stessi blocchi rimappando stili, numerazioni, risorse e segnalibri. |
| 19 | [Generazione batch](19-batch/README.md) | Un documento per ramo del Data Tree, con percorsi distinti e controllo delle collisioni. |
| 20 | [Relazione tecnica completa](20-relazione-completa/README.md) | Combinare copertina, sommario, dati, immagini, equazioni, note e sezioni. |

[Matrice dei componenti e delle funzionalità](COVERAGE.md).

Gli input Run sono salvati su False. La riapertura disarma i trigger: inviare False→True. Gli esempi sono portabili se si conserva la cartella assets. Capture Rhino View richiede una sessione con viste disponibili; il componente è incluso disabilitato nel caso 07.

Verifiche automatiche: verification.json (motore e casi di errore), grasshopper-verification.json (salvataggio, riapertura e contenuti dei file GH). Vedere anche il rapporto generale in ../docs/VERIFICHE.md.
