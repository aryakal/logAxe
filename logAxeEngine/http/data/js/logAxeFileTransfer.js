class LogAxeFileUploader {

    constructor(appRoot, ctlId) {

        this._container = document.getElementById(ctlId);
        this._appRoot = appRoot + "files";
        this._container.addEventListener('dragover', this.set_drag_event.bind(this), false);
        this._container.addEventListener('dragenter', this.set_drag_event.bind(this), false);
        this._container.addEventListener('drop', this.set_drop_event.bind(this), false);
        this._debug_enabled = true;
        this._files = [];
        this._ndx_upload = -1;
        this._files_size = 0
    }

    ldgb(msg) {
        if (this._debug_enabled) {
            console.log("lf, ",
                this._ndx_upload + " of " + this._files.length + " [ " + this._files_size + " ] ",
                msg);
        }
    }

    set_drag_event(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    set_drop_event(e) {
        this.set_drag_event(e);
        if (e.dataTransfer && e.dataTransfer.files.length) {
            // var files = e.dataTransfer.files;
            // for (let ndx = 0; ndx < files.length; ndx++) {
            //     var f = files[ndx];
            //     this.set_uploadFile(f);
            // }            
            this._files = e.dataTransfer.files;
            this._ndx_upload = -1;
            this._files_size = this.get_total_fsize();
            this.set_upload();
        }
    }

    set_upload() {
        var obj = this;
        setTimeout(function() {
            if (obj._ndx_upload == -1) {
                // obj.ldgb("Total files: " + files.length + ", total file size: " + obj.get_total_fsize(obj._files));
                // obj.ldgb("" + (obj._ndx_upload + 1) + " of " + obj._files.length + "upload start");

            }
            obj._ndx_upload++;
            if (obj._ndx_upload < obj._files.length) {
                obj.set_uploadFile(obj._files[obj._ndx_upload]);
            } else {
                var url = obj._appRoot + "?op=process";
                return $.getJSON(url, function(result) {
                    console.log(result);
                });

            }

        }, 0);
    }

    set_uploadFile(f) {
        var reader = new FileReader();
        var obj = this;
        reader.onload = function(event) {
            var url = obj._appRoot + "?op=addFile" + "&n=" + f.name;
            if (event.target.readyState !== FileReader.DONE) {
                return;
            }
            $.ajax({
                type: 'POST',
                url: url,
                data: reader.result,
                success: function(response) {
                    obj.ldgb(response);
                    obj.set_upload();
                },
                error: function(response) {
                    obj.ldgb(response);
                    obj.set_upload();
                },
            });
        };
        reader.readAsDataURL(f);
    }

    get_total_fsize() {
        var fsize = 0;
        for (let ndx = 0; ndx < this._files.length; ndx++) {
            fsize += this._files[ndx].size;
        }
        return fsize;
    }
}