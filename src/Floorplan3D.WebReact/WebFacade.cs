using Microsoft.JSInterop;
using Floorplan3D.WebReact.Base;

namespace Floorplan3D.WebReact
{
    public static class WebFacade
    {        
        private const string BaseInvokeClassName = "Floorplan3D.WebReact.WebFacade";

        [JSInvokable($"{BaseInvokeClassName}:StartEvergineOnCanvas")]
        public static void StartEvergineOnCanvas(string canvasId)
        {
            WebEventsController.StartEvergineOnCanvas(canvasId);     
        }

        [JSInvokable($"{BaseInvokeClassName}:StopEvergineOnCanvas")]
        public static void StopEvergineOnCanvas(string canvasId)
        {
            WebEventsController.StopEvergineOnCanvas(canvasId);
        }

        [JSInvokable($"{BaseInvokeClassName}:UpdateSizeOnCanvas")]
        public static void UpdateSizeOnCanvas(string canvasId)
        {
            WebEventsController.UpdateSizeOnCanvas(canvasId);
        }
    }
}

