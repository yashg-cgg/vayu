namespace Vayu.CommonAccessLibrary
{
    public class ServiceConnections
    {
        public static string GetLMPServiceAddress()
        {
            return "net.tcp://10.10.7.47:8000/ISubscribe";
            //return "net.tcp://localhost:8000/ISubscribe";
        }
        public static string GetLMPFiveMinService()
        {
            return "net.tcp://10.10.7.47:8001/ISubscribe";
           // return "net.tcp://localhost:8001/ISubscribe";
        }
        public static string GetLatestConstraintService()
        {
            return "net.tcp://10.10.7.47:8002/ISubscribe";
            //  return "net.tcp://localhost:8002/ISubscribe";
        }
        public static string GetLoadGraphService()
        {
            return "net.tcp://10.10.7.47:8003/ISubscribe";
            // return "net.tcp://localhost:8003/ISubscribe";
        }
        public static string GetErcotPtpUpload()
        {
            return "net.tcp://10.10.7.47:8004/ISubscribe";
            //return "net.tcp://localhost:8004/ISubscribe";
        }
        public static string EnergyPriceService()
        {
            return "net.tcp://10.10.7.47:8005/ISubscribe";
            //return "net.tcp://localhost:8005/ISubscribe";
        }
        public static string GetFTRService()//
        {
            //return "net.tcp://10.10.7.47:8006/ISubscribe";
             return "net.tcp://localhost:8006/ISubscribe";
        }

        public static string GetCRRPNLService()//
        {
            return "net.tcp://10.10.7.47:8007/ISubscribe";
            //return "net.tcp://localhost:8007/ISubscribe";
        }
        public static string GetCreditCRRService()
        {
            return "net.tcp://10.10.7.47:8008/ISubscribe";
            //return "net.tcp://localhost:8008/ISubscribe";
        }
        public static string GetCRRSubmissionService()
        {
            return "net.tcp://10.10.7.47:8009/ISubscribe";
            //return "net.tcp://localhost:8009/ISubscribe";
        }

        public static string GetWindService()
        {
            return "net.tcp://10.10.7.47:8010/ISubscribe";
            //return "net.tcp://localhost:8010/ISubscribe";
        }
        public static string GetTemperatureService()
        {
            return "net.tcp://10.10.7.47:8011/ISubscribe";
            // return "net.tcp://localhost:8011/ISubscribe";
        }
        public static string GetLMPMonitorService()
        {
            return "net.tcp://10.10.7.47:8012/ISubscribe";
            //  return "net.tcp://localhost:8012/ISubscribe";
        }
        public static string GetMarketViewService()
        {
            return "net.tcp://10.10.7.47:8015/ISubscribe";
            //return "net.tcp://localhost:8015/ISubscribe";
        }

        public static string GetPJMSubmissionService()
        {
            return "net.tcp://10.30.1.4:7001/ISubscribe";
            //return "net.tcp://localhost:7003/ISubscribe";
        }
        public static string GetPJMDispatchRatesService()
        {
            return "net.tcp://10.30.1.4:7006/ISubscribe";
            //return "net.tcp://localhost:7003/ISubscribe";
        }
        public static string GetPJMReactiveInterfaceService()
        {
            return "net.tcp://10.30.1.4:7007/ISubscribe";
            //return "net.tcp://localhost:7003/ISubscribe";
        }
        public static string GetAceMonitorService()
        {
            return "net.tcp://10.30.1.4:7008/ISubscribe";
            //return "net.tcp://localhost:7003/ISubscribe";
        }
        public static string GetAncillaryService()
        {
            return "net.tcp://10.30.1.4:7009/ISubscribe";
            //return "net.tcp://localhost:7003/ISubscribe";
        }
        public static string GetPJMVirtualSubmissionService()
        {
            return "net.tcp://10.30.1.4:7010/ISubscribe";
            // return "net.tcp://localhost:7010/ISubscribe";
        }
        public static string GetFTRSubmissionService()
        {
            return "net.tcp://10.30.1.4:7017/ISubscribe";
            //return "net.tcp://localhost:7017/ISubscribe";
        }







    }
}
