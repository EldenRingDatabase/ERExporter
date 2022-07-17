# ERExporter

tl;dr needed a CLI-based Yapped CSV exporter.

A small wrapper around SoulsFormats provinding a CLI interface for:

* Decrypting `regulation.bin` into `param/gameparam/` where all `.param`s are placed.
* Unpacking `.param` files into `.csv` in the same directory.
* Likely do something with MSBs and TAEs in the future.

Shamelessly copies Yabber, just drag and drop files onto the exe.

# IMPORTANT NOTE

This requires paramdefs for unpacking `.param` files, make sure `Defs` and `Names` folders are next to the exe. There are stolen versions from [Yapped Rune Bear](https://github.com/vawser/Yapped-Rune-Bear/tree/main/Paramdex/ER) included with release.