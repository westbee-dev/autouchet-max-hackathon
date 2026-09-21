using System.Collections.Generic;
using System.Threading.Tasks;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using Autouchet_Bot.Keyboards;

namespace Autouchet_Bot.Handlers
{
    public class CallbackHandler
    {
        private readonly string _miniAppUrl;

        public CallbackHandler(string miniAppUrl)
        {
            _miniAppUrl = miniAppUrl;
        }

        public async Task HandleAsync(MessageCallbackUpdate callbackUpdate, MaxBotClient client)
        {
            var callback = callbackUpdate.Callback;
            if (callback == null || string.IsNullOrEmpty(callback.Payload))
            {
                return;
            }

            string payload = callback.Payload;
            string responseText = string.Empty;
            Attachment keyboard = null;

            switch (payload)
            {
                case "help":
                    responseText = "Вы попали в раздел помощи. Выбирите " +
                        "интересующий вопрос";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_about_app":
                    responseText = "Что такое автоучёт? \n Это приложение, которое " +
                        "упрощает жизнь самозанятым";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_self_employed":
                    responseText = "Что такое самозанятость? \n Это налоговоый режим";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_support":
                    responseText = "Если остались вопросы, то пишете на почту `support@autotech.ru`";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_back":
                    responseText = "Главное меню:";
                    keyboard = MainKeyboard.GetMainMenu(_miniAppUrl);
                    break;

                default:
                    return;
            }

            var answerRequest = new AnswerCallbackRequest
            {
                CallbackId = callback.CallbackId,
                Message = new NewMessageBody
                {
                    Text = responseText,
                    Attachments = new List<Attachment> { keyboard },
                    Format = MessageFormat.Markdown
                }
            };

            await client.AnswerCallbackAsync(answerRequest);
        }
    }
}