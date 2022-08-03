# [logAxe](../README.md)

# Storing strings in Memory

Csharp strings take 2 bytes to store the data in memory. So for 1 Gb of the data it will take 2gb of space.

In logAxe implements two type of string db (IStorageString)
* StorageStringList - Used to collect the strings while it is parsed
* StorageStringDB - Used to collect stings after optimized.
* StorageStringXEfficient - stores in a byte array with a record.


### StorageStringList

Features
* Stores the string int a string list
* Does not store string empty/null string.
* Is made to be fast but not efficent in terms of memory.


### StorageStringDB

Features
* Uses StorageStringXEfficient and StringGhost
* Ti
