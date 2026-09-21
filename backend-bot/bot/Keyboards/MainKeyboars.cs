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

            var helpButton = new CallbackButton
            {
                Text = "Помощь",
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

        public static Attachment GetHelpMenu()
        {
            return new InlineKeyboardAttachment
            {
                Payload = new InlineKeyboardPayload
                {
                    Buttons = new List<List<Button>>
                    {
                        new List<Button>
                        {
                            new CallbackButton { Text = "Для чего нужен MiniApps автоучёт?",
                                Payload = "help_about_app" }
                        },
                        new List<Button>
                        {
                            new CallbackButton { Text = "Что такое самозанятость?",
                                Payload = "help_self_employed" }
                        },
                        new List<Button>
                        {
                            new CallbackButton { Text = "Другой вопрос",
                                Payload = "help_support" }
                        },
                        new List<Button>
                        {
                            new CallbackButton { Text = "Назад", Payload = "help_back" }
                        }
                    }
                }
            };
        }

        public static Attachment GetAgreementKeyboard()
        {
            return new InlineKeyboardAttachment
            {
                Payload = new InlineKeyboardPayload
                {
                    Buttons = new List<List<Button>>
                    {
                        new List<Button>
                        {
                            new CallbackButton
                            {
                                Text = "Принять",
                                Payload = "accept_agreement"
                            },
                            new CallbackButton
                            {
                                Text = "Отклонить",
                                Payload = "decline_agreement"
                            }
                        }
                    }
                }
            };
        }
        public static Attachment GetBackToAgreementKeyboard()
        {
            return new InlineKeyboardAttachment
            {
                Payload = new InlineKeyboardPayload
                {
                    Buttons = new List<List<Button>>
                    {
                        new List<Button>
                        {
                            new CallbackButton
                            {
                                Text = "Назад",
                                Payload = "back_to_agreement"
                            }
                        }
                    }
                }
            };
        }
    }
}