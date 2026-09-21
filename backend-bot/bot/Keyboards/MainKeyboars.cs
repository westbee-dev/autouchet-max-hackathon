
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;
using System.Collections.Generic;

namespace Autouchet_Bot.Keyboards
{
    public static class MainKeyboard
    {
        public static Attachment GetMainMenu(string miniAppUrl)
        {
            var openMiniAppButton = new LinkButton
            {
                Text = "Запустить",
                Url = miniAppUrl
            };

            var helpButton = new Callback
            {
                Payload = "help"
            };

            return new InlineKeyboardAttachment
            {
                Payload = new InlineKeyboardPayload
                {
                    Buttons = new List<List<Button>>
                    {
                        new List<Button> { openMiniAppButton },
                        new List<Button> { helpButton }
                    }
                }
            };
        }
    }
}