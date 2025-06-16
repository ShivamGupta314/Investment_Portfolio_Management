document.addEventListener("DOMContentLoaded", function () {
    let observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                document.body.style.overflowX = "hidden"; // Hide scrollbar
                entry.target.classList.add("visible");
                setTimeout(() => {
                    document.body.style.overflowX = "auto"; // Restore scrolling after animation
                }, 800); // Matches transition duration
            }
        });
    });

    document.querySelectorAll(".imagecon").forEach(el => {
        observer.observe(el);
    });
});
