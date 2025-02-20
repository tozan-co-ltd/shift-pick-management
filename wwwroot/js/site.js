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
            "url": language_url,
            searchPlaceholder: "フリーワード(全件)"
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
            "sSearch": ""
        },
        dom: dom_structure
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



//------------------- Excel出力 ------------------//
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

function onExportFileTripsCommon(page, data) {

    // フォーム情報取得
    let url = window.location.href + '/ExportFile';
    let method = 'Post';
    tableDisplay(page);
    event.preventDefault();
    // Ajax call
    $.ajax({
        url: url,
        method: method,
        data: data
    }).done(function (response) {
        var { contentType, fileContents, fileDownloadName } = response.data;
        {
            const link = document.createElement("a");
            link.href = `data:${contentType};base64,${fileContents}`;
            link.download = fileDownloadName;
            link.click();
        }

    }).fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 404) {
            var errorMessage = jqXHR.responseJSON.errorMessage;
            $("#div-error-message").show();
            $("#div-error-message").text(errorMessage);
        } else {
            var errorMessage = 'E3002: サーバーに接続できませんでした。' + ' HttpRequest : ' + jqXHR.status + ' textStatus : ' + textStatus;
            $("#div-error-message").show();
            $("#div-error-message").text(errorMessage);
        }
    });
}
//--------------------------------------------------------//


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

// データセットの作成、登録
function createAndPushDatasets(targetChart, color, data, label) {
    var datasets = createChartDatasets(color);
    datasets.data = data;
    datasets.label = label;
    targetChart.data.datasets.push(datasets);
}
//--------------------------------------------------------//


//-----------------カレンダー日付複数選択-------------------//
let selectedDates
$.datetimepicker.setLocale('ja');
$('.pickerDateForLoadOperationRecord').datetimepicker({
format: "Y/m/d",
allowTimes: ['00:00'],
onShow: function (dp, $input) {
selectedDates = [];
this.setOptions({
highlightedDates: selectedDates
})
},
onSelectDate: function (dp, $input) {
// 選択された日付を取得
var selectedDate = $input.val();
selectedDates.push(selectedDate)
this.setOptions({
highlightedDates: selectedDates
})
}

});
//--------------------------------------------------------//

//----------------------荷量画像モーダル関連--------------//
let model;

// 便実績テーブルの要素数
let tableLength = 0;

// 便実績テーブルの行情報配列
let arrayTrs = [];

// 到着荷量画像ボタン押下
function OnArrivalLoadImageClick(tripRecordID, button, page) {
    // 配列の初期化
    arrayTrs = [];
    // 到着
    var isArrived = true;
    // テーブルに表示されている便実績のIDをリスト化
    trs = button.parentNode.parentNode.parentNode.childNodes;
    for (i = 0; i < trs.length - tableLength; i++) {
        arrayTrs.push(trs[i + tableLength].childNodes[1].textContent);
    }
    EditModal(tripRecordID, isArrived, page);
}

// 出発荷量画像ボタン押下
function OnDepartureLoadImageClick(tripRecordID, button, page) {
    // 配列の初期化
    arrayTrs = [];
    // 出発
    var isArrived = false;
    // テーブルに表示されている便実績のIDをリスト化
    trs = button.parentNode.parentNode.parentNode.childNodes;
    for (i = 0; i < trs.length - tableLength; i++) {
        arrayTrs.push(trs[i + tableLength].childNodes[1].textContent);
    }
    EditModal(tripRecordID, isArrived, page);
}

// 荷量画像モーダル作成
function EditModal(tripRecordID, isArrived, page) {
    // デフォルトの操作を無効化
    event.preventDefault();

    // LoadOutputModel取得
    var tripRecordModel = model.tripRecordList;
    tripRecordModel.forEach(function (item) {
        if (item.tripRecordID == tripRecordID) {

            // フォーム情報取得
            let url = window.location.href + '/GetModalItems';
            url = url.replace(page, 'LoadRecord');
            let method = 'POST';
            let data = { model: item, isArrived: isArrived };

            // Ajax call
            $.ajax({
                url: url,
                method: method,
                data: data
            }).done(function (response) {
                var imagePath = "";
                var arriveOrDeparture = "";
                var arriveOrDepartureDate = "";
                var loadStatus = "";
                var downloadFileName = "";
                var workDayForFile = GetDayStringForFile(new Date(item.workDay));
                var tripNameAndBranchSeq = item.tripName + "_" + item.tripBranchSeq;
                var truckNumber = item.truckNumber;
                if (truckNumber == "0") truckNumber = "-";
                if (tripNameAndBranchSeq == "-_-") tripNameAndBranchSeq = "-";

                // 到着か出発かで変わる要素の登録
                if (isArrived) {
                    $('#detail-modal-label').text("到着荷量画像");
                    loadStatus = item.arrivalLoadStatus;
                    imagePath = response.arrivalLoadImgPath;
                    arriveOrDeparture = "到着時間";
                    arriveOrDepartureDate = GetDateString(new Date(item.arrivedAt));
                    downloadFileName = item.tripName + "_" + item.tripBranchSeq + "_" + workDayForFile + "_A_" + loadStatus + ".jpg";
                }
                else {
                    $('#detail-modal-label').text("出発荷量画像");
                    loadStatus = item.departureLoadStatus;
                    imagePath = response.departureLoadImgPath;
                    arriveOrDeparture = "出発時間";
                    arriveOrDepartureDate = GetDateString(new Date(item.departedAt));
                    downloadFileName = item.tripName + "_" + item.tripBranchSeq + "_" + workDayForFile + "_D_" + loadStatus + ".jpg";
                }

                $('#modalImage').attr("src", "data:image/jpeg;base64," + imagePath);

                var workDay = GetDayString(new Date(item.workDay));

                // 表示している行の上下の行の便実績ID取得
                var tripRecordIDIndex = arrayTrs.indexOf(tripRecordID);
                var previousTripRecordID = arrayTrs[tripRecordIDIndex - 1];
                var nextTripRecordID = arrayTrs[tripRecordIDIndex + 1];


                // ボタンの追加
                var buttons = $("#tripRecord-buttons");
                buttons.empty();
                let div = "<div class=\"d-flex xs-block justify-content-start align-items-center p-0 mb-2 trip-record-buttons\">"
                    + "<div class=\"input-group-append mr-3\" >"
                    + " <a href=\"#\" class=\"btn btn-secondary btn-icon-split\" onclick = \"onOtherModalClick('" + previousTripRecordID + "','" + isArrived + "','" + page + "')\" >"
                    + "<span class=\"text\" >▲上へ</span>"
                    + "</a>"
                    + "</div>"
                    + "<div class=\"input-group-append mr-3\" >"
                    + " <a href=\"#\" class=\"btn btn-secondary btn-icon-split\" onclick = \"onOtherModalClick('" + nextTripRecordID + "','" + isArrived + "','" + page + "')\" >"
                    + "<span class=\"text\" >▼下へ</span>"
                    + "</a>"
                    + "</div>"
                    + "<div class=\"input-group-append mr-3 right-button\" >"
                    + " <a href=\"data:image/jpeg;base64," + imagePath + "\", download=\"" + downloadFileName + "\" class=\"btn btn-info btn-icon-split\"  >"
                    + "<span class=\"icon text-white-50\" >"
                    + "<i class=\"fa-solid fa-circle-down\" > </i>"
                    + "</span>"
                    + "<span class=\"text\" >画像出力</span>"
                    + "</a>"
                    + "</div>";
                buttons.append(div);

                // テーブルの追加
                var container = $("#tripRecord-contatiner");
                container.empty();
                let tr = "<tr>"
                    + "<td>荷量</td>"
                    + "<td>" + loadStatus + "%</td>"
                    + "</tr><tr>"
                    + "<td>荷量の相違あり</td>"
                    + "<td>"
                    + "<div class=\"select-modal d-flex xs-block justify-content-start align-items-center p-0\">";
                if (!(userName == "服部 正次" || userName == "林 恭佑")) {
                    tr += "<label id=\"loadStatusSelect\" ></label>";
                } else {
                    tr += "<select name=\"loadStatusSelect\"  class=\"form-select mr-2\" id=\"loadStatusSelect\" >"
                        + "<option value=\"\" hidden></option>"
                        + "<option value=\"1\">0%</option>"
                        + "<option value=\"3\">1-10%</option>"
                        + "<option value=\"4\">11-20%</option>"
                        + "<option value=\"5\">21-30%</option>"
                        + "<option value=\"6\">31-40%</option>"
                        + "<option value=\"7\">41-50%</option>"
                        + "<option value=\"8\">51-60%</option>"
                        + "<option value=\"9\">61-70%</option>"
                        + "<option value=\"10\">71-80%</option>"
                        + "<option value=\"11\">81-90%</option>"
                        + "<option value=\"12\">91-100%</option>"
                        + "</select>"
                        + "<a href=\"#\" class=\"btn btn-update\" onclick=\"onVerificationRequiredClick('" + item.tripRecordID + "', '" + isArrived + "', '" + page + "')\" id=\"verificationRequired\">"
                        + "<span class=\"text\">要検証</span>"
                        + "</a>";
                }

                tr += "</div>"
                    + "</td>"
                    + "</tr><tr>"
                    + "<td>便名称_便枝番</td>"
                    + "<td>" + tripNameAndBranchSeq + "</td>"
                    + "</tr><tr>"
                    + "<td>乗務員</td>"
                    + "<td>" + item.driverName + "</td>"
                    + "</tr><tr>"
                    + "<td>ステーション名</td>"
                    + "<td>" + item.stationName + "</td>"
                    + "</tr><tr>"
                    + "<td>車両番号</td>"
                    + "<td>" + truckNumber + "</td>"
                    + "</tr><tr>"
                    + "<td>識別番号</td>"
                    + "<td>" + item.identifyNumber + "</td>"
                    + "</tr><tr>"
                    + "<td>稼働日</td>"
                    + "<td>" + workDay + "</td>"
                    + "</tr><tr>"
                    + "<td>" + arriveOrDeparture + "</td>"
                    + "<td>" + arriveOrDepartureDate + "</td>"
                    + "</tr>";
                container.append(tr);
                if (userName == "服部 正次" || userName == "林 恭佑") {
                    $('#loadStatusSelect').val(response.annotationLoadClass);
                } else if (response.annotationLoadStatus != null) {
                    $('#loadStatusSelect').text(response.annotationLoadStatus + "%");
                } else {
                    $('#loadStatusSelect').text("-");
                }

            }).fail(function (jqXHR, textStatus, errorThrown) {
                var errorMessage = jqXHR.responseJSON.errorMessage;
                $("#edit-modal-error-message").text(errorMessage);
            });

        }
    });
}

// モーダル内の「上へ」「下へ」ボタン押下時
function onOtherModalClick(otherTripRecordID, isArrived, page) {
    event.preventDefault();
    if (isArrived == "true") {
        isArrived = true;
    } else {
        isArrived = false;
    }
    EditModal(otherTripRecordID, isArrived, page);
}

// 要検証ボタン押下時
function onVerificationRequiredClick(tripRecordID, isArrived, page) {
    event.preventDefault();
    var loadStatus = $('[name=loadStatusSelect]').val();
    if (loadStatus != "") {

        DeleteErrorMessages()
        // フォーム情報取得
        let url = window.location.href + '/InsertOrUpdateAnnotationLoads';
        url = url.replace(page, 'LoadRecord');
        let method = 'POST';
        let data = { tripRecordID: tripRecordID, loadStatus: loadStatus, isArrived: isArrived };

        // Ajax call
        $.ajax({
            url: url,
            method: method,
            data: data
        }).done(function (response) {
            // 完了モーダル表示
            alert('登録が完了しました。');
        }).fail(function (jqXHR, textStatus, errorThrown) {
            var errorMessage = jqXHR.responseJSON.errorMessage;
            $("#edit-modal-error-message").text(errorMessage);
        });
    }
}
//---------------------------------------------------------------------//


//-----------------------------期間が90日以内か否か-------------------//
function isWithin90Days() {
    var startOfPeriod = new Date($('#startOfPeriod').val());
    var endOfPeriod = new Date($('#endOfPeriod').val());
    var diffMilliSec = endOfPeriod - startOfPeriod;
    var diffDays = parseInt(diffMilliSec / 1000 / 60 / 60 / 24);
    if (diffDays <= 90) {
        return true;
    }
    else {
        return false;
    }
}
//-------------------------------------------------------------------//

//-------------------------------------------------------------------//
// 便実績テーブル表示の共通部分
function tableDisplayCommon(page, data) {

    // フォーム情報取得
    let url = window.location.href + '/SearchData';
    let method = 'Post';

    // Ajax call
    $.ajax({
        url: url,
        method: method,
        data: data
    }).done(function (response) {
        var tableHTML = response.searchedTripRecordHTML;
        tableLength = response.searchedTripRecordLength + 1;
        $('#tripRecordTable').empty().html(tableHTML);
        // 日本語表示
        const language_url = "https://cdn.datatables.net/plug-ins/1.11.5/i18n/ja.json";
        const dom_structure = "<'top d-flex align-items-center'li<'ml-auto'f>>rt<'bottom'p><'clear'>";

        // ID(2列目)昇順
        $("#tripRecordDataTable").DataTable({
            "language": {
                "url": language_url,
                searchPlaceholder: "フリーワード(全件)"
            },
            lengthChange: true,     // 件数切替
            info: true,            // 総件数
            scrollX: true,          // 横スクロール可
            scrollCollapse: true,   // 縦スクロール表示
            searchHighlight: true,  // 検索ハイライト
            orderFixed: [1, "asc"],
            order: [[2, "asc"] , [9, "asc"], [3, "asc"]],    // ID昇順
            "oLanguage": {
                "sSearch": ""
            },
            dom: dom_structure,
        });
        var table = $("#tripRecordDataTable").DataTable();
        table.on('draw', function () {
            var body = $(table.table().body());

            body.unhighlight();
            body.highlight(table.search());
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        var errorMessage = jqXHR.responseJSON.errorMessage;
        $("#edit-modal-error-message").text(errorMessage);
    });
}

//-------------------------------------------------------------------//

//-------------------------------------------------------------------//
// 画像一括出力共通処理
function onExportAllImagesCommon(page, data) {

    // フォーム情報取得
    let url = window.location.origin + '/' + page + '/ZipDownload';
    let method = 'POST';

    tableDisplay(page);

    // Ajax call
    $.ajax({
        url: url,
        method: method,
        data: data
    }).done(function (response) {
        var { contentType, fileContents, fileDownloadName } = response.data;
        {
            const link = document.createElement("a");
            link.href = `data:${contentType};base64,${fileContents}`;
            link.download = fileDownloadName;
            link.click();
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        var errorMessage = jqXHR.responseJSON.errorMessage;
        $("#div-error-message").text(errorMessage);
    });
}
//-------------------------------------------------------------------//

//-------------------------------------------------------------------//
// エラーメッセージ削除
function DeleteErrorMessages() {
    $('#div-error-message').text('');
    $('#edit-modal-error-message').text('');
    $('#delete-modal-error-message').text('');
    $('.text-danger').text('');
    $('input').removeClass('input-validation-error');
}
//-------------------------------------------------------------------//


//------------------------------時間の表示形式変換-------------------------------//
// 日付の表示形式変換(yyyy/MM/dd)
function GetDayString(date) {
    var day = date.toLocaleDateString("ja-JP", {
        year: "numeric", month: "2-digit",
        day: "2-digit"
    });
    return day;
}


// 時刻の表示形式変換(HH:mm)
function GetTimeString(date) {
    var hour = date.getHours();
    if (hour < 10) {
        hour = "0" + hour;
    }
    var minute = date.getMinutes();
    if (minute < 10) {
        minute = "0" + minute;
    }
    return hour + ":" + minute;
}

// 日時の表示形式変換(yyyy/MM/dd hh:mm)
function GetDateString(date) {
    var day = GetDayString(date);
    var time = GetTimeString(date);
    return day + " " + time;
}


// 日付の表示形式変換(yyyyMMdd)
function GetDayStringForFile(date) {
    var year = date.getFullYear().toString();
    var month = ("00" + (date.getMonth() + 1)).slice(-2);
    var day = ("00" + date.getDate()).slice(-2);
    return year + month + day;
}


// 日付の表示形式変換(MM/dd)
function GetDayStringForChart(date) {
    var month = (date.getMonth() + 1).toString();
    var day = date.getDate().toString();
    return month + "/" + day;
}
//-------------------------------------------------------------------//


// 選択された便の配列
let arrayTrips = [];

// 追加ボタン押下時
function addTrips(selectedTripName) {
    event.preventDefault();
    // 20件の便が選択されているとき
    if (20 <= arrayTrips.length) {
        var errorMessage = 'E1008: 選択できる便数(便＋枝番)は最大20件です。20件を超えないように選択してください。';
        $("#div-error-message").text(errorMessage);
        return;
    }

    console.log("nunu");

    var tripNameAndBranchSeq = selectedTripName.split("_");

    // 便名称と便枝番取得
    var tripName = tripNameAndBranchSeq[0];
    var tripBranchSeq = tripNameAndBranchSeq[1];

    // 入力されていない場合、エラー出力
    if (tripName == "" || tripBranchSeq == "" || tripName == null || tripBranchSeq == null) {
        return;
    }

    // 1行目の場合、項目追加
    if (arrayTrips.length == 0) 
        document.getElementById("selectedTrips0").innerHTML = "<span class=\"warehouse-name-drs span-paragraph mb-3\">選択された便</span>";

    // 重複チェック
    const tripNames = arrayTrips.map(d => d.selectedTripName);
    if (tripNames.includes(selectedTripName)) {
        return;
    }

    // 選択された便を追加する行を設定
    var selectedTripsNumber = Math.floor(arrayTrips.length / 5);
    var selectedTrips = $('#selectedTrips' + selectedTripsNumber)[0];

    // 選択された便に追加
    arrayTrips.push({ tripName, tripBranchSeq, selectedTripName });
    pushLabelToSelectedTrips(selectedTrips, selectedTripName);
}

// 選択された便ラベルを作成、指定した親要素の子として登録
function pushLabelToSelectedTrips(selectedTrips, selectedTripName) {
    var selectedTripLabel = document.createElement("label");
    selectedTripLabel.innerText = selectedTripName;
    selectedTripLabel.innerHTML += "<a href=\"#\" class=\"label-delete ml-1 \" onclick=\"onLabelDeleteClick('" + selectedTripName + "')\" id=\"\">×</a>";
    selectedTripLabel.className += "mr-2 mb-3 selected-trip-label";
    selectedTripLabel.style.backgroundColor = "rgba(200, 200, 200, 0.6)";
    selectedTripLabel.style.padding = "0.5em";
    selectedTripLabel.style.borderRadius = "5px";
    selectedTrips.appendChild(selectedTripLabel);
}

// 選択された便の×ボタン押下時
function onLabelDeleteClick(selectedTripName) {
    event.preventDefault();
    // ラベル全削除
    $('.selected-trip-label').remove();

    // 配列から削除
    const tripNames = arrayTrips.map(d => d.selectedTripName);
    var deleteIndex = tripNames.indexOf(selectedTripName);

    if (deleteIndex < 0)
        return;

    arrayTrips.splice(deleteIndex, 1);

    $('#' + selectedTripName).prop('checked', false);

    // 選択された便が1つも無くなった場合
    if (arrayTrips.length == 0) {
        document.getElementById("selectedTrips0").innerHTML = "";
        return;
    }

    // 選択された便の再表示
    for (var i = 0; i < arrayTrips.length; i++) {
        // 選択された便を追加する行を設定
        var selectedTripsNumber = Math.floor(i / 5);
        var selectedTrips = $('#selectedTrips' + selectedTripsNumber)[0];

        var selectedTripName = arrayTrips[i].selectedTripName;
        pushLabelToSelectedTrips(selectedTrips, selectedTripName);
    }
}

// 追加した選択肢の一括クリアボタン押下時
function clearTrips() {
    event.preventDefault();
    // ラベルと配列から全削除
    $('.selected-trip-label').remove();
    $('[name="tripNameAndBranchSeq"]').prop('checked', false);
    var selectedTrips = document.getElementById("selectedTrips0");
    if (selectedTrips != null)
        selectedTrips.innerHTML = "";
    var selectedWorkDays = document.getElementById("selectedWorkDays0");
    if (selectedWorkDays != null)
        selectedWorkDays.innerHTML = "";
    arrayTrips = [];
    arrayWorkDays = [];
}

function condtionChange(page) {

}

// 期間変更時
function periodChange(page) {
    // 期間の開始、終了日付の取得
    var startOfPeriod = GetDayString(new Date($('#startOfPeriod').val()));
    var endOfPeriod = GetDayString(new Date($('#endOfPeriod').val()));

    var checkedDepos = getCheckedDepos();

    // 選択中の便名称取得
    var currentTripName = $('[name=TripName]').val();

    // 便選択の選択肢の生成
    createToggleSelectCheckBox(startOfPeriod, endOfPeriod, checkedDepos, page);
}

// 便選択の選択肢の生成
function createToggleSelectCheckBox(startOfPeriod, endOfPeriod, checkedDepos, page) {

    // フォーム情報取得
    let url = window.location.href + '/GetTripNameAndBranchSeqHTML';
    url = url.replace(page, 'LoadRecord');
    let method = 'Post';
    let data = { startOfPeriod: startOfPeriod, endOfPeriod: endOfPeriod, checkedDepos: checkedDepos };

    // Ajax call
    $.ajax({
        url: url,
        method: method,
        data: data
    }).done(function (response) {
        $('#createToggleCheckBox').empty().html(response);
        // 中項目のヘッダーをクリックで小項目を表示/非表示
        let mediumHeaders = dropdownContent.querySelectorAll('.medium-header');
        mediumHeaders.forEach(header => {
            header.addEventListener('click', () => {
                const smallItems = header.nextElementSibling;
                const toggleIcon = header.querySelector('.toggle-icon');
                if (smallItems.classList.contains('show')) {
                    smallItems.classList.remove('show');
                    toggleIcon.classList.add('collapsed');
                } else {
                    smallItems.classList.add('show');
                    toggleIcon.classList.remove('collapsed');
                }
            });
        });

        // チェックボックス切り替え時のイベント設定
        $(function () {
            $('input[name="tripNameAndBranchSeq"]').change(function () {
                // デフォルトの操作を無効化
                event.preventDefault();

                var checkBox = $(this).prop('checked');
                var selectedTripName = $(this).val();

                // イベントの発火元取得
                // チェックボックスの状態取得
                if (checkBox) {
                    // 便追加
                    addTrips(selectedTripName);

                } else {
                    // 便削除
                    onLabelDeleteClick(selectedTripName);
                }

            })
        });

    }).fail(function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status === 404) {
            var errorMessage = jqXHR.responseJSON.errorMessage;
            $("#div-error-message").text(errorMessage);
        } else {
            var errorMessage = 'E3002: サーバーに接続できませんでした。' + ' HttpRequest : ' + jqXHR.status + ' textStatus : ' + textStatus;
            $("#div-error-message").text(errorMessage);
        }
    });

    
}