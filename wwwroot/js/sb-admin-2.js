(function($) {
  "use strict"; // Start of use strict

  // Toggle the side navigation
  $("#sidebarToggle, #sidebarToggleTop").on('click', function(e) {
    $("body").toggleClass("sidebar-toggled");
    $(".sidebar").toggleClass("toggled");
      if ($(".sidebar").hasClass("toggled")) {
          $('.sidebar .collapse').collapse('hide');

          // 追加（山本）
          $('#fadeLayer').removeClass('fadeon')
          var w_width = $(window).width();
          if (w_width >= 768) {
              $('.container-fluid').css({ "padding-left": "7.5rem" });
          }
      }
      else {
          // 追加（山本）
          $('.sidebar').css({ "width": "6.5rem" });
          //$('.sidebar .nav-item .collapse').css({ "z-index": "1" });
          //$('.sidebar .sidebar-brand').css({ "z-index": "1" });
          $('.sidebar').css({ "z-index": "100"});

          var w_width = $(window).width();

          if (w_width < 768) {
              $('#fadeLayer').addClass('fadeon');
          }
          else if (w_width >= 768) {
              $('.container-fluid').css({ "padding-left": "15rem" });
          }

      }
  });

    // 追加（山本）画面開いたとき、画面サイズが小さければメニューかくしておく
    var w_width = $(window).width();
    if (w_width <= 768) {
        $("body").addClass('sidebar-toggled');
        $(".sidebar").addClass('toggled');
    }

  // Close any open menu accordions when window is resized below 768px
  $(window).resize(function() {
    if ($(window).width() < 768) {
        $('.sidebar .collapse').collapse('hide');
        $("body").addClass('sidebar-toggled');
        $(".sidebar").addClass('toggled');
    };
    
    // Toggle the side navigation when window is resized below 480px
    if ($(window).width() < 480 && !$(".sidebar").hasClass("toggled")) {
      $("body").addClass("sidebar-toggled");
      $(".sidebar").addClass("toggled");
      $('.sidebar .collapse').collapse('hide');
      };

      // 追加（山本）リサイズでサイドバーの大きさが変わるたび、メインの余白を変更する
      var sidberWidth = $(".sidebar").width();
      //console.log(sidberWidth);
      if (sidberWidth === 0) {
          $('.container-fluid').css({ "padding-left": "1rem" });
          $('#fadeLayer').removeClass('fadeon');
      }
      else if (sidberWidth <= 110) {
          $('.container-fluid').css({ "padding-left": "7.8rem" });
      }
      else {
          $('.container-fluid').css({ "padding-left": "15rem" });
      }

  });

  // Prevent the content wrapper from scrolling when the fixed side navigation hovered over
  $('body.fixed-nav .sidebar').on('mousewheel DOMMouseScroll wheel', function(e) {
    if ($(window).width() > 768) {
      var e0 = e.originalEvent,
        delta = e0.wheelDelta || -e0.detail;
      this.scrollTop += (delta < 0 ? 1 : -1) * 30;
      e.preventDefault();
    }
  });

  // Scroll to top button appear
  $(document).on('scroll', function() {
    var scrollDistance = $(this).scrollTop();
    if (scrollDistance > 100) {
      $('.scroll-to-top').fadeIn();
    } else {
      $('.scroll-to-top').fadeOut();
    }
  });

  // Smooth scrolling using jQuery easing
  $(document).on('click', 'a.scroll-to-top', function(e) {
    var $anchor = $(this);
    $('html, body').stop().animate({
      scrollTop: ($($anchor.attr('href')).offset().top)
    }, 1000, 'easeInOutExpo');
    e.preventDefault();
  });

})(jQuery); // End of use strict
