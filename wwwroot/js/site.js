$(document).ready(function () {
    //------------------- サイドメニュー ------------------//
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
    //--------------------------------------------------------//

    //------------------- DataTables ------------------//
    // 日本語表示
    var projectName = window.location.pathname.split('/')[1];
    var original_url = window.location.origin + '/' + projectName + '/';
    const language_url = "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json";
    const dom_structure = "<'top d-flex align-items-center'li<'ml-auto'f>>rt<'bottom'p><'clear'>";

    // ID(2列目)昇順
    $('.datatable-normal').DataTable({
        "language": {
            "url": language_url
        },
        lengthChange: true,     // 件数切替
        info: true,            // 総件数
        scrollX: true,          // 横スクロール可
        scrollCollapse: true,   // 縦スクロール表示
        order: [[1, "asc"]],    // ID昇順
        searchHighlight: true,  // 検索ハイライト
        columnDefs: [
            { targets: 0, sortable: false },    // インデックス0列(アイコン列)のソート禁止
        ],
        "oLanguage": {
            "sSearch": "フリーワード(全件)"
        },
        dom: dom_structure
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
        scrollX: true,          // 横スクロール可
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
        scrollMonth: false,
        scrollInput: false,
        onShow: function (ct) {
            this.setOptions({
                maxDate: jQuery("#end_datetimepicker").val() ? jQuery("#end_datetimepicker").val() : false,
                formatDate: "Y/m/d"
            })
        }
    });
});
//--------------------------------------------------------//

//------------------- サイドメニュー ------------------//
// スマホ用のSidebarを開いている時は、画面遷移時に自動でSidebarを閉じるようカスタマイズ
function toggleSidebar() {
    if (window.innerWidth < 1200) {
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
    }
}
//--------------------------------------------------------//

//------------------- CSV取込 ------------------//
function onUploadFile(page) {

    // ページの更新を禁止する
    event.preventDefault();

    $('#div-error-message').empty;

    var formData = new FormData(document.querySelector('#' + page + ''));

    var fileUpload = document.getElementById('UploadFileList');
    if (fileUpload.files.length <= 0) {
        $('#div-error-message').text('E1019: ファイルが選択されていません。');
        $("#div-error-message").show();
        return false;
    }

    var IsFirst = true;
    for (var file of formData) {
        if (IsFirst) {
            if (file[1]["size"] <= 0) {
                $('#div-error-message').text('E1019: ファイルが選択されていません。');
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
        var loginUrl = document.getElementById('login_action_url').value;

        showLoading();
        $.ajax({
            url: importUrl,
            method: 'post',
            data: formData,
            processData: false,
            contentType: false
        }).done(function (response) {
            var errorCode = "E1016";
            // エラーコードを含む戻り値をチェック
            if (response.indexOf(errorCode) == -1) {
                hideLoading();
                AlertMessage('', '取込', '登録が完了しました。', null, null);
            }
            else {
                window.location.href = loginUrl;
            }
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
//--------------------------------------------------------//


//------------------- CSV,Excel出力 ------------------//
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

//------------------- 数秒待機 ------------------//
function WaitSeconds() {
    return new Promise(resolve => {
        setTimeout(() => resolve(), 2000);
    });
}
//--------------------------------------------------------//

//------------------- 仕入先かんばんマスターバリデーションチェック ------------------//
function CheckValidationMSupplierKanban() {
    var checkFlag = true;

    // 識別文字
    var IdentifyStringStartIndex = $("#IdentifyStringStartIndex");
    var IdentifyString = $("#IdentifyString");
    if (IdentifyStringStartIndex.val() <= 0) {
        IdentifyStringStartIndex.addClass("input-validation-error");
        checkFlag = false;
    }
    if (parseInt(IdentifyString.val().length, 10) <= 0) {
        IdentifyString.addClass("input-validation-error");
        checkFlag = false;
    }

    // 重複許容フラグ
    var selectedValue = $('input[name="AllowedDuplicatesFlag"]:checked').val();
    var requiredCheck = false;
    if (selectedValue == '0') {
        requiredCheck = true;
    }

    // 仕入先かんばん名
    var kanbanName = document.getElementById("SupplierKanbanName");
    if (kanbanName.value.length <= 0) {
        kanbanName.classList.add("input-validation-error");
        checkFlag = false;
    }

    // 桁数・開始位置
    var checkProductNumber = CheckPairValueMSupplierKanban("ProductNumberLength", "ProductNumberStartIndex", true);
    var checkQuantity = CheckPairValueMSupplierKanban("QuantityLength", "QuantityStartIndex");
    var checkLot = CheckPairValueMSupplierKanban("LotLength", "LotStartIndex");
    var checkMainProductKey = CheckPairValueMSupplierKanban("MainProductKeyLength", "MainProductKeyStartIndex", requiredCheck);
    var checkFirstSubProductKey = CheckPairValueMSupplierKanban("FirstSubProductKeyLength", "FirstSubProductKeyStartIndex");
    var checkSecondSubProductKey = CheckPairValueMSupplierKanban("SecondSubProductKeyLength", "SecondSubProductKeyStartIndex");
    var checkProductBranchNumber = CheckPairValueMSupplierKanban("ProductBranchNumberLength", "ProductBranchNumberStartIndex");
    var checkOrderNumber = CheckPairValueMSupplierKanban("OrderNumberLength", "OrderNumberStartIndex");

    if (!checkProductNumber || !checkQuantity || !checkLot || !checkFirstSubProductKey
        || !checkSecondSubProductKey || !checkProductBranchNumber || !checkOrderNumber) {
        checkFlag = false;
    } 

    if (!checkFlag) {
        $("#div-error-message").text("E1017: 入力値に不正な値があります。正しい値を入力してください。");
    }

    if (!checkMainProductKey) {
        $("#duplicate-error-message").text("E1024: 重複許容フラグが0の場合、メインキーは必須項目です。");
        $(".MainProductKey").addClass("input-validation-error");
    }

    if (!checkFlag) return false;
    if (!checkMainProductKey) return false;
}

// 入力必須項目チェック
function CheckPairValueMSupplierKanban(id1, id2, required = false) {
    var checkFlag = true;
    var length = $("#" + id1);
    var startIndex = $("#" + id2);
    var lengthValue = parseInt(length.val(), 10);
    var startIndexValue = parseInt(startIndex.val(), 10);

    // 空白の場合は0に変換
    if (Number.isNaN(lengthValue)) {
        length.val(0);
    }
    if (Number.isNaN(startIndexValue)) {
        startIndex.val(0);
    }

    // どちらかが0の場合はエラー
    if (lengthValue > 0 && (startIndexValue <= 0 || Number.isNaN(startIndexValue))) {
        startIndex.addClass("input-validation-error");
        checkFlag = false;
    }
    if ((lengthValue <= 0 || Number.isNaN(lengthValue)) && startIndexValue > 0) {
        length.addClass("input-validation-error");
        checkFlag = false;
    }

    // 必須項目が0未満の場合はエラー
    if (required) {
        if (Number.isNaN(lengthValue) || lengthValue <= 0) {
            length.addClass("input-validation-error");
            checkFlag = false;
        }
        if (Number.isNaN(startIndexValue) || startIndexValue <= 0) {
            startIndex.addClass("input-validation-error");
            checkFlag = false;
        }
    }
    return checkFlag;
}
//--------------------------------------------------------//

//------------------- バリデーションチェック ------------------//
// 小数点とMaxLengthチェック
function CheckInputNumber() {
    var checkFlag = true;
    $('input[type="number"]').each(function () {
        var value = $(this).val();
        // 小数点
        if (!Number.isInteger(Number(value)) || Number(value) < 0) {
            $(this).addClass('input-validation-error');
            checkFlag = false;
        }
        // MaxLength
        if (value.length > 10 || isNaN(value)) {
            $(this).addClass('input-validation-error');
            checkFlag = false;
        }
    });
    return checkFlag;
}
//--------------------------------------------------------//

// --------異なるログインを検出したため自動ログアウトされ、テーブルデータを表示--------//
function HasOtherLogin(response, url, colNum) {
    var errorCode = "E1016";
    // エラーコードを含む戻り値をチェック
    if (response.indexOf(errorCode) !== -1)
        window.location.href = url;
    else {
        $("#table-datatable tbody").empty();
        $('#table-datatable').DataTable().destroy();
        $("#div-table").show();
        $("#table-datatable tbody").html(response);
        $("#table-datatable").DataTable({
            "language": {           // 日本語表示
                "url": "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json"
            },
            lengthChange: true,     // 表示件数
            info: false,            // 総件数
            scrollX: true,          // 横スクロール可
            order: [[colNum, "asc"]],    // 仕入先品番昇順
        });
    }
}
//--------------------------------------------------------//

// ------------------------Chart.js関連------------------------//

// グラフ凡例で用いるカラーコード
const graphColors = [
    'rgba(77, 121, 167, 1)',
    'rgba(255, 88, 88, 1)',
    'rgba(158, 118, 95, 1)',
    'rgba(90, 161, 80, 1)',
    'rgba(242, 143, 41, 1)',
    'rgba(161, 203, 232, 1)',
    'rgba(240, 206, 100, 1)',
    'rgba(177, 122, 162, 1)',
    'rgba(71, 152, 147, 1)',
    'rgba(183, 153, 44, 1)',
    'rgba(255, 157, 154, 1)',
    'rgba(121, 112, 110, 1)',
    'rgba(212, 113, 149, 1)',
    'rgba(139, 209, 127, 1)',
    'rgba(209, 175, 201, 1)',
    'rgba(185, 176, 172, 1)',
    'rgba(249, 192, 211, 1)',
    'rgba(255, 190, 125, 1)',
    'rgba(134, 187, 182, 1)',
    'rgba(215, 181, 168, 1)'
];
//--------------------------------------------------------//


    (function ($) {
        $.fn.cehckcalendar = function (options) {
            var settings = $.extend({
                'week': ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
                'roop': 1,
                'prefix': 'mycld_',   //カレンダーで使われる変数名 他と被らないようユニークに
                'delimiter': '-',        //送信時の日付の区切り文字 yyy/mm/dd
                'td_on': '#B3D39B',  //日付をONにしたときの背景色
                'td_off': '#FFFFFF',  //日付をOFFにしたときの背景色
                'send': 'days'      //postするときのinputのid <input type="hidden" id="days" name="days" >
            }, options);

            $(this).html('');//カレンダー展開場所の中身をクリア

            var $div = $(this);
            var str_Date = null;
            var end_Date = null;

            var class_td = settings.prefix + 'td';
            var class_month_on = settings.prefix + 'month_on';
            var class_month_off = settings.prefix + 'month_off';

            /*
             * 開始日を設定
             */
            if (settings.start) {
                var s_Date = settings.start.split("-");
                str_Date = new Date(s_Date[0], s_Date[1] - 1, 1);
            } else {
                var sdate = new Date();
                var s_yy = sdate.getFullYear();
                var s_mm = sdate.getMonth() + 1;
                var s_dd = sdate.getDate();
                str_Date = new Date(s_yy, s_mm - 1, s_dd);
            }

            var y = str_Date.getFullYear();
            var m = str_Date.getMonth() + 1;

            /*
             * 日付をクリックしたら背景色を変更する
             */
            $(document).on('click', '.' + class_td, function () {
                var flag = $(this).data("flag");
                if (flag == 'on') {
                    $(this).css({ 'background-color': settings.td_off });
                    $(this).data("flag", "off");
                } else if (flag == 'off') {
                    $(this).css({ 'background-color': settings.td_on });
                    $(this).data("flag", "on");
                }
                getDate();
            });

            /*
             * 全選択
             */
            $(document).on('click', '#' + class_month_on, function (e) {
                $(this).parents('table').find('td').css({ 'background-color': settings.td_on });
                $(this).parents('table').find('td').data("flag", "on");
                getDate();
                e.preventDefault();
            });

            /*
             * 全解除
             */
            $(document).on('click', '#' + class_month_off, function (e) {
                $(this).parents('table').find('td').css({ 'background-color': settings.td_off });
                $(this).parents('table').find('td').data("flag", "off");
                getDate();
                e.preventDefault();
            });


            /*
             * 選択した日付を得る
             */
            var getDate = function () {
                var data = '';
                $div.find("td").each(function () {
                    var flag = $(this).data('flag');
                    if (flag == 'on' && $(this).text()) { //空白日は除外
                        var id = $(this).attr('id');
                        id = id.replace(settings.prefix, ""); //prefixを消して日付だけにする
                        data += id + ',';
                    }
                });
                data = data.slice(0, -1); //末尾のカンマを取り除く

                $("#" + settings.send).val(data); // inputのhiddenへ
            };

            /*
             * カレンダー展開
             */
            var Calendar = function (obj, yyyy, mmmm) {
                var week = settings.week;
                var html = '';

                for (var i = 0; i < settings.roop; i++) {
                    var sdate = new Date(yyyy, (mmmm - 1) + i, 1);
                    var s_yy = sdate.getFullYear(); //年
                    var s_mm = sdate.getMonth() + 1;  //月
                    //var s_dd  = sdate.getDate();
                    var blank = sdate.getDay() | 0;           //月始めの空白
                    var last = lastDay(s_yy, s_mm) | 0;        //月末の日
                    var cal = Math.ceil((blank + last) / 7); //行数を求める

                    var table_ID = settings.prefix + '' + s_yy + '' + s_mm;

                    html += '<table class="calendar_button" id="' + table_ID + '">';
                    html += '  <tr>';
                    html += '    <th colspan="7">' + s_yy + '/' + s_mm + ' &nbsp; <a href="#" id="' + class_month_on + '">全選択</a> <a href="#" id="' + class_month_off + '">解除</a></th>';
                    html += '  </tr>';

                    html += '<tr>';
                    html += '  <th>' + settings.week[0] + '</th>';
                    html += '  <th>' + settings.week[1] + '</th>';
                    html += '  <th>' + settings.week[2] + '</th>';
                    html += '  <th>' + settings.week[3] + '</th>';
                    html += '  <th>' + settings.week[4] + '</th>';
                    html += '  <th>' + settings.week[5] + '</th>';
                    html += '  <th>' + settings.week[6] + '</th>';
                    html += '</tr>';

                    //settings.prefix   delimiter

                    //行数だけループ
                    var setDay = 0;
                    for (var r = 0; r < cal; r++) {
                        html += '<tr>';
                        //1週間分をループ
                        for (var d = 0; d < 7; d++) {
                            var day = '';
                            var ymd = '';
                            if (r == 0 && d < blank) {
                                day = '';
                            } else {
                                setDay++;
                                if (setDay <= last) {
                                    day = setDay;
                                    ymd = s_yy + '' + settings.delimiter + '' + s_mm + '' + settings.delimiter + '' + setDay;
                                } else {
                                    day = '';
                                }
                            }
                            var id = settings.prefix + '' + ymd;
                            html += '  <td data-flag="off" class="' + class_td + '" id="' + id + '">' + day + '</td>';
                        }
                        html += '</tr>';
                    }
                    html += '</table>';
                }

                obj.html(html);//HTMLを挿入

            };

            /*
             * 月末を得る
             */
            var lastDay = function (y, m) {
                var dt = new Date(y, m, 0);
                return dt.getDate();
            };

            return Calendar($(this), y, m);
        };
    })(jQuery);