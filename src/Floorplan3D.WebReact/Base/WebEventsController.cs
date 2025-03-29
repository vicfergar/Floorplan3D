namespace Floorplan3D.WebReact.Base;

public static class WebEventsController
{
    public static void StartEvergineOnCanvas(string canvasId)
    {
        Program.Run(canvasId);        
    }

    public static void StopEvergineOnCanvas(string canvasId)
    {
        if (Program.AppCanvas.ContainsKey(canvasId))
        {
            Program.AppCanvas[canvasId].Dispose();
            Program.AppCanvas.Remove(canvasId);
        }

        Program.WindowsSystem?.Dispose();
        Program.Application?.Dispose();
        
        Program.Application = null;
        Program.WindowsSystem = null;
    }

    public static void UpdateSizeOnCanvas(string canvasId)
    {
        if (Program.AppCanvas.TryGetValue(canvasId, out var surface))
        {
            surface.RefreshSize();
        }
    }   
}

