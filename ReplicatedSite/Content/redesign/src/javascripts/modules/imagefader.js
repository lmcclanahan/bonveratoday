define(["jquery"], function ($) {

    // Jquery change class, based on a timer
    //--------------------
    //The script runs after page is fully loaded incl. graphics.
    $(window).on("load",
        function () {

            // Rotator Index
            var x = 0;

            // Image array
            var images = ['first', 'second', 'third', 'fourth'];

            // Fade class in/out based on a timer
            function changeImage() {
                $('body .irotate').fadeOut(function () {
                    $(this).attr('class', images[x++]);
                    $(this).fadeIn(1700);
                    x = x % images.length;
                });
            }

            // The timer
            changeImage();
            setInterval(changeImage, 6000); 
        });

});