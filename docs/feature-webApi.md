# [logAxe](../README.md)

| method | url | remark |
|----------|-|-|
| get| /logAxe/ or / | redirects to the main /logAxe/ui/mainUI.html
| get| /logAxe/ui/mainUI.html | gets the main ui.
| get| /logAxe/ui/<b>filepath</b> | gets any file from the application resource folder.
| get| /logAxe/vw?op=createNew | creates a new view and gives the id back.
| get| /logAxe/vw?op=info&n=view1 | gets the current info
| get| /logAxe/vw?op=line&n=view1&s=0&l=10| gets the loglin(e) from the view.
| get| /logAxe/vw?op=filter&n=view1 | gets current filter info
| post| /logAxe/vw?op=filter&n=view1 | sets the current filter.
| post| /logAxe/openView | Opens a new window with without any filter.

#  Files currently loaded.


| method | url | remark |
|----------|-|-|
| post| /logAxe/files?op=addFile | { paths: [file path as list] } | upload the file path to server.
| post| /logAxe/files?op=lst | { paths: [ {n: name, p: path, pl: plugin, l: load status} ] } | upload the file path to server.

# file browser

| method | url | request |  response |
|----------|-|-|-|
| post| /logAxe/fileBrowser |  { op: lst, path: <b>Path of the folder</b> } | { op:lst, paths: [{ p: path, n: name, t: d/f }] }
| post| /logAxe/fileBrowser |  { op: fav } | { op:fav, paths: [{ n: name}] }
| post| /logAxe/fileBrowser |  { op: addFav, n: name, p: path } | { op:fav}

#  ~~not implemented~~

| method | url | remark |
|----------|-|-|
| get| /logAxe/config | gets the current configuration
| post| /logAxe/config | sets the current configuration



