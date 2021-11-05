using Blazored.Toast.Services;
using Blazored.Toast;
using Microsoft.AspNetCore.Components;
using RainbowOF.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Components.Modals
{
    public partial class PopUpAndLogNotification : ComponentBase
    {

        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error,
            Debug
        }

        [Parameter]
        public string NotificationTitle { get; set; } = "";
        [Parameter]
        public string NotificationMessage { get; set; } = "Message";
        [Parameter]
        public Blazored.Toast.Configuration.ToastPosition NotificationPosition { get; set; } = Blazored.Toast.Configuration.ToastPosition.TopRight;
        [Parameter]
        public int NotificationTimeout { get; set; } = 10;

        public PopUpAndLogNotification PopUpRef;
        [Inject]
        public ILoggerManager NotificationLogger { get; set; }
        [Inject]
        public IToastService NotificationToastService { get; set; }

        public void ShowNotification(NotificationType pNotificationType)
        {
            switch (pNotificationType)
            {
                case NotificationType.Info:
                    NotificationToastService.ShowInfo(NotificationMessage, NotificationTitle );
                    if (String.IsNullOrEmpty(NotificationTitle))
                        NotificationLogger.LogInfo($"Info: {NotificationMessage}");
                    else
                        NotificationLogger.LogInfo($"{NotificationTitle}: {NotificationMessage}");
                    break;
                case NotificationType.Success:
                    NotificationToastService.ShowSuccess(NotificationMessage, NotificationTitle);
                    if (String.IsNullOrEmpty(NotificationTitle))
                        NotificationLogger.LogInfo($"Success: {NotificationMessage}");
                    else NotificationLogger.LogInfo($"{NotificationTitle}: {NotificationMessage}");
                    break;
                case NotificationType.Warning:
                    NotificationToastService.ShowWarning(NotificationMessage, NotificationTitle);
                    if (String.IsNullOrEmpty(NotificationTitle))
                        NotificationLogger.LogWarn($"Warning: {NotificationMessage}");
                    else
                        NotificationLogger.LogWarn($"{NotificationTitle}: {NotificationMessage}");
                    break;
                case NotificationType.Error:
                    NotificationToastService.ShowError(NotificationMessage, NotificationTitle);
                    if (String.IsNullOrEmpty(NotificationTitle))
                        NotificationLogger.LogError($"Error: {NotificationMessage}");
                    else NotificationLogger.LogError($"{NotificationTitle}: {NotificationMessage}");
                    break;
                default:     // assume this is debug
                    NotificationToastService.ShowToast(ToastLevel.Info, NotificationMessage, NotificationTitle);
                    if (String.IsNullOrEmpty(NotificationTitle))
                        NotificationLogger.LogDebug($"Debug: {NotificationMessage}");
                    else
                        NotificationLogger.LogDebug($"{NotificationTitle}: {NotificationMessage}");
                    break;
            }
        }

        public void ShowNotification(NotificationType pNotificationType, string pMessage)
        {
            NotificationMessage = pMessage;
            NotificationTitle = "";
            ShowNotification(pNotificationType);
        }

        public void ShowNotification(NotificationType pNotificationType, string pMessage, string pTtitle)
        {
            NotificationMessage = pMessage;
            NotificationTitle = pTtitle;
            ShowNotification(pNotificationType);
        }

        public void ShowQuickNotification(NotificationType pNotificationType, string pMessage)
        {
            NotificationTimeout = 5;  // not sure were to send this
            NotificationMessage = pMessage;
            NotificationTitle = "";
            ShowNotification(pNotificationType);
        }
    }
}
