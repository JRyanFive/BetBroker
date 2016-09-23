using BcWin.Common.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Core.WCF
{
    public class TestWCF
    {
        public void Test()
        {
            //The value in which the result will be stored
            string value = String.Empty;
            int result;
            //using a proxy for the interface IService1 call the method GetData
            //call a sync method
            
            new ProxyHelper<IBcService>().Use(serviceProxy =>
                 {
                     //save the return value in value
                     result = serviceProxy.Ping();
                 }, "WCFEndPoint");//use the end point name WCFEndPoint for this proxy

        
            //using a proxy for the interface IService1 call the method GetData
            //call a async method            
            new ProxyHelper<IBcService>().UseAsync(serviceProxy =>
            {
                //serviceProxy.GetData(5);
            }, "WCFEndPoint" //use the end point WCFEndPoint
            , AsyncResultCallBack //When the method is done call this method
            , Guid.NewGuid() //This is the Id to identify when callback happens in case many call backs take place.
            );

            new ProxyHelper<IBcService>().UseAsync(serviceProxy =>
            {
                //serviceProxy.GetDataUsingDataContract(compositeType);
            }, "WCFEndPoint", AsyncResultCallBack, Guid.NewGuid());

            new ProxyHelper<IBcService>().UseAsyncWithReturnValue((proxy, obj) =>
            {
                //save the return value in value
                //value = proxy.GetData(9);
                CallBackForReturnValueOfGetData(value, (Guid)obj);
            }, "WCFEndPoint", Guid.NewGuid());//use the end point name WCFEndPoint for this proxy

           
        }

        static void AsyncResultCallBack(IAsyncResult ar)
        {
            Console.WriteLine("Completed execution of " + ar.AsyncState);
        }
        static void CallBackForReturnValueOfGetData(string returnValue, Guid id)
        {
            Console.WriteLine("The return value is =" + returnValue + " which was called on the Guid = " + id);
        }
    }
}
