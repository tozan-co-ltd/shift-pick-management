// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// サイドバーがアクティブ
//$(document).ready(function () {
//    $("#sidebarToggle").click(function () {
//        $.removeCookie("Sidebar", { path: '/' });
//        $("body").toggleClass("sb-sidenav-toggled");
//        $("#sidebarToggle").toggleClass("rotateBtn");
//        let className = $("body").attr("class");
//        $.cookie("Sidebar", className, { path: '/' });
//    });
//})

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
});


