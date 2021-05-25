# space requirment

| Line per file | total file | 
|---------------|------------|
| 100 000       | 100        |

29 x 100 000 x 100 = 290 000 000 bytes = 276.57 mb
<table>
<tr><th> Memmory per struct </th><th> Total memory usage per 100 000 x 100 lines </th></tr>
<tr><td>
  
| Per line      | type | bytes | 
|---------------|------|-------|
| TimeStamp     | long |  8    | 
| MsdId         | int  |  4    |
| StackId       | int  |  4    |
| TagID         | int  |  4    |
| FileIndex     | int  |  4    |
| LineNumber    | int  |  4    |
| LogType       | byte |  1    |
| Total         |      |  29   |
</td><td>


| IndexName     | type     | bytes           | Remar        | 
|---------------|------    |-----------      |-----------   |
| GlobalLogLine | int[]    | 38.147          | Single index |
| indexMessages | int[][]  | 38.147 + 38.147 | Double index |
| indexTags     | int[][]  | 38.147 + 38.147 | Double index |
| _indexLogType | int[][]  | 38.147 + 38.147 | Double index |
|  per line st  | struct   | 276.57          | strcut mem   |
|  Total        |          | 619.89 mb       | |


  
</td></tr> </table>
