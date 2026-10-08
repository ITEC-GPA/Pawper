# Icone Pawper

Marchio originale: foglio piegato e impronta di zampa. Verde petrolio `#144E4A`, carta avorio `#FFFDF5`, piega verde chiaro `#D3E8DC` e accento terracotta `#DD7442`.

- `pawper.svg`: sorgente vettoriale del marchio, sfondo trasparente.
- `pawper-24.png`, `pawper-128.png`, `pawper-256.png`: marchio raster alle dimensioni indicate.
- PNG dei 35 componenti e dei 7 parametri: esportazioni delle icone effettivamente usate dal plugin, 24 × 24 pixel.
- `pawper-icons.png`: tavola delle icone con nomi e anteprima alla dimensione effettiva.

Il disegno delle icone è implementato in `src/Pawper.Grasshopper/PawperIcons.cs`. Il generatore `Pawper.Definitions` esporta e controlla tutte le icone e aggiorna la tavola. Il caricamento del plugin registra il marchio nella scheda Pawper di Grasshopper.
