using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business
{
    public static class ConversationBank
    {
        public static IReadOnlyList<ConversationScenario> All { get; } =
        [
            new()
            {
                Id = "cafe",
                Title = "Order at a cafe",
                Level = "Beginner",
                Setting = "Bạn đứng quầy. Nhân viên chờ bạn gọi món.",
                Turns =
                [
                    new()
                    {
                        Tutor = "Good morning! What would you like?",
                        Hint = "Gọi một ly cà phê lịch sự.",
                        Keywords = ["coffee", "please"],
                        Sample = "A coffee, please."
                    },
                    new()
                    {
                        Tutor = "Sure. Anything else?",
                        Hint = "Xin thêm nước hoặc nói that's all.",
                        Keywords = ["water", "that's all", "no"],
                        Sample = "Some water, please. That's all."
                    },
                    new()
                    {
                        Tutor = "That's 3 dollars. Here you are.",
                        Hint = "Cảm ơn.",
                        Keywords = ["thank", "thanks"],
                        Sample = "Thank you."
                    }
                ]
            },
            new()
            {
                Id = "airport",
                Title = "At the airport gate",
                Level = "Elementary",
                Setting = "Nhân viên kiểm tra vé trước cửa lên máy bay.",
                Turns =
                [
                    new()
                    {
                        Tutor = "Can I see your ticket and passport, please?",
                        Hint = "Đưa vé và hộ chiếu.",
                        Keywords = ["ticket", "passport", "here"],
                        Sample = "Here is my ticket and passport."
                    },
                    new()
                    {
                        Tutor = "Thank you. Gate 12 is on the left.",
                        Hint = "Hỏi lại cổng hoặc cảm ơn.",
                        Keywords = ["gate", "thank", "left"],
                        Sample = "Thank you. Is gate 12 on the left?"
                    },
                    new()
                    {
                        Tutor = "Yes. Have a nice flight.",
                        Hint = "Chúc một lời lịch sự.",
                        Keywords = ["thank", "you"],
                        Sample = "Thank you. You too."
                    }
                ]
            },
            new()
            {
                Id = "meeting",
                Title = "A short work meeting",
                Level = "Intermediate",
                Setting = "Cuộc họp ngắn với đồng nghiệp.",
                Turns =
                [
                    new()
                    {
                        Tutor = "Let's start. The sales numbers are up. What do you think?",
                        Hint = "Đồng ý và đề nghị gọi khách.",
                        Keywords = ["think", "client", "should"],
                        Sample = "I think we should call the client today."
                    },
                    new()
                    {
                        Tutor = "Good idea. Could we meet tomorrow to talk about the report?",
                        Hint = "Đồng ý lịch họp.",
                        Keywords = ["yes", "tomorrow", "meet"],
                        Sample = "Yes, we could meet tomorrow afternoon."
                    },
                    new()
                    {
                        Tutor = "Does anyone have a question?",
                        Hint = "Nói chưa có câu hỏi hoặc hỏi một ý.",
                        Keywords = ["no", "question", "budget"],
                        Sample = "No questions from me. The budget is ready."
                    }
                ]
            },
            new()
            {
                Id = "directions",
                Title = "Ask for directions",
                Level = "Beginner",
                Setting = "Bạn hỏi đường tới nhà ga.",
                Turns =
                [
                    new()
                    {
                        Tutor = "Hello. Can I help you?",
                        Hint = "Hỏi nhà ga lịch sự.",
                        Keywords = ["excuse", "station", "where"],
                        Sample = "Excuse me, where is the station?"
                    },
                    new()
                    {
                        Tutor = "Go straight and turn left. It is next to the bank.",
                        Hint = "Nhắc lại đường để chắc.",
                        Keywords = ["straight", "left", "thank"],
                        Sample = "Go straight and turn left. Thank you."
                    }
                ]
            },
            new()
            {
                Id = "hotel",
                Title = "Check in at a hotel",
                Level = "A2",
                Setting = "Role play: lễ tân khách sạn.",
                Turns =
                [
                    new()
                    {
                        Tutor = "Good evening. Do you have a reservation?",
                        Hint = "Nói có đặt phòng và nêu tên.",
                        Keywords = ["reservation", "name", "yes"],
                        Sample = "Yes, I have a reservation. My name is Minh."
                    },
                    new()
                    {
                        Tutor = "A single room for two nights. May I see your passport?",
                        Hint = "Đưa hộ chiếu.",
                        Keywords = ["passport", "here", "sure"],
                        Sample = "Sure. Here is my passport."
                    },
                    new()
                    {
                        Tutor = "Breakfast is from 7 to 10. Enjoy your stay.",
                        Hint = "Hỏi giờ / cảm ơn.",
                        Keywords = ["thank", "breakfast", "where"],
                        Sample = "Thank you. Where is the breakfast room?"
                    }
                ]
            },
            new()
            {
                Id = "doctor",
                Title = "See a doctor",
                Level = "B1",
                Setting = "Role play: phòng khám.",
                Turns =
                [
                    new()
                    {
                        Tutor = "What seems to be the problem today?",
                        Hint = "Nói triệu chứng.",
                        Keywords = ["headache", "fever", "pain", "cough"],
                        Sample = "I have a headache and a small fever."
                    },
                    new()
                    {
                        Tutor = "How long have you felt like this?",
                        Hint = "Nói thời gian.",
                        Keywords = ["days", "since", "yesterday", "two"],
                        Sample = "For two days, since yesterday morning."
                    },
                    new()
                    {
                        Tutor = "I will give you some medicine. Rest and drink water.",
                        Hint = "Hỏi cách uống thuốc.",
                        Keywords = ["how", "often", "thank", "times"],
                        Sample = "Thank you. How often should I take it?"
                    }
                ]
            }
        ];

        public static ConversationScenario? Find(string? id) =>
            All.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));

        public static int ScoreReply(ConversationTurn turn, string? reply)
        {
            var text = " " + (reply ?? string.Empty).Trim().ToLowerInvariant() + " ";
            if (string.IsNullOrWhiteSpace(reply))
            {
                return 0;
            }

            var hits = turn.Keywords.Count(key => text.Contains(key.ToLowerInvariant()));
            var coverage = (int)Math.Round(100d * hits / Math.Max(1, turn.Keywords.Length));
            var length = reply.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var lengthScore = length >= 2 ? 80 : 40;
            return Math.Clamp((int)Math.Round(coverage * 0.7 + lengthScore * 0.3), 0, 100);
        }
    }
}
