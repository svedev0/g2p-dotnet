# g2p-dotnet

A grapheme to phoneme (G2P) tool for phonemicizing text. The phonemicized text
tokens can be used for Mel spectrogram generation, the input for speech
synthesis voice transformer models.

The phoneme mapping is slightly tuned for Brittish English pronunciation but
can also easily be used for US and international English as is.

This tool does not handle pre-tokenization and tokenization because the exact
implementation required may vary from model to model.

### Example (text to IPA phonemes):

```
Input:  the quick brown fox
Output: ðə kwɪk braʊn fɒks
```
