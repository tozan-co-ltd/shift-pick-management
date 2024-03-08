$(document).ready(function () {
    // #sidebarToggleがクリックされたときに、サイドバーの表示/非表示の状態をトグルし、クッキーに保存する
    $("#sidebarToggle").click(function () {
        // Sidebar クッキーを削除し、パスをルートに設定
        $.removeCookie("Sidebar", { path: '/' });

        // body 要素に sb-sidenav-toggled クラスをトグルする
        $("body").toggleClass("sb-sidenav-toggled");

        // #sidebarToggle 要素に rotateBtn クラスをトグルする
        $("#sidebarToggle").toggleClass("rotateBtn");

        // body 要素のクラスを取得
        let $className = $("body").attr("class");

        // Sidebar クッキーに body 要素のクラスを保存し、パスをルートに設定
        $.cookie("Sidebar", $className, { path: '/' });
    });

    //------------------- DataTables ------------------//
    // 日本語表示
    const language_url = "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json";

    // ID(2列目)昇順
    $('.datatable-normal').DataTable({
        "language": {
            "url": language_url
        },
        lengthChange: true,     // 件数切替
        info: false,            // 総件数
        scrollX: true,          // 横スクロール可
        scrollCollapse: true,   // 縦スクロール表示
        order: [[1, "asc"]],    // ID昇順
        columnDefs: [
            { targets: 0, sortable: false },    // インデックス0列(アイコン列)のソート禁止
        ]
    });

    // 作成日時(1列目)降順
    $('.datatable-createdat-desc').DataTable({
        "language": {
            "url": language_url
        },
        lengthChange: true,     // 件数切替
        info: false,            // 総件数
        scrollX: true,          // 横スクロール可
        scrollCollapse: true,   // 縦スクロール表示
        order: [[0, "desc"]],   // 作成日時降順
    });

    // ハンディエラーメッセージ用(作成日時(2列目)降順,縦スクロールあり,件数非表示,検索非表示)
    $('.datatable-handyErrorMessage').DataTable({
        "language": {
            "url": language_url
        },
        lengthChange: false,    // 件数切替非表示
        info: false,            // 総件数非表示
        searching: false,       // 検索欄非表示
        paging: false,          // ページング非表示
        scrollCollapse: true,   // 縦スクロール表示
        scrollY: '200px',       // 縦スクロールサイズ
        order: [[1, "desc"]],   // 作成日時降順
        columnDefs: [
            { targets: 0, sortable: false },    // インデックス0列(アイコン列)のソート禁止
        ]
    });

    // 出荷実績照会用(仕入先品番(10列目)昇順)
    $('.datatable-shipment').DataTable({
        "language": {
            "url": language_url
        },
        lengthChange: true,     // 件数切替
        info: false,            // 総件数
        scrollX: true,          // 横スクロール可
        scrollCollapse: true,   // 縦スクロール表示
        order: [[9, "asc"]],    // 仕入先品番日時昇順
        columnDefs: [
            { targets: 0, sortable: false },    // インデックス0列(アイコン列)のソート禁止
        ]
    });
    //--------------------------------------------------------//

    // ページ上のすべてのファイル入力にfileselectイベント付与
    $(document).on("change", ":file", function () {
        var input = $(this),
            numFiles = input.get(0).files ? input.get(0).files.length : 1,
            label = input
                .val()
                .replace(/\\/g, "/")
                .replace(/.*\//, "");
        input.trigger("fileselect", [numFiles, label]);
    });

    // fileselectイベント監視
    $(":file").on("fileselect", function (event, numFiles, label) {
        var input = $(this)
            .parents(".input-group")
            .find(":text"),
            log = numFiles > 1 ? numFiles + " files selected" : label;

        if (input.length) {
            input.val(log);
        } else {
            if (log) alert(log);
        }
    });

    // タイムピッカー
    $.datetimepicker.setLocale('ja');
    $('.pickerDate').datetimepicker({
        format: "Y/m/d",
        timepicker: false,
        onShow: function (ct) {
            this.setOptions({
                maxDate: jQuery("#end_datetimepicker").val() ? jQuery("#end_datetimepicker").val() : false,
                formatDate: "Y/m/d"
            })
        }
    });
});
//--------------------------------------------------------//


//------------------- CSV取込 ------------------//
function onUploadFile(page) {

    // ページの更新を禁止する
    event.preventDefault();

    $('#div-error-message').empty;

    var formData = new FormData(document.querySelector('#' + page + ''));

    var fileUpload = document.getElementById('UploadFileList');
    if (fileUpload.files.length <= 0) {
        $('#div-error-message').text('ファイルが選択されていません。');
        $("#div-error-message").show();
        return false;
    }

    var IsFirst = true;
    for (var file of formData) {
        if (IsFirst) {
            if (file[1]["size"] <= 0) {
                $('#div-error-message').text('ファイルが選択されていません。');
                $("#div-error-message").show();
                return false;
            }
            else {
                $("#div-error-message").hide();
                IsFirst = false;
            }
        }

    }

    const dialog = document.getElementById("import-modal");
    if (dialog) {
        dialog.parentNode.removeChild(dialog);
    }

    $('body').append(
        '<div class="modal fade" id="import-modal" tabindex="-1" role="dialog" aria-labelledby="importModalCenterTitle" aria-hidden="true">' +
        '    <div class="modal-dialog modal-dialog-centered" role="document">' +
        '        <div class="modal-content">' +
        '            <div class="modal-header">' +
        '                <h5 class="modal-title" id="importModalCenterTitle">取込</h5>' +
        '                <button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '                    <span aria-hidden="true">&times;</span>' +
        '                </button>' +
        '            </div>' +
        '            <div class="modal-body">' +
        '                <p>ファイル取込を行います。よろしいですか？</p > ' +
        '            </div>' +
        '            <div class="modal-footer">' +
        '                <button type="button" class="btn btn-secondary cancel" data-dismiss="modal">キャンセル</button>' +
        '                <button type="button" class="btn btn-primary ok">OK</button>' +
        '            </div>' +
        '        </div>' +
        '    </div>' +
        '</div>'
    );

    $('#import-modal').modal('show');

    $('#import-modal .cancel, #import-modal .close').click(function () {
        $('#import-modal').modal('hide');
        return false;
    });

    $('#import-modal .ok').click(function () {
        $('#import-modal').modal('hide');

        var importUrl = document.getElementById('import_action_url').value;

        showLoading();
        $.ajax({
            url: importUrl,
            method: 'post',
            data: formData,
            processData: false,
            contentType: false
        }).done(function (response) {
            hideLoading();
            AlertMessage('', '取込', '登録が完了しました。', null, null);
        }).fail(function (jqXHR, textStatus, errorThrown) {
            hideLoading();
            if (jqXHR.status === 404) {
                // データが見つからなかった場合
                var errorMessage = jqXHR.responseJSON.errorMessage;
                $("#div-error-message").html(errorMessage);
            } else {
                // その他のエラーの場合
                var errorMessage = 'E3002: サーバーに接続できませんでした。' + ' HttpRequest : ' + jqXHR.status + ' textStatus : ' + textStatus;
                $("#div-error-message").html(errorMessage);
            }
            $("#div-error-message").show();
            $('#' + page + '')[0].reset();
        });
    });
}

function showLoading() {
    $("#file-upload").addClass('btn-disable')
    $("#file-upload .text").text("取込中")
    $("#file-upload i").removeClass('fa-solid fa-file-export')
    $("#file-upload i").addClass('fas fa-spinner fa-pulse')
}

function hideLoading() {
    $("#file-upload").removeClass('btn-disable')
    $("#file-upload .text").text("取込")
    $("#file-upload i").addClass('fa-solid fa-file-export')
    $("#file-upload i").removeClass('fas fa-spinner fa-pulse')
}

//// データエラー表示
//function RenderErrorBlock(data) {
//    let html = "";
//    for (let i = 0; i < data.length; i++) {
//        html += '<span class="text-danger">' + data[i] + '</span>';
//    }
//    document.getElementById('ErrorBlock').innerHTML = html;
//}
//--------------------------------------------------------//


//------------------- CSV出力 ------------------//
async function onExportFile(page, gamenName) {
    event.preventDefault();
    $('#div-error-message').text("");

    const response = await fetch('' + page + '/ExportFile?gamenName=' + gamenName, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });
    var { data, errorMessage } = await response.json();

    if (errorMessage) {
        $("#div-error-message").show();
        $("#div-error-message").text(errorMessage);
    } else {
        var { contentType, fileContents, fileDownloadName } = data;
        {
            const link = document.createElement("a");
            link.href = `data:${contentType};base64,${fileContents}`;
            link.download = fileDownloadName;
            link.click();
        }
    }
}

// 条件あり
function onExportCsvByCondition(page, formData) {

    $.ajax({
        url: '' + page + '/ExportCsv',
        type: 'post',
        data: formData,
        contentType: false,
        processData: false,
    }).done(function (response) {
        if (response.data != null) {
            var { contentType, fileContents, fileDownloadName } = response.data;
            {
                const link = document.createElement("a");
                link.href = `data:${contentType};base64,${fileContents}`;
                link.download = fileDownloadName;
                link.click();
            }
        }
        else {
            $("#div-error-message").show();
            $("#div-error-message").html(response.errorMessage);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 404) {
            // データが見つからなかった場合
            var errorMessage = jqXHR.responseJSON.errorMessage;
            $("#div-error-message").show();
            $("#div-error-message").html(errorMessage);
        } else {
            // その他のエラーの場合
            $("#div-error-message").show();
            AlertMessage('bg-danger', 'エラー', 'E3003 サーバーに接続できませんでした。<br> ' + 'HttpRequest : ' + jqXHR.status + '<br> ' + 'textStatus : ' + textStatus, null, null);
        }
    });
}
//------------------- CSV出力 ------------------//


//------------------- モーダル表示 ------------------//
function AlertMessage(type, title, message, isRedirect, urlRedirect, isNotReload = false) {
    const dialog = document.getElementById("alert-modal");
    if (dialog) {
        dialog.parentNode.removeChild(dialog);
    }
    $('body').append(
        '<div class="modal fade" id="alert-modal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">' +
        '  <div class="modal-dialog" role="document">' +
        '    <div class="modal-content">' +
        '      <div class="modal-header ' + type + '">' +
        '        <h5 class="modal-title">' + title + '</h5 > ' +
        '        <button type="button" class="close" data-dismiss="modal" aria-label="Close" style="display:none">' +
        '          <span aria-hidden="true">&times;</span > ' +
        '        </button>' +
        '      </div>' +
        '      <div class="modal-body">' +
        '        <p>' + message + '</p > ' +
        '      </div>' +
        '      <div class="modal-footer d-flex flex-wrap justify-content-center">' +
        '        <button type="button" class="btn btn-accent confirm" data-dismiss="modal">OK</button>' +
        '      </div>' +
        '    </div>' +
        '  </div>' +
        '</div>'
    );

    $('#alert-modal').modal({ backdrop: 'static' });
    $('.modal-backdrop').css({ 'opacity': '0.1' });
    $('#alert-modal').modal('show');
    
    $('#alert-modal .confirm, #alert-modal .close').on('click', function () {
        $('#alert-modal').modal('hide');
        $('.modal-backdrop').hide();

        if (isNotReload) return;
        if (isRedirect)
            window.location.href = urlRedirect;
        else
            location.reload();
    });
}
//--------------------------------------------------------//
