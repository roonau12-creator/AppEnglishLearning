document.addEventListener("DOMContentLoaded", function () {
  var cards = document.querySelectorAll(".course-card, .feature-card, .card, .today-card, .skill-tile");
  cards.forEach(function (card, index) {
    if (!card.classList.contains("reveal")) {
      card.style.animationDelay = Math.min(index * 0.05, 0.4) + "s";
      card.classList.add("reveal");
    }
  });
});

document.addEventListener("click", function (event) {
  const toggle = event.target.closest("[data-password-toggle]");
  if (!toggle) {
    return;
  }

  const input = toggle.closest(".auth-input")?.querySelector("input");
  const icon = toggle.querySelector("i");
  if (!input) {
    return;
  }

  const showPassword = input.type === "password";
  input.type = showPassword ? "text" : "password";
  if (icon) {
    icon.classList.toggle("bi-eye", !showPassword);
    icon.classList.toggle("bi-eye-slash", showPassword);
  }
});
