// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// サイドバーがアクティブ
$(document).ready(function () {
    $("#sidebarToggle").click(function () {
        $.removeCookie("Sidebar", { path: '/' });
        $("body").toggleClass("sb-sidenav-toggled");
        $("#sidebarToggle").toggleClass("rotateBtn");
        let className = $("body").attr("class");
        $.cookie("Sidebar", className, { path: '/' });
    });
})



