using System.Diagnostics;

namespace AppLearningEnglish.Identity
{
    internal static class SeedMedia
    {
        public static void Ensure(string webRoot)
        {
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                return;
            }

            var audioDir = Path.Combine(webRoot, "audio");
            var wordDir = Path.Combine(audioDir, "words");
            var imageDir = Path.Combine(webRoot, "images", "courses");
            Directory.CreateDirectory(wordDir);
            Directory.CreateDirectory(imageDir);

            WriteSvg(
                Path.Combine(imageDir, "english-for-beginners.svg"),
                "#4f46e5",
                "#2563eb",
                "English for Beginners");
            WriteSvg(
                Path.Combine(imageDir, "daily-conversation.svg"),
                "#0f766e",
                "#14b8a6",
                "Daily Conversation");
            WriteSvg(
                Path.Combine(imageDir, "travel-english.svg"),
                "#c2410c",
                "#f97316",
                "Travel English");

            foreach (var item in ListeningScripts)
            {
                EnsureMp3(Path.Combine(audioDir, item.Key + ".mp3"), item.Value);
            }

            foreach (var item in WordScripts)
            {
                EnsureMp3(Path.Combine(wordDir, item.Key + ".mp3"), item.Value);
            }
        }

        public static readonly Dictionary<string, string> ListeningScripts = new()
        {
            ["hello-introductions"] =
                "Hi! My name is Anna. Nice to meet you. Hello, Anna. My name is Minh.",
            ["at-the-cafe"] =
                "Good morning. A coffee, please. Sure. Here you are. Thank you.",
            ["at-the-airport"] =
                "Can I see your ticket, please? Yes. Gate 12 is on the left.",
            ["numbers-and-time"] =
                "What time is it? It is seven o'clock. How many books do you have? I have three books.",
            ["meeting-friends"] =
                "Hi! How are you? I'm good, thanks. And you? Great. This is my friend Lan.",
            ["asking-for-directions"] =
                "Excuse me, where is the station? Go straight and turn left. Thank you very much.",
            ["workplace-email"] =
                "Hi Anna. Could we meet tomorrow afternoon to talk about the report? Best regards, Minh.",
            ["workplace-meeting"] =
                "Let's start. The sales numbers are up. I think we should call the client today. Does anyone have a question?",
            ["advanced-emphasis"] =
                "What I need is a clear plan. Although the idea is interesting, it is not practical yet. On the other hand, the budget is ready.",
            ["my-family"] =
                "This is my family. My mother cooks lunch. My brother and I help. In the afternoon we watch a film.",
            ["making-plans"] =
                "Are you free on Saturday? Let's go to the cafe at four o'clock. I can meet you there.",
            ["at-the-hotel"] =
                "I have a reservation. My name is Minh. Can I have the key, please? The room is on the second floor.",
            ["at-the-shop"] =
                "Excuse me, do you have this in a smaller size? How much is it? I'll take it.",
            ["asking-the-price"] =
                "How much are these apples? They are two dollars. Can I pay by card?"
        };

        public static readonly Dictionary<string, string> WordScripts = new()
        {
            ["hello"] = "hello",
            ["goodbye"] = "goodbye",
            ["name"] = "name",
            ["coffee"] = "coffee",
            ["please"] = "please",
            ["menu"] = "menu",
            ["water"] = "water",
            ["ticket"] = "ticket",
            ["airport"] = "airport",
            ["gate"] = "gate",
            ["passport"] = "passport",
            ["number"] = "number",
            ["time"] = "time",
            ["oclock"] = "o'clock",
            ["today"] = "today",
            ["friend"] = "friend",
            ["thanks"] = "thanks",
            ["nice"] = "nice",
            ["meet"] = "meet",
            ["excuse"] = "excuse me",
            ["left"] = "left",
            ["right"] = "right",
            ["station"] = "station",
            ["straight"] = "straight",
            ["where"] = "where",
            ["email"] = "email",
            ["regards"] = "regards",
            ["report"] = "report",
            ["tomorrow"] = "tomorrow",
            ["meeting"] = "meeting",
            ["client"] = "client",
            ["sales"] = "sales",
            ["question"] = "question",
            ["although"] = "although",
            ["however"] = "however",
            ["practical"] = "practical",
            ["budget"] = "budget",
            ["family"] = "family",
            ["mother"] = "mother",
            ["brother"] = "brother",
            ["free"] = "free",
            ["saturday"] = "Saturday",
            ["plan"] = "plan",
            ["hotel"] = "hotel",
            ["reservation"] = "reservation",
            ["key"] = "key",
            ["size"] = "size",
            ["shirt"] = "shirt",
            ["price"] = "price",
            ["card"] = "card",
            ["dollar"] = "dollar"
        };

        public static string WordAudioUrl(string wordText)
        {
            var key = FileName(wordText);
            return $"/audio/words/{key}.mp3";
        }

        public static string FileName(string wordText)
        {
            var chars = wordText.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
            return new string(chars);
        }

        private static void WriteSvg(string path, string from, string to, string title)
        {
            if (File.Exists(path))
            {
                return;
            }

            var svg =
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"800\" height=\"450\" viewBox=\"0 0 800 450\">" +
                $"<defs><linearGradient id=\"g\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">" +
                $"<stop offset=\"0%\" stop-color=\"{from}\"/><stop offset=\"100%\" stop-color=\"{to}\"/>" +
                $"</linearGradient></defs>" +
                $"<rect width=\"800\" height=\"450\" fill=\"url(#g)\"/>" +
                $"<text x=\"400\" y=\"230\" text-anchor=\"middle\" fill=\"white\" font-size=\"40\" " +
                $"font-family=\"Arial, sans-serif\" font-weight=\"700\">{title}</text></svg>";
            File.WriteAllText(path, svg);
        }

        private static void EnsureMp3(string mp3Path, string text)
        {
            if (File.Exists(mp3Path) && new FileInfo(mp3Path).Length > 0)
            {
                return;
            }

            var wavPath = Path.ChangeExtension(mp3Path, ".wav");
            try
            {
                Run("espeak-ng", $"-v en-us -s 140 -w \"{wavPath}\" \"{Escape(text)}\"");
                if (!File.Exists(wavPath))
                {
                    Run("espeak", $"-v en-us -s 140 -w \"{wavPath}\" \"{Escape(text)}\"");
                }

                if (!File.Exists(wavPath))
                {
                    return;
                }

                Run("ffmpeg", $"-y -i \"{wavPath}\" -codec:a libmp3lame -qscale:a 5 \"{mp3Path}\"");
            }
            catch
            {
                // Audio is optional at runtime; files are also generated during setup.
            }
            finally
            {
                if (File.Exists(wavPath))
                {
                    try { File.Delete(wavPath); } catch { }
                }
            }
        }

        private static string Escape(string text) =>
            text.Replace("\"", "'");

        private static void Run(string fileName, string arguments)
        {
            var start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(start);
            process?.WaitForExit(15000);
        }
    }
}
