using Autouchet_Bot.Keyboards;
using Autouchet_Bot.Services;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autouchet_Bot.Handlers
{
    public class CallbackHandler
    {
        private readonly string _miniAppUrl;
        private readonly BackendApiClient _apiClient;

        public CallbackHandler(string miniAppUrl, BackendApiClient apiClient)
        {
            _miniAppUrl = miniAppUrl;
            _apiClient = apiClient;
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
            long senderId = callback.User.Id;
            string firstName = callback.User.FirstName;
            Attachment keyboard = null;

            switch (payload)
            {
                case "accept_agreement":
                    UserService.Accept(senderId);
                    responseText = "Вы дали согласие на обработку персональных данных, можете пользоваться ботом!";
                    keyboard = MainKeyboard.GetMainMenu(_miniAppUrl);
                    await _apiClient.CreateUserAsync(senderId, firstName);
                    break;

                case "decline_agreement":
                    responseText = "Без согласия, пользоваться ботом невозможно! " +
                        "Вернитесь в начало!";
                    keyboard = MainKeyboard.GetBackToAgreementKeyboard(); 
                    break;

                case "back_to_agreement":
                    responseText = $"Для использования бота, нужно" +
                        $" принять пользовательское соглашение";
                    keyboard = MainKeyboard.GetAgreementKeyboard();
                    break;

                case "help":
                    responseText = "Вы попали в раздел помощи. Выбирите " +
                        "интересующий вопрос";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_about_app":
                    responseText = "**Что такое автоучёт?** \n Это приложение, которое " +
                        "упрощает жизнь самозанятым, помогает создавать чеки" +
                        "и отправляет отчётность в налоговую";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_self_employed":
                    responseText = "**Что такое самозанятость?** \n Это специальный налоговый режим \n" 
                        + "для граждан, которые работают на себя."
                        + "\n **Условия самозанятости**: \n 1. Доход не больше 2.4 млн. рублей в год \n " +
                        "2. Нельзя нанимать сотрудников по друдовым договорам \n" +
                        "3. Можно продавать только только товары собственного" +
                        " производства или оказывать услуги \n "+
                        "*Налоговые ставки* \n" +
                        "4% - при работе с физическоми лицами\n" +
                        "6% - при работе с юридическими лицами и индивидуальными предпринимателями";
                        
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_support":
                    responseText = "Если остались вопросы, то пишете на почту `support@autotech.ru`";
                    keyboard = MainKeyboard.GetHelpMenu();
                    break;

                case "help_back":
                    responseText = "*Главное меню*";
                    keyboard = MainKeyboard.GetMainMenu(_miniAppUrl);
                    break;

                default:
                    return;
            }

            List<Attachment> attachments = null;
            if (keyboard != null)
            {
                attachments = new List<Attachment> { keyboard };
            }

            var answerRequest = new AnswerCallbackRequest
            {
                CallbackId = callback.CallbackId,
                Message = new NewMessageBody
                {
                    Text = responseText,
                    Attachments = attachments,
                    Format = MessageFormat.Markdown
                }
            };

            await client.AnswerCallbackAsync(answerRequest);
        }
    }
}