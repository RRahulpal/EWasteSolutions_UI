document.addEventListener("DOMContentLoaded", function () {
    const navbar = document.getElementById("mainNavbar");
    const backToTop = document.getElementById("backToTop");

    /* Navbar and back-to-top */
    function handlePageScroll() {
        const scrollPosition = window.scrollY;

        if (navbar) {
            navbar.classList.toggle("scrolled", scrollPosition > 20);
        }

        if (backToTop) {
            backToTop.classList.toggle("show", scrollPosition > 450);
        }
    }

    window.addEventListener("scroll", handlePageScroll);
    handlePageScroll();

    if (backToTop) {
        backToTop.addEventListener("click", function () {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        });
    }

    /* Scroll reveal animation */
    const animationElements = document.querySelectorAll(
        ".reveal, .reveal-left, .reveal-right, .reveal-zoom"
    );

    if ("IntersectionObserver" in window) {
        const revealObserver = new IntersectionObserver(
            function (entries, observer) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("visible");
                        observer.unobserve(entry.target);
                    }
                });
            },
            {
                threshold: 0.12,
                rootMargin: "0px 0px -40px 0px"
            }
        );

        animationElements.forEach(function (element) {
            revealObserver.observe(element);
        });
    } else {
        animationElements.forEach(function (element) {
            element.classList.add("visible");
        });
    }

    /* Animated counters */
    const counters = document.querySelectorAll(".counter-number");

    if ("IntersectionObserver" in window && counters.length > 0) {
        const counterObserver = new IntersectionObserver(
            function (entries, observer) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                });
            },
            {
                threshold: 0.5
            }
        );

        counters.forEach(function (counter) {
            counterObserver.observe(counter);
        });
    }

    /* Close mobile navbar after clicking a normal menu link */
    const navbarCollapse = document.getElementById("mainMenu");

    document
        .querySelectorAll("#mainMenu .nav-link:not(.dropdown-toggle)")
        .forEach(function (link) {
            link.addEventListener("click", function () {
                if (
                    navbarCollapse &&
                    navbarCollapse.classList.contains("show") &&
                    window.bootstrap
                ) {
                    const bootstrapCollapse =
                        bootstrap.Collapse.getOrCreateInstance(navbarCollapse);

                    bootstrapCollapse.hide();
                }
            });
        });
});

function animateCounter(element) {
    const target = Number(element.dataset.target || 0);
    const suffix = element.dataset.suffix || "";
    const duration = 1600;
    const startTime = performance.now();

    function updateCounter(currentTime) {
        const progress = Math.min(
            (currentTime - startTime) / duration,
            1
        );

        const easedProgress = 1 - Math.pow(1 - progress, 3);
        const currentValue = Math.floor(target * easedProgress);

        element.textContent =
            currentValue.toLocaleString("en-IN") + suffix;

        if (progress < 1) {
            requestAnimationFrame(updateCounter);
        }
    }

    requestAnimationFrame(updateCounter);
}

function openWhatsApp(message) {
    const phone = "918104881897";
    const url =
        "https://wa.me/" +
        phone +
        "?text=" +
        encodeURIComponent(message);

    window.open(url, "_blank", "noopener,noreferrer");
}

function submitContactForm(event) {
    event.preventDefault();

    const form = event.target;

    const message =
        `Hello E-Waste Solutions,

Contact Enquiry

Name: ${form.name.value}
Company: ${form.company.value || "Not provided"}
Phone: ${form.phone.value}
Email: ${form.email.value || "Not provided"}

Message:
${form.message.value}`;

    openWhatsApp(message);
}

function submitPickupForm(event) {
    event.preventDefault();

    const form = event.target;

    const message =
        `Hello E-Waste Solutions,

I want to request an e-waste pickup.

Name: ${form.name.value}
Company: ${form.company.value || "Individual"}
Phone: ${form.phone.value}
City: ${form.city.value}
Asset Type: ${form.assetType.value}
Approximate Quantity: ${form.quantity.value}
Pickup Address: ${form.address.value}

Additional Details:
${form.details.value || "Not provided"}`;

    openWhatsApp(message);
}

function enquireProduct(productName) {
    const message =
        `Hello E-Waste Solutions,

I am interested in the following product:

${productName}

Please share:
- Current price
- Product condition
- Specifications
- Warranty
- Availability`;

    openWhatsApp(message);
}