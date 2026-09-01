document.addEventListener("DOMContentLoaded", function () {
    var myElement = document.getElementById('bannerSakkola');
    if (typeof bootstrap !== 'undefined') {
        var myCarousel = new bootstrap.Carousel(myElement, {
            interval: 3000,
            ride: 'carousel'
        });
        myCarousel.cycle();
    } else {
        console.error("A biblioteca do Bootstrap não foi encontrada.");
    }
});