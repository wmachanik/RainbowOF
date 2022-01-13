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
        #region Internal classes and vars
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error,
            Debug
        }
        #endregion
        #region Parameters
        [Parameter]
        [EditorRequired]
        public string NotificationTitle { get; set; } = "";
        [Parameter]
        public string NotificationMessage { get; set; } = "Message";
        [Parameter]
        public Blazored.Toast.Configuration.ToastPosition NotificationPosition { get; set; } = Blazored.Toast.Configuration.ToastPosition.TopRight;
        [Parameter]
        public int NotificationTimeout { get; set; } = 10;
        #endregion
        #region Injections
        [Inject]
        public ILoggerManager NotificationLogger { get; set; }
        [Inject]
        public IToastService NotificationToastService { get; set; }
        #endregion
        #region Public vars
        public PopUpAndLogNotification PopUpRef;
        #endregion
        #region Exposed Routines
        public void ShowNotification(NotificationType pNotificationType)
        {
            switch (pNotificationType)
            {
                case NotificationType.Info:
                    NotificationToastService.ShowInfo(NotificationMessage, NotificationTitle);
                    NotificationLogger.LogInfo( $"Info: {NotificationMessage}");
                    break;
                case NotificationType.Success:
                    NotificationToastService.ShowSuccess(NotificationMessage, NotificationTitle);
                    NotificationLogger.LogInfo($"Success: {NotificationMessage}");
                    break;
                case NotificationType.Warning:
                    NotificationToastService.ShowWarning(NotificationMessage, NotificationTitle);
                    NotificationLogger.LogWarn($"Warning: {NotificationMessage}");
                    break;
                case NotificationType.Error:
                    NotificationToastService.ShowError(NotificationMessage, NotificationTitle);
                    NotificationLogger.LogError($"Error: {NotificationMessage}");
                    break;
                default:     // assume this is debug
                    NotificationToastService.ShowToast(ToastLevel.Info, NotificationMessage, NotificationTitle);
                    NotificationLogger.LogDebug($"Debug: {NotificationMessage}");
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
        #endregion
    }
}
