(function () {
  const clock = document.getElementById("adminClock");
  if (clock) {
    const tick = () => {
      clock.textContent = new Intl.DateTimeFormat("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
        weekday: "short",
        day: "2-digit",
        month: "2-digit"
      }).format(new Date());
    };
    tick();
    setInterval(tick, 30000);
  }

  const app = document.querySelector(".admin-app");
  document.querySelectorAll("[data-admin-toggle]").forEach((button) => {
    button.addEventListener("click", () => app?.classList.toggle("sidebar-open"));
  });

  document.querySelectorAll("[data-count]").forEach((el) => {
    const end = Number(el.getAttribute("data-count")) || 0;
    const decimals = Number(el.getAttribute("data-decimals") || 0);
    const started = performance.now();
    const duration = 700;

    const frame = (now) => {
      const progress = Math.min((now - started) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      el.textContent = (end * eased).toFixed(decimals);
      if (progress < 1) {
        requestAnimationFrame(frame);
      }
    };

    requestAnimationFrame(frame);
  });
})();
