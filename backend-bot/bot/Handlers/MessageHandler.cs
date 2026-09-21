using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MAX.Bot;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using Autouchet_Bot.Keyboards;

namespace Autouchet_Bot.Handlers
{
    public class MessageHandler
    {
        private readonly string _miniAppUrl;

        public MessageHandler(string miniAppUrl)
        {
            _miniAppUrl = miniAppUrl;
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

            string responseText;
            if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                responseText = $"Привет, {firstName}! Добро пожаловать.";
            }
            else
            {
                responseText = $"Я не могу ответить на ваш вопросы. Выбирите подходящий вариант кнопки";
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
    }
}