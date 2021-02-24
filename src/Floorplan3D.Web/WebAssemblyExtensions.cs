using System;
using System.Threading.Tasks;
using WebAssembly;

namespace Floorplan3D.Web
{
    public static class WebAssemblyExtensions
    {
        public static bool TryGetGlobalObject<T>(string name, out T value)
        {
            if (Runtime.GetGlobalObject(name) is T iResult)
            {
                value = iResult;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetObjectProperty<T>(this JSObject jSObject, string name, out T value)
        {
            if (jSObject.GetObjectProperty(name) is T iResult)
            {
                value = iResult;
                return true;
            }

            value = default;
            return false;
        }

        public static async Task<T> GetObjectPropertyAsync<T>(this JSObject jSObject, string name)
        {

            if (jSObject.TryGetObjectProperty(name, out Task<object> promiseTask))
            {
                var resultObj = await promiseTask;
                if (resultObj is T result)
                {
                    return result;
                }
            }

            return default;
        }

        public static void AddEventListener(this JSObject jSObject, string eventName, Action<JSObject> callback)
        {
            jSObject.Invoke("addEventListener", eventName, callback, false);
        }

        public static bool TryInvoke<T>(this JSObject jSObject, string method,  out T result, params object[] args)
        {
            if (jSObject.Invoke(method, args) is T iResult)
            {
                result = iResult;
                return true;
            }

            result = default;
            return false;
        }

        public static T Invoke<T>(this JSObject jSObject, string method, params object[] args)
        {
            if (jSObject.TryInvoke(method, out T result, args))
            {
                return result;
            }

            return default;
        }

        public static async Task<bool> TryInvokeAsync(this JSObject jSObject, string method, params object[] args)
        {
            if (jSObject.TryInvoke(method, out Task promiseTask, args))
            {
                await promiseTask;
                return true;
            }

            return false;
        }

        public static async Task<T> InvokeAsync<T>(this JSObject jSObject, string method, params object[] args)
        {

            if (jSObject.TryInvoke(method, out Task<object> promiseTask, args))
            {
                var resultObj = await promiseTask;
                if (resultObj is T result)
                {
                    return result;
                }
            }

            return default;
        }
    }
}
