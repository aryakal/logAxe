class LogAxeFileExplorer {

    constructor(containerNameId, urlLocId, favPaneId, lstPaneId, favSelId, appRoot) {

        this._container = document.getElementById(containerNameId);
        this._urlLoc = document.getElementById(urlLocId);
        this._favPane = document.getElementById(favPaneId);
        this._lstPane = document.getElementById(lstPaneId);
        this._selPane = document.getElementById(favSelId)
        this._urlLoc.addEventListener('keyup', this.set_url_on_input_event.bind(this), false);
        this._favPane.innerHTML = "";
        this._favData = null;
        this._lstData = null;
        this._currentSelected = {}
        var obj = this;
        this._backend = new LogAxeBackend(appRoot);

        this._backend.register_event("get_fav_items", function(result) {
            obj.render_fav(result);
        });

        this._backend.register_event("get_dir_items", function(result) {
            obj.render_dir(result);
        });
    }

    render_dir(result) {
        var obj = this;
        this._lstData = result;
        obj.removeAllChildNodes(obj._lstPane);
        for (var ndx in result.files) {
            var info = result.files[ndx];
            var div = document.createElement("div");
            var text = document.createTextNode("F | " + info.n);
            div.appendChild(text);
            obj._lstPane.appendChild(div);
            div.addEventListener('click', obj.set_on_click.bind(obj, false, false, ndx), false);
        }

        for (var ndx in result.dirs) {
            var info = result.dirs[ndx];
            var div = document.createElement("div");
            div.classList.add("feDir1");
            var text = document.createTextNode("D | " + info.n);
            div.appendChild(text);
            obj._lstPane.appendChild(div);
            div.addEventListener('click', obj.set_on_click.bind(obj, false, true, ndx), false);
        }
    }

    render_fav(result) {
        var obj = this;
        this._favData = result;
        for (var ndx in result.dirs) {
            var info = result.dirs[ndx];
            var div = document.createElement("div");
            div.classList.add("feDir");
            var text = document.createTextNode(info.n);
            div.appendChild(text);
            obj._favPane.appendChild(div);
            div.addEventListener('click', obj.set_on_click.bind(obj, true, true, ndx), false);
            //favPaneContent += "&nbsp;[D]&nbsp;" + dirInfo.n + "<br>"
        }
    }

    get_fav() {
        this._backend.get_fav_items();
    }

    get_dir(path) {
        this._backend.get_dir_items(path);
    }

    set_on_click(isFav, isDir, ndx) {
        if (isFav) {
            if (isDir) {
                var info = this._favData.dirs[ndx];
                this._urlLoc.value = info.p;
                this.get_dir(this._urlLoc.value);
            }

        } else {
            if (isDir) {
                var info = this._lstData.dirs[ndx];
                this._urlLoc.value = info.p;
                this.get_dir(this._urlLoc.value);
            } else {
                var info = this._lstData.files[ndx];
                console.log(info);
                this._currentSelected[info.p] = info;
                console.log(this._currentSelected)

                this.removeAllChildNodes(this._selPane);
                for (var ndx in this._currentSelected) {
                    info = this._currentSelected[ndx];
                    var div = document.createElement("div");
                    var text = document.createTextNode("F | " + info.n);
                    div.appendChild(text);
                    this._selPane.appendChild(div);

                }

                //div.addEventListener('click', obj.set_on_click.bind(obj, false, false, ndx), false);
            }
        }
    }

    set_url_on_input_event(e) {
        if (e.key === "Enter") {
            this.get_dir(this._urlLoc.value);
        }
    }

    set_hide() {
    }

    removeAllChildNodes(parent) {
        if (parent.firstChild != null)
            while (parent.firstChild) {
                parent.removeChild(parent.firstChild);
            }
    }
}

class LogAxeBackend {
    constructor(appRoot) {
        this._appRoot = appRoot;
        this._appRoot_browser = appRoot + "fileBrowser";
        this._callbacks = {}
    }

    register_event(event_name, funct_ptr) {
        this._callbacks[event_name] = funct_ptr;
    }

    get_dir_items(path) {
        var url = this._appRoot_browser + "?op=lst&path=" + path;
        this._get_json_data(url, this._callbacks["get_dir_items"])
    }

    get_fav_items(path) {
        var url = this._appRoot_browser + "?op=fav"
        this._get_json_data(url, this._callbacks["get_fav_items"])
    }

    _get_json_data(url, callback) {
        if (null == callback)
            return;
        console.log(url)
        $.getJSON(url, function(result) {
            callback(result)
        });
    }
}

// class LogAxeFilePlugin {
//     constructor(appRoot, onPathData, onFavData) {
//         this._appRoot = appRoot + "fileBrowser";
//         this._currentSelected = {}
//         this._onPathData = onPathData
//         this._onFavData = onFavData
//     }

//     get_dir_items(path) {
//         var obj = this;
//         var url = this._appRoot + "?op=lst&path=" + path;
//         if (this._onPathData != null) {
//             $.getJSON(url, function(result) {
//                 obj._onPathData(result)
//             });
//         }
//     }

//     get_fav_items() {
//         var obj = this;
//         var url = this._appRoot + "?op=fav"
//         if (this._onFavData != null) {
//             $.getJSON(url, function(result) {
//                 obj._onFavData(result)
//             });
//         }
//     }
// }
