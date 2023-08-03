# StreamingAsset

This class is used to manage a collection of file paths to streaming assets.

## Properties
- `streamingAssetFolder`: The folder containing the streaming assets.
- `extension`: The file extension of the streaming assets.
- `filePaths`: An array of file paths relative to the streaming asset folder.

## Methods

### `BuildDataStore()`
Builds the data store by appending the streaming asset folder path to each file path. Returns the built data store.

### `GetCurrentFileUrl()`
Returns the file name without extension of the currently selected file.

### `GetFirstFileUrl()`
Returns the URL of the first file in the data store. If the data store is empty, returns an empty string.

### `GetRandomFileUrl()`
Returns the URL of a random file in the data store. If the data store is empty, returns an empty string.

### `GetNextFileUrl()`
Returns the URL of the next file in the data store. If the data store is empty, returns an empty string. If the last file is reached, wraps around to the first file.

### `GetPrevFileUrl()`
Returns the URL of the previous file in the data store. If the data store is empty, returns an empty string. If the first file is reached, wraps around to the last file.
