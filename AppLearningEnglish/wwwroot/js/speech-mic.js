(function () {
  window.AppSpeechMic = function (options) {
    var button = document.querySelector(options.button || "#startSpeech");
    var input = document.querySelector(options.input || "#heardText");
    var statusEl = document.querySelector(options.status || "#speechStatus");
    var durationEl = document.querySelector(options.duration || "#durationMs");
    var form = button && button.form ? button.form : document.querySelector(options.form || "#speechForm");
    var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    if (!button) {
      return;
    }

    if (!SpeechRecognition) {
      button.disabled = true;
      if (statusEl) {
        statusEl.textContent = "Trình duyệt không hỗ trợ Web Speech API. Dùng Chrome/Edge hoặc gõ tay.";
      }
      return;
    }

    var recognition = new SpeechRecognition();
    recognition.lang = options.lang || "en-US";
    recognition.interimResults = true;
    recognition.maxAlternatives = 1;
    recognition.continuous = false;
    var startedAt = 0;
    var listening = false;

    function setListening(on) {
      listening = on;
      button.classList.toggle("is-listening", on);
      button.setAttribute("aria-pressed", on ? "true" : "false");
    }

    button.addEventListener("click", function () {
      if (listening) {
        recognition.stop();
        return;
      }
      if (statusEl) statusEl.textContent = "Đang nghe… hãy nói tiếng Anh.";
      startedAt = Date.now();
      setListening(true);
      try {
        recognition.start();
      } catch (err) {
        setListening(false);
      }
    });

    recognition.addEventListener("result", function (event) {
      var transcript = "";
      for (var i = 0; i < event.results.length; i++) {
        transcript += event.results[i][0].transcript;
      }
      if (input) input.value = transcript.trim();
      if (durationEl && startedAt) {
        durationEl.value = String(Date.now() - startedAt);
      }
      if (event.results[event.results.length - 1].isFinal && statusEl) {
        statusEl.textContent = "Đã nhận giọng. Đang chấm điểm…";
      }
    });

    recognition.addEventListener("end", function () {
      setListening(false);
      if (input && input.value && form && options.autoSubmit !== false) {
        form.requestSubmit ? form.requestSubmit() : form.submit();
      }
    });

    recognition.addEventListener("error", function () {
      setListening(false);
      if (statusEl) statusEl.textContent = "Không nhận được giọng. Thử lại hoặc gõ tay rồi chấm.";
    });
  };
})();
