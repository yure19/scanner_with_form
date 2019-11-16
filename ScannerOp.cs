using System;
using System.Net.NetworkInformation;

namespace scanner_with_form
{
    class ScannerOp
    {
        public int PingReply(String ip)
        {
            PingReply reply;
            try
            {
                reply = new Ping().Send(ip, 250);
            }
            catch (PingException e)
            {
                Console.WriteLine("The following error has occurred: " + e.InnerException.Message);
                return -2;
            }
            return reply.Status.GetHashCode();
        }

        public String GetNextIp(String ip)
        {
            String[] ipSpl = ip.Split('.');

            if (!ipSpl[3].Equals("255"))
            {
                ipSpl[3] = (Int32.Parse(ipSpl[3]) + 1).ToString();
                return string.Join(".", ipSpl);
            }

            ipSpl[3] = "0";

            if (!ipSpl[2].Equals("255")) {
                ipSpl[2] = (Int32.Parse(ipSpl[2]) + 1).ToString();
                return string.Join(".", ipSpl);
            }

            ipSpl[2] = "0";

            if (!ipSpl[1].Equals("255")) {
                ipSpl[1] = (Int32.Parse(ipSpl[1]) + 1).ToString();
                return string.Join(".", ipSpl);
            }

            ipSpl[1] = "0";

            if (!ipSpl[0].Equals("255")) {
                ipSpl[0] = (Int32.Parse(ipSpl[0]) + 1).ToString();
                return string.Join(".", ipSpl);
            }

            ipSpl[0] = "0";

            return string.Join(".", ipSpl);                
        }

        public long GetTotal(String fromIp, String toIp)
        {
            return GetIpInDecimalBase(toIp) - GetIpInDecimalBase(fromIp) + 1;
        }

        public long GetIpInDecimalBase(String ip)
        {
            String[] toIpSpl = ip.Split('.');

            long ipInDecimalBase = (Int64.Parse(toIpSpl[0]) * Convert.ToInt64(Math.Pow(256, 3))) +
                                   (Int64.Parse(toIpSpl[1]) * Convert.ToInt64(Math.Pow(256, 2))) +
                                   (Int64.Parse(toIpSpl[2]) * Convert.ToInt64(Math.Pow(256, 1))) +
                                   (Int64.Parse(toIpSpl[3]) * Convert.ToInt64(Math.Pow(256, 0)));
            return ipInDecimalBase;
        }

        public double GetRelation(double part, double total)
        {
            return part / total;
        }

        public CompUnit CompareFromAndToIp(String fromIp, String toIp)
        {
            String[] ipFromSpl = fromIp.Split('.');
            String[] ipToSpl = toIp.Split('.');

            for (int i = 0; i < ipFromSpl.Length; i++)
            {
                CompUnit compResult = CompareTwoOct(ipFromSpl[i], ipToSpl[i]);
                if (compResult != CompUnit.Equal)
                {
                    return compResult;
                }
            }
            return CompUnit.Equal;
        }

        public CompUnit CompareTwoOct(String fromIpOct, String toIpOct) 
        {
            if (Int32.Parse(fromIpOct) > Int32.Parse(toIpOct))
            {
                return CompUnit.Greater;
            }
            else if (Int32.Parse(fromIpOct) < Int32.Parse(toIpOct))
            {
                return CompUnit.Lesser;
            }
            else
            {
                return CompUnit.Equal;
            }
        }
    }
}
