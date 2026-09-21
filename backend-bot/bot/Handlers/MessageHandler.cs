using Autouchet_Bot.Keyboards;
using Autouchet_Bot.Services;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autouchet_Bot.Handlers
{
    public class MessageHandler
    {
        private readonly string _miniAppUrl;
        private readonly string _agreementFilePath;

        public MessageHandler(string miniAppUrl, string _agreementFilePath = "term.pdf")
        {
            _miniAppUrl = miniAppUrl;
            _agreementFilePath = _agreementFilePath;
        }

        public async Task HandleAsync(MessageCreatedUpdate messageCreated, MaxBotClient client)
        {
            var msg = messageCreated.Message;
            if (msg == null)
            {
                return;
            }

            string text = msg.Body?.Text ?? string.Empty;
            long senderId = msg.Sender.Id;
            string firstName = msg.Sender.FirstName ?? "Пользователь";

            if (!UserService.HasAccepted(senderId))
            {
                await SendAgreementRequestAsync(client, senderId, firstName);
                return;
            }

            string responseText;
            if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                responseText = $"Привет, {firstName}! Добро пожаловать в автоучёт";
            }
            else
            {
                responseText = $"Я не могу ответить на ваш вопрос. Выберите подходящий вариант кнопки";
            }

            var keyboardAttachment = MainKeyboard.GetMainMenu(_miniAppUrl);

            var sendRequest = new SendMessageRequest
            {
                UserId = senderId,
                ChatId = null,
                Text = responseText,
                Attachments = new List<Attachment> { keyboardAttachment },
                Notify = true
            };

            await client.SendMessageAsync(sendRequest);
        }

        private async Task SendAgreementRequestAsync(MaxBotClient client, long senderId, string firstName)
        {
            string text = $"Здравствуйте, {firstName}!\n\n" +
                          "Перед началом использования бота, " +
                          "пожалуйста, ознакомьтесь с пользовательским соглашением " +
                          "и примите его, нажав на соответствующую кнопку ниже." +
                          "1. Мы не несем ответственность за данные, которые вы предоставляет" +
                          "2. бла бла бла"
                          +
                          "ля ля ля";

            var attachments = new List<Attachment>
            {
                MainKeyboard.GetAgreementKeyboard()
            };

            var sendRequest = new SendMessageRequest
            {
                UserId = senderId,
                Text = text,
                Attachments = attachments,
                Notify = true
            };

            await client.SendMessageAsync(sendRequest);
        }
    }
}