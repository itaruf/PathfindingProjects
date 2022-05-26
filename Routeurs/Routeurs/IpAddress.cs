using System;

namespace Routeurs
{
    public class IpAddress
    {
        uint address;

        public uint GetAddress()
        {
            return address;
        }

        public string GetStringAddress()
        {
            string ret = String.Format("{0}.{1}.{2}.{3}",
                        (address >> 24) & 0xFF,
                        (address >> 16) & 0xFF,
                        (address >> 8) & 0xFF,
                        address & 0xFF);
            return ret;
        }

        public uint GetAdressRange()
        {
            return (uint)(address & 0xFFFFFF00);
        }

        public IpAddress(uint address)
        {
            this.address = address;
        }

        public IpAddress(string address)
        {
            string[] numbers = address.Split('.');
            if (numbers.Length != 4)
            {
                throw new Exception("Bad Ip address format");
            }
            this.address = 0;
            foreach (string number in numbers)
            {
                uint val = uint.Parse(number);
                if (val < 0 || val > 255)
                {
                    throw new Exception("Bad Ip address format");
                }
                this.address <<= 8;
                this.address += val;
            }
        }

    }
}
