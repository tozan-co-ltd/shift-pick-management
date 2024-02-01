// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ドキュメントが読み込まれたら実行
$(document).ready(function () {
    // #sidebarToggle 要素がクリックされたときに、サイドバーの表示/非表示の状態をトグルし、クッキーに保存する
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

    //------------------- DataTables　------------------//
    //トープ
    $('#handyErrorTbl').DataTable({
        "language": {           // 日本語表示
            "url": "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json"
        },
        lengthChange: true,     // 表示件数
        info: false,            // 総件数
        scrollX: true,
        scrollY: false,
        order: [[1, "asc"]],   // 読取日時昇順
    });

    //M_User
    $('#userTbl').DataTable({
        "language": {           // 日本語表示
            "url": "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json"
        },
        lengthChange: true,     // 表示件数
        info: false,            // 総件数
        scrollX: false,
        scrollY: false,
        order: [[1, "asc"]],   // ユーザーID
    });
    //------------------- DataTables　------------------//

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
});
//--------------------------------------------------------//


//------------------- パスワード変更　------------------//
$("#changePasswordForm").submit(function () {
    $(".changePassword .alert-success").css("display", "none");
});

// パスワード表示アイコン
$('#eye-change-pass').click(function () {
    if ($(this).hasClass('fa-eye')) {
        $(this).removeClass('fa-eye');
        $(this).addClass('fa-eye-slash');
        $('#password-field').attr('type', 'text');
    } else {
        $(this).removeClass('fa-eye-slash');
        $(this).addClass('fa-eye');
        $('#password-field').attr('type', 'password');
    }
});

$('#eye-login').click(function () {
    if ($(this).hasClass('fa-eye')) {
        $(this).removeClass('fa-eye');
        $(this).addClass('fa-eye-slash');
        $('.login-text').attr('type', 'text');
    } else {
        $(this).removeClass('fa-eye-slash');
        $(this).addClass('fa-eye');
        $('.input-password .login-text').attr('type', 'password');
    }
});

//------------------- Excel取込　------------------//
function onUploadFile(page) {
    $("#FailMsg").text("");
    $('#import-res').empty;
    $('#import-res').removeClass('text-danger');

    console.log(page);

    // FormDataオブジェクト利用
    var formData = new FormData(document.querySelector('#' + page + ''));
    var IsFirst = true;
    for (var file of formData) {
        if (IsFirst) {
            if (file[1]["size"] <= 0) {
                $('#import-res').text('ファイルが選択されていません。');
                $("#import-res").show()
                $("#import-res").addClass('text-danger');
                $("#ErrorBlock").hide()
                return false;
            }
            else {
                $("#import-res").hide()
                $("#ErrorBlock").hide()
                IsFirst = false;
            }
        }

    }

    const dialog = document.getElementById("ImportModel");

    if (dialog) {
        dialog.parentNode.removeChild(dialog);
    }

    var modelTitle = "";
    if (page == 'shipping-plan-import-upload-form')
        modelTitle = "出荷計画取込";

    if (page == 'user-master-upload-form')
        modelTitle = "ユーザーマスター";

    if (page == 'shipping-master-upload-form')
        modelTitle = "出荷レーンマスター";

    if (page == 'm-routes-master-upload-form')
        modelTitle = "運行便マスター";

    $('body').append(
        '<div class="modal fade" id="ImportModel" tabindex="-1" role="dialog" aria-labelledby="importModalCenterTitle" aria-hidden="true">' +
        '    <div class="modal-dialog modal-dialog-centered" role="document">' +
        '        <div class="modal-content">' +
        '            <div class="modal-header">' +
        '                <h5 class="modal-title" id="importModalCenterTitle">' + modelTitle + '</h5>' +
        '                <button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '                    <span aria-hidden="true">&times;</span>' +
        '                </button>' +
        '            </div>' +
        '            <div class="modal-body">' +
        '                <p>ファイル取込を行います。よろしいですか？</p > ' +
        '            </div>' +
        '            <div class="modal-footer">' +
        '                <button type="button" class="btn btn-secondary" data-dismiss="modal">キャンセル</button>' +
        '                <button type="button" class="btn btn-primary">OK</button>' +
        '            </div>' +
        '        </div>' +
        '    </div>' +
        '</div>'
    );

    $('#ImportModel').modal('show');

    $('#ImportModel').on('hidden.bs.modal', function (e) {
        //なし
    });

    $('#ImportModel .btn-secondary').click(function () {
        $('#ImportModel').modal('hide');
        return false;
    });

    $('#ImportModel .btn-primary').click(function () {
        $('#ImportModel').modal('hide');

        var importUrl = document.getElementById('import_action_url').value;
        formData.append("userName", "@User.Identity.Name");
        showLoading()
        $.ajax({
            url: importUrl,
            method: 'post',
            data: formData,
            processData: false,
            contentType: false
        }).done(function (response) {
            if (response.res == "OK") {
                if (response.data != "OK") {
                    let data = JSON.parse(response.data);
                    console.log(data);
                    $("#import-res").addClass('text-danger');
                    $("#ErrorBlock").show()
                    RenderErrorBlock(data);
                    $('#' + page + '')[0].reset();
                    hideLoading()
                }
                else {
                    hideLoading()
                    AlertMessage('', '' + modelTitle + '', '登録が完了しました。', null, null);
                }
            }
            else {
                hideLoading()
                $("#ErrorBlock").hide()
                $("#import-res").show()
                $("#import-res").addClass('text-danger');
                $("#import-res").text(response.error);
                $('#' + page + '')[0].reset();
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            hideLoading()
            console.log("jqXHR", jqXHR.status);
            console.log("textStatus", textStatus);
            console.log("errorThrown", errorThrown.message);
            $("#import-res").addClass('text-danger');
            $("#import-res").show()
            AlertMessage('bg-danger', 'エラー', 'E3003 サーバーに接続できませんでした。<br> ' + 'HttpRequest : ' + jqXHR.status + '<br> ' + 'textStatus : ' + textStatus, null, null);
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

// データエラー表示
function RenderErrorBlock(data) {
    let html = "";
    for (let i = 0; i < data.length; i++) {
        html += '<span class="text-danger">' + data[i] + '</span>';
    }
    document.getElementById('ErrorBlock').innerHTML = html;
}
//--------------------------------------------------------//

//------------------- フォール出力　------------------//
async function onExportFile(page) {
    event.preventDefault();
    $("#ErrorBlock").text("");
    $('#import-res').text("");

    console.log(page);

    const response = await fetch('' + page + '/ExportFile', {
        method: 'GET', // *GET, POST, PUT, DELETE, etc.
        headers: {
            'Content-Type': 'application/json'
        },
    });
    var { data, error } = await response.json();
    if (error) {
        //エラー
        $("#ErrorBlock").show()
        $("#ErrorBlock").text(error);
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
function onExportExcelByCondition(page) {
    var startDate = $("#startDate").val();
    var endDate = $("#endDate").val();
    formData = { startDate: startDate, endDate: endDate };
    $.ajax({
        type: 'POST',
        url: '' + page + '/ExportExcel',
        data: formData,
        success: function (response) {
            if (response.res == "OK") {
                var { contentType, fileContents, fileDownloadName } = response.data;
                {
                    const link = document.createElement("a");
                    link.href = `data:${contentType};base64,${fileContents}`;
                    link.download = fileDownloadName;
                    link.click();
                }
            } else {
                let html = '<span class="text-danger">' + response.error + '</span>';
                document.getElementById('ErrorBlock').innerHTML = html;
            }
        },
        error: function (request, status, error) {
            alert(request.responseText);
        }
    });
}
//--------------------------------------------------------//

// モーダル表示
function AlertMessage(type, title, message, isRedirect, urlRedirect) {
    const dialog = document.getElementById("AlertDialogId");
    if (dialog) {
        dialog.parentNode.removeChild(dialog);
    }
    var actionAfter = "OK";
    if (isRedirect) {
        message = "登録が完了しました。ユーザーマスター画面へ戻ります。";
        actionAfter = "OK";
    }
    $('body').append(
        '<div class="modal fade" id="AlertDialogId" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">' +
        '  <div class="modal-dialog" role="document">' +
        '    <div class="modal-content">' +
        '      <div class="modal-header ' + type + '">' +
        '        <h5 class="modal-title">' + title + '</h5 > ' +
        '        <button type="button" class="close" data-dismiss="modal" aria-label="Close">' +
        '          <span aria-hidden="true">&times;</span > ' +
        '        </button>' +
        '      </div>' +
        '      <div class="modal-body">' +
        '        <p>' + message + '</p > ' +
        '      </div>' +
        '      <div class="modal-footer d-flex flex-wrap justify-content-center">' +
        '        <button type="button" class="btn btn-accent confirm" data-dismiss="modal">' + actionAfter + '</button > ' +
        '      </div>' +
        '    </div>' +
        '  </div>' +
        '</div>');

    $('#AlertDialogId').modal({ backdrop: 'static' });
    $('#AlertDialogId').modal('show');
    
    $('#AlertDialogId .confirm, #AlertDialogId .close').on('click', function () {
        $('#AlertDialogId').modal('hide');
        if (isRedirect)
            window.location.href = urlRedirect;　// ユーザーマスターへ戻る
        else
            location.reload();
    });
}
//--------------------------------------------------------//



//------------------- タイムピッカー ------------------//
$(document).ready(function () {
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
