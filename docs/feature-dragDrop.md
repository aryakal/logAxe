# [logAxe](../README.md)

# Drag and drop of Files

In logAxe application user can drag and drop 
* text/log files
* zip files containing text/log files
* folders containing zip files or log files.

### Internal mechanism 

Internally the files are added to the logAxeEngine using the interface IFileObject.
IFileObject has three implementation
* RegularFile, to add files from disk as regular file.
* WebFile, this features allows the file to be created in memory and assign arbitary file.
* CompressedFile, The file extracted from zip file is marked as Compressed File 
* BadFile, used for testing.

This way it allows the engine to be abstracted from the files being added to the system.
Once the files have been enumerated, they are checked with plugin(s) to find which plugin can parse the log file from native format to logAxe logging format/structure. 

